using Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace Api.Middlewares;

public class ApiExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiExceptionHandlingMiddleware> _logger;

    public ApiExceptionHandlingMiddleware(RequestDelegate next, ILogger<ApiExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            string result = string.Empty;

            switch (ex)
            {
                case ApiException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    Handle400Error(ex, ref result);
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    HandleInternalServerError(ex, ref result);
                    break;
            }

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(result);
        }
    }

    // used ref keyword to change result value from inside this function
    private void Handle400Error(Exception ex, ref string result)
    {
        _logger.LogError("Api Exception : {message}", ex.Message);

        var error = new Dictionary<string, string[]> { { "Error", new[] { ex.Message } } };
        var problemDetails = new ValidationProblemDetails(error)
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "One or more validation errors occurred.",
            Status = (int)HttpStatusCode.BadRequest
        };
        result = JsonSerializer.Serialize(problemDetails);
    }

    private void HandleInternalServerError(Exception ex, ref string result)
    {
        _logger.LogError(ex, $"An unhandled exception has occurred, {ex.Message}");
        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "Internal Server Error.",
            Status = (int)HttpStatusCode.InternalServerError,
            Detail = "Internal Server Error!"
        };
        result = JsonSerializer.Serialize(problemDetails);
    }
}
