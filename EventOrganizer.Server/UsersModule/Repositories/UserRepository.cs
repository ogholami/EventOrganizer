using EventOrganizer.Server.Models;
using EventOrganizer.Server.Settings;
using EventOrganizer.Server.Tools.EventOrganizer.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EventOrganizer.Server.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _collection;
    private readonly ApplicationDbContext _dbContext;


    public UserRepository(IOptions<MongoDbSettings> settings, ApplicationDbContext dbContext)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var db = client.GetDatabase(settings.Value.DatabaseName);
        _collection = db.GetCollection<User>("Users");
        _dbContext = dbContext;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _collection.Find(u => u.Email == email).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(User user)
    {
        await _collection.InsertOneAsync(user);
    }

    public async Task<User?> FindByEmailVerificationTokenAsync(string token)
    {
        return await _collection.Find(u => u.EmailVerificationToken == token).FirstOrDefaultAsync();
    }

    public async Task<User?> FindByResetTokenAsync(string token)
    {
        return await _collection.Find(u => u.PasswordResetToken == token).FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(User user)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
        await _collection.ReplaceOneAsync(filter, user);
    }
    public async Task<User?> FindByPasswordResetTokenAsync(string token)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == token);
    }
}
