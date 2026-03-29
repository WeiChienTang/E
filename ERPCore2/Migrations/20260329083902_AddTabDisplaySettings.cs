using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddTabDisplaySettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TabDisplaySettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TargetModule = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TabKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsVisible = table.Column<bool>(type: "bit", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDraft = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TabDisplaySettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TabDisplaySettings_TargetModule_TabKey",
                table: "TabDisplaySettings",
                columns: new[] { "TargetModule", "TabKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TabDisplaySettings");
        }
    }
}
