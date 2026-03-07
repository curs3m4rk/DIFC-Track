using DIFC.Application.DTOs.Role;
using DIFC.Application.Interfaces;
using DIFC.Application.Interfaces.Role;
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

        [HttpPost("createRole")]
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

        [HttpGet("{roleName}")]
        public async Task<IActionResult> GetRoleByName(string roleName)
        {
            var result = await _roleService.GetRoleByNameAsync(roleName);

            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result.Data);
        }
    }
}