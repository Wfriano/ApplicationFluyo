namespace FluyoV2.Features.Goals.Dtos;

public class CreateGoalRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public DateTime? TargetDate { get; set; }
    public string? ImageUrl { get; set; } = string.Empty;
}