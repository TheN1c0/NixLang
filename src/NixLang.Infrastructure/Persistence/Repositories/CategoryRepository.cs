using Microsoft.EntityFrameworkCore;
using NixLang.Domain.Entities;
using NixLang.Domain.Repositories;

namespace NixLang.Infrastructure.Persistence.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly NixLangDbContext _dbContext;

    public CategoryRepository(NixLangDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var nameLower = name.Trim().ToLower();
        return await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.Name.ToLower() == nameLower, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var nameLower = name.Trim().ToLower();
        return await _dbContext.Categories
            .AnyAsync(c => c.Name.ToLower() == nameLower, cancellationToken);
    }

    public async Task AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        await _dbContext.Categories.AddAsync(category, cancellationToken);
    }

    public async Task DeleteAsync(Category category, CancellationToken cancellationToken = default)
    {
        _dbContext.Categories.Remove(category);
        await Task.CompletedTask;
    }
}
