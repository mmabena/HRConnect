namespace HRConnect.Api.Interfaces
{
    using HRConnect.Api.Models;
    public interface IAuthService
    {
        Task<string?> LoginAsync(string email, string password);
        Task<User?> GetUserByEmailAsync(string email);
        Task<(string pin, DateTime expiresAt)> ForgotPasswordAsync(string email);
        Task VerifyPinAsync(string email, string pin);
        Task ResetPasswordAsync(HRConnect.Api.DTOs.User.ResetPasswordDto dto);
    }
}
