using Solene.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Solene.MobileApp.Core.Models
{
    public class PlayerProfile
    {
        public Player PlayerInfo { get; set; }
        public List<Question> Questions { get; set; }
    }
}
