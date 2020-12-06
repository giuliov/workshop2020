using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using WorkshopAPI.DataModels;

namespace WorkshopAPI
{
    public static class EntitiesFunction
    {
        const string wellKnownEntityRowKey = "00000000-1111-2222-3333-444444444444";

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        [FunctionName(nameof(EntitiesFunction))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "entities/{rowKey}")] HttpRequest req,
            [Table(nameof(SomeEntity), SomeEntity.PARTITION_KEY, "{rowKey}", Connection = "_WorkshopAPI_SomeEntity")] SomeEntity entity,
            string rowKey,
            ILogger log)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {

            if (rowKey == wellKnownEntityRowKey)
            {
                log.LogInformation("Special RowKey processing.");

                var wellKnownEntity = new SomeEntity { Name = "well-known-item", Description = $"SPECIAL RECORD", SomeNumber = 42 };

                return new OkObjectResult(wellKnownEntity);
            }
            else if (entity != null)
            {
                log.LogInformation("Entity found.");
                return new OkObjectResult(entity);
            }
            else
            {
                log.LogWarning("Entity not found.");
                return new BadRequestObjectResult("Unknown key");
            }
        }
    }
}
