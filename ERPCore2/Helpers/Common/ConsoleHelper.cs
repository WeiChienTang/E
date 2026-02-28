namespace ERPCore2.Helpers
{
    /// <summary>
    /// Console 輸出輔助工具 - 提供彩色輸出功能
    /// </summary>
    public static class ConsoleHelper
    {
        /// <summary>
        /// 輸出成功訊息（綠色）
        /// </summary>
        public static void WriteSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// 輸出錯誤訊息（紅色）
        /// </summary>
        public static void WriteError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"✗ {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// 輸出警告訊息（黃色）
        /// </summary>
        public static void WriteWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"⚠ {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// 輸出資訊訊息（藍色）
        /// </summary>
        public static void WriteInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"ℹ {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// 輸出除錯訊息（灰色）
        /// </summary>
        public static void WriteDebug(string message)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"🔍 {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// 輸出標題訊息（洋紅色，加粗效果）
        /// </summary>
        public static void WriteTitle(string message)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"\n═══════════════════════════════════");
            Console.WriteLine($"  {message}");
            Console.WriteLine($"═══════════════════════════════════\n");
            Console.ResetColor();
        }

        /// <summary>
        /// 輸出自訂顏色訊息
        /// </summary>
        public static void WriteColor(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        /// <summary>
        /// 輸出步驟訊息（帶編號，綠色）
        /// </summary>
        public static void WriteStep(int step, string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"[步驟 {step}] ");
            Console.ResetColor();
            Console.WriteLine(message);
        }

        /// <summary>
        /// 輸出分隔線
        /// </summary>
        public static void WriteSeparator(char character = '-', int length = 50)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(new string(character, length));
            Console.ResetColor();
        }
    }
}
