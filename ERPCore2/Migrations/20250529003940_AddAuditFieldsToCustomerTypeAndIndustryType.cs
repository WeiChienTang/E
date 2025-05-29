using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditFieldsToCustomerTypeAndIndustryType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "IndustryTypes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "IndustryTypes",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()");

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "IndustryTypes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "IndustryTypes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "CustomerTypes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "CustomerTypes",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()");

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "CustomerTypes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "CustomerTypes",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "IndustryTypes");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "IndustryTypes");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "IndustryTypes");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "IndustryTypes");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "CustomerTypes");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "CustomerTypes");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "CustomerTypes");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "CustomerTypes");
        }
    }
}
