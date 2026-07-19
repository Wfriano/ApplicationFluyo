using FluyoV2.Users.Dtos;

namespace FluyoV2.Users.Services;

public interface IUserService
{
    Task<UserResponse?> GetProfileAsync(string userId);

    Task<ChangePasswordResponse> ChangePasswordAsync(
        string userId,
        ChangePasswordRequest request
    );
}