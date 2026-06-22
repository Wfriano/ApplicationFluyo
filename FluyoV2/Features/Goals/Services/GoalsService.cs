using FluyoV2.Features.Goals.Dtos;
using FluyoV2.Features.Goals.Models;
using FluyoV2.Features.Goals.Repositories;
using Microsoft.Extensions.Logging;

namespace FluyoV2.Features.Goals.Services;

public class GoalsService
{
    private readonly GoalsRepository _repository;
    private readonly ILogger<GoalsService> _logger;

    public GoalsService(
        GoalsRepository repository,
        ILogger<GoalsService> logger)
    {
        _repository = repository;
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
            Description = request.Description,
            TargetAmount = request.TargetAmount,
            TargetDate = request.TargetDate,
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

    public async Task<List<GoalResponse>> GetAllAsync(
        string userId)
    {
        var goals = await _repository.GetByUserAsync(userId);

        _logger.LogInformation(
            "Metas consultadas. UserId: {UserId}, Total: {Total}",
            userId,
            goals.Count);

        return goals.Select(Map).ToList();
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

        return Map(goal);
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
        goal.Description = request.Description;
        goal.TargetAmount = request.TargetAmount;
        goal.TargetDate = request.TargetDate;

        await _repository.UpdateAsync(goal);

        _logger.LogInformation(
            "Meta actualizada. UserId: {UserId}, GoalId: {GoalId}",
            userId,
            id);

        return Map(goal);
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

    public async Task<GoalResponse?> DepositAsync(
        string id,
        string userId,
        decimal amount)
    {
        var goal = await _repository.GetByIdAsync(id);

        if (goal is null || goal.UserId != userId)
        {
            _logger.LogWarning(
                "Intento de abono a meta inexistente. UserId: {UserId}, GoalId: {GoalId}",
                userId,
                id);

            return null;
        }

        goal.CurrentAmount += amount;

        if (goal.CurrentAmount >= goal.TargetAmount)
        {
            goal.CurrentAmount = goal.TargetAmount;
            goal.IsCompleted = true;
        }

        await _repository.UpdateAsync(goal);

        _logger.LogInformation(
            "Abono registrado. UserId: {UserId}, GoalId: {GoalId}, Amount: {Amount}",
            userId,
            id,
            amount);

        return Map(goal);
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

        return Map(goal);
    }

    private static GoalResponse Map(Goal goal)
    {
        return new GoalResponse
        {
            Id = goal.Id,
            Name = goal.Name,
            Description = goal.Description,
            TargetAmount = goal.TargetAmount,
            CurrentAmount = goal.CurrentAmount,
            TargetDate = goal.TargetDate,
            IsCompleted = goal.IsCompleted,
            ProgressPercentage =
                goal.TargetAmount == 0
                    ? 0
                    : Math.Round(
                        (goal.CurrentAmount / goal.TargetAmount) * 100,
                        2)
        };
    }
}