using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddSalespersonIdToSalesTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesDeliveries_Customers_CustomerId",
                table: "SalesDeliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesDeliveries_Employees_ApprovedBy",
                table: "SalesDeliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesDeliveries_Employees_EmployeeId",
                table: "SalesDeliveries");

            migrationBuilder.AddColumn<int>(
                name: "SalespersonId",
                table: "SalesOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SalespersonId",
                table: "SalesDeliveries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SalespersonId",
                table: "Quotations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_SalespersonId",
                table: "SalesOrders",
                column: "SalespersonId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesDeliveries_SalespersonId",
                table: "SalesDeliveries",
                column: "SalespersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_SalespersonId",
                table: "Quotations",
                column: "SalespersonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quotations_Employees_SalespersonId",
                table: "Quotations",
                column: "SalespersonId",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesDeliveries_Customers_CustomerId",
                table: "SalesDeliveries",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesDeliveries_Employees_ApprovedBy",
                table: "SalesDeliveries",
                column: "ApprovedBy",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesDeliveries_Employees_EmployeeId",
                table: "SalesDeliveries",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesDeliveries_Employees_SalespersonId",
                table: "SalesDeliveries",
                column: "SalespersonId",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrders_Employees_SalespersonId",
                table: "SalesOrders",
                column: "SalespersonId",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quotations_Employees_SalespersonId",
                table: "Quotations");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesDeliveries_Customers_CustomerId",
                table: "SalesDeliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesDeliveries_Employees_ApprovedBy",
                table: "SalesDeliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesDeliveries_Employees_EmployeeId",
                table: "SalesDeliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesDeliveries_Employees_SalespersonId",
                table: "SalesDeliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrders_Employees_SalespersonId",
                table: "SalesOrders");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrders_SalespersonId",
                table: "SalesOrders");

            migrationBuilder.DropIndex(
                name: "IX_SalesDeliveries_SalespersonId",
                table: "SalesDeliveries");

            migrationBuilder.DropIndex(
                name: "IX_Quotations_SalespersonId",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "SalespersonId",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "SalespersonId",
                table: "SalesDeliveries");

            migrationBuilder.DropColumn(
                name: "SalespersonId",
                table: "Quotations");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesDeliveries_Customers_CustomerId",
                table: "SalesDeliveries",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesDeliveries_Employees_ApprovedBy",
                table: "SalesDeliveries",
                column: "ApprovedBy",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesDeliveries_Employees_EmployeeId",
                table: "SalesDeliveries",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");
        }
    }
}
