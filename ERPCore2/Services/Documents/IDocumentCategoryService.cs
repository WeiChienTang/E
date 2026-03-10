using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    public interface IDocumentCategoryService : IGenericManagementService<DocumentCategory>
    {
        Task<(List<DocumentCategory> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<DocumentCategory>, IQueryable<DocumentCategory>>? filterFunc,
            int pageNumber,
            int pageSize);

        Task<bool> IsDocumentCategoryCodeExistsAsync(string code, int? excludeId = null);
    }
}
