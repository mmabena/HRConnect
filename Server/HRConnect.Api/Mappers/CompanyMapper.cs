namespace HRConnect.Api.Mappers
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using HRConnect.Api.Models;
  using HRConnect.Api.DTOs.Company;
  public static class CompanyMapper
  {
    public static CompanyDto ToCompanyDto(this Company companyModel)
    {
      return new CompanyDto
      {
        CompanyId = companyModel.CompanyId,
        CompanyName = companyModel.CompanyName,
        RegistrationNumber = companyModel.RegistrationNumber,
        UIFNumber = companyModel.UIFNumber,
        VATNumber = companyModel.VATNumber,
        ContactNumber = companyModel.ContactNumber,
        CompanyAddress = companyModel.CompanyAddress
      };
    }
    public static Company ToCompanyFromCreateDTO(this CreateCompanyRequestDto companyRequestDto)
    {
      return new Company
      {
        CompanyId = companyRequestDto.CompanyId,
        CompanyName = companyRequestDto.CompanyName,
        RegistrationNumber = companyRequestDto.RegistrationNumber,
        UIFNumber = companyRequestDto.UIFNumber,
        VATNumber = companyRequestDto.VATNumber,
        ContactNumber = companyRequestDto.ContactNumber,
        CompanyAddress = companyRequestDto.CompanyAddress
      };
    }
  }
}