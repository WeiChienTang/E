using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class CaptureRemainingModelChanges2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultPageSize",
                table: "EmployeePreferences");

            migrationBuilder.DropColumn(
                name: "DefaultReminderMinutes",
                table: "EmployeePreferences");

            migrationBuilder.DropColumn(
                name: "EnableCalendar",
                table: "EmployeePreferences");

            migrationBuilder.DropColumn(
                name: "EnableStickyNote",
                table: "EmployeePreferences");

            migrationBuilder.DropColumn(
                name: "ShortcutCalendar",
                table: "EmployeePreferences");

            migrationBuilder.DropColumn(
                name: "ShortcutPageSearch",
                table: "EmployeePreferences");

            migrationBuilder.DropColumn(
                name: "ShortcutQuickAction",
                table: "EmployeePreferences");

            migrationBuilder.DropColumn(
                name: "ShortcutReportSearch",
                table: "EmployeePreferences");

            migrationBuilder.DropColumn(
                name: "ShortcutStickyNotes",
                table: "EmployeePreferences");

            migrationBuilder.DropColumn(
                name: "ShowCalendarBadge",
                table: "EmployeePreferences");

            migrationBuilder.DropColumn(
                name: "ShowDisabledModules",
                table: "EmployeePreferences");

            migrationBuilder.DropColumn(
                name: "ShowNoteBadge",
                table: "EmployeePreferences");

            migrationBuilder.DropColumn(
                name: "ShowUnsavedChangesWarning",
                table: "EmployeePreferences");

            migrationBuilder.DropColumn(
                name: "Theme",
                table: "EmployeePreferences");

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

            migrationBuilder.DropColumn(
                name: "Zoom",
                table: "EmployeePreferences");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DefaultPageSize",
                table: "EmployeePreferences",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DefaultReminderMinutes",
                table: "EmployeePreferences",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "EnableCalendar",
                table: "EmployeePreferences",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableStickyNote",
                table: "EmployeePreferences",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ShortcutCalendar",
                table: "EmployeePreferences",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortcutPageSearch",
                table: "EmployeePreferences",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortcutQuickAction",
                table: "EmployeePreferences",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortcutReportSearch",
                table: "EmployeePreferences",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortcutStickyNotes",
                table: "EmployeePreferences",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShowCalendarBadge",
                table: "EmployeePreferences",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowDisabledModules",
                table: "EmployeePreferences",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowNoteBadge",
                table: "EmployeePreferences",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowUnsavedChangesWarning",
                table: "EmployeePreferences",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Theme",
                table: "EmployeePreferences",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ToastErrorDurationMs",
                table: "EmployeePreferences",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ToastInfoDurationMs",
                table: "EmployeePreferences",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ToastSuccessDurationMs",
                table: "EmployeePreferences",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ToastWarningDurationMs",
                table: "EmployeePreferences",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Zoom",
                table: "EmployeePreferences",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
