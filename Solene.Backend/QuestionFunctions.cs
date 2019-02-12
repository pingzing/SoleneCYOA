using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using Solene.Database;
using Solene.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Solene.Backend
{
    public static class QuestionFunctions
    {
        [FunctionName("AddQuestion")]
        public static async Task<IActionResult> AddQuestion(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "player/{playerId}/questions")]HttpRequest req,
            string playerId,
            ILogger log)
        {
            if (req.Headers.ContentLength == 0 || req.Headers?.ContentLength == null)
            {
                log.LogError($"{DateTime.UtcNow}: Request body was empty.");
                return new BadRequestObjectResult("Request body cannot be empty.");
            }

            if (!Validation.TryValidateGuid(playerId, out Guid playerGuidId))
            {
                log.LogError($"Player ID ({playerId}) was not a valid GUID.");
                return new BadRequestObjectResult("Invalid Player ID.");
            }

            var dbClient = Database.GetDatabaseClient(log);

            log.LogInformation($"{DateTime.UtcNow}: AddQuestion called for player with ID {playerGuidId}");

            Question question = JsonConvert.DeserializeObject<Question>(await req.ReadAsStringAsync());

            log.LogInformation($"{DateTime.UtcNow}: Adding question with text {question.Text} to player with ID {playerGuidId}");

            Question addedQuestion = await dbClient.AddQuestionToPlayer(playerGuidId, question);
            if (addedQuestion == null)
            {
                return new BadRequestResult();
            }

            // FCM and WNS both enforce size limits on data payloads. We're fudging this a bit because
            // calculating the actual final size of the payload is tricky.
            // FCM is 4062 (for notification + data payload TOTAL)
            // WNS is 5KB total.
            string addedQuestionJson = JsonConvert.SerializeObject(addedQuestion);
            string base64Question = null;
            if (addedQuestionJson.Length < 2500)
            {
                base64Question = Convert.ToBase64String(Encoding.UTF8.GetBytes(addedQuestionJson));
            }
            await PushNotifications.SendPushNotification(addedQuestion.SequenceNumber, playerGuidId, question.Title, question.Text, base64Question, log);

            return new CreatedResult("", addedQuestion);
        }

        [FunctionName("GetPlayerQuestions")]
        public static async Task<IActionResult> GetPlayerQuestions(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "player/{playerId}/questions")]HttpRequest req,
            string playerId,
            ILogger log)
        {
            if (!Validation.TryValidateGuid(playerId, out Guid playerGuidId))
            {
                log.LogError($"Player ID ({playerId}) was not a valid GUID.");
                return new BadRequestObjectResult("Invalid Player ID.");
            }

            var dbClient = Database.GetDatabaseClient(log);

            log.LogInformation($"{DateTime.UtcNow}: GetPlayerQuestions called for player with ID {playerGuidId}");

            IEnumerable<Question> playerQuestions = await dbClient.GetPlayerQuestions(playerGuidId);
            if (playerQuestions == null)
            {
                return new NotFoundResult();
            }

            var realizedQuestions = playerQuestions.ToList();
            if (realizedQuestions.Count == 0)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(realizedQuestions);
        }

        [FunctionName("UpdateQuestion")]
        public static async Task<IActionResult> UpdateQuestion(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "question/{questionId}")]HttpRequest req,
            string questionId,
            ILogger log)
        {
            if (!Validation.TryValidateGuid(questionId, out Guid questionGuidId))
            {
                log.LogError($"Question ID ({questionId}) was not a valid GUID.");
                return new BadRequestObjectResult("Invalid Question ID.");
            }

            var dbClient = Database.GetDatabaseClient(log);

            log.LogInformation($"{DateTime.UtcNow}: UpdateQuestion called for question with ID {questionGuidId}");

            Question question = JsonConvert.DeserializeObject<Question>(await req.ReadAsStringAsync());

            bool success = await dbClient.UpdateQuestion(question);
            if (!success)
            {
                return new BadRequestResult();
            }

            return new OkResult();
        }

        [FunctionName("AnswerQuestion")]
        public static async Task<IActionResult> AnswerQuestion(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "question/answer/{questionId}")] HttpRequest req,
            string questionId,
            ILogger log)
        {
            if (!Validation.TryValidateGuid(questionId, out Guid questionGuidId))
            {
                log.LogError($"Question ID ({questionId}) was not a valid GUID.");
                return new BadRequestObjectResult("Invalid Question ID.");
            }

            var dbClient = Database.GetDatabaseClient(log);

            log.LogInformation($"{DateTime.UtcNow}: AnswerQuestion called for question with ID {questionGuidId}");

            var answerRequest = JsonConvert.DeserializeObject<QuestionAnswerRequest>(await req.ReadAsStringAsync());

            bool success = await dbClient.AnswerQuestion(questionGuidId, answerRequest.Answer);
            if (!success)
            {
                return new BadRequestResult();
            }

            await SendEmailToAdmin(dbClient, questionGuidId, log);

            return new OkResult();
        }

        [FunctionName("SimulateDeveloperResponse")]
        public static async Task<IActionResult> SimulateDeveloperResponse(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "question/{playerId}/simulateDeveloper")] HttpRequest req,
            string playerId,
            ILogger log)
        {
            if (!Validation.TryValidateGuid(playerId, out Guid playerGuidId))
            {
                log.LogError($"Player ID ({playerId}) was not a valid GUID.");
                return new BadRequestObjectResult("Invalid Player ID.");
            }

            var dbClient = Database.GetDatabaseClient(log);

            log.LogInformation($"{DateTime.UtcNow}: SimulateDeveloperResponse called for player with ID {playerGuidId}");

            await dbClient.AddQuestionToPlayer(playerGuidId, new Question
            {
                Title = "Simulated Developer Response",
                Text = "This is the body of a question as it would appear when added by the developer.\n" +
                "It contains text, and can contain an arbitrary number of pre-selected responses. This " +
                $"question includes two. It was added at: {DateTime.UtcNow}, UTC.",
                PrefilledAnswers = new List<string>
                {
                    "Repsonse 1",
                    "Response the second"
                }
            });

            return new CreatedResult("", null);
        }

        [FunctionName("DeleteOrphans")]
        public static async Task<IActionResult> DeleteOrphans(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "question/orphans")]HttpRequest req,
            ILogger log)
        {
            var dbClient = Database.GetDatabaseClient(log);

            log.LogInformation($"{DateTime.UtcNow}: DeleteOrphans called.");

            Stopwatch deleteStopwatch = new Stopwatch();
            deleteStopwatch.Start();
            bool success = await dbClient.DeleteOrphanQuestions();
            deleteStopwatch.Stop();

            log.LogInformation($"DeleteOrphans complete. Time elapsed: {deleteStopwatch.Elapsed}");

            if (!success)
            {
                log.LogError("Failed to delete all orphan questions.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return new OkResult();
        }

        // Possible perf win: Make this pop a message onto a queue which gets processed elsewhere instead of
        // holding up the request while this completes.
        private static async Task SendEmailToAdmin(SoleneTableClient dbClient, Guid questionId, ILogger logger)
        {
            var question = await dbClient.GetQuestion(questionId);
            if (question == null)
            {
                logger.LogError($"Unable to send email: no question with the ID {questionId} exists.");
                return;
            }
            var player = await dbClient.GetPlayer(question.PlayerId);
            if (player == null)
            {
                logger.LogError($"Unable to get player: no player with ID {question.PlayerId} found.");
                return;
            }

            var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY", EnvironmentVariableTarget.Process);
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("donotreply@solene.com", "Solene Automated Sender");
            string subject = $"Player {player.Name} has answered {question.Title} with: '{question.ChosenAnswer}'";
            subject = subject.Substring(0, Math.Min(subject.Length, 78)); // Truncate subject to 78 chars.
            string gameAdmin = Environment.GetEnvironmentVariable("GAME_ADMIN_EMAIL", EnvironmentVariableTarget.Process);
            var to = new EmailAddress(gameAdmin);
            string body = $"Name:{player.Name}\r\n" +
                $"ID: {question.PlayerId}\r\n" +
                $"Gender: {player.Gender}\r\n" +
                $"Question: {question.SequenceNumber}. {question.Title}: {question.Text}\r\n" +
                $"'{question.ChosenAnswer}'";
            var email = MailHelper.CreateSingleEmail(from, to, subject, body, null);
            var response = await client.SendEmailAsync(email);
            if (response.StatusCode != HttpStatusCode.Accepted)
            {
                logger.LogError($"Failed to send email {gameAdmin}. Error: {await response.Body.ReadAsStringAsync()}");
            }
        }
    }
}
