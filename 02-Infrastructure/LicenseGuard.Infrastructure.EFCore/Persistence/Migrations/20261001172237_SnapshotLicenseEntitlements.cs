using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LicenseGuard.Infrastructure.EFCore.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SnapshotLicenseEntitlements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "Licenses",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "IssuedExpirationDate",
                table: "Licenses",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "PlanId",
                table: "Licenses",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "PolicyVersion",
                table: "Licenses",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductId",
                table: "Licenses",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.Sql("UPDATE `Licenses` AS l INNER JOIN `Subscriptions` AS s ON s.`Id` = l.`SubscriptionId` SET l.`CustomerId` = s.`CustomerId`, l.`ProductId` = s.`ProductId`, l.`PlanId` = s.`PlanId`, l.`PolicyVersion` = '1', l.`IssuedExpirationDate` = l.`ExpirationDate`");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Licenses");

            migrationBuilder.DropColumn(
                name: "IssuedExpirationDate",
                table: "Licenses");

            migrationBuilder.DropColumn(
                name: "PlanId",
                table: "Licenses");

            migrationBuilder.DropColumn(
                name: "PolicyVersion",
                table: "Licenses");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "Licenses");
        }
    }
}
