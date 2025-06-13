using System.Security.Cryptography;
using System.Text;

namespace ERPCore2.Data.SeedDataManager.Helpers
{
    /// <summary>
    /// 種子資料共用工具類別
    /// </summary>
    public static class SeedDataHelper
    {
        /// <summary>
        /// 密碼雜湊
        /// </summary>
        /// <param name="password">原始密碼</param>
        /// <returns>雜湊後的密碼</returns>
        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var saltedPassword = password + "ERPCore2_Salt";
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
            return Convert.ToBase64String(hashedBytes);
        }

        /// <summary>
        /// 生成統一的建立時間和建立者資訊
        /// </summary>
        /// <param name="daysAgo">建立日期距今天數（預設為0）</param>
        /// <returns></returns>
        public static (DateTime CreatedAt, string CreatedBy) GetSystemCreateInfo(int daysAgo = 0)
        {
            return (DateTime.Now.AddDays(-daysAgo), "System");
        }
    }
}
