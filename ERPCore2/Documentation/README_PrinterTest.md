# 印表機測試列印功能實作指南

## 📋 功能概述

為印表機配置編輯組件實作了完整的「測試列印」功能，支援真實的印表機連接和列印操作，讓使用者能在配置印表機後立即驗證設定是否正確。

## 🎯 實作功能

### 1. 印表機測試服務 (IPrinterTestService / PrinterTestService)

完整實作了印表機測試功能的服務：

**主要功能：**
- 自動偵測系統中的印表機（使用 WMI）
- 支援多種印表機格式（PCL、ESC/POS、純文字）
- 真實的列印操作（不再只是模擬）
- 智能格式選擇和驅動程式整合
- 詳細的錯誤診斷和日誌記錄

**檔案位置：**
- `Services/Systems/IPrinterTestService.cs`
- `Services/Systems/PrinterTestService.cs`

### 2. 印表機自動偵測和管理

新增了完整的印表機偵測和管理功能：

**自動偵測功能：**
- 使用 Windows WMI 查詢系統中所有印表機
- 根據連接埠自動配對印表機和設定
- 支援多種連接埠類型：LPT1-LPT9、USB001-USB999、COM1-COM99

**印表機資訊類別：**
```csharp
public class PrinterInfo
{
    public string Name { get; set; } = "";           // 印表機名稱
    public string PortName { get; set; } = "";       // 連接埠
    public string DriverName { get; set; } = "";     // 驅動程式名稱
    public string Status { get; set; } = "";         // 印表機狀態
    public bool IsDefault { get; set; }              // 是否為預設印表機
    public bool IsLocal { get; set; }                // 是否為本機印表機
    public bool IsShared { get; set; }               // 是否為共享印表機
}
```

### 3. 多種印表機格式支援

實作了三種主要的印表機控制語言：

**PCL (Printer Control Language)：**
- 適用於HP LaserJet等雷射印表機
- 包含完整的重置、字型、邊界設定命令
- 針對HP印表機優化的命令序列

**ESC/POS：**
- 適用於熱感應印表機和收據印表機
- 支援字元編碼、切紙命令
- 自動處理UTF-8編碼

**智能純文字：**
- 使用ASCII編碼確保相容性
- 添加適當的控制字元（CR+LF、Form Feed）
- 針對不同印表機類型動態調整

### 4. Windows API 整合

實作了真正的Windows列印API整合：

**System.Drawing.Printing（推薦方法）：**
- 使用Windows Graphics Device Interface (GDI)
- 自動透過印表機驅動程式處理格式轉換
- 支援圖形化列印內容

**Windows Spooler API（低階方法）：**
- 直接使用winspool.drv API
- 支援RAW和TEXT DataType
- 智能選擇適當的DataType

**API宣告：**
```csharp
[DllImport("winspool.drv", SetLastError = true)]
static extern bool OpenPrinter(string printerName, out IntPtr hPrinter, IntPtr pd);

[DllImport("winspool.drv", SetLastError = true)]
static extern bool StartDocPrinter(IntPtr hPrinter, int level, ref DOC_INFO_1 docInfo);

[DllImport("winspool.drv", SetLastError = true)]
static extern bool WritePrinter(IntPtr hPrinter, byte[] data, int count, out int written);
```

### 5. 編輯組件UI增強

在印表機配置編輯Modal中新增了「測試列印」按鈕：

**新增功能：**
- 按鈕位於Modal底部，與「取消」、「儲存」按鈕並列
- 智能驗證：確保必要的配置已填寫
- 即時反饋：顯示測試進度和結果
- 更新的幫助文字：顯示支援的埠格式

**檔案位置：**
- `Components/Pages/Systems/PrinterConfigurationEditModalComponent.razor`

## 🔧 技術實作細節

### 改進的測試流程

1. **配置驗證**
   - 檢查印表機名稱是否已填寫
   - 根據連接類型驗證必要設定：
     - 網路印表機：IP位址必填
     - USB印表機：埠格式驗證（支援LPT、USB、COM格式）

2. **印表機偵測**
   - 使用WMI查詢系統中所有印表機
   - 根據配置的埠名稱自動配對印表機
   - 記錄印表機詳細資訊（驅動程式、狀態等）

3. **連接測試**
   - 網路印表機：Ping測試 + TCP連接測試（埠9100）
   - USB印表機：檢查印表機是否在系統中註冊

4. **智能列印測試**
   - **優先方法**：使用System.Drawing.Printing（最可靠）
   - **備用方法**：使用Windows Spooler API with TEXT DataType
   - **最後方法**：使用Windows Spooler API with RAW DataType
   - 自動選擇最適合的印表機格式

