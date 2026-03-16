using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeSalaryItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JournalEntries_SourceDocumentType_SourceDocumentId",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "MealAllowance",
                table: "EmployeeSalaries");

            migrationBuilder.DropColumn(
                name: "PositionAllowance",
                table: "EmployeeSalaries");

            migrationBuilder.DropColumn(
                name: "TransportAllowance",
                table: "EmployeeSalaries");

            migrationBuilder.CreateTable(
                name: "EmployeeSalaryItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeSalaryId = table.Column<int>(type: "int", nullable: false),
                    PayrollItemId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeSalaryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeSalaryItems_EmployeeSalaries_EmployeeSalaryId",
                        column: x => x.EmployeeSalaryId,
                        principalTable: "EmployeeSalaries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeSalaryItems_PayrollItems_PayrollItemId",
                        column: x => x.PayrollItemId,
                        principalTable: "PayrollItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // 建唯一索引前，先刪除重複的傳票（保留 Id 最大的那筆）
            migrationBuilder.Sql(@"
                DELETE FROM JournalEntries
                WHERE Id NOT IN (
                    SELECT MAX(Id)
                    FROM JournalEntries
                    WHERE SourceDocumentType IS NOT NULL
                    GROUP BY SourceDocumentType, SourceDocumentId
                )
                AND SourceDocumentType IS NOT NULL;
            ");

            migrationBuilder.CreateIndex(
                name: "UX_JournalEntry_SourceDocument",
                table: "JournalEntries",
                columns: new[] { "SourceDocumentType", "SourceDocumentId" },
                unique: true,
                filter: "[SourceDocumentType] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSalaryItems_EmployeeSalaryId",
                table: "EmployeeSalaryItems",
                column: "EmployeeSalaryId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSalaryItems_PayrollItemId",
                table: "EmployeeSalaryItems",
                column: "PayrollItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeSalaryItems");

            migrationBuilder.DropIndex(
                name: "UX_JournalEntry_SourceDocument",
                table: "JournalEntries");

            migrationBuilder.AddColumn<decimal>(
                name: "MealAllowance",
                table: "EmployeeSalaries",
                type: "decimal(18,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PositionAllowance",
                table: "EmployeeSalaries",
                type: "decimal(18,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TransportAllowance",
                table: "EmployeeSalaries",
                type: "decimal(18,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_SourceDocumentType_SourceDocumentId",
                table: "JournalEntries",
                columns: new[] { "SourceDocumentType", "SourceDocumentId" });
        }
    }
}
