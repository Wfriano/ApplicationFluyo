using FluentValidation;
using FluyoV2.Controllers.Base;
using FluyoV2.Features.Transactions.Dtos;
using FluyoV2.Features.Transactions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FluyoV2.Controllers;

[Route("api/transactions")]
[Authorize]
public class TransactionsController : BaseController
{
    private readonly TransactionsService _service;
    private readonly IValidator<CreateTransactionRequest> _validator;
    private readonly IValidator<CreateTransactionWithRecurrenceRequest> _withRecurrenceValidator;

    public TransactionsController(
        TransactionsService service,
        IValidator<CreateTransactionRequest> validator,
        IValidator<CreateTransactionWithRecurrenceRequest> withRecurrenceValidator)
    {
        _service = service;
        _validator = validator;
        _withRecurrenceValidator = withRecurrenceValidator;
    }

    [HttpPost("income")]
    public async Task<IActionResult> Income(CreateTransactionRequest request)
    {
        var validation = await _validator.ValidateAsync(request);

        if (!validation.IsValid)
            return Failure(validation.Errors.First().ErrorMessage);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var result = await _service.CreateIncomeAsync(userId, request);

        return Success(result, "Ingreso registrado correctamente");
    }

    [HttpPost("expense")]
    public async Task<IActionResult> Expense(CreateTransactionRequest request)
    {
        var validation = await _validator.ValidateAsync(request);

        if (!validation.IsValid)
            return Failure(validation.Errors.First().ErrorMessage);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var result = await _service.CreateExpenseAsync(userId, request);

        return Success(result, "Gasto registrado correctamente");
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var result = await _service.GetAllAsync(userId);

        return Success(result, "Movimientos consultados correctamente");
    }

    [HttpPost("with-recurrence")]
    public async Task<IActionResult> CreateWithRecurrence(CreateTransactionWithRecurrenceRequest request)
    {
        // basic validation for required fields
        if (request is null)
            return Failure("La información enviada no es válida");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var result = await _service.CreateWithOptionalRecurrenceAsync(userId, request);

        return Success(result, "Movimiento registrado correctamente");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var result = await _service.GetByIdAsync(userId, id);

        if (result is null)
            return NotFoundResponse("Movimiento no encontrado");

        return Success(result, "Movimiento consultado");
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, TransactionResponse request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        try
        {
            await _service.UpdateAsync(userId, request);
            return Success<object>(null, "Movimiento actualizado");
        }
        catch (ArgumentException ex)
        {
            return Failure(ex.Message);
        }
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
            return Success<object>(null, "Movimiento eliminado");
        }
        catch (ArgumentException ex)
        {
            return Failure(ex.Message);
        }
    }
}