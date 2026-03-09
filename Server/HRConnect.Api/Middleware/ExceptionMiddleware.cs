using System.Net;
using System.Text.Json;
using HRConnect.Api.Services;
using Microsoft.AspNetCore.Http;

namespace HRConnect.Api.Middleware
{

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
            catch (ValidationException ex)
            {
                await HandleException(context, ex, HttpStatusCode.BadRequest);
            }
            catch (BusinessRuleException ex)
            {
                await HandleException(context, ex, HttpStatusCode.BadRequest);
            }
            catch (NotFoundException ex)
            {
                await HandleException(context, ex, HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                await HandleException(context, ex, HttpStatusCode.InternalServerError);
            }
        }

        private static async Task HandleException(HttpContext context, Exception ex, HttpStatusCode code)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            var response = new
            {
                errors = MapMessageToField(ex.Message)
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }

        private static Dictionary<string, string> MapMessageToField(string message)
        {
            var errors = new Dictionary<string, string>();

            if (message.Contains("Email", StringComparison.OrdinalIgnoreCase))
                errors["email"] = message;
            else if (message.Contains("ContactNumber", StringComparison.OrdinalIgnoreCase))
                errors["contactNumber"] = message;
            else if (message.Contains("Tax Number", StringComparison.OrdinalIgnoreCase))
                errors["taxNumber"] = message;
            else if (message.Contains("ID Number", StringComparison.OrdinalIgnoreCase))
                errors["idNumber"] = message;
            else if (message.Contains("Start date", StringComparison.OrdinalIgnoreCase))
                errors["startDate"] = message;
            else if (message.Contains("Position", StringComparison.OrdinalIgnoreCase))
                errors["jobTitle"] = message;
            else if (message.Contains("Branch", StringComparison.OrdinalIgnoreCase))
                errors["branch"] = message;
            else if (message.Contains("Monthly salary", StringComparison.OrdinalIgnoreCase))
                errors["monthlySalary"] = message;
            else if (message.Contains("Career Manager", StringComparison.OrdinalIgnoreCase))
                errors["reportsTo"] = message;
            else if (message.Contains("surname", StringComparison.OrdinalIgnoreCase))
                errors["lastName"] = message;
            else if (message.Contains("name", StringComparison.OrdinalIgnoreCase))
                errors["firstName"] = message;
            else if (message.Contains("title", StringComparison.OrdinalIgnoreCase))
                errors["title"] = message;
            else
                errors["general"] = message;

            return errors;
        }
    }
}