namespace HRConnect.Api.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HRConnect.Api.Data;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models.CompanyContributions;
    public interface IEmployeeCompanyContributionService
    {
        Task<List<EmployeeCompanyContribution>> GetByPayRunIdAsync(int payRunId);
    }
}