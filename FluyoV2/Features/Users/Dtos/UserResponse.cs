namespace FluyoV2.Users.Dtos;

public class UserResponse
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string PhotoUser { get; set; } = string.Empty;
}