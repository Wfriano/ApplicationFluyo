using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FluyoV2.Features.Goals.Dtos;

public class UpdateGoalRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public string? ImageUrl { get; set; } = string.Empty;
    public DateTime? TargetDate { get; set; }
}