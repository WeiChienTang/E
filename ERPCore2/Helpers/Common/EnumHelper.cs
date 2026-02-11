using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ERPCore2.Helpers
{
    /// <summary>
    /// Enum 輔助類別 - 提供 Enum 相關的共用功能
    /// </summary>
    public static class EnumHelper
    {
        /// <summary>
        /// 取得 Enum 值的 Display Name（從 [Display(Name = "...")] 屬性讀取）
        /// </summary>
        /// <param name="enumValue">Enum 值</param>
        /// <returns>Display Name，如果沒有設定則回傳 ToString()</returns>
        public static string GetDisplayName(Enum enumValue)
        {
            var memberInfo = enumValue.GetType().GetMember(enumValue.ToString()).FirstOrDefault();
            if (memberInfo == null) return enumValue.ToString();

            var displayAttribute = memberInfo.GetCustomAttribute<DisplayAttribute>();
            return displayAttribute?.Name ?? enumValue.ToString();
        }
    }
}
