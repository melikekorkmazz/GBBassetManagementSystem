using GBBassetManagementSystem.Entity.Entities;

namespace GBBassetManagementSystem.Service.Interfaces;

public interface IDepartmentService
{
    Task<List<Department>> GetAllAsync();

    Task<Department?> GetByIdAsync(Guid id);

    Task AddAsync(Department department);

    Task UpdateAsync(Department department);

    Task DeleteAsync(Guid id);
}