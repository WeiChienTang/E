namespace ERPCore2.Models.Enums
{
    /// <summary>
    /// 客訴狀態
    /// </summary>
    public enum ComplaintStatus
    {
        /// <summary>待處理</summary>
        Open = 1,
        /// <summary>處理中</summary>
        InProgress = 2,
        /// <summary>已解決</summary>
        Resolved = 3,
        /// <summary>已關閉</summary>
        Closed = 4
    }

    /// <summary>
    /// 客訴類別
    /// </summary>
    public enum ComplaintCategory
    {
        /// <summary>產品品質</summary>
        ItemQuality = 1,
        /// <summary>交期延誤</summary>
        DeliveryDelay = 2,
        /// <summary>服務態度</summary>
        ServiceAttitude = 3,
        /// <summary>價格爭議</summary>
        PriceDispute = 4,
        /// <summary>其他</summary>
        Other = 5
    }
}
