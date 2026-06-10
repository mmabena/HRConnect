namespace HRConnect.Api.Controllers
{
  using HRConnect.Api.Models;
  using HRConnect.Api.Interfaces.AccessControl;
  using HRConnect.Api.DTOs.AccessControl;
  using Microsoft.AspNetCore.Mvc;

  [Route("api/permissions")]
  [ApiController]
  public class PermissionsController : ControllerBase
  {
    private readonly IRolePermissionService _rolePermissionService;
    private readonly IRoleService _roleService;
    private readonly IPermissionService _permissionService;
    public PermissionsController(IRolePermissionService rolePermissionService,
    IRoleService roleService, IPermissionService permissionService)
    {
      _rolePermissionService = rolePermissionService;
      _roleService = roleService;
      _permissionService = permissionService;
    }

    [HttpGet("roles")] //Get All Permissions
    public async Task<IActionResult> GetAllRoles()
    {
      var roles = await _roleService.GetAllRolesAsync();
      return Ok(roles);
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllPermissions()
    {
      var permissions = await _permissionService.GetAllPermissionsAsync();
      return Ok(permissions);
    }

    //Get all permissions for a roles
    [HttpGet("roles/{roleId}/permissions")]
    public async Task<IActionResult> GetPermissionsForRole(int roleId)
    {
      IEnumerable<Permissions> permissions = await _rolePermissionService
      .GetPermissionsForRoleAsync(roleId);
      return Ok(permissions);
    }

    //Assign a permissions to a role
    [HttpPost("/roles/{roleId}/permissions")]
    public async Task<IActionResult> AssignPermissionToRole(int roleId, [FromBody] AssignPermissionToRoleDTO dto)
    {
      try
      {
        await _rolePermissionService.AssignPermissionsToRoleByIdAsync(roleId, dto.PermissionsArray);

        return Ok($"Successfully Assigned Permissions To Role");
      }
      catch
      {
        return BadRequest($"Failed To Assign Permissions To Role");
      }
    }

    [HttpDelete("roles/{roleId}/permissions/{permissionId}")]
    public async Task<IActionResult> DeletePermissionFromRole(int roleId, int permissionId)
    {
      try
      {
        await _rolePermissionService.RemovePermissionsFromRoleAsync(roleId, permissionId);
        return Ok($"Successfully Remove Permission From Role");
      }
      catch
      {
        return BadRequest($"Failed To Remove Permission");
      }
    }

  }
}