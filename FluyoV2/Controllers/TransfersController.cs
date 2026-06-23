using FluyoV2.Controllers.Base;
using FluyoV2.Features.Transfers.Dtos;
using FluyoV2.Features.Transfers.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FluyoV2.Controllers;

[Route("api/transfers")]
[Authorize]
public class TransfersController : BaseController
{
    private readonly TransfersService _service;

    public TransfersController(
        TransfersService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateTransferRequest request)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var result = await _service.CreateAsync(
            userId,
            request);

        return Success(
            result,
            "Transferencia realizada correctamente");
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var result = await _service.GetAllAsync(
            userId);

        return Success(
            result,
            "Transferencias consultadas correctamente");
    }
}