﻿using Grpc.Core;

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

        public static string GetInternalExceptionMessage(Exception exception)
        {
            return "Oops, internal error happened!" + Environment.NewLine +
                   "Exception message:" + Environment.NewLine + exception.Message + Environment.NewLine +
                   "Exception stack trace:" + Environment.NewLine + exception.StackTrace;
        }

        public static void ThrowInternalException(Exception exception)
        {
            string exceptionMessage = GetInternalExceptionMessage(exception);
            throw new RpcException(new Status(StatusCode.Internal, exceptionMessage));
        }

        public static void ThrowBadRequestException(Exception exception)
        {
            string exceptionMessage = "Oops! Your request seems to be incorrect" + Environment.NewLine +
                                      "Exception message:" + Environment.NewLine + exception.Message + Environment.NewLine +
                                      "Exception stack trace:" + Environment.NewLine + exception.StackTrace;
            throw new RpcException(new Status(StatusCode.InvalidArgument, exceptionMessage));
        }
    }
}
