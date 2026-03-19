using System.ComponentModel;

namespace ERPCore2.Models.Enums
{
    /// <summary>
    /// 逐日出勤狀態
    /// </summary>
    public enum DailyAttendanceStatus
    {
        /// <summary>出勤</summary>
        [Description("出勤")]
        Present = 1,

        /// <summary>曠職（無薪，扣全日工資）</summary>
        [Description("曠職")]
        Absent = 2,

        /// <summary>病假（半薪）</summary>
        [Description("病假")]
        SickLeave = 3,

        /// <summary>事假（無薪）</summary>
        [Description("事假")]
        PersonalLeave = 4,

        /// <summary>特休（有薪）</summary>
        [Description("特休")]
        AnnualLeave = 5,

        /// <summary>休息日（週六日）</summary>
        [Description("休息日")]
        RestDay = 6,

        /// <summary>國定假日</summary>
        [Description("國定假日")]
        NationalHoliday = 7,

        /// <summary>補班（配合彈性放假）</summary>
        [Description("補班")]
        MakeUpWork = 8,
    }
}
