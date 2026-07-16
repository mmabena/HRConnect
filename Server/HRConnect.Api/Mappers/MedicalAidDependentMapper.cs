namespace HRConnect.Api.Mappers
{
    using HRConnect.Api.Models;
  using HRConnect.Api.DTOs.MedicalAidDependent;
    public static class MedicalAidDependentMapper
    {

        public static MedicalAidDependentDTO ToMedicalAidDependentDto(this MedicalAidDependent medicalAidDependentModel)
        {
            return new MedicalAidDependentDTO
            {
                DependentId = medicalAidDependentModel.DependentId,
                EmployeeId = medicalAidDependentModel.EmployeeId,
                FirstName = medicalAidDependentModel.FirstName,
                LastName = medicalAidDependentModel.LastName,
                IdNumber = medicalAidDependentModel.IdNumber,
                PassportNumber = medicalAidDependentModel.PassportNumber,
                Gender = medicalAidDependentModel.Gender,
                DateOfBirth = medicalAidDependentModel.DateOfBirth,
                Relationship = medicalAidDependentModel.Relationship,
                IsActive = medicalAidDependentModel.IsActive,
                CreatedDate = medicalAidDependentModel.CreatedDate,
                UpdatedDate = medicalAidDependentModel.UpdatedDate
            };
        }
        public static MedicalAidDependent ToMedicalAidDependentFromCreateDTO(this CreateMedicalAidDependentRequestDTO medicalAidDependentRequestDto)
        {
            return new MedicalAidDependent
            {
                DependentId = medicalAidDependentRequestDto.DependentId,
                FirstName = medicalAidDependentRequestDto.FirstName,
                LastName = medicalAidDependentRequestDto.LastName,
                IdNumber = medicalAidDependentRequestDto.IdNumber,
                PassportNumber = medicalAidDependentRequestDto.PassportNumber,
                Gender = medicalAidDependentRequestDto.Gender!.Value,
                DateOfBirth = medicalAidDependentRequestDto.DateOfBirth,
                Relationship = medicalAidDependentRequestDto.Relationship,
                IsActive = medicalAidDependentRequestDto.IsActive,
            };
        }  
    }
}