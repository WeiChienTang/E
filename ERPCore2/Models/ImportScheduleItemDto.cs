namespace ERPCore2.Models
{
    /// <summary>
    /// 匯入排程項目 DTO - 用於庫存匯入排程 Modal
    /// </summary>
    public class ImportScheduleItemDto
    {
        /// <summary>
        /// ProductionScheduleItem ID
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// 序號（顯示用）
        /// </summary>
        public int RowNumber { get; set; }
        
        /// <summary>
        /// 銷售訂單編號
        /// </summary>
        public string SalesOrderCode { get; set; } = string.Empty;
        
        /// <summary>
        /// 客戶名稱
        /// </summary>
        public string CustomerName { get; set; } = string.Empty;
        
        /// <summary>
        /// 商品 ID
        /// </summary>
        public int ProductId { get; set; }
        
        /// <summary>
        /// 商品編號
        /// </summary>
        public string ProductCode { get; set; } = string.Empty;
        
        /// <summary>
        /// 商品名稱
        /// </summary>
        public string ProductName { get; set; } = string.Empty;
        
        /// <summary>
        /// 倉庫 ID（使用者選擇）
        /// </summary>
        public int? WarehouseId { get; set; }
        
        /// <summary>
        /// 庫位 ID（使用者選擇，可為空）
        /// </summary>
        public int? WarehouseLocationId { get; set; }
        
        /// <summary>
        /// 選取的倉庫位置 Key（格式：WarehouseId|WarehouseLocationId 或 WarehouseId|）
        /// 用於合併的倉庫+庫位下拉選單
        /// </summary>
        public string? SelectedLocationKey { get; set; }
        
        /// <summary>
        /// 解析 SelectedLocationKey 並設定 WarehouseId 和 WarehouseLocationId
        /// </summary>
        public void ParseLocationKey()
        {
            if (string.IsNullOrEmpty(SelectedLocationKey))
            {
                WarehouseId = null;
                WarehouseLocationId = null;
                return;
            }
            
            var parts = SelectedLocationKey.Split('|');
            if (parts.Length >= 1 && int.TryParse(parts[0], out var warehouseId))
            {
                WarehouseId = warehouseId;
            }
            if (parts.Length >= 2 && int.TryParse(parts[1], out var locationId))
            {
                WarehouseLocationId = locationId;
            }
            else
            {
                WarehouseLocationId = null;
            }
        }
        
        /// <summary>
        /// 排程數量
        /// </summary>
        public decimal ScheduledQuantity { get; set; }
        
        /// <summary>
        /// 已完成數量
        /// </summary>
        public decimal CompletedQuantity { get; set; }
        
        /// <summary>
        /// 待匯入數量（排程數量 - 已完成數量）
        /// </summary>
        public decimal PendingQuantity => ScheduledQuantity - CompletedQuantity;
        
        /// <summary>
        /// 本次完成數量（使用者輸入，預設為 PendingQuantity）
        /// </summary>
        public decimal ImportQuantity { get; set; }
        
        /// <summary>
        /// 是否被選取
        /// </summary>
        public bool IsSelected { get; set; }
        
        /// <summary>
        /// 是否結案（入庫後標記為不再生產）
        /// </summary>
        public bool IsClosed { get; set; }
    }
}
