using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "GroupsChange",
                table: "Documents",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "GroupsView",
                table: "Documents",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "UsersChange",
                table: "Documents",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "UsersView",
                table: "Documents",
                type: "text[]",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GroupsChange",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "GroupsView",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "UsersChange",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "UsersView",
                table: "Documents");
        }
    }
}
