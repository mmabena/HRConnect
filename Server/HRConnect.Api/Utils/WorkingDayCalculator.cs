
namespace HRConnect.Api.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class WorkingDayCalculator
    {
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