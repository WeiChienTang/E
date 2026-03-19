using ERPCore2.Models.Charts;

namespace ERPCore2.Services.FinancialManagement;

public interface IFinancialChartService
{
    /// <summary>依沖款類型分布（應收/應付）</summary>
    Task<List<ChartDataItem>> GetSetoffByTypeAsync();

    /// <summary>每月沖款金額趨勢（近 N 個月）</summary>
    Task<List<ChartDataItem>> GetMonthlySetoffTrendAsync(int months = 12);

    /// <summary>依關聯方沖款金額排行 Top N</summary>
    Task<List<ChartDataItem>> GetTopSetoffByAmountAsync(int top = 10);

    /// <summary>依傳票類型分布（自動產生/手動輸入/調整…）</summary>
    Task<List<ChartDataItem>> GetJournalByTypeAsync();

    /// <summary>每月傳票金額趨勢（近 N 個月，已過帳傳票）</summary>
    Task<List<ChartDataItem>> GetMonthlyJournalTrendAsync(int months = 12);

    /// <summary>依傳票狀態分布（草稿/已過帳/已取消/已沖銷）</summary>
    Task<List<ChartDataItem>> GetJournalByStatusAsync();

    /// <summary>取得財務管理統計摘要</summary>
    Task<FinancialChartSummary> GetSummaryAsync();

    // ===== Drill-down 明細查詢 =====

    /// <summary>依沖款類型 Drill-down：顯示該類型最近沖款單清單</summary>
    Task<List<ChartDetailItem>> GetSetoffDetailsByTypeAsync(string typeLabel);

    /// <summary>依關聯方沖款 Drill-down：顯示該關聯方的沖款明細</summary>
    Task<List<ChartDetailItem>> GetSetoffDetailsByPartyAsync(string partyLabel);

    /// <summary>依傳票類型 Drill-down：顯示該類型最近傳票清單</summary>
    Task<List<ChartDetailItem>> GetJournalDetailsByTypeAsync(string typeLabel);

    /// <summary>依傳票狀態 Drill-down：顯示該狀態的傳票清單</summary>
    Task<List<ChartDetailItem>> GetJournalDetailsByStatusAsync(string statusLabel);
}
