
namespace HRConnect.Api.Interfaces
{
    using HRConnect.Api.Models;
    using System.Threading.Tasks;

    public interface ILeaveTypeRepository
    {
        Task<List<LeaveType>> GetAllAsync();
    }
}