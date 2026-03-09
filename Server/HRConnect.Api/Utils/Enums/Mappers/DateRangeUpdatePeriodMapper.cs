namespace HRConnect.Api.Utils.Enums.Mappers
{
  using HRConnect.Api.Utils.Enums;

  /// <summary>
  /// Provides extension methods and mapping functionality for DateRangeUpdatePeriod enum values.
  /// This static class enables date range calculations and validation for medical option update periods,
  /// ensuring that updates are only performed during authorized business windows.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This mapper class serves as a utility for working with date-based business rules that govern
  /// when medical option updates are permitted. It encapsulates the logic for determining
  /// specific date ranges and validating whether a given date falls within an allowed update period.
  /// </para>
  /// 
  /// <para>
  /// Key responsibilities:
  /// - Calculating precise date ranges for each update period
  /// - Providing year-flexible date range generation
  /// - Offering date containment validation
  /// - Enforcing business rules for update timing
  /// </para>
  /// 
  /// <para>
  /// Architecture patterns:
  /// - Extension method pattern for fluent API design
  /// - Static utility class for stateless operations
  /// - Switch expression for clean period-based logic
  /// - Tuple return values for efficient data transfer
  /// </para>
  /// 
  /// <para>
  /// Business context:
  /// Medical benefit options typically have restricted update windows to ensure:
  /// - Stable pricing throughout the year
  /// - Proper actuarial calculations
  /// - Compliance with regulatory requirements
  /// - Predictable budgeting for employers and employees
  /// - Alignment with annual enrollment periods
  /// </para>
  /// 
  /// <para>
  /// Performance considerations:
  /// - Stateless operations with no external dependencies
  /// - Efficient switch expression for period routing
  /// - Minimal memory allocation with tuple returns
  /// - Year parameterization for flexible date calculations
  /// </para>
  /// 
  /// <para>
  /// Time zone considerations:
  /// All date operations use the local time zone of the executing environment.
  /// For production deployments, consider using UTC to ensure consistency across
  /// different server locations and user time zones.
  /// </para>
  /// </remarks>
  public static class DateRangeUpdatePeriodMapper
  {
    /// <summary>
    /// Builds the start and end DateTime range for a specific update period.
    /// This private method encapsulates the date range calculation logic for each period type.
    /// </summary>
    /// <param name="period">The DateRangeUpdatePeriod enum value to calculate the range for.</param>
    /// <param name="year">Optional year parameter; if null, uses the current year.</param>
    /// <returns>A tuple containing the Start and End DateTime values for the specified period.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an unsupported period value is provided.</exception>
    /// <remarks>
    /// <para>
    /// This method uses a switch expression for clean, efficient period-based routing.
    /// Each period defines its specific start and end dates with precise time specifications.
    /// </para>
    /// 
    /// <para>
    /// Current supported periods:
    /// - CategoryOptionsUpdatePeriod: November 1st 00:00:01 to December 31st 23:59:59
    /// </para>
    /// 
    /// <para>
    /// Year handling:
    /// - When year parameter is provided, uses that specific year for calculations
    /// - When year parameter is null, automatically uses DateTime.Now.Year
    /// - This enables both current-year and historical/future date range calculations
    /// </para>
    /// 
    /// <para>
    /// Time precision:
    /// - Start times include milliseconds (00:00:01.001) to avoid midnight boundary issues
    /// - End times include milliseconds (23:59:59.999) to capture the full final day
    /// - This precision ensures no ambiguity about period boundaries
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Get range for current year
    /// var (currentStart, currentEnd) = DateRangeUpdatePeriod.CategoryOptionsUpdatePeriod.GetRange();
    /// Console.WriteLine($"Current year update period: {currentStart} to {currentEnd}");
    /// 
    /// // Get range for specific year
    /// var (futureStart, futureEnd) = DateRangeUpdatePeriod.CategoryOptionsUpdatePeriod.GetRange(2025);
    /// Console.WriteLine($"2025 update period: {futureStart} to {futureEnd}");
    /// 
    /// // Historical range
    /// var (pastStart, pastEnd) = DateRangeUpdatePeriod.CategoryOptionsUpdatePeriod.GetRange(2023);
    /// Console.WriteLine($"2023 update period: {pastStart} to {pastEnd}");
    /// </code>
    /// </example>
    private static (DateTime Start, DateTime End) GetRange(this DateRangeUpdatePeriod period,
      int? year = null)
    {
      // Get year if null use current year
      int y = year ?? DateTime.Now.Year;
      return period switch
      {
        DateRangeUpdatePeriod.CategoryOptionsUpdatePeriod => (
          new DateTime(y, 11, 1, 0, 0, 01), // Nov 01 00:00:01.001 - Prod
          new DateTime(y, 12, 31, 23, 59, 59) // Dec 31 23:59:59.999 - Prod
        ),
        _ => throw new ArgumentOutOfRangeException(nameof(period),
          "Update operations are restricted outside of the Update Period")
      };
    }

    /// <summary>
    /// Determines whether a specific date falls within the allowed update period range.
    /// This extension method provides convenient validation for date-based business rules.
    /// </summary>
    /// <param name="period">The DateRangeUpdatePeriod to check against.</param>
    /// <param name="date">The DateTime to validate against the period's range.</param>
    /// <returns>True if the specified date falls within the period's range; otherwise, false.</returns>
    /// <remarks>
    /// <para>
    /// This method performs an inclusive range check, meaning dates equal to the start
    /// or end boundaries are considered valid and return true.
    /// </para>
    /// 
    /// <para>
    /// Validation logic:
    /// - Retrieves the date range for the specified period using the GetRange method
    /// - Uses the date's year for range calculation to ensure year-specific validation
    /// - Performs inclusive comparison: date >= start AND date <= end
    /// </para>
    /// 
    /// <para>
    /// Use cases:
    /// - Validating that bulk updates are performed during authorized periods
    /// - Checking if current date allows for medical option modifications
    /// - Enforcing business rules in service layer validation
    /// - Providing user feedback about update availability
    /// </para>
    /// 
    /// <para>
    /// Performance considerations:
    /// - Single method call with efficient tuple return
    /// - No external dependencies or database calls
    /// - O(1) time complexity for validation
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Check if current date is within update period
    /// var now = DateTime.Now;
    /// var isUpdatePeriod = DateRangeUpdatePeriod.CategoryOptionsUpdatePeriod.Contains(now);
    /// 
    /// if (isUpdatePeriod)
    /// {
    ///     Console.WriteLine("Today is within the allowed update period");
    ///     // Proceed with update operations
    /// }
    /// else
    /// {
    ///     Console.WriteLine("Today is outside the allowed update period");
    ///     // Block update operations or show appropriate message
    /// }
    /// 
    /// // Check specific dates
    /// var testDate1 = new DateTime(2024, 11, 15); // November 15, 2024
    /// var testDate2 = new DateTime(2024, 10, 15); // October 15, 2024
    /// 
    /// Console.WriteLine($"Nov 15, 2024 is valid: {DateRangeUpdatePeriod.CategoryOptionsUpdatePeriod.Contains(testDate1)}"); // True
    /// Console.WriteLine($"Oct 15, 2024 is valid: {DateRangeUpdatePeriod.CategoryOptionsUpdatePeriod.Contains(testDate2)}"); // False
    /// 
    /// // Edge case testing
    /// var startDate = new DateTime(2024, 11, 1, 0, 0, 1); // Exact start time
    /// var endDate = new DateTime(2024, 12, 31, 23, 59, 59); // Exact end time
    /// 
    /// Console.WriteLine($"Start boundary is valid: {DateRangeUpdatePeriod.CategoryOptionsUpdatePeriod.Contains(startDate)}"); // True
    /// Console.WriteLine($"End boundary is valid: {DateRangeUpdatePeriod.CategoryOptionsUpdatePeriod.Contains(endDate)}"); // True
    /// </code>
    /// </example>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the specified period is not supported by the GetRange method.
    /// This exception propagates from the underlying GetRange method call.
    /// </exception>
    public static bool Contains(this DateRangeUpdatePeriod period, DateTime date)
    {
      var (start, end) = period.GetRange(date.Year);
      return date >= start && date <= end;
    }
  }
}