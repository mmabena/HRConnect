namespace HRConnect.Api.Models
{
  /// <summary>
  /// Multi-Factor-Authentication User Secrets tables used to create user secret
  /// secrets from purely random keys. This secret is used with the RFC-6238 algorithm
  /// to create the Time-Based One Time Pin. The OTP is verified against EncryptedSecret  
  /// </summary>
  public class MFAUserSecret
  {
    public int SecretId { get; set; }
    public int UserId { get; set; }
    public byte[] EncryptedUserSecret { get; set; } = null!;
    public int KeyVersion { get; set; }//versioning allows for stronger key rotation
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public User User { get; set; } = null!;
  }
}