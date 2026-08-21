namespace HRConnect.Api.Services
{
    using System;
    using System.Collections.Generic;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Interfaces.Notification;
    using HRConnect.Api.Models;
    using System.Globalization;
    using HRConnect.Api.Models.Payroll;
    using HRConnect.Api.DTOs.MedicalOption;
    using HRConnect.Api.DTOs.Notification;
    using HRConnect.Api.Utils;
    using HRConnect.Api.Models.PayrollDeduction;
    using System.Linq;
    using System.Threading.Tasks;
    public class MedicalAidDependentNotificationService : IMedicalAidDependentNotificationService
    {
        private readonly IMedicalAidDependentRepository _medicalDependentRepo;
        private readonly IMedicalAidDeductionRepository _medicalDeductionRepo;
        private readonly IMedicalOptionRepository _medicalOption;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly INotificationFactory _notificationFactory;
        // private readonly IPayrollRunService _payrollRunService;

        public MedicalAidDependentNotificationService(IMedicalAidDependentRepository medicalDependentRepo, IEmailTemplateService emailTemplateService, INotificationFactory notificationFactory, IMedicalAidDeductionRepository medicalDeductionRepo, IMedicalOptionRepository medicalOption)
        {
            _medicalDependentRepo = medicalDependentRepo;
            _emailTemplateService = emailTemplateService;
            _notificationFactory = notificationFactory;
            _medicalDeductionRepo = medicalDeductionRepo;
            _medicalOption = medicalOption;
        }
        /// <summary>
        /// Checks active child dependents and notifies employees when a child dependent
        /// will turn 21 during the next payroll period.
        /// Calculates the expected Medical Aid premium change from a child premium to an adult premium.
        /// </summary>
        /// <param name="currentRun">The current payroll run used to determine the next payroll period.</param>
        /// <returns>A task representing the asynchronous notification operation.</returns>
        public async Task NotifyChildrenTurning21Async(PayrollRun currentRun)
        {
            Console.WriteLine("==============================================");
            Console.WriteLine("Checking for child dependents turning 21...");
            Console.WriteLine("==============================================");

            // Get the currently active payroll run
            // PayrollRun currentRun = await _payrollRunService.GetCurrentRunAsync();

            Console.WriteLine("========== DEBUG ==========");
            Console.WriteLine($"Current Payroll Run from GetCurrentRunAsync(): {currentRun.PayrollRunNumber}");
            Console.WriteLine($"Current Run Locked: {currentRun.IsLocked}");
            Console.WriteLine($"Current Run Finalised: {currentRun.IsFinalised}");
            Console.WriteLine("===========================");

            // Current payroll run number (1 - 12)
            int currentRunNumber = currentRun.PayrollRunNumber;

            // Determine the next payroll run
            int nextRunNumber = currentRunNumber == 12
                ? 1
                : currentRunNumber + 1;

            // Convert payroll run to calendar month
            int nextPayrollMonth = PayrollRunToCalendarMonth(nextRunNumber);

            // Determine the calendar year for the next payroll
            int nextPayrollYear;

            if (nextPayrollMonth >= 4)
            {
                // April -> December belong to the financial year's start year
                nextPayrollYear = currentRun.Period.StartDate.Year;
            }
            else
            {
                // January -> March belong to the following calendar year
                nextPayrollYear = currentRun.Period.StartDate.Year + 1;
            }

            Console.WriteLine($"Current Payroll Run : {currentRunNumber}");
            Console.WriteLine($"Next Payroll Run    : {nextRunNumber}");
            Console.WriteLine($"Next Payroll Month  : {nextPayrollMonth}");
            Console.WriteLine($"Next Payroll Year   : {nextPayrollYear}");

            // Get every dependent in the system
            List<MedicalAidDependent> dependents =
                await _medicalDependentRepo.GetAllMedicalAidDependentsAsync();

            // Only children qualify
            List<MedicalAidDependent> childDependents =
                dependents
                    .Where(d =>
                        d.IsActive &&
                        d.Relationship == Relationship.Child)
                    .ToList();

            Console.WriteLine($"Found {childDependents.Count} active child dependents.");

            foreach (MedicalAidDependent dependent in childDependents)
            {
                if (!dependent.DateOfBirth.HasValue)
                {
                    Console.WriteLine(
                        $"Dependent {dependent.DependentId} has no Date of Birth.");
                    continue;
                }
                // Calculate the child's 21st birthday
                DateTime twentyFirstBirthday =
                    dependent.DateOfBirth.Value.AddYears(21);

                Console.WriteLine("--------------------------------");
                Console.WriteLine($"Employee : {dependent.EmployeeId}");
                Console.WriteLine($"Dependent: {dependent.FirstName} {dependent.LastName}");
                Console.WriteLine($"21st Birthday : {twentyFirstBirthday:d}");

                // Does the 21st birthday occur during the NEXT payroll month?
                if (twentyFirstBirthday.Month == nextPayrollMonth &&
                    twentyFirstBirthday.Year == nextPayrollYear)
                {
                    Console.WriteLine("Notification Required.");

                    MedicalAidDeduction deduction =
                        await _medicalDeductionRepo.GetMedicalAidDeductionByEmployeeAndPayrollRunAsync(dependent.EmployeeId, currentRun.PayrollRunId);

                    if (deduction == null)
                    {
                        Console.WriteLine($"No active deduction found for {dependent.EmployeeId}");
                        continue;
                    }

                    MedicalOptionDto medicalOption =
                        await _medicalOption.GetMedicalOptionByIdAsync(deduction.MedicalOptionId);

                    decimal currentTotal = deduction.TotalDeductionAmount;

                    decimal childPremium = medicalOption.TotalMonthlyContributionsChild;

                    decimal adultPremium = medicalOption.TotalMonthlyContributionsAdult;

                    decimal newTotal =
                        currentTotal
                        - childPremium
                        + adultPremium;

                    decimal premiumIncrease = adultPremium - childPremium;

                    string htmlMessage =
                    await _emailTemplateService.GetMedicalAidDependentTurning21TemplateAsync(
                        $"{dependent.FirstName} {dependent.LastName}",
                        deduction.OptionName,
                            childPremium.ToString("C", CultureInfo.GetCultureInfo("en-ZA")),
                            adultPremium.ToString("C", CultureInfo.GetCultureInfo("en-ZA")),
                            premiumIncrease.ToString("C", CultureInfo.GetCultureInfo("en-ZA")),
                            currentTotal.ToString("C", CultureInfo.GetCultureInfo("en-ZA")),
                            newTotal.ToString("C", CultureInfo.GetCultureInfo("en-ZA"))
                        );


                    CreateNotificationDto notification = new CreateNotificationDto
                    {
                        EmployeeId = dependent.EmployeeId,
                        Subject = "Medical Aid Dependent Turning 21",
                        Message =
                            $"Your child dependent {dependent.FirstName} {dependent.LastName} " +
                            $"will turn 21 during the next payroll period.\n\n" +

                            $"Medical Aid Option: {deduction.OptionName}\n\n" +

                            $"Premium Change:\n" +
                            $"Child Premium: {childPremium:C}\n" +
                            $"Adult Premium: {adultPremium:C}\n\n" +

                            $"Current Total Deduction: {currentTotal:C}\n" +
                            $"New Total Deduction: {newTotal:C}",

                        HtmlMessage = htmlMessage,
                        Type = NotificationType.MedicalAidDependent,
                        Severity = NotificationSeverity.Warning
                    };

                    // In-App
                    notification.DeliveryChannel = DeliveryChannel.InApp;
                    await _notificationFactory.ProduceNotificationAsync(notification);

                    // Email
                    notification.DeliveryChannel = DeliveryChannel.Email;
                    await _notificationFactory.ProduceNotificationAsync(notification);
                }
                else
                {
                    Console.WriteLine("No notification required.");
                }
            }

            Console.WriteLine("Finished checking child dependents.");
        }
        /// <summary>
        /// Converts a payroll run number to its corresponding calendar month.
        /// </summary>
        /// <param name="payrollRunNumber">The payroll run number from 1 to 12.</param>
        /// <returns>
        /// The calendar month associated with the payroll run number.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the payroll run number is outside the range of 1 to 12.</exception>
        private int PayrollRunToCalendarMonth(int payrollRunNumber)
        {
            return payrollRunNumber switch
            {
                1 => 4,   // April
                2 => 5,
                3 => 6,
                4 => 7,
                5 => 8,   // August
                6 => 9,
                7 => 10,
                8 => 11,
                9 => 12,
                10 => 1,  // January
                11 => 2,
                12 => 3, // March
                _ => throw new ArgumentOutOfRangeException(nameof(payrollRunNumber))
            };
        }

    }
}