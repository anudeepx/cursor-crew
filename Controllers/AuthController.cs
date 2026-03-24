using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailOrderingWebsite.Data;
using RetailOrderingWebsite.DTOs;
using RetailOrderingWebsite.DTOs.Auth;
using RetailOrderingWebsite.Models;
using RetailOrderingWebsite.Services;

namespace RetailOrderingWebsite.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private const int PasswordWorkFactor = 10;
    private readonly AppDbContext _dbContext;
    private readonly JwtTokenService _jwtTokenService;
    private readonly UserRoleService _userRoleService;

    public AuthController(AppDbContext dbContext, JwtTokenService jwtTokenService, UserRoleService userRoleService)
    {
        _dbContext = dbContext;
        _jwtTokenService = jwtTokenService;
        _userRoleService = userRoleService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register(RegisterRequestDto request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var existingUser = await _dbContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email.ToLower() == normalizedEmail);
        if (existingUser)
        {
            return BadRequest(ApiResponse<AuthResponseDto>.Fail("Email already registered."));
        }

        var user = new User
        {
            Name = request.Name,
            Email = request.Email.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, PasswordWorkFactor)
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var token = _jwtTokenService.GenerateToken(user);
        var role = _userRoleService.GetRoleForUser(user);
        var response = new AuthResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = role,
            Token = token
        };

        return Ok(ApiResponse<AuthResponseDto>.Ok(response, "User registered successfully."));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login(LoginRequestDto request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(ApiResponse<AuthResponseDto>.Fail("Invalid email or password."));
        }

        var token = _jwtTokenService.GenerateToken(user);
        var role = _userRoleService.GetRoleForUser(user);
        var response = new AuthResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = role,
            Token = token
        };

        return Ok(ApiResponse<AuthResponseDto>.Ok(response, "Login successful."));
    }

    [HttpPost("logout")]
    [Authorize]
    public ActionResult<ApiResponse<string>> Logout()
    {
        return Ok(ApiResponse<string>.Ok("Logged out", "Logout successful. Remove token on client side."));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Me()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized(ApiResponse<AuthResponseDto>.Fail("Invalid token."));
        }

        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
        {
            return NotFound(ApiResponse<AuthResponseDto>.Fail("User not found."));
        }

        var role = User.FindFirstValue(ClaimTypes.Role) ?? _userRoleService.GetRoleForUser(user);
        var response = new AuthResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = role,
            Token = string.Empty
        };

        return Ok(ApiResponse<AuthResponseDto>.Ok(response));
    }
}
