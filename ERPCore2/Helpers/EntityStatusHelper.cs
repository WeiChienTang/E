using ERPCore2.Components.Shared.Forms;
using ERPCore2.Data.Enums;

namespace ERPCore2.Helpers
{
    /// <summary>
    /// 實體狀態輔助類別 - 提供狀態相關的共用功能
    /// </summary>
    public static class EntityStatusHelper
    {
        /// <summary>
        /// 取得實體狀態選項清單
        /// </summary>
        /// <returns>包含啟用和停用選項的清單</returns>
        public static List<SelectOption> GetEntityStatusOptions()
        {
            return new List<SelectOption>
            {
                new() { Text = "啟用", Value = EntityStatus.Active.ToString() },
                new() { Text = "停用", Value = EntityStatus.Inactive.ToString() }
            };
        }

        /// <summary>
        /// 取得實體狀態顯示名稱
        /// </summary>
        /// <param name="status">實體狀態</param>
        /// <returns>顯示名稱</returns>
        public static string GetStatusDisplayName(EntityStatus status)
        {
            return status switch
            {
                EntityStatus.Active => "啟用",
                EntityStatus.Inactive => "停用",
                EntityStatus.Deleted => "已刪除",
                _ => status.ToString()
            };
        }

        /// <summary>
        /// 取得實體狀態徽章CSS類別
        /// </summary>
        /// <param name="status">實體狀態</param>
        /// <returns>Bootstrap徽章CSS類別</returns>
        public static string GetStatusBadgeClass(EntityStatus status)
        {
            return status switch
            {
                EntityStatus.Active => "badge bg-success",
                EntityStatus.Inactive => "badge bg-secondary",
                EntityStatus.Deleted => "badge bg-danger",
                _ => "badge bg-secondary"
            };
        }

        /// <summary>
        /// 取得布林狀態選項清單（是/否）
        /// </summary>
        /// <returns>包含是和否選項的清單</returns>
        public static List<SelectOption> GetBooleanOptions()
        {
            return new List<SelectOption>
            {
                new() { Text = "是", Value = "true" },
                new() { Text = "否", Value = "false" }
            };
        }

        /// <summary>
        /// 取得啟用/停用狀態選項清單（對應布林值）
        /// </summary>
        /// <returns>包含啟用和停用選項的清單（布林值）</returns>
        public static List<SelectOption> GetActiveStatusOptions()
        {
            return new List<SelectOption>
            {
                new() { Text = "啟用", Value = "true" },
                new() { Text = "停用", Value = "false" }
            };
        }
    }
}
