namespace RedConnect.Middleware;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using RedConnect.Exceptions;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var referer = context.Request.Headers["Referer"].ToString();

            string redirectUrl = "/"; // default fallback

            if (!string.IsNullOrEmpty(referer))
            {
                try
                {
                    var uri = new Uri(referer);

                    // Ensure it's from same host 
                    if (uri.Host == context.Request.Host.Host)
                    {
                        redirectUrl = uri.PathAndQuery;
                    }
                }
                catch
                {
                    // ignore invalid URL
                }
            }

            // Add error parameter
            var separator = redirectUrl.Contains("?") ? "&" : "?";

            context.Response.Redirect(
                redirectUrl + $"{separator}error=" + Uri.EscapeDataString(ex.Message)
            );

            return;
        }
    }
}