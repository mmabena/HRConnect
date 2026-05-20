namespace HRConnect.Api.Utils
{
  using System.Collections.Generic;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.DTOs.Employee;
  using System.Threading.Tasks;
  using System.Net.Http.Json;
  using System.Text.Json;

  public class UserEmployeeHttpClient : IUserEmployeeHttpClient
  {

    private readonly HttpClient _httpClient;
    public UserEmployeeHttpClient(HttpClient httpClient)
    {
      _httpClient = httpClient;
    }
    public async Task<string> ResolveEmployeeIdFromUserIdAsync(int userId)
    {
      try
      {
        // Fetch user by userId
        var userResponse = await _httpClient.GetAsync($"user/{userId}");
        Console.WriteLine($"==============================Status Code for user/{userId}: {userResponse.StatusCode}");

        if (userResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
          throw new KeyNotFoundException($"User with ID {userId} not found.");
        }

        User? user = await userResponse.Content.ReadFromJsonAsync<User>();
        if (user == null)
        {
          throw new KeyNotFoundException($"User with ID {userId} not found.");
        }

        Console.WriteLine($">>>>>>>>>>>>>>USER STUFF {user?.Email}");

        // Fetch employee by email
        var employeeResponse = await _httpClient.GetAsync($"employee/email/{user.Email}");
        Console.WriteLine($"Status Code for employee/email/{user.Email}: {employeeResponse.StatusCode}");

        if (employeeResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
          throw new KeyNotFoundException($"Employee not found for User ID {userId}.");
        }

        EmployeeDto? employee = await employeeResponse.Content.ReadFromJsonAsync<EmployeeDto>();
        if (employee == null)
        {
          throw new KeyNotFoundException($"Employee not found for User ID {userId}.");
        }

        return employee.EmployeeId ?? string.Empty;
      }
      catch (JsonException ex)
      {
        throw new JsonException($"Error parsing response: {ex.Message}");
      }
    }
  }
}