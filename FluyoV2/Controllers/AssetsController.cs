using FluentValidation;
using FluyoV2.Controllers.Base;
using FluyoV2.Features.Assets.Dtos;
using FluyoV2.Features.Assets.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FluyoV2.Controllers;

[Route("api/assets")]
[Authorize]
public class AssetsController : BaseController
{
    private readonly AssetsService _service;
    private readonly IValidator<CreateAssetRequest> _createValidator;
    private readonly IValidator<UpdateAssetRequest> _updateValidator;

    public AssetsController(
        AssetsService service,
        IValidator<CreateAssetRequest> createValidator,
        IValidator<UpdateAssetRequest> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateAssetRequest request)
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
            "Bien creado correctamente");
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
            "Bienes consultados correctamente");
    }

    [HttpGet("total")]
    public async Task<IActionResult> GetTotalValue()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var total = await _service.GetTotalValueAsync(userId);

        return Success(total, "Valor total consultado correctamente");
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
                "Bien no encontrado");

        return Success(
            result,
            "Bien consultado correctamente");
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        string id,
        UpdateAssetRequest request)
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
                "Bien no encontrado");

        return Success(
            result,
            "Bien actualizado correctamente");
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
                "Bien no encontrado");

        return Success(
            true,
            "Bien eliminado correctamente");
    }
}
