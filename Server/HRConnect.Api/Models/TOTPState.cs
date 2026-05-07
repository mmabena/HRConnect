namespace HRConnect.Api.Models
{
  /// <summary>
  /// This is an isolated table that is used for handling replay attempts (attacks)
  /// This prevents OTP being used outside of the time window and to make sure the 
  /// pin has not been used again after the fact
  /// </summary>
  public class TOTPState
  {
    public int UserId { get; set; }
    public long LastUsedTimeStamp { get; set; }
  }
}