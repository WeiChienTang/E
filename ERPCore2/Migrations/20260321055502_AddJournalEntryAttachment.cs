using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddJournalEntryAttachment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BankId1",
                table: "EmployeeBankAccounts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "JournalEntryAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JournalEntryId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StoredFilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadedByEmployeeId = table.Column<int>(type: "int", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDraft = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalEntryAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalEntryAttachments_Employees_UploadedByEmployeeId",
                        column: x => x.UploadedByEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_JournalEntryAttachments_JournalEntries_JournalEntryId",
                        column: x => x.JournalEntryId,
                        principalTable: "JournalEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeBankAccounts_BankId1",
                table: "EmployeeBankAccounts",
                column: "BankId1");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryAttachments_JournalEntryId",
                table: "JournalEntryAttachments",
                column: "JournalEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryAttachments_UploadedAt",
                table: "JournalEntryAttachments",
                column: "UploadedAt");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryAttachments_UploadedByEmployeeId",
                table: "JournalEntryAttachments",
                column: "UploadedByEmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeBankAccounts_Banks_BankId1",
                table: "EmployeeBankAccounts",
                column: "BankId1",
                principalTable: "Banks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeBankAccounts_Banks_BankId1",
                table: "EmployeeBankAccounts");

            migrationBuilder.DropTable(
                name: "JournalEntryAttachments");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeBankAccounts_BankId1",
                table: "EmployeeBankAccounts");

            migrationBuilder.DropColumn(
                name: "BankId1",
                table: "EmployeeBankAccounts");
        }
    }
}
