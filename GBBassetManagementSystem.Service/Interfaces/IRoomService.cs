using GBBassetManagementSystem.Entity.Entities;

namespace GBBassetManagementSystem.Service.Interfaces;

public interface IRoomService
{
    Task<List<Room>> GetAllAsync();

    Task<Room?> GetByIdAsync(Guid id);

    Task AddAsync(Room room);

    Task UpdateAsync(Room room);

    Task DeleteAsync(Guid id);
}