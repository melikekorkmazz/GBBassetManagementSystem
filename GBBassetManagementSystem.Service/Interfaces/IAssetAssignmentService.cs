using GBBassetManagementSystem.Entity.Entities;

namespace GBBassetManagementSystem.Service.Interfaces;

public interface IAssetAssignmentService
{
    Task<List<AssetAssignment>> GetAllAsync();

    Task<AssetAssignment?> GetByIdAsync(Guid id);

    Task AssignAsync(AssetAssignment assignment);
}