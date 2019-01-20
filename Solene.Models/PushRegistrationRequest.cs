using System;
using System.Collections.Generic;
using System.Text;

namespace Solene.Models
{
    public class PushRegistrationRequest
    {
        public PushNotificationPlatform PushPlatform { get; set; }
        public string PnsToken { get; set; }
        public string PlatformPushTemplate { get; set; }
    }
}
