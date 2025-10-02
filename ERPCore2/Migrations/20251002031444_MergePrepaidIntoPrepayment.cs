using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class MergePrepaidIntoPrepayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: 修改 Prepayments 表結構 - 先重新命名欄位
            migrationBuilder.RenameColumn(
                name: "PrepaymentDate",
                table: "Prepayments",
                newName: "PaymentDate");

            migrationBuilder.RenameColumn(
                name: "PrepaymentAmount",
                table: "Prepayments",
                newName: "Amount");

            migrationBuilder.RenameIndex(
                name: "IX_Prepayments_PrepaymentDate",
                table: "Prepayments",
                newName: "IX_Prepayments_PaymentDate");

            // Step 2: 修改 SetoffId 為可為 null
            migrationBuilder.AlterColumn<int>(
                name: "SetoffId",
                table: "Prepayments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            // Step 3: 新增欄位
            migrationBuilder.AddColumn<int>(
                name: "CustomerId",
                table: "Prepayments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PrepaymentType",
                table: "Prepayments",
                type: "int",
                nullable: false,
                defaultValue: 1); // 預設為預收款(Prepayment = 1)

            migrationBuilder.AddColumn<int>(
                name: "SupplierId",
                table: "Prepayments",
                type: "int",
                nullable: true);

            // Step 4: 更新現有 Prepayments 資料的類型為預收款
            migrationBuilder.Sql(
                @"UPDATE Prepayments SET PrepaymentType = 1"); // 1 = Prepayment(預收款)

            // Step 5: 遷移 Prepaids 的資料到 Prepayments
            migrationBuilder.Sql(
                @"INSERT INTO Prepayments (Code, PaymentDate, Amount, PrepaymentType, SupplierId, CustomerId, SetoffId, 
                    Status, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, Remarks)
                  SELECT 
                    Code, 
                    PrepaidDate, 
                    PrepaidAmount, 
                    2 as PrepaymentType, -- 2 = Prepaid(預付款)
                    SupplierId,
                    NULL as CustomerId,
                    NULL as SetoffId,
                    Status, 
                    CreatedAt, 
                    CreatedBy, 
                    UpdatedAt, 
                    UpdatedBy, 
                    Remarks
                  FROM Prepaids");

            // Step 6: 刪除 Prepaids 表
            migrationBuilder.DropTable(
                name: "Prepaids");

            migrationBuilder.CreateIndex(
                name: "IX_Prepayments_CustomerId",
                table: "Prepayments",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Prepayments_PrepaymentType",
                table: "Prepayments",
                column: "PrepaymentType");

            migrationBuilder.CreateIndex(
                name: "IX_Prepayments_SupplierId",
                table: "Prepayments",
                column: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_Prepayments_Customers_CustomerId",
                table: "Prepayments",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Prepayments_Suppliers_SupplierId",
                table: "Prepayments",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Step 1: 重建 Prepaids 表
            migrationBuilder.CreateTable(
                name: "Prepaids",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PrepaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PrepaidDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prepaids", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prepaids_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Step 2: 將預付款資料從 Prepayments 遷移回 Prepaids
            migrationBuilder.Sql(
                @"INSERT INTO Prepaids (Code, PrepaidDate, PrepaidAmount, SupplierId, 
                    Status, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, Remarks)
                  SELECT 
                    Code, 
                    PaymentDate, 
                    Amount, 
                    SupplierId,
                    Status, 
                    CreatedAt, 
                    CreatedBy, 
                    UpdatedAt, 
                    UpdatedBy, 
                    Remarks
                  FROM Prepayments 
                  WHERE PrepaymentType = 2"); // 2 = Prepaid(預付款)

            // Step 3: 從 Prepayments 刪除預付款資料
            migrationBuilder.Sql(
                @"DELETE FROM Prepayments WHERE PrepaymentType = 2");

            // Step 4: 建立 Prepaids 索引
            migrationBuilder.CreateIndex(
                name: "IX_Prepaids_Code",
                table: "Prepaids",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_Prepaids_PrepaidDate",
                table: "Prepaids",
                column: "PrepaidDate");

            migrationBuilder.CreateIndex(
                name: "IX_Prepaids_SupplierId",
                table: "Prepaids",
                column: "SupplierId");

            // Step 5: 移除 Prepayments 的外鍵和索引
            migrationBuilder.DropForeignKey(
                name: "FK_Prepayments_Customers_CustomerId",
                table: "Prepayments");

            migrationBuilder.DropForeignKey(
                name: "FK_Prepayments_Suppliers_SupplierId",
                table: "Prepayments");

            migrationBuilder.DropIndex(
                name: "IX_Prepayments_CustomerId",
                table: "Prepayments");

            migrationBuilder.DropIndex(
                name: "IX_Prepayments_PrepaymentType",
                table: "Prepayments");

            migrationBuilder.DropIndex(
                name: "IX_Prepayments_SupplierId",
                table: "Prepayments");

            // Step 6: 移除新增的欄位
            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Prepayments");

            migrationBuilder.DropColumn(
                name: "PrepaymentType",
                table: "Prepayments");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "Prepayments");

            // Step 7: 還原欄位名稱
            migrationBuilder.RenameColumn(
                name: "PaymentDate",
                table: "Prepayments",
                newName: "PrepaymentDate");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "Prepayments",
                newName: "PrepaymentAmount");

            migrationBuilder.RenameIndex(
                name: "IX_Prepayments_PaymentDate",
                table: "Prepayments",
                newName: "IX_Prepayments_PrepaymentDate");

            // Step 8: 將 SetoffId 改回不可為 null
            migrationBuilder.AlterColumn<int>(
                name: "SetoffId",
                table: "Prepayments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
