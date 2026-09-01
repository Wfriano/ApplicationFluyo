using FluentValidation;
using FluyoV2.Controllers.Base;
using FluyoV2.Features.Commitments.Dtos;
using FluyoV2.Features.Commitments.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Security.Claims;

namespace FluyoV2.Controllers;

[Route("api/commitments")]
[Authorize]
public class CommitmentsController : BaseController
{
    private readonly CommitmentsService _service;
    private readonly IValidator<CreateCommitmentRequest> _createValidator;
    private readonly IValidator<UpdateCommitmentRequest> _updateValidator;
    private readonly ILogger<CommitmentsController> _logger;

    public CommitmentsController(
        CommitmentsService service,
        IValidator<CreateCommitmentRequest> createValidator,
        IValidator<UpdateCommitmentRequest> updateValidator,
        ILogger<CommitmentsController> logger)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] JsonElement body)
    {
        CreateCommitmentRequest? request = null;

        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            request = JsonSerializer.Deserialize<CreateCommitmentRequest?>(body.GetRawText(), options);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Invalid JSON in CreateCommitmentRequest");
            return Failure("JSON inválido en la solicitud de creación de compromiso");
        }

        if (request is null)
            return Failure("El request es obligatorio");

        var validation = await _createValidator.ValidateAsync(request);

        if (!validation.IsValid)
            return Failure(validation.Errors.First().ErrorMessage);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        try
        {
            var result = await _service.CreateAsync(userId, request);
            return Success(result, "Compromiso creado correctamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating commitment");
            return Failure("Error interno al crear compromiso");
        }
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

    [HttpGet("balance")]
    public async Task<IActionResult> GetPendingBalance()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var total = await _service.GetPendingTotalAsync(userId);

        return Success(total, "Balance pendiente consultado correctamente");
    }

    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcoming(int? month = null, int? year = null)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var result = await _service.GetUpcomingAsync(userId, month, year);

        return Success(result, "Próximos compromisos consultados correctamente");
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
        [FromBody] JsonElement body)
    {
        UpdateCommitmentRequest? request = null;

        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            request = JsonSerializer.Deserialize<UpdateCommitmentRequest?>(body.GetRawText(), options);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Invalid JSON in UpdateCommitmentRequest");
            return Failure("JSON inválido en la solicitud de actualización de compromiso");
        }

        if (request is null)
            return Failure("El request es obligatorio");

        var validation = await _updateValidator.ValidateAsync(request);

        if (!validation.IsValid)
            return Failure(validation.Errors.First().ErrorMessage);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        try
        {
            var result = await _service.UpdateAsync(id, userId, request);

            if (result is null)
                return NotFoundResponse("Compromiso no encontrado");

            return Success(result, "Compromiso actualizado correctamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating commitment");
            return Failure("Error interno al actualizar compromiso");
        }
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

    [HttpDelete("{id}/series")]
    public async Task<IActionResult> DeleteSeries(
        string id)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var deleted = await _service.DeleteRecurringSeriesAsync(
            id,
            userId);

        if (!deleted)
            return NotFoundResponse(
                "Compromiso no encontrado");

        return Success(
            true,
            "Serie de compromisos recurrentes eliminada correctamente");
    }

    [HttpPost("{id}/pay")]
    public async Task<IActionResult> Pay(
        string id,
        [FromBody] FluyoV2.Features.Commitments.Dtos.PayCommitmentRequest? request)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        if (request is null || string.IsNullOrEmpty(request.AccountId))
            return Failure("La cuenta es obligatoria para marcar como pagado");

        var result = await _service.PayCommitmentAsync(
            id,
            userId,
            request);

        if (result is null)
            return NotFoundResponse(
                "Compromiso no encontrado");

        return Success(
            result,
            "Compromiso pagado correctamente");
    }
}