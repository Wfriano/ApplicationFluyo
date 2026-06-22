using FluentValidation;
using FluyoV2.Controllers.Base;
using FluyoV2.Features.Goals.Dtos;
using FluyoV2.Features.Goals.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FluyoV2.Controllers;

[Route("api/goals")]
[Authorize]
public class GoalsController : BaseController
{
    private readonly GoalsService _service;
    private readonly IValidator<CreateGoalRequest> _validator;

    public GoalsController(
        GoalsService service,
        IValidator<CreateGoalRequest> validator)
    {
        _service = service;
        _validator = validator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateGoalRequest request)
    {
        var validation = await _validator.ValidateAsync(request);

        if (!validation.IsValid)
            return Failure(validation.Errors.First().ErrorMessage);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var result = await _service.CreateAsync(userId, request);

        return Success(result, "Meta creada correctamente");
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var result = await _service.GetAllAsync(userId);

        return Success(result, "Metas consultadas correctamente");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var goal = await _service.GetByIdAsync(id, userId);

        if (goal is null)
            return NotFoundResponse("Meta no encontrada");

        return Success(goal, "Meta consultada correctamente");
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        string id,
        UpdateGoalRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var goal = await _service.UpdateAsync(id, userId, request);

        if (goal is null)
            return NotFoundResponse("Meta no encontrada");

        return Success(goal, "Meta actualizada correctamente");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var deleted = await _service.DeleteAsync(id, userId);

        if (!deleted)
            return NotFoundResponse("Meta no encontrada");

        return Success(true, "Meta eliminada correctamente");
    }

    [HttpPost("{id}/deposit")]
    public async Task<IActionResult> Deposit(
        string id,
        DepositGoalRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var goal = await _service.DepositAsync(id, userId, request.Amount);

        if (goal is null)
            return NotFoundResponse("Meta no encontrada");

        return Success(goal, "Abono registrado correctamente");
    }

    [HttpPost("{id}/complete")]
    public async Task<IActionResult> Complete(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var goal = await _service.CompleteAsync(id, userId);

        if (goal is null)
            return NotFoundResponse("Meta no encontrada");

        return Success(goal, "Meta completada correctamente");
    }
}