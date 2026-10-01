using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LicenseGuard.Infrastructure.EFCore.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SnapshotLicenseFeatureCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FeatureCodeSnapshot",
                table: "LicenseFeatures",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql("UPDATE `LicenseFeatures` AS lf INNER JOIN `Features` AS f ON f.`Id` = lf.`FeatureId` SET lf.`FeatureCodeSnapshot` = f.`Code`");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FeatureCodeSnapshot",
                table: "LicenseFeatures");
        }
    }
}
