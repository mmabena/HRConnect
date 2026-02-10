
namespace HRConnect.Api.Interfaces
{
    using System.Threading.Tasks;
    using HRConnect.Api.Models;
    public interface IEmployeeRepository
    {
        Task<Employee?> GetByIdAsync(int employeeId);

    }
}