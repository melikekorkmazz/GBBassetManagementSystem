using System.ComponentModel.DataAnnotations;
using GBBassetManagementSystem.Core.Entities;

namespace GBBassetManagementSystem.Entity.Entities;

public class Personnel : EntityBase
{
    [Required(ErrorMessage = "First name is required.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    public string LastName { get; set; } = string.Empty;

    [Display(Name = "Registration Number")]
    [Required(ErrorMessage = "Registration number is required.")]
    [RegularExpression(
        @"^[0-9]+$",
        ErrorMessage = "Registration number must contain only numbers.")]
    public string RegistrationNumber { get; set; } = string.Empty;

    [Display(Name = "National Identity Number")]
    [Required(ErrorMessage = "National identity number is required.")]
    [RegularExpression(
        @"^[0-9]{11}$",
        ErrorMessage = "National identity number must contain exactly 11 digits.")]
    public string NationalIdentityNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Phone Number")]
    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(
        @"^[0-9]{11}$",
        ErrorMessage = "Phone number must contain exactly 11 digits.")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Display(Name = "Department")]
    [Required(ErrorMessage = "Department selection is required.")]
    public Guid DepartmentId { get; set; }

    public Department? Department { get; set; }
}