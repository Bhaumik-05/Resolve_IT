using System.Text.Json;

namespace rsit.Middleware;

public class ValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ValidationMiddleware> _logger;

    public ValidationMiddleware(
        RequestDelegate next,
        ILogger<ValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(
                ex,
                "Validation error occurred.");

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode =
                    StatusCodes.Status400BadRequest;

                context.Response.ContentType =
                    "application/json";

                var response = new
                {
                    success = false,
                    message = "Invalid request.",
                    error = ex.Message
                };

                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(response));
            }
        }
    }
}