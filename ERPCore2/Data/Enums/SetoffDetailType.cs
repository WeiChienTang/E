using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Data.Enums
{
    /// <summary>
    /// 沖銷明細類型列舉 - 定義可進行沖銷的單據明細類型
    /// </summary>
    public enum SetoffDetailType
    {
        /// <summary>銷貨訂單明細</summary>
        [Display(Name = "銷貨明細")]
        SalesOrderDetail = 1,
        
        /// <summary>銷貨退回明細</summary>
        [Display(Name = "銷貨退回明細")]
        SalesReturnDetail = 2,
        
        /// <summary>採購進貨明細</summary>
        [Display(Name = "入庫明細")]
        PurchaseReceivingDetail = 3,
        
        /// <summary>採購退回明細</summary>
        [Display(Name = "入庫退回明細")]
        PurchaseReturnDetail = 4
    }
}
