namespace HRConnect.Api.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HRConnect.Api.Models;
    using HRConnect.Api.Models.Payroll;
    using System.Threading.Tasks;
    public interface IMedicalAidDependentNotificationService
    {
        Task NotifyChildrenTurning21Async(PayrollRun currentRun);

    }
}