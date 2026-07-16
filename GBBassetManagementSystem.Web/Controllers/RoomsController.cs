using GBBassetManagementSystem.Entity.Entities;
using GBBassetManagementSystem.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GBBassetManagementSystem.Web.Controllers;

public class RoomsController : Controller
{
    private readonly IRoomService _roomService;
    private readonly IDepartmentService _departmentService;

    public RoomsController(
        IRoomService roomService,
        IDepartmentService departmentService)
    {
        _roomService = roomService;
        _departmentService = departmentService;
    }

    public async Task<IActionResult> Index()
    {
        var rooms = await _roomService.GetAllAsync();
        return View(rooms);
    }

    public async Task<IActionResult> Create()
    {
        await LoadDepartmentsAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Room room)
    {
        if (!ModelState.IsValid)
        {
            await LoadDepartmentsAsync(room.DepartmentId);
            return View(room);
        }

        await _roomService.AddAsync(room);

        TempData["SuccessMessage"] = "Room added successfully.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var room = await _roomService.GetByIdAsync(id);

        if (room is null)
        {
            return NotFound();
        }

        await LoadDepartmentsAsync(room.DepartmentId);

        return View(room);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, Room room)
    {
        if (id != room.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await LoadDepartmentsAsync(room.DepartmentId);
            return View(room);
        }

        await _roomService.UpdateAsync(room);

        TempData["SuccessMessage"] = "Room updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var room = await _roomService.GetByIdAsync(id);

        if (room is null)
        {
            return NotFound();
        }

        return View(room);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        await _roomService.DeleteAsync(id);

        TempData["SuccessMessage"] = "Room deleted successfully.";

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadDepartmentsAsync(Guid? selectedDepartment = null)
    {
        var departments = await _departmentService.GetAllAsync();

        ViewBag.Departments = new SelectList(
            departments,
            "Id",
            "Name",
            selectedDepartment);
    }
}