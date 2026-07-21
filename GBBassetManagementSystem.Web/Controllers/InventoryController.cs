using GBBassetManagementSystem.Data.Context;
using GBBassetManagementSystem.Entity.Enums;
using GBBassetManagementSystem.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GBBassetManagementSystem.Web.Controllers;

public class InventoryController : Controller
{
    private readonly ApplicationDbContext _context;

    public InventoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Inventory ana sayfası
    // Genel durum özeti ve kategori dağılımı
    public async Task<IActionResult> Index()
    {
        var assets = await _context.Assets
            .AsNoTracking()
            .Include(asset => asset.Category)
            .ToListAsync();

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

            Categories = assets
                .Where(asset => asset.Category != null)
                .GroupBy(asset => new
                {
                    asset.CategoryId,
                    CategoryName = asset.Category!.Name
                })
                .Select(group => new InventoryCategorySummaryViewModel
                {
                    CategoryId = group.Key.CategoryId,
                    CategoryName = group.Key.CategoryName,

                    Total = group.Count(),

                    Available = group.Count(
                        asset => asset.Status == AssetStatus.Available),

                    Assigned = group.Count(
                        asset => asset.Status == AssetStatus.Assigned),

                    Broken = group.Count(
                        asset => asset.Status == AssetStatus.Broken),

                    UnderMaintenance = group.Count(
                        asset =>
                            asset.Status == AssetStatus.UnderMaintenance),

                    Lost = group.Count(
                        asset => asset.Status == AssetStatus.Lost)
                })
                .OrderBy(category => category.CategoryName)
                .ToList()
        };

        return View(model);
    }

    // Örnek:
    // Inventory/CategoryDetails/CATEGORY_ID
    //
    // Bu sayfada:
    // - kategori özeti
    // - departman dağılımı
    // - oda dağılımı
    // - demirbaş listesi
    // - filtreler
    public async Task<IActionResult> CategoryDetails(
        Guid id,
        AssetStatus? status,
        Guid? departmentId,
        Guid? roomId,
        string? search)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(category => category.Id == id);

        if (category == null)
        {
            return NotFound();
        }

        var categoryAssets = await _context.Assets
            .AsNoTracking()
            .Where(asset => asset.CategoryId == id)
            .OrderBy(asset => asset.AssetCode)
            .ToListAsync();

        var assetIds = categoryAssets
            .Select(asset => asset.Id)
            .ToList();

        var activeAssignments = await _context.AssetAssignments
            .AsNoTracking()
            .Include(assignment => assignment.Personnel)
                .ThenInclude(personnel => personnel!.Department)
            .Include(assignment => assignment.Room)
                .ThenInclude(room => room!.Department)
            .Where(assignment =>
                assignment.IsActive &&
                assetIds.Contains(assignment.AssetId))
            .OrderByDescending(assignment => assignment.AssignmentDate)
            .ToListAsync();

        var assetRows = new List<InventoryAssetRowViewModel>();

        foreach (var asset in categoryAssets)
        {
            // Aynı demirbaşa yanlışlıkla birden fazla aktif zimmet
            // kaydedilmişse en yeni olanı kullanır.
            var activeAssignment = activeAssignments
                .FirstOrDefault(assignment =>
                    assignment.AssetId == asset.Id);

            var row = new InventoryAssetRowViewModel
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
               
            };

            if (activeAssignment != null)
            {
                // Personele zimmetli demirbaş
                if (activeAssignment.Personnel != null)
                {
                    row.AssignedTo =
                        $"{activeAssignment.Personnel.FirstName} " +
                        $"{activeAssignment.Personnel.LastName}";

                    row.DepartmentId =
                        activeAssignment.Personnel.DepartmentId;

                    row.DepartmentName =
                        activeAssignment.Personnel.Department?.Name ?? "—";
                }

                // Odaya zimmetli demirbaş
                if (activeAssignment.Room != null)
                {
                    row.RoomId = activeAssignment.Room.Id;

                    row.RoomName = BuildRoomDisplayName(
                        activeAssignment.Room.Name,
                        activeAssignment.Room.RoomNumber,
                        activeAssignment.Room.Building);

                    row.DepartmentId =
                        activeAssignment.Room.DepartmentId;

                    row.DepartmentName =
                        activeAssignment.Room.Department?.Name ?? "—";

                    row.Location = row.RoomName;
                }
                else if (activeAssignment.Personnel != null)
                {
                    // Personele atanmış fakat oda bilgisi yoksa
                    // konum olarak personelin departmanını gösterir.
                    row.Location = row.DepartmentName;
                }
            }

            assetRows.Add(row);
        }

        // Dağılımlar, filtre uygulanmadan önce bütün kategori üzerinden
        // hesaplanır.
        var departmentSummaries = assetRows
            .Where(asset =>
                asset.DepartmentId.HasValue &&
                asset.DepartmentName != "—")
            .GroupBy(asset => new
            {
                DepartmentId = asset.DepartmentId!.Value,
                asset.DepartmentName
            })
            .Select(group => new InventoryDepartmentSummaryViewModel
            {
                DepartmentId = group.Key.DepartmentId,
                DepartmentName = group.Key.DepartmentName,
                AssetCount = group.Count()
            })
            .OrderByDescending(summary => summary.AssetCount)
            .ThenBy(summary => summary.DepartmentName)
            .ToList();
 

        var roomSummaries = assetRows
            .Where(asset =>
                asset.RoomId.HasValue &&
                asset.RoomName != "—")
            .GroupBy(asset => new
            {
                RoomId = asset.RoomId!.Value,
                asset.RoomName,
                asset.DepartmentName
            })
            .Select(group => new InventoryRoomSummaryViewModel
            {
                RoomId = group.Key.RoomId,
                RoomName = group.Key.RoomName,
                DepartmentName = group.Key.DepartmentName,
                AssetCount = group.Count()
            })
            .OrderByDescending(summary => summary.AssetCount)
            .ThenBy(summary => summary.RoomName)
            .ToList();


                  // Top 3 most assigned models in this category
