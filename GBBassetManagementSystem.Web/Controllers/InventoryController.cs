using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using GBBassetManagementSystem.Data.Context;
using GBBassetManagementSystem.Entity.Entities;
using GBBassetManagementSystem.Entity.Enums;
using GBBassetManagementSystem.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GBBassetManagementSystem.Web.Controllers;

[Authorize(Roles = "Admin,DepartmentUser")]
public class InventoryController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public InventoryController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Inventory main page.
    // Admin sees all assets.
    // DepartmentUser sees assets actively assigned to their department.
    public async Task<IActionResult> Index()
    {
        var userDepartmentId =
            await GetCurrentDepartmentIdAsync();

        if (User.IsInRole("DepartmentUser") &&
            !userDepartmentId.HasValue)
        {
            return Forbid();
        }

        var assetsQuery = _context.Assets
            .AsNoTracking()
            .Include(asset => asset.Category)
            .AsQueryable();

        if (User.IsInRole("DepartmentUser"))
        {
            var departmentId = userDepartmentId!.Value;

            var departmentAssetIds =
                _context.AssetAssignments
                    .AsNoTracking()
                    .Where(assignment =>
                        assignment.IsActive &&
                        (
                            assignment.Personnel != null &&
                            assignment.Personnel.DepartmentId ==
                            departmentId
                            ||
                            assignment.Room != null &&
                            assignment.Room.DepartmentId ==
                            departmentId
                        ))
                    .Select(assignment => assignment.AssetId)
                    .Distinct();

            assetsQuery = assetsQuery.Where(asset =>
                departmentAssetIds.Contains(asset.Id));
        }

        var assets = await assetsQuery.ToListAsync();

        var model = new InventoryViewModel
        {
            TotalAssets = assets.Count,

            AvailableAssets = assets.Count(asset =>
                asset.Status == AssetStatus.Available),

            AssignedAssets = assets.Count(asset =>
                asset.Status == AssetStatus.Assigned),

            MaintenanceAssets = assets.Count(asset =>
                asset.Status == AssetStatus.Maintenance),

            LostAssets = assets.Count(asset =>
                asset.Status == AssetStatus.Lost),

            DisposedAssets = assets.Count(asset =>
                asset.Status == AssetStatus.Disposed),

            Categories = assets
                .Where(asset => asset.Category != null)
                .GroupBy(asset => new
                {
                    asset.CategoryId,
                    CategoryName = asset.Category!.Name
                })
                .Select(group =>
                    new InventoryCategorySummaryViewModel
                    {
                        CategoryId =
                            group.Key.CategoryId,

                        CategoryName =
                            group.Key.CategoryName,

                        Total = group.Count(),

                        Available = group.Count(asset =>
                            asset.Status ==
                            AssetStatus.Available),

                        Assigned = group.Count(asset =>
                            asset.Status ==
                            AssetStatus.Assigned),

                        Maintenance = group.Count(asset =>
                            asset.Status ==
                            AssetStatus.Maintenance),

                        Lost = group.Count(asset =>
                            asset.Status ==
                            AssetStatus.Lost),

                        Disposed = group.Count(asset =>
                            asset.Status ==
                            AssetStatus.Disposed)
                    })
                .OrderBy(category =>
                    category.CategoryName)
                .ToList()
        };

        return View(model);
    }

    // Category inventory detail page.
    // Admin sees all departments.
    // DepartmentUser sees only their own department.
    public async Task<IActionResult> CategoryDetails(
        Guid id,
        AssetStatus? status,
        Guid? departmentId,
        Guid? roomId,
        string? search)
    {
        var userDepartmentId =
            await GetCurrentDepartmentIdAsync();

        if (User.IsInRole("DepartmentUser"))
        {
            if (!userDepartmentId.HasValue)
            {
                return Forbid();
            }

            // Prevent access to another department by changing the URL.
            if (departmentId.HasValue &&
                departmentId.Value !=
                userDepartmentId.Value)
            {
                return Forbid();
            }
        }

        var category = await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(category =>
                category.Id == id);

        if (category is null)
        {
            return NotFound();
        }

        var categoryAssetsQuery =
            _context.Assets
                .AsNoTracking()
                .Where(asset =>
                    asset.CategoryId == id);

        if (User.IsInRole("DepartmentUser"))
        {
            var currentDepartmentId =
                userDepartmentId!.Value;

            var departmentAssetIds =
                _context.AssetAssignments
                    .AsNoTracking()
                    .Where(assignment =>
                        assignment.IsActive &&
                        (
                            assignment.Personnel != null &&
                            assignment.Personnel.DepartmentId ==
                            currentDepartmentId
                            ||
                            assignment.Room != null &&
                            assignment.Room.DepartmentId ==
                            currentDepartmentId
                        ))
                    .Select(assignment =>
                        assignment.AssetId)
                    .Distinct();

            categoryAssetsQuery =
                categoryAssetsQuery.Where(asset =>
                    departmentAssetIds.Contains(asset.Id));
        }

        var categoryAssets =
            await categoryAssetsQuery
                .OrderBy(asset => asset.AssetCode)
                .ToListAsync();

        var assetIds = categoryAssets
            .Select(asset => asset.Id)
            .ToList();

        var activeAssignmentsQuery =
            _context.AssetAssignments
                .AsNoTracking()
                .Include(assignment =>
                    assignment.Personnel)
                    .ThenInclude(personnel =>
                        personnel!.Department)
                .Include(assignment =>
                    assignment.Room)
                    .ThenInclude(room =>
                        room!.Department)
               .Where(assignment =>
    assignment.IsActive &&
    assignment.AssetId.HasValue &&
    assetIds.Contains(
        assignment.AssetId.Value));

        if (User.IsInRole("DepartmentUser"))
        {
            var currentDepartmentId =
                userDepartmentId!.Value;

            activeAssignmentsQuery =
                activeAssignmentsQuery.Where(
                    assignment =>
                        (
                            assignment.Personnel != null &&
                            assignment.Personnel.DepartmentId ==
                            currentDepartmentId
                        )
                        ||
                        (
                            assignment.Room != null &&
                            assignment.Room.DepartmentId ==
                            currentDepartmentId
                        ));
        }

        var activeAssignments =
            await activeAssignmentsQuery
                .OrderByDescending(assignment =>
                    assignment.AssignmentDate)
                .ToListAsync();

        var assetRows =
            new List<InventoryAssetRowViewModel>();

        foreach (var asset in categoryAssets)
        {
            // If multiple active assignments accidentally exist,
            // use the most recent one.
            var activeAssignment =
                activeAssignments.FirstOrDefault(
                    assignment =>
                        assignment.AssetId == asset.Id);

            var row =
                new InventoryAssetRowViewModel
                {
                    AssetId = asset.Id,
                    AssetCode = asset.AssetCode,
                    Name = asset.Name,
                    Brand = asset.Brand,
                    Model = asset.Model,
                    SerialNumber = asset.SerialNumber,
                    Status = asset.Status,

                    AssignedTo = "—",

                    DepartmentName = "—",
                    DepartmentId = null,

                    RoomName = "—",
                    RoomId = null,

                    Location = "—"
                };

            if (activeAssignment != null)
            {
                // Asset assigned to personnel.
                if (activeAssignment.Personnel != null)
                {
                    row.AssignedTo =
                        $"{activeAssignment.Personnel.FirstName} " +
                        $"{activeAssignment.Personnel.LastName}";

                    row.DepartmentId =
                        activeAssignment.Personnel.DepartmentId;

                    row.DepartmentName =
                        activeAssignment.Personnel
                            .Department?.Name ?? "—";
                }

                // Asset assigned to a room.
                if (activeAssignment.Room != null)
                {
                    row.AssignedTo =
                        BuildRoomDisplayName(
                            activeAssignment.Room.Name,
                            activeAssignment.Room.RoomNumber,
                            activeAssignment.Room.Building);

                    row.RoomId =
                        activeAssignment.Room.Id;

                    row.RoomName =
                        BuildRoomDisplayName(
                            activeAssignment.Room.Name,
                            activeAssignment.Room.RoomNumber,
                            activeAssignment.Room.Building);

                    row.DepartmentId =
                        activeAssignment.Room.DepartmentId;

                    row.DepartmentName =
                        activeAssignment.Room
                            .Department?.Name ?? "—";

                    row.Location = row.RoomName;
                }
                else if (activeAssignment.Personnel != null)
                {
                    // If the asset is assigned to personnel
                    // without room information, use department
                    // as the location.
                    row.Location =
                        row.DepartmentName;
                }
            }

            assetRows.Add(row);
        }

        // Extra in-memory protection.
        if (User.IsInRole("DepartmentUser"))
        {
            var currentDepartmentId =
                userDepartmentId!.Value;

            assetRows = assetRows
                .Where(asset =>
                    asset.DepartmentId ==
                    currentDepartmentId)
                .ToList();
        }

        // Distribution summaries are calculated before
        // user-selected filters are applied.
        var departmentSummaries =
            assetRows
                .Where(asset =>
                    asset.DepartmentId.HasValue &&
                    asset.DepartmentName != "—")
                .GroupBy(asset => new
                {
                    DepartmentId =
                        asset.DepartmentId!.Value,

                    asset.DepartmentName
                })
                .Select(group =>
                    new InventoryDepartmentSummaryViewModel
                    {
                        DepartmentId =
                            group.Key.DepartmentId,

                        DepartmentName =
                            group.Key.DepartmentName,

                        AssetCount =
                            group.Count()
                    })
                .OrderByDescending(summary =>
                    summary.AssetCount)
                .ThenBy(summary =>
                    summary.DepartmentName)
                .ToList();

        var roomSummaries =
            assetRows
                .Where(asset =>
                    asset.RoomId.HasValue &&
                    asset.RoomName != "—")
                .GroupBy(asset => new
                {
                    RoomId =
                        asset.RoomId!.Value,

                    asset.RoomName,
                    asset.DepartmentName
                })
                .Select(group =>
                    new InventoryRoomSummaryViewModel
                    {
                        RoomId =
                            group.Key.RoomId,

                        RoomName =
                            group.Key.RoomName,

                        DepartmentName =
                            group.Key.DepartmentName,

                        AssetCount =
                            group.Count()
                    })
                .OrderByDescending(summary =>
                    summary.AssetCount)
                .ThenBy(summary =>
                    summary.RoomName)
                .ToList();

        var topAssignedModelsQuery =
            _context.AssetAssignments
                .AsNoTracking()
                .Where(assignment =>
                    assignment.Asset != null &&
                    assignment.Asset.CategoryId == id);

        if (User.IsInRole("DepartmentUser"))
        {
            var currentDepartmentId =
                userDepartmentId!.Value;

            topAssignedModelsQuery =
                topAssignedModelsQuery.Where(
                    assignment =>
                        (
                            assignment.Personnel != null &&
                            assignment.Personnel.DepartmentId ==
                            currentDepartmentId
                        )
                        ||
                        (
                            assignment.Room != null &&
                            assignment.Room.DepartmentId ==
                            currentDepartmentId
                        ));
        }

        var topAssignedModels =
            await topAssignedModelsQuery
                .GroupBy(assignment => new
                {
                    Brand =
                        assignment.Asset!.Brand,

                    Model =
                        assignment.Asset!.Model
                })
                .Select(group =>
                    new TopAssignedModelViewModel
                    {
                        Brand =
                            group.Key.Brand,

                        Model =
                            group.Key.Model,

                        AssignmentCount =
                            group.Count()
                    })
                .OrderByDescending(model =>
                    model.AssignmentCount)
                .Take(3)
                .ToListAsync();

        IEnumerable<InventoryAssetRowViewModel>
            filteredAssets = assetRows;

        if (status.HasValue)
        {
            filteredAssets =
                filteredAssets.Where(asset =>
                    asset.Status == status.Value);
        }

        Guid? effectiveDepartmentId = departmentId;

        if (User.IsInRole("DepartmentUser"))
        {
            effectiveDepartmentId =
                userDepartmentId!.Value;
        }

        if (effectiveDepartmentId.HasValue)
        {
            filteredAssets =
                filteredAssets.Where(asset =>
                    asset.DepartmentId ==
                    effectiveDepartmentId.Value);
        }

        if (roomId.HasValue)
        {
            var selectedRoom =
                await _context.Rooms
                    .AsNoTracking()
                    .FirstOrDefaultAsync(room =>
                        room.Id == roomId.Value);

            if (selectedRoom is null)
            {
                return NotFound();
            }

            if (User.IsInRole("DepartmentUser") &&
                selectedRoom.DepartmentId !=
                userDepartmentId!.Value)
            {
                return Forbid();
            }

            filteredAssets =
                filteredAssets.Where(asset =>
                    asset.RoomId == roomId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchValue = search.Trim();

            filteredAssets =
                filteredAssets.Where(asset =>
                    ContainsText(
                        asset.AssetCode,
                        searchValue)
                    ||
                    ContainsText(
                        asset.Name,
                        searchValue)
                    ||
                    ContainsText(
                        asset.Brand,
                        searchValue)
                    ||
                    ContainsText(
                        asset.Model,
                        searchValue)
                    ||
                    ContainsText(
                        asset.SerialNumber,
                        searchValue)
                    ||
                    ContainsText(
                        asset.AssignedTo,
                        searchValue)
                    ||
                    ContainsText(
                        asset.DepartmentName,
                        searchValue)
                    ||
                    ContainsText(
                        asset.RoomName,
                        searchValue)
                    ||
                    ContainsText(
                        asset.Location,
                        searchValue));
        }

        var model =
            new InventoryCategoryDetailsViewModel
            {
                CategoryId = category.Id,
                CategoryName = category.Name,

                Total = assetRows.Count,

                Available = assetRows.Count(asset =>
                    asset.Status ==
                    AssetStatus.Available),

                Assigned = assetRows.Count(asset =>
                    asset.Status ==
                    AssetStatus.Assigned),

                Maintenance = assetRows.Count(asset =>
                    asset.Status ==
                    AssetStatus.Maintenance),

                Lost = assetRows.Count(asset =>
                    asset.Status ==
                    AssetStatus.Lost),

                Disposed = assetRows.Count(asset =>
                    asset.Status ==
                    AssetStatus.Disposed),

                Search = search,
                SelectedStatus = status,

                SelectedDepartmentId =
                    effectiveDepartmentId,

                SelectedRoomId = roomId,

                Assets = filteredAssets
                    .OrderBy(asset =>
                        asset.AssetCode)
                    .ToList(),

                Departments =
                    departmentSummaries,

                Rooms =
                    roomSummaries,

                TopAssignedModels =
                    topAssignedModels
            };

        return View(model);
    }

    private async Task<Guid?> GetCurrentDepartmentIdAsync()
    {
        if (User.IsInRole("Admin"))
        {
            return null;
        }

        var currentUser =
            await _userManager.GetUserAsync(User);

        return currentUser?.DepartmentId;
    }

    private static string BuildRoomDisplayName(
        string roomName,
        string roomNumber,
        string? building)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(roomName))
        {
            parts.Add(roomName);
        }

        if (!string.IsNullOrWhiteSpace(roomNumber))
        {
            parts.Add($"Room {roomNumber}");
        }

        if (!string.IsNullOrWhiteSpace(building))
        {
            parts.Add(building);
        }

        return parts.Count == 0
            ? "—"
            : string.Join(" - ", parts);
    }

    private static bool ContainsText(
        string? value,
        string search)
    {
        return !string.IsNullOrWhiteSpace(value) &&
               value.Contains(
                   search,
                   StringComparison.OrdinalIgnoreCase);
    }
}