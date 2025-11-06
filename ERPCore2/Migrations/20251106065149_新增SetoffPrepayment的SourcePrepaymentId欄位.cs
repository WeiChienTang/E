using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class 新增SetoffPrepayment的SourcePrepaymentId欄位 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SourcePrepaymentId",
                table: "SetoffPrepayments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SetoffPrepayments_SourcePrepaymentId",
                table: "SetoffPrepayments",
                column: "SourcePrepaymentId");

            migrationBuilder.AddForeignKey(
                name: "FK_SetoffPrepayments_SetoffPrepayments_SourcePrepaymentId",
                table: "SetoffPrepayments",
                column: "SourcePrepaymentId",
                principalTable: "SetoffPrepayments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SetoffPrepayments_SetoffPrepayments_SourcePrepaymentId",
                table: "SetoffPrepayments");

            migrationBuilder.DropIndex(
                name: "IX_SetoffPrepayments_SourcePrepaymentId",
                table: "SetoffPrepayments");

            migrationBuilder.DropColumn(
                name: "SourcePrepaymentId",
                table: "SetoffPrepayments");
        }
    }
}
