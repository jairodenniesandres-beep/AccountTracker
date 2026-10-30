using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountTracker.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GmailAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmailAddress = table.Column<string>(type: "TEXT", maxLength: 254, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    EncryptedPassword = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    RecoveryEmail = table.Column<string>(type: "TEXT", maxLength: 254, nullable: true),
                    RecoveryPhone = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    TwoFactorMethod = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Purpose = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DateCreated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastPasswordChange = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastLogin = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Version = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GmailAccounts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GmailAccounts_EmailAddress",
                table: "GmailAccounts",
                column: "EmailAddress",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GmailAccounts");
        }
    }
}
