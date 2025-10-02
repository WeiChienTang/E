using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCompanyIdFromPrepayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prepayments_Companies_CompanyId",
                table: "Prepayments");

            migrationBuilder.DropIndex(
                name: "IX_Prepayments_CompanyId",
                table: "Prepayments");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Prepayments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Prepayments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Prepayments_CompanyId",
                table: "Prepayments",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Prepayments_Companies_CompanyId",
                table: "Prepayments",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
