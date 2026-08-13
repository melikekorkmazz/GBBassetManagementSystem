namespace GBBassetManagementSystem.Web.Models;

public class UserListViewModel
{
    public string Id { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string DepartmentName { get; set; } = "-";

    public string RoleName { get; set; } = string.Empty;
}