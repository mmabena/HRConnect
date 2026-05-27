namespace HRConnect.Api.Controllers
{
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.AspNetCore.Authorization;
  [Route("api/permissions")]
  [Authorize(Roles = "SuperUser")]
  [ApiController]
  public class PermissionsController : ControllerBase
  {
    public PermissionsController()
    {
    }

    [HttpGet("roles")] //Get All Permissions
    public async Task<IActionResult> GetAllRoles()
    {
      throw new NotImplementedException();
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllPermissions()
    {
      throw new NotImplementedException();
    }

    //Get all permissions for a roles
    [HttpGet("roles/{roleId}/permissions")]
    public async Task<IActionResult> GetPermissionsForRole(int roleId)
    {
      throw new NotImplementedException();
    }

    //Assign a permissions to a role
    [HttpPost("/roles/{roleId}/permissions")]
    public async Task<IActionResult> AssignPermissionToRole(int roleId)
    {
      throw new NotImplementedException();
    }

    [HttpDelete("roles/{roleid}/permissions/{permissionId}")]
    public async Task<IActionResult> DeletePermissionFromRole(int roleId, int permissionId)
    {
      throw new NotImplementedException();
    }

  }
}