using System.ComponentModel;

namespace ERPCore2.Models.Enums
{
    /// <summary>
    /// 印表機連接方式枚舉
    /// </summary>
    public enum PrinterConnectionType
    {
        /// <summary>
        /// 網路連接（透過 IP 位址）
        /// </summary>
        [Description("網路連接")]
        Network = 1,

        /// <summary>
        /// USB 連接
        /// </summary>
        [Description("USB 連接")]
        USB = 2,
    }
}
