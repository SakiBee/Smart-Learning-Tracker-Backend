using Microsoft.EntityFrameworkCore;
using SLT.Core.Entities;
using SLT.Core.Interfaces;
using SLT.Infrastructure.Data;
using System.Linq.Expressions;

namespace SLT.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id) =>
        await _context.Users.FindAsync(id);

    public async Task<IEnumerable<User>> GetAllAsync() =>
        await _context.Users.ToListAsync();

    public async Task<IEnumerable<User>> FindAsync(Expression<Func<User, bool>> predicate) =>
        await _context.Users.Where(predicate).ToListAsync();

    public async Task<User?> GetByEmailAsync(string email) =>
        await _context.Users.FirstOrDefaultAsync(u => u.Email == email.ToLower());

    public async Task<bool> EmailExistsAsync(string email) =>
        await _context.Users.AnyAsync(u => u.Email == email.ToLower());

    public async Task AddAsync(User entity) =>
        await _context.Users.AddAsync(entity);

    public void Update(User entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Users.Update(entity);
    }

    public void Remove(User entity) =>
        _context.Users.Remove(entity);

    public async Task<int> SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}