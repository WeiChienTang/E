using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class FixSubAccountParentCodeDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 修正上一個 migration 將統制科目代碼寫入空字串的問題
            // 僅更新值為空字串的欄位，不覆蓋使用者已自訂的值
            migrationBuilder.Sql(@"
                UPDATE SystemParameters
                SET
                    CustomerSubAccountParentCode = CASE WHEN CustomerSubAccountParentCode = '' THEN '1191' ELSE CustomerSubAccountParentCode END,
                    SupplierSubAccountParentCode = CASE WHEN SupplierSubAccountParentCode = '' THEN '2171' ELSE SupplierSubAccountParentCode END,
                    ProductSubAccountParentCode  = CASE WHEN ProductSubAccountParentCode  = '' THEN '1231' ELSE ProductSubAccountParentCode  END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Down 無需回復資料（空字串為無效值，不需還原）
        }
    }
}
