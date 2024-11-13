using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class useridsettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UISettings_AspNetUsers_UserId",
                table: "UISettings");

            migrationBuilder.DropIndex(
                name: "IX_UISettings_UserId",
                table: "UISettings");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UISettings",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string[]>(
                name: "Permissions",
                table: "Groups",
                type: "text[]",
                nullable: true,
                oldClrType: typeof(int[]),
                oldType: "integer[]",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Superuser_status",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UISettings_UserId",
                table: "UISettings",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UISettings_AspNetUsers_UserId",
                table: "UISettings",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UISettings_AspNetUsers_UserId",
                table: "UISettings");

            migrationBuilder.DropIndex(
                name: "IX_UISettings_UserId",
                table: "UISettings");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UISettings",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int[]>(
                name: "Permissions",
                table: "Groups",
                type: "integer[]",
                nullable: true,
                oldClrType: typeof(string[]),
                oldType: "text[]",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Superuser_status",
                table: "AspNetUsers",
                type: "boolean",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.CreateIndex(
                name: "IX_UISettings_UserId",
                table: "UISettings",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UISettings_AspNetUsers_UserId",
                table: "UISettings",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
