using EventOrganizer.Server.Models;

namespace EventOrganizer.Server.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task CreateAsync(User user);
    Task<User?> FindByEmailVerificationTokenAsync(string token);
    Task<User?> FindByResetTokenAsync(string token);
    Task UpdateAsync(User user);
    Task<User?> FindByPasswordResetTokenAsync(string token);
}