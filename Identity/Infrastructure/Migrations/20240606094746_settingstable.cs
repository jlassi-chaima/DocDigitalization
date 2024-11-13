using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class settingstable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UISettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Tour_complete = table.Column<bool>(type: "boolean", nullable: false),
                    DocumentListSize = table.Column<int>(type: "integer", nullable: false),
                    DarkMode_use_system = table.Column<bool>(type: "boolean", nullable: false),
                    DarkMode_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    DarkMode_thumb_inverted = table.Column<bool>(type: "boolean", nullable: false),
                    Notes_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    Language = table.Column<string>(type: "text", nullable: false),
                    Default_view_users = table.Column<string[]>(type: "text[]", nullable: false),
                    Default_view_groups = table.Column<List<Guid>>(type: "uuid[]", nullable: false),
                    Default_edit_users = table.Column<string[]>(type: "text[]", nullable: false),
                    Default_edit_groups = table.Column<List<Guid>>(type: "uuid[]", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UISettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UISettings_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_UISettings_UserId",
                table: "UISettings",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UISettings");
        }
    }
}
