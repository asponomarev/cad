using Grpc.Core;

namespace UhlnocsServer.Utils
{
    public static class ExceptionUtils
    {
        public static void ThrowUnknownException(Exception exception)
        {
            string exceptionMessage = "Oops! Unknown error happened, please report it to developers" + Environment.NewLine +
                                      "Exception message:" + Environment.NewLine + exception.Message + Environment.NewLine +
                                      "Exception stack trace:" + Environment.NewLine + exception.StackTrace;
            throw new RpcException(new Status(StatusCode.Unknown, exceptionMessage));
        }
    }
}
