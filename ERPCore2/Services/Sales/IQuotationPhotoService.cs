using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 報價單照片服務介面
    /// </summary>
    public interface IQuotationPhotoService : IGenericManagementService<QuotationPhoto>
    {
        Task<List<QuotationPhoto>> GetByQuotationAsync(int quotationId);
    }
}
