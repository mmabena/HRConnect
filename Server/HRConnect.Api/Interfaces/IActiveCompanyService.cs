namespace HRConnect.Api.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    public interface IActiveCompanyService
    {
        Task<string> GetActiveCompanyIdAsync(int userId);
    }
}