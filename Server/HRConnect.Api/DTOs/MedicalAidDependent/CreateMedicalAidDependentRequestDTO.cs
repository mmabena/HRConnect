namespace HRConnect.Api.DTOs.MedicalAidDependent
{
    using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HRConnect.Api.Models;
    public class CreateMedicalAidDependentRequestDTO
    {
        public string DependentId { get; set; } = string.Empty;
        public string? FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; } = string.Empty;
        public string? IdNumber { get; set; } = string.Empty;
        public string? PassportNumber { get; set; } = string.Empty;
        public Gender? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public Relationship Relationship { get; set; }
        public bool IsActive { get; set; } = true;

    }
}