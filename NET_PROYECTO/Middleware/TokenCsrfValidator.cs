using Microsoft.AspNetCore.Antiforgery;


public class TokenCsrfValidator
{

    private readonly RequestDelegate _next;
    private readonly IAntiforgery _antiforgery;

    public TokenCsrfValidator(RequestDelegate next, IAntiforgery antiforgery)
    {
        _next = next;
        _antiforgery = antiforgery;
    }

    public async Task InvokeAsync(HttpContext context, IAntiforgery antiforgery)
    {
        var routePrefixes = new[] { "/Auth" };

        // Check if the current request path matches any of the route prefixes
        if (routePrefixes.Any(prefix => context.Request.Path.StartsWithSegments(prefix)) ||

            (!HttpMethods.IsPost(context.Request.Method) &&
            !HttpMethods.IsPut(context.Request.Method) &&
            !HttpMethods.IsDelete(context.Request.Method)))
        {
            await _next(context); // Skip middleware logic for these routes
            return;
        }


        try
        {
            await antiforgery.ValidateRequestAsync(context);
            await _next(context);

        }
        catch (AntiforgeryValidationException) // Catch the specific CSRF validation exception
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                Error = "CSRF validation failed"
            };

            // Serialize the response to JSON and send it
            await context.Response.WriteAsJsonAsync(errorResponse);

        }
        catch (Exception) // Catch any other exceptions that may occur
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync("An unexpected error occurred.");
        }

    }
}

public static class TokenCsrfValidatorExtension
{
    public static IApplicationBuilder UseCsrfTokenValidator(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TokenCsrfValidator>();
    }
}

