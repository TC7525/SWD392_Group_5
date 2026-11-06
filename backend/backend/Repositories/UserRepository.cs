using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.Models.Entities;

namespace backend.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<User> QueryWithIncludes()
        {
            return _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role);
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await QueryWithIncludes()
                .FirstOrDefaultAsync(u => u.UserId == id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            var normalizedEmail = email.Trim().ToLower();
            return await QueryWithIncludes()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
        }

        public async Task<User?> GetByEmployeeCodeAsync(string code)
        {
            return await QueryWithIncludes()
                .FirstOrDefaultAsync(u => u.EmployeeCode == code);
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await QueryWithIncludes()
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }

        public async Task<User> AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
