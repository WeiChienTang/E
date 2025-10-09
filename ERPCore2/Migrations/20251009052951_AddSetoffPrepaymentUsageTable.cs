using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddSetoffPrepaymentUsageTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SetoffPrepayments_SourceDocumentCode",
                table: "SetoffPrepayments");

            migrationBuilder.CreateTable(
                name: "SetoffPrepaymentUsages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SetoffPrepaymentId = table.Column<int>(type: "int", nullable: false),
                    SetoffDocumentId = table.Column<int>(type: "int", nullable: false),
                    UsedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UsageDate = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_SetoffPrepaymentUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SetoffPrepaymentUsages_SetoffDocuments_SetoffDocumentId",
                        column: x => x.SetoffDocumentId,
                        principalTable: "SetoffDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SetoffPrepaymentUsages_SetoffPrepayments_SetoffPrepaymentId",
                        column: x => x.SetoffPrepaymentId,
                        principalTable: "SetoffPrepayments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SetoffPrepayments_SourceDocumentCode",
                table: "SetoffPrepayments",
                column: "SourceDocumentCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SetoffPrepaymentUsages_SetoffDocumentId",
                table: "SetoffPrepaymentUsages",
                column: "SetoffDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_SetoffPrepaymentUsages_SetoffPrepaymentId",
                table: "SetoffPrepaymentUsages",
                column: "SetoffPrepaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_SetoffPrepaymentUsages_UsageDate",
                table: "SetoffPrepaymentUsages",
                column: "UsageDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SetoffPrepaymentUsages");

            migrationBuilder.DropIndex(
                name: "IX_SetoffPrepayments_SourceDocumentCode",
                table: "SetoffPrepayments");

            migrationBuilder.CreateIndex(
                name: "IX_SetoffPrepayments_SourceDocumentCode",
                table: "SetoffPrepayments",
                column: "SourceDocumentCode");
        }
    }
}
