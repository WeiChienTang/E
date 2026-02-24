using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Services.Reports.Interfaces;

/// <summary>
/// 總分類帳報表服務介面
/// 顯示所有科目的帳戶卡片（期初餘額 + 逐筆明細 + 期末餘額）
/// </summary>
public interface IGeneralLedgerReportService
{
    Task<BatchPreviewResult> RenderBatchToImagesAsync(GeneralLedgerCriteria criteria);
}
