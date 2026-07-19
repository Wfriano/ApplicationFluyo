using FluyoV2.Features.Goals.Dtos;

namespace FluyoV2.Features.Goals.Interfaces
{
    public interface IGoalsService
    {
        Task<GoalResponse> CreateAsync(string userId, CreateGoalRequest request);
        Task<List<GoalResponse>> GetActiveStatusAsync(string userId, bool isActive);
        Task<GoalResponse?> GetByIdAsync(string id, string userId);
        Task<GoalResponse?> UpdateAsync(string id, string userId, UpdateGoalRequest request);
        Task<bool> DeleteAsync(string id, string userId);
        Task<GoalResponse?> CompleteAsync(string id, string userId);
    }
}