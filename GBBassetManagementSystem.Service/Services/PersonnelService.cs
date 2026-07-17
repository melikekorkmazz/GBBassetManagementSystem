using GBBassetManagementSystem.Data.Context;
using GBBassetManagementSystem.Entity.Entities;
using GBBassetManagementSystem.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GBBassetManagementSystem.Service.Services;

public class PersonnelService : IPersonnelService
{
    private readonly ApplicationDbContext _context;

    public PersonnelService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Personnel>> GetAllAsync()
    {
        return await _context.Personnel
            .Include(personnel => personnel.Department)
            .OrderBy(personnel => personnel.FirstName)
            .ThenBy(personnel => personnel.LastName)
            .ToListAsync();
    }

    public async Task<Personnel?> GetByIdAsync(Guid id)
    {
        return await _context.Personnel
            .Include(personnel => personnel.Department)
            .FirstOrDefaultAsync(personnel => personnel.Id == id);
    }

    public async Task AddAsync(Personnel personnel)
    {
        _context.Personnel.Add(personnel);
        await _context.SaveChangesAsync();
    }

   public async Task UpdateAsync(Personnel personnel)
{
    var existingPersonnel =
        await _context.Personnel.FindAsync(personnel.Id);

    if (existingPersonnel is null)
    {
        throw new KeyNotFoundException("Personnel was not found.");
    }

    existingPersonnel.FirstName = personnel.FirstName;
    existingPersonnel.LastName = personnel.LastName;
    existingPersonnel.RegistrationNumber =personnel.RegistrationNumber;   
    existingPersonnel.NationalIdentityNumber = personnel.NationalIdentityNumber;
    existingPersonnel.Email = personnel.Email;
    existingPersonnel.PhoneNumber = personnel.PhoneNumber;
    existingPersonnel.DepartmentId = personnel.DepartmentId;

    await _context.SaveChangesAsync();
}

    public async Task DeleteAsync(Guid id)
    {
        var personnel = await _context.Personnel.FindAsync(id);

        if (personnel is null)
        {
            throw new KeyNotFoundException("Personnel was not found.");
        }

        _context.Personnel.Remove(personnel);
        await _context.SaveChangesAsync();
    }
}