using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Threading.Tasks;

namespace WorkshopAPI
{
    public static class VersionFunction
    {

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        [FunctionName(nameof(VersionFunction))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "version")] HttpRequest req,
            ILogger log)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var ver = Assembly.GetExecutingAssembly().GetName().Version;
            return new OkObjectResult(ver);
        }
    }
}
