using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        public static async Task Run(
            [QueueTrigger("%Storage:QuestionQueue%", Connection = "Storage:Connection")]string message,
            ILogger log,
            ExecutionContext context)
        {
            var config = new ConfigurationBuilder()
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
            if (response.IsSuccessStatusCode)
            {   
                var s = await response.Content.ReadAsStringAsync();
            }

            log.LogInformation($"C# Queue trigger function processed: {message}");
        }
    }
}
