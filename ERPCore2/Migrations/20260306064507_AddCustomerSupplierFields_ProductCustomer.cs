using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerSupplierFields_ProductCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BankAccount",
                table: "Suppliers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "Suppliers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingAddress",
                table: "Suppliers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentPayable",
                table: "Suppliers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceTitle",
                table: "Suppliers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentDay",
                table: "Suppliers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SupplierStatus",
                table: "Suppliers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SupplierType",
                table: "Suppliers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductionScheduleId",
                table: "MaterialIssues",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductionScheduleDetailId",
                table: "MaterialIssueDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccount",
                table: "Customers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "Customers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingAddress",
                table: "Customers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreditRating",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CustomerSource",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CustomerType",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DefaultDiscountRate",
                table: "Customers",
                type: "decimal(5,4)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductCustomers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    IsPreferred = table.Column<bool>(type: "bit", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    CustomerPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DiscountRate = table.Column<decimal>(type: "decimal(5,4)", nullable: true),
                    CustomerProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastSalePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LastSaleDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LeadTimeDays = table.Column<int>(type: "int", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCustomers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductCustomers_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductCustomers_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaterialIssues_ProductionScheduleId",
                table: "MaterialIssues",
                column: "ProductionScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialIssueDetails_ProductionScheduleDetailId",
                table: "MaterialIssueDetails",
                column: "ProductionScheduleDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCustomer_CustomerId",
                table: "ProductCustomers",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCustomer_ProductId_IsPreferred_Priority",
                table: "ProductCustomers",
                columns: new[] { "ProductId", "IsPreferred", "Priority" });

            migrationBuilder.CreateIndex(
                name: "UX_ProductCustomer_ProductId_CustomerId",
                table: "ProductCustomers",
                columns: new[] { "ProductId", "CustomerId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MaterialIssueDetails_ProductionScheduleDetails_ProductionScheduleDetailId",
                table: "MaterialIssueDetails",
                column: "ProductionScheduleDetailId",
                principalTable: "ProductionScheduleDetails",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MaterialIssues_ProductionSchedules_ProductionScheduleId",
                table: "MaterialIssues",
                column: "ProductionScheduleId",
                principalTable: "ProductionSchedules",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaterialIssueDetails_ProductionScheduleDetails_ProductionScheduleDetailId",
                table: "MaterialIssueDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_MaterialIssues_ProductionSchedules_ProductionScheduleId",
                table: "MaterialIssues");

            migrationBuilder.DropTable(
                name: "ProductCustomers");

            migrationBuilder.DropIndex(
                name: "IX_MaterialIssues_ProductionScheduleId",
                table: "MaterialIssues");

            migrationBuilder.DropIndex(
                name: "IX_MaterialIssueDetails_ProductionScheduleDetailId",
                table: "MaterialIssueDetails");

            migrationBuilder.DropColumn(
                name: "BankAccount",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "BillingAddress",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "CurrentPayable",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "InvoiceTitle",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "PaymentDay",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "SupplierStatus",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "SupplierType",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "ProductionScheduleId",
                table: "MaterialIssues");

            migrationBuilder.DropColumn(
                name: "ProductionScheduleDetailId",
                table: "MaterialIssueDetails");

            migrationBuilder.DropColumn(
                name: "BankAccount",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "BillingAddress",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CreditRating",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CustomerSource",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CustomerType",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "DefaultDiscountRate",
                table: "Customers");
        }
    }
}
