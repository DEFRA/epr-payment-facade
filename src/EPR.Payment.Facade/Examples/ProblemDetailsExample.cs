using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace EPR.Payment.Facade.Examples;

public class ProblemDetailsExample : IExamplesProvider<ProblemDetails>
{
    public ProblemDetails GetExamples()
    {
        var examples = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Title = "Bad request",
            Status = new int?(400)
        };
        examples.Extensions.Add("traceId", (object)"00-11dc1a21a6bc20489d3009fb27d57b87-ddc5556eac52a943-00");
        return examples;
    }
}
