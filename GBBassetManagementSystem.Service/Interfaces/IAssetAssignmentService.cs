using GBBassetManagementSystem.Entity.Entities;

namespace GBBassetManagementSystem.Service.Interfaces;

public interface IAssetAssignmentService
{
    Task<List<AssetAssignment>> GetAllAsync();

    Task<AssetAssignment?> GetByIdAsync(Guid id);

    Task<List<AssetAssignment>> GetByAssetIdAsync(Guid assetId);

    Task<List<AssetAssignment>> GetByPersonnelIdAsync(Guid personnelId);

    Task AssignAsync(AssetAssignment assignment);

    Task ReturnAsync(
        Guid assignmentId,
        string receivedBy,
        string condition,
        string? damageDescription,
        string? notes);
}