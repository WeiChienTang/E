using ERPCore2.Data.Enums;

namespace ERPCore2.Models;

/// <summary>
/// 進貨單搜尋模型
/// </summary>
public class PurchaseReceiptSearchModel
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
    public PurchaseReceiptStatus? ReceiptStatus { get; set; }

    /// <summary>
    /// 進貨日期起始
    /// </summary>
    public DateTime? ReceiptDateFrom { get; set; }

    /// <summary>
    /// 進貨日期結束
    /// </summary>
    public DateTime? ReceiptDateTo { get; set; }
}
