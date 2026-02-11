using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Models.Enums
{
    /// <summary>
    /// 儀表板小工具分類
    /// </summary>
    public enum DashboardWidgetCategory
    {
        [Display(Name = "人力資源管理")]
        HumanResource = 1,

        [Display(Name = "供應鏈管理")]
        SupplyChain = 2,

        [Display(Name = "客戶關係管理")]
        CustomerRelation = 3,

        [Display(Name = "商品管理")]
        ProductManagement = 4,

        [Display(Name = "庫存管理")]
        InventoryManagement = 5,

        [Display(Name = "採購管理")]
        PurchaseManagement = 6,

        [Display(Name = "銷售管理")]
        SalesManagement = 7,

        [Display(Name = "財務管理")]
        FinancialManagement = 8,

        [Display(Name = "系統管理")]
        SystemManagement = 9
    }

    /// <summary>
    /// 小工具點擊行為類型
    /// </summary>
    public enum DashboardWidgetAction
    {
        [Display(Name = "導航至頁面")]
        Route = 1,

        [Display(Name = "觸發動作")]
        Action = 2
    }
}
