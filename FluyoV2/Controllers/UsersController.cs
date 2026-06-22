using FluyoV2.Controllers.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FluyoV2.Controllers;

[Route("api/users")]
[Authorize]
public class UsersController : BaseController
{
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

        return Success(result, "Usuario consultado correctamente");
    }
}