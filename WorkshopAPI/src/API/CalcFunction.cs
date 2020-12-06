using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WorkshopAPI
{
    public static class CalcFunction
    {

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        [FunctionName(nameof(CalcFunction))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "calc/{anInteger}")] HttpRequest req,
            int anInteger,
            ILogger log)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            int result;
            if (IsFeatureOn("FF_USE_V2_ALGORITHM"))
            {
                log.LogInformation("V2 Algorithm.");
                result = (2 * anInteger + 42) / 3;
            }
            else
            {
                log.LogInformation("V1 Algorithm.");
                result = (anInteger + 42) / 2;
            }
            return new OkObjectResult(result);
        }

        static readonly string[] onValues = new string[] { "true", "1", "yes", "on" };
        static bool IsFeatureOn(string featureFlagName)
        {
            string featureFlag = Environment.GetEnvironmentVariable(featureFlagName, EnvironmentVariableTarget.Process);
            if (featureFlag == null)
                return false;
            featureFlag = featureFlag.ToLower();
            return onValues.Any(s => s == featureFlag);
        }
    }
}
