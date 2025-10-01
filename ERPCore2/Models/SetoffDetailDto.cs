using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Models
{
    /// <summary>
    /// 沖款明細 DTO - 統一處理銷貨訂單和銷貨退回明細的沖款資料
    /// </summary>
    public class SetoffDetailDto
    {
        /// <summary>
        /// 明細 ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 明細類型：SalesOrder 或 SalesReturn
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// 明細類型顯示名稱
        /// </summary>
        public string TypeDisplayName => Type switch
        {
            "SalesOrder" => "銷貨訂單",
            "SalesReturn" => "銷貨退回",
            _ => "未知"
        };

        /// <summary>
        /// 單據號碼
        /// </summary>
        public string DocumentNumber { get; set; } = string.Empty;

        /// <summary>
        /// 單據日期
        /// </summary>
        public DateTime DocumentDate { get; set; }

        /// <summary>
        /// 商品 ID
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// 商品名稱
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// 商品編號
        /// </summary>
        public string ProductCode { get; set; } = string.Empty;

        /// <summary>
        /// 數量
        /// </summary>
        [Display(Name = "數量")]
        public decimal Quantity { get; set; }

        /// <summary>
        /// 單價
        /// </summary>
        [Display(Name = "單價")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// 總金額（應收/應付金額）
        /// </summary>
        [Display(Name = "總金額")]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// 已沖款金額
        /// </summary>
        [Display(Name = "已沖款金額")]
        public decimal SettledAmount { get; set; }

        /// <summary>
        /// 已折讓金額 - 從 FinancialTransaction 計算得出
        /// </summary>
        [Display(Name = "已折讓金額")]
        public decimal DiscountedAmount { get; set; }

        /// <summary>
        /// 待沖款金額（剩餘金額）
        /// </summary>
        [Display(Name = "待沖款金額")]
        public decimal PendingAmount => TotalAmount - SettledAmount - DiscountedAmount;

        /// <summary>
        /// 本次沖款金額
        /// </summary>
        [Display(Name = "本次沖款金額")]
        public decimal ThisTimeAmount { get; set; } = 0;

        /// <summary>
        /// 本次折讓金額
        /// </summary>
        [Display(Name = "本次折讓金額")]
        public decimal ThisTimeDiscountAmount { get; set; } = 0;

        /// <summary>
        /// 載入時的原始本次沖款金額（用於動態計算）
        /// </summary>
        public decimal OriginalThisTimeAmount { get; set; } = 0;

        /// <summary>
        /// 載入時的原始本次折讓金額（用於動態計算）
        /// </summary>
        public decimal OriginalThisTimeDiscountAmount { get; set; } = 0;

        /// <summary>
        /// 是否為編輯模式
        /// </summary>
        public bool IsEditMode { get; set; } = false;

        /// <summary>
        /// 用於驗證的待沖款金額（在編輯模式下排除當前沖款單的影響）
        /// </summary>
        public decimal PendingAmountForValidation 
        { 
            get 
            {
                if (IsEditMode)
                {
                    // 編輯模式：排除當前沖款單的金額進行驗證
                    return TotalAmount - (SettledAmount - ThisTimeAmount) - (DiscountedAmount - ThisTimeDiscountAmount);
                }
                else
                {
                    // 新增模式：使用原始計算
                    return PendingAmount;
                }
            } 
        }

        /// <summary>
        /// 動態計算的已沖款金額（用於UI顯示）
        /// 編輯模式: 移除原始沖款後加上當前沖款 = SettledAmount - OriginalThisTimeAmount + ThisTimeAmount
        /// 新增模式: 現有沖款加上本次沖款 = SettledAmount + ThisTimeAmount
        /// </summary>
        public decimal DynamicSettledAmount => IsEditMode ? 
            SettledAmount - OriginalThisTimeAmount + ThisTimeAmount : 
            SettledAmount + ThisTimeAmount;

        /// <summary>
        /// 動態計算的已折讓金額（用於UI顯示）
        /// 編輯模式: DiscountedAmount(已排除當前沖款單) + ThisTimeDiscountAmount(當前編輯的折讓)
        /// 新增模式: 現有折讓加上本次折讓 = DiscountedAmount + ThisTimeDiscountAmount
        /// </summary>
        public decimal DynamicDiscountedAmount => DiscountedAmount + ThisTimeDiscountAmount;

        /// <summary>
        /// 動態計算的待沖款金額（用於UI顯示）
        /// </summary>
        public decimal DynamicPendingAmount => TotalAmount - DynamicSettledAmount - DynamicDiscountedAmount;

        /// <summary>
        /// 是否選中進行沖款
        /// </summary>
        [Display(Name = "選擇")]
        public bool IsSelected { get; set; } = false;

        /// <summary>
        /// 是否已結清
        /// </summary>
        [Display(Name = "是否結清")]
        public bool IsSettled { get; set; }

        /// <summary>
        /// 客戶 ID
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// 客戶名稱
        /// </summary>
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// 幣別
        /// </summary>
        public string Currency { get; set; } = "TWD";

        /// <summary>
        /// 備註
        /// </summary>
        public string? Remark { get; set; }

        /// <summary>
        /// 原始實體的參考 ID（用於更新時定位）
        /// </summary>
        public int OriginalEntityId { get; set; }

        /// <summary>
        /// 驗證本次沖款金額
        /// </summary>
        /// <returns>驗證結果</returns>
        public (bool IsValid, string? ErrorMessage) ValidateThisTimeAmount()
        {
            if (ThisTimeAmount < 0)
            {
                return (false, "沖款金額不能為負數");
            }

            var pendingForValidation = PendingAmountForValidation;
            if (ThisTimeAmount > pendingForValidation)
            {
                return (false, $"沖款金額不能超過待沖款金額 {pendingForValidation:N2}");
            }

            return (true, null);
        }

        /// <summary>
        /// 驗證本次折讓金額
        /// </summary>
        /// <returns>驗證結果</returns>
        public (bool IsValid, string? ErrorMessage) ValidateThisTimeDiscountAmount()
        {
            if (ThisTimeDiscountAmount < 0)
            {
                return (false, "折讓金額不能為負數");
            }

            var pendingForValidation = PendingAmountForValidation;
            if (ThisTimeDiscountAmount > pendingForValidation)
            {
                return (false, $"折讓金額不能超過待沖款金額 {pendingForValidation:N2}");
            }

            return (true, null);
        }

        /// <summary>
        /// 驗證本次沖款和折讓總金額
        /// </summary>
        /// <returns>驗證結果</returns>
        public (bool IsValid, string? ErrorMessage) ValidateTotalThisTimeAmount()
        {
            var totalThisTime = ThisTimeAmount + ThisTimeDiscountAmount;
            
            if (totalThisTime < 0)
            {
                return (false, "沖款和折讓總金額不能為負數");
            }

            var pendingForValidation = PendingAmountForValidation;
            if (totalThisTime > pendingForValidation)
            {
                return (false, $"沖款和折讓總金額 ({totalThisTime:N2}) 不能超過待沖款金額 {pendingForValidation:N2}");
            }

            return (true, null);
        }

        /// <summary>
        /// 取得金額顯示格式
        /// </summary>
        /// <param name="amount">金額</param>
        /// <returns>格式化後的金額字串</returns>
        public static string FormatAmount(decimal amount)
        {
            return amount.ToString("N2");
        }
    }
}