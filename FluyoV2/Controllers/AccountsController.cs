using FluentValidation;
using FluyoV2.Controllers.Base;
using FluyoV2.Features.Accounts.Dtos;
using FluyoV2.Features.Accounts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FluyoV2.Controllers;

[Route("api/accounts")]
[Authorize]
public class AccountsController : BaseController
{
    private readonly AccountsService _accountsService;
    private readonly IValidator<CreateAccountRequest> _validator;

    public AccountsController(
        AccountsService accountsService,
        IValidator<CreateAccountRequest> validator)
    {
        _accountsService = accountsService;
        _validator = validator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateAccountRequest request)
    {
        var validation = await _validator.ValidateAsync(request);

        if (!validation.IsValid)
            return Failure(validation.Errors.First().ErrorMessage);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var result = await _accountsService.CreateAsync(userId, request);

        return Success(result, "Cuenta creada correctamente");
    }

    [HttpGet]
    public async Task<IActionResult> GetMyAccounts()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var result = await _accountsService.GetByUserAsync(userId);

        return Success(result, "Cuentas consultadas correctamente");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var account = await _accountsService.GetByIdAsync(id, userId);

        if (account is null)
            return NotFoundResponse("Cuenta no encontrada");

        return Success(account, "Cuenta consultada correctamente");
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        string id,
        UpdateAccountRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var account = await _accountsService.UpdateAsync(id, userId, request);

        if (account is null)
            return NotFoundResponse("Cuenta no encontrada");

        return Success(account, "Cuenta actualizada correctamente");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        // First fetch the account to verify ownership and balance
        var account = await _accountsService.GetByIdAsync(id, userId);

        if (account is null)
            return NotFoundResponse("Cuenta no encontrada");

        // Do not allow deletion if the account has a non-zero balance
        if (account.Balance != 0)
            return Failure("No se puede eliminar una cuenta con saldo");

        var deleted = await _accountsService.DeleteAsync(id, userId);

        if (!deleted)
            return NotFoundResponse("Cuenta no encontrada");

        return Success(true, "Cuenta eliminada correctamente");
    }

    [HttpGet("balance-summary")]
    public async Task<IActionResult> BalanceSummary()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var result = await _accountsService.GetBalanceSummaryAsync(userId);

        return Success(result, "Resumen de saldos consultado correctamente");
    }
}