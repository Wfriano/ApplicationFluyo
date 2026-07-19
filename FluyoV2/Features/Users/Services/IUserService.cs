using FluyoV2.Users.Dtos;

namespace FluyoV2.Users.Services;

public interface IUserService
{
    Task<ChangePasswordResponse> ChangePasswordAsync(
        string userId,
        ChangePasswordRequest request
    );
}