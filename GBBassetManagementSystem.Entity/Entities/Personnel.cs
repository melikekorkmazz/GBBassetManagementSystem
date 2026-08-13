using System.ComponentModel.DataAnnotations;
using GBBassetManagementSystem.Core.Entities;

namespace GBBassetManagementSystem.Entity.Entities;

public class Personnel : EntityBase
{
    [Required(ErrorMessage = "FirstNameRequired")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "LastNameRequired")]
    public string LastName { get; set; } = string.Empty;

    [Display(Name = "RegistrationNumber")]
    [Required(ErrorMessage = "RegistrationNumberRequired")]
    [RegularExpression(
        @"^[0-9]+$",
        ErrorMessage = "RegistrationNumberNumbersOnly")]
    public string RegistrationNumber { get; set; } = string.Empty;

    [Display(Name = "NationalIdentityNumber")]
    [Required(ErrorMessage = "NationalIdentityNumberRequired")]
    [RegularExpression(
        @"^[0-9]{11}$",
        ErrorMessage = "NationalIdentityNumberLength")]
    public string NationalIdentityNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "EmailRequired")]
    [EmailAddress(ErrorMessage = "InvalidEmail")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "PhoneNumber")]
    [Required(ErrorMessage = "PhoneNumberRequired")]
    [RegularExpression(
        @"^[0-9]{11}$",
        ErrorMessage = "PhoneNumberLength")]
    public string PhoneNumber { get; set; } = string.Empty;

    // Guid is not nullable because every personnel must belong
    // to a department. Department selection is validated
    // in the controller.
    public Guid DepartmentId { get; set; }

    public Department? Department { get; set; }
}