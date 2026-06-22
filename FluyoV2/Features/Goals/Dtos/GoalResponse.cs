namespace FluyoV2.Features.Goals.Dtos;

public class GoalResponse
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal TargetAmount { get; set; }

    public decimal CurrentAmount { get; set; }

    public DateTime? TargetDate { get; set; }

    public bool IsCompleted { get; set; }

    public decimal ProgressPercentage { get; set; }
}