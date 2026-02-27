// HRConnect.Api/Middleware/GlobalExceptionHandler.cs
namespace HRConnect.Api.Middleware
{
    using Microsoft.AspNetCore.Http;
    using System.Text.Json;
    using HRConnect.Api.DTOs;
    using Microsoft.Extensions.Logging;
    using Microsoft.EntityFrameworkCore;

    public class GlobalExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IHostEnvironment _env;

        // Cache both development and production JsonSerializerOptions - creating a new one on each
        // call is not performance efficient
        private static readonly JsonSerializerOptions _productionJsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        private static readonly JsonSerializerOptions _developmentJsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        // LoggerMessage delegates for better performance
        private static readonly Action<ILogger, string, Exception?> _validationWarningLogger =
            LoggerMessage.Define<string>(LogLevel.Warning, new EventId(1, "ValidationError"),
                "Validation error occurred. Correlation ID: {CorrelationId}");

        private static readonly Action<ILogger, string, Exception?> _notFoundWarningLogger =
            LoggerMessage.Define<string>(LogLevel.Warning, new EventId(2, "NotFound"),
                "Resource not found. Correlation ID: {CorrelationId}");

        private static readonly Action<ILogger, string, Exception?> _argumentWarningLogger =
            LoggerMessage.Define<string>(LogLevel.Warning, new EventId(3, "ArgumentError"),
                "Argument error occurred. Correlation ID: {CorrelationId}");

        private static readonly Action<ILogger, string, Exception?> _invalidOperationWarningLogger 
          = LoggerMessage.Define<string>(LogLevel.Warning, new EventId(4, "InvalidOperation"),
                "Invalid operation attempted. Correlation ID: {CorrelationId}");

        private static readonly Action<ILogger, string, Exception?> _unauthorizedWarningLogger =
            LoggerMessage.Define<string>(LogLevel.Warning, new EventId(5, "Unauthorized"),
                "Unauthorized access attempt. Correlation ID: {CorrelationId}");

        private static readonly Action<ILogger, string, Exception?> _unhandledExceptionLogger =
            LoggerMessage.Define<string>(LogLevel.Error, new EventId(6, "UnhandledException"),
                "Unhandled exception occurred. Correlation ID: {CorrelationId}");

        public GlobalExceptionHandler(
            RequestDelegate next,
            ILogger<GlobalExceptionHandler> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var correlationId = context.TraceIdentifier;
            var (statusCode, errorResponse) = GetErrorResponse(exception, correlationId);

            // Log the exception using optimized logger delegates
            LogException(exception, correlationId);

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            // Use pre-cached JsonSerializerOptions based on environment
            var jsonOptions = _env.IsDevelopment() ? _developmentJsonOptions : _productionJsonOptions;
            var jsonResponse = JsonSerializer.Serialize(errorResponse, jsonOptions);
            await context.Response.WriteAsync(jsonResponse);
        }

        private (int statusCode, ErrorResponse response) GetErrorResponse(Exception exception, string correlationId)
        {
            return exception switch
            {
                ValidationException validationEx => (
                    StatusCodes.Status400BadRequest,
                    new ErrorResponse
                    {
                        Type = "Validation",
                        Message = validationEx.Message,
                        ValidationErrors = validationEx.ValidationErrors,
                        CorrelationId = correlationId,
                        StackTrace = _env.IsDevelopment() ? validationEx.StackTrace : null
                    }
                ),
                
                KeyNotFoundException keyEx => (
                    StatusCodes.Status404NotFound,
                    new ErrorResponse
                    {
                        Type = "NotFound",
                        Message = $"Resource not found: {keyEx.Message}",
                        CorrelationId = correlationId,
                        StackTrace = _env.IsDevelopment() ? keyEx.StackTrace : null
                    }
                ),
                
                ArgumentException argEx => (
                    StatusCodes.Status400BadRequest,
                    new ErrorResponse
                    {
                        Type = "ArgumentError",
                        Message = argEx.Message,
                        CorrelationId = correlationId,
                        StackTrace = _env.IsDevelopment() ? argEx.StackTrace : null
                    }
                ),
                
                InvalidOperationException invalidOpEx => (
                    StatusCodes.Status400BadRequest,
                    new ErrorResponse
                    {
                        Type = "InvalidOperation",
                        Message = invalidOpEx.Message,
                        CorrelationId = correlationId,
                        StackTrace = _env.IsDevelopment() ? invalidOpEx.StackTrace : null
                    }
                ),
                
                UnauthorizedAccessException unauthorizedEx => (
                    StatusCodes.Status401Unauthorized,
                    new ErrorResponse
                    {
                        Type = "Unauthorized",
                        Message = "Unauthorized access",
                        CorrelationId = correlationId,
                        StackTrace = _env.IsDevelopment() ? unauthorizedEx.StackTrace : null
                    }
                ),
                
                DbUpdateException dbEx => (
                    StatusCodes.Status500InternalServerError,
                    new ErrorResponse
                    {
                        Type = "DatabaseError",
                        Message = _env.IsDevelopment() 
                            ? $"Database error: {dbEx.Message}"
                            : "An error occurred while accessing the database",
                        CorrelationId = correlationId,
                        StackTrace = _env.IsDevelopment() ? dbEx.StackTrace : null
                    }
                ),
                
                TimeoutException timeoutEx => (
                    StatusCodes.Status408RequestTimeout,
                    new ErrorResponse
                    {
                        Type = "Timeout",
                        Message = "Request timed out",
                        CorrelationId = correlationId,
                        StackTrace = _env.IsDevelopment() ? timeoutEx.StackTrace : null
                    }
                ),
                
                _ => (
                    StatusCodes.Status500InternalServerError,
                    new ErrorResponse
                    {
                        Type = "InternalServerError",
                        Message = _env.IsDevelopment() 
                            ? exception.Message 
                            : "An unexpected error occurred. Please try again later.",
                        CorrelationId = correlationId,
                        StackTrace = _env.IsDevelopment() ? exception.StackTrace : null
                    }
                )
            };
        }

        private void LogException(Exception exception, string correlationId)
        {
            switch (exception)
            {
                case ValidationException validationEx:
                    _validationWarningLogger(_logger, correlationId, validationEx);
                    break;
                case KeyNotFoundException keyEx:
                    _notFoundWarningLogger(_logger, correlationId, keyEx);
                    break;
                case ArgumentException argEx:
                    _argumentWarningLogger(_logger, correlationId, argEx);
                    break;
                case InvalidOperationException invalidOpEx:
                    _invalidOperationWarningLogger(_logger, correlationId, invalidOpEx);
                    break;
                case UnauthorizedAccessException unauthorizedEx:
                    _unauthorizedWarningLogger(_logger, correlationId, unauthorizedEx);
                    break;
                default:
                    _unhandledExceptionLogger(_logger, correlationId, exception);
                    break;
            }
        }
    }

    // Extension method for easy registration
    public static class GlobalExceptionHandlerExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(
          this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionHandler>();
        }
    }

    // Custom validation exception for business logic validation
    public class ValidationException : Exception
    {
        public Dictionary<string, string[]> ValidationErrors { get; }

        public ValidationException(string message, 
          Dictionary<string, string[]>? validationErrors = null)
            : base(message)
        {
            ValidationErrors = validationErrors ?? new Dictionary<string, string[]>();
        }
    }
}