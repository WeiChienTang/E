using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddMonthlyAttendanceSummary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MonthlyAttendanceSummaries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    ScheduledWorkDays = table.Column<decimal>(type: "decimal(5,1)", nullable: false),
                    ActualWorkDays = table.Column<decimal>(type: "decimal(5,1)", nullable: false),
                    AbsentDays = table.Column<decimal>(type: "decimal(5,1)", nullable: false),
                    SickLeaveDays = table.Column<decimal>(type: "decimal(5,1)", nullable: false),
                    OvertimeHours1 = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    OvertimeHours2 = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    HolidayOvertimeHours = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    NationalHolidayHours = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyAttendanceSummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonthlyAttendanceSummaries_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyAttendanceSummaries_Year_Month",
                table: "MonthlyAttendanceSummaries",
                columns: new[] { "Year", "Month" });

            migrationBuilder.CreateIndex(
                name: "UX_MonthlyAttendance_Employee_Period",
                table: "MonthlyAttendanceSummaries",
                columns: new[] { "EmployeeId", "Year", "Month" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MonthlyAttendanceSummaries");
        }
    }
}
