using FluentValidation;
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
    private readonly IValidator<CreateCommitmentRequest> _createValidator;
    private readonly IValidator<UpdateCommitmentRequest> _updateValidator;

    public CommitmentsController(
        CommitmentsService service,
        IValidator<CreateCommitmentRequest> createValidator,
        IValidator<UpdateCommitmentRequest> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateCommitmentRequest request)
    {
        var validation = await _createValidator.ValidateAsync(request);

        if (!validation.IsValid)
            return Failure(
                validation.Errors.First().ErrorMessage);

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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(
        string id)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var result = await _service.GetByIdAsync(
            id,
            userId);

        if (result is null)
            return NotFoundResponse(
                "Compromiso no encontrado");

        return Success(
            result,
            "Compromiso consultado correctamente");
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        string id,
        UpdateCommitmentRequest request)
    {
        var validation = await _updateValidator.ValidateAsync(request);

        if (!validation.IsValid)
            return Failure(
                validation.Errors.First().ErrorMessage);

        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var result = await _service.UpdateAsync(
            id,
            userId,
            request);

        if (result is null)
            return NotFoundResponse(
                "Compromiso no encontrado");

        return Success(
            result,
            "Compromiso actualizado correctamente");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        string id)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var deleted = await _service.DeleteAsync(
            id,
            userId);

        if (!deleted)
            return NotFoundResponse(
                "Compromiso no encontrado");

        return Success(
            true,
            "Compromiso eliminado correctamente");
    }

    [HttpPost("{id}/pay")]
    public async Task<IActionResult> Pay(
        string id)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var result = await _service.PayCommitmentAsync(
            id,
            userId);

        if (result is null)
            return NotFoundResponse(
                "Compromiso no encontrado");

        return Success(
            result,
            "Compromiso pagado correctamente");
    }
}