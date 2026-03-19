using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Services.Reports.Interfaces;

/// <summary>
/// 應付帳款帳齡分析報表服務介面
/// 依收貨日 + 廠商付款天數計算到期日，彙總各帳齡區間未付金額
/// </summary>
public interface IAPAgingReportService
{
    /// <summary>
    /// 以應付帳款帳齡分析篩選條件渲染報表為圖片
    /// </summary>
    Task<BatchPreviewResult> RenderBatchToImagesAsync(APAgingCriteria criteria);
}
