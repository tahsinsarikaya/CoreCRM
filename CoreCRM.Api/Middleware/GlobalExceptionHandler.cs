using CoreCRM.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CoreCRM.Api.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var (statusCode, title) = MapException(exception);

            _logger.LogError(exception, "Request failed with {StatusCode}: {Title}", statusCode, title);

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = exception.Message
            };

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }

        private static (int StatusCode, string Title) MapException(Exception exception) => exception switch
        {
            EntityDefinitionHasRecordsException ex
                => (StatusCodes.Status409Conflict, $"Cannot delete: {ex.RecordCount} record(s) still reference this entity definition"),

            // Aynı isimde EntityDefinition veya aynı entity içinde aynı isimde field çakışması
            DbUpdateException { InnerException: PostgresException { SqlState: "23505" } }
                => (StatusCodes.Status409Conflict, "Duplicate entry"),

            // FieldDefinition var olan bir kayda referans veriyorken EntityDefinition silinmeye çalışılırsa (Restrict)
            DbUpdateException { InnerException: PostgresException { SqlState: "23503" } }
                => (StatusCodes.Status409Conflict, "Referenced by existing records"),

            // AddField() içindeki "alan zaten var" kontrolü, veya domain constructor'larındaki validasyonlar
            InvalidOperationException => (StatusCodes.Status409Conflict, "Invalid operation"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request"),

            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };
    }
}
