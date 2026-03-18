using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddManufacturingOrderCodeAndResponsibleEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ResponsibleEmployeeId",
                table: "ProductionScheduleItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionScheduleItems_Code",
                table: "ProductionScheduleItems",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionScheduleItems_ResponsibleEmployeeId",
                table: "ProductionScheduleItems",
                column: "ResponsibleEmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionScheduleItems_Employees_ResponsibleEmployeeId",
                table: "ProductionScheduleItems",
                column: "ResponsibleEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionScheduleItems_Employees_ResponsibleEmployeeId",
                table: "ProductionScheduleItems");

            migrationBuilder.DropIndex(
                name: "IX_ProductionScheduleItems_Code",
                table: "ProductionScheduleItems");

            migrationBuilder.DropIndex(
                name: "IX_ProductionScheduleItems_ResponsibleEmployeeId",
                table: "ProductionScheduleItems");

            migrationBuilder.DropColumn(
                name: "ResponsibleEmployeeId",
                table: "ProductionScheduleItems");
        }
    }
}
