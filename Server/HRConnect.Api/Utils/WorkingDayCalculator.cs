namespace HRConnect.Api.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class WorkingDayCalculator
    {
        /// <summary>
        /// Calculates the number of working days (Monday to Friday) between two given dates, inclusive of the start and end dates,
        /// by iterating through the range of dates and counting the days that are not Saturdays or Sundays, 
        /// while returning the total count of working days between the specified start and end dates,
        /// and returning 0 if the end date is earlier than the start date.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static int CountWorkingDays(DateOnly start, DateOnly end)
        {
            if (end < start)
                return 0;

            int totalDays = end.DayNumber - start.DayNumber + 1;
            int workingDays = 0;

            for (int i = 0; i < totalDays; i++)
            {
                var current = start.AddDays(i);
                if (current.DayOfWeek != DayOfWeek.Saturday &&
                    current.DayOfWeek != DayOfWeek.Sunday)
                {
                    workingDays++;
                }
            }

            return workingDays;
        }
    }
}