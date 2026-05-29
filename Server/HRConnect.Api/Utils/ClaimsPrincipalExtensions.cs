namespace HRConnect.Api.Utils
{
  using System;
  using System.Globalization;
  using System.Security.Claims;
  public static class ClaimsPrincipalExtensions
  {
    public static int GetUserId(this ClaimsPrincipal user)
    {
      var userIdClaim = user.FindFirst("UserId")?.Value;

      if (string.IsNullOrEmpty(userIdClaim))
        throw new UnauthorizedAccessException("UserId claim is missing");

      return int.Parse(userIdClaim, CultureInfo.InvariantCulture);
    }
  }
}