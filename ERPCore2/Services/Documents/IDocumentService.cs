using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    public interface IDocumentService : IGenericManagementService<Document>
    {
        Task<List<Document>> GetByCategoryAsync(int categoryId);
        Task<bool> IsDocumentCodeExistsAsync(string code, int? excludeId = null);
    }
}
