using System;
using System.Collections.Generic;

namespace Solene.Models
{
    public class Player
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public List<Question> Questions { get; set; }
    }
}
