using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GBBassetManagementSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserArchiveFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedDate",
                table: "AspNetUsers",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "AspNetUsers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArchivedDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "AspNetUsers");
        }
    }
}
