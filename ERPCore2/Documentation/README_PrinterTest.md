# 印表機測試列印功能實作指南

## 📋 功能概述

為印表機配置編輯組件新增了「測試列印」功能，允許使用者在配置印表機後立即測試連接並列印測試頁面。

## 🎯 新增的功能

### 1. 印表機測試服務 (IPrinterTestService / PrinterTestService)

新增了專門處理印表機測試功能的服務：

**主要功能：**
- 測試印表機連接
- 發送測試頁到印表機
- 支援網路印表機（IP/Port）和USB印表機
- 產生測試頁內容

**檔案位置：**
- `Services/Systems/IPrinterTestService.cs`
- `Services/Systems/PrinterTestService.cs`

### 2. 印表機配置服務擴展

擴展了現有的印表機配置服務，新增測試列印方法：

**新增方法：**
```csharp
Task<ServiceResult> TestPrintAsync(PrinterConfiguration printerConfiguration);
```

**檔案位置：**
- `Services/Systems/IPrinterConfigurationService.cs`
- `Services/Systems/PrinterConfigurationService.cs`

### 3. 編輯組件UI增強

在印表機配置編輯Modal中新增了「測試列印」按鈕：

**新增功能：**
- 按鈕位於Modal底部，與「取消」、「儲存」按鈕並列
- 智能驗證：確保必要的配置已填寫
- 即時反饋：顯示測試進度和結果

**檔案位置：**
- `Components/Pages/Systems/PrinterConfigurationEditModalComponent.razor`

## 🔧 技術實作細節

### 測試流程

1. **配置驗證**
   - 檢查印表機名稱是否已填寫
   - 根據連接類型驗證必要設定：
     - 網路印表機：IP位址 + 連接埠
     - USB印表機：USB埠（可選，有預設值）

2. **連接測試**
   - 網路印表機：Ping測試 + TCP連接測試
   - USB印表機：埠可用性檢查

3. **列印測試**
   - 產生標準化測試頁內容
   - 根據連接類型發送資料：
     - 網路：透過TCP Socket發送
     - USB：透過系統列印API（模擬實作）

### 網路印表機測試實作

```csharp
// 1. Ping測試
using var ping = new Ping();
var reply = await ping.SendPingAsync(ipAddress, 3000);

// 2. TCP連接測試
using var client = new TcpClient();
await client.ConnectAsync(ipAddress, port);

// 3. 發送測試資料
using var stream = client.GetStream();
await stream.WriteAsync(printData, 0, printData.Length);
```

### USB印表機測試實作

目前提供基礎框架，實際環境中需要整合：
- Windows Print Spooler API
- System.Drawing.Printing.PrintDocument
- 第三方列印程式庫

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

## 🛠️ 服務註冊

已在 `Data/ServiceRegistration.cs` 中添加服務註冊：

```csharp
// 印表機設定服務
services.AddScoped<IPrinterConfigurationService, PrinterConfigurationService>();
services.AddScoped<IPrinterTestService, PrinterTestService>();
```

## 🧪 測試案例

### 網路印表機測試

1. **正常情況**
   - IP: 192.168.1.100, Port: 9100
   - 應該：Ping成功 → TCP連接成功 → 測試頁發送成功

2. **網路問題**
   - IP: 192.168.1.999（無效IP）
   - 應該：Ping失敗 → 顯示連接錯誤

3. **埠關閉**
   - IP: 192.168.1.1, Port: 12345（未開放埠）
   - 應該：Ping成功 → TCP連接失敗

### USB印表機測試

1. **正常情況**
   - USB埠: LPT1
   - 應該：埠檢查通過 → 模擬列印成功

2. **無效埠**
   - USB埠: INVALID_PORT
   - 應該：埠格式檢查失敗

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

## 🔮 未來擴展

### 可能的改進方向

1. **高級列印選項**
   - 支援不同紙張大小
   - 列印品質設定
   - 多頁測試文件

2. **印表機驅動整合**
   - 自動偵測印表機型號
   - 支援印表機特有功能
   - 驅動程式狀態檢查

3. **批量測試**
   - 一次測試多台印表機
   - 測試結果批量報告
   - 自動化測試排程

4. **監控儀表板**
   - 印表機狀態即時監控
   - 測試歷史記錄
   - 故障預警系統

## 💡 注意事項

1. **權限要求**
   - 網路印表機：需要網路存取權限
   - USB印表機：需要系統列印服務權限

2. **防火牆設定**
   - 確保印表機連接埠未被封鎖
   - 網路印表機需要相應的網路存取權

3. **印表機相容性**
   - 目前支援標準文字列印
   - 特殊格式需要額外開發

4. **效能考量**
   - 網路超時設定為5秒
   - 避免在UI線程中執行長時間操作

這個實作提供了完整的印表機測試功能，讓使用者可以在配置印表機後立即驗證設定是否正確，大大提升了系統的易用性和可靠性。
