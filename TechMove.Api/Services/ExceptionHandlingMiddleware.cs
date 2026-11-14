using System.Net;
using System.Text.Json;

namespace TechMove.Api.Services;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (BusinessRuleException exception)
        {
            await WriteProblemAsync(context, HttpStatusCode.BadRequest, exception.Message);
        }
        catch (NotFoundException exception)
        {
            await WriteProblemAsync(context, HttpStatusCode.NotFound, exception.Message);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled API exception.");
            await WriteProblemAsync(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, HttpStatusCode statusCode, string detail)
    {
        if (context.Response.HasStarted) return;
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new { status = (int)statusCode, detail }));
    }
}
