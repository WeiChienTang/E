using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class 移除Number : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SetoffDocuments_SetoffNumber",
                table: "SetoffDocuments");

            migrationBuilder.DropColumn(
                name: "SetoffNumber",
                table: "SetoffDocuments");

            migrationBuilder.CreateIndex(
                name: "IX_SetoffDocuments_Code",
                table: "SetoffDocuments",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SetoffDocuments_Code",
                table: "SetoffDocuments");

            migrationBuilder.AddColumn<string>(
                name: "SetoffNumber",
                table: "SetoffDocuments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_SetoffDocuments_SetoffNumber",
                table: "SetoffDocuments",
                column: "SetoffNumber",
                unique: true);
        }
    }
}
