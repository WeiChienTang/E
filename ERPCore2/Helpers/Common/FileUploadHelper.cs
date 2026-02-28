using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;

namespace ERPCore2.Helpers
{
    /// <summary>
    /// 檔案上傳輔助類別
    /// </summary>
    public static class FileUploadHelper
    {
        // 允許的圖片格式
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".svg" };

        // 允許的文件格式（檔案存留用）
        private static readonly string[] AllowedDocumentExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png" };

        // Logo 檔案大小限制 (500KB)
        private const long MaxLogoFileSize = 500 * 1024;

        // 一般圖片檔案大小限制 (2MB)
        private const long MaxImageFileSize = 2 * 1024 * 1024;

        // 文件檔案大小限制 (20MB)
        private const long MaxDocumentFileSize = 20 * 1024 * 1024;

        /// <summary>
        /// 上傳公司 Logo
        /// </summary>
        public static async Task<(bool Success, string Message, string? FilePath)> UploadCompanyLogoAsync(
            IBrowserFile file,
            int companyId,
            IWebHostEnvironment environment,
            string? oldFilePath = null)
        {
            try
            {
                var validationResult = ValidateImageFile(file, MaxLogoFileSize);
                if (!validationResult.IsValid)
                    return (false, validationResult.Message, null);

                var extension = Path.GetExtension(file.Name).ToLowerInvariant();
                var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                var fileName = $"company_{companyId}_{timestamp}{extension}";

                var uploadPath = Path.Combine(environment.WebRootPath, "uploads", "company-logos");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, fileName);
                await using var fileStream = new FileStream(filePath, FileMode.Create);
                await file.OpenReadStream(MaxLogoFileSize).CopyToAsync(fileStream);

                if (!string.IsNullOrEmpty(oldFilePath))
                    DeleteFile(oldFilePath, environment);

                var relativePath = $"/uploads/company-logos/{fileName}";
                return (true, "Logo 上傳成功", relativePath);
            }
            catch (Exception ex)
            {
                return (false, $"上傳失敗：{ex.Message}", null);
            }
        }

        /// <summary>
        /// 上傳一般圖片檔案
        /// </summary>
        public static async Task<(bool Success, string Message, string? FilePath)> UploadImageAsync(
            IBrowserFile file,
            string subFolder,
            IWebHostEnvironment environment,
            string? oldFilePath = null)
        {
            try
            {
                var validationResult = ValidateImageFile(file, MaxImageFileSize);
                if (!validationResult.IsValid)
                    return (false, validationResult.Message, null);

                var extension = Path.GetExtension(file.Name).ToLowerInvariant();
                var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                var randomString = Guid.NewGuid().ToString("N").Substring(0, 8);
                var fileName = $"{timestamp}_{randomString}{extension}";

                var uploadPath = Path.Combine(environment.WebRootPath, "uploads", subFolder);
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, fileName);
                await using var fileStream = new FileStream(filePath, FileMode.Create);
                await file.OpenReadStream(MaxImageFileSize).CopyToAsync(fileStream);

                if (!string.IsNullOrEmpty(oldFilePath))
                    DeleteFile(oldFilePath, environment);

                var relativePath = $"/uploads/{subFolder}/{fileName}";
                return (true, "圖片上傳成功", relativePath);
            }
            catch (Exception ex)
            {
                return (false, $"上傳失敗：{ex.Message}", null);
            }
        }

        /// <summary>
        /// 上傳文件檔案（支援 PDF、Word、Excel、圖片，最大 20MB）
        /// </summary>
        public static async Task<(bool Success, string Message, string? FilePath, string? MimeType)> UploadDocumentAsync(
            IBrowserFile file,
            IWebHostEnvironment environment,
            string? oldFilePath = null)
        {
            try
            {
                var validationResult = ValidateDocumentFile(file);
                if (!validationResult.IsValid)
                    return (false, validationResult.Message, null, null);

                var extension = Path.GetExtension(file.Name).ToLowerInvariant();
                var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                var randomString = Guid.NewGuid().ToString("N").Substring(0, 8);
                var fileName = $"{timestamp}_{randomString}{extension}";

                var uploadPath = Path.Combine(environment.WebRootPath, "uploads", "documents");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, fileName);
                await using var fileStream = new FileStream(filePath, FileMode.Create);
                await file.OpenReadStream(MaxDocumentFileSize).CopyToAsync(fileStream);

                if (!string.IsNullOrEmpty(oldFilePath))
                    DeleteFile(oldFilePath, environment);

                var mimeType = GetDocumentMimeType(extension);
                var relativePath = $"/uploads/documents/{fileName}";
                return (true, "文件上傳成功", relativePath, mimeType);
            }
            catch (Exception ex)
            {
                return (false, $"上傳失敗：{ex.Message}", null, null);
            }
        }

        /// <summary>
        /// 驗證圖片檔案
        /// </summary>
        private static (bool IsValid, string Message) ValidateImageFile(IBrowserFile file, long maxFileSize)
        {
            if (file == null)
                return (false, "請選擇檔案");

            if (file.Size > maxFileSize)
            {
                var maxSizeMB = maxFileSize / (1024.0 * 1024.0);
                return (false, $"檔案大小不可超過 {maxSizeMB:F1} MB");
            }

            var extension = Path.GetExtension(file.Name).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(extension))
            {
                var allowedFormats = string.Join(", ", AllowedImageExtensions);
                return (false, $"不支援的檔案格式。允許的格式：{allowedFormats}");
            }

            return (true, "驗證通過");
        }

        /// <summary>
        /// 驗證文件檔案
        /// </summary>
        private static (bool IsValid, string Message) ValidateDocumentFile(IBrowserFile file)
        {
            if (file == null)
                return (false, "請選擇檔案");

            if (file.Size > MaxDocumentFileSize)
                return (false, "檔案大小不可超過 20 MB");

            var extension = Path.GetExtension(file.Name).ToLowerInvariant();
            if (!AllowedDocumentExtensions.Contains(extension))
            {
                var allowedFormats = string.Join(", ", AllowedDocumentExtensions);
                return (false, $"不支援的檔案格式。允許的格式：{allowedFormats}");
            }

            return (true, "驗證通過");
        }

        /// <summary>
        /// 根據副檔名取得文件 MIME 類型
        /// </summary>
        public static string GetDocumentMimeType(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };
        }

        /// <summary>
        /// 轉換檔案為 Base64 字串
        /// </summary>
        public static async Task<string?> ConvertToBase64Async(IBrowserFile file, long maxFileSize = MaxImageFileSize)
        {
            try
            {
                var validationResult = ValidateImageFile(file, maxFileSize);
                if (!validationResult.IsValid)
                    return null;

                using var memoryStream = new MemoryStream();
                await file.OpenReadStream(maxFileSize).CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();
                var base64String = Convert.ToBase64String(fileBytes);

                var mimeType = file.ContentType;
                if (string.IsNullOrEmpty(mimeType))
                {
                    var extension = Path.GetExtension(file.Name).ToLowerInvariant();
                    mimeType = extension switch
                    {
                        ".jpg" or ".jpeg" => "image/jpeg",
                        ".png" => "image/png",
                        ".gif" => "image/gif",
                        ".svg" => "image/svg+xml",
                        _ => "image/png"
                    };
                }

                return $"data:{mimeType};base64,{base64String}";
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 刪除檔案
        /// </summary>
        public static bool DeleteFile(string relativePath, IWebHostEnvironment environment)
        {
            try
            {
                if (string.IsNullOrEmpty(relativePath))
                    return false;

                var cleanPath = relativePath.TrimStart('/');
                var fullPath = Path.Combine(environment.WebRootPath, cleanPath);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 取得檔案大小（人類可讀格式）
        /// </summary>
        public static string GetFileSizeString(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }
    }
}
