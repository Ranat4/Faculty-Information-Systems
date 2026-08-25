using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacultyInformationSystem_FIS_.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleManagementModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RoleAccesses_RoleId",
                table: "RoleAccesses");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "RoleAccesses",
                newName: "Id");

            migrationBuilder.AddColumn<int>(
                name: "Module",
                table: "RoleAccesses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PasswordResetCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetCodes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "RoleAccesses",
                columns: new[] { "Id", "Access", "Module", "RoleId" },
                values: new object[] { 8, 3, 0, 1 });

            migrationBuilder.CreateIndex(
                name: "IX_RoleAccesses_RoleId_Module_Access",
                table: "RoleAccesses",
                columns: new[] { "RoleId", "Module", "Access" },
                unique: true,
                filter: "[Module] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PasswordResetCodes");

            migrationBuilder.DropIndex(
                name: "IX_RoleAccesses_RoleId_Module_Access",
                table: "RoleAccesses");

            migrationBuilder.DeleteData(
                table: "RoleAccesses",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DropColumn(
                name: "Module",
                table: "RoleAccesses");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "RoleAccesses",
                newName: "ID");

            migrationBuilder.CreateIndex(
                name: "IX_RoleAccesses_RoleId",
                table: "RoleAccesses",
                column: "RoleId");
        }
    }
}
