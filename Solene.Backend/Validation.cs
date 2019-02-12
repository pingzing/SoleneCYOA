using System;


namespace Solene.Backend
{
    public static class Validation
    {
        public static bool TryValidateGuid(string guidString, out Guid validGuid)
        {
            if (string.IsNullOrWhiteSpace(guidString))
            {
                return false;
            }

            if (!Guid.TryParse(guidString, out validGuid))
            {
                return false;
            }

            return true;
        }
    }
}
