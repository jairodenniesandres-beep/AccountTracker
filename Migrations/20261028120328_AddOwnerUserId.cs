using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GmailAccounts_EmailAddress",
                table: "GmailAccounts");

            migrationBuilder.AddColumn<string>(
                name: "OwnerUserId",
                table: "GmailAccounts",
                type: "TEXT",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_GmailAccounts_OwnerUserId",
                table: "GmailAccounts",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GmailAccounts_OwnerUserId_EmailAddress",
                table: "GmailAccounts",
                columns: new[] { "OwnerUserId", "EmailAddress" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GmailAccounts_AspNetUsers_OwnerUserId",
                table: "GmailAccounts",
                column: "OwnerUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GmailAccounts_AspNetUsers_OwnerUserId",
                table: "GmailAccounts");

            migrationBuilder.DropIndex(
                name: "IX_GmailAccounts_OwnerUserId",
                table: "GmailAccounts");

            migrationBuilder.DropIndex(
                name: "IX_GmailAccounts_OwnerUserId_EmailAddress",
                table: "GmailAccounts");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "GmailAccounts");

            migrationBuilder.CreateIndex(
                name: "IX_GmailAccounts_EmailAddress",
                table: "GmailAccounts",
                column: "EmailAddress",
                unique: true);
        }
    }
}
