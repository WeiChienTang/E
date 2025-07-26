using ERPCore2.Data.Enums;

namespace ERPCore2.Models;

/// <summary>
/// 進貨單搜尋模型
/// </summary>
public class PurchaseReceivingSearchModel
{
    /// <summary>
    /// 搜尋關鍵字
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// 採購訂單ID
    /// </summary>
    public int? PurchaseOrderId { get; set; }

    /// <summary>
    /// 倉庫ID
    /// </summary>
    public int? WarehouseId { get; set; }

    /// <summary>
    /// 進貨狀態
    /// </summary>
    public PurchaseReceivingStatus? ReceiptStatus { get; set; }

    /// <summary>
    /// 開始日期
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 結束日期
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 供應商ID
    /// </summary>
    public int? SupplierId { get; set; }
}
