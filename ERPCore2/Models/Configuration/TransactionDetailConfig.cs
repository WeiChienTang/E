using ERPCore2.Models.Enums;

namespace ERPCore2.Models.Configuration
{
    /// <summary>
    /// 交易明細管理組件配置
    /// </summary>
    public class TransactionDetailConfig
    {
        /// <summary>組件標題</summary>
        public string Title { get; set; } = "明細";
        
        /// <summary>新增按鈕文字</summary>
        public string AddButtonText { get; set; } = "新增商品";
        
        /// <summary>圖標CSS類別</summary>
        public string IconClass { get; set; } = "fas fa-list";
        
        /// <summary>是否為只讀模式</summary>
        public bool IsReadOnly { get; set; } = false;
        
        /// <summary>數量欄位標題</summary>
        public string QuantityColumnTitle { get; set; } = "數量";
        
        /// <summary>預期日期欄位標題</summary>
        public string ExpectedDateColumnTitle { get; set; } = "預計日期";
        
        /// <summary>庫存欄位標題</summary>
        public string StockColumnTitle { get; set; } = "庫存";
        
        /// <summary>空狀態訊息</summary>
        public string EmptyStateMessage { get; set; } = "商品";
        
        /// <summary>是否顯示預期日期欄位</summary>
        public bool ShowExpectedDateColumn { get; set; } = true;
        
        /// <summary>是否顯示備註欄位</summary>
        public bool ShowRemarksColumn { get; set; } = false;
        
        /// <summary>數量格式化字串</summary>
        public string QuantityFormat { get; set; } = "N0";
        
        /// <summary>庫存影響類型</summary>
        public InventoryImpactType InventoryImpact { get; set; } = InventoryImpactType.None;

        /// <summary>
        /// 取得預設配置
        /// </summary>
        /// <param name="transactionType">交易類型</param>
        /// <returns>預設配置</returns>
        public static TransactionDetailConfig GetDefaultConfig(TransactionDetailType transactionType)
        {
            return transactionType switch
            {
                TransactionDetailType.Purchase => new TransactionDetailConfig
                {
                    Title = "採購明細",
                    IconClass = "fas fa-shopping-cart",
                    QuantityColumnTitle = "訂購數量",
                    ExpectedDateColumnTitle = "預計到貨",
                    StockColumnTitle = "庫存",
                    EmptyStateMessage = "採購商品",
                    ShowExpectedDateColumn = true,
                    ShowRemarksColumn = false,
                    InventoryImpact = InventoryImpactType.Increase
                },
                TransactionDetailType.PurchaseReturn => new TransactionDetailConfig
                {
                    Title = "採購退貨明細",
                    IconClass = "fas fa-undo",
                    QuantityColumnTitle = "退貨數量",
                    ExpectedDateColumnTitle = "預計退貨日",
                    StockColumnTitle = "庫存",
                    EmptyStateMessage = "退貨商品",
                    ShowExpectedDateColumn = true,
                    ShowRemarksColumn = true,
                    InventoryImpact = InventoryImpactType.Decrease
                },
                TransactionDetailType.Sale => new TransactionDetailConfig
                {
                    Title = "銷售明細",
                    IconClass = "fas fa-cash-register",
                    QuantityColumnTitle = "銷售數量",
                    ExpectedDateColumnTitle = "預計出貨",
                    StockColumnTitle = "可用庫存",
                    EmptyStateMessage = "銷售商品",
                    ShowExpectedDateColumn = true,
                    ShowRemarksColumn = false,
                    InventoryImpact = InventoryImpactType.Decrease
                },
                TransactionDetailType.SaleReturn => new TransactionDetailConfig
                {
                    Title = "銷售退貨明細",
                    IconClass = "fas fa-receipt",
                    QuantityColumnTitle = "退貨數量",
                    ExpectedDateColumnTitle = "預計收貨日",
                    StockColumnTitle = "庫存",
                    EmptyStateMessage = "退貨商品",
                    ShowExpectedDateColumn = true,
                    ShowRemarksColumn = true,
                    InventoryImpact = InventoryImpactType.Increase
                },
                TransactionDetailType.InventoryAdjustment => new TransactionDetailConfig
                {
                    Title = "庫存調整明細",
                    IconClass = "fas fa-balance-scale",
                    QuantityColumnTitle = "調整數量",
                    ExpectedDateColumnTitle = "調整日期",
                    StockColumnTitle = "當前庫存",
                    EmptyStateMessage = "調整商品",
                    ShowExpectedDateColumn = false,
                    ShowRemarksColumn = true,
                    InventoryImpact = InventoryImpactType.None
                },
                _ => new TransactionDetailConfig()
            };
        }
    }
}
