
using Microsoft.AspNetCore.Authorization;using GBBassetManagementSystem.Entity.Entities;
using GBBassetManagementSystem.Entity.Enums;
using GBBassetManagementSystem.Service.Interfaces;
using GBBassetManagementSystem.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GBBassetManagementSystem.Web.Controllers;

[Authorize(Roles = "Admin,DepartmentUser")]
public class AssetsController : Controller
{
    private readonly IAssetService _assetService;
    private readonly ICategoryService _categoryService;
    private readonly IAssetAssignmentService _assignmentService;

    public AssetsController(
        IAssetService assetService,
        ICategoryService categoryService,
        IAssetAssignmentService assignmentService)
    {
        _assetService = assetService;
        _categoryService = categoryService;
        _assignmentService = assignmentService;
    }

   public async Task<IActionResult> Index(string? search)
{
    var assets = await _assetService.GetAllAsync();

    if (!string.IsNullOrWhiteSpace(search))
    {
        var searchValue = search.Trim();

        assets = assets
            .Where(asset =>
                ContainsText(asset.AssetCode, searchValue) ||
                ContainsText(asset.Name, searchValue) ||
                ContainsText(asset.Category?.Name, searchValue) ||
                ContainsText(asset.Brand, searchValue) ||
                ContainsText(asset.Model, searchValue) ||
                ContainsText(asset.SerialNumber, searchValue) ||
                ContainsText(asset.Status.ToString(), searchValue))
            .ToList();
    }

    ViewBag.Search = search;

    return View(assets);
}

    public async Task<IActionResult> Details(Guid id)
    {
        var asset = await _assetService.GetByIdAsync(id);

        if (asset is null)
        {
            return NotFound();
        }

        var assignmentHistory =
            await _assignmentService.GetByAssetIdAsync(id);

        var model = new AssetDetailsViewModel
        {
            Asset = asset,
            AssignmentHistory = assignmentHistory
        };

        return View(model);
    }

[Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        await LoadFormDataAsync();

        return View();
    }

[Authorize(Roles = "Admin")]

 [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(Asset asset)
{
 // Since AssetCode no longer comes from the form, we are removing the old validation result.
    ModelState.Remove(nameof(Asset.AssetCode));
// A new asset is always created as Available.
    asset.Status = AssetStatus.Available;

    if (asset.CategoryId == Guid.Empty)
    {
        ModelState.AddModelError(
            nameof(Asset.CategoryId),
            "Please select a category.");
    }
    else
    {
        try
        {
            asset.AssetCode =
                await GenerateAssetCodeAsync(asset.CategoryId);
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(
                nameof(Asset.CategoryId),
                exception.Message);
        }
    }

    if (!ModelState.IsValid)
    {
        await LoadFormDataAsync(
            asset.CategoryId,
            AssetStatus.Available);

        return View(asset);
    }

    await _assetService.AddAsync(asset);

    TempData["SuccessMessage"] =
        $"Asset {asset.AssetCode} was created successfully.";

    return RedirectToAction(nameof(Index));
}

[Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var asset = await _assetService.GetByIdAsync(id);

        if (asset is null)
        {
            return NotFound();
        }

        await LoadFormDataAsync(
            asset.CategoryId,
            asset.Status);

        return View(asset);
    }

[Authorize(Roles = "Admin")]

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        Guid id,
        Asset asset)
    {
        if (id != asset.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await LoadFormDataAsync(
                asset.CategoryId,
                asset.Status);

            return View(asset);
        }

        try
        {
            await _assetService.UpdateAsync(asset);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] =
            "Asset updated successfully.";

        return RedirectToAction(nameof(Index));
    }

[Authorize(Roles = "Admin")]
   public async Task<IActionResult> Dispose(Guid id)
{
    var asset = await _assetService.GetByIdAsync(id);

    if (asset is null)
    {
        return NotFound();
    }

    var assignmentHistory =
        await _assignmentService.GetByAssetIdAsync(id);

    var hasActiveAssignment =
        assignmentHistory.Any(assignment => assignment.IsActive);

    if (hasActiveAssignment ||
        asset.Status == AssetStatus.Assigned)
    {
        TempData["ErrorMessage"] =
            "Assigned assets cannot be disposed. Return the asset first.";

        return RedirectToAction(nameof(Index));
    }

    if (asset.Status == AssetStatus.Disposed)
    {
        TempData["ErrorMessage"] =
            "This asset has already been disposed.";

        return RedirectToAction(nameof(Index));
    }

    return View(asset);
}

[Authorize(Roles = "Admin")]
   [HttpPost, ActionName("Dispose")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> DisposeConfirmed(Guid id)
{
    var asset = await _assetService.GetByIdAsync(id);

    if (asset is null)
    {
        return NotFound();
    }

    var assignmentHistory =
        await _assignmentService.GetByAssetIdAsync(id);

    var hasActiveAssignment =
        assignmentHistory.Any(assignment => assignment.IsActive);

    if (hasActiveAssignment ||
        asset.Status == AssetStatus.Assigned)
    {
        TempData["ErrorMessage"] =
            "Assigned assets cannot be disposed. Return the asset first.";

        return RedirectToAction(nameof(Index));
    }

    if (asset.Status == AssetStatus.Disposed)
    {
        TempData["ErrorMessage"] =
            "This asset has already been disposed.";

        return RedirectToAction(nameof(Index));
    }

   try
{
    await _assetService.DisposeAsync(id);
}
catch (KeyNotFoundException)
{
    return NotFound();
}
catch (InvalidOperationException exception)
{
    TempData["ErrorMessage"] = exception.Message;

    return RedirectToAction(nameof(Index));
}
    TempData["SuccessMessage"] =
        $"Asset {asset.AssetCode} was disposed successfully.";

    return RedirectToAction(nameof(Index));
    
}

    private async Task LoadFormDataAsync(
        Guid? selectedCategoryId = null,
        AssetStatus? selectedStatus = null)
    {
        var categories =
            await _categoryService.GetAllAsync();

        ViewBag.Categories = new SelectList(
            categories,
            "Id",
            "Name",
            selectedCategoryId);

        ViewBag.Statuses = new SelectList(
            Enum.GetValues<AssetStatus>()
                .Select(status => new
                {
                    Id = (int)status,
                    Name = status.ToString()
                }),
            "Id",
            "Name",
            selectedStatus is null
                ? null
                : (int)selectedStatus);
    }

    private async Task<string> GenerateAssetCodeAsync(Guid categoryId)
{
    var category = await _categoryService.GetByIdAsync(categoryId);

    if (category is null)
    {
        throw new InvalidOperationException("Selected category was not found.");
    }

    if (string.IsNullOrWhiteSpace(category.Code))
    {
        throw new InvalidOperationException(
            "The selected category does not have a category code.");
    }

    var categoryCode = category.Code.Trim().ToUpperInvariant();
    var assetCodePrefix = $"GBB-{categoryCode}-";

    var assets = await _assetService.GetAllAsync();

    var highestNumber = assets
        .Where(asset =>
            !string.IsNullOrWhiteSpace(asset.AssetCode) &&
            asset.AssetCode.StartsWith(
                assetCodePrefix,
                StringComparison.OrdinalIgnoreCase))
        .Select(asset =>
        {
            var numberPart = asset.AssetCode.Substring(assetCodePrefix.Length);

            return int.TryParse(numberPart, out var number)
                ? number
                : 0;
        })
        .DefaultIfEmpty(0)
        .Max();

    var nextNumber = highestNumber + 1;

    return $"{assetCodePrefix}{nextNumber:0000}";
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