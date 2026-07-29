namespace HRConnect.Api.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HRConnect.Api.DTOs.MedicalAidDependent;
    using HRConnect.Api.Data;
    public interface IMedicalAidDependentService
    {
        Task<List<MedicalAidDependentDTO>> GetAllMedicalAidDependentsAsync();
        Task<MedicalAidDependentDTO> GetMedicalAidDependentsByIdAsync(string dependentId);
        Task<MedicalAidDependentDTO> ValidateMedicalAidDependentAsync(string employeeId, CreateMedicalAidDependentRequestDTO medicalAidDependentRequestDto);
        Task<MedicalAidDependentDTO> CreateMedicalAidDependentAsync(string employeeId, CreateMedicalAidDependentRequestDTO medicalAidDependentRequestDto);
        Task<List<MedicalAidDependentDTO>> GetMedicalAidDependentsByEmployeeIdAsync(string employeeId);
        // Task<MedicalAidDependentDTO?> UpdateMedicalAidDependent(string dependentId, UpdateMedicalAidDependentRequestDTO dto);
        // Task<bool> DeleteMedicalAidDependent(string dependentId);

    }
}