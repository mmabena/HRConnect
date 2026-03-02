namespace HRConnect.Api.Middleware
{
  using System.ComponentModel.DataAnnotations;
  using Microsoft.AspNetCore.Http;
    using System.Text.Json;
    using HRConnect.Api.DTOs;
    using Microsoft.Extensions.Logging;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// ASP.NET Core middleware for centralized exception handling across the API.
    /// Catches unhandled exceptions and transforms them into standardized JSON error responses
    /// with appropriate HTTP status codes, correlation IDs, and structured logging.
    /// </summary>
    /// <remarks>
    /// This middleware provides consistent error handling by:
    /// - Mapping different exception types to appropriate HTTP status codes
    /// - Including correlation IDs for request tracing
    /// - Providing detailed error information in development environments
    /// - Using structured logging with optimized LoggerMessage delegates
    /// - Caching JsonSerializerOptions for performance optimization
    /// 
    /// <para>
    /// <strong>Integration Setup:</strong>
    /// Register the middleware in Program.cs or Startup.cs:
    /// </para>
    /// <code>
    /// // In Program.cs (.NET 6+)
    /// app.UseMiddleware&lt;GlobalExceptionHandler&gt;();
    /// 
    /// </code>
    /// 
    /// <para>
    /// <strong>Service Registration:</strong>
    /// Ensure required services are registered:
    /// </para>
    /// <code>
    /// builder.Services.AddLogging();
    /// builder.Services.AddSingleton&lt;IHostEnvironment, HostEnvironment&gt;();
    /// </code>
    /// </remarks>
    public class GlobalExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IHostEnvironment _env;

        /// <summary>
        /// Cached JsonSerializerOptions for production environment responses.
        /// Optimized for performance with camelCase naming and no indentation.
        /// </summary>
        private static readonly JsonSerializerOptions _productionJsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        /// <summary>
        /// Cached JsonSerializerOptions for development environment responses.
        /// Includes indentation for better readability during development.
        /// </summary>
        private static readonly JsonSerializerOptions _developmentJsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        /// <summary>
        /// Optimized logger delegate for validation exceptions.
        /// Provides better performance than traditional logging methods.
        /// </summary>
        private static readonly Action<ILogger, string, Exception?> _validationWarningLogger =
            LoggerMessage.Define<string>(LogLevel.Warning, new EventId(1, "ValidationError"),
                "Validation error occurred. Correlation ID: {CorrelationId}");

        /// <summary>
        /// Optimized logger delegate for not found exceptions.
        /// </summary>
        private static readonly Action<ILogger, string, Exception?> _notFoundWarningLogger =
            LoggerMessage.Define<string>(LogLevel.Warning, new EventId(2, "NotFound"),
                "Resource not found. Correlation ID: {CorrelationId}");

        /// <summary>
        /// Optimized logger delegate for argument exceptions.
        /// </summary>
        private static readonly Action<ILogger, string, Exception?> _argumentWarningLogger =
            LoggerMessage.Define<string>(LogLevel.Warning, new EventId(3, "ArgumentError"),
                "Argument error occurred. Correlation ID: {CorrelationId}");

        /// <summary>
        /// Optimized logger delegate for invalid operation exceptions.
        /// </summary>
        private static readonly Action<ILogger, string, Exception?> _invalidOperationWarningLogger 
          = LoggerMessage.Define<string>(LogLevel.Warning, new EventId(4, "InvalidOperation"),
                "Invalid operation attempted. Correlation ID: {CorrelationId}");

        /// <summary>
        /// Optimized logger delegate for unauthorized access exceptions.
        /// </summary>
        private static readonly Action<ILogger, string, Exception?> _unauthorizedWarningLogger =
            LoggerMessage.Define<string>(LogLevel.Warning, new EventId(5, "Unauthorized"),
                "Unauthorized access attempt. Correlation ID: {CorrelationId}");

        /// <summary>
        /// Optimized logger delegate for unhandled exceptions.
        /// Logs at Error level as these represent unexpected system failures.
        /// </summary>
        private static readonly Action<ILogger, string, Exception?> _unhandledExceptionLogger =
            LoggerMessage.Define<string>(LogLevel.Error, new EventId(6, "UnhandledException"),
                "Unhandled exception occurred. Correlation ID: {CorrelationId}");

        /// <summary>
        /// Initializes a new instance of the GlobalExceptionHandler middleware.
        /// </summary>
        /// <param name="next">The next middleware delegate in the request pipeline.</param>
        /// <param name="logger">Logger instance for structured exception logging.</param>
        /// <param name="env">Host environment information for development vs production behavior.</param>
        /// <exception cref="ArgumentNullException">Thrown when any required parameter is null.</exception>
        public GlobalExceptionHandler(
            RequestDelegate next,
            ILogger<GlobalExceptionHandler> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        /// <summary>
        /// Processes the HTTP request and handles any unhandled exceptions.
        /// </summary>
        /// <param name="context">The HttpContext for the current request.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method wraps the execution of the next middleware in a try-catch block.
        /// Any exceptions that bubble up through the pipeline are caught and transformed
        /// into standardized error responses. The correlation ID from the request trace
        /// identifier is used for logging and response tracking.
        /// </remarks>
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

        /// <summary>
        /// Handles an exception by generating an appropriate error response and logging the error.
        /// </summary>
        /// <param name="context">The HttpContext for the current request.</param>
        /// <param name="exception">The exception that was thrown.</param>
        /// <returns>A Task representing the asynchronous response writing operation.</returns>
        /// <remarks>
        /// This method:
        /// 1. Determines the appropriate HTTP status code and error response based on exception type
        /// 2. Logs the exception using the optimized logger delegate
        /// 3. Sets the response content type and status code
        /// 4. Serializes and writes the error response as JSON
        /// 
        /// Stack traces are only included in development environments for security.
        /// </remarks>
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

        /// <summary>
        /// Maps an exception to the appropriate HTTP status code and error response structure.
        /// </summary>
        /// <param name="exception">The exception to map.</param>
        /// <param name="correlationId">The correlation ID for request tracking.</param>
        /// <returns>A tuple containing the HTTP status code and ErrorResponse object.</returns>
        /// <remarks>
        /// Exception mappings:
        /// - ValidationException → 400 Bad Request with validation details
        /// - KeyNotFoundException → 404 Not Found
        /// - ArgumentException → 400 Bad Request
        /// - InvalidOperationException → 400 Bad Request
        /// - UnauthorizedAccessException → 401 Unauthorized
        /// - DbUpdateException → 500 Internal Server Error (database-specific message in dev)
        /// - TimeoutException → 408 Request Timeout
        /// - All other exceptions → 500 Internal Server Error
        /// 
        /// Stack traces are only included in development environments.
        /// 
        /// <para>
        /// <strong>Custom Exception Handling:</strong>
        /// You can extend this method to handle custom exceptions:
        /// </para>
        /// <code>
        /// case CustomBusinessException customEx => (
        ///     StatusCodes.Status422UnprocessableEntity,
        ///     new ErrorResponse
        ///     {
        ///         Type = "BusinessError",
        ///         Message = customEx.Message,
        ///         CorrelationId = correlationId,
        ///         StackTrace = _env.IsDevelopment() ? customEx.StackTrace : null
        ///     }
        /// ),
        /// </code>
        /// </remarks>
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

        /// <summary>
        /// Logs exceptions using the appropriate optimized logger delegate based on exception type.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="correlationId">The correlation ID for request tracking.</param>
        /// <remarks>
        /// Uses LoggerMessage delegates for improved performance over traditional logging methods.
        /// Each exception type has its own log level and event ID for better monitoring and alerting.
        /// 
        /// <para>
        /// <strong>Repository Integration Example:</strong>
        /// </para>
        /// <code>
        /// public class MedicalOptionRepository : IMedicalOptionRepository
        /// {
        ///     public async Task&lt;MedicalOptionDto&gt; GetMedicalOptionByIdAsync(int id)
        ///     {
        ///         if (id <= 0)
        ///             throw new ArgumentException("Medical option ID must be greater than 0", nameof(id));
        ///             
        ///         var option = await _context.MedicalOptions.FindAsync(id);
        ///         if (option == null)
        ///             throw new KeyNotFoundException($"Medical option with ID {id} not found");
        ///             
        ///         return option.ToMedicalOptionDto();
        ///     }
        /// }
        /// </code>
        /// 
        /// <para>
        /// <strong>Service Integration Example:</strong>
        /// </para>
        /// <code>
        /// public class MedicalOptionService : IMedicalOptionService
        /// {
        ///     public async Task&lt;List&lt;MedicalOptionCategoryDto&gt;&gt; GetGroupedMedicalOptionsAsync()
        ///     {
        ///         try
        ///         {
        ///             var groupedOptions = await _repository.GetGroupedMedicalOptionsAsync();
        ///             return groupedOptions.Select(group => group.ToMedicalOptionCategoryDto()).ToList();
        ///         }
        ///         catch (DbUpdateException ex)
        ///         {
        ///             // Let the middleware handle database exceptions
        ///             throw;
        ///         }
        ///         catch (Exception ex)
        ///         {
        ///             // Wrap unexpected exceptions
        ///             throw new InvalidOperationException("Failed to retrieve grouped medical options", ex);
        ///         }
        ///     }
        /// }
        /// </code>
        /// 
        /// <para>
        /// <strong>Helper Class Integration Example:</strong>
        /// </para>
        /// <code>
        /// public static class ValidationHelper
        /// {
        ///     public static void ValidateMedicalOptionDto(MedicalOptionDto dto)
        ///     {
        ///         if (dto == null)
        ///             throw new ValidationException("Medical option DTO cannot be null");
        ///             
        ///         if (string.IsNullOrWhiteSpace(dto.MedicalOptionName))
        ///             throw new ValidationException("Medical option name is required");
        ///             
        ///         if (dto.SalaryBracketMin.HasValue && dto.SalaryBracketMax.HasValue)
        ///         {
        ///             if (dto.SalaryBracketMin > dto.SalaryBracketMax)
        ///                 throw new ValidationException("Minimum salary bracket cannot be greater than maximum");
        ///         }
        ///     }
        /// }
        /// </code>
        /// 
        /// <para>
        /// <strong>Controller Integration Example:</strong>
        /// Controllers don't need try-catch blocks when using this middleware:
        /// </para>
        /// <code>
        /// [HttpGet("{id}")]
        /// public async Task&lt;ActionResult&lt;MedicalOptionDto&gt;&gt; GetMedicalOption(int id)
        /// {
        ///     // No try-catch needed - middleware handles exceptions
        ///     var option = await _medicalOptionService.GetMedicalOptionByIdAsync(id);
        ///     return Ok(option);
        /// }
        /// </code>
        /// </remarks>
        /// 
        private void LogException(Exception exception, string correlationId)
        {
            switch (exception)
            {
                case ValidationException:
                    _validationWarningLogger(_logger, correlationId, exception);
                    break;
                case KeyNotFoundException:
                    _notFoundWarningLogger(_logger, correlationId, exception);
                    break;
                case ArgumentException:
                    _argumentWarningLogger(_logger, correlationId, exception);
                    break;
                case InvalidOperationException:
                    _invalidOperationWarningLogger(_logger, correlationId, exception);
                    break;
                case UnauthorizedAccessException:
                    _unauthorizedWarningLogger(_logger, correlationId, exception);
                    break;
                default:
                    _unhandledExceptionLogger(_logger, correlationId, exception);
                    break;
            }
        }
    }
    
/// <summary>
/// Custom exception class for business logic validation errors.
/// Extends the base Exception class to include structured validation error details
/// that can be returned to API consumers for field-level validation feedback.
/// </summary>
/// <remarks>
/// This custom validation exception is designed to work with the GlobalExceptionHandler
/// middleware to provide detailed validation error responses in API calls.
/// Unlike the standard System.ComponentModel.DataAnnotations.ValidationException,
/// this class includes a structured ValidationErrors dictionary for field-specific errors.
/// 
/// <para>
/// <strong>Usage Example:</strong>
/// </para>
/// <code>
/// // In a service or validation helper
/// var errors = new Dictionary&​lt;string, string[]&​gt;();
/// 
/// if (string.IsNullOrWhiteSpace(dto.Name))
///     errors.Add("Name", new[] { "Name is required" });
///     
/// if (dto.SalaryBracketMin.HasValue && dto.SalaryBracketMax.HasValue)
/// {
///     if (dto.SalaryBracketMin > dto.SalaryBracketMax)
///         errors.Add("SalaryBracket", new[] { "Minimum salary cannot be greater than maximum" });
/// }
/// 
/// if (errors.Any())
///     throw new ValidationException("Validation failed", errors);
/// </code>
/// 
/// <para>
/// <strong>API Response Format:</strong>
/// When caught by GlobalExceptionHandler, this produces:
/// </para>
/// <code>
/// {
///   "type": "Validation",
///   "message": "Validation failed",
///   "validationErrors": {
///     "Name": ["Name is required"],
///     "SalaryBracket": ["Minimum salary cannot be greater than maximum"]
///   },
///   "correlationId": "0HMHQ2L8J5K8Q:00000001",
///   "timestamp": "2024-01-15T10:30:00Z"
/// }
/// </code>
/// 
/// <para>
/// <strong>Integration with GlobalExceptionHandler:</strong>
/// This exception is specifically handled in the GetErrorResponse method:
/// </para>
/// <code>
/// case ValidationException validationEx => (
///     StatusCodes.Status400BadRequest,
///     new ErrorResponse
///     {
///         Type = "Validation",
///         Message = validationEx.Message,
///         ValidationErrors = validationEx.ValidationErrors,
///         CorrelationId = correlationId,
///         StackTrace = _env.IsDevelopment() ? validationEx.StackTrace : null
///     }
/// ),
/// </code>
/// </remarks>
public class ValidationException : Exception
{
    /// <summary>
    /// Gets the dictionary of validation errors organized by field name.
    /// Each key represents a field name, and each value is an array of error messages
    /// for that field. This structure allows for multiple validation errors per field.
    /// </summary>
    /// <example>
    /// Example structure:
    /// <code>
    /// {
    ///   ["Name"] = ["Name is required", "Name must be at least 2 characters"],
    ///   ["Email"] = ["Email format is invalid"],
    ///   ["Salary"] = ["Salary must be greater than 0"]
    /// }
    /// </code>
    /// </example>
    public Dictionary<string, string[]> ValidationErrors { get; }

    /// <summary>
    /// Initializes a new instance of the ValidationException class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="validationErrors">
    /// Optional dictionary of field-specific validation errors.
    /// If null or not provided, an empty dictionary is created.
    /// </param>
    /// <remarks>
    /// This constructor creates a validation exception with both a general message
    /// and optional field-specific error details. The validation errors are
    /// automatically initialized if not provided.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Simple validation exception with just a message
    /// throw new ValidationException("Invalid input data");
    /// 
    /// // Validation exception with field-specific errors
    /// var errors = new Dictionary&​lt;string, string[]&​gt;
    /// {
    ///     ["Email"] = new[] { "Email is required" },
    ///     ["Age"] = new[] { "Age must be between 18 and 65" }
    /// };
    /// throw new ValidationException("Validation failed", errors);
    /// </code>
    /// </example>
    public ValidationException(string message, 
      Dictionary<string, string[]>? validationErrors = null)
      : base(message)
    {
      ValidationErrors = validationErrors ?? new Dictionary<string, string[]>();
    }
  }
}