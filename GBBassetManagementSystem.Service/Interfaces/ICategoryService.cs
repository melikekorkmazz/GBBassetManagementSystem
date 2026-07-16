using System.Collections.Generic;
using GBBassetManagementSystem.Entity.Entities;

namespace GBBassetManagementSystem.Service.Interfaces;

public interface ICategoryService
{
    Task<List<Category>> GetAllAsync();

    Task<Category?> GetByIdAsync(Guid id);

    Task AddAsync(Category category);

    Task UpdateAsync(Category category);

    Task DeleteAsync(Guid id);
}