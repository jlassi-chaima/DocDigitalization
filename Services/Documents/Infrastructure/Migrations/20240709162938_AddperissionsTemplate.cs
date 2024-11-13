using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddperissionsTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<Guid>>(
                name: "Assign_change_groups",
                table: "Templates",
                type: "uuid[]",
                nullable: true);

            migrationBuilder.AddColumn<List<Guid>>(
                name: "Assign_change_users",
                table: "Templates",
                type: "uuid[]",
                nullable: true);

            migrationBuilder.AddColumn<List<Guid>>(
                name: "Assign_view_groups",
                table: "Templates",
                type: "uuid[]",
                nullable: true);

            migrationBuilder.AddColumn<List<Guid>>(
                name: "Assign_view_users",
                table: "Templates",
                type: "uuid[]",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Assign_change_groups",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "Assign_change_users",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "Assign_view_groups",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "Assign_view_users",
                table: "Templates");
        }
    }
}
