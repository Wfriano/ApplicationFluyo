using FluyoV2.Controllers.Base;
using FluyoV2.Features.Transactions.Dtos;
using FluyoV2.Features.Transactions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FluyoV2.Controllers;

[Route("api/recurrences")]
[Authorize]
public class RecurrencesController : BaseController
{
    private readonly RecurrencesService _service;

    public RecurrencesController(RecurrencesService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateRecurrenceRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        try
        {
            var result = await _service.CreateAsync(userId, request);
            return Success(result, "Recurrencia creada correctamente");
        }
        catch (ArgumentException ex)
        {
            return Failure(ex.Message);
        }
    }

    [HttpGet("transaction/{transactionId}")]
    public async Task<IActionResult> GetByTransaction(string transactionId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var result = await _service.GetByTransactionIdAsync(userId, transactionId);

        if (result is null)
            return NotFoundResponse("Recurrencia no encontrada");

        return Success(result, "Recurrencia consultada");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        // try by transaction id first
        var byTx = await _service.GetByTransactionIdAsync(userId, id);
        if (byTx is not null)
            return Success(byTx, "Recurrencia consultada");

        // else try fetch all and find by id
        var all = await _service.GetAllByUserAsync(userId);
        var item = all.FirstOrDefault(x => x.Id == id);

        if (item is null)
            return NotFoundResponse("Recurrencia no encontrada");

        return Success(item, "Recurrencia consultada");
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, RecurrenceResponse request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        try
        {
            await _service.UpdateAsync(userId, request);
            return Success<object>(null, "Recurrencia actualizada");
        }
        catch (ArgumentException ex)
        {
            return Failure(ex.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var result = await _service.GetAllByUserAsync(userId);
        return Success(result, "Recurrencias consultadas");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        try
        {
            await _service.DeleteAsync(userId, id);
            return Success<object>(null, "Recurrencia eliminada");
        }
        catch (ArgumentException ex)
        {
            return Failure(ex.Message);
        }
    }
}
