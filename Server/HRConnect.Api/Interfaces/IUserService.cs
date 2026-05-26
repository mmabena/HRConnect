namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.DTOs.User;
  using HRConnect.Api.Models;
  public interface IUserService
  {
    Task<List<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User> CreateUserAsync(CreateUserRequestDto dto);
    Task<User?> UpdateUserAsync(int id, UpdateUserRequestDto dto);
    Task<User?> UpdateUserRoleAsync(int id, UpdateUserRoleRequestDto dto);
    Task<User?> UpdateEmployeeUserRoleAsync(string employeeId, UpdateUserRoleRequestDto dto);
    Task<bool> DeleteUserAsync(int id);
    Task<List<UserRoleOptionDto>> GetRoleOptionsAsync();
    Task SyncEmployeeUserAsync();
    Task<bool> ChangePasswordAsync(ChangePasswordRequestDto dto);
    Task<List<string>> OrganiseSuperUsersAsync(UserRole role = UserRole.SuperUser);
  }
}