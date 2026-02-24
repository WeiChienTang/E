using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 子科目自動產生服務介面
    /// 負責在統制科目下自動建立與客戶/廠商/商品對應的明細子科目
    /// 代碼格式：{上層科目代碼}.{流水號3位}（例如 1191.001）
    /// </summary>
    public interface ISubAccountService
    {
        // ===== 查詢（不建立）=====

        /// <summary>取得客戶對應的應收帳款子科目（若不存在則回傳 null）</summary>
        Task<AccountItem?> GetSubAccountForCustomerAsync(int customerId);

        /// <summary>取得廠商對應的應付帳款子科目（若不存在則回傳 null）</summary>
        Task<AccountItem?> GetSubAccountForSupplierAsync(int supplierId);

        /// <summary>取得商品對應的存貨子科目（若不存在則回傳 null）</summary>
        Task<AccountItem?> GetSubAccountForProductAsync(int productId);

        // ===== 建立（若已存在則直接回傳）=====

        /// <summary>
        /// 取得或建立客戶子科目。
        /// 若系統參數未啟用 AutoCreateCustomerSubAccount，回傳 null。
        /// </summary>
        Task<AccountItem?> GetOrCreateCustomerSubAccountAsync(int customerId, string createdBy);

        /// <summary>
        /// 取得或建立廠商子科目。
        /// 若系統參數未啟用 AutoCreateSupplierSubAccount，回傳 null。
        /// </summary>
        Task<AccountItem?> GetOrCreateSupplierSubAccountAsync(int supplierId, string createdBy);

        /// <summary>
        /// 取得或建立商品子科目。
        /// 若系統參數未啟用 AutoCreateProductSubAccount，回傳 null。
        /// </summary>
        Task<AccountItem?> GetOrCreateProductSubAccountAsync(int productId, string createdBy);

        // ===== 批次補建（為現有資料補建子科目）=====

        /// <summary>為所有現有 Active 客戶補建子科目（已有子科目者略過）</summary>
        Task<(int Created, int Skipped, List<string> Errors)> BatchCreateForAllCustomersAsync(string createdBy);

        /// <summary>為所有現有 Active 廠商補建子科目（已有子科目者略過）</summary>
        Task<(int Created, int Skipped, List<string> Errors)> BatchCreateForAllSuppliersAsync(string createdBy);

        /// <summary>為所有現有 Active 商品補建子科目（已有子科目者略過）</summary>
        Task<(int Created, int Skipped, List<string> Errors)> BatchCreateForAllProductsAsync(string createdBy);
    }
}
