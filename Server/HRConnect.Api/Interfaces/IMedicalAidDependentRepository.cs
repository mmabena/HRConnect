namespace HRConnect.Api.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HRConnect.Api.Models;
    using System.Threading.Tasks;
    public interface IMedicalAidDependentRepository
    {
        Task<List<MedicalAidDependent>> GetAllMedicalAidDependentsAsync();
        Task<MedicalAidDependent> GetMedicalAidDependentByIdAsync (string dependentId);
        Task<MedicalAidDependent> CreateMedicalAidDependentAsync(MedicalAidDependent medicalAidDependentModel);
        Task<List<MedicalAidDependent>> GetMedicalAidDependentsByEmployeeIdAsync(string employeeId);
        Task<MedicalAidDependent?> UpdateMedicalAidDependentAsync(MedicalAidDependent medicalAidDependentModel);
        Task<List<MedicalAidDependent>> GetActiveChildDependentsAsync();
        Task<bool> DeleteMedicalAidDependentAsync(string dependentId);


    }
}