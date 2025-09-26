using ERPCore2.Data.Entities;
using ERPCore2.Services;

namespace ERPCore2.Services
{
    /// <summary>
    /// 應收帳款沖款服務介面
    /// </summary>
    public interface IAccountsReceivableSetoffService : IGenericManagementService<AccountsReceivableSetoff>
    {
        /// <summary>
        /// 取得所有沖款單 (包含相關實體)
        /// </summary>
        /// <returns>沖款單清單</returns>
        Task<List<AccountsReceivableSetoff>> GetAllWithDetailsAsync();

        /// <summary>
        /// 根據ID取得沖款單 (包含明細)
        /// </summary>
        /// <param name="id">沖款單ID</param>
        /// <returns>沖款單實體</returns>
        Task<AccountsReceivableSetoff?> GetByIdWithDetailsAsync(int id);

        /// <summary>
        /// 取得指定客戶的沖款單
        /// </summary>
        /// <param name="customerId">客戶ID</param>
        /// <returns>沖款單清單</returns>
        Task<List<AccountsReceivableSetoff>> GetByCustomerIdAsync(int customerId);

        /// <summary>
        /// 取得未完成的沖款單
        /// </summary>
        /// <returns>未完成沖款單清單</returns>
        Task<List<AccountsReceivableSetoff>> GetIncompleteAsync();

        /// <summary>
        /// 取得待審核的沖款單
        /// </summary>
        /// <returns>待審核沖款單清單</returns>
        Task<List<AccountsReceivableSetoff>> GetPendingApprovalAsync();

        /// <summary>
        /// 生成沖款單號
        /// </summary>
        /// <returns>沖款單號</returns>
        Task<string> GenerateSetoffNumberAsync();

        /// <summary>
        /// 檢查沖款單號是否存在
        /// </summary>
        /// <param name="setoffNumber">沖款單號</param>
        /// <param name="excludeId">排除的ID</param>
        /// <returns>是否存在</returns>
        Task<bool> IsSetoffNumberExistsAsync(string setoffNumber, int? excludeId = null);

        /// <summary>
        /// 完成沖款
        /// </summary>
        /// <param name="id">沖款單ID</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> CompleteSetoffAsync(int id);

        /// <summary>
        /// 取消沖款
        /// </summary>
        /// <param name="id">沖款單ID</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> CancelSetoffAsync(int id);

        /// <summary>
        /// 審核沖款單
        /// </summary>
        /// <param name="id">沖款單ID</param>
        /// <param name="approverId">審核者ID</param>
        /// <param name="isApproved">是否通過</param>
        /// <param name="remarks">審核備註</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> ApproveSetoffAsync(int id, int approverId, bool isApproved, string? remarks = null);

        /// <summary>
        /// 取得沖款統計資訊
        /// </summary>
        /// <returns>統計資訊</returns>
        Task<SetoffStatistics> GetStatisticsAsync();

        /// <summary>
        /// 取得可沖款的銷貨明細
        /// </summary>
        /// <param name="customerId">客戶ID</param>
        /// <returns>可沖款明細清單</returns>
        Task<List<ReceivableDetailViewModel>> GetReceivableDetailsAsync(int customerId);
    }

    /// <summary>
    /// 應收帳款沖款明細服務介面
    /// </summary>
    public interface IAccountsReceivableSetoffDetailService : IGenericManagementService<AccountsReceivableSetoffDetail>
    {
        /// <summary>
        /// 根據沖款單ID取得明細
        /// </summary>
        /// <param name="setoffId">沖款單ID</param>
        /// <returns>明細清單</returns>
        Task<List<AccountsReceivableSetoffDetail>> GetBySetoffIdAsync(int setoffId);

        /// <summary>
        /// 批量建立沖款明細
        /// </summary>
        /// <param name="details">明細清單</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> CreateBatchAsync(List<AccountsReceivableSetoffDetail> details);

        /// <summary>
        /// 批量更新沖款明細
        /// </summary>
        /// <param name="details">明細清單</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> UpdateBatchAsync(List<AccountsReceivableSetoffDetail> details);

        /// <summary>
        /// 驗證沖款明細的合理性
        /// </summary>
        /// <param name="detail">明細實體</param>
        /// <returns>驗證結果</returns>
        Task<ServiceResult> ValidateSetoffDetailAsync(AccountsReceivableSetoffDetail detail);
    }

    /// <summary>
    /// 沖款統計資訊
    /// </summary>
    public class SetoffStatistics
    {
        /// <summary>
        /// 總沖款單數量
        /// </summary>
        public int TotalSetoffCount { get; set; }

        /// <summary>
        /// 未完成沖款單數量
        /// </summary>
        public int IncompleteSetoffCount { get; set; }

        /// <summary>
        /// 待審核沖款單數量
        /// </summary>
        public int PendingApprovalCount { get; set; }

        /// <summary>
        /// 本月沖款總額
        /// </summary>
        public decimal MonthlySetoffAmount { get; set; }

        /// <summary>
        /// 今日沖款總額
        /// </summary>
        public decimal TodaySetoffAmount { get; set; }

        /// <summary>
        /// 平均沖款金額
        /// </summary>
        public decimal AverageSetoffAmount { get; set; }
    }

    /// <summary>
    /// 可沖款明細視圖模型
    /// </summary>
    public class ReceivableDetailViewModel
    {
        /// <summary>
        /// 明細ID
        /// </summary>
        public int DetailId { get; set; }

        /// <summary>
        /// 單據類型
        /// </summary>
        public string DocumentType { get; set; } = string.Empty;

        /// <summary>
        /// 單據編號
        /// </summary>
        public string DocumentNumber { get; set; } = string.Empty;

        /// <summary>
        /// 單據日期
        /// </summary>
        public DateTime DocumentDate { get; set; }

        /// <summary>
        /// 商品名稱
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// 數量
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// 單位
        /// </summary>
        public string? UnitName { get; set; }

        /// <summary>
        /// 應收金額
        /// </summary>
        public decimal ReceivableAmount { get; set; }

        /// <summary>
        /// 已收金額
        /// </summary>
        public decimal ReceivedAmount { get; set; }

        /// <summary>
        /// 餘額
        /// </summary>
        public decimal BalanceAmount => ReceivableAmount - ReceivedAmount;

        /// <summary>
        /// 是否結清
        /// </summary>
        public bool IsSettled => BalanceAmount <= 0;
    }
}