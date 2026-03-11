namespace HRConnect.Api.DTOs
{
  public class RequestEligibileOptionsDto
  {
    //What I need from the client sideto determine the eligible options for a user
    public int NumberOfPrincipals { get; set; } = 1; // Default to 1 principal, as most employees will have at least themselves as a principal
    public int NumberOfAdults { get; set; }
    public int NumberOfChildren { get; set; }
  }
}
