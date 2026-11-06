using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Models.Entities;

namespace backend.Repositories
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(int id);
        Task<Product?> GetByCodeAsync(string code);
        Task<List<Product>> GetAllAsync();
        Task<List<Product>> GetActiveProductsAsync();
        Task<List<Product>> GetByIdsAsync(IEnumerable<int> ids);
        Task<Product> AddAsync(Product product);
        Task UpdateAsync(Product product);
    }
}
