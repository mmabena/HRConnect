namespace HRConnect.Api.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public interface IEmailTemplateService
    {
        Task<string> GetMedicalAidDependentTurning21TemplateAsync(
            string dependentName,
            string medicalAidOption,
            string childPremium,
            string adultPremium,
            string premiumIncrease,
            string currentTotal,
            string newTotal);
    }

    public class EmailTemplateService : IEmailTemplateService
    {
        public async Task<string> GetMedicalAidDependentTurning21TemplateAsync(
            string dependentName,
            string medicalAidOption,
            string childPremium,
            string adultPremium,
            string premiumIncrease,
            string currentTotal,
            string newTotal)
        {
            string templatePath = Path.Combine(
                AppContext.BaseDirectory,
                "EmailTemplates",
                "MedicalAidDependentTurning21.html"
            );

            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException(
                    $"Email template not found: {templatePath}");
            }

            string template = await File.ReadAllTextAsync(templatePath);

            return template
                .Replace("{{DependentName}}", dependentName)
                .Replace("{{MedicalAidOption}}", medicalAidOption)
                .Replace("{{ChildPremium}}", childPremium)
                .Replace("{{AdultPremium}}", adultPremium)
                .Replace("{{PremiumIncrease}}", premiumIncrease)
                .Replace("{{CurrentTotal}}", currentTotal)
                .Replace("{{NewTotal}}", newTotal);
        }

    }
}