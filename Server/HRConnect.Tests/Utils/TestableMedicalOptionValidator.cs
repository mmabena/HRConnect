namespace HRConnect.Tests.Utils
{
    using HRConnect.Api.Utils.MedicalOption;
    using HRConnect.Api.Models;
    using HRConnect.Api.DTOs.MedicalOption;
    using HRConnect.Api.Interfaces;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System;
    using System.Linq;
    using Api.Utils.Enums;
    using Api.Utils.Enums.Mappers;

    public class TestableMedicalOptionValidator
    {
        private readonly Func<DateTime> _dateTimeProvider;

        public TestableMedicalOptionValidator(Func<DateTime> dateTimeProvider = null)
        {
            _dateTimeProvider = dateTimeProvider ?? (() => DateTime.UtcNow);
        }

        /// <summary>
        /// Testable wrapper for ValidateAllCategoryVariantsComprehensiveAsync
        /// that allows mocking current date/time.
        /// </summary>
        public async Task<BulkValidationResult> ValidateAllCategoryVariantsComprehensiveAsync(
            int categoryId,
            IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto,
            IMedicalOptionRepository repository,
            IEnumerable<MedicalOption> dbData)
        {
            // Use provided date provider instead of DateTime.UtcNow
            var currentDate = _dateTimeProvider();

            // Replicate update period validation logic with our testable date
            if (!IsInUpdatePeriod(currentDate))
            {
                return new BulkValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Update operations are restricted outside of the Update Period"
                };
            }

            // Call ID validation logic
            var updateIds = bulkUpdateDto.Select(x => x.MedicalOptionId).ToHashSet();
            var dbIds = dbData.Select(x => x.MedicalOptionId).ToHashSet();
            var nonExistentIds = updateIds.Except(dbIds).ToList();

            if (nonExistentIds.Count != 0)
            {
                return new BulkValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"Medical option IDs {string.Join(", ", nonExistentIds)} do not exist in category {categoryId}"
                };
            }

            return new BulkValidationResult { IsValid = true };
        }

        private static bool IsInUpdatePeriod(DateTime date)
        {
            return DateRangeUpdatePeriod.CategoryOptionsUpdatePeriod.Contains(date);
        }
    }
}