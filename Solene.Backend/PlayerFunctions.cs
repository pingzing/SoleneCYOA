using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Solene.Database;
using Solene.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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

            Player newPlayer = await dbClient.CreatePlayer(player, GetStartingQuestions());
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

        [FunctionName("GetAllPlayers")]
        public static async Task<IActionResult> GetAllPlayers(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "players")]HttpRequest req,
            ILogger log)
        {
            string connectionString = Environment.GetEnvironmentVariable("SOLENE_CONNECTION_STRING", EnvironmentVariableTarget.Process);
            var dbClient = new SoleneTableClient(connectionString, log);

            List<Player> getAllPlayersResult = (await dbClient.GetAllPlayers())?.ToList();
            if (getAllPlayersResult == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(getAllPlayersResult);
        }

        [FunctionName("GetAllPlayersAndDetails")]
        public static async Task<IActionResult> GetAllPlayersAndDetails(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "players-and-details")]HttpRequest req,
            ILogger log)
        {
            string connectionString = Environment.GetEnvironmentVariable("SOLENE_CONNECTION_STRING", EnvironmentVariableTarget.Process);
            var dbClient = new SoleneTableClient(connectionString, log);

            List<Task<IEnumerable<AdminQuestion>>> getQuestionsTasks = new List<Task<IEnumerable<AdminQuestion>>>();
            var getAllPlayersResult = (await dbClient.GetAllPlayers()).ToList();
            foreach(var player in getAllPlayersResult)
            {
                getQuestionsTasks.Add(dbClient.GetPlayerAdminQuestions(player.Id));
            }

            var allQuestions = (await Task.WhenAll(getQuestionsTasks)).SelectMany(x => x);
            return new OkObjectResult(new PlayersAndDetails
            {
                AllPlayers = getAllPlayersResult,
                AllQuestions = allQuestions.ToList()
            });
        }

        [FunctionName("RegisterPush")]
        public static async Task<IActionResult> RegisterPushNotifications(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "player/{playerId}/push")]HttpRequest req,
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

            PushRegistrationRequest pushRegistration = JsonConvert.DeserializeObject<PushRegistrationRequest>(await req.ReadAsStringAsync());

            bool success = await PushNotifications.Register(playerGuidId,
                pushRegistration.PushPlatform,
                pushRegistration.PnsToken,
                pushRegistration.PlatformPushTemplate,
                log);

            if (!success)
            {
                return new BadRequestResult();
            }

            return new CreatedResult("", "");
        }

        private static IEnumerable<Question> GetStartingQuestions()
        {
            return new[]
            {
                new Question
                {
                    Title = "Welcome!",
                    Text = "You've taken your first step in an adventure in the twin cities of Aurinsol and Kuulene--the City of the Sun and the Metropolis of the Moon. " +
                    "Before we get started, let's establish a few more things about you. \n\n" +
                    "First: which of these attributes define you?",
                    PrefilledAnswers = new List<string> {"Speed", "Intelligence", "Strength"},
                },
                new Question
                {
                    Title = "Your Background",
                    Text = "Okay! Now, for the second question, this time about your background:\n\n" +
                    "How wealthy would you say you were while growing up?",
                    PrefilledAnswers = new List<string> {"Poor", "Comfortable", "Rich"},
                },
                new Question
                {
                    Title = "Your Personality",
                    Text = "Good, we'll let the tax collectors know. Now, finally, let's get a little bit more...personal.\n\n" +
                    "What character trait most defines you?",
                    PrefilledAnswers = new List<string> {"Bravery", "Stubbornness", "Kindness", "Precision", "Rage",
                                                            "All-consuming Ennui" },
                },
                new Question
                {
                    Title = "Ready?",
                    Text = "Okay. That's all we need for now. All the other questions are going to be custom-tailored to you.\n" +
                    "Now, for the most important question.\n\n" +
                    "Are you ready?\n\n" +
                    "(Please note that after answering this, a real live human has to read your input, and write content for you. This can take some time!)",
                    PrefilledAnswers = new List<string> { "Yes" },
                },
            };
        }
    }
}
