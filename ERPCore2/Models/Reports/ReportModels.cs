using System.ComponentModel;

namespace ERPCore2.Models.Reports
{
    /// <summary>
    /// 報表配置類別 - 定義報表的基本設定
    /// </summary>
    public class ReportConfiguration
    {
        /// <summary>
        /// 報表標題
        /// </summary>
        public string Title { get; set; } = string.Empty;
        
        /// <summary>
        /// 報表副標題（可選）
        /// </summary>
        public string? Subtitle { get; set; }
        
        /// <summary>
        /// 公司名稱
        /// </summary>
        public string CompanyName { get; set; } = "公司名稱";
        
        /// <summary>
        /// 公司地址
        /// </summary>
        public string? CompanyAddress { get; set; }
        
        /// <summary>
        /// 公司電話
        /// </summary>
        public string? CompanyPhone { get; set; }
        
        /// <summary>
        /// 報表生成時間
        /// </summary>
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
        
        /// <summary>
        /// 頁首區段定義
        /// </summary>
        public List<ReportHeaderSection> HeaderSections { get; set; } = new();
        
        /// <summary>
        /// 標頭左側資訊（與公司名稱並排顯示）
        /// </summary>
        public List<ReportField> HeaderLeftFields { get; set; } = new();
        
        /// <summary>
        /// 表格欄位定義
        /// </summary>
        public List<ReportColumnDefinition> Columns { get; set; } = new();
        
        /// <summary>
        /// 頁尾區段定義
        /// </summary>
        public List<ReportFooterSection> FooterSections { get; set; } = new();
        
        /// <summary>
        /// 是否顯示頁碼
        /// </summary>
        public bool ShowPageNumbers { get; set; } = true;
        
        /// <summary>
        /// 紙張方向
        /// </summary>
        public PageOrientation Orientation { get; set; } = PageOrientation.Portrait;
        
        /// <summary>
        /// 紙張大小
        /// </summary>
        public PageSize PageSize { get; set; } = PageSize.ContinuousForm;
    }
    
    /// <summary>
    /// 報表頁首區段
    /// </summary>
    public class ReportHeaderSection
    {
        /// <summary>
        /// 區段標題
        /// </summary>
        public string Title { get; set; } = string.Empty;
        
        /// <summary>
        /// 區段內容欄位
        /// </summary>
        public List<ReportField> Fields { get; set; } = new();
        
        /// <summary>
        /// 欄位配置（決定每行顯示幾個欄位）
        /// </summary>
        public int FieldsPerRow { get; set; } = 2;
        
        /// <summary>
        /// 排序順序
        /// </summary>
        public int Order { get; set; } = 0;
        
        /// <summary>
        /// 是否顯示外框
        /// </summary>
        public bool HasBorder { get; set; } = false;
    }
    
    /// <summary>
    /// 報表欄位定義
    /// </summary>
    public class ReportField
    {
        /// <summary>
        /// 欄位標籤
        /// </summary>
        public string Label { get; set; } = string.Empty;
        
        /// <summary>
        /// 對應的屬性名稱
        /// </summary>
        public string PropertyName { get; set; } = string.Empty;
        
        /// <summary>
        /// 欄位值（可直接指定）
        /// </summary>
        public string? Value { get; set; }
        
        /// <summary>
        /// 格式化字串（如日期、數字格式）
        /// </summary>
        public string? Format { get; set; }
        
        /// <summary>
        /// 是否為粗體
        /// </summary>
        public bool IsBold { get; set; } = false;
        
        /// <summary>
        /// 欄位寬度權重（用於 CSS grid 或 flexbox）
        /// </summary>
        public int Width { get; set; } = 1;
    }
    
    /// <summary>
    /// 報表表格欄位定義
    /// </summary>
    public class ReportColumnDefinition
    {
        /// <summary>
        /// 欄位標題
        /// </summary>
        public string Header { get; set; } = string.Empty;
        
        /// <summary>
        /// 對應的屬性名稱
        /// </summary>
        public string PropertyName { get; set; } = string.Empty;
        
        /// <summary>
        /// 欄位寬度（百分比或固定值）
        /// </summary>
        public string Width { get; set; } = "auto";
        
        /// <summary>
        /// 文字對齊方式
        /// </summary>
        public TextAlignment Alignment { get; set; } = TextAlignment.Left;
        
        /// <summary>
        /// 格式化字串
        /// </summary>
        public string? Format { get; set; }
        
        /// <summary>
        /// 是否顯示
        /// </summary>
        public bool IsVisible { get; set; } = true;
        
        /// <summary>
        /// 排序順序
        /// </summary>
        public int Order { get; set; } = 0;
        
        /// <summary>
        /// 自訂值產生器（用於複雜邏輯）
        /// </summary>
        public Func<object, string>? ValueGenerator { get; set; }
    }
    
    /// <summary>
    /// 報表頁尾區段
    /// </summary>
    public class ReportFooterSection
    {
        /// <summary>
        /// 區段標題
        /// </summary>
        public string Title { get; set; } = string.Empty;
        
        /// <summary>
        /// 區段內容欄位
        /// </summary>
        public List<ReportField> Fields { get; set; } = new();
        
        /// <summary>
        /// 欄位配置
        /// </summary>
        public int FieldsPerRow { get; set; } = 2;
        
        /// <summary>
        /// 排序順序
        /// </summary>
        public int Order { get; set; } = 0;
        
        /// <summary>
        /// 是否為統計區段（數字加總等）
        /// </summary>
        public bool IsStatisticsSection { get; set; } = false;
        
        /// <summary>
        /// 頂部間距（以公分為單位）
        /// </summary>
        public int TopMargin { get; set; } = 0;
    }
    
    /// <summary>
    /// 報表資料容器
    /// </summary>
    /// <typeparam name="TMainEntity">主要實體類型</typeparam>
    /// <typeparam name="TDetailEntity">明細實體類型（可選）</typeparam>
    public class ReportData<TMainEntity, TDetailEntity> where TMainEntity : class
    {
        /// <summary>
        /// 主要實體資料
        /// </summary>
        public TMainEntity MainEntity { get; set; } = default!;
        
        /// <summary>
        /// 明細實體列表
        /// </summary>
        public List<TDetailEntity>? DetailEntities { get; set; }
        
        /// <summary>
        /// 額外的報表資料（如供應商資訊、商品資訊等）
        /// </summary>
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }
    
    /// <summary>
    /// 文字對齊方式
    /// </summary>
    public enum TextAlignment
    {
        [Description("左對齊")]
        Left,
        
        [Description("置中")]
        Center,
        
        [Description("右對齊")]
        Right
    }
    
    /// <summary>
    /// 頁面方向
    /// </summary>
    public enum PageOrientation
    {
        [Description("直向")]
        Portrait,
        
        [Description("橫向")]
        Landscape
    }
    
    /// <summary>
    /// 頁面大小
    /// </summary>
    public enum PageSize
    {
        [Description("中一刀報表紙 (8.46\" × 5.5\")")]
        ContinuousForm,
        
        [Description("A4 紙張 (8.27\" × 11.69\")")]
        A4,
        
        [Description("Letter 紙張 (8.5\" × 11\")")]
        Letter
    }
    
    /// <summary>
    /// 報表輸出格式
    /// </summary>
    public enum ReportFormat
    {
        [Description("HTML")]
        Html,
        
        [Description("Excel")]
        Excel
    }
}
