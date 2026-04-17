namespace HRConnect.Api.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HRConnect.Api.Models;
    using HRConnect.Api.DTOs.UserCompany;
    using System.Threading.Tasks;
    public static class UserCompanyMapper
    {
        public static UserCompanyDto ToUserCompanyDto(this UserCompany userCompanyModel)
        {
            return new UserCompanyDto
            {
                CompanyId = userCompanyModel.CompanyId,
                CompanyName = userCompanyModel.Company.CompanyName,
                IsDefault = userCompanyModel.IsDefault
            };
        }
        public static UserCompany ToUserCompanyFromCreateDTO(this CreateUserCompanyDto userCompanyRequestDto)
        {
            return new UserCompany
            {
                UserId = userCompanyRequestDto.UserId,
                CompanyId = userCompanyRequestDto.CompanyId,
                IsDefault = userCompanyRequestDto.IsDefault,
                CreatedAt = DateTime.UtcNow
                
            };
            
        }


    }
}