namespace ERPCore2.Models
{
    /// <summary>
    /// 業績目標矩陣輸入列（用於批次設定 Modal）
    /// 每列代表一位業務員（或公司整體），欄位為各月目標金額和全年整體目標
    /// </summary>
    public class SalesTargetMatrixRow
    {
        public int? SalespersonId { get; set; }
        public string SalespersonName { get; set; } = "";

        // 各月目標（0 = 無目標）
        public decimal M01 { get; set; }
        public decimal M02 { get; set; }
        public decimal M03 { get; set; }
        public decimal M04 { get; set; }
        public decimal M05 { get; set; }
        public decimal M06 { get; set; }
        public decimal M07 { get; set; }
        public decimal M08 { get; set; }
        public decimal M09 { get; set; }
        public decimal M10 { get; set; }
        public decimal M11 { get; set; }
        public decimal M12 { get; set; }

        /// <summary>全年整體目標（Month = null 的那筆記錄）</summary>
        public decimal Annual { get; set; }

        /// <summary>月份合計（M01–M12 之和，計算屬性）</summary>
        public decimal MonthlySum =>
            M01 + M02 + M03 + M04 + M05 + M06 + M07 + M08 + M09 + M10 + M11 + M12;

        /// <summary>是否為公司整體目標列</summary>
        public bool IsCompanyTotal => SalespersonId == null;

        public decimal GetMonth(int month) => month switch
        {
            1 => M01, 2 => M02, 3 => M03, 4 => M04,
            5 => M05, 6 => M06, 7 => M07, 8 => M08,
            9 => M09, 10 => M10, 11 => M11, 12 => M12,
            _ => 0
        };

        public void SetMonth(int month, decimal value)
        {
            switch (month)
            {
                case 1: M01 = value; break;
                case 2: M02 = value; break;
                case 3: M03 = value; break;
                case 4: M04 = value; break;
                case 5: M05 = value; break;
                case 6: M06 = value; break;
                case 7: M07 = value; break;
                case 8: M08 = value; break;
                case 9: M09 = value; break;
                case 10: M10 = value; break;
                case 11: M11 = value; break;
                case 12: M12 = value; break;
            }
        }
    }
}
