using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Solene.Database.EntityModels;
using Solene.Database.ExtensionMethods;
using Solene.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Solene.Database
{
    public class SoleneTableClient
    {
        private CloudTableClient _tableClient;
        private readonly ILogger _logger;

        public SoleneTableClient(string connectionString, ILogger logger)
        {
            _logger = logger;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            _tableClient = storageAccount.CreateCloudTableClient();
        }

        public async Task<Player> CreatePlayer(Player player, IEnumerable<Question> startingQuestions)
        {
            var table = _tableClient.GetTableReference(TableNames.Player);
            
            player.Id = Guid.NewGuid();

            TableOperation insertOperation = TableOperation.Insert(new PlayerEntity(player), true);
            TableResult result = await table.ExecuteAsync(insertOperation);
            if (result.Result is PlayerEntity newPlayerEntity)
            {
                // Create starting questions
                var questionEntities = startingQuestions.Select((x, index) =>
                {
                    x.Id = Guid.NewGuid();
                    x.PlayerId = player.Id;
                    x.SequenceNumber = (uint)index;
                    return new QuestionEntity(x);
                });

                var questionsResult = await AddQuestionsToPlayer(questionEntities);
                if (questionsResult.Any(x => x.HttpStatusCode != 201))
                {
                    _logger.LogError($"Failed to add at least one starting question when creating player {player.Id}");
                }

                return newPlayerEntity.ToPlayer();
            }
            else
            {
                _logger.LogError($"Failed to create new player with name {player.Name}. Table HTTP Code: {result.HttpStatusCode}");
                return null;
            }
        }

        public async Task<Player> GetPlayer(Guid playerId)
        {
            var table = _tableClient.GetTableReference(TableNames.Player);
            TableOperation getOperation = TableOperation.Retrieve<PlayerEntity>(PartitionKeys.Player, playerId.ToString("N"));
            TableResult result = await table.ExecuteAsync(getOperation);
            if (result.Result is PlayerEntity playerEntity)
            {
                return playerEntity.ToPlayer();
            }
            else
            {
                _logger.LogError($"Failed to retrieve player with ID {playerId}. Table HTTP code: {result.HttpStatusCode}");
                return null;
            }
        }

        public async Task<IEnumerable<Player>> GetAllPlayers()
        {
            var table = _tableClient.GetTableReference(TableNames.Player);
            TableQuery<PlayerEntity> query = new TableQuery<PlayerEntity>().Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, PartitionKeys.Player));

            IEnumerable<PlayerEntity> playerEntities = await table.ExecuteQueryAsync(query);
            return playerEntities.Select(x => x.ToPlayer());
        }

        public async Task<Question> AddQuestionToPlayer(Guid playerId, Question question)
        {
            var table = _tableClient.GetTableReference(TableNames.Player);
            var playerQuestions = await GetPlayerQuestions(playerId);

            question.Id = Guid.NewGuid();
            question.PlayerId = playerId;
            question.SequenceNumber = (uint)playerQuestions.Count(); // SequenceNumber is zero-indexed.            

            QuestionEntity questionEntity = new QuestionEntity(question);
            TableOperation insertOperation = TableOperation.Insert(questionEntity, true);
            TableResult result = await table.ExecuteAsync(insertOperation);
            if (result.Result is QuestionEntity newQuestionEntity)
            {
                return newQuestionEntity.ToQuestion();
            }
            else
            {
                _logger.LogError($"Failed to add question with text {question.Text} for player with ID: {playerId}. DB Status code: {result.HttpStatusCode}");
                return null;
            }
        }        

        public async Task<IEnumerable<Question>> GetPlayerQuestions(Guid playerId)
        {
            return (await GetPlayerQuestionEntities(playerId))?.Select(x => x.ToQuestion());
        }

        public async Task<bool> UpdateQuestion(Question question)
        {
            var table = _tableClient.GetTableReference(TableNames.Player);

            // Get old entity
            QuestionEntity oldEntity = await GetQuestion(question.Id);
            if (oldEntity == null)
            {
                _logger.LogError($"Could not update question, no question with ID {question.Id} found.");
                return false;
            }

            QuestionEntity newQuestion = new QuestionEntity(question) { ETag = oldEntity.ETag };
            TableOperation mergeOperation = TableOperation.Merge(newQuestion);
            TableResult result = await table.ExecuteAsync(mergeOperation);
            if (result.HttpStatusCode != 204)
            {
                return false;
            }

            return true;
        }

        public async Task<TableResult> DeletePlayer(Guid playerId)
        {
            var table = _tableClient.GetTableReference(TableNames.Player);
            TableOperation retrieveOperation = TableOperation.Retrieve<PlayerEntity>(PartitionKeys.Player, playerId.ToString("N"));
            TableResult retrieveResult = await table.ExecuteAsync(retrieveOperation);
            if (retrieveResult.Result is PlayerEntity player)
            {
                TableOperation deleteOperation = TableOperation.Delete(player);
                return await table.ExecuteAsync(deleteOperation);
            }
            else
            {
                _logger.LogError($"Failed to find any player with ID: {playerId}. Aborting.");
                return retrieveResult;
            }
        }

        private async Task<QuestionEntity> GetQuestion(Guid questionId)
        {
            var table = _tableClient.GetTableReference(TableNames.Player);

            TableOperation retrieveOperation = TableOperation.Retrieve<QuestionEntity>(PartitionKeys.Question, questionId.ToString("N"));
            TableResult retrieveResult = await table.ExecuteAsync(retrieveOperation);
            if (!(retrieveResult.Result is QuestionEntity questionEntity))
            {
                _logger.LogError($"Failed to find any Question with ID {questionId}.");
                return null;
            }

            return questionEntity;
        }

        private async Task<IEnumerable<QuestionEntity>> GetPlayerQuestionEntities(Guid playerId)
        {
            var table = _tableClient.GetTableReference(TableNames.Player);
            TableQuery<QuestionEntity> questionsForPlayerQuery = new TableQuery<QuestionEntity>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, PartitionKeys.Question),
                    TableOperators.And,
                    TableQuery.GenerateFilterConditionForGuid("PlayerId", QueryComparisons.Equal, playerId)));

            return await table.ExecuteQueryAsync(questionsForPlayerQuery);
        }

        private async Task<IList<TableResult>> AddQuestionsToPlayer(IEnumerable<QuestionEntity> questions)
        {
            var table = _tableClient.GetTableReference(TableNames.Player);
            TableBatchOperation insertBatch = new TableBatchOperation();
            foreach(var question in questions)
            {
                insertBatch.Insert(question);
            }

            return await table.ExecuteBatchAsync(insertBatch);
        }
    }
}
