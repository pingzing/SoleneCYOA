using Solene.Models;

namespace Solene.MobileApp.Core.Services
{
    public static class NetworkMaybeResult
    {
        public static MaybeResult<T, GenericErrorResult> Success<T>(T success)
        {
            return MaybeResult<T, GenericErrorResult>.CreateOk(success);
        }

        public static MaybeResult<T, GenericErrorResult> Failure<T>(GenericErrorResult error)
        {
            return MaybeResult<T, GenericErrorResult>.CreateError(error);
        }
    }
}
