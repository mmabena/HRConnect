namespace HRConnect.Api.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HRConnect.Api.Data;
    using HRConnect.Api.Models.CompanyContributions;
    using System.Threading.Tasks;
    using HRConnect.Api.Interfaces;
    public interface IEmployeeCompanyContributionService
    {
        Task<List<EmployeeCompanyContribution>> GetByPayRunIdAsync(int payRunId);
    }
}