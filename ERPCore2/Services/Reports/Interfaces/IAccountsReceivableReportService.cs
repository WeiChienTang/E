using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Services.Reports.Interfaces;

/// <summary>
/// 應收帳款報表服務介面
/// 依期間與客戶篩選應收帳款明細，顯示出貨金額、已收金額與未收餘額
/// </summary>
public interface IAccountsReceivableReportService
{
    /// <summary>
    /// 以應收帳款篩選條件渲染報表為圖片
    /// </summary>
    Task<BatchPreviewResult> RenderBatchToImagesAsync(AccountsReceivableCriteria criteria);
}
