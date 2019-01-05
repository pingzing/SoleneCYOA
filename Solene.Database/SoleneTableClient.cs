using Microsoft.Azure.CosmosDB.Table;
using Microsoft.Azure.Storage;
using Solene.Database.EntityModels;
using Solene.Models;
using System;
using System.Threading.Tasks;

namespace Solene.Database
{
    public class SoleneTableClient
    {        
        private CloudTableClient _tableClient;

        public SoleneTableClient(string connectionString)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString); // todo connection string rar
            _tableClient = storageAccount.CreateCloudTableClient();
        }

        public async Task<TableResult> CreatePlayer(Player player)
        {
            var table = _tableClient.GetTableReference(TableNames.Player);
            // TODO: Prefill all new players with starting questions
            TableOperation insertOperation = TableOperation.Insert(new PlayerEntity(player));
            return await table.ExecuteAsync(insertOperation);
        }

        public async Task<TableResult> UpdatePlayer(Player player)
        {
            var table = _tableClient.GetTableReference(TableNames.Player);
            TableOperation updateOperation = TableOperation.Merge(new PlayerEntity(player));
            return await table.ExecuteAsync(updateOperation);
        }

        public async Task<TableResult> DeletePlayer(Guid playerId)
        {
            var table = _tableClient.GetTableReference(TableNames.Player);
            TableOperation retrieveOperation = TableOperation.Retrieve(TableNames.Player, playerId.ToString("N"));
            TableResult retrieveResult = await table.ExecuteAsync(retrieveOperation);
            if (retrieveResult.Result is PlayerEntity player)
            {
                TableOperation deleteOperation = TableOperation.Delete(player);
                return await table.ExecuteAsync(deleteOperation);
            }
            else
            {
                return retrieveResult;
            }
        }
    }
}
