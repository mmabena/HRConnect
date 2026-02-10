namespace HRConnect.Api.Repository
{
    using HRConnect.Api.Data;
    using HRConnect.Api.DTOs;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly ApplicationDBContext _context;

        public EmployeeRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<Employee?> GetByIdAsync(int employeeId)
        {
            return await _context.Employees.FindAsync(employeeId);
        }

        public async Task AddAsync(Employee employee)
        {
            await _context.Employees.AddAsync(employee);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<LeaveEntitlementResponse>> GetLeaveEntitlementsAsync(int employeeId)
        {
            return await _context.LeaveEntitlementRules
                .Where(e => e.EmployeeId == employeeId)
                .Select(e => new LeaveEntitlementResponse
                {
                    EmployeeName = _context.Employees
                        .Where(emp => emp.EmployeeId == employeeId)
                        .Select(emp => emp.Name)
                        .First(),

                    EmployeeSurname = _context.Employees
                        .Where(emp => emp.EmployeeId == employeeId)
                        .Select(emp => emp.Surname)
                        .First(),

                    LeaveType = _context.LeaveTypes
                        .Where(t => t.LeaveTypeId == e.LeaveTypeId)
                        .Select(t => t.Name)
                        .First(),

                    DaysEntitled = e.DaysEntitled,

                    DaysAvailable = _context.EmployeeLeaveBalances
                        .Where(b =>
                            b.EmployeeId == employeeId &&
                            b.LeaveTypeId == e.LeaveTypeId)
                        .Select(b => b.DaysAvailable)
                        .First()
                })
                .ToListAsync();
        }
    }
}
