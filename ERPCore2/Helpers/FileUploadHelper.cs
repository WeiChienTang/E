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
        
        // Logo 檔案大小限制 (500KB)
        private const long MaxLogoFileSize = 500 * 1024;
        
        // 一般圖片檔案大小限制 (2MB)
        private const long MaxImageFileSize = 2 * 1024 * 1024;

        /// <summary>
        /// 上傳公司 Logo
        /// </summary>
        /// <param name="file">上傳的檔案</param>
        /// <param name="companyId">公司ID</param>
        /// <param name="environment">Web Host Environment</param>
        /// <param name="oldFilePath">舊檔案路徑（用於刪除）</param>
        /// <returns>成功狀態、訊息和新檔案路徑</returns>
        public static async Task<(bool Success, string Message, string? FilePath)> UploadCompanyLogoAsync(
            IBrowserFile file, 
            int companyId,
            IWebHostEnvironment environment,
            string? oldFilePath = null)
        {
            try
            {
                // 1. 驗證檔案
                var validationResult = ValidateImageFile(file, MaxLogoFileSize);
                if (!validationResult.IsValid)
                {
                    return (false, validationResult.Message, null);
                }

                // 2. 生成檔案名稱
                var extension = Path.GetExtension(file.Name).ToLowerInvariant();
                var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                var fileName = $"company_{companyId}_{timestamp}{extension}";

                // 3. 確保目錄存在
                var uploadPath = Path.Combine(environment.WebRootPath, "uploads", "company-logos");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // 4. 儲存檔案
                var filePath = Path.Combine(uploadPath, fileName);
                await using var fileStream = new FileStream(filePath, FileMode.Create);
                await file.OpenReadStream(MaxLogoFileSize).CopyToAsync(fileStream);

                // 5. 刪除舊檔案（如果存在）
                if (!string.IsNullOrEmpty(oldFilePath))
                {
                    DeleteFile(oldFilePath, environment);
                }

                // 6. 返回相對路徑（用於儲存到資料庫）
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
        /// <param name="file">上傳的檔案</param>
        /// <param name="subFolder">子資料夾名稱</param>
        /// <param name="environment">Web Host Environment</param>
        /// <param name="oldFilePath">舊檔案路徑（用於刪除）</param>
        /// <returns>成功狀態、訊息和新檔案路徑</returns>
        public static async Task<(bool Success, string Message, string? FilePath)> UploadImageAsync(
            IBrowserFile file,
            string subFolder,
            IWebHostEnvironment environment,
            string? oldFilePath = null)
        {
            try
            {
                // 1. 驗證檔案
                var validationResult = ValidateImageFile(file, MaxImageFileSize);
                if (!validationResult.IsValid)
                {
                    return (false, validationResult.Message, null);
                }

                // 2. 生成檔案名稱
                var extension = Path.GetExtension(file.Name).ToLowerInvariant();
                var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                var randomString = Guid.NewGuid().ToString("N").Substring(0, 8);
                var fileName = $"{timestamp}_{randomString}{extension}";

                // 3. 確保目錄存在
                var uploadPath = Path.Combine(environment.WebRootPath, "uploads", subFolder);
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // 4. 儲存檔案
                var filePath = Path.Combine(uploadPath, fileName);
                await using var fileStream = new FileStream(filePath, FileMode.Create);
                await file.OpenReadStream(MaxImageFileSize).CopyToAsync(fileStream);

                // 5. 刪除舊檔案（如果存在）
                if (!string.IsNullOrEmpty(oldFilePath))
                {
                    DeleteFile(oldFilePath, environment);
                }

                // 6. 返回相對路徑
                var relativePath = $"/uploads/{subFolder}/{fileName}";
                return (true, "圖片上傳成功", relativePath);
            }
            catch (Exception ex)
            {
                return (false, $"上傳失敗：{ex.Message}", null);
            }
        }

        /// <summary>
        /// 驗證圖片檔案
        /// </summary>
        /// <param name="file">要驗證的檔案</param>
        /// <param name="maxFileSize">最大檔案大小</param>
        /// <returns>驗證結果</returns>
        private static (bool IsValid, string Message) ValidateImageFile(IBrowserFile file, long maxFileSize)
        {
            // 檢查檔案是否為空
            if (file == null)
            {
                return (false, "請選擇檔案");
            }

            // 檢查檔案大小
            if (file.Size > maxFileSize)
            {
                var maxSizeMB = maxFileSize / (1024.0 * 1024.0);
                return (false, $"檔案大小不可超過 {maxSizeMB:F1} MB");
            }

            // 檢查檔案格式
            var extension = Path.GetExtension(file.Name).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(extension))
            {
                var allowedFormats = string.Join(", ", AllowedImageExtensions);
                return (false, $"不支援的檔案格式。允許的格式：{allowedFormats}");
            }

            return (true, "驗證通過");
        }

        /// <summary>
        /// 轉換檔案為 Base64 字串
        /// </summary>
        /// <param name="file">要轉換的檔案</param>
        /// <param name="maxFileSize">最大檔案大小（預設2MB）</param>
        /// <returns>Base64 字串（包含 data URI scheme）</returns>
        public static async Task<string?> ConvertToBase64Async(IBrowserFile file, long maxFileSize = MaxImageFileSize)
        {
            try
            {
                // 驗證檔案
                var validationResult = ValidateImageFile(file, maxFileSize);
                if (!validationResult.IsValid)
                {
                    return null;
                }

                // 讀取檔案內容
                using var memoryStream = new MemoryStream();
                await file.OpenReadStream(maxFileSize).CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();

                // 轉換為 Base64
                var base64String = Convert.ToBase64String(fileBytes);

                // 取得 MIME type
                var mimeType = file.ContentType;
                if (string.IsNullOrEmpty(mimeType))
                {
                    // 根據副檔名推斷 MIME type
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

                // 返回 data URI
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
        /// <param name="relativePath">相對路徑（例如：/uploads/company-logos/xxx.png）</param>
        /// <param name="environment">Web Host Environment</param>
        /// <returns>是否刪除成功</returns>
        public static bool DeleteFile(string relativePath, IWebHostEnvironment environment)
        {
            try
            {
                if (string.IsNullOrEmpty(relativePath))
                {
                    return false;
                }

                // 移除開頭的斜線
                var cleanPath = relativePath.TrimStart('/');

                // 組合完整路徑
                var fullPath = Path.Combine(environment.WebRootPath, cleanPath);

                // 檢查檔案是否存在
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
        /// <param name="bytes">位元組大小</param>
        /// <returns>格式化的檔案大小字串</returns>
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
