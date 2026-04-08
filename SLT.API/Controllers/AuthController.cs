using Microsoft.AspNetCore.Mvc;
using SLT.Application.DTOs;
using SLT.Core.Entities;
using SLT.Core.Interfaces;
using SLT.Core.Specifications;

namespace SLT.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IRepository<User> _userRepo;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IConfiguration _configuration;

    public AuthController(
        IRepository<User> userRepo,
        IJwtTokenService jwtTokenService,
        IConfiguration configuration)
    {
        _userRepo = userRepo;
        _jwtTokenService = jwtTokenService;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Password) ||
            string.IsNullOrWhiteSpace(dto.FullName))
            return BadRequest(new { message = "All fields are required." });

        var emailExists = await _userRepo.AnyAsync(
            new UserEmailExistsSpec(dto.Email));

        if (emailExists)
            return Conflict(new { message = "An account with this email already exists." });

        var user = new User
        {
            FullName = dto.FullName.Trim(),
            Email = dto.Email.ToLower().Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        await _userRepo.AddAsync(user);
        await _userRepo.SaveChangesAsync();

        var token = _jwtTokenService.GenerateToken(user);

        return CreatedAtAction(nameof(Register), new AuthResponseDto
        {
            Token = token,
            Email = user.Email,
            FullName = user.FullName,
            UserId = user.Id,
            ExpiresAt = _jwtTokenService.GetTokenExpiry()
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new { message = "Email and password are required." });

        var user = await _userRepo.GetEntityWithSpec(
            new UserByEmailSpec(dto.Email));

        if (user == null)
            return Unauthorized(new { message = "Invalid email or password." });

        var isPasswordValid = BCrypt.Net.BCrypt
            .Verify(dto.Password, user.PasswordHash);

        if (!isPasswordValid)
            return Unauthorized(new { message = "Invalid email or password." });

        var token = _jwtTokenService.GenerateToken(user);

        return Ok(new AuthResponseDto
        {
            Token = token,
            Email = user.Email,
            FullName = user.FullName,
            UserId = user.Id,
            ExpiresAt = _jwtTokenService.GetTokenExpiry()
        });
    }

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleAuthDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.IdToken))
            return BadRequest(new { message = "Google token is required." });

        try
        {
            var clientId = _configuration["Google:ClientId"]!;
            var settings = new Google.Apis.Auth.GoogleJsonWebSignature
                .ValidationSettings { Audience = new[] { clientId } };

            var payload = await Google.Apis.Auth.GoogleJsonWebSignature
                .ValidateAsync(dto.IdToken, settings);

            var user = await _userRepo.GetEntityWithSpec(
                new UserByEmailSpec(payload.Email));

            if (user == null)
            {
                user = new User
                {
                    FullName = payload.Name ?? payload.Email.Split('@')[0],
                    Email = payload.Email.ToLower().Trim(),
                    PasswordHash = BCrypt.Net.BCrypt
                        .HashPassword(Guid.NewGuid().ToString())
                };
                await _userRepo.AddAsync(user);
                await _userRepo.SaveChangesAsync();
            }

            var token = _jwtTokenService.GenerateToken(user);

            return Ok(new AuthResponseDto
            {
                Token = token,
                Email = user.Email,
                FullName = user.FullName,
                UserId = user.Id,
                ExpiresAt = _jwtTokenService.GetTokenExpiry()
            });
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = "Invalid Google token.", detail = ex.Message });
        }
    }
}