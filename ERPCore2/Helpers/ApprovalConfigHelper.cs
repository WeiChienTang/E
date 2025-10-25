namespace ERPCore2.Helpers
{
    /// <summary>
    /// 審核配置輔助類 - 統一管理審核相關邏輯
    /// </summary>
    public static class ApprovalConfigHelper
    {
        /// <summary>
        /// 根據審核開關決定是否需要鎖定欄位
        /// </summary>
        /// <param name="isApprovalEnabled">是否啟用審核功能</param>
        /// <param name="isApproved">實體是否已審核通過</param>
        /// <param name="hasUndeletableDetails">是否有不可刪除的明細（已有下一步動作）</param>
        /// <returns>是否應該鎖定欄位</returns>
        public static bool ShouldLockFieldByApproval(
            bool isApprovalEnabled,
            bool isApproved,
            bool hasUndeletableDetails)
        {
            if (!isApprovalEnabled)
            {
                // 未啟用審核：只根據「是否有下一步動作」鎖定
                return hasUndeletableDetails;
            }
            else
            {
                // 已啟用審核：根據「審核狀態 或 是否有下一步動作」鎖定
                return isApproved || hasUndeletableDetails;
            }
        }

        /// <summary>
        /// 判斷是否可以執行某個動作（如轉單、列印等）
        /// </summary>
        /// <param name="isApprovalEnabled">是否啟用審核功能</param>
        /// <param name="isApproved">實體是否已審核通過</param>
        /// <returns>是否可以執行該動作</returns>
        public static bool CanPerformActionRequiringApproval(
            bool isApprovalEnabled,
            bool isApproved)
        {
            if (!isApprovalEnabled)
            {
                // 未啟用審核：隨時可以執行
                return true;
            }
            else
            {
                // 已啟用審核：需要已核准
                return isApproved;
            }
        }

        /// <summary>
        /// 判斷在審核狀態下是否可以儲存
        /// </summary>
        /// <param name="isApprovalEnabled">是否啟用審核功能</param>
        /// <param name="isApproved">實體是否已審核通過</param>
        /// <param name="isPreApprovalSave">是否為審核前自動儲存</param>
        /// <returns>是否可以儲存</returns>
        public static bool CanSaveWhenApproved(
            bool isApprovalEnabled,
            bool isApproved,
            bool isPreApprovalSave = false)
        {
            if (!isApprovalEnabled)
            {
                // 未啟用審核：隨時可以儲存
                return true;
            }
            
            if (isPreApprovalSave)
            {
                // 審核前自動儲存：允許
                return true;
            }
            
            // 已啟用審核且不是審核前儲存：已核准後不可儲存
            return !isApproved;
        }

        /// <summary>
        /// 取得審核狀態的警告訊息
        /// </summary>
        /// <param name="isApprovalEnabled">是否啟用審核功能</param>
        /// <param name="isApproved">實體是否已審核通過</param>
        /// <param name="entityName">實體名稱（如：採購單）</param>
        /// <returns>警告訊息，如果不需要顯示則返回 null</returns>
        public static string? GetApprovalWarningMessage(
            bool isApprovalEnabled,
            bool isApproved,
            string entityName = "單據")
        {
            if (!isApprovalEnabled)
            {
                // 未啟用審核：不顯示警告
                return null;
            }

            if (isApproved)
            {
                return $"{entityName}已核准，無法修改";
            }

            return null;
        }
    }
}
