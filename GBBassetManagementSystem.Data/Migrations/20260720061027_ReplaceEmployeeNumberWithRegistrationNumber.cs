using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GBBassetManagementSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceEmployeeNumberWithRegistrationNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EmployeeNumber",
                table: "Personnel",
                newName: "RegistrationNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RegistrationNumber",
                table: "Personnel",
                newName: "EmployeeNumber");
        }
    }
}
