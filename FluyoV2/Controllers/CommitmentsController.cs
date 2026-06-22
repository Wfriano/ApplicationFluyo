using FluyoV2.Controllers.Base;
using FluyoV2.Features.Commitments.Dtos;
using FluyoV2.Features.Commitments.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FluyoV2.Controllers;

[Route("api/commitments")]
[Authorize]
public class CommitmentsController : BaseController
{
    private readonly CommitmentsService _service;

    public CommitmentsController(
        CommitmentsService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateCommitmentRequest request)
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
            "Compromiso creado correctamente");
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
            "Compromisos consultados correctamente");
    }
} 