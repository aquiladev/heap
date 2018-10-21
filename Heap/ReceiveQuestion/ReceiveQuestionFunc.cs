using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Heap.ReceiveQuestion
{
    public static class ReceiveQuestionFunc
    {
        [FunctionName("ReceiveQuestion")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [Queue("%Storage:QuestionQueue%", Connection = "Storage:Connection")] ICollector<string> output,
            ILogger log,
            ExecutionContext context)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string ids = data?.ids;

            if (ids == null)
            {
                return new BadRequestObjectResult("Please pass a ids in the request body");
            }

            string[] array = ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string id in array)
            {
                output.Add(id);
            }

            return new OkObjectResult("OK");
        }
    }
}
