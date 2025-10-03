using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class RenamePrepaymentDetailsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SetoffPrepaymentDetails_AccountsPayableSetoffs_AccountsPayableSetoffId",
                table: "SetoffPrepaymentDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_SetoffPrepaymentDetails_AccountsReceivableSetoffs_AccountsReceivableSetoffId",
                table: "SetoffPrepaymentDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_SetoffPrepaymentDetails_Prepayments_PrepaymentId",
                table: "SetoffPrepaymentDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SetoffPrepaymentDetails",
                table: "SetoffPrepaymentDetails");

            migrationBuilder.RenameTable(
                name: "SetoffPrepaymentDetails",
                newName: "PrepaymentDetails");

            migrationBuilder.RenameIndex(
                name: "IX_SetoffPrepaymentDetails_PrepaymentId",
                table: "PrepaymentDetails",
                newName: "IX_PrepaymentDetails_PrepaymentId");

            migrationBuilder.RenameIndex(
                name: "IX_SetoffPrepaymentDetails_AccountsReceivableSetoffId",
                table: "PrepaymentDetails",
                newName: "IX_PrepaymentDetails_AccountsReceivableSetoffId");

            migrationBuilder.RenameIndex(
                name: "IX_SetoffPrepaymentDetails_AccountsPayableSetoffId",
                table: "PrepaymentDetails",
                newName: "IX_PrepaymentDetails_AccountsPayableSetoffId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PrepaymentDetails",
                table: "PrepaymentDetails",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PrepaymentDetails_AccountsPayableSetoffs_AccountsPayableSetoffId",
                table: "PrepaymentDetails",
                column: "AccountsPayableSetoffId",
                principalTable: "AccountsPayableSetoffs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PrepaymentDetails_AccountsReceivableSetoffs_AccountsReceivableSetoffId",
                table: "PrepaymentDetails",
                column: "AccountsReceivableSetoffId",
                principalTable: "AccountsReceivableSetoffs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PrepaymentDetails_Prepayments_PrepaymentId",
                table: "PrepaymentDetails",
                column: "PrepaymentId",
                principalTable: "Prepayments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PrepaymentDetails_AccountsPayableSetoffs_AccountsPayableSetoffId",
                table: "PrepaymentDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_PrepaymentDetails_AccountsReceivableSetoffs_AccountsReceivableSetoffId",
                table: "PrepaymentDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_PrepaymentDetails_Prepayments_PrepaymentId",
                table: "PrepaymentDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PrepaymentDetails",
                table: "PrepaymentDetails");

            migrationBuilder.RenameTable(
                name: "PrepaymentDetails",
                newName: "SetoffPrepaymentDetails");

            migrationBuilder.RenameIndex(
                name: "IX_PrepaymentDetails_PrepaymentId",
                table: "SetoffPrepaymentDetails",
                newName: "IX_SetoffPrepaymentDetails_PrepaymentId");

            migrationBuilder.RenameIndex(
                name: "IX_PrepaymentDetails_AccountsReceivableSetoffId",
                table: "SetoffPrepaymentDetails",
                newName: "IX_SetoffPrepaymentDetails_AccountsReceivableSetoffId");

            migrationBuilder.RenameIndex(
                name: "IX_PrepaymentDetails_AccountsPayableSetoffId",
                table: "SetoffPrepaymentDetails",
                newName: "IX_SetoffPrepaymentDetails_AccountsPayableSetoffId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SetoffPrepaymentDetails",
                table: "SetoffPrepaymentDetails",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SetoffPrepaymentDetails_AccountsPayableSetoffs_AccountsPayableSetoffId",
                table: "SetoffPrepaymentDetails",
                column: "AccountsPayableSetoffId",
                principalTable: "AccountsPayableSetoffs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SetoffPrepaymentDetails_AccountsReceivableSetoffs_AccountsReceivableSetoffId",
                table: "SetoffPrepaymentDetails",
                column: "AccountsReceivableSetoffId",
                principalTable: "AccountsReceivableSetoffs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SetoffPrepaymentDetails_Prepayments_PrepaymentId",
                table: "SetoffPrepaymentDetails",
                column: "PrepaymentId",
                principalTable: "Prepayments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
