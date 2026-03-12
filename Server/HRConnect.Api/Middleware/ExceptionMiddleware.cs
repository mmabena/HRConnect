using System.Net;
using System.Text.Json;
using HRConnect.Api.Services;
using Microsoft.AspNetCore.Http;

namespace HRConnect.Api.Middleware
{
  ///<summary>
  ///Middleware responsible for globally catching and handling exceptions that occur during HTTP request processing.
  ///It converts application exceptions into standardized JSON error responses with appropriate HTTP status codes. Basically it throws valicadtion exceptions from the service layer and catches them here to return a 400 bad request  and sends then to the frontend in a consistent format. 
  /// It also handles not found exceptions and general exceptions to ensure the API responds gracefully to errors.
  ///</summary>
  public class ExceptionMiddleware
  {
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
      _next = next;
    }
    ///<summary>
    ///Intercepts incoming HTTP requests and executes the next middleware in the pipeline.
    ///If an exception occurs, it catches the exception and returns a structured JSON error response.
    ///</summary>
    ///<param name="context">The current HTTP request context.</param>
    ///<returns>
    ///A Task representing the asynchronous middleware execution.
    ///</returns>
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
  ///<summary>
    ///Maps an exception message to the corresponding API field name so that the frontend
    ///can associate the error message with the correct form input field.
    ///</summary>
    ///<param name="message">The exception message generated during validation or business rule processing.</param>
    ///<returns>
    ///A dictionary where the key represents the form field name and the value represents the corresponding error message.
    ///</returns>
    private static Dictionary<string, string> MapMessageToField(string message)
    {
      var errors = new Dictionary<string, string>();

      if (message.Contains("Email", StringComparison.OrdinalIgnoreCase))
        errors["email"] = message;
      else if (message.Contains("Contact Number", StringComparison.OrdinalIgnoreCase))
        errors["contactNumber"] = message;
      else if (message.Contains("Tax Number", StringComparison.OrdinalIgnoreCase))
        errors["taxNumber"] = message;
      else if (message.Contains("ID Number", StringComparison.OrdinalIgnoreCase))
        errors["idNumber"] = message;
      else if (message.Contains("Passport Number", StringComparison.OrdinalIgnoreCase))
        errors["passportNumber"] = message;
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
        else if (message.Contains("zipCode", StringComparison.OrdinalIgnoreCase))
        errors["zipCode"] = message;
      else if (message.Contains("title", StringComparison.OrdinalIgnoreCase))
        errors["title"] = message;
        else if (message.Contains("city", StringComparison.OrdinalIgnoreCase))
        errors["city"] = message;
        else if (message.Contains("disabilityType", StringComparison.OrdinalIgnoreCase))
        errors["disabilityType"] = message;
      else
        errors["general"] = message;

      return errors;
    }
  }
}