namespace HRConnect.Api.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HRConnect.Api.Models;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    public class EmployeeLeaveBalance
    {
        public int Id { get; set; }

        public Guid EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        public int LeaveTypeId { get; set; }
        public LeaveType LeaveType { get; set; } = null!;

        public decimal AccruedDays { get; set; }
        public DateOnly? LastAccrualDate { get; set; }

        public decimal TakenDays { get; set; }
        public decimal AvailableDays { get; set; }
        public decimal CarryoverDays { get; set; }
        public decimal ForfeitedDays { get; set; }
        public int? LastResetYear { get; set; }
        public DateOnly? LastCalculatedDate { get; set; }

        [Timestamp]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}