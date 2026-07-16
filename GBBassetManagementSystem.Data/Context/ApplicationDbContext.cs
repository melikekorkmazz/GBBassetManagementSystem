using GBBassetManagementSystem.Entity.Entities;
using Microsoft.EntityFrameworkCore;

namespace GBBassetManagementSystem.Data.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Department> Departments { get; set; } = null!;
    public DbSet<Personnel> Personnel { get; set; } = null!;
    public DbSet<Asset> Assets { get; set; } = null!;
    public DbSet<AssetAssignment> AssetAssignments { get; set; } = null!;
    public DbSet<AssetReturn> AssetReturns { get; set; } = null!;
    public DbSet<Room> Rooms { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Asset>()
            .Property(asset => asset.PurchasePrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<AssetAssignment>()
            .HasOne(assignment => assignment.Personnel)
            .WithMany()
            .HasForeignKey(assignment => assignment.PersonnelId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AssetAssignment>()
            .HasOne(assignment => assignment.Room)
            .WithMany()
            .HasForeignKey(assignment => assignment.RoomId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}