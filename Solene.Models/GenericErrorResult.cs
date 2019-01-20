using System.Net;

namespace Solene.Models
{
    public enum GenericErrorResult
    {
        BadRequest,
        NoResponse,
        NotFound,
        RemoteError, 
        Unknown = 9999,
    }

    public static class HttpStatusCodeExtensions
    {
        public static GenericErrorResult ToErrorCode(this HttpStatusCode statusCode)
        {
            switch (statusCode)
            {
                case HttpStatusCode.BadRequest:
                    return GenericErrorResult.BadRequest;
                case HttpStatusCode.NotFound:
                    return GenericErrorResult.NotFound;
                case HttpStatusCode.InternalServerError:
                    return GenericErrorResult.RemoteError;
                default:
                    return GenericErrorResult.Unknown;

            }
        }
    }
}
