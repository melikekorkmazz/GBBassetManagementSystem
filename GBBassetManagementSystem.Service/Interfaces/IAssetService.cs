using GBBassetManagementSystem.Entity.Entities;

namespace GBBassetManagementSystem.Service.Interfaces;

public interface IAssetService
{
    Task<List<Asset>> GetAllAsync();

    Task<Asset?> GetByIdAsync(Guid id);

    Task AddAsync(Asset asset);

    Task UpdateAsync(Asset asset);

    Task DeleteAsync(Guid id);
}