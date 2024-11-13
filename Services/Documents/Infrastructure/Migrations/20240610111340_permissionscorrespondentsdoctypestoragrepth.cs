using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class permissionscorrespondentsdoctypestoragrepth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "GroupsChange",
                table: "StoragePaths",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "GroupsView",
                table: "StoragePaths",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "UsersChange",
                table: "StoragePaths",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "UsersView",
                table: "StoragePaths",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "GroupsChange",
                table: "DocumentTypes",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "GroupsView",
                table: "DocumentTypes",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "UsersChange",
                table: "DocumentTypes",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "UsersView",
                table: "DocumentTypes",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "GroupsChange",
                table: "Correspondents",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "GroupsView",
                table: "Correspondents",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "UsersChange",
                table: "Correspondents",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "UsersView",
                table: "Correspondents",
                type: "text[]",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GroupsChange",
                table: "StoragePaths");

            migrationBuilder.DropColumn(
                name: "GroupsView",
                table: "StoragePaths");

            migrationBuilder.DropColumn(
                name: "UsersChange",
                table: "StoragePaths");

            migrationBuilder.DropColumn(
                name: "UsersView",
                table: "StoragePaths");

            migrationBuilder.DropColumn(
                name: "GroupsChange",
                table: "DocumentTypes");

            migrationBuilder.DropColumn(
                name: "GroupsView",
                table: "DocumentTypes");

            migrationBuilder.DropColumn(
                name: "UsersChange",
                table: "DocumentTypes");

            migrationBuilder.DropColumn(
                name: "UsersView",
                table: "DocumentTypes");

            migrationBuilder.DropColumn(
                name: "GroupsChange",
                table: "Correspondents");

            migrationBuilder.DropColumn(
                name: "GroupsView",
                table: "Correspondents");

            migrationBuilder.DropColumn(
                name: "UsersChange",
                table: "Correspondents");

            migrationBuilder.DropColumn(
                name: "UsersView",
                table: "Correspondents");
        }
    }
}
