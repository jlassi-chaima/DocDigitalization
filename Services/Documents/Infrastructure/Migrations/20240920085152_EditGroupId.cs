using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EditGroupId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ArchiveSerialNumbers",
                table: "ArchiveSerialNumbers");

            migrationBuilder.AlterColumn<string>(
                name: "Owner",
                table: "ArchiveSerialNumbers",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArchiveSerialNumbers",
                table: "ArchiveSerialNumbers",
                column: "GroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ArchiveSerialNumbers",
                table: "ArchiveSerialNumbers");

            migrationBuilder.AlterColumn<string>(
                name: "Owner",
                table: "ArchiveSerialNumbers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArchiveSerialNumbers",
                table: "ArchiveSerialNumbers",
                column: "Id");
        }
    }
}
