using System.Net;

namespace BizlyWeb.Services.Exceptions
{
    public class ApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string? ResponseContent { get; }

        public ApiException(string message, HttpStatusCode statusCode, string? responseContent, Exception? inner = null)
            : base(message, inner)
        {
            StatusCode = statusCode;
            ResponseContent = responseContent;
        }
    }
}

