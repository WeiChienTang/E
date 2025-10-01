using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRedundantFieldsFromAccountsReceivableSetoffDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentNumber",
                table: "AccountsReceivableSetoffDetails");

            migrationBuilder.DropColumn(
                name: "DocumentType",
                table: "AccountsReceivableSetoffDetails");

            migrationBuilder.DropColumn(
                name: "IsFullyReceived",
                table: "AccountsReceivableSetoffDetails");

            migrationBuilder.DropColumn(
                name: "PreviousReceivedAmount",
                table: "AccountsReceivableSetoffDetails");

            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "AccountsReceivableSetoffDetails");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "AccountsReceivableSetoffDetails");

            migrationBuilder.DropColumn(
                name: "RemainingAmount",
                table: "AccountsReceivableSetoffDetails");

            migrationBuilder.DropColumn(
                name: "UnitName",
                table: "AccountsReceivableSetoffDetails");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DocumentNumber",
                table: "AccountsReceivableSetoffDetails",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentType",
                table: "AccountsReceivableSetoffDetails",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsFullyReceived",
                table: "AccountsReceivableSetoffDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "PreviousReceivedAmount",
                table: "AccountsReceivableSetoffDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "AccountsReceivableSetoffDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                table: "AccountsReceivableSetoffDetails",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RemainingAmount",
                table: "AccountsReceivableSetoffDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "UnitName",
                table: "AccountsReceivableSetoffDetails",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }
    }
}
