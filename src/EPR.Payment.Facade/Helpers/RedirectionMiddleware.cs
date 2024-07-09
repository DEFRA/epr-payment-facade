public class RedirectionMiddleware
{
    private readonly RequestDelegate _next;

    public RedirectionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Response.StatusCode == StatusCodes.Status302Found && context.Response.Headers.ContainsKey("Location"))
        {
            var redirectUrl = context.Response.Headers["Location"].ToString();
            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync($"<html><head><meta http-equiv='refresh' content='0;url={redirectUrl}' /></head><body>Redirecting to <a href='{redirectUrl}'>{redirectUrl}</a></body></html>");
        }
        else
        {
            await _next(context);
        }
    }
}
