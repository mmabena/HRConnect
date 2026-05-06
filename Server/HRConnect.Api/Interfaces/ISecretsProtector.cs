namespace HRConnect.Api.Interfaces
{
  public interface ISecretsProtector
  {
    byte[] Wrap(byte[] rawData);
    byte[] UnWrap(byte[] encryptedData);

  }
}