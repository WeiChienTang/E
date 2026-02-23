using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 批次轉傳票服務介面
    /// 負責查詢待轉傳票的業務單據，並依據標準三行分錄規則（含稅）自動建立並過帳傳票
    /// </summary>
    public interface IJournalEntryAutoGenerationService
    {
        /// <summary>
        /// 查詢尚未轉傳票的進貨入庫單
        /// </summary>
        Task<List<PurchaseReceiving>> GetPendingPurchaseReceivingsAsync(DateTime? from = null, DateTime? to = null);

        /// <summary>
        /// 查詢尚未轉傳票的進貨退回單
        /// </summary>
        Task<List<PurchaseReturn>> GetPendingPurchaseReturnsAsync(DateTime? from = null, DateTime? to = null);

        /// <summary>
        /// 查詢尚未轉傳票的銷貨出貨單
        /// </summary>
        Task<List<SalesDelivery>> GetPendingSalesDeliveriesAsync(DateTime? from = null, DateTime? to = null);

        /// <summary>
        /// 查詢尚未轉傳票的銷貨退回單
        /// </summary>
        Task<List<SalesReturn>> GetPendingSalesReturnsAsync(DateTime? from = null, DateTime? to = null);

        /// <summary>
        /// 將指定進貨入庫單轉為傳票（借：商品存貨+進項稅額 / 貸：應付帳款）
        /// </summary>
        Task<(bool Success, string ErrorMessage)> JournalizePurchaseReceivingAsync(int id, string createdBy);

        /// <summary>
        /// 將指定進貨退回單轉為傳票（借：應付帳款 / 貸：商品存貨+進項稅額）
        /// </summary>
        Task<(bool Success, string ErrorMessage)> JournalizePurchaseReturnAsync(int id, string createdBy);

        /// <summary>
        /// 將指定銷貨出貨單轉為傳票（借：應收帳款 / 貸：銷貨收入+銷項稅額）
        /// </summary>
        Task<(bool Success, string ErrorMessage)> JournalizeSalesDeliveryAsync(int id, string createdBy);

        /// <summary>
        /// 將指定銷貨退回單轉為傳票（借：銷貨收入+銷項稅額 / 貸：應收帳款）
        /// </summary>
        Task<(bool Success, string ErrorMessage)> JournalizeSalesReturnAsync(int id, string createdBy);
    }
}
