using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class 移除Number屬性 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductionSchedules_ScheduleNumber",
                table: "ProductionSchedules");

            migrationBuilder.DropColumn(
                name: "ScheduleNumber",
                table: "ProductionSchedules");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionSchedules_Code",
                table: "ProductionSchedules",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductionSchedules_Code",
                table: "ProductionSchedules");

            migrationBuilder.AddColumn<string>(
                name: "ScheduleNumber",
                table: "ProductionSchedules",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionSchedules_ScheduleNumber",
                table: "ProductionSchedules",
                column: "ScheduleNumber",
                unique: true);
        }
    }
}
