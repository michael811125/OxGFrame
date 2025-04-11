using System;

namespace OxGFrame.CenterFrame.APICenter
{
    public struct ErrorInfo
    {
        public string url;
        public string message;
        public Exception exception;
    }

    public delegate void ResponseHandle(string response);
    public delegate void ResponseErrorHandle(ErrorInfo errorInfo);
}