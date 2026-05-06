namespace HRConnect.Api.Utils
{

  using HRConnect.Api.Interfaces;
  using Microsoft.AspNetCore.DataProtection;

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