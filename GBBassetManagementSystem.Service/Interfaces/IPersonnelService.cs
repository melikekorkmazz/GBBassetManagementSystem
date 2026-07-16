using GBBassetManagementSystem.Entity.Entities;

namespace GBBassetManagementSystem.Service.Interfaces;

public interface IPersonnelService
{
    Task<List<Personnel>> GetAllAsync();
    Task<Personnel?> GetByIdAsync(Guid id);
    Task AddAsync(Personnel personnel);
    Task UpdateAsync(Personnel personnel);
    Task DeleteAsync(Guid id);
}