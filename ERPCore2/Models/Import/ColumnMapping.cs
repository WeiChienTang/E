namespace ERPCore2.Models.Import
{
    /// <summary>
    /// 單一欄位的對應定義（目標 Entity 屬性 ↔ 來源 Excel 欄位）
    /// </summary>
    public class ColumnMapping
    {
        /// <summary>目標 Entity 的屬性名稱</summary>
        public string TargetPropertyName { get; set; } = string.Empty;

        /// <summary>目標屬性的完整資訊</summary>
        public EntityPropertyInfo TargetProperty { get; set; } = new();

        /// <summary>對應的來源欄位名稱（Excel 標頭）。null 表示未對應。</summary>
        public string? SourceColumnName { get; set; }

        /// <summary>預設值（字串形式，匯入時依型別轉換）。用於未對應的必填欄位。</summary>
        public string? DefaultValue { get; set; }

        /// <summary>對應狀態</summary>
        public MappingStatus Status { get; set; } = MappingStatus.Unmapped;

        /// <summary>自動配對的相似度分數（0~100）</summary>
        public int AutoMatchScore { get; set; }

        /// <summary>是否已配對（有來源欄位或有預設值）</summary>
        public bool IsResolved => !string.IsNullOrEmpty(SourceColumnName) || !string.IsNullOrEmpty(DefaultValue);

        /// <summary>是否有問題（必填但未解決）</summary>
        public bool HasProblem => TargetProperty.IsRequired && !IsResolved;
    }

    /// <summary>
    /// 對應狀態
    /// </summary>
    public enum MappingStatus
    {
        /// <summary>未對應</summary>
        Unmapped = 0,
        /// <summary>已對應到來源欄位</summary>
        Mapped = 1,
        /// <summary>使用預設值</summary>
        DefaultValue = 2,
        /// <summary>自動配對建議（使用者尚未確認）</summary>
        AutoSuggested = 3
    }
}
