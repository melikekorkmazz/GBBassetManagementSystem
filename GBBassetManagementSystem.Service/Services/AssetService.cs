using GBBassetManagementSystem.Data.Context;
using GBBassetManagementSystem.Entity.Entities;
using GBBassetManagementSystem.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GBBassetManagementSystem.Service.Services;

public class AssetService : IAssetService
{
    private readonly ApplicationDbContext _context;

    public AssetService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Asset>> GetAllAsync()
    {
        return await _context.Assets
            .Include(asset => asset.Category)
            .OrderBy(asset => asset.AssetCode)
            .ToListAsync();
    }

    public async Task<Asset?> GetByIdAsync(Guid id)
    {
        return await _context.Assets
            .Include(asset => asset.Category)
            .FirstOrDefaultAsync(asset => asset.Id == id);
    }

    public async Task AddAsync(Asset asset)
    {
        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Asset asset)
    {
        var existingAsset = await _context.Assets.FindAsync(asset.Id);

        if (existingAsset is null)
        {
            throw new KeyNotFoundException("Asset was not found.");
        }

        existingAsset.AssetCode = asset.AssetCode;
        existingAsset.Name = asset.Name;
        existingAsset.Brand = asset.Brand;
        existingAsset.Model = asset.Model;
        existingAsset.SerialNumber = asset.SerialNumber;
        existingAsset.PurchaseDate = asset.PurchaseDate;
        existingAsset.PurchasePrice = asset.PurchasePrice;
        existingAsset.Status = asset.Status;
        existingAsset.WarrantyExpirationDate = asset.WarrantyExpirationDate;
        existingAsset.Notes = asset.Notes;
        existingAsset.CategoryId = asset.CategoryId;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var asset = await _context.Assets.FindAsync(id);

        if (asset is null)
        {
            throw new KeyNotFoundException("Asset was not found.");
        }

        _context.Assets.Remove(asset);
        await _context.SaveChangesAsync();
    }
}