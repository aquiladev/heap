using Heap.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace Heap.FetchQuestion
{
    public static class FetchQuestionFunc
    {
        [FunctionName("FetchQuestion")]
        [return: Queue("%Storage:ProcessQueue%", Connection = "Storage:Connection")]
        public static async Task<string> Run(
            [QueueTrigger("%Storage:QuestionQueue%", Connection = "Storage:Connection")]string message,
            [Table("%Storage:QuestionTable%", Connection = "Storage:Connection")] CloudTable table,
            ILogger log,
            ExecutionContext context)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            HttpClient client = new HttpClient(handler);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));

            UriBuilder builder = new UriBuilder(config["SE:QuestionsEndpoint"]);
            var parameters = HttpUtility.ParseQueryString(string.Empty);
            parameters["access_token"] = config["SE:AccessToken"];
            parameters["key"] = config["SE:Key"];
            parameters["site"] = "ethereum.stackexchange";
            parameters["order"] = "desc";
            parameters["sort"] = "activity";
            parameters["filter"] = "withbody";
            builder.Query = parameters.ToString();
            builder.Path += message;

            HttpResponseMessage response = await client.GetAsync(builder.Uri.ToString());
            response.EnsureSuccessStatusCode();

            var entry = new QuestionEntity()
            {
                PartitionKey = message,
                RowKey = "",
                Payload = await response.Content.ReadAsStringAsync()
            };
            TableOperation insertOperation = TableOperation.Insert(entry);
            await table.ExecuteAsync(insertOperation);

            return JsonConvert.SerializeObject(entry);
        }
    }
}
