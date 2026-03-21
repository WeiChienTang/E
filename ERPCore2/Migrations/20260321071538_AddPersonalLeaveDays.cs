using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonalLeaveDays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PersonalLeaveDays",
                table: "PayrollRecords",
                type: "decimal(5,1)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PersonalLeaveDays",
                table: "MonthlyAttendanceSummaries",
                type: "decimal(5,1)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PersonalLeaveDays",
                table: "PayrollRecords");

            migrationBuilder.DropColumn(
                name: "PersonalLeaveDays",
                table: "MonthlyAttendanceSummaries");
        }
    }
}
