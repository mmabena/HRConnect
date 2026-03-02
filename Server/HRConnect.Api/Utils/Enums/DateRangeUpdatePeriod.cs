namespace HRConnect.Api.Utils.Enums
{
    /// <summary>
    /// Defines the specific time periods during which medical option category updates are allowed.
    /// This enum is used to enforce business rules that restrict when medical benefit options
    /// can be modified, typically aligning with annual enrollment periods or policy update cycles.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Business Context:</strong>
    /// Medical benefit options often have restricted update windows to ensure:
    /// - Stable pricing throughout the year
    /// - Proper actuarial calculations
    /// - Compliance with regulatory requirements
    /// - Predictable budgeting for employers and employees
    /// </para>
    /// 
    /// <para>
    /// <strong>Usage in Validation:</strong>
    /// This enum is typically used in validation logic to ensure that bulk updates
    /// to medical options are only performed during authorized periods:
    /// </para>
    /// <code>
    /// public void ValidateUpdatePeriod(DateRangeUpdatePeriod period)
    /// {
    ///     var now = DateTime.UtcNow;
    ///     var currentYear = now.Year;
    ///     
    ///     if (period == DateRangeUpdatePeriod.CategoryOptionsUpdatePeriod)
    ///     {
    ///         var updateStart = new DateTime(currentYear, 11, 1, 0, 0, 1); // November 1st 00:00:01
    ///         var updateEnd = new DateTime(currentYear, 12, 31, 23, 59, 59); // December 31st 23:59:59
    ///         
    ///         if (now < updateStart || now > updateEnd)
    ///         {
    ///             throw new ValidationException(
    ///                 "Category options can only be updated during the annual update period (November 1 - December 31)",
    ///                 new Dictionary&lt;string, string[]&gt;
    ///                 {
    ///                     ["UpdatePeriod"] = new[] { "Updates are only allowed from November 1st to December 31st" }
    ///                 });
    ///         }
    ///     }
    /// }
    /// </code>
    /// 
    /// <para>
    /// <strong>Integration with Services:</strong>
    /// Services that perform bulk updates should validate the current date against
    /// the allowed update periods before executing modifications:
    /// </para>
    /// <code>
    /// public async Task&lt;IReadOnlyList&lt;MedicalOptionDto&gt;&gt; BulkUpdateMedicalOptionsByCategoryAsync(
    ///     int categoryId, 
    ///     IReadOnlyCollection&lt;UpdateMedicalOptionVariantsDto&gt; bulkUpdateDto,
    ///     DateRangeUpdatePeriod updatePeriod)
    /// {
    ///     // Validate that we're in an allowed update period
    ///     _dateRangeValidator.ValidateUpdatePeriod(updatePeriod);
    ///     
    ///     // Proceed with the update if validation passes
    ///     return await _repository.BulkUpdateByCategoryIdAsync(categoryId, bulkUpdateDto);
    /// }
    /// </code>
    /// 
    /// <para>
    /// <strong>Configuration Considerations:</strong>
    /// The date ranges can be made configurable through appsettings.json for different
    /// environments or business requirements:
    /// </para>
    /// <code>
    /// // appsettings.json
    /// {
    ///   "MedicalOptions": {
    ///     "UpdatePeriods": {
    ///       "CategoryOptionsUpdatePeriod": {
    ///         "StartMonth": 11,
    ///         "StartDay": 1,
    ///         "StartHour": 0,
    ///         "StartMinute": 0,
    ///         "StartSecond": 1,
    ///         "EndMonth": 12,
    ///         "EndDay": 31,
    ///         "EndHour": 23,
    ///         "EndMinute": 59,
    ///         "EndSecond": 59
    ///       }
    ///     }
    ///   }
    /// }
    /// </code>
    /// 
    /// <para>
    /// <strong>Time Zone Considerations:</strong>
    /// All date comparisons should use UTC to ensure consistency across different
    /// deployment environments and user locations. The specified times (00:00:01 and 23:59:59)
    /// are in UTC format.
    /// </para>
    /// 
    /// <para>
    /// <strong>Future Extensibility:</strong>
    /// Additional update periods can be added as the business requirements evolve:
    /// </para>
    /// <code>
    /// public enum DateRangeUpdatePeriod
    /// {
    ///     CategoryOptionsUpdatePeriod, // November 1st 00:00:01 to December 31st 23:59:59
    ///     EmergencyUpdatePeriod,      // For critical updates (any time with approval)
    ///     QuarterlyReviewPeriod,       // Q1: Jan 1-15, Q2: Apr 1-15, Q3: Jul 1-15, Q4: Oct 1-15
    ///     MidYearAdjustmentPeriod     // June 15-30 for mid-year policy changes
    /// }
    /// </code>
    /// </remarks>
    public enum DateRangeUpdatePeriod
    {
        /// <summary>
        /// Represents the annual category options update period.
        /// Allows updates to medical option categories from November 1st at 00:00:01 UTC
        /// through December 31st at 23:59:59 UTC.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <strong>Time Range Details:</strong>
        /// - Start: November 1st, 00:00:01 UTC
        /// - End: December 31st, 23:59:59 UTC
        /// - Duration: Approximately 61 days
        /// </para>
        /// 
        /// <para>
        /// <strong>Business Purpose:</strong>
        /// This period typically aligns with:
        /// - Annual benefits enrollment windows
        /// - Insurance policy renewal cycles
        /// - Year-end budget planning
        /// - Actuarial calculation periods
        /// </para>
        /// 
        /// <para>
        /// <strong>Validation Example:</strong>
        /// </para>
        /// <code>
        /// public bool IsCategoryUpdatePeriodActive()
        /// {
        ///     var now = DateTime.UtcNow;
        ///     var currentYear = now.Year;
        ///     
        ///     var updateStart = new DateTime(currentYear, 11, 1, 0, 0, 1);
        ///     var updateEnd = new DateTime(currentYear, 12, 31, 23, 59, 59);
        ///     
        ///     return now >= updateStart && now <= updateEnd;
        /// }
        /// </code>
        /// </remarks>
        CategoryOptionsUpdatePeriod // November 1st 00:00:01 to December 31st 23:59:59
    } 
}