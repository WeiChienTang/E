namespace ERPCore2.Helpers.EditModal;

/// <summary>
/// Edit Modal 共用訊息常數
/// 集中管理各 EditModalComponent 中重複使用的警告訊息文字
/// </summary>
public static class EditModalMessages
{
    /// <summary>
    /// 明細有其他動作時的欄位鎖定警告
    /// 適用於：採購單、進貨單、銷貨單、出貨單等所有有明細關聯的單據
    /// </summary>
    public const string UndeletableDetailsWarning =
        "因部分明細有其他動作，為保護資料完整性主檔欄位已設唯讀。";

    /// <summary>
    /// 採購單審核通過的欄位鎖定警告
    /// </summary>
    public const string PurchaseOrderApprovedWarning =
        "採購單已審核通過，主檔欄位已鎖定。您仍可修改明細的「完成進貨」狀態並儲存。";

    /// <summary>
    /// 銷貨單審核通過的欄位鎖定警告
    /// </summary>
    public const string SalesOrderApprovedWarning =
        "銷貨訂單已審核通過，主檔欄位已鎖定。您仍可修改明細的「完成出貨」狀態並儲存。";
}
