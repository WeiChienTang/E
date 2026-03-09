namespace ERPCore2.Helpers.EditModal;

/// <summary>
/// Edit Modal 共用訊息常數
/// 集中管理各 EditModalComponent 中重複使用的警告訊息文字
/// </summary>
public static class EditModalMessages
{
    /// <summary>
    /// 通用審核通過的欄位鎖定警告（緊湊模式用）
    /// </summary>
    public const string ApprovedWarning =
        "已審核通過 — 主檔欄位已鎖定，如需修改請先執行「駁回」";

    /// <summary>
    /// 明細有其他動作時的欄位鎖定警告
    /// 適用於：採購單、進貨單、銷貨單、出貨單等所有有明細關聯的單據
    /// </summary>
    public const string UndeletableDetailsWarning =
        "部分明細有關聯操作 — 主檔欄位已鎖定";

    /// <summary>
    /// 明細鎖定時的操作提示（紅字補充說明）
    /// </summary>
    public const string UndeletableDetailsHint =
        "仍可在下方明細新增補充項目";

    /// <summary>
    /// 採購單審核通過的欄位鎖定警告
    /// </summary>
    public const string PurchaseOrderApprovedWarning =
        "採購單已審核 — 欄位已鎖定，若需修改請主管先執行「駁回」";

    /// <summary>
    /// 銷貨單審核通過的欄位鎖定警告
    /// </summary>
    public const string SalesOrderApprovedWarning =
        "銷貨訂單已審核 — 欄位已鎖定，若需修改請主管先執行「駁回」";
}
