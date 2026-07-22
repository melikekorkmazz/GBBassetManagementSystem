using System.Collections.Generic;
using GBBassetManagementSystem.Data.Context;
using GBBassetManagementSystem.Entity.Entities;
using GBBassetManagementSystem.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GBBassetManagementSystem.Service.Services;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _context;

    public CategoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetAllAsync()
    {
        return await _context.Categories
            .OrderBy(category => category.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(Guid id)
    {
        return await _context.Categories.FindAsync(id);
    }

    public async Task AddAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Category category)
    {
        var existingCategory = await _context.Categories
            .FindAsync(category.Id);

        if (existingCategory is null)
        {
            throw new KeyNotFoundException("Category was not found.");
        }

        existingCategory.Name = category.Name;
        existingCategory.Description = category.Description;

        await _context.SaveChangesAsync();
    }

    
    public async Task DeleteAsync(Guid id)
{
    var category = await _context.Categories.FindAsync(id);

    if (category is null)
    {
        throw new KeyNotFoundException("Category was not found.");
    }

    var hasAssets = await _context.Assets
        .AnyAsync(asset => asset.CategoryId == id);

    if (hasAssets)
    {
        throw new InvalidOperationException(
            "This category cannot be deleted because it contains assets.");
    }

    _context.Categories.Remove(category);

    await _context.SaveChangesAsync();
}
}