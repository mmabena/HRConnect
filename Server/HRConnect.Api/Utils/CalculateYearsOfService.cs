namespace HRConnect.Api.Utils
{
    public static class CalculateYearsOfService
    {
        /// <summary>
        /// Calculates completed years of service using anniversary logic.
        /// </summary>
        /// <param name="startDate">Employee start date</param>
        /// <returns>Completed years of service</returns>
        public static int UsingStartDate(DateOnly startDate)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (startDate > today)
                return 0;

            var yearsOfService =
                today.Year - startDate.Year;

            if (today < startDate.AddYears(yearsOfService))
            {
                yearsOfService--;
            }

            return yearsOfService;
        }
    }
}