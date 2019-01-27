using Solene.Models;
using System.Collections.Generic;

namespace Solene.AdminClient.Models
{
    public class AdminPlayerProfile
    {
        public Player PlayerInfo { get; set; }
        public List<AdminQuestion> Questions { get; set; }
    }
}
