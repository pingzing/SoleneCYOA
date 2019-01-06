using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Solene.Database;
using Solene.Models;
using System;
using System.Threading.Tasks;

namespace Solene.Backend
{
    public static class PlayerFunctions
    {
        [FunctionName("CreatePlayer")]
        public static async Task<IActionResult> CreatePlayer([HttpTrigger(AuthorizationLevel.Function, "post", Route = "player")]HttpRequest req, ILogger log)
        {
            if (req.Headers.ContentLength == 0 || req.Headers?.ContentLength == null)
            {
                log.LogError($"{DateTime.UtcNow}: Request body was empty.");
                return new BadRequestObjectResult("Request body cannot be empty.");
            }

            string connectionString = Environment.GetEnvironmentVariable("SOLENE_CONNECTION_STRING", EnvironmentVariableTarget.Process);
            var dbClient = new SoleneTableClient(connectionString, log);

            log.LogInformation($"{DateTime.UtcNow}: CreatePlayer called.");

            Player player = JsonConvert.DeserializeObject<Player>(await req.ReadAsStringAsync());

            log.LogInformation($"{DateTime.UtcNow}: Creating player with name {player.Name} and gender {player.Gender}");

            Player newPlayer = await dbClient.CreatePlayer(player);
            if (newPlayer == null)
            {
                log.LogError($"DB could not create new player.");
                return new BadRequestObjectResult($"Failed to create new player.");
            }

            return new CreatedResult("", newPlayer);
        }

        [FunctionName("DeletePlayer")]
        public static async Task<IActionResult> DeletePlayer(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "player/{playerId}")]HttpRequest req,
            string playerId,
            ILogger log)
        {
            if (String.IsNullOrWhiteSpace(playerId))
            {
                log.LogError($"{DateTime.UtcNow}: Player ID was null or empty.");
                return new BadRequestObjectResult("Player ID must not be null or empty.");
            }

            if (!Guid.TryParse(playerId, out Guid playerGuidId))
            {
                log.LogError($"Player ID ({playerId}) was not a valid GUID.");
                return new BadRequestObjectResult("Invalid Player ID.");
            }

            string connectionString = Environment.GetEnvironmentVariable("SOLENE_CONNECTION_STRING", EnvironmentVariableTarget.Process);
            var dbClient = new SoleneTableClient(connectionString, log);

            log.LogInformation($"{DateTime.UtcNow}: DeletePlayer called for player with ID {playerGuidId}.");

            var deleteResult = await dbClient.DeletePlayer(playerGuidId);
            if (deleteResult.HttpStatusCode != StatusCodes.Status204NoContent)
            {
                log.LogError($"DB did not return 204. Could not delete player with ID {playerGuidId}");
            }

            return new OkResult();
        }

        [FunctionName("GetPlayer")]
        public static async Task<IActionResult> GetPlayer(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "player/{playerId}")]HttpRequest req,
            string playerId,
            ILogger log)
        {
            if (String.IsNullOrWhiteSpace(playerId))
            {
                log.LogError($"{DateTime.UtcNow}: Player ID was null or empty.");
                return new BadRequestObjectResult("Player ID must not be null or empty.");
            }

            if (!Guid.TryParse(playerId, out Guid playerGuidId))
            {
                log.LogError($"Player ID ({playerId}) was not a valid GUID.");
                return new BadRequestObjectResult("Invalid Player ID.");
            }

            string connectionString = Environment.GetEnvironmentVariable("SOLENE_CONNECTION_STRING", EnvironmentVariableTarget.Process);
            var dbClient = new SoleneTableClient(connectionString, log);

            log.LogInformation($"{DateTime.UtcNow}: GetPlayer called for player with ID {playerGuidId}.");

            Player getResult = await dbClient.GetPlayer(playerGuidId);
            if (getResult == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(getResult);
        }
    }
}
