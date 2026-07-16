using GBBassetManagementSystem.Data.Context;
using GBBassetManagementSystem.Entity.Entities;
using GBBassetManagementSystem.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GBBassetManagementSystem.Service.Services;

public class RoomService : IRoomService
{
    private readonly ApplicationDbContext _context;

    public RoomService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Room>> GetAllAsync()
    {
        return await _context.Rooms
            .Include(room => room.Department)
            .OrderBy(room => room.Building)
            .ThenBy(room => room.Floor)
            .ThenBy(room => room.RoomNumber)
            .ToListAsync();
    }

    public async Task<Room?> GetByIdAsync(Guid id)
    {
        return await _context.Rooms
            .Include(room => room.Department)
            .FirstOrDefaultAsync(room => room.Id == id);
    }

    public async Task AddAsync(Room room)
    {
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Room room)
    {
        var existingRoom = await _context.Rooms.FindAsync(room.Id);

        if (existingRoom is null)
        {
            throw new KeyNotFoundException("Room was not found.");
        }

        existingRoom.Name = room.Name;
        existingRoom.RoomNumber = room.RoomNumber;
        existingRoom.Floor = room.Floor;
        existingRoom.Building = room.Building;
        existingRoom.DepartmentId = room.DepartmentId;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var room = await _context.Rooms.FindAsync(id);

        if (room is null)
        {
            throw new KeyNotFoundException("Room was not found.");
        }

        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();
    }
}