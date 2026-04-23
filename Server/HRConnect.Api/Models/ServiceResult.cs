namespace HRConnect.Api.Models
{
  public class ServiceResult
  {
    public bool IsSuccess { get; private set; }
    public string Message { get; private set; }

    private ServiceResult(bool success, string message)
    {
      IsSuccess = success;
      Message = message;
    }

    public static ServiceResult Success(string message)
    {
      return new ServiceResult(true, message);
    }

    public static ServiceResult Failure(string message)
    {
      return new ServiceResult(false, message);
    }
  }
}
