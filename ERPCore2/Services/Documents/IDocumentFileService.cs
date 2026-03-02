using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    public interface IDocumentFileService : IGenericManagementService<DocumentFile>
    {
        Task<List<DocumentFile>> GetByDocumentAsync(int documentId);
    }
}
