using ERPCore2.Data.Entities;
using ERPCore2.Services;

namespace ERPCore2.Services
{
    /// <summary>
    /// 銷貨退回明細服務介面
    /// </summary>
    public interface ISalesReturnDetailService : IGenericManagementService<SalesReturnDetail>
    {
        /// <summary>
        /// 根據銷貨退回ID取得明細清單
        /// </summary>
        /// <param name="salesReturnId">銷貨退回ID</param>
        /// <returns>明細清單</returns>
        Task<List<SalesReturnDetail>> GetBySalesReturnIdAsync(int salesReturnId);

        /// <summary>
        /// 根據產品ID取得退回明細清單
        /// </summary>
        /// <param name="productId">產品ID</param>
        /// <returns>明細清單</returns>
        Task<List<SalesReturnDetail>> GetByProductIdAsync(int productId);

        /// <summary>
        /// 根據銷貨訂單明細ID取得退回明細清單
        /// </summary>
        /// <param name="salesOrderDetailId">銷貨訂單明細ID</param>
        /// <returns>明細清單</returns>
        Task<List<SalesReturnDetail>> GetBySalesOrderDetailIdAsync(int salesOrderDetailId);



        /// <summary>
        /// 計算明細小計
        /// </summary>
        /// <param name="detail">明細實體</param>
        /// <returns>小計金額</returns>
        decimal CalculateSubtotal(SalesReturnDetail detail);

        /// <summary>
        /// 更新處理數量
        /// </summary>
        /// <param name="detailId">明細ID</param>
        /// <param name="processedQuantity">已處理數量</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> UpdateProcessedQuantityAsync(int detailId, decimal processedQuantity);

        /// <summary>
        /// 設定入庫資訊
        /// </summary>
        /// <param name="detailId">明細ID</param>
        /// <param name="restockedQuantity">入庫數量</param>
        /// <param name="qualityCondition">品質狀況</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> SetRestockInfoAsync(int detailId, decimal restockedQuantity, string? qualityCondition = null);

        /// <summary>
        /// 設定報廢數量
        /// </summary>
        /// <param name="detailId">明細ID</param>
        /// <param name="scrapQuantity">報廢數量</param>
        /// <param name="remarks">報廢原因</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> SetScrapQuantityAsync(int detailId, decimal scrapQuantity, string? remarks = null);

        /// <summary>
        /// 批量更新明細
        /// </summary>
        /// <param name="details">明細清單</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> UpdateDetailsAsync(List<SalesReturnDetail> details);

        /// <summary>
        /// 檢查明細是否可以刪除
        /// </summary>
        /// <param name="detailId">明細ID</param>
        /// <returns>是否可以刪除</returns>
        Task<bool> CanDeleteDetailAsync(int detailId);

        /// <summary>
        /// 取得明細統計資訊
        /// </summary>
        /// <param name="salesReturnId">銷貨退回ID</param>
        /// <returns>統計資訊</returns>
        Task<SalesReturnDetailStatistics> GetDetailStatisticsAsync(int salesReturnId);

        /// <summary>
        /// 驗證退回數量
        /// </summary>
        /// <param name="detail">明細實體</param>
        /// <returns>驗證結果</returns>
        Task<ServiceResult> ValidateReturnQuantityAsync(SalesReturnDetail detail);

        /// <summary>
        /// 取得指定銷售訂單明細的已退貨數量
        /// </summary>
        /// <param name="salesOrderDetailId">銷售訂單明細ID</param>
        /// <returns>已退貨數量</returns>
        Task<decimal> GetReturnedQuantityByOrderDetailAsync(int salesOrderDetailId);
    }

    /// <summary>
    /// 銷貨退回明細統計資訊
    /// </summary>
    public class SalesReturnDetailStatistics
    {
        public int TotalDetails { get; set; }
        public decimal TotalReturnQuantity { get; set; }
        public decimal TotalProcessedQuantity { get; set; }
        public decimal TotalPendingQuantity { get; set; }
        public decimal TotalRestockedQuantity { get; set; }
        public decimal TotalScrapQuantity { get; set; }
        public decimal TotalReturnAmount { get; set; }
        public int ProductCount { get; set; }
        public Dictionary<int, decimal> ProductReturnQuantities { get; set; } = new();
    }
}
