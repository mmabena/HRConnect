namespace HRConnect.Api.Services
{
  using HRConnect.Api.Data;
  using HRConnect.Api.Utils.AccessControl;
  using HRConnect.Api.Interfaces.AccessControl;
  using HRConnect.Api.Models;
  using Microsoft.EntityFrameworkCore;
  using System.Text.RegularExpressions;

  public interface IRBACEnsurer
  {
    Task EnsureRoleBasedAccessControlAsync();
    Task ConfigureHierarchyAsync();
  }
  ///<summary>
  ///This is used to ensure that all the roles and permissions exist in the 
  ///system. This is later used to be able to migrate to new Role Based
  ///Access Control.
  ///Sealed so that this class is not inheritable
  ///</summary>
  public sealed partial class RBACEnsurer : IRBACEnsurer
  {
    [GeneratedRegex("(?<=[A-Z])(?=[A-Z]|[a-z])|(?<!^)(A-Z)")]
    private static partial Regex AddSpacesRegex();
    private readonly ApplicationDBContext _context;
    private readonly IRolesRepository _roleRepo;
    private readonly IPermissionsRepository _permissionsRepo;

    public RBACEnsurer(ApplicationDBContext context, IRolesRepository roleRepo,
        IPermissionsRepository permissionsRepo)
    {
      _context = context;
      _roleRepo = roleRepo;
      _permissionsRepo = permissionsRepo;
    }
    private static string AddSpaces(string s)
    {
      return AddSpacesRegex().Replace(s, "$1");
    }
    public async Task EnsureRoleBasedAccessControlAsync()
    {
      // Create permissions first
      await EnsurePermissionsExistsAsync();

      //Create Roles and Configure hierarchy 
      await EnsureRoleExistsAsync();

      //Configure roles hierarchy 
      // await ConfigureHierarchyAsync();
    }
    public async Task ConfigureHierarchyAsync()
    {
      //CEO -> Executive -> SuperUser -> User
      Roles? ceo = await _roleRepo.GetRoleByNameAsync("CEO");
      Roles? executive = await _roleRepo.GetRoleByNameAsync("Executive");
      Roles? superUser = await _roleRepo.GetRoleByNameAsync("SuperUser");
      Roles? normalUser = await _roleRepo.GetRoleByNameAsync("NormalUser");

      //Walk up the hierarchy and link the roles correctly
      normalUser!.ParentRole = superUser;
      normalUser!.ParentRoleId = superUser!.RoleId;

      superUser!.ParentRole = executive;
      superUser!.ParentRoleId = executive!.RoleId;
      superUser.ChildRoles.Add(normalUser!);

      executive!.ParentRole = ceo!;
      executive!.ParentRoleId = ceo!.RoleId;
      executive!.ChildRoles.Add(superUser);

      ceo!.ParentRoleId = null;
      ceo!.ParentRole = null;
      ceo!.ChildRoles.Add(executive!);

      _ = await _context.SaveChangesAsync();
    }

    private async Task EnsurePermissionsExistsAsync()
    {
      string[] permissionsList =
     [
          PermissionSet.EmployeeViewOwn,
          PermissionSet.PayrollViewOwn,
          PermissionSet.LeaveApply,
          PermissionSet.PayrollToolsCalculator,
          PermissionSet.EmployeeViewAll,
          PermissionSet.EmployeeCreate,
          PermissionSet.EmployeeEdit,
          PermissionSet.EmployeeViewPayslip,
          PermissionSet.CompanySwitch,
          PermissionSet.TaxManagePartial,
          PermissionSet.LeaveManagePartial,
          PermissionSet.PositionManagePartial,
          PermissionSet.CompanyViewDetails,
          PermissionSet.BudgetSet,
          PermissionSet.BudgetView,
          PermissionSet.PayrollBenchmarkCapture,
          PermissionSet.PayrollBenchmarkView,
          PermissionSet.BudgetViewOnly,
          PermissionSet.BudgetApprove,
          PermissionSet.BudgetComment,
          PermissionSet.PayrollBenchmarkViewOnly
     ];

      foreach (string permission in permissionsList)
      {
        bool exists = await _context.Permissions
          .AnyAsync(p => p.Key == permission);
        if (!exists)
        {
          _ = await _context.Permissions.AddAsync(new Permissions
          {
            Key = permission,
          });
        }
      }
      _ = await _context.SaveChangesAsync();
    }

    private async Task EnsureRoleExistsAsync()
    {
      string[] roles =
     [
        RoleSet.SuperUser,
        RoleSet.NormalUser,
        RoleSet.Executive,
        RoleSet.CEO,
     ];
      // string addSpaces(string s)
      // {
      //   return Regex.Replace(
      //      s, "(?<=[A-Z])(?=[A-Z]|[a-z])|(?<!^)(A-Z)");
      // }

      foreach (string role in roles)
      {
        bool exists = await _context.Roles.AnyAsync(r => r.Name == role);
        if (!exists)
        {
          //Parse string to RoleName and ignore casing 
          Enum.TryParse<RoleName>(role, true, out RoleName roleName);
          _ = await _context.Roles.AddAsync(new Roles
          {
            Name = AddSpaces(role),
            RoleName = roleName
          });
          Console.WriteLine($"REGEX {role} -> {AddSpaces(role)}");
        }
      }
      // _ = await _context.SaveChangesAsync();
    }
  }
}