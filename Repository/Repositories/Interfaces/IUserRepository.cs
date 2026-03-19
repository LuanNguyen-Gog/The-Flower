using Repository.Models;

namespace Repository.Repositories.Interfaces;

public interface IUserRepository
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int userId);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<List<User>> GetAllByRoleAsync(string role);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
}
