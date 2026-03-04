namespace ERPCore2.Helpers
{
    /// <summary>
    /// 審核配置輔助類 - 統一管理審核相關邏輯
    /// 審核永遠啟用；isManualApproval 控制審核方式：
    ///   false = 系統自動審核（儲存時自動核准）
    ///   true  = 人工審核（需人工按核准鍵）
    /// </summary>
    public static class ApprovalConfigHelper
    {
        /// <summary>
        /// 根據審核模式決定是否需要鎖定欄位
        /// </summary>
        /// <param name="isManualApproval">是否為人工審核（false=系統自動審核，true=人工審核）</param>
        /// <param name="isApproved">實體是否已審核通過</param>
        /// <param name="hasUndeletableDetails">是否有不可刪除的明細（已有下一步動作）</param>
        /// <returns>是否應該鎖定欄位</returns>
        public static bool ShouldLockFieldByApproval(
            bool isManualApproval,
            bool isApproved,
            bool hasUndeletableDetails)
        {
            if (!isManualApproval)
            {
                // 系統自動審核：只根據「是否有下一步動作」鎖定；下次儲存會再次自動核准
                return hasUndeletableDetails;
            }
            else
            {
                // 人工審核：根據「審核狀態 或 是否有下一步動作」鎖定
                return isApproved || hasUndeletableDetails;
            }
        }

        /// <summary>
        /// 判斷是否可以執行某個動作（如轉單、列印等）
        /// </summary>
        /// <param name="isManualApproval">是否為人工審核（false=系統自動審核，true=人工審核）</param>
        /// <param name="isApproved">實體是否已審核通過</param>
        /// <returns>是否可以執行該動作</returns>
        public static bool CanPerformActionRequiringApproval(
            bool isManualApproval,
            bool isApproved)
        {
            if (!isManualApproval)
            {
                // 系統自動審核：儲存後即已核准，隨時可以執行
                return true;
            }
            else
            {
                // 人工審核：需要已核准
                return isApproved;
            }
        }

        /// <summary>
        /// 判斷在審核狀態下是否可以儲存
        /// </summary>
        /// <param name="isManualApproval">是否為人工審核（false=系統自動審核，true=人工審核）</param>
        /// <param name="isApproved">實體是否已審核通過</param>
        /// <param name="isPreApprovalSave">是否為審核前自動儲存</param>
        /// <returns>是否可以儲存</returns>
        public static bool CanSaveWhenApproved(
            bool isManualApproval,
            bool isApproved,
            bool isPreApprovalSave = false)
        {
            if (!isManualApproval)
            {
                // 系統自動審核：隨時可以儲存（儲存後自動核准）
                return true;
            }

            if (isPreApprovalSave)
            {
                // 審核前自動儲存：允許
                return true;
            }

            // 人工審核且不是審核前儲存：已核准後不可儲存
            return !isApproved;
        }

        /// <summary>
        /// 判斷是否應該執行庫存更新
        /// 自動審核模式：儲存時即可更新庫存（系統會自動核准）
        /// 人工審核模式：需核准後才可更新庫存
        /// </summary>
        /// <param name="isManualApproval">是否為人工審核（false=系統自動審核，true=人工審核）</param>
        /// <param name="isApproved">實體是否已審核通過</param>
        /// <returns>是否應該執行庫存更新</returns>
        public static bool ShouldUpdateInventory(
            bool isManualApproval,
            bool isApproved)
        {
            if (!isManualApproval)
            {
                // 系統自動審核：儲存後系統自動核准，庫存可立即更新
                return true;
            }
            else
            {
                // 人工審核：需核准後才允許更新庫存
                return isApproved;
            }
        }

        /// <summary>
        /// 取得審核狀態的警告訊息
        /// </summary>
        /// <param name="isManualApproval">是否為人工審核（false=系統自動審核，true=人工審核）</param>
        /// <param name="isApproved">實體是否已審核通過</param>
        /// <param name="entityName">實體名稱（如：採購單）</param>
        /// <returns>警告訊息，如果不需要顯示則返回 null</returns>
        public static string? GetApprovalWarningMessage(
            bool isManualApproval,
            bool isApproved,
            string entityName = "單據")
        {
            if (!isManualApproval)
            {
                // 系統自動審核：不顯示警告
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
