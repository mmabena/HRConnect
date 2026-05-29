namespace HRConnect.Tests
{

  using HRConnect.Api.Models;
  public static class UserTestData
  {
    public static TheoryData<List<User>, List<Employee>, List<string>> SetRandomUsersAndEmployees => new()
    {
      {
         new List<User>
        {
                new User
                {
                    UserId = 1,
                    Email = "user1@singular.co.za",
                    Role = UserRole.SuperUser
                },

                new User
                {
                    UserId = 2,
                    Email = "user2@singular.co.za",
                    Role = UserRole.NormalUser
                },

                new User
                {
                    UserId = 3,
                    Email = "user3@singular.co.za",
                    Role = UserRole.SuperUser
                }
       },
    new List<Employee>
    {
        new Employee{EmployeeId="EMP001",Email="user1@singular.co.za"},
        new Employee{EmployeeId="EMP002",Email = "user2@singular.co.za"},
        new Employee{EmployeeId="EMP003",Email = "user3@singular.co.za"}
    },
        new List<string>
        {
                "EMP001",
                "EMP003"
        }
      }
    };


    public static TheoryData<Employee> OrganisedSuperUserByEmployeeId => new TheoryData<Employee>
    {
        new Employee{EmployeeId="EMP001",Email="user1@singular.co.za"},
        new Employee{EmployeeId="EMP002",Email = "user2@singular.co.za"},
        new Employee{EmployeeId="EMP003",Email = "user3@singular.co.za"}
    };
  }
}