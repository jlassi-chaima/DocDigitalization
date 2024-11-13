using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class editKeysColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "key",
                table: "Documents",
                newName: "Key");

            migrationBuilder.RenameColumn(
                name: "iv",
                table: "Documents",
                newName: "Iv");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Key",
                table: "Documents",
                newName: "key");

            migrationBuilder.RenameColumn(
                name: "Iv",
                table: "Documents",
                newName: "iv");
        }
    }
}
