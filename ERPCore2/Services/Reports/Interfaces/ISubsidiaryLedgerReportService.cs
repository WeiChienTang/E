using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Services.Reports.Interfaces;

/// <summary>
/// 明細分類帳報表服務介面
/// 依科目關鍵字查詢特定科目的帳戶卡片（期初餘額 + 逐筆明細 + 期末餘額）
/// </summary>
public interface ISubsidiaryLedgerReportService
{
    Task<BatchPreviewResult> RenderBatchToImagesAsync(SubsidiaryLedgerCriteria criteria);
}
