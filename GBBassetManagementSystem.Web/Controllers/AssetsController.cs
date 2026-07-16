using GBBassetManagementSystem.Entity.Entities;
using GBBassetManagementSystem.Entity.Enums;
using GBBassetManagementSystem.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GBBassetManagementSystem.Web.Controllers;

public class AssetsController : Controller
{
    private readonly IAssetService _assetService;
    private readonly ICategoryService _categoryService;

    public AssetsController(
        IAssetService assetService,
        ICategoryService categoryService)
    {
        _assetService = assetService;
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index()
    {
        var assets = await _assetService.GetAllAsync();
        return View(assets);
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
        if (!ModelState.IsValid)
        {
            await LoadFormDataAsync(asset.CategoryId, asset.Status);
            return View(asset);
        }

        await _assetService.AddAsync(asset);

        TempData["SuccessMessage"] = "Asset saved successfully.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var asset = await _assetService.GetByIdAsync(id);

        if (asset is null)
        {
            return NotFound();
        }

        await LoadFormDataAsync(asset.CategoryId, asset.Status);

        return View(asset);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, Asset asset)
    {
        if (id != asset.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await LoadFormDataAsync(asset.CategoryId, asset.Status);
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

        TempData["SuccessMessage"] = "Asset updated successfully.";

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

        TempData["SuccessMessage"] = "Asset deleted successfully.";

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadFormDataAsync(
        Guid? selectedCategoryId = null,
        AssetStatus? selectedStatus = null)
    {
        var categories = await _categoryService.GetAllAsync();

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
            selectedStatus is null ? null : (int)selectedStatus);
    }
}