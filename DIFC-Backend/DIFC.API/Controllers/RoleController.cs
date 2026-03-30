using DIFC.Application.DTOs.Role;
using DIFC.Application.Interfaces;
using DIFC.Application.Interfaces.Role;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace DIFC.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateRole([FromBody] RoleRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _roleService.CreateRoleAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result.Message);
        }

        [HttpGet("getAllRoles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _roleService.GetAllRolesAsync();

            return Ok(roles);
        }

        [HttpGet("getByName/{roleName}")]
        public async Task<IActionResult> GetRoleByName(string roleName)
        {
            var result = await _roleService.GetRoleByNameAsync(roleName);

            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result.Data);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{roleName}")]
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            var result = await _roleService.DeleteRoleAsync(roleName);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result.Message);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateRole(UpdateRoleDTO request)
        { 
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _roleService.UpdateRoleAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result.Message);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("assign")]
        public async Task<IActionResult> AssignRoles([FromBody] AssignRoleRequest request)
        {
            var result = await _roleService.AssignRolesAsync(request);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(new
            {
                message = result.Message,
                roles = result.Data
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("unassign")]
        public async Task<IActionResult> UnassignRoles([FromBody] AssignRoleRequest request)
        {
            var result = await _roleService.UnassignRolesAsync(request);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(new
            {
                message = result.Message,
                roles = result.Data
            });
        }

    }
}