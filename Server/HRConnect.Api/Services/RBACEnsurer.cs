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
    Task MigrateAndBackfillUsersToRole();
  }

  ///<summary>
  ///This is used to ensure that all the roles and permissions exist in the 
  ///system. This is later used to be able to migrate to new Role Based
  ///Access Control.
  ///Sealed so that this class is not inheritable
  ///</summary>
  public sealed partial class RBACEnsurer : IRBACEnsurer
  {
    [GeneratedRegex("(?<=[A-Z])(?=[A-Z][a-z])|(?<!^)([A-Z][a-z])")]
    private static partial Regex AddSpacesRegex();
    private readonly ApplicationDBContext _context;
    private readonly IRolesRepository _roleRepo;
    private readonly IUserRolesRepository _userRoleRepo;

    public RBACEnsurer(ApplicationDBContext context, IRolesRepository roleRepo,
        IUserRolesRepository userRoleRepo)
    {
      _context = context;
      _roleRepo = roleRepo;
      _userRoleRepo = userRoleRepo;
    }
    private static string AddSpaces(string s)
    {
      return AddSpacesRegex().Replace(s, " $1");
    }
    public async Task MigrateAndBackfillUsersToRole()
    {
      await _userRoleRepo.MigrateUserEnumRoles();
    }
    public async Task EnsureRoleBasedAccessControlAsync()
    {
      // Create permissions first
      await EnsurePermissionsExistsAsync();

      //Create Roles and Configure hierarchy 
      if (await EnsureRoleExistsAsync())
        await ConfigureHierarchyAsync();

    }

    private async Task SetDefaultAccessControl(Roles? role, params string[] permissionsList)
    {
      List<Permissions> permissions = await _context.Permissions
        .Where(p => permissionsList.Contains(p.Key)).ToListAsync();

      foreach (Permissions p in permissions)
      {
        bool alreadyAssigned = role!.RolePermissions.Any(rp => rp.PermissionsId == p.PermissionsId);

        if (alreadyAssigned)
          continue;

        _ = await _context.RolePermissions.AddAsync(new RolePermissions
        {
          RoleId = role.RoleId,
          PermissionsId = p.PermissionsId,
          Role = role,
          Permissions = p,
          IsGranted = true
        });
      }
      await _context.SaveChangesAsync();
    }

    public async Task ConfigureHierarchyAsync()
    {
      //CEO -> Executive -> SuperUser -> User
      Roles? ceo = await _roleRepo.GetRoleByNameAsync("CEO");
      Roles? executive = await _roleRepo.GetRoleByNameAsync("Executive User");
      Roles? superUser = await _roleRepo.GetRoleByNameAsync("Super User");
      Roles? normalUser = await _roleRepo.GetRoleByNameAsync("Normal User");

      //Walk up the hierarchy and link the roles correctly
      normalUser!.ParentRole = superUser;
      normalUser!.ParentRoleId = superUser!.RoleId;

      superUser!.ParentRole = executive;
      superUser!.ParentRoleId = executive!.RoleId;

      executive!.ParentRole = ceo!;
      executive!.ParentRoleId = ceo!.RoleId;

      ceo!.ParentRoleId = null;
      ceo!.ParentRole = null;

      await SetDefaultAccessControl(normalUser,
          PermissionSet.PayrollViewOwn,
          PermissionSet.PayrollViewOwn,
          PermissionSet.LeaveApply,
          PermissionSet.LeaveViewOwn,
          PermissionSet.PayrollToolsCalculator);

      await SetDefaultAccessControl(superUser,
          PermissionSet.EmployeeViewAll,
          PermissionSet.EmployeeCreate,
          PermissionSet.EmployeeEdit,
          PermissionSet.EmployeeViewPayslip,
          PermissionSet.CompanySwitch,
          PermissionSet.TaxManagePartial,
          PermissionSet.LeaveManagePartial,
          PermissionSet.PositionManagePartial,
          PermissionSet.CompanyViewDetails
        );
      await SetDefaultAccessControl(executive,
          PermissionSet.BudgetSet,
          PermissionSet.BudgetView,
          PermissionSet.PayrollBenchmarkCapture,
          PermissionSet.PayrollBenchmarkView
      );
      await SetDefaultAccessControl(ceo,
          PermissionSet.BudgetViewOnly,
          PermissionSet.BudgetApprove,
          PermissionSet.BudgetComment,
          PermissionSet.PayrollBenchmarkViewOnly
      );

      superUser!.ChildRoles.Add(normalUser!);
      executive!.ChildRoles.Add(superUser);
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
          PermissionSet.LeaveViewOwn,
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
            Description = PermissionSet.PermissionDescription[permission]
          });
        }
      }
      _ = await _context.SaveChangesAsync();
    }

    private async Task<bool> EnsureRoleExistsAsync()
    {
      bool shouldConfigure = false;
      string[] roles =
     [
        RoleSet.SuperUser,
        RoleSet.NormalUser,
        RoleSet.Executive,
        RoleSet.CEO,
     ];

      foreach (string role in roles)
      {
        Enum.TryParse<RoleName>(role, true, out RoleName roleName);
        bool exists = await _context.Roles.AnyAsync(r => r.RoleName == roleName);
        if (!exists)
        {
          //Parse string to RoleName and ignore casing 
          _ = await _context.Roles.AddAsync(new Roles
          {
            Name = AddSpaces(role),
            RoleName = roleName
          });
          shouldConfigure = true;
        }
      }
      _ = await _context.SaveChangesAsync();
      return shouldConfigure;
    }
  }
}