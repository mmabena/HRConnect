namespace HRConnect.Api.Utils
{
  using Microsoft.AspNetCore.DataProtection;
  using System.Text;
  using Microsoft.CodeAnalysis.CSharp.Syntax;

  /// <summary>
  /// Protects user secrets for Time-Based One Time Pin by wrapping and unwrapping 
  /// the sercret when storing in the database. Hashing would not work as hashing
  /// alogrithms are unidirectional (you can't 'unhash' a hashed password) 
  /// </summary>
  public static class SecretsProtector
  {
    private static IDataProtector _protector = null!;
    public static void Init(IDataProtector protector)
    {
      _protector = protector;
    }

    //Wrap a string in protection  
    public static string Wrap<T>(T? data)
    {
      if (_protector == null)
        throw new InvalidOperationException($"No Data Encryptor Initialised");

      //wrap based on type
#pragma warning disable CS8603
      return data switch
      {
        string s => _protector.Protect(s),
        byte[] b => _protector.Protect(Convert.ToBase64String(b)),
        _ => throw new InvalidDataException($"Type {typeof(T)} Not Supported") //this is just to silence the compiler warning
        //possibly returning a null
      };
#pragma warning restore CS8603
    }
    public static T UnWrap<T>(string data)
    {
      if (_protector == null)
        throw new InvalidOperationException($"No Data Encryptor Initialised");

      string? unprotectedData = _protector.Unprotect(data);
      return typeof(T) switch
      {
        Type t when t == typeof(string) => (T)(object)unprotectedData,
        Type t when t == typeof(byte[]) => (T)(object)Convert.FromBase64String(unprotectedData),
        _ => throw new InvalidDataException($"Type {typeof(T)} Not Supported")
      };
    }

    public static byte[] WrapBytes(byte[] data)
    {
      if (_protector == null)
        throw new InvalidOperationException("No Data Encryptor Initialised");

      string base64 = Convert.ToBase64String(data);
      string protectedString = _protector.Protect(base64);
      return Encoding.UTF8.GetBytes(protectedString);
    }

    public static byte[] UnWrapBytes(byte[] encryptedBytes)
    {
      if (_protector == null)
        throw new InvalidOperationException("No Data Encryptor Initialised");

      string cipherString = Encoding.UTF8.GetString(encryptedBytes);
      string unprotectedBase64 = _protector.Unprotect(cipherString);
      return Convert.FromBase64String(unprotectedBase64);
    }
  }
}