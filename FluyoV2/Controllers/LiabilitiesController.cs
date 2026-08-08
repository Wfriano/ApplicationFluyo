using FluentValidation;
using FluyoV2.Controllers.Base;
using FluyoV2.Features.Liabilities.Dtos;
using FluyoV2.Features.Liabilities.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FluyoV2.Controllers;

[Route("api/liabilities")]
[Authorize]
public class LiabilitiesController : BaseController
{
    private readonly LiabilitiesService _service;
    private readonly IValidator<CreateLiabilityRequest> _createValidator;
    private readonly IValidator<UpdateLiabilityRequest> _updateValidator;

    public LiabilitiesController(
        LiabilitiesService service,
        IValidator<CreateLiabilityRequest> createValidator,
        IValidator<UpdateLiabilityRequest> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateLiabilityRequest request)
    {
        var validation = await _createValidator.ValidateAsync(request);

        if (!validation.IsValid)
            return Failure(validation.Errors.First().ErrorMessage);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var result = await _service.CreateAsync(userId, request);

        return Success(result, "Deuda creada correctamente");
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var result = await _service.GetAllAsync(userId);

        return Success(result, "Deudas consultadas correctamente");
    }

    [HttpGet("total")]
    public async Task<IActionResult> GetTotalAmount()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var total = await _service.GetTotalAmountAsync(userId);

        return Success(total, "Monto total consultado correctamente");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var result = await _service.GetByIdAsync(id, userId);

        if (result is null)
            return NotFoundResponse("Deuda no encontrada");

        return Success(result, "Deuda consultada correctamente");
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        string id,
        UpdateLiabilityRequest request)
    {
        var validation = await _updateValidator.ValidateAsync(request);

        if (!validation.IsValid)
            return Failure(validation.Errors.First().ErrorMessage);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var result = await _service.UpdateAsync(id, userId, request);

        if (result is null)
            return NotFoundResponse("Deuda no encontrada");

        return Success(result, "Deuda actualizada correctamente");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var deleted = await _service.DeleteAsync(id, userId);

        if (!deleted)
            return NotFoundResponse("Deuda no encontrada");

        return Success(true, "Deuda eliminada correctamente");
    }
}
