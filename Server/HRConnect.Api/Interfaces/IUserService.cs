namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models;
  using HRConnect.Api.DTOs.User;
  public interface IUserService
  {
    Task<List<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User> CreateUserAsync(CreateUserRequestDto dto);
    Task<User?> UpdateUserAsync(int id, UpdateUserRequestDto dto);
    Task<bool> DeleteUserAsync(int id);
  }
}
