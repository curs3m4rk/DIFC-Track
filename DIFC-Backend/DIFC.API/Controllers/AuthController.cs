using DIFC.Application.DTOs.Auth;
using DIFC.Application.Interfaces.Auth;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterRequestDTO request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.RegisterAsync(request);

        if(!result.Success)
            return BadRequest(result);

        return Ok(result.Message);
    }

    /// <summary>
    /// POST /api/auth/login
    /// Accepts email + password, returns JWT access token + refresh token.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
    {
        if(!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await _authService.LoginAsync(request);

        if(!response.Success)
        {
            // 401 Unauthorized if user is invalid or account is locked
            return Unauthorized(new {message = response.Error});
        }

        // 200 OK with token data if login is successful
        return Ok(response.Data);
    }

}