namespace FluyoV2.Features.Accounts.Dtos;

public class CreateAccountRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    // Optional color for the account icon (hex code or color name)
    public string IconColor { get; set; } = string.Empty;
}