using FluyoV2.Controllers.Base;
using FluyoV2.Features.Notifications.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FluyoV2.Controllers;

[Route("api/notifications")]
[Authorize]
public class NotificationsController : BaseController
{
    private readonly NotificationsService _service;

    public NotificationsController(NotificationsService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var result = await _service.GetAllAsync(userId);

        return Success(result, "Notificaciones consultadas correctamente");
    }

    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkAsRead(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var ok = await _service.MarkAsReadAsync(userId, id);

        if (!ok)
            return NotFoundResponse("Notificación no encontrada");

        return Success(true, "Notificación marcada como leída");
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        await _service.MarkAllAsReadAsync(userId);

        return Success(true, "Notificaciones marcadas como leídas");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var ok = await _service.DeleteAsync(userId, id);

        if (!ok)
            return NotFoundResponse("Notificación no encontrada");

        return Success(true, "Notificación eliminada");
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteAll()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        await _service.DeleteAllAsync(userId);

        return Success(true, "Notificaciones eliminadas");
    }
}
