using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    public interface IDocumentService : IGenericManagementService<Document>
    {
        Task<List<Document>> GetByCategoryAsync(int categoryId);
        Task<bool> IsDocumentCodeExistsAsync(string code, int? excludeId = null);
        Task<Document?> GetWithFilesAsync(int id);

        #region 伺服器端分頁

        /// <summary>
        /// 取得分頁資料（支援動態篩選）
        /// </summary>
        Task<(List<Document> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<Document>, IQueryable<Document>>? filterFunc,
            int pageNumber,
            int pageSize);

        #endregion
    }
}
