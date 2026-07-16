using GBBassetManagementSystem.Data.Context;
using GBBassetManagementSystem.Entity.Entities;
using GBBassetManagementSystem.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GBBassetManagementSystem.Service.Services;

public class DepartmentService : IDepartmentService
{
    private readonly ApplicationDbContext _context;

    public DepartmentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Department>> GetAllAsync()
    {
        return await _context.Departments
            .OrderBy(department => department.Name)
            .ToListAsync();
    }

    public async Task<Department?> GetByIdAsync(Guid id)
    {
        return await _context.Departments.FindAsync(id);
    }

    public async Task AddAsync(Department department)
    {
        _context.Departments.Add(department);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Department department)
    {
        var existingDepartment =
            await _context.Departments.FindAsync(department.Id);

        if (existingDepartment is null)
        {
            throw new KeyNotFoundException("Department was not found.");
        }

        existingDepartment.Name = department.Name;
        existingDepartment.Description = department.Description;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var department = await _context.Departments.FindAsync(id);

        if (department is null)
        {
            throw new KeyNotFoundException("Department was not found.");
        }

        _context.Departments.Remove(department);
        await _context.SaveChangesAsync();
    }
}