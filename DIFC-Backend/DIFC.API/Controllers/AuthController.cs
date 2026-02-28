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

    /// <summary>
    /// POST /api/auth/refresh
    /// 
    /// Called by the client when their access token expires.
    /// Client sends the expired access token + their refresh token.
    /// Returns a brand new access token + new refresh token.
    /// 
    /// The client should call this automatically (silently) when they
    /// get a 401 response — the user never sees a "please log in" popup
    /// unless the refresh token itself has also expired.
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDTO request)
    {
        if(!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await _authService.RefreshAsync(request);

        if(!response.Success)
        {
            return Unauthorized(new {message = response.Error});
        }

        return Ok(response.Data);
    }

}