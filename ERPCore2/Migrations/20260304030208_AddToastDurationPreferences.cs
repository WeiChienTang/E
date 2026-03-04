using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddToastDurationPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ToastErrorDurationMs",
                table: "EmployeePreferences",
                type: "int",
                nullable: false,
                defaultValue: 2000);

            migrationBuilder.AddColumn<int>(
                name: "ToastInfoDurationMs",
                table: "EmployeePreferences",
                type: "int",
                nullable: false,
                defaultValue: 2000);

            migrationBuilder.AddColumn<int>(
                name: "ToastSuccessDurationMs",
                table: "EmployeePreferences",
                type: "int",
                nullable: false,
                defaultValue: 2000);

            migrationBuilder.AddColumn<int>(
                name: "ToastWarningDurationMs",
                table: "EmployeePreferences",
                type: "int",
                nullable: false,
                defaultValue: 2000);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ToastErrorDurationMs",
                table: "EmployeePreferences");

            migrationBuilder.DropColumn(
                name: "ToastInfoDurationMs",
                table: "EmployeePreferences");

            migrationBuilder.DropColumn(
                name: "ToastSuccessDurationMs",
                table: "EmployeePreferences");

            migrationBuilder.DropColumn(
                name: "ToastWarningDurationMs",
                table: "EmployeePreferences");
        }
    }
}
