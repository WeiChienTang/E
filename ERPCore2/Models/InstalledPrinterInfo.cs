namespace ERPCore2.Models;

/// <summary>
/// 已安裝印表機資訊模型
/// 用於顯示系統中已安裝的印表機列表供使用者選擇
/// </summary>
public class InstalledPrinterInfo
{
    /// <summary>
    /// 印表機名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 連接埠名稱 (例如: USB001, LPT1, IP_192.168.1.100)
    /// </summary>
    public string PortName { get; set; } = string.Empty;
    
    /// <summary>
    /// 是否為系統預設印表機
    /// </summary>
    public bool IsSystemDefault { get; set; }
    
    /// <summary>
    /// 驅動程式名稱
    /// </summary>
    public string DriverName { get; set; } = string.Empty;
    
    /// <summary>
    /// 印表機狀態描述
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// 是否為網路印表機
    /// </summary>
    public bool IsNetworkPrinter { get; set; }
    
    /// <summary>
    /// 共用名稱（如果是共用印表機）
    /// </summary>
    public string? ShareName { get; set; }
}
