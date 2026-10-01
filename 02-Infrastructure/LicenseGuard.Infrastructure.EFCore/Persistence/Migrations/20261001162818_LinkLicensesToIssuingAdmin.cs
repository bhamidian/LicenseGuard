using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LicenseGuard.Infrastructure.EFCore.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LinkLicensesToIssuingAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Licenses_AdminId",
                table: "Licenses",
                column: "AdminId");

            migrationBuilder.AddForeignKey(
                name: "FK_Licenses_Users_AdminId",
                table: "Licenses",
                column: "AdminId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Licenses_Users_AdminId",
                table: "Licenses");

            migrationBuilder.DropIndex(
                name: "IX_Licenses_AdminId",
                table: "Licenses");

        }
    }
}
