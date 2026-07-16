namespace HRConnect.Api.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using System.ComponentModel.DataAnnotations.Schema;
    public enum Relationship
    {
        Spouse,
        Child,
        Parent,
        Sibling,
        Other
    }
    public class MedicalAidDependent
    {
        [Key]
        public string DependentId { get; set; } = string.Empty;
        [Required]
        public string EmployeeId { get; set; } = string.Empty;
        public Employee Employee { get; set; } = null!;
        [Required]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;
        [StringLength(13)]
        public string? IdNumber { get; set; } = string.Empty;
        public string? PassportNumber { get; set; } = string.Empty;
        [Required]
        public Gender Gender { get; set; }
        [Required]
        public DateTime? DateOfBirth { get; set; }
        [Required]
        public Relationship Relationship { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? CreatedDate { get; set; } =  DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; } = DateTime.UtcNow;

    }
}