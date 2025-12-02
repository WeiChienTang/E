using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesOrderCompositionDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuotationCompositionDetails_Products_ComponentProductId",
                table: "QuotationCompositionDetails");

            migrationBuilder.CreateTable(
                name: "SalesOrderCompositionDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesOrderDetailId = table.Column<int>(type: "int", nullable: false),
                    ComponentProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: true),
                    ComponentCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
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
                    table.PrimaryKey("PK_SalesOrderCompositionDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesOrderCompositionDetails_Products_ComponentProductId",
                        column: x => x.ComponentProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesOrderCompositionDetails_SalesOrderDetails_SalesOrderDetailId",
                        column: x => x.SalesOrderDetailId,
                        principalTable: "SalesOrderDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesOrderCompositionDetails_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderCompositionDetails_ComponentProductId",
                table: "SalesOrderCompositionDetails",
                column: "ComponentProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderCompositionDetails_SalesOrderDetailId_ComponentProductId",
                table: "SalesOrderCompositionDetails",
                columns: new[] { "SalesOrderDetailId", "ComponentProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderCompositionDetails_UnitId",
                table: "SalesOrderCompositionDetails",
                column: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuotationCompositionDetails_Products_ComponentProductId",
                table: "QuotationCompositionDetails",
                column: "ComponentProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuotationCompositionDetails_Products_ComponentProductId",
                table: "QuotationCompositionDetails");

            migrationBuilder.DropTable(
                name: "SalesOrderCompositionDetails");

            migrationBuilder.AddForeignKey(
                name: "FK_QuotationCompositionDetails_Products_ComponentProductId",
                table: "QuotationCompositionDetails",
                column: "ComponentProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
