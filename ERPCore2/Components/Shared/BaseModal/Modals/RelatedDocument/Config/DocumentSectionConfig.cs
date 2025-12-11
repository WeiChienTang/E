using ERPCore2.Models;

namespace ERPCore2.Components.Shared.BaseModal.Modals.RelatedDocument.Config;

/// <summary>
/// 定義單據區塊的顯示配置
/// </summary>
public class DocumentSectionConfig
{
    /// <summary>
    /// 區塊標題（例如：「商品合成表」）
    /// </summary>
    public string Title { get; init; } = "";
    
    /// <summary>
    /// 標題圖示（Bootstrap Icons 類別名稱，例如：「diagram-3」）
    /// </summary>
    public string Icon { get; init; } = "";
    
    /// <summary>
    /// 標題文字顏色（Bootstrap 顏色類別，例如：「purple」）
    /// </summary>
    public string TextColor { get; init; } = "primary";
    
    /// <summary>
    /// Badge 背景顏色（Bootstrap 顏色類別）
    /// </summary>
    public string BadgeColor { get; init; } = "primary";
    
    /// <summary>
    /// Badge 文字顏色（例如：「text-dark」用於淺色背景）
    /// </summary>
    public string BadgeTextClass { get; init; } = "";
    
    /// <summary>
    /// 是否顯示「新增」按鈕
    /// </summary>
    public bool ShowAddButton { get; init; }
    
    /// <summary>
    /// 「新增」按鈕的文字
    /// </summary>
    public string AddButtonText { get; init; } = "+ 新增";
    
    /// <summary>
    /// 根據單據類型取得對應的配置
    /// </summary>
    public static DocumentSectionConfig GetConfig(RelatedDocumentType type)
    {
        return type switch
        {
            RelatedDocumentType.ProductComposition => new()
            {
                Title = "商品合成表",
                Icon = "diagram-3",
                TextColor = "purple",
                BadgeColor = "purple",
                ShowAddButton = true,
                AddButtonText = "+ 新增合成表"
            },
            
            RelatedDocumentType.SalesOrder => new()
            {
                Title = "銷貨訂單",
                Icon = "cart-check",
                TextColor = "primary",
                BadgeColor = "primary",
                ShowAddButton = false
            },
            
            RelatedDocumentType.ReceivingDocument => new()
            {
                Title = "入庫記錄",
                Icon = "box-seam",
                TextColor = "info",
                BadgeColor = "info",
                ShowAddButton = false
            },
            
            RelatedDocumentType.ReturnDocument => new()
            {
                Title = "退貨記錄",
                Icon = "arrow-return-left",
                TextColor = "warning",
                BadgeColor = "warning",
                BadgeTextClass = "text-dark",
                ShowAddButton = false
            },
            
            RelatedDocumentType.SetoffDocument => new()
            {
                Title = "沖款記錄",
                Icon = "cash-coin",
                TextColor = "success",
                BadgeColor = "success",
                ShowAddButton = false
            },
            
            RelatedDocumentType.DeliveryDocument => new()
            {
                Title = "出貨記錄",
                Icon = "truck",
                TextColor = "info",
                BadgeColor = "info",
                ShowAddButton = false
            },
            
            RelatedDocumentType.ProductionSchedule => new()
            {
                Title = "生產排程",
                Icon = "calendar-check",
                TextColor = "dark",
                BadgeColor = "dark",
                ShowAddButton = false
            },
            
            RelatedDocumentType.SupplierRecommendation => new()
            {
                Title = "供應商推薦",
                Icon = "shop",
                TextColor = "success",
                BadgeColor = "success",
                ShowAddButton = true,
                AddButtonText = "+ 立即採購"
            },
            
            _ => throw new ArgumentException($"未知的單據類型: {type}")
        };
    }
}
