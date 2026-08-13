using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using GBBassetManagementSystem.Entity.Entities;
using GBBassetManagementSystem.Service.Interfaces;

namespace GBBassetManagementSystem.Web.Controllers;

[Authorize(Roles = "Admin")]
public class CategoriesController : Controller
{
    private readonly ICategoryService _categoryService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public CategoriesController(
        ICategoryService categoryService,
        IStringLocalizer<SharedResource> localizer)
    {
        _categoryService = categoryService;
        _localizer = localizer;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _categoryService.GetAllAsync();

        return View(categories);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        if (!string.IsNullOrWhiteSpace(category.Code))
        {
            category.Code = category.Code.Trim().ToUpperInvariant();
        }

        if (!string.IsNullOrWhiteSpace(category.Name))
        {
            category.Name = category.Name.Trim();
        }

        if (!ModelState.IsValid)
        {
            return View(category);
        }

        await _categoryService.AddAsync(category);

        TempData["SuccessMessage"] =
            _localizer["CategoryCreated"].Value;

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var category = await _categoryService.GetByIdAsync(id);

        if (category is null)
        {
            return NotFound();
        }

        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, Category category)
    {
        if (id != category.Id)
        {
            return BadRequest();
        }

        if (!string.IsNullOrWhiteSpace(category.Code))
        {
            category.Code = category.Code.Trim().ToUpperInvariant();
        }

        if (!string.IsNullOrWhiteSpace(category.Name))
        {
            category.Name = category.Name.Trim();
        }

        if (!ModelState.IsValid)
        {
            return View(category);
        }

        try
        {
            await _categoryService.UpdateAsync(category);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] =
            _localizer["CategoryUpdated"].Value;

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var category = await _categoryService.GetByIdAsync(id);

        if (category is null)
        {
            return NotFound();
        }

        return View(category);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        try
        {
            await _categoryService.DeleteAsync(id);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException)
        {
            TempData["ErrorMessage"] =
                _localizer["CategoryContainsAssets"].Value;

            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] =
            _localizer["CategoryDeleted"].Value;

        return RedirectToAction(nameof(Index));
    }
}