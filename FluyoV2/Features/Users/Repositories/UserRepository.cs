using FluyoV2.Features.Auth.Models;
using FluyoV2.Infrastructure.Persistence;
using MongoDB.Driver;

namespace FluyoV2.Users.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _usersCollection;

    public UserRepository(MongoDbContext context)
    {
        _usersCollection = context.Users;
    }

    public async Task<User?> GetByIdAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        return await _usersCollection
            .Find(x => x.Id == userId)
            .FirstOrDefaultAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var normalizedEmail = email.Trim().ToLowerInvariant();

        return await _usersCollection
            .Find(x => x.Email == normalizedEmail)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> UpdatePasswordAsync(
        string userId,
        string newPasswordHash,
        DateTime passwordUpdatedAt)
    {
        var update = Builders<User>.Update
            .Set(x => x.PasswordHash, newPasswordHash)
            .Set(x => x.PasswordUpdatedAt, passwordUpdatedAt)
            .Set(x => x.RefreshToken, null);

        var result = await _usersCollection.UpdateOneAsync(
            x => x.Id == userId,
            update
        );

        return result.ModifiedCount > 0;
    }
    public async Task<bool> UpdateProfileAsync(
    string userId,
    string fullName,
    string email,
    string? phoneNumber,
    DateTime? dateOfBirth,
    string photoUser)
    {
        var update = Builders<User>.Update
            .Set(x => x.FullName, fullName)
            .Set(x => x.Email, email)
            .Set(x => x.PhoneNumber, phoneNumber)
            .Set(x => x.DateOfBirth, dateOfBirth)
            .Set(x => x.PhotoUser, photoUser);

        var result = await _usersCollection.UpdateOneAsync(
            x => x.Id == userId,
            update
        );

        return result.MatchedCount > 0;
    }
}