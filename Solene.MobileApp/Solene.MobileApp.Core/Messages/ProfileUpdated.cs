using Solene.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Solene.MobileApp.Core.Messages
{
    public class ProfileUpdated
    {
        public Guid ProfileId { get; set; }
        public Question NewQuestion { get; set; }
    }
}
