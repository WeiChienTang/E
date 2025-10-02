using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddSetoffPrepaymentDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prepayments_AccountsReceivableSetoffs_SetoffId",
                table: "Prepayments");

            migrationBuilder.DropIndex(
                name: "IX_Prepayments_SetoffId",
                table: "Prepayments");

            migrationBuilder.DropColumn(
                name: "SetoffId",
                table: "Prepayments");

            migrationBuilder.CreateTable(
                name: "SetoffPrepaymentDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountsReceivableSetoffId = table.Column<int>(type: "int", nullable: true),
                    AccountsPayableSetoffId = table.Column<int>(type: "int", nullable: true),
                    PrepaymentId = table.Column<int>(type: "int", nullable: false),
                    UseAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_SetoffPrepaymentDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SetoffPrepaymentDetails_AccountsPayableSetoffs_AccountsPayableSetoffId",
                        column: x => x.AccountsPayableSetoffId,
                        principalTable: "AccountsPayableSetoffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SetoffPrepaymentDetails_AccountsReceivableSetoffs_AccountsReceivableSetoffId",
                        column: x => x.AccountsReceivableSetoffId,
                        principalTable: "AccountsReceivableSetoffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SetoffPrepaymentDetails_Prepayments_PrepaymentId",
                        column: x => x.PrepaymentId,
                        principalTable: "Prepayments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SetoffPrepaymentDetails_AccountsPayableSetoffId",
                table: "SetoffPrepaymentDetails",
                column: "AccountsPayableSetoffId");

            migrationBuilder.CreateIndex(
                name: "IX_SetoffPrepaymentDetails_AccountsReceivableSetoffId",
                table: "SetoffPrepaymentDetails",
                column: "AccountsReceivableSetoffId");

            migrationBuilder.CreateIndex(
                name: "IX_SetoffPrepaymentDetails_PrepaymentId",
                table: "SetoffPrepaymentDetails",
                column: "PrepaymentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SetoffPrepaymentDetails");

            migrationBuilder.AddColumn<int>(
                name: "SetoffId",
                table: "Prepayments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Prepayments_SetoffId",
                table: "Prepayments",
                column: "SetoffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Prepayments_AccountsReceivableSetoffs_SetoffId",
                table: "Prepayments",
                column: "SetoffId",
                principalTable: "AccountsReceivableSetoffs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
