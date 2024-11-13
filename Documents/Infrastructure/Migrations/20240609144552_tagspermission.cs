using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class tagspermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "GroupsChange",
                table: "Tags",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "GroupsView",
                table: "Tags",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "UsersChange",
                table: "Tags",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "UsersView",
                table: "Tags",
                type: "text[]",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GroupsChange",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "GroupsView",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "UsersChange",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "UsersView",
                table: "Tags");
        }
    }
}
