using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Services.Reports.Interfaces;

/// <summary>
/// 應收帳款帳齡分析報表服務介面
/// 依出貨日 + 客戶付款天數計算到期日，彙總各帳齡區間未收金額
/// </summary>
public interface IARAgingReportService
{
    /// <summary>
    /// 以應收帳款帳齡分析篩選條件渲染報表為圖片
    /// </summary>
    Task<BatchPreviewResult> RenderBatchToImagesAsync(ARAgingCriteria criteria);
}
