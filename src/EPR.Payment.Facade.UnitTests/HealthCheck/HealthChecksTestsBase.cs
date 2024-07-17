using System.Net;

namespace EPR.Payment.Facade.UnitTests.HealthCheck
{
    public class HealthChecksTestsBase
    {
        protected static HttpResponseMessage ResponseMessageOk => BuildResponseMessage(HttpStatusCode.OK);
        protected static HttpResponseMessage ResponseMessageBadRequest => BuildResponseMessage(HttpStatusCode.BadRequest);

        private static HttpResponseMessage BuildResponseMessage(HttpStatusCode statusCode)
        {
            return new HttpResponseMessage
            {
                StatusCode = statusCode,
                Version = new Version()
            };
        }
    }
}