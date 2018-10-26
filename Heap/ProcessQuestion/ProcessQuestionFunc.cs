using Heap.Entities;
using HtmlAgilityPack;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Heap.TagBodyQuestion
{
    internal class Payload
    {
        public Payload()
        {
            Items = new List<Item>();
        }

        public IList<Item> Items { get; set; }
    }

    internal class Item
    {
        public string Body { get; set; }
    }

    public static class ProcessQuestionFunc
    {
        [FunctionName("ProcessQuestion")]
        public async static Task Run(
            [QueueTrigger("%Storage:ProcessQueue%", Connection = "Storage:Connection")]QuestionEntity message,
            [Table("%Storage:QuestionTable%", Connection = "Storage:Connection")] CloudTable table,
            ILogger log,
            ExecutionContext context)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            Payload payload = JsonConvert.DeserializeObject<Payload>(message.Payload);
            if (payload.Items.Count == 0)
            {
                return;
            }

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(payload.Items[0].Body);

            string text = "";
            foreach (HtmlNode node in doc.DocumentNode.ChildNodes.Where(n => n.Name.ToLower() != "pre"))
            {
                text += node.InnerText;
            }

            message.Aylien = await GetAylien(config, text);
            TableOperation mergeOperation = TableOperation.Merge(message);
            await table.ExecuteAsync(mergeOperation);

            log.LogInformation($"C# Queue trigger function processed: {message}");
        }

        public async static Task<string> GetAylien(IConfigurationRoot config, string text)
        {
            var handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            HttpClient client = new HttpClient(handler);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            client.DefaultRequestHeaders.Add("X-AYLIEN-TextAPI-Application-Key", config["Aylien:Key"]);
            client.DefaultRequestHeaders.Add("X-AYLIEN-TextAPI-Application-ID", config["Aylien:AppID"]);

            var dict = new List<KeyValuePair<string, string>>();
            dict.Add(new KeyValuePair<string, string>("text", text));
            dict.Add(new KeyValuePair<string, string>("endpoint", "extract"));
            dict.Add(new KeyValuePair<string, string>("endpoint", "entities"));
            dict.Add(new KeyValuePair<string, string>("endpoint", "concepts"));
            dict.Add(new KeyValuePair<string, string>("endpoint", "summarize"));
            dict.Add(new KeyValuePair<string, string>("endpoint", "language"));
            dict.Add(new KeyValuePair<string, string>("endpoint", "sentiment"));
            dict.Add(new KeyValuePair<string, string>("endpoint", "hashtags"));

            HttpResponseMessage response = await client.PostAsync(
                config["Aylien:Endpoint"] + "/combined",
                new FormUrlEncodedContent(dict));
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
