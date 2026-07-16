using GBBassetManagementSystem.Entity.Enums;
using GBBassetManagementSystem.Service.Interfaces;
using GBBassetManagementSystem.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace GBBassetManagementSystem.Web.Controllers;

public class InventoryController : Controller
{
    private readonly IAssetService _assetService;
    private readonly IAssetAssignmentService _assignmentService;
    private readonly ICategoryService _categoryService;

    public InventoryController(
        IAssetService assetService,
        IAssetAssignmentService assignmentService,
        ICategoryService categoryService)
    {
        _assetService = assetService;
        _assignmentService = assignmentService;
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index()
    {
        var assets = await _assetService.GetAllAsync();
        var categories = await _categoryService.GetAllAsync();

        var model = new InventoryViewModel
        {
            TotalAssets = assets.Count,

            AvailableAssets = assets.Count(
                asset => asset.Status == AssetStatus.Available),

            AssignedAssets = assets.Count(
                asset => asset.Status == AssetStatus.Assigned),

            BrokenAssets = assets.Count(
                asset => asset.Status == AssetStatus.Broken),

            UnderMaintenanceAssets = assets.Count(
                asset => asset.Status == AssetStatus.UnderMaintenance),

            LostAssets = assets.Count(
                asset => asset.Status == AssetStatus.Lost),

            Categories = categories
                .Select(category =>
                {
                    var categoryAssets = assets
                        .Where(asset => asset.CategoryId == category.Id)
                        .ToList();

                    return new InventoryCategorySummaryViewModel
                    {
                        CategoryId = category.Id,
                        CategoryName = category.Name,
                        Total = categoryAssets.Count,

                        Available = categoryAssets.Count(
                            asset => asset.Status == AssetStatus.Available),

                        Assigned = categoryAssets.Count(
                            asset => asset.Status == AssetStatus.Assigned),

                        Broken = categoryAssets.Count(
                            asset => asset.Status == AssetStatus.Broken),

                        UnderMaintenance = categoryAssets.Count(
                            asset => asset.Status ==
                                     AssetStatus.UnderMaintenance),

                        Lost = categoryAssets.Count(
                            asset => asset.Status == AssetStatus.Lost)
                    };
                })
                .OrderBy(category => category.CategoryName)
                .ToList()
        };

        return View(model);
    }

    public async Task<IActionResult> CategoryDetails(
        Guid id,
        string? search,
        AssetStatus? status,
        Guid? departmentId,
        Guid? roomId)
    {
        var category = await _categoryService.GetByIdAsync(id);

        if (category is null)
        {
            return NotFound();
        }

        var allAssets = await _assetService.GetAllAsync();
        var assignments = await _assignmentService.GetAllAsync();

        var categoryAssets = allAssets
            .Where(asset => asset.CategoryId == id)
            .ToList();

        var assetRows = categoryAssets
            .Select(asset =>
            {
                var activeAssignment = assignments
                    .Where(assignment =>
                        assignment.AssetId == asset.Id &&
                        assignment.IsActive)
                    .OrderByDescending(assignment =>
                        assignment.AssignmentDate)
                    .FirstOrDefault();

                var assignedTo = "—";
                var departmentName = "—";
                Guid? activeDepartmentId = null;
                var roomName = "—";
                Guid? activeRoomId = null;

                if (activeAssignment?.Personnel is not null)
                {
                    assignedTo =
                        $"{activeAssignment.Personnel.FirstName} " +
                        $"{activeAssignment.Personnel.LastName}";

                    departmentName =
                        activeAssignment.Personnel.Department?.Name ?? "—";

                    activeDepartmentId =
                        activeAssignment.Personnel.DepartmentId;
                }
                else if (activeAssignment?.Room is not null)
                {
                    assignedTo = activeAssignment.Room.Name;

                    departmentName =
                        activeAssignment.Room.Department?.Name ?? "—";

                    activeDepartmentId =
                        activeAssignment.Room.DepartmentId;

                    activeRoomId = activeAssignment.Room.Id;

                    roomName =
                        $"{activeAssignment.Room.Name} " +
                        $"({activeAssignment.Room.RoomNumber})";
                }

                var location = asset.Location ?? "—";

                if (activeAssignment?.Room is not null)
                {
                    location =
                        $"{activeAssignment.Room.Building} / " +
                        $"{activeAssignment.Room.Floor} / " +
                        $"{activeAssignment.Room.RoomNumber}";
                }
                else if (activeAssignment?.Personnel is not null)
                {
                    location = departmentName;
                }

                return new InventoryAssetRowViewModel
                {
                    AssetId = asset.Id,
                    AssetCode = asset.AssetCode,
                    Name = asset.Name,
                    Brand = asset.Brand,
                    Model = asset.Model,
                    SerialNumber = asset.SerialNumber,
                    Status = asset.Status,
                    AssignedTo = assignedTo,
                    DepartmentId = activeDepartmentId,
                    DepartmentName = departmentName,
                    RoomId = activeRoomId,
                    RoomName = roomName,
                    Location = location
                };
            })
            .ToList();

        var departmentSummaries = assetRows
            .Where(asset => asset.DepartmentId.HasValue)
            .GroupBy(asset => new
            {
                asset.DepartmentId,
                asset.DepartmentName
            })
            .Select(group => new InventoryDepartmentSummaryViewModel
            {
                DepartmentId = group.Key.DepartmentId!.Value,
                DepartmentName = group.Key.DepartmentName,
                AssetCount = group.Count()
            })
            .OrderByDescending(item => item.AssetCount)
            .ToList();

        var roomSummaries = assetRows
            .Where(asset => asset.RoomId.HasValue)
            .GroupBy(asset => new
            {
                asset.RoomId,
                asset.RoomName,
                asset.DepartmentName
            })
            .Select(group => new InventoryRoomSummaryViewModel
            {
                RoomId = group.Key.RoomId!.Value,
                RoomName = group.Key.RoomName,
                DepartmentName = group.Key.DepartmentName,
                AssetCount = group.Count()
            })
            .OrderByDescending(item => item.AssetCount)
            .ToList();

        var filteredAssets = assetRows.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            filteredAssets = filteredAssets.Where(asset =>
                asset.AssetCode.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase) ||
                asset.Name.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase) ||
                asset.Brand.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase) ||
                asset.Model.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase) ||
                asset.SerialNumber.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase) ||
                asset.AssignedTo.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase));
        }

        if (status.HasValue)
        {
            filteredAssets = filteredAssets
                .Where(asset => asset.Status == status.Value);
        }

        if (departmentId.HasValue)
        {
            filteredAssets = filteredAssets
                .Where(asset =>
                    asset.DepartmentId == departmentId.Value);
        }

        if (roomId.HasValue)
        {
            filteredAssets = filteredAssets
                .Where(asset => asset.RoomId == roomId.Value);
        }

        var model = new InventoryCategoryDetailsViewModel
        {
            CategoryId = category.Id,
            CategoryName = category.Name,

            Total = categoryAssets.Count,

            Available = categoryAssets.Count(
                asset => asset.Status == AssetStatus.Available),

            Assigned = categoryAssets.Count(
                asset => asset.Status == AssetStatus.Assigned),

            Broken = categoryAssets.Count(
                asset => asset.Status == AssetStatus.Broken),

            UnderMaintenance = categoryAssets.Count(
                asset => asset.Status ==
                         AssetStatus.UnderMaintenance),

            Lost = categoryAssets.Count(
                asset => asset.Status == AssetStatus.Lost),

            Search = search,
            SelectedStatus = status,
            SelectedDepartmentId = departmentId,
            SelectedRoomId = roomId,

            Assets = filteredAssets
                .OrderBy(asset => asset.AssetCode)
                .ToList(),

            Departments = departmentSummaries,
            Rooms = roomSummaries
        };

        return View(model);
    }
}