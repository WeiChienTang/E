using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 系統參數服務介面
    /// </summary>
    public interface ISystemParameterService : IGenericManagementService<SystemParameter>
    {
        /// <summary>
        /// 取得系統稅率
        /// </summary>
        /// <returns>當前系統稅率</returns>
        Task<decimal> GetTaxRateAsync();

        /// <summary>
        /// 設定系統稅率
        /// </summary>
        /// <param name="taxRate">稅率值</param>
        /// <returns>是否設定成功</returns>
        Task<bool> SetTaxRateAsync(decimal taxRate);

        /// <summary>
        /// 取得第一個系統參數設定
        /// </summary>
        /// <returns>系統參數物件</returns>
        Task<SystemParameter?> GetSystemParameterAsync();
    }
}