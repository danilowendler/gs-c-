using System.Net;
using System.Text.Json;

namespace Refresca.Api.Infrastructure.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (KeyNotFoundException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            await WriteError(context, "Recurso não encontrado", ex.Message);
        }
        catch (ArgumentException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await WriteError(context, "Dados inválidos", ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro não tratado na API Refresca.");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await WriteError(context, "Erro interno", "Ocorreu um erro inesperado.");
        }
    }

    private static async Task WriteError(HttpContext context, string title, string detail)
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            status = context.Response.StatusCode,
            title,
            detail
        }));
    }
}
