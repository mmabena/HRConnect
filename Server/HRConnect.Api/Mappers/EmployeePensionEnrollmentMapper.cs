namespace HRConnect.Api.Mappers
{
  using HRConnect.Api.DTOs.Employee.Pension;
  using HRConnect.Api.Models.Pension;

  public static class EmployeePensionEnrollmentMapper
  {
    public static EmployeePensionEnrollment EmployeePensionEnrollmentToAddDTO(this EmployeePensionEnrollmentAddDto employeePensionEnrollmentDto)
    {
      return new EmployeePensionEnrollment
      {
        EmployeeId = employeePensionEnrollmentDto.EmployeeId,
        VoltunaryContribution = employeePensionEnrollmentDto.VoltunaryContribution,
        IsVoluntaryContributionPermament = employeePensionEnrollmentDto.IsVoluntaryContributionPermament,
      };
    }

    public static EmployeePensionEnrollmentDto ToEmployeePensionEnrollmentDto(this EmployeePensionEnrollment employeePensionEnrollment)
    {
      return new EmployeePensionEnrollmentDto
      {
        PensionOptionId = employeePensionEnrollment.PensionOptionId,
        EmployeeId = employeePensionEnrollment.EmployeeId,
        StartDate = employeePensionEnrollment.StartDate,
        EffectiveDate = employeePensionEnrollment.EffectiveDate,
        VoltunaryContribution = employeePensionEnrollment.VoltunaryContribution,
        IsVoluntaryContributionPermament = employeePensionEnrollment.IsVoluntaryContributionPermament,
        PayrollRunId = employeePensionEnrollment.PayrollRunId
      };
    }
  }
}
