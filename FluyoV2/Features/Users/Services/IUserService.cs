using FluyoV2.Users.Dtos;

namespace FluyoV2.Users.Services;

public interface IUserService
{
    Task<UserResponse?> GetProfileAsync(string userId);

    Task<UserResponse?> UpdateProfileAsync(
        string userId,
        UpdateUserRequest request
    );

    Task<ChangePasswordResponse> ChangePasswordAsync(
        string userId,
        ChangePasswordRequest request
    );
}