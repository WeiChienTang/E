using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Services.Reports.Interfaces;

/// <summary>
/// 明細科目餘額表報表服務介面
/// 彙總各科目期初餘額、本期借方、本期貸方、期末餘額（無逐筆明細）
/// </summary>
public interface IDetailAccountBalanceReportService
{
    Task<BatchPreviewResult> RenderBatchToImagesAsync(DetailAccountBalanceCriteria criteria);
}
