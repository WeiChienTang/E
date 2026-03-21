using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Services.Reports.Interfaces;

/// <summary>
/// 銀行存款餘額調節表報表服務介面
/// 依對帳期間查詢已建立的銀行對帳單，顯示已配對與未配對明細，提供差異分析
/// </summary>
public interface IBankReconciliationReportService
{
    /// <summary>
    /// 以銀行對帳篩選條件渲染銀行存款餘額調節表為圖片
    /// </summary>
    Task<BatchPreviewResult> RenderBatchToImagesAsync(BankReconciliationCriteria criteria);
}
