using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using GBBassetManagementSystem.Entity.Entities;
using GBBassetManagementSystem.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GBBassetManagementSystem.Web.Controllers;

[Authorize(Roles = "Admin")]
public class DepartmentsController : Controller
{
    private readonly IDepartmentService _departmentService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public DepartmentsController(
        IDepartmentService departmentService,
        IStringLocalizer<SharedResource> localizer)
    {
        _departmentService = departmentService;
        _localizer = localizer;
    }

    public async Task<IActionResult> Index()
    {
        var departments =
            await _departmentService.GetAllAsync();

        return View(departments);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        Department department)
    {
        if (!ModelState.IsValid)
        {
            return View(department);
        }

        await _departmentService.AddAsync(department);

        TempData["SuccessMessage"] =
            _localizer["DepartmentSavedSuccessfully"].Value;

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var department =
            await _departmentService.GetByIdAsync(id);

        if (department is null)
        {
            return NotFound();
        }

        return View(department);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        Guid id,
        Department department)
    {
        if (id != department.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(department);
        }

        try
        {
            await _departmentService.UpdateAsync(department);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] =
            _localizer["DepartmentUpdatedSuccessfully"].Value;

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var department =
            await _departmentService.GetByIdAsync(id);

        if (department is null)
        {
            return NotFound();
        }

        return View(department);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        try
        {
            await _departmentService.DeleteAsync(id);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] =
            _localizer["DepartmentDeletedSuccessfully"].Value;

        return RedirectToAction(nameof(Index));
    }
}