using ProductCatalog.Api.Models;

namespace ProductCatalog.Api.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);

    Task<User?> GetByEmailAsync(string email);

    Task AddAsync(User user);

    Task<IReadOnlyCollection<User>> GetAllAsync();
}
