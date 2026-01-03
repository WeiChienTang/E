namespace ERPCore2.Helpers
{
    /// <summary>
    /// 數字格式化輔助類別
    /// 提供智能數字格式化功能，根據數值是否為整數自動決定是否顯示小數點
    /// </summary>
    public static class NumberFormatHelper
    {
        /// <summary>
        /// 智能格式化數字：整數不顯示小數點，有小數才顯示
        /// </summary>
        /// <param name="value">要格式化的數值</param>
        /// <param name="decimalPlaces">小數位數（預設2位）</param>
        /// <param name="useThousandsSeparator">是否使用千分位分隔符號（預設true）</param>
        /// <returns>格式化後的字串</returns>
        public static string FormatSmart(decimal value, int decimalPlaces = 2, bool useThousandsSeparator = true)
        {
            // 檢查是否為整數
            if (value % 1 == 0)
            {
                // 整數：不顯示小數點
                return useThousandsSeparator 
                    ? value.ToString("N0")  // 使用千分位，無小數點
                    : value.ToString("F0"); // 不使用千分位，無小數點
            }
            else
            {
                // 有小數：顯示指定的小數位數
                var format = useThousandsSeparator 
                    ? $"N{decimalPlaces}"  // 使用千分位
                    : $"F{decimalPlaces}"; // 不使用千分位
                return value.ToString(format);
            }
        }

        /// <summary>
        /// 智能格式化數字（可為null）：整數不顯示小數點，有小數才顯示
        /// </summary>
        /// <param name="value">要格式化的數值（可為null）</param>
        /// <param name="decimalPlaces">小數位數（預設2位）</param>
        /// <param name="useThousandsSeparator">是否使用千分位分隔符號（預設true）</param>
        /// <param name="nullDisplayText">null時顯示的文字（預設為空字串）</param>
        /// <returns>格式化後的字串</returns>
        public static string FormatSmart(decimal? value, int decimalPlaces = 2, bool useThousandsSeparator = true, string nullDisplayText = "")
        {
            if (!value.HasValue)
            {
                return nullDisplayText;
            }

            return FormatSmart(value.Value, decimalPlaces, useThousandsSeparator);
        }

        /// <summary>
        /// 智能格式化數字（零值顯示為空）：整數不顯示小數點，有小數才顯示
        /// </summary>
        /// <param name="value">要格式化的數值</param>
        /// <param name="decimalPlaces">小數位數（預設2位）</param>
        /// <param name="useThousandsSeparator">是否使用千分位分隔符號（預設true）</param>
        /// <param name="zeroDisplayText">零值時顯示的文字（預設為空字串）</param>
        /// <returns>格式化後的字串</returns>
        public static string FormatSmartZeroAsEmpty(decimal value, int decimalPlaces = 2, bool useThousandsSeparator = true, string zeroDisplayText = "")
        {
            if (value == 0)
            {
                return zeroDisplayText;
            }

            return FormatSmart(value, decimalPlaces, useThousandsSeparator);
        }

        /// <summary>
        /// 智能格式化數字（可為null，零值顯示為空）：整數不顯示小數點，有小數才顯示
        /// </summary>
        /// <param name="value">要格式化的數值（可為null）</param>
        /// <param name="decimalPlaces">小數位數（預設2位）</param>
        /// <param name="useThousandsSeparator">是否使用千分位分隔符號（預設true）</param>
        /// <param name="emptyDisplayText">null或零值時顯示的文字（預設為空字串）</param>
        /// <returns>格式化後的字串</returns>
        public static string FormatSmartZeroAsEmpty(decimal? value, int decimalPlaces = 2, bool useThousandsSeparator = true, string emptyDisplayText = "")
        {
            if (!value.HasValue || value.Value == 0)
            {
                return emptyDisplayText;
            }

            return FormatSmart(value.Value, decimalPlaces, useThousandsSeparator);
        }

        /// <summary>
        /// 格式化為固定小數位數（始終顯示小數點）
        /// </summary>
        /// <param name="value">要格式化的數值</param>
        /// <param name="decimalPlaces">小數位數（預設2位）</param>
        /// <param name="useThousandsSeparator">是否使用千分位分隔符號（預設true）</param>
        /// <returns>格式化後的字串</returns>
        public static string FormatFixed(decimal value, int decimalPlaces = 2, bool useThousandsSeparator = true)
        {
            var format = useThousandsSeparator 
                ? $"N{decimalPlaces}"  // 使用千分位
                : $"F{decimalPlaces}"; // 不使用千分位
            return value.ToString(format);
        }

        /// <summary>
        /// 格式化為固定小數位數（可為null，始終顯示小數點）
        /// </summary>
        /// <param name="value">要格式化的數值（可為null）</param>
        /// <param name="decimalPlaces">小數位數（預設2位）</param>
        /// <param name="useThousandsSeparator">是否使用千分位分隔符號（預設true）</param>
        /// <param name="nullDisplayText">null時顯示的文字（預設為空字串）</param>
        /// <returns>格式化後的字串</returns>
        public static string FormatFixed(decimal? value, int decimalPlaces = 2, bool useThousandsSeparator = true, string nullDisplayText = "")
        {
            if (!value.HasValue)
            {
                return nullDisplayText;
            }

            return FormatFixed(value.Value, decimalPlaces, useThousandsSeparator);
        }

        /// <summary>
        /// 智能格式化數字用於輸入框顯示（不使用千分位，適合input type="number"）
        /// 整數不顯示小數點，有小數才顯示，零值顯示為空
        /// </summary>
        /// <param name="value">要格式化的數值</param>
        /// <returns>格式化後的字串</returns>
        public static string FormatForInput(decimal value)
        {
            // 零值顯示為空字串
            if (value == 0)
            {
                return string.Empty;
            }

            // 整數：不顯示小數點（使用 F0）
            if (value % 1 == 0)
            {
                return value.ToString("F0");
            }
            
            // 有小數：使用 G29 格式（自動移除尾隨零，最多29位有效數字）
            return value.ToString("G29");
        }

        /// <summary>
        /// 智能格式化數字用於輸入框顯示（支援負數，不使用千分位，適合input type="number"）
        /// 整數不顯示小數點，有小數才顯示，零值顯示為空
        /// </summary>
        /// <param name="value">要格式化的數值</param>
        /// <param name="applyNegative">是否套用負號</param>
        /// <returns>格式化後的字串</returns>
        public static string FormatForInput(decimal value, bool applyNegative)
        {
            // 零值顯示為空字串
            if (value == 0)
            {
                return string.Empty;
            }

            // 根據參數決定是否取負數
            var displayValue = applyNegative ? -value : value;

            // 整數：不顯示小數點（使用 F0）
            if (displayValue % 1 == 0)
            {
                return displayValue.ToString("F0");
            }
            
            // 有小數：使用 G29 格式（自動移除尾隨零，最多29位有效數字）
            return displayValue.ToString("G29");
        }
    }
}
