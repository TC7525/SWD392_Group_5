using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Models.Entities;

namespace backend.Repositories
{
    public interface ISupplierRepository
    {
        Task<Supplier?> GetByIdAsync(int id);
        Task<Supplier?> GetByCodeAsync(string code);
        Task<List<Supplier>> GetAllAsync();
        Task<List<Supplier>> GetActiveSuppliersAsync();
        Task<Supplier> AddAsync(Supplier supplier);
        Task UpdateAsync(Supplier supplier);
    }
}
