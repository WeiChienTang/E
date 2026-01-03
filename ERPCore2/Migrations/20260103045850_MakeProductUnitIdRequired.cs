using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class MakeProductUnitIdRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 先更新現有的 NULL 值為第一個有效的單位 ID（如果存在）
            // 這樣可以避免外鍵錯誤
            migrationBuilder.Sql(@"
                UPDATE Products 
                SET UnitId = (SELECT TOP 1 Id FROM Units WHERE Status = 1 ORDER BY Id)
                WHERE UnitId IS NULL 
                AND EXISTS (SELECT 1 FROM Units WHERE Status = 1)
            ");
            
            migrationBuilder.AlterColumn<int>(
                name: "UnitId",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "UnitId",
                table: "Products",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
