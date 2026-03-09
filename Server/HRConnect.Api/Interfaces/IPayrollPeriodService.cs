using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRConnect.Api.Interfaces
{
  public interface IPayrollPeriodService
  {
    Task ExecuteRolloverAsync();
  }
}