namespace FluyoV2.Features.Goals.Dtos;

public class CreateGoalRequest
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal TargetAmount { get; set; }

    public DateTime? TargetDate { get; set; }
}