using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddCodeSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CodeSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModuleKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ModuleDisplayName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsAutoCode = table.Column<bool>(type: "bit", nullable: false),
                    Prefix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FormatTemplate = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CurrentSeq = table.Column<int>(type: "int", nullable: false),
                    CurrentYear = table.Column<int>(type: "int", nullable: true),
                    CurrentMonth = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
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
                    table.PrimaryKey("PK_CodeSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CodeSettings_ModuleKey",
                table: "CodeSettings",
                column: "ModuleKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CodeSettings");
        }
    }
}
