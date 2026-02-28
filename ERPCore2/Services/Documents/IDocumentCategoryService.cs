using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    public interface IDocumentCategoryService : IGenericManagementService<DocumentCategory>
    {
        Task<bool> IsDocumentCategoryCodeExistsAsync(string code, int? excludeId = null);
    }
}
