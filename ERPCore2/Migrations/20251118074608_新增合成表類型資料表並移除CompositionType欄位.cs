using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class 新增合成表類型資料表並移除CompositionType欄位 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompositionType",
                table: "ProductCompositions");

            migrationBuilder.AddColumn<int>(
                name: "CompositionCategoryId",
                table: "ProductCompositions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CompositionCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompositionCategories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductCompositions_CompositionCategoryId",
                table: "ProductCompositions",
                column: "CompositionCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CompositionCategories_Name",
                table: "CompositionCategories",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCompositions_CompositionCategories_CompositionCategoryId",
                table: "ProductCompositions",
                column: "CompositionCategoryId",
                principalTable: "CompositionCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductCompositions_CompositionCategories_CompositionCategoryId",
                table: "ProductCompositions");

            migrationBuilder.DropTable(
                name: "CompositionCategories");

            migrationBuilder.DropIndex(
                name: "IX_ProductCompositions_CompositionCategoryId",
                table: "ProductCompositions");

            migrationBuilder.DropColumn(
                name: "CompositionCategoryId",
                table: "ProductCompositions");

            migrationBuilder.AddColumn<int>(
                name: "CompositionType",
                table: "ProductCompositions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
