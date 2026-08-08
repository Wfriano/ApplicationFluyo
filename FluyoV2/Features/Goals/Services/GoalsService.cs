using FluyoV2.Features.Accounts.Repositories;
using FluyoV2.Features.Commitments.Repositories;
using FluyoV2.Features.Goals.Dtos;
using FluyoV2.Features.Goals.Interfaces;
using FluyoV2.Features.Goals.Models;
using FluyoV2.Features.Goals.Repositories;

namespace FluyoV2.Features.Goals.Services;

public class GoalsService : IGoalsService
{
    private readonly GoalsRepository _repository;
    private readonly AccountsRepository _accountsRepository;
    private readonly CommitmentsRepository _commitmentsRepository;
    private readonly ILogger<GoalsService> _logger;

    public GoalsService(
        GoalsRepository repository,
        AccountsRepository accountsRepository,
        CommitmentsRepository commitmentsRepository,
        ILogger<GoalsService> logger)
    {
        _repository = repository;
        _accountsRepository = accountsRepository;
        _commitmentsRepository = commitmentsRepository;
        _logger = logger;
    }

    public async Task<GoalResponse> CreateAsync(
        string userId,
        CreateGoalRequest request)
    {
        var goal = new Goal
        {
            UserId = userId,
            Name = request.Name,
            TargetAmount = request.TargetAmount,
            TargetDate = request.TargetDate,
            Image = request.ImageUrl ?? string.Empty,
            CurrentAmount = 0,
            IsCompleted = false
        };

        await _repository.CreateAsync(goal);

        _logger.LogInformation(
            "Meta creada. UserId: {UserId}, Goal: {GoalName}, TargetAmount: {TargetAmount}",
            userId,
            goal.Name,
            goal.TargetAmount);

        return Map(goal);
    }

    public async Task<List<GoalResponse>> GetActiveStatusAsync(
        string userId,
        bool isActive)
    {
        var goals = await _repository.GetByUserAsync(userId);

        // Disponible = saldo total - compromisos pendientes
        var availableBalance = await GetAvailableBalanceAsync(userId);

        if (isActive)
            goals = goals.Where(g => !g.IsCompleted)?.ToList();
        else
            goals = goals.Where(g => g.IsCompleted)?.ToList();

        _logger.LogInformation(
            "Metas consultadas. UserId: {UserId}, Total: {Total}",
            userId,
            goals.Count);

        return goals.Select(g =>
        {
            var resp = Map(g);
            resp.CurrentAmount = availableBalance;
            resp.RemainingAmount = Math.Max(0, resp.TargetAmount - availableBalance);
            resp.ProgressPercentage =
                resp.TargetAmount == 0
                    ? 0
                    : Math.Round((availableBalance / resp.TargetAmount) * 100, 2);
            return resp;
        }).ToList();
    }

    public async Task<GoalResponse?> GetByIdAsync(
        string id,
        string userId)
    {
        var goal = await _repository.GetByIdAsync(id);

        if (goal is null || goal.UserId != userId)
        {
            _logger.LogWarning(
                "Meta no encontrada. UserId: {UserId}, GoalId: {GoalId}",
                userId,
                id);

            return null;
        }

        var resp = Map(goal);
        resp.CurrentAmount = await GetAvailableBalanceAsync(userId);
        resp.RemainingAmount = Math.Max(0, resp.TargetAmount - resp.CurrentAmount);
        resp.ProgressPercentage =
            resp.TargetAmount == 0
                ? 0
                : Math.Round((resp.CurrentAmount / resp.TargetAmount) * 100, 2);

        return resp;
    }

    public async Task<GoalResponse?> UpdateAsync(
        string id,
        string userId,
        UpdateGoalRequest request)
    {
        var goal = await _repository.GetByIdAsync(id);

        if (goal is null || goal.UserId != userId)
        {
            _logger.LogWarning(
                "Intento de actualizar meta inexistente. UserId: {UserId}, GoalId: {GoalId}",
                userId,
                id);

            return null;
        }

        goal.Name = request.Name;
        goal.TargetAmount = request.TargetAmount;
        goal.TargetDate = request.TargetDate;
        goal.Image = request.ImageUrl ?? string.Empty;

        await _repository.UpdateAsync(goal);

        _logger.LogInformation(
            "Meta actualizada. UserId: {UserId}, GoalId: {GoalId}",
            userId,
            id);

        var resp = Map(goal);
        resp.CurrentAmount = await GetAvailableBalanceAsync(userId);
        resp.RemainingAmount = Math.Max(0, resp.TargetAmount - resp.CurrentAmount);
        resp.ProgressPercentage =
            resp.TargetAmount == 0
                ? 0
                : Math.Round((resp.CurrentAmount / resp.TargetAmount) * 100, 2);

        return resp;
    }

    public async Task<bool> DeleteAsync(
        string id,
        string userId)
    {
        var goal = await _repository.GetByIdAsync(id);

        if (goal is null || goal.UserId != userId)
        {
            _logger.LogWarning(
                "Intento de eliminar meta inexistente. UserId: {UserId}, GoalId: {GoalId}",
                userId,
                id);

            return false;
        }

        await _repository.DeleteAsync(id);

        _logger.LogInformation(
            "Meta eliminada. UserId: {UserId}, GoalId: {GoalId}",
            userId,
            id);

        return true;
    }

    public async Task<GoalResponse?> CompleteAsync(
        string id,
        string userId)
    {
        var goal = await _repository.GetByIdAsync(id);

        if (goal is null || goal.UserId != userId)
        {
            _logger.LogWarning(
                "Intento de completar meta inexistente. UserId: {UserId}, GoalId: {GoalId}",
                userId,
                id);

            return null;
        }

        goal.CurrentAmount = goal.TargetAmount;
        goal.IsCompleted = true;

        await _repository.UpdateAsync(goal);

        _logger.LogInformation(
            "Meta completada. UserId: {UserId}, GoalId: {GoalId}",
            userId,
            id);

        var resp = Map(goal);
        resp.CurrentAmount = await GetAvailableBalanceAsync(userId);
        resp.RemainingAmount = Math.Max(0, resp.TargetAmount - resp.CurrentAmount);
        resp.ProgressPercentage =
            resp.TargetAmount == 0
                ? 0
                : Math.Round((resp.CurrentAmount / resp.TargetAmount) * 100, 2);

        return resp;
    }

    private async Task<decimal> GetAvailableBalanceAsync(string userId)
    {
        var totalBalance = await _accountsRepository.GetTotalBalanceAsync(userId);
        var commitments = await _commitmentsRepository.GetByUserAsync(userId);

        var pendingCommitments = commitments
            .Where(x => x.IsActive)
            .Sum(x => x.Amount);

        return totalBalance - pendingCommitments;
    }

    private static GoalResponse Map(Goal goal)
    {
        return new GoalResponse
        {
            Id = goal.Id,
            Name = goal.Name,
            TargetAmount = goal.TargetAmount,
            IsCompleted = goal.IsCompleted,
            CurrentAmount = goal.CurrentAmount,
            RemainingAmount = Math.Max(0, goal.TargetAmount - goal.CurrentAmount),
            TargetDate = goal.TargetDate,
            ProgressPercentage =
                goal.TargetAmount == 0
                    ? 0
                    : Math.Round(
                        (goal.CurrentAmount / goal.TargetAmount) * 100,
                        2)
        };
    }
}
