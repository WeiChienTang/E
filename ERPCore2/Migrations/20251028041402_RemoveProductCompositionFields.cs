using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProductCompositionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductCompositions_EffectiveDate",
                table: "ProductCompositions");

            migrationBuilder.DropIndex(
                name: "IX_ProductCompositions_ParentProductId_IsDefault",
                table: "ProductCompositions");

            migrationBuilder.DropIndex(
                name: "IX_ProductCompositions_ParentProductId_Version",
                table: "ProductCompositions");

            migrationBuilder.DropColumn(
                name: "BaseQuantity",
                table: "ProductCompositions");

            migrationBuilder.DropColumn(
                name: "EffectiveDate",
                table: "ProductCompositions");

            migrationBuilder.DropColumn(
                name: "EstimatedCost",
                table: "ProductCompositions");

            migrationBuilder.DropColumn(
                name: "EstimatedTime",
                table: "ProductCompositions");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "ProductCompositions");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "ProductCompositions");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "ProductCompositions");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCompositions_ParentProductId",
                table: "ProductCompositions",
                column: "ParentProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductCompositions_ParentProductId",
                table: "ProductCompositions");

            migrationBuilder.AddColumn<decimal>(
                name: "BaseQuantity",
                table: "ProductCompositions",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveDate",
                table: "ProductCompositions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedCost",
                table: "ProductCompositions",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstimatedTime",
                table: "ProductCompositions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryDate",
                table: "ProductCompositions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "ProductCompositions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "ProductCompositions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductCompositions_EffectiveDate",
                table: "ProductCompositions",
                column: "EffectiveDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCompositions_ParentProductId_IsDefault",
                table: "ProductCompositions",
                columns: new[] { "ParentProductId", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductCompositions_ParentProductId_Version",
                table: "ProductCompositions",
                columns: new[] { "ParentProductId", "Version" },
                unique: true,
                filter: "[Version] IS NOT NULL");
        }
    }
}
