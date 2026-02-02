using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class RemoveReportTypeFromReportPrintConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReportPrintConfigurations_ReportType",
                table: "ReportPrintConfigurations");

            migrationBuilder.DropColumn(
                name: "ReportType",
                table: "ReportPrintConfigurations");

            migrationBuilder.CreateIndex(
                name: "IX_ReportPrintConfigurations_ReportName",
                table: "ReportPrintConfigurations",
                column: "ReportName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReportPrintConfigurations_ReportName",
                table: "ReportPrintConfigurations");

            migrationBuilder.AddColumn<string>(
                name: "ReportType",
                table: "ReportPrintConfigurations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ReportPrintConfigurations_ReportType",
                table: "ReportPrintConfigurations",
                column: "ReportType",
                unique: true);
        }
    }
}
