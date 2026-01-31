using System.ComponentModel;
using ERPCore2.Models; // 引用 SortDirection 枚舉

namespace ERPCore2.Models.Reports
{
    /// <summary>
    /// 批次列印篩選條件通用模型
    /// 設計理念：提供彈性的篩選條件組合，適用於所有單據類型的批次列印
    /// </summary>
    public class BatchPrintCriteria
    {
        /// <summary>
        /// 開始日期（用於日期範圍篩選）
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 結束日期（用於日期範圍篩選）
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// 關聯實體ID列表（例如：廠商ID、客戶ID、倉庫ID等）
        /// 語意：當列表為空時，表示「全部」；當有值時，表示「僅包含這些ID」
        /// </summary>
        public List<int> RelatedEntityIds { get; set; } = new();

        /// <summary>
        /// 狀態篩選列表（例如：Pending、Confirmed、Completed等）
        /// 語意：當列表為空時，表示「全部狀態」；當有值時，表示「僅包含這些狀態」
        /// </summary>
        public List<string> Statuses { get; set; } = new();

        /// <summary>
        /// 公司ID篩選（多公司系統適用）
        /// </summary>
        public int? CompanyId { get; set; }

        /// <summary>
        /// 倉庫ID篩選（適用於有倉庫關聯的單據）
        /// </summary>
        public int? WarehouseId { get; set; }

        /// <summary>
        /// 單據編號關鍵字搜尋（模糊搜尋）
        /// </summary>
        public string? DocumentNumberKeyword { get; set; }

        /// <summary>
        /// 自訂篩選欄位（彈性擴充用）
        /// 用法：可存放任何額外的篩選條件，例如 {"MinAmount": 1000, "MaxAmount": 50000}
        /// </summary>
        public Dictionary<string, object> CustomFilters { get; set; } = new();

        /// <summary>
        /// 報表類型編號（用於選擇對應的列印配置）
        /// 例如：PurchaseOrder、PurchaseReceiving、SalesOrder等
        /// </summary>
        public string? ReportType { get; set; }

        /// <summary>
        /// 列印配置ID（可選，若指定則使用特定的列印配置）
        /// </summary>
        public int? PrintConfigurationId { get; set; }

        /// <summary>
        /// 排序欄位（例如：OrderDate、DocumentNumber等）
        /// </summary>
        public string SortBy { get; set; } = "OrderDate";

        /// <summary>
        /// 排序方向
        /// </summary>
        public SortDirection SortDirection { get; set; } = SortDirection.Descending;

        /// <summary>
        /// 最大查詢筆數限制（避免一次處理過多資料）
        /// </summary>
        public int? MaxResults { get; set; } = 100;

        /// <summary>
        /// 是否包含已取消的單據
        /// </summary>
        public bool IncludeCancelled { get; set; } = false;

        /// <summary>
        /// 驗證篩選條件是否有效
        /// </summary>
        /// <returns>驗證結果</returns>
        public ValidationResult Validate()
        {
            var errors = new List<string>();

            // 日期範圍驗證
            if (StartDate.HasValue && EndDate.HasValue && StartDate.Value > EndDate.Value)
            {
                errors.Add("開始日期不能大於結束日期");
            }

            // 日期範圍不能過大（例如超過1年）
            if (StartDate.HasValue && EndDate.HasValue)
            {
                var daysDiff = (EndDate.Value - StartDate.Value).TotalDays;
                if (daysDiff > 365)
                {
                    errors.Add("日期範圍不能超過一年");
                }
            }

            // 最大筆數驗證
            if (MaxResults.HasValue && MaxResults.Value <= 0)
            {
                errors.Add("最大筆數必須大於0");
            }

            if (MaxResults.HasValue && MaxResults.Value > 1000)
            {
                errors.Add("最大筆數不能超過1000筆（避免效能問題）");
            }

            return new ValidationResult
            {
                IsValid = errors.Count == 0,
                Errors = errors
            };
        }

        /// <summary>
        /// 取得篩選條件摘要（用於顯示給使用者）
        /// </summary>
        /// <returns>篩選條件描述</returns>
        public string GetSummary()
        {
            var summary = new List<string>();

            if (StartDate.HasValue || EndDate.HasValue)
            {
                var dateRange = $"{StartDate?.ToString("yyyy/MM/dd") ?? "不限"} ~ {EndDate?.ToString("yyyy/MM/dd") ?? "不限"}";
                summary.Add($"日期範圍：{dateRange}");
            }

            if (RelatedEntityIds.Any())
            {
                summary.Add($"篩選 {RelatedEntityIds.Count} 個關聯實體");
            }

            if (Statuses.Any())
            {
                summary.Add($"狀態：{string.Join(", ", Statuses)}");
            }

            if (!string.IsNullOrEmpty(DocumentNumberKeyword))
            {
                summary.Add($"單據編號包含：{DocumentNumberKeyword}");
            }

            if (MaxResults.HasValue)
            {
                summary.Add($"最多 {MaxResults} 筆");
            }

            return summary.Any() ? string.Join(" | ", summary) : "無篩選條件（列印全部）";
        }
    }

    // 注意：SortDirection 已移至 ERPCore2.Models 命名空間（Models/ReportModels.cs）
    // 以維持向後相容性，避免現有程式碼需要修改

    /// <summary>
    /// 驗證結果類別
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// 是否驗證通過
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// 錯誤訊息列表
        /// </summary>
        public List<string> Errors { get; set; } = new();

        /// <summary>
        /// 取得第一個錯誤訊息
        /// </summary>
        public string? FirstError => Errors.FirstOrDefault();

        /// <summary>
        /// 取得所有錯誤訊息（換行分隔）
        /// </summary>
        public string GetAllErrors() => string.Join("\n", Errors);
    }
}
