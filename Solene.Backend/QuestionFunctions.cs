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
using System.Linq;
using System.Net;
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

            log.LogInformation($"{DateTime.UtcNow}: AddQuestion called for player with ID {playerGuidId}");

            Question question = JsonConvert.DeserializeObject<Question>(await req.ReadAsStringAsync());

            log.LogInformation($"{DateTime.UtcNow}: Adding question with text {question.Text} to player with ID {playerGuidId}");

            Question addedQuestion = await dbClient.AddQuestionToPlayer(playerGuidId, question);
            if (addedQuestion == null)
            {
                return new BadRequestResult();
            }

            await PushNotifications.SendPushNotification(playerGuidId, question.Title, question.Text, log);

            return new CreatedResult("", addedQuestion);
        }

        [FunctionName("GetPlayerQuestions")]
        public static async Task<IActionResult> GetPlayerQuestions(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "player/{playerId}/questions")]HttpRequest req,
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
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "question/{questionId}")]HttpRequest req,
            string questionId,
            ILogger log)
        {
            if (String.IsNullOrWhiteSpace(questionId))
            {
                log.LogError($"{DateTime.UtcNow}: Question ID was null or empty.");
                return new BadRequestObjectResult("Question ID must not be null or empty.");
            }

            if (!Guid.TryParse(questionId, out Guid questionGuidId))
            {
                log.LogError($"Question ID ({questionId}) was not a valid GUID.");
                return new BadRequestObjectResult("Invalid Question ID.");
            }

            string connectionString = Environment.GetEnvironmentVariable("SOLENE_CONNECTION_STRING", EnvironmentVariableTarget.Process);
            var dbClient = new SoleneTableClient(connectionString, log);

            log.LogInformation($"{DateTime.UtcNow}: UpdateQuestion called for player with ID {questionGuidId}");

            Question question = JsonConvert.DeserializeObject<Question>(await req.ReadAsStringAsync());

            bool success = await dbClient.UpdateQuestion(question);
            if (!success)
            {
                return new BadRequestResult();
            }

            await SendEmailToAdmin(question.PlayerId, question, log);

            return new OkResult();
        }

        private static async Task SendEmailToAdmin(Guid playerId, Question question, ILogger logger)
        {
            var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY", EnvironmentVariableTarget.Process);
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("donotreply@solene.com", "Solene Admin Automated Sender");
            string subject = $"Player {playerId} has answered {question.Title} with: '{question.ChosenAnswer}'";
            subject = subject.Substring(0, Math.Min(subject.Length, 78)); // Truncate subject to 78 chars.
            string gameAdmin = Environment.GetEnvironmentVariable("GAME_ADMIN_EMAIL", EnvironmentVariableTarget.Process);
            var to = new EmailAddress(gameAdmin);
            string body = $"Player with ID {playerId} has answered question {question.SequenceNumber} with:\n" +
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
