using System.Net;

namespace EPR.Payment.Facade.Common.Exceptions
{
    [Serializable]
    public class ResponseCodeException : Exception
    {
        public ResponseCodeException(HttpStatusCode statusCode, string message) : base(message)
        {
            this.StatusCode = statusCode;
        }

        public ResponseCodeException(HttpStatusCode statusCode)
        {
            this.StatusCode = statusCode;
        }

        public HttpStatusCode StatusCode { get; set; }
    }
}
