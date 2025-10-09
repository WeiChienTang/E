using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSetoffPrepaymentCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SetoffPrepayments_SetoffDocuments_SetoffDocumentId",
                table: "SetoffPrepayments");

            migrationBuilder.DropIndex(
                name: "IX_SetoffPrepayments_Code",
                table: "SetoffPrepayments");

            migrationBuilder.AddForeignKey(
                name: "FK_SetoffPrepayments_SetoffDocuments_SetoffDocumentId",
                table: "SetoffPrepayments",
                column: "SetoffDocumentId",
                principalTable: "SetoffDocuments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SetoffPrepayments_SetoffDocuments_SetoffDocumentId",
                table: "SetoffPrepayments");

            migrationBuilder.CreateIndex(
                name: "IX_SetoffPrepayments_Code",
                table: "SetoffPrepayments",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_SetoffPrepayments_SetoffDocuments_SetoffDocumentId",
                table: "SetoffPrepayments",
                column: "SetoffDocumentId",
                principalTable: "SetoffDocuments",
                principalColumn: "Id");
        }
    }
}
