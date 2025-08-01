namespace ERPCore2.Models;

/// <summary>
/// 採購訂單搜尋模型
/// </summary>
public class PurchaseOrderSearchModel
{
    /// <summary>
    /// 搜尋關鍵字
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// 供應商ID
    /// </summary>
    public int? SupplierId { get; set; }

    /// <summary>
    /// 倉庫ID
    /// </summary>
    public int? WarehouseId { get; set; }

    /// <summary>
    /// 訂單開始日期
    /// </summary>
    public DateTime? OrderDateFrom { get; set; }

    /// <summary>
    /// 訂單結束日期
    /// </summary>
    public DateTime? OrderDateTo { get; set; }

    /// <summary>
    /// 採購人員
    /// </summary>
    public string? PurchasePersonnel { get; set; }
}
