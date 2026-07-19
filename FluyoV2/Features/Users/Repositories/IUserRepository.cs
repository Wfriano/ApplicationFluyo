using FluyoV2.Features.Auth.Models;

namespace FluyoV2.Users.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string userId);

    Task<User?> GetByEmailAsync(string email);

    Task<bool> UpdatePasswordAsync(
        string userId,
        string newPasswordHash,
        DateTime passwordUpdatedAt
    );
}