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

    public async Task<AssetAssignment?> GetByIdAsync(Guid id)
    {
        return await _context.AssetAssignments
            .Include(a => a.Asset)
            .Include(a => a.Personnel)
                .ThenInclude(p => p!.Department)
            .Include(a => a.Room)
                .ThenInclude(r => r!.Department)
            .FirstOrDefaultAsync(a => a.Id == id);
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

        asset.Status = AssetStatus.Assigned;

        _context.AssetAssignments.Add(assignment);

        await _context.SaveChangesAsync();
    }
}