using GBBassetManagementSystem.Data.Context;
using GBBassetManagementSystem.Entity.Entities;
using GBBassetManagementSystem.Entity.Enums;
using GBBassetManagementSystem.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GBBassetManagementSystem.Service.Services;

public class AssetAssignmentService : IAssetAssignmentService
{
    private readonly ApplicationDbContext _context;

    public AssetAssignmentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AssetAssignment>> GetAllAsync()
    {
        return await _context.AssetAssignments
            .Include(a => a.Asset)
            .Include(a => a.Personnel)
                .ThenInclude(p => p!.Department)
            .Include(a => a.Room)
                .ThenInclude(r => r!.Department)
            .OrderByDescending(a => a.AssignmentDate)
            .ToListAsync();
    }

    public async Task<List<AssetAssignment>> GetByPersonnelIdAsync(
    Guid personnelId)
{
    return await _context.AssetAssignments
        .Where(assignment => assignment.PersonnelId == personnelId)
        .Include(assignment => assignment.Asset)
            .ThenInclude(asset => asset!.Category)
        .Include(assignment => assignment.Personnel)
            .ThenInclude(personnel => personnel!.Department)
        .OrderByDescending(assignment => assignment.AssignmentDate)
        .ToListAsync();
}
    public async Task<List<AssetAssignment>> GetByAssetIdAsync(Guid assetId)
    {
        return await _context.AssetAssignments
            .Where(a => a.AssetId == assetId)
            .Include(a => a.Asset)
            .Include(a => a.Personnel)
                .ThenInclude(p => p!.Department)
            .Include(a => a.Room)
                .ThenInclude(r => r!.Department)
            .OrderByDescending(a => a.AssignmentDate)
            .ToListAsync();
    }

    public async Task AssignAsync(AssetAssignment assignment)
    {
        var asset = await _context.Assets.FindAsync(assignment.AssetId);

        if (asset is null)
        {
            throw new KeyNotFoundException("Asset was not found.");
        }

        if (asset.Status != AssetStatus.Available)
        {
            throw new InvalidOperationException(
                "Only available assets can be assigned.");
        }

        switch (assignment.AssignmentType)
        {
            case AssignmentType.Personnel:
                if (assignment.PersonnelId is null)
                {
                    throw new InvalidOperationException(
                        "Please select personnel.");
                }

                assignment.RoomId = null;
                break;

            case AssignmentType.Room:
                if (assignment.RoomId is null)
                {
                    throw new InvalidOperationException(
                        "Please select a room.");
                }

                assignment.PersonnelId = null;
                break;

            default:
                throw new InvalidOperationException(
                    "Invalid assignment type.");
        }

        assignment.IsActive = true;
        assignment.ReturnDate = null;

        asset.Status = AssetStatus.Assigned;

        _context.AssetAssignments.Add(assignment);

        await _context.SaveChangesAsync();
    }

    public async Task ReturnAsync(
        Guid assignmentId,
        string receivedBy,
        string condition,
        string? damageDescription,
        string? notes)
    {
        var assignment = await _context.AssetAssignments
            .Include(a => a.Asset)
            .FirstOrDefaultAsync(a => a.Id == assignmentId);

        if (assignment is null)
        {
            throw new KeyNotFoundException(
                "Assignment was not found.");
        }

        if (!assignment.IsActive)
        {
            throw new InvalidOperationException(
                "This asset has already been returned.");
        }

        if (assignment.Asset is null)
        {
            throw new KeyNotFoundException(
                "Assigned asset was not found.");
        }

        assignment.IsActive = false;
        assignment.ReturnDate = DateTime.Today;

        assignment.Asset.Status =
            condition == "Broken"
                ? AssetStatus.Broken
                : AssetStatus.Available;

        var assetReturn = new AssetReturn
        {
            AssetAssignmentId = assignment.Id,
            ReturnDate = DateTime.Today,
            ReceivedBy = receivedBy,
            Condition = condition,
            DamageDescription = damageDescription,
            Notes = notes
        };

        _context.AssetReturns.Add(assetReturn);

        await _context.SaveChangesAsync();
    }
}