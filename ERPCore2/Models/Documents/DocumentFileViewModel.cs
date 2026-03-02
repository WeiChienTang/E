using Microsoft.AspNetCore.Components.Forms;

namespace ERPCore2.Models.Documents
{
    /// <summary>
    /// 文件附件檔案的 ViewModel（橋接 IBrowserFile 與已儲存的 DocumentFile）
    /// </summary>
    public class DocumentFileViewModel
    {
        public int? Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string? FilePath { get; set; }
        public long FileSize { get; set; }
        public string? MimeType { get; set; }
        public IBrowserFile? PendingFile { get; set; }
        public bool IsDeleted { get; set; }
        public int SortOrder { get; set; }
        public bool IsPending => PendingFile != null;
    }
}
