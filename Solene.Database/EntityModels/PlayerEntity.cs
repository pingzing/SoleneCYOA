using Microsoft.Azure.Cosmos.Table;
using Solene.Models;
using System;
using System.Collections.Generic;

namespace Solene.Database.EntityModels
{
    internal class PlayerEntity : TableEntity
    {
        public PlayerEntity() { }

        public PlayerEntity(Player player)
        {
            PartitionKey = PartitionKeys.Player;
            RowKey = player.Id.ToString("N");
            Name = player.Name;
            Gender = player.Gender;            
        }

        public string Name { get; set; }
        public string Gender { get; set; }        

        public Player ToPlayer()
        {
            return new Player
            {
                Gender = Gender,
                Id = Guid.Parse(RowKey),
                Name = Name,
            };
        }
    }
}
