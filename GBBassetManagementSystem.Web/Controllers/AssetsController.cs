using GBBassetManagementSystem.Entity.Entities;
using GBBassetManagementSystem.Entity.Enums;
using GBBassetManagementSystem.Service.Interfaces;
using GBBassetManagementSystem.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GBBassetManagementSystem.Web.Controllers;

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

    public async Task<IActionResult> Index()
    {
        var assets = await _assetService.GetAllAsync();

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

    public async Task<IActionResult> Create()
    {
        await LoadFormDataAsync();

        return View();
    }

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

    public async Task<IActionResult> Delete(Guid id)
    {
        var asset = await _assetService.GetByIdAsync(id);

        if (asset is null)
        {
            return NotFound();
        }

        return View(asset);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        try
        {
            await _assetService.DeleteAsync(id);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] =
            "Asset deleted successfully.";

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
}