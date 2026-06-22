using FluyoV2.Features.Auth.Models;
using FluyoV2.Infrastructure.Persistence;
using MongoDB.Driver;

namespace FluyoV2.Features.Auth.Repositories;

public class AuthRepository
{
    private readonly MongoDbContext _context;

    public AuthRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Find(x => x.Email == email.ToLower())
            .FirstOrDefaultAsync();
    }

    public async Task CreateAsync(User user)
    {
        await _context.Users.InsertOneAsync(user);
    }

    public async Task UpdateRefreshTokenAsync(string userId, string refreshToken)
    {
        var update = Builders<User>.Update
            .Set(x => x.RefreshToken, refreshToken);

        await _context.Users.UpdateOneAsync(
            x => x.Id == userId,
            update
        );
    }
}