using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddReportPrintConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReportPrintConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReportName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PrinterConfigurationId = table.Column<int>(type: "int", nullable: true),
                    PaperSettingId = table.Column<int>(type: "int", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportPrintConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportPrintConfigurations_PaperSettings_PaperSettingId",
                        column: x => x.PaperSettingId,
                        principalTable: "PaperSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ReportPrintConfigurations_PrinterConfigurations_PrinterConfigurationId",
                        column: x => x.PrinterConfigurationId,
                        principalTable: "PrinterConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportPrintConfigurations_PaperSettingId",
                table: "ReportPrintConfigurations",
                column: "PaperSettingId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportPrintConfigurations_PrinterConfigurationId",
                table: "ReportPrintConfigurations",
                column: "PrinterConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportPrintConfigurations_ReportType",
                table: "ReportPrintConfigurations",
                column: "ReportType",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportPrintConfigurations");
        }
    }
}
