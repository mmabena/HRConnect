namespace HRConnect.Api.Utils
{
  using HRConnect.Api.Interfaces;
  using Microsoft.AspNetCore.DataProtection;

  /// <summary>
  /// Protects user secrets for Time-Based One Time Pin by wrapping and unwrapping 
  /// the sercret when storing in the database. Hashing would not work as hashing
  /// alogrithms are unidirectional (you can't 'unhash' a hashed password) 
  /// </summary>
  public class SecretsProtector : ISecretsProtector
  {
    private readonly IDataProtector _protector;
    public SecretsProtector(IDataProtectionProvider protector)
    {
      _protector = protector.CreateProtector("TotpSecret.v1");
    }
    public byte[] Wrap(byte[] rawData)
    {
      return _protector.Protect(rawData);
    }
    public byte[] UnWrap(byte[] encryptedData)
    {
      return _protector.Unprotect(encryptedData);
    }
  }
}