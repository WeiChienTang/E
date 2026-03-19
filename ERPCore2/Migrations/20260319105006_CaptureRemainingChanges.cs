using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class CaptureRemainingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TotalWorkHours",
                table: "PayrollRecords",
                type: "decimal(7,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalWorkHours",
                table: "MonthlyAttendanceSummaries",
                type: "decimal(7,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "AttendanceDailyRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    WorkHours = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    OvertimeHours1 = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    OvertimeHours2 = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    HolidayOvertimeHours = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    NationalHolidayHours = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceDailyRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttendanceDailyRecords_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceDailyRecords_Date",
                table: "AttendanceDailyRecords",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "UX_AttendanceDaily_Employee_Date",
                table: "AttendanceDailyRecords",
                columns: new[] { "EmployeeId", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttendanceDailyRecords");

            migrationBuilder.DropColumn(
                name: "TotalWorkHours",
                table: "PayrollRecords");

            migrationBuilder.DropColumn(
                name: "TotalWorkHours",
                table: "MonthlyAttendanceSummaries");
        }
    }
}
