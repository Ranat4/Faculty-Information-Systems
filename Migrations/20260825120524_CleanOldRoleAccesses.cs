using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacultyInformationSystem_FIS_.Migrations
{
    /// <inheritdoc />
    public partial class CleanOldRoleAccesses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove old RoleAccess records that were created
            // before Module was introduced.
            migrationBuilder.Sql(
                "DELETE FROM RoleAccesses WHERE Module IS NULL"
            );

            migrationBuilder.DropIndex(
                name: "IX_RoleAccesses_RoleId_Module_Access",
                table: "RoleAccesses");

            migrationBuilder.AlterColumn<int>(
                name: "Module",
                table: "RoleAccesses",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleAccesses_RoleId_Module_Access",
                table: "RoleAccesses",
                columns: new[] { "RoleId", "Module", "Access" },
                unique: true);
        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RoleAccesses_RoleId_Module_Access",
                table: "RoleAccesses");

            migrationBuilder.AlterColumn<int>(
                name: "Module",
                table: "RoleAccesses",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_RoleAccesses_RoleId_Module_Access",
                table: "RoleAccesses",
                columns: new[] { "RoleId", "Module", "Access" },
                unique: true,
                filter: "[Module] IS NOT NULL");
        }
    }
}
