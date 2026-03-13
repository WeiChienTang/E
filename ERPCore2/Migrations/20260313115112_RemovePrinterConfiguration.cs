using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class RemovePrinterConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReportPrintConfigurations_PrinterConfigurations_PrinterConfigurationId",
                table: "ReportPrintConfigurations");

            migrationBuilder.DropTable(
                name: "PrinterConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_ReportPrintConfigurations_PrinterConfigurationId",
                table: "ReportPrintConfigurations");

            migrationBuilder.DropColumn(
                name: "PrinterConfigurationId",
                table: "ReportPrintConfigurations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PrinterConfigurationId",
                table: "ReportPrintConfigurations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PrinterConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ConnectionType = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsDraft = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UsbPort = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrinterConfigurations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportPrintConfigurations_PrinterConfigurationId",
                table: "ReportPrintConfigurations",
                column: "PrinterConfigurationId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReportPrintConfigurations_PrinterConfigurations_PrinterConfigurationId",
                table: "ReportPrintConfigurations",
                column: "PrinterConfigurationId",
                principalTable: "PrinterConfigurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
