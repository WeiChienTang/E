namespace ERPCore2.Models.Schedule
{
    /// <summary>
    /// 用料結算 DTO - 最後一次完工時傳入，記錄每個 BOM 組件的實際用料去向
    /// </summary>
    public class MaterialSettlementDto
    {
        /// <summary>對應的 BOM 組件行（ProductionScheduleDetail.Id）</summary>
        public int ProductionScheduleDetailId { get; set; }

        /// <summary>實際消耗量</summary>
        public decimal ActualUsedQty { get; set; }

        /// <summary>退料量（0 = 不退）</summary>
        public decimal ReturnQty { get; set; }

        /// <summary>退料目標倉庫（ReturnQty > 0 時必填）</summary>
        public int? ReturnWarehouseId { get; set; }

        /// <summary>退料目標庫位（可選）</summary>
        public int? ReturnLocationId { get; set; }

        /// <summary>損耗量（0 = 無損耗）</summary>
        public decimal ScrapQty { get; set; }

        /// <summary>損耗備註（純文字）</summary>
        public string? ScrapReason { get; set; }
    }
}