### 網路印表機測試實作

```csharp
// 1. Ping測試
using var ping = new Ping();
var reply = await ping.SendPingAsync(ipAddress, 3000);

// 2. TCP連接測試（埠9100）
using var client = new TcpClient();
await client.ConnectAsync(ipAddress, 9100);

// 3. 嘗試多種格式直到成功
var formats = new[] { "PCL", "ESC/POS", "Plain Text" };
foreach (var format in formats)
{
    try
    {
        await stream.WriteAsync(formatData, 0, formatData.Length);
        // 成功則跳出迴圈
        break;
    }
    catch (Exception ex)
    {
        // 記錄錯誤，嘗試下一種格式
    }
}
```

### USB印表機測試實作

```csharp
// 1. 偵測印表機
var printer = FindPrinterByPort(usbPort); // 使用WMI查詢

// 2. 優先使用System.Drawing.Printing
using var printDocument = new System.Drawing.Printing.PrintDocument();
printDocument.PrinterSettings.PrinterName = printer.Name;
printDocument.PrintPage += (sender, e) => {
    // 渲染測試頁內容
};
printDocument.Print();

// 3. 備用：使用Windows API
await PrintUsingWindowsApi(printer.Name, textData, "ERP 測試頁");
```

## 📱 使用者體驗

### 操作流程

1. **開啟印表機配置編輯**
   - 新增或編輯印表機設定

2. **填寫必要資訊**
   - 印表機名稱（必填）
   - 選擇連接方式（網路/USB）
   - 根據連接方式填寫對應設定

3. **執行測試列印**
   - 點擊「測試列印」按鈕
   - 系統自動驗證配置
   - 顯示測試進度和結果

### 錯誤處理

- **配置不完整**：顯示具體缺少的設定項目
- **連接失敗**：提供詳細的錯誤訊息和可能的解決方案
- **列印失敗**：指引使用者檢查印表機狀態和驅動程式

## � 依賴套件

### 新增的NuGet套件

```xml
<PackageReference Include="System.Management" Version="9.0.0" />
<PackageReference Include="System.Drawing.Common" Version="9.0.0" />
```

### 平台相容性

- **Windows平台**：完整支援所有功能
- **非Windows平台**：自動降級，僅支援基本功能
- **編譯條件**：使用 `#if WINDOWS` 確保跨平台相容性

## 🧪 測試案例

### 網路印表機測試

1. **正常情況（HP LaserJet）**
   - IP: 192.168.1.100, Port: 9100
   - 預期：Ping成功 → TCP連接成功 → PCL格式列印成功

2. **網路問題**
   - IP: 192.168.1.999（無效IP）
   - 預期：Ping失敗 → 顯示連接錯誤

3. **埠關閉**
   - IP: 192.168.1.1, Port: 12345（未開放埠）
   - 預期：Ping成功 → TCP連接失敗

### USB印表機測試

1. **正常情況（HP LaserJet M14-M17）**
   - USB埠: USB002
   - 預期：偵測到印表機 → System.Drawing.Printing成功 → 實際列印測試頁

2. **印表機離線**
   - USB埠: USB001
   - 預期：WMI查詢失敗 → 顯示「找不到印表機」錯誤

3. **無效埠格式**
   - USB埠: INVALID_PORT
   - 預期：埠格式檢查失敗 → 顯示支援的格式說明

### 實際測試記錄

**成功案例：**
```
info: 找到印表機 'HP LaserJet M14-M17 PCLmS' 使用埠 'USB002'
info: 印表機詳細資訊: 驅動程式=HP LaserJet M14-M17 PCLmS, 狀態=3, 預設=False  
info: 成功列印 429 位元組到印表機 'HP LaserJet M14-M17 PCLmS'
info: 使用 PCL 格式列印成功
```

**常見問題解決：**
- **列印成功但無輸出**：已修改為使用TEXT DataType，讓Windows驅動程式處理格式轉換
- **權限不足**：確保程式以管理員身分執行
- **埠格式錯誤**：更新UI顯示支援的埠格式範例

## 📄 測試頁內容

系統會產生包含以下內容的測試頁：

```
=== 印表機測試頁 ===
測試時間: 2025-09-01 15:30:00
印表機名稱: [配置的印表機名稱]
連接方式: [網路連接/USB連接]
IP位址: [如果是網路印表機]
連接埠: [如果是網路印表機]
USB埠: [如果是USB印表機]

如果您能看到此測試頁，表示印表機配置正確！

測試內容:
- 中文字體測試: 這是中文測試文字
- 英文字體測試: This is English test text  
- 數字測試: 0123456789
- 符號測試: !@#$%^&*()

=== 測試頁結束 ===
```

