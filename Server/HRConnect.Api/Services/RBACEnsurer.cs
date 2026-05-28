namespace HRConnect.Api.Services
{
  using HRConnect.Api.Data;
  using HRConnect.Api.Utils.AccessControl;
  using HRConnect.Api.Interfaces.AccessControl;
  using HRConnect.Api.Models;
  using Microsoft.EntityFrameworkCore;

  public interface IRBACEnsurer
  {
    Task EnsureRoleBasedAccessControlAsync();
  }
  ///<summary>
  ///This is used to ensure that all the roles and permissions exist in the 
  ///system. This is later used to be able to migrate to new Role Based
  ///Access Control.
  ///Sealed so that this class is not inheritable
  ///</summary>
  public sealed class RBACEnsurer : IRBACEnsurer
  {

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
    public async Task EnsureRoleBasedAccessControlAsync()
    {
      // Create permissions first
      await EnsurePermissionsExistsAsync();

      //Create Roles and Configure hierarchy 
      await EnsureRoleExistsAsync();
    }
    private async Task ConfigureHierarchyAsync()
    {
      //CEO -> Executive -> SuperUser -> User
      var ceo = await _roleRepo.GetRoleByNameAsync("CEO");
      var executive = await _roleRepo.GetRoleByNameAsync("Executive");
      var superUser = await _roleRepo.GetRoleByNameAsync("SuperUser");
      var normalUser = await _roleRepo.GetRoleByNameAsync("NormalUser");

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

      await _context.SaveChangesAsync();
    }

    private async Task EnsurePermissionsExistsAsync()
    {
      var permissionsList = new[]
      {
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
      };

      foreach (var permission in permissionsList)
      {
        bool exits = await _context.Permissions
          .AnyAsync(p => p.Key == permission);
        if (!exits)
        {
          await _context.Permissions.AddAsync(new Permissions
          {
            Key = permission,
          });
        }
      }
      await _context.SaveChangesAsync();
    }

    private async Task EnsureRoleExistsAsync()
    {
      var roles = new[]
      {
        RoleSet.SuperUser,
        RoleSet.NormalUser,
        RoleSet.Executive,
        RoleSet.CEO,
      };

      foreach (var role in roles)
      {
        var exist = await _context.Roles.AnyAsync(r => r.Name == role);
        if (!exist)
        {
          await _context.Roles.AddAsync(new Roles
          {
            Name = role
          });
        }
      }
    }

  }
}