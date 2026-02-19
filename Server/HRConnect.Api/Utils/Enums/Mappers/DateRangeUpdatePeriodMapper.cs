namespace HRConnect.Api.Utils.Enums.Mappers
{
  using HRConnect.Api.Utils.Enums;
   public static class DateRangeUpdatePeriodMapper
   {
     // this builds the start and end DateTime for each period
     private static (DateTime Start, DateTime End) GetRange(this DateRangeUpdatePeriod period,
       int? year = null)
     {
       // Get year if null use current year
       int y = year ?? DateTime.Now.Year;
       return period switch
       {
         DateRangeUpdatePeriod.CategoryOptionsUpdatePeriod => (
           new DateTime(y, 11, 1, 0, 0, 01), // Nov 01 00:00:01.001
           new DateTime(y, 12, 31, 23, 59, 59) // Dec 31 23:59:59.999
         ),
         _ => throw new ArgumentOutOfRangeException(nameof(period),
           "Update operations are restricted outside of the Update Period")
       };
     }
     
     // Returns true if the given date falls within the period's range
     public static bool Contains(this DateRangeUpdatePeriod period, DateTime date)
     {
       var (start, end) = period.GetRange(date.Year);
       return date >= start && date <= end;
     }

   } 
}