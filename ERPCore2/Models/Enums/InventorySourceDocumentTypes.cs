namespace ERPCore2.Models.Enums
{
    /// <summary>
    /// 庫存異動來源單據類型（使用常數類別以保持彈性）
    /// </summary>
    public static class InventorySourceDocumentTypes
    {
        /// <summary>
        /// 採購進貨
        /// </summary>
        public const string PurchaseReceiving = "PurchaseReceiving";
        
        /// <summary>
        /// 採購退貨（進貨退出）
        /// </summary>
        public const string PurchaseReturn = "PurchaseReturn";
        
        /// <summary>
        /// 銷貨出貨
        /// </summary>
        public const string SalesDelivery = "SalesDelivery";
        
        /// <summary>
        /// 銷貨退回
        /// </summary>
        public const string SalesReturn = "SalesReturn";
        
        /// <summary>
        /// 盤點調整
        /// </summary>
        public const string StockTaking = "StockTaking";
        
        /// <summary>
        /// 領料出庫
        /// </summary>
        public const string MaterialIssue = "MaterialIssue";
        
        /// <summary>
        /// 領料退回
        /// </summary>
        public const string MaterialReturn = "MaterialReturn";
        
        /// <summary>
        /// 倉庫調撥
        /// </summary>
        public const string Transfer = "Transfer";
        
        /// <summary>
        /// 手動調整
        /// </summary>
        public const string Adjustment = "Adjustment";
        
        /// <summary>
        /// 期初建立
        /// </summary>
        public const string Initial = "Initial";
        
        /// <summary>
        /// 生產投料
        /// </summary>
        public const string ProductionConsumption = "ProductionConsumption";
        
        /// <summary>
        /// 生產完工
        /// </summary>
        public const string ProductionCompletion = "ProductionCompletion";
        
        /// <summary>
        /// 報廢
        /// </summary>
        public const string Scrap = "Scrap";

        /// <summary>
        /// 廢料收料
        /// </summary>
        public const string WasteRecord = "WasteRecord";
        
        /// <summary>
        /// 取得來源類型的顯示名稱
        /// </summary>
        public static string GetDisplayName(string? sourceType)
        {
            return sourceType switch
            {
                PurchaseReceiving => "採購進貨",
                PurchaseReturn => "採購退貨",
                SalesDelivery => "銷貨出貨",
                SalesReturn => "銷貨退回",
                StockTaking => "盤點調整",
                MaterialIssue => "領料出庫",
                MaterialReturn => "領料退回",
                Transfer => "倉庫調撥",
                Adjustment => "手動調整",
                Initial => "期初建立",
                ProductionConsumption => "生產投料",
                ProductionCompletion => "生產完工",
                Scrap => "報廢",
                WasteRecord => "廢料收料",
                _ => sourceType ?? "未知"
            };
        }
    }
}
