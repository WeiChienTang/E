using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class 新增報價單工程名稱欄位 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProjectName",
                table: "Quotations",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProjectName",
                table: "Quotations");
        }
    }
}
