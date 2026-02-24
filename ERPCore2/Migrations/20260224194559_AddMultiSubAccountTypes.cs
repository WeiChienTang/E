using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiSubAccountTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerAdvanceSubAccountParentCode",
                table: "SystemParameters",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerNoteSubAccountParentCode",
                table: "SystemParameters",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerReturnSubAccountParentCode",
                table: "SystemParameters",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SupplierAdvanceSubAccountParentCode",
                table: "SystemParameters",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SupplierNoteSubAccountParentCode",
                table: "SystemParameters",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SupplierReturnSubAccountParentCode",
                table: "SystemParameters",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SubAccountLinkType",
                table: "AccountItems",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerAdvanceSubAccountParentCode",
                table: "SystemParameters");

            migrationBuilder.DropColumn(
                name: "CustomerNoteSubAccountParentCode",
                table: "SystemParameters");

            migrationBuilder.DropColumn(
                name: "CustomerReturnSubAccountParentCode",
                table: "SystemParameters");

            migrationBuilder.DropColumn(
                name: "SupplierAdvanceSubAccountParentCode",
                table: "SystemParameters");

            migrationBuilder.DropColumn(
                name: "SupplierNoteSubAccountParentCode",
                table: "SystemParameters");

            migrationBuilder.DropColumn(
                name: "SupplierReturnSubAccountParentCode",
                table: "SystemParameters");

            migrationBuilder.DropColumn(
                name: "SubAccountLinkType",
                table: "AccountItems");
        }
    }
}
