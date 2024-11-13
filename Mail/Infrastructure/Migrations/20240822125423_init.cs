using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MailAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IMAP_Server = table.Column<string>(type: "text", nullable: false),
                    IMAP_Port = table.Column<int>(type: "integer", nullable: false),
                    IMAP_Security = table.Column<int>(type: "integer", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    Is_token = table.Column<bool>(type: "boolean", nullable: false),
                    Character_set = table.Column<string>(type: "text", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MailRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Folder = table.Column<string>(type: "text", nullable: false),
                    Maximum_age = table.Column<long>(type: "bigint", nullable: false),
                    Attachment_type = table.Column<int>(type: "integer", nullable: false),
                    Consumption_scope = table.Column<int>(type: "integer", nullable: false),
                    Order = table.Column<long>(type: "bigint", nullable: false),
                    Filter_from = table.Column<string>(type: "text", nullable: true),
                    Filter_to = table.Column<string>(type: "text", nullable: true),
                    Filter_subject = table.Column<string>(type: "text", nullable: true),
                    Filter_body = table.Column<string>(type: "text", nullable: true),
                    Filter_attachment_filename = table.Column<string>(type: "text", nullable: true),
                    Action = table.Column<int>(type: "integer", nullable: true),
                    Assign_title_from = table.Column<int>(type: "integer", nullable: true),
                    Action_parameter = table.Column<string>(type: "text", nullable: true),
                    Assign_tags = table.Column<List<Guid>>(type: "uuid[]", nullable: true),
                    Assign_document_type = table.Column<Guid>(type: "uuid", nullable: true),
                    Assign_correspondent_from = table.Column<int>(type: "integer", nullable: true),
                    Assign_correspondent = table.Column<Guid>(type: "uuid", nullable: true),
                    Assign_owner_from_rule = table.Column<bool>(type: "boolean", nullable: true),
                    Owner = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MailRules_MailAccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "MailAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MailRules_AccountId",
                table: "MailRules",
                column: "AccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MailRules");

            migrationBuilder.DropTable(
                name: "MailAccounts");
        }
    }
}
