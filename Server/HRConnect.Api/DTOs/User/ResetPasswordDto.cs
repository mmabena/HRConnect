namespace HRConnect.Api.DTOs.User
{
  public class ResetPasswordDto
  {
    public string Email { get; set; } = string.Empty;
    public string Pin { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;

  }
}