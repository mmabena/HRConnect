namespace HRConnect.Api.Utils
{
  using System.Collections.Generic;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.DTOs.User;
  using System.Threading.Tasks;
  using System.Net.Http.Json;
  using System.Text.Json;

  public class UserHttpClient : IUserHttpClient
  {

    private readonly HttpClient _httpClient;
    public UserHttpClient(HttpClient httpClient)
    {
      _httpClient = httpClient;
    }

    public async Task<UserRegisterDto> ResolveUserFromId(int userId)
    {
      try
      {
        var userResponse = await _httpClient.GetAsync($"user/{userId}");
        if (userResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
          throw new KeyNotFoundException($"User with ID {userId} not found.");
        }

        UserRegisterDto? user = await userResponse.Content.ReadFromJsonAsync<UserRegisterDto>();

        if (user == null)
        {
          throw new KeyNotFoundException($"User with ID {userId} not found.");
        }

        return user;
      }
      catch (JsonException ex)
      {
        throw new JsonException($"Error parsing response: {ex.Message}");
      }
    }
  }
}