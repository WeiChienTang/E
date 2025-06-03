using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntityStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "ContactTypes");

            migrationBuilder.RenameColumn(
                name: "ModifiedDate",
                table: "IndustryTypes",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "ModifiedBy",
                table: "IndustryTypes",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "IndustryTypes",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "IndustryTypeId",
                table: "IndustryTypes",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ModifiedDate",
                table: "CustomerTypes",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "ModifiedBy",
                table: "CustomerTypes",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "CustomerTypes",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "CustomerTypeId",
                table: "CustomerTypes",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ModifiedDate",
                table: "Customers",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "ModifiedBy",
                table: "Customers",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "Customers",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ContactId",
                table: "CustomerContacts",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "AddressId",
                table: "CustomerAddresses",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ModifiedDate",
                table: "ContactTypes",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "ModifiedBy",
                table: "ContactTypes",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "ContactTypeId",
                table: "ContactTypes",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ModifiedDate",
                table: "AddressTypes",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "ModifiedBy",
                table: "AddressTypes",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "AddressTypes",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "AddressTypeId",
                table: "AddressTypes",
                newName: "Id");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "IndustryTypes",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "IndustryTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "CustomerTypes",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "CustomerTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Customers",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Customers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Customers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "CustomerContacts",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<bool>(
                name: "IsPrimary",
                table: "CustomerContacts",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "CustomerContacts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "CustomerContacts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "CustomerContacts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "CustomerContacts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "CustomerContacts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "CustomerAddresses",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<bool>(
                name: "IsPrimary",
                table: "CustomerAddresses",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "CustomerAddresses",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "CustomerAddresses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "CustomerAddresses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "CustomerAddresses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "CustomerAddresses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "ContactTypes",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ContactTypes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ContactTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "AddressTypes",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "AddressTypes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AddressTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "IndustryTypes");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "CustomerTypes");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "CustomerContacts");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "CustomerContacts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "CustomerContacts");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "CustomerContacts");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "CustomerContacts");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "CustomerAddresses");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "CustomerAddresses");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "CustomerAddresses");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "CustomerAddresses");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "CustomerAddresses");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ContactTypes");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ContactTypes");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AddressTypes");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "IndustryTypes",
                newName: "ModifiedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "IndustryTypes",
                newName: "ModifiedDate");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "IndustryTypes",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "IndustryTypes",
                newName: "IndustryTypeId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "CustomerTypes",
                newName: "ModifiedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "CustomerTypes",
                newName: "ModifiedDate");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "CustomerTypes",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "CustomerTypes",
                newName: "CustomerTypeId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Customers",
                newName: "ModifiedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Customers",
                newName: "ModifiedDate");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Customers",
                newName: "CustomerId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "CustomerContacts",
                newName: "ContactId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "CustomerAddresses",
                newName: "AddressId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "ContactTypes",
                newName: "ModifiedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "ContactTypes",
                newName: "ModifiedDate");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ContactTypes",
                newName: "ContactTypeId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "AddressTypes",
                newName: "ModifiedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "AddressTypes",
                newName: "ModifiedDate");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "AddressTypes",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "AddressTypes",
                newName: "AddressTypeId");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "IndustryTypes",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "CustomerTypes",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Customers",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Customers",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "CustomerContacts",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsPrimary",
                table: "CustomerContacts",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "CustomerAddresses",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsPrimary",
                table: "CustomerAddresses",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "ContactTypes",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "ContactTypes",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "AddressTypes",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "AddressTypes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }
    }
}
