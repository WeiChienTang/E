using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;

namespace ERPCore2.Services
{
    /// <summary>
    /// 沖銷商品明細服務介面
    /// </summary>
    public interface ISetoffProductDetailService : IGenericManagementService<SetoffProductDetail>
    {
        /// <summary>
        /// 根據沖款單ID取得商品明細
        /// </summary>
        Task<List<SetoffProductDetail>> GetBySetoffDocumentIdAsync(int setoffDocumentId);

        /// <summary>
        /// 取得特定關聯方的未結清明細（應收帳款）
        /// </summary>
        /// <param name="customerId">客戶ID</param>
        /// <returns>未結清的銷貨訂單明細和銷貨退回明細</returns>
        Task<List<UnsettledDetailDto>> GetUnsettledReceivableDetailsAsync(int customerId);

        /// <summary>
        /// 取得特定關聯方的未結清明細（應付帳款）
        /// </summary>
        /// <param name="supplierId">廠商ID</param>
        /// <returns>未結清的採購進貨明細和採購退回明細</returns>
        Task<List<UnsettledDetailDto>> GetUnsettledPayableDetailsAsync(int supplierId);

        /// <summary>
        /// 根據來源明細類型和ID取得明細資訊（不論是否結清）
        /// </summary>
        /// <param name="sourceDetailType">來源明細類型</param>
        /// <param name="sourceDetailId">來源明細ID</param>
        /// <returns>來源明細資訊，若找不到則返回 null</returns>
        Task<UnsettledDetailDto?> GetSourceDetailInfoAsync(
            SetoffDetailType sourceDetailType, 
            int sourceDetailId);

        /// <summary>
        /// 更新來源明細的累計沖款金額
        /// </summary>
        Task<ServiceResult> UpdateSourceDetailTotalAmountAsync(
            int sourceDetailId, 
            SetoffDetailType sourceType);

        /// <summary>
        /// 批次建立沖銷商品明細
        /// </summary>
        Task<ServiceResult> CreateBatchWithValidationAsync(List<SetoffProductDetail> details);

        /// <summary>
        /// 檢查沖款金額是否超過未結清餘額
        /// </summary>
        Task<ServiceResult> ValidateSetoffAmountAsync(
            int sourceDetailId, 
            SetoffDetailType sourceType, 
            decimal currentSetoffAmount,
            decimal currentAllowanceAmount);
    }

    /// <summary>
    /// 未結清明細 DTO - 用於顯示可沖銷的商品明細
    /// </summary>
    public class UnsettledDetailDto
    {
        /// <summary>來源明細類型</summary>
        public SetoffDetailType SourceDetailType { get; set; }

        /// <summary>來源明細ID</summary>
        public int SourceDetailId { get; set; }

        /// <summary>來源單號</summary>
        public string SourceDocumentNumber { get; set; } = string.Empty;

        /// <summary>商品ID</summary>
        public int ProductId { get; set; }

        /// <summary>商品名稱</summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>商品編號</summary>
        public string ProductCode { get; set; } = string.Empty;

        /// <summary>應收/應付金額（小計金額）</summary>
        public decimal TotalAmount { get; set; }

        /// <summary>已沖款金額（從來源明細表取得）</summary>
        public decimal PaidAmount { get; set; }

        /// <summary>歷史累計沖款金額（從 SetoffProductDetail 表查詢）</summary>
        public decimal TotalHistoricalSetoffAmount { get; set; }

        /// <summary>歷史累計折讓金額（從 SetoffProductDetail 表查詢）</summary>
        public decimal TotalHistoricalAllowanceAmount { get; set; }

        /// <summary>未沖款餘額</summary>
        public decimal RemainingAmount => TotalAmount - PaidAmount;

        /// <summary>單據日期</summary>
        public DateTime DocumentDate { get; set; }

        /// <summary>是否為退貨/退回（金額為負）</summary>
        public bool IsReturn { get; set; }
    }
}
