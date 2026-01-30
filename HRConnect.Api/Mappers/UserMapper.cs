namespace HRConnect.Api.Mappers
{
    using HRConnect.Api.DTOs.User;
    using HRConnect.Api.Models;
    public static class UserMapper
    {
        public static UserRegisterDto ToUserDto(this User userModel)
        {
            return new UserRegisterDto
            {
            /// <summary>
            /// Maps the email from the user model to the DTO while leaving the password empty
            /// for security reasons. The password is intentionally not included to prevent exposing
            /// sensitive information.
            /// </summary>
                Email = userModel.Email,
                Password = string.Empty 
            };
        }

        public static User ToUserFromCreateUserRequestDto(this CreateUserRequestDto createUserDto)
        {
            return new User
            {
                Email = createUserDto.Email,
                /// <summary>
                // PasswordHash should not be set here. Controller is responsible for hashing.
                /// </summary>
                PasswordHash = string.Empty,
                
                Role = createUserDto.Role
            };
        }
    }
}