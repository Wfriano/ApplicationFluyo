using FluyoV2.Features.Accounts.Dtos;
using FluyoV2.Features.Accounts.Models;
using FluyoV2.Features.Accounts.Repositories;
using Microsoft.Extensions.Logging;

namespace FluyoV2.Features.Accounts.Services;

public class AccountsService
{
    private readonly AccountsRepository _repository;
    private readonly ILogger<AccountsService> _logger;

    public AccountsService(
        AccountsRepository repository,
        ILogger<AccountsService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<AccountResponse> CreateAsync(
        string userId,
        CreateAccountRequest request)
    {
        var account = new Account
        {
            UserId = userId,
            Name = request.Name,
            Type = request.Type,
            Balance = request.Balance,
            Currency = request.Currency
                ,
                IconColor = request.IconColor
        };

        await _repository.CreateAsync(account);

        _logger.LogInformation(
            "Cuenta creada. UserId: {UserId}, AccountId: {AccountId}, Name: {Name}",
            userId,
            account.Id,
            account.Name);

        return Map(account);
    }

    public async Task<List<AccountResponse>> GetByUserAsync(
        string userId)
    {
        var accounts = await _repository.GetByUserIdAsync(userId);

        _logger.LogInformation(
            "Cuentas consultadas. UserId: {UserId}, Total: {Total}",
            userId,
            accounts.Count);

        return accounts.Select(Map).ToList();
    }

    public async Task<AccountResponse?> GetByIdAsync(
        string id,
        string userId)
    {
        var account = await _repository.GetByIdAsync(id);

        if (account is null || account.UserId != userId)
        {
            _logger.LogWarning(
                "Cuenta no encontrada. UserId: {UserId}, AccountId: {AccountId}",
                userId,
                id);

            return null;
        }

        return Map(account);
    }

    public async Task<AccountResponse?> UpdateAsync(
        string id,
        string userId,
        UpdateAccountRequest request)
    {
        var account = await _repository.GetByIdAsync(id);

        if (account is null || account.UserId != userId)
        {
            _logger.LogWarning(
                "Intento de actualizar cuenta inexistente. UserId: {UserId}, AccountId: {AccountId}",
                userId,
                id);

            return null;
        }

        account.Name = request.Name;
        account.Type = request.Type;
        account.Balance = request.Balance;
        account.Currency = request.Currency;
        account.IconColor = request.IconColor;

        await _repository.UpdateAsync(account);

        _logger.LogInformation(
            "Cuenta actualizada. UserId: {UserId}, AccountId: {AccountId}",
            userId,
            account.Id);

        return Map(account);
    }

    public async Task<bool> DeleteAsync(
        string id,
        string userId)
    {
        var account = await _repository.GetByIdAsync(id);

        if (account is null || account.UserId != userId)
        {
            _logger.LogWarning(
                "Intento de eliminar cuenta inexistente. UserId: {UserId}, AccountId: {AccountId}",
                userId,
                id);

            return false;
        }

        await _repository.DeleteAsync(id);

        _logger.LogInformation(
            "Cuenta eliminada. UserId: {UserId}, AccountId: {AccountId}",
            userId,
            id);

        return true;
    }

    public async Task<BalanceSummaryResponse>
        GetBalanceSummaryAsync(string userId)
    {
        var accounts = await _repository.GetByUserIdAsync(userId);

        var result = new BalanceSummaryResponse
        {
            TotalAccounts = accounts.Count,
            TotalBalance = accounts.Sum(x => x.Balance)
        };

        _logger.LogInformation(
            "Resumen de saldos consultado. UserId: {UserId}, TotalAccounts: {TotalAccounts}, TotalBalance: {TotalBalance}",
            userId,
            result.TotalAccounts,
            result.TotalBalance);

        return result;
    }

    private static AccountResponse Map(Account account)
    {
        return new AccountResponse
        {
            Id = account.Id,
            Name = account.Name,
            Type = account.Type,
            Balance = account.Balance,
            Currency = account.Currency,
            IconColor = account.IconColor,
            IsArchived = account.IsArchived,
            CreatedAt = account.CreatedAt
        };
    }
}