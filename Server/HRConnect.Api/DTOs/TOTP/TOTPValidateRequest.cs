namespace HRConnect.Api.DTOs.TOTP
{
  public class TOTPValidateRequestDto
  {
    public int UserId { get; set; }
    public string Code { get; set; } = string.Empty;
  }
}