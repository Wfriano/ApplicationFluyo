using FluyoV2.Controllers.Base;
using FluyoV2.Users.Dtos;
using FluyoV2.Users.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FluyoV2.Controllers;

[Route("api/users")]
[Authorize]
public class UsersController : BaseController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("me")]
    public IActionResult Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var fullName = User.FindFirstValue(ClaimTypes.Name);
        var email = User.FindFirstValue(ClaimTypes.Email);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var result = new
        {
            UserId = userId,
            FullName = fullName,
            Email = email
        };

        return Success(result, "Usuario consultado correctamente"
        );
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        if (string.IsNullOrWhiteSpace(userId))
            return Failure("Usuario no autorizado");

        var result = await _userService.GetProfileAsync(userId);

        if (result is null)
            return Failure("Usuario no encontrado");

        return Success(
            result,
            "Perfil consultado correctamente"
        );
    }

    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        if (string.IsNullOrWhiteSpace(userId))
            return Failure("Usuario no autorizado");

        var result = await _userService.ChangePasswordAsync(
            userId,
            request
        );

        if (!result.Success)
            return Failure(result.Message);

        return Success(
            new
            {
                PasswordUpdated = true
            },
            result.Message
        );
    }
}