## 🔮 未來擴展建議

### 已計劃的改進

1. **增強的印表機偵測**
   - 支援網路印表機自動發現（使用SNMP）
   - 印表機型號和功能自動識別
   - 即時狀態監控（紙張、墨水等）

2. **高級列印選項**
   - 支援不同紙張大小和方向
   - 列印品質和速度設定
   - 多頁測試文件和自訂內容

3. **企業級功能**
   - 批量印表機配置和測試
   - 列印使用量統計和報告
   - 自動化印表機健康檢查

4. **跨平台支援**
   - Linux CUPS整合
   - macOS Core Graphics支援
   - 統一的跨平台API介面

### 技術債務清理

1. **程式碼重構**
   - 將Windows特定程式碼分離到獨立模組
   - 建立印表機抽象介面層
   - 改善錯誤處理和日誌記錄

2. **測試覆蓋率**
   - 增加單元測試覆蓋率
   - 整合測試和端對端測試
   - 模擬印表機環境進行自動化測試

## 🛠️ 服務註冊

已在 `Data/ServiceRegistration.cs` 中添加服務註冊：

```csharp
// 印表機設定服務
services.AddScoped<IPrinterConfigurationService, PrinterConfigurationService>();
services.AddScoped<IPrinterTestService, PrinterTestService>();
```

## 💡 重要注意事項

### 已解決的關鍵問題

1. **DataType選擇問題**
   ```csharp
   // ❌ 舊版本：使用RAW繞過驅動程式
   DataType = "RAW"
   
   // ✅ 新版本：智能選擇，讓驅動程式處理
   DataType = DetermineDataType(printerName); // 返回 "TEXT" 或 "RAW"
   ```

2. **印表機偵測問題**
   ```csharp
   // ❌ 舊版本：假設USB001是有效路徑
   await File.WriteAllBytesAsync(@"\\.\USB001", data);
   
   // ✅ 新版本：使用WMI找到真實印表機
   var printer = FindPrinterByPort("USB001");
   await PrintUsingWindowsApi(printer.Name, data);
   ```

3. **為什麼Windows測試頁能成功？**
   - Windows使用GDI (Graphics Device Interface)
   - 透過印表機驅動程式進行格式轉換
   - 我們的新實作模擬了這個流程

### 權限和環境要求

1. **管理員權限**
   - USB印表機需要管理員權限存取系統印表機API
   - 網路印表機不需要特殊權限

2. **防火牆設定**
   - 確保印表機連接埠（通常是9100）未被封鎖
   - 允許ping和TCP連接到印表機IP

3. **印表機驅動程式**
   - 確保已安裝正確的印表機驅動程式
   - 驅動程式狀態正常（可在裝置管理員中檢查）

### 支援的埠格式

- **LPT埠**：LPT1, LPT2, ..., LPT9
- **USB埠**：USB001, USB002, ..., USB999
- **COM埠**：COM1, COM2, ..., COM99

### 效能考量

- 網路超時設定：5秒連接 + 2秒處理
- USB列印等待：3秒讓印表機處理資料
- WMI查詢快取：避免重複查詢系統印表機

## 📄 測試頁內容

系統會產生包含以下內容的標準化測試頁：

```
=== 印表機測試頁 ===
測試時間: 2025-09-02 15:30:00
印表機名稱: HP LaserJet M14-M17 PCLmS
連接方式: USB 連接
USB埠: USB002

如果您能看到此測試頁，表示印表機配置正確！

測試內容:
- 中文字體測試: 這是中文測試文字
- 英文字體測試: This is English test text  
- 數字測試: 0123456789
- 符號測試: !@#$%^&*()

=== 測試頁結束 ===
```

**新增的測試頁特色：**
- 動態顯示實際偵測到的印表機名稱
- 根據連接類型顯示對應的設定資訊
- 多語言字元測試確保編碼正確
- 適當的格式化確保在各種印表機上都能正確顯示

---

## 結論

這個完整的印表機測試功能實作解決了原本「顯示成功但無法列印」的問題，透過：

1. **真實的印表機偵測**：使用WMI查詢系統印表機
2. **智能格式選擇**：根據印表機類型選擇最適合的格式和DataType
3. **多層級降級策略**：從System.Drawing.Printing到Windows API的多種方法
4. **完整的錯誤處理**：詳細的日誌記錄和使用者友善的錯誤訊息

現在使用者可以真正信賴這個測試功能來驗證印表機設定是否正確，大大提升了系統的可用性和可靠性。
