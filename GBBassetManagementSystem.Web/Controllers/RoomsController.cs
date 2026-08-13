using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using GBBassetManagementSystem.Entity.Entities;
using GBBassetManagementSystem.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
namespace GBBassetManagementSystem.Web.Controllers;

[Authorize(Roles = "Admin,DepartmentUser")]
public class RoomsController : Controller
{
    private readonly IRoomService _roomService;
    private readonly IDepartmentService _departmentService;
    private readonly UserManager<ApplicationUser> _userManager;
private readonly IStringLocalizer<SharedResource> _localizer;
   public RoomsController(
    IRoomService roomService,
    IDepartmentService departmentService,
    UserManager<ApplicationUser> userManager,
    IStringLocalizer<SharedResource> localizer)
{
    _roomService = roomService;
    _departmentService = departmentService;
    _userManager = userManager;
    _localizer = localizer;
}
    public async Task<IActionResult> Index()
    {
        var rooms = await _roomService.GetAllAsync();

        if (User.IsInRole("DepartmentUser"))
        {
            var departmentId = await GetCurrentDepartmentIdAsync();

            if (departmentId is null)
            {
                return Forbid();
            }

            rooms = rooms
                .Where(room =>
                    room.DepartmentId == departmentId.Value)
                .ToList();
        }

        return View(rooms);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadDepartmentsForCurrentUserAsync();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Room room)
    {
        if (User.IsInRole("DepartmentUser"))
        {
            var departmentId = await GetCurrentDepartmentIdAsync();

            if (departmentId is null)
            {
                return Forbid();
            }

            room.DepartmentId = departmentId.Value;

            ModelState.Remove(nameof(Room.DepartmentId));
        }
if (room.DepartmentId == Guid.Empty)
{
    ModelState.AddModelError(
        nameof(Room.DepartmentId),
        _localizer["DepartmentRequired"]);
}
        if (!ModelState.IsValid)
        {
            await LoadDepartmentsForCurrentUserAsync(
                room.DepartmentId);

            return View(room);
        }

        await _roomService.AddAsync(room);

        TempData["SuccessMessage"] =
            "Room added successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var room = await _roomService.GetByIdAsync(id);

        if (room is null)
        {
            return NotFound();
        }

        if (!await CanAccessRoomAsync(room))
        {
            return Forbid();
        }

        await LoadDepartmentsForCurrentUserAsync(
            room.DepartmentId);

        return View(room);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        Guid id,
        Room room)
    {
        if (id != room.Id)
        {
            return BadRequest();
        }

        var existingRoom =
            await _roomService.GetByIdAsync(id);

        if (existingRoom is null)
        {
            return NotFound();
        }

        if (!await CanAccessRoomAsync(existingRoom))
        {
            return Forbid();
        }

        if (User.IsInRole("DepartmentUser"))
        {
            var departmentId = await GetCurrentDepartmentIdAsync();

            if (departmentId is null)
            {
                return Forbid();
            }

            room.DepartmentId = departmentId.Value;

            ModelState.Remove(nameof(Room.DepartmentId));
        }
if (room.DepartmentId == Guid.Empty)
{
    ModelState.AddModelError(
        nameof(Room.DepartmentId),
        _localizer["DepartmentRequired"]);
}
        if (!ModelState.IsValid)
        {
            await LoadDepartmentsForCurrentUserAsync(
                room.DepartmentId);

            return View(room);
        }

        try
        {
            await _roomService.UpdateAsync(room);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] =
            "Room updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var room = await _roomService.GetByIdAsync(id);

        if (room is null)
        {
            return NotFound();
        }

        if (!await CanAccessRoomAsync(room))
        {
            return Forbid();
        }

        return View(room);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var room = await _roomService.GetByIdAsync(id);

        if (room is null)
        {
            return NotFound();
        }

        if (!await CanAccessRoomAsync(room))
        {
            return Forbid();
        }

        try
        {
            await _roomService.DeleteAsync(id);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] =
            "Room deleted successfully.";

        return RedirectToAction(nameof(Index));
    }

    private async Task<Guid?> GetCurrentDepartmentIdAsync()
    {
        var currentUser =
            await _userManager.GetUserAsync(User);

        return currentUser?.DepartmentId;
    }

    private async Task<bool> CanAccessRoomAsync(Room room)
    {
        if (User.IsInRole("Admin"))
        {
            return true;
        }

        var departmentId =
            await GetCurrentDepartmentIdAsync();

        return departmentId.HasValue &&
               room.DepartmentId == departmentId.Value;
    }

    private async Task LoadDepartmentsForCurrentUserAsync(
        Guid? selectedDepartmentId = null)
    {
        var departments =
            await _departmentService.GetAllAsync();

        if (User.IsInRole("DepartmentUser"))
        {
            var departmentId =
                await GetCurrentDepartmentIdAsync();

            departments = departmentId.HasValue
                ? departments
                    .Where(department =>
                        department.Id == departmentId.Value)
                    .ToList()
                : [];

            selectedDepartmentId = departmentId;
        }

        ViewBag.Departments = new SelectList(
            departments,
            "Id",
            "Name",
            selectedDepartmentId);
    }
}