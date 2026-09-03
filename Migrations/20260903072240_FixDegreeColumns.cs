using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacultyInformationSystem_FIS_.Migrations
{
    /// <inheritdoc />
    public partial class FixDegreeColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Degrees",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "Degrees",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewComment",
                table: "Degrees",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Degrees",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Degrees");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "Degrees");

            migrationBuilder.DropColumn(
                name: "ReviewComment",
                table: "Degrees");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Degrees");
        }
    }
}