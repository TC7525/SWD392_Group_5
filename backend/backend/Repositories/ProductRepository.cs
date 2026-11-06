using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.Models.Entities;

namespace backend.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id);
        }

        public async Task<Product?> GetByCodeAsync(string code)
        {
            return await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductCode == code);
        }

        public async Task<List<Product>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .OrderBy(p => p.ProductName)
                .ToListAsync();
        }

        public async Task<List<Product>> GetActiveProductsAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Status == "Active")
                .OrderBy(p => p.ProductName)
                .ToListAsync();
        }

        public async Task<List<Product>> GetByIdsAsync(IEnumerable<int> ids)
        {
            var idList = ids.ToList();
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => idList.Contains(p.ProductId))
                .ToListAsync();
        }

        public async Task<Product> AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }
    }
}
