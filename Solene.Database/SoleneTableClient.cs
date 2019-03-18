using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
                    x.SequenceNumber = (uint)index + 1;
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

        public async Task<bool> DeletePlayerQuestions(Guid playerGuidId)
        {
            var table = _tableClient.GetTableReference(TableNames.Player);
            TableQuery<DynamicTableEntity> questionsForPlayerQuery = new TableQuery<DynamicTableEntity>()
                .Where(TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, PartitionKeys.Question),
                    TableOperators.And,
                    TableQuery.GenerateFilterConditionForGuid("PlayerId", QueryComparisons.Equal, playerGuidId)))
                .Select(new string[] { "PartitionKey", "RowKey" });

            return await DeleteEntities(await table.ExecuteQueryAsync(questionsForPlayerQuery));            
        }

        public async Task<bool> DeleteOrphanQuestions()
        {
            var table = _tableClient.GetTableReference(TableNames.Player);
            TableQuery<DynamicTableEntity> playerIdsQuery = new TableQuery<DynamicTableEntity>().Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, PartitionKeys.Player))
            .Select(new string[] { "RowKey" });

            var playerIds = (await table.ExecuteQueryAsync(playerIdsQuery))
                .Select(x => Guid.Parse(x.RowKey))
                .ToList();

            TableQuery<DynamicTableEntity> allQuestionsQuery = new TableQuery<DynamicTableEntity>().Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, PartitionKeys.Question))
                .Select(new string[] { "PartitionKey", "RowKey", "PlayerId" });
            EntityResolver<QuestionEntity> questionResolver = (pk, rk, ts, props, etag) =>
                props.ContainsKey("PlayerId")
                ? new QuestionEntity { PartitionKey = pk, RowKey = rk, PlayerId = props["PlayerId"].GuidValue.Value }
                : null;

            var questions = table.ExecuteQuery(allQuestionsQuery, questionResolver, null, null).ToList();
            var orphanQuestions = questions.Where(x => !playerIds.Contains(x.PlayerId));
            return await DeleteEntities(orphanQuestions);
        }

        public async Task<Question> AddQuestionToPlayer(Guid playerId, Question question)
        {
            var table = _tableClient.GetTableReference(TableNames.Player);
            var playerQuestions = await GetPlayerQuestions(playerId);

            question.Id = Guid.NewGuid();
            question.PlayerId = playerId;
            question.SequenceNumber = (uint)playerQuestions.Count() + 1;

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

        public async Task<IEnumerable<AdminQuestion>> GetPlayerAdminQuestions(Guid playerId)
        {
            return (await GetPlayerQuestionEntities(playerId))?.Select(x => new AdminQuestion
            {
                ChosenAnswer = x.ChosenAnswer,
                Id = Guid.Parse(x.RowKey),
                PlayerId = x.PlayerId,
                PrefilledAnswers = JsonConvert.DeserializeObject<List<string>>(x.PrefilledAnswersJson),
                SequenceNumber = (uint)x.SequenceNumber,
                Text = x.Text,
                Title = x.Title,
                UpdatedTimestamp = x.Timestamp
            });
        }

        public async Task<bool> AnswerQuestion(Guid questionId, string answer)
        {
            var table = _tableClient.GetTableReference(TableNames.Player);

            QuestionEntity oldQuestionEntity = await GetQuestionEntity(questionId);
            if (oldQuestionEntity == null)
            {
                _logger.LogError($"Could not answer question with '${answer}', no question with ID {questionId} found.");
                return false;
            }

            oldQuestionEntity.ChosenAnswer = answer;

            TableOperation updateOperation = TableOperation.Merge(oldQuestionEntity);
            TableResult result = await table.ExecuteAsync(updateOperation);
            if (result.HttpStatusCode != 204)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> UpdateQuestion(Question question)
        {
            var table = _tableClient.GetTableReference(TableNames.Player);

            // Get old entity
            QuestionEntity oldEntity = await GetQuestionEntity(question.Id);
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

        public async Task<bool> SetPlayerVisibility(Guid playerGuidId, bool isPublic)
        {
            var table = _tableClient.GetTableReference(TableNames.Player);
            var entity = new DynamicTableEntity(PartitionKeys.Player, playerGuidId.ToString("N"))
            {
                ETag = "*",
            };
            entity.Properties.Add(nameof(PlayerEntity.IsPublic), new EntityProperty(isPublic));
            TableOperation mergeOperation = TableOperation.Merge(entity);
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

        public async Task<Question> GetQuestion(Guid questionId)
        {
            var questionEntity = await GetQuestionEntity(questionId);
            return questionEntity?.ToQuestion();
        }

        private async Task<QuestionEntity> GetQuestionEntity(Guid questionId)
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

        private async Task<bool> DeleteEntities(IEnumerable<ITableEntity> entities)
        {
            var table = _tableClient.GetTableReference(TableNames.Player);
            // Split into groups of 100, as batch operations allow a max of 100 items
            var sublists = entities
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / 100)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();

            var deleteBatchOps = sublists.Select(sublist =>
            {
                TableBatchOperation deleteOp = new TableBatchOperation();
                foreach (var entity in sublist)
                {
                    if (string.IsNullOrWhiteSpace(entity.ETag))
                    {
                        entity.ETag = "*";
                    }
                    deleteOp.Delete(entity);
                }
                return deleteOp;
            });

            var deletionResults = (await Task.WhenAll(deleteBatchOps.Select(x => table.ExecuteBatchAsync(x))))
                .SelectMany(x => x);
            if (deletionResults.Any(x => x.HttpStatusCode != 204))
            {
                return false;
            }

            return true;
        }
    }
}
