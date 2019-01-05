using Microsoft.Azure.CosmosDB.Table;
using Solene.Models;
using System.Collections.Generic;

namespace Solene.Database.EntityModels
{
    internal class PlayerEntity : TableEntity
    {
        public PlayerEntity() { }

        public PlayerEntity(Player player)
        {
            PartitionKey = TableNames.Player;
            RowKey = player.Id.ToString("N");
            Name = player.Name;
            Gender = player.Gender;
            Questions = player.Questions;
        }

        public string Name { get; set; }
        public string Gender { get; set; }
        public List<Question> Questions { get; set; }
    }
}