var topAssignedModels = await _context.AssetAssignments
    .AsNoTracking()
    .Include(x => x.Asset)
    .Where(x =>
        x.Asset != null &&
        x.Asset.CategoryId == id)
    .GroupBy(x => new
    {
        Brand = x.Asset!.Brand,
        Model = x.Asset!.Model
    })
    .Select(group => new TopAssignedModelViewModel
    {
        Brand = group.Key.Brand,
        Model = group.Key.Model,
        AssignmentCount = group.Count()
    })
    .OrderByDescending(model => model.AssignmentCount)
    .Take(3)
    .ToListAsync();

        IEnumerable<InventoryAssetRowViewModel> filteredAssets =
            assetRows;

        if (status.HasValue)
        {
            filteredAssets = filteredAssets.Where(
                asset => asset.Status == status.Value);
        }

        if (departmentId.HasValue)
        {
            filteredAssets = filteredAssets.Where(
                asset => asset.DepartmentId == departmentId.Value);
        }

        if (roomId.HasValue)
        {
            filteredAssets = filteredAssets.Where(
                asset => asset.RoomId == roomId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchValue = search.Trim();

            filteredAssets = filteredAssets.Where(asset =>
                ContainsText(asset.AssetCode, searchValue) ||
                ContainsText(asset.Name, searchValue) ||
                ContainsText(asset.Brand, searchValue) ||
                ContainsText(asset.Model, searchValue) ||
                ContainsText(asset.SerialNumber, searchValue) ||
                ContainsText(asset.AssignedTo, searchValue) ||
                ContainsText(asset.DepartmentName, searchValue) ||
                ContainsText(asset.RoomName, searchValue) ||
                ContainsText(asset.Location, searchValue));
        }

      var model = new InventoryCategoryDetailsViewModel
{
    CategoryId = category.Id,
    CategoryName = category.Name,

    Total = assetRows.Count,

    Available = assetRows.Count(
        asset => asset.Status == AssetStatus.Available),

    Assigned = assetRows.Count(
        asset => asset.Status == AssetStatus.Assigned),

    Broken = assetRows.Count(
        asset => asset.Status == AssetStatus.Broken),

    UnderMaintenance = assetRows.Count(
        asset => asset.Status == AssetStatus.UnderMaintenance),

    Lost = assetRows.Count(
        asset => asset.Status == AssetStatus.Lost),

    Search = search,
    SelectedStatus = status,
    SelectedDepartmentId = departmentId,
    SelectedRoomId = roomId,

    Assets = filteredAssets
        .OrderBy(asset => asset.AssetCode)
        .ToList(),

    Departments = departmentSummaries,
    Rooms = roomSummaries,
    TopAssignedModels = topAssignedModels
};

        return View(model);
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