using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Models.Entities;

namespace backend.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByEmployeeCodeAsync(string code);
        Task<List<User>> GetAllAsync();
        Task<User> AddAsync(User user);
        Task UpdateAsync(User user);
    }
}
