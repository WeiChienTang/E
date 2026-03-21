using ERPCore2.Data.Entities;
using ERPCore2.Models.Import;
using Microsoft.AspNetCore.Components.Forms;

namespace ERPCore2.Services
{
    /// <summary>
    /// 銀行對帳單 CSV 匯入服務介面
    ///
    /// CSV 固定格式（UTF-8 優先，自動 fallback Big5）：
    ///   交易日期,交易說明,支出金額,收入金額,銀行流水號,交易後餘額
    ///
    /// 欄位說明：
    ///   - 交易日期（必填）：yyyy/MM/dd 或 yyyy-MM-dd
    ///   - 交易說明（必填）：最多 200 字
    ///   - 支出金額（選填）：數字，空白=0
    ///   - 收入金額（選填）：數字，空白=0
    ///   - 銀行流水號（選填）：最多 100 字，存入 DB 供未來自動配對
    ///   - 交易後餘額（選填）：數字，僅供預覽顯示，不存入 DB
    /// </summary>
    public interface IBankStatementImportService
    {
        /// <summary>
        /// 解析 CSV 檔案並驗證每列資料
        /// 自動偵測編碼（UTF-8 → Big5 fallback）
        /// </summary>
        Task<BankStatementImportResult> ParseCsvAsync(IBrowserFile file);

        /// <summary>
        /// 驗證匯入前提條件（對帳單是否有未配對的既有明細行）
        /// 有任何未配對行時回傳 false，要求使用者先清空明細再匯入。
        /// </summary>
        Task<(bool CanImport, string? BlockReason)> ValidatePreConditionAsync(int bankStatementId);

        /// <summary>
        /// 將已驗證的行寫入資料庫
        /// 只寫入 IsValid=true 的行；若對帳單期間有設定，也會驗證日期是否在範圍內。
        /// </summary>
        Task<(bool Success, string Message, int ImportedCount)> ImportAsync(
            int bankStatementId,
            List<BankStatementImportRow> rows,
            string operatorName);

        /// <summary>
        /// 產生 CSV 範本的文字內容（供下載）
        /// </summary>
        string GenerateTemplateCsv();
    }
}
