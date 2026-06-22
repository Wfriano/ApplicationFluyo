namespace FluyoV2.Features.Accounts.Dtos;

public class AccountResponse
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public decimal Balance { get; set; }

    public string Currency { get; set; } = "COP";

    public bool IsArchived { get; set; }

    public DateTime CreatedAt { get; set; }
}
