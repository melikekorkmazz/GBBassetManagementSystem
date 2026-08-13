using System.ComponentModel.DataAnnotations;
using GBBassetManagementSystem.Core.Entities;

namespace GBBassetManagementSystem.Entity.Entities;

public class Category : EntityBase
{
    [Display(Name = "CategoryName")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "CategoryCode")]
    public string Code { get; set; } = string.Empty;

    [Display(Name = "Description")]
    public string? Description { get; set; }
}