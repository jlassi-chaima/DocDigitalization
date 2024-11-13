using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddperissionsTemplate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<List<string>>(
                name: "Assign_view_users",
                table: "Templates",
                type: "text[]",
                nullable: true,
                oldClrType: typeof(List<Guid>),
                oldType: "uuid[]",
                oldNullable: true);

            migrationBuilder.AlterColumn<List<string>>(
                name: "Assign_view_groups",
                table: "Templates",
                type: "text[]",
                nullable: true,
                oldClrType: typeof(List<Guid>),
                oldType: "uuid[]",
                oldNullable: true);

            migrationBuilder.AlterColumn<List<string>>(
                name: "Assign_change_users",
                table: "Templates",
                type: "text[]",
                nullable: true,
                oldClrType: typeof(List<Guid>),
                oldType: "uuid[]",
                oldNullable: true);

            migrationBuilder.AlterColumn<List<string>>(
                name: "Assign_change_groups",
                table: "Templates",
                type: "text[]",
                nullable: true,
                oldClrType: typeof(List<Guid>),
                oldType: "uuid[]",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<List<Guid>>(
                name: "Assign_view_users",
                table: "Templates",
                type: "uuid[]",
                nullable: true,
                oldClrType: typeof(List<string>),
                oldType: "text[]",
                oldNullable: true);

            migrationBuilder.AlterColumn<List<Guid>>(
                name: "Assign_view_groups",
                table: "Templates",
                type: "uuid[]",
                nullable: true,
                oldClrType: typeof(List<string>),
                oldType: "text[]",
                oldNullable: true);

            migrationBuilder.AlterColumn<List<Guid>>(
                name: "Assign_change_users",
                table: "Templates",
                type: "uuid[]",
                nullable: true,
                oldClrType: typeof(List<string>),
                oldType: "text[]",
                oldNullable: true);

            migrationBuilder.AlterColumn<List<Guid>>(
                name: "Assign_change_groups",
                table: "Templates",
                type: "uuid[]",
                nullable: true,
                oldClrType: typeof(List<string>),
                oldType: "text[]",
                oldNullable: true);
        }
    }
}
