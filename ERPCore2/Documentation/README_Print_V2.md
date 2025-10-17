# 列印功能實作指南（新版 V2）

## 📋 概述

本指南說明新版列印功能的架構與使用方式，採用**精確尺寸控制**的方式，完全消除瀏覽器列印邊距問題。

## 🏗️ 架構說明

### 核心組件

1. **ReportController.cs** - API 端點
   - 位置：`Controllers/ReportController.cs`
   - 提供 RESTful API 供前端調用
   - 端點：
     - `GET /api/report/purchase-order/{id}` - 生成報表（支援 `?autoprint=true` 參數自動列印）
     - `GET /api/report/purchase-order/{id}/print` - 生成報表並加入自動列印腳本（備用端點）
     - `GET /api/report/purchase-order/{id}/preview` - 預覽報表

2. **PurchaseOrderReportServiceV2.cs** - 新版報表服務
   - 位置：`Services/Reports/PurchaseOrderReportServiceV2.cs`
   - 使用 StringBuilder 直接生成 HTML
   - 完全控制 CSS 和樣式
   - 精確設定中一刀格式尺寸（215.9mm × 139.7mm）

3. **print-styles.css** - 通用列印樣式
   - 位置：`wwwroot/css/print-styles.css`
   - 包含 `@media print` 規則
   - 設定 `@page { margin: 0mm !important; }`
   - 完全消除瀏覽器預設邊距

4. **ReportPrintHelper.cs** - ⭐ 通用列印輔助類別（新增）
   - 位置：`Helpers/ReportPrintHelper.cs`
   - 提供可重用的列印方法
   - 包含驗證、URL 建立、隱藏 iframe 列印等功能
   - **所有單據的列印功能都應該使用此 Helper**

5. **PrintLayoutBase.razor** - 通用布局組件（備用）
   - 位置：`Components/Reports/Shared/PrintLayoutBase.razor`
   - 提供可重用的列印布局結構
   - 目前 V2 版本暫時不使用，保留供未來擴展

6. **PurchaseOrderPrintTemplate.razor** - Razor 範本（備用）
   - 位置：`Components/Reports/PurchaseOrderPrintTemplate.razor`
   - Blazor 頁面形式的列印範本
   - 可直接訪問路由 `/reports/purchase-order/{id}`

## 🎯 使用方式

### 1. 使用通用 Helper 進行列印（推薦）⭐

從 V2.1 版本開始，所有列印功能都應該使用 `ReportPrintHelper` 提供的通用方法：

#### 完整範例（採購單）

```csharp
private async Task HandlePrint()
{
    try
    {
        // 步驟 1: 使用通用 Helper 進行完整驗證（實體、ID、核准狀態）
        var (isValid, errorMessage) = ReportPrintHelper.ValidateForPrint(
            entity: editModalComponent?.Entity,
            entityId: PurchaseOrderId,
            isApproved: editModalComponent?.Entity?.IsApproved ?? false,
            entityName: "採購單",
            requireApproval: true  // 是否需要核准才能列印
        );
        
        if (!isValid)
        {
            await NotificationService.ShowWarningAsync(errorMessage);
            return;
        }
        
        // 步驟 2: 執行列印
        await HandleDirectPrint(null);
    }
    catch (Exception ex)
    {
        await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(HandlePrint), GetType());
        await NotificationService.ShowErrorAsync("列印處理時發生錯誤");
    }
}

private async Task HandleDirectPrint(ReportPrintConfiguration? printConfig)
{
    try
    {
        if (!PurchaseOrderId.HasValue)
        {
            await NotificationService.ShowWarningAsync("無法列印：採購單ID不存在");
            return;
        }

        // 步驟 3: 驗證列印配置
        var (isValid, errorMessage) = ReportPrintHelper.ValidateConfiguration(printConfig);
        if (!isValid)
        {
            await NotificationService.ShowWarningAsync($"列印配置無效：{errorMessage}");
            return;
        }

        // 步驟 4: 使用通用 Helper 建立列印 URL
        var printUrl = ReportPrintHelper.BuildPrintUrl(
            baseUrl: NavigationManager.BaseUri,
            reportType: "purchase-order",  // 單據類型
            documentId: PurchaseOrderId.Value,
            configuration: printConfig,
            autoprint: true
        );

        // 步驟 5: 使用通用 Helper 執行列印（隱藏 iframe 方式）
        var success = await ReportPrintHelper.ExecutePrintWithHiddenIframeAsync(
            printUrl: printUrl,
            jsRuntime: JSRuntime,
            iframeId: "printFrame"  // 可自訂 iframe ID
        );
        
        if (success)
        {
            var configName = ReportPrintHelper.GetDisplayName(printConfig);
            await NotificationService.ShowSuccessAsync($"已使用 {configName} 送出列印");
        }
        else
        {
            await NotificationService.ShowErrorAsync("列印執行失敗");
        }
    }
    catch (Exception ex)
    {
        await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(HandleDirectPrint), GetType());
        await NotificationService.ShowErrorAsync("列印執行時發生錯誤");
    }
}
```

#### Helper 方法說明

**1. ValidateForPrint** - 完整驗證
```csharp
// 一次性驗證實體、ID、核准狀態
var (isValid, errorMessage) = ReportPrintHelper.ValidateForPrint(
    entity: editModalComponent?.Entity,      // 實體物件
    entityId: PurchaseOrderId,               // 實體 ID
    isApproved: entity?.IsApproved ?? false, // 核准狀態
    entityName: "採購單",                     // 實體名稱（錯誤訊息用）
    requireApproval: true                     // 是否需要核准才能列印
);
```

**2. BuildPrintUrl** - 建立列印 URL
```csharp
// 自動處理 baseUrl、reportType、autoprint 參數
var printUrl = ReportPrintHelper.BuildPrintUrl(
    baseUrl: NavigationManager.BaseUri,      // 基礎 URL
    reportType: "purchase-order",            // 報表類型（API 路由）
    documentId: 123,                         // 單據 ID
    configuration: printConfig,              // 列印配置（可選）
    autoprint: true                          // 是否自動列印
);
// 產生: http://localhost:5000/api/report/purchase-order/123?autoprint=true
```

**3. ExecutePrintWithHiddenIframeAsync** - 執行列印
```csharp
// 使用隱藏 iframe 載入報表並自動觸發列印
var success = await ReportPrintHelper.ExecutePrintWithHiddenIframeAsync(
    printUrl: printUrl,       // 完整的列印 URL
    jsRuntime: JSRuntime,     // JavaScript 運行時
    iframeId: "printFrame"    // iframe ID（可自訂避免衝突）
);
```

**4. ValidateConfiguration** - 驗證列印配置
```csharp
// 驗證列印配置是否有效
var (isValid, errorMessage) = ReportPrintHelper.ValidateConfiguration(printConfig);
```

**5. GetDisplayName** - 取得配置顯示名稱
```csharp
// 取得友善的配置名稱（用於訊息顯示）
var configName = ReportPrintHelper.GetDisplayName(printConfig);
// 產生: "系統預設設定" 或 "採購單 (HP LaserJet) - A4"
```

### 2. 不同單據類型的列印範例

#### 進貨單列印
```csharp
var printUrl = ReportPrintHelper.BuildPrintUrl(
    baseUrl: NavigationManager.BaseUri,
    reportType: "purchase-receiving",  // 修改報表類型
    documentId: PurchaseReceivingId.Value,
    autoprint: true
);
```

#### 銷貨單列印
```csharp
var printUrl = ReportPrintHelper.BuildPrintUrl(
    baseUrl: NavigationManager.BaseUri,
    reportType: "sales-order",  // 修改報表類型
    documentId: SalesOrderId.Value,
    autoprint: true
);
```

#### 不需要核准的單據
```csharp
// 例如：出貨單可能不需要核准就能列印
var (isValid, errorMessage) = ReportPrintHelper.ValidateForPrint(
    entity: editModalComponent?.Entity,
    entityId: ShipmentId,
    isApproved: true,              // 不檢查核准狀態
    entityName: "出貨單",
    requireApproval: false         // 設為 false 跳過核准檢查
);
```

### 3. 舊版方式（不推薦，僅供參考）

<details>
<summary>點擊展開舊版程式碼</summary>

### 在 EditModal 中調用列印

目前的 `PurchaseOrderEditModalComponent.razor` 已經有列印邏輯：

```csharp
private async Task HandleDirectPrint(ReportPrintConfiguration? printConfig)
{
    if (!PurchaseOrderId.HasValue)
    {
        await NotificationService.ShowWarningAsync("無法列印：採購單ID不存在");
        return;
    }

    // 驗證列印配置
    var (isValid, errorMessage) = ReportPrintHelper.ValidateConfiguration(printConfig);
    if (!isValid)
    {
        await NotificationService.ShowWarningAsync($"列印配置無效：{errorMessage}");
        return;
    }

    // 建立列印 URL - 使用 autoprint 參數讓報表自動觸發列印
    var baseUrl = NavigationManager.BaseUri.TrimEnd('/');
    var printUrl = $"{baseUrl}/api/report/purchase-order/{PurchaseOrderId.Value}?autoprint=true";

    // 使用完全隱藏的 iframe 載入報表，報表會自動觸發列印
    await JSRuntime.InvokeVoidAsync("eval", $@"
        (function() {{
            // 移除舊的列印 iframe（如果存在）
            const oldFrame = document.getElementById('printFrame');
            if (oldFrame) {{
                oldFrame.remove();
            }}
            
            // 建立新的完全隱藏 iframe
            const iframe = document.createElement('iframe');
            iframe.id = 'printFrame';
            iframe.style.display = 'none';
            iframe.style.position = 'absolute';
            iframe.style.width = '0';
            iframe.style.height = '0';
            iframe.style.border = 'none';
            iframe.style.visibility = 'hidden';
            
            // 設定 iframe 來源並加入頁面（不需要 onload，報表自己會處理）
            document.body.appendChild(iframe);
            iframe.src = '{printUrl}';
        }})();
    ");
    
    var configName = ReportPrintHelper.GetDisplayName(printConfig);
    await NotificationService.ShowSuccessAsync($"已使用 {configName} 送出列印");
}
```

**重點說明**：
- 使用 `?autoprint=true` 參數而不是 `/print` 端點
- 使用完全隱藏的 iframe 載入報表（不會顯示額外網頁）
- 報表內建的腳本會檢測 `autoprint` 參數並自動觸發列印
- 只會出現一次瀏覽器的列印對話框

</details>

### 4. 測試列印功能

1. 啟動應用程式
2. 開啟採購單編輯頁面
3. 選擇或建立一張採購單
4. 核准採購單（列印功能需要已核准的單據）
5. 點擊「列印」按鈕
6. 系統會直接顯示瀏覽器的列印對話框（不會開啟新視窗或預覽頁面）

### 5. 手動測試 API

在瀏覽器中直接訪問：
```
# 一般查看
http://localhost:5000/api/report/purchase-order/1

# 自動列印模式
http://localhost:5000/api/report/purchase-order/1?autoprint=true
```

## 🔧 關鍵技術細節

### CSS 邊距消除

```css
@media print {
    @page {
        size: 215.9mm 139.7mm;
        margin: 0mm !important;  /* 完全消除邊距 */
    }
    
    html, body {
        margin: 0mm !important;
        padding: 0mm !important;
        width: 215.9mm !important;
        height: 139.7mm !important;
        overflow: hidden !important;
    }
}
```

**重點**：
- 使用 `!important` 強制覆蓋瀏覽器預設值
- 同時設定 `@page`、`html`、`body` 的邊距為 0
- 明確指定寬高尺寸，防止自動縮放

### 中一刀格式尺寸

- **寬度**: 215.9mm (8.5 英吋)
- **高度**: 139.7mm (5.5 英吋)
- **方向**: Portrait (直向)

### 自動列印觸發

```javascript
window.addEventListener('load', function() {
    const urlParams = new URLSearchParams(window.location.search);
    if (urlParams.get('autoprint') === 'true') {
        setTimeout(function() {
            window.print();
        }, 500);
    }
});
```

## 📊 目前支援的單據

- ✅ 採購單 (PurchaseOrder)
- ⏳ 進貨單 (PurchaseReceiving) - 待實作
- ⏳ 進貨退回 (PurchaseReturn) - 待實作
- ⏳ 銷貨單 (SalesOrder) - 待實作
- ⏳ 銷貨退回 (SalesReturn) - 待實作

## 🚀 新增其他單據列印

### 快速步驟（使用通用 Helper）⭐

1. **建立報表服務**
   - 複製 `PurchaseOrderReportServiceV2.cs`
   - 修改為對應的單據類型（例如：`PurchaseReceivingReportServiceV2.cs`）
   - 調整資料載入邏輯
   - 修改 HTML 生成邏輯

2. **註冊服務**
   - 在 `Data/ServiceRegistration.cs` 中註冊新服務

3. **新增 Controller 端點**
   - 在 `ReportController.cs` 中新增對應的 API 方法

4. **更新 EditModal（使用 Helper）**
   - 在對應的 EditModal 中加入列印按鈕
   - **使用 `ReportPrintHelper` 進行驗證和列印**

### 完整範例（進貨單）：

```csharp
// ===== 步驟 1: 建立服務介面 =====
public interface IPurchaseReceivingReportService
{
    Task<string> GeneratePurchaseReceivingReportAsync(
        int purchaseReceivingId, 
        ReportFormat format = ReportFormat.Html);
}

// ===== 步驟 2: 實作服務 =====
public class PurchaseReceivingReportServiceV2 : IPurchaseReceivingReportService
{
    // 複製 PurchaseOrderReportServiceV2 的架構
    private readonly IPurchaseReceivingService _purchaseReceivingService;
    private readonly ISupplierService _supplierService;
    // ... 其他依賴注入
    
    public async Task<string> GeneratePurchaseReceivingReportAsync(
        int purchaseReceivingId, 
        ReportFormat format = ReportFormat.Html)
    {
        // 載入資料
        var receiving = await _purchaseReceivingService.GetByIdAsync(purchaseReceivingId);
        // ... 載入其他相關資料
        
        // 生成 HTML
        return GenerateHtmlReport(receiving, ...);
    }
    
    private string GenerateHtmlReport(...)
    {
        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        // ... 生成 HTML 內容（參考 PurchaseOrderReportServiceV2）
        return html.ToString();
    }
}

// ===== 步驟 3: 註冊服務 =====
// 在 Data/ServiceRegistration.cs 中
services.AddScoped<IPurchaseReceivingReportService, PurchaseReceivingReportServiceV2>();

// ===== 步驟 4: 新增 Controller 端點 =====
// 在 Controllers/ReportController.cs 中
[HttpGet("purchase-receiving/{id}")]
public async Task<IActionResult> GetPurchaseReceivingReport(
    int id, 
    [FromQuery] bool autoprint = false)
{
    try
    {
        var html = await _purchaseReceivingReportService
            .GeneratePurchaseReceivingReportAsync(id);
        return Content(html, "text/html", Encoding.UTF8);
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"生成進貨單報表時發生錯誤：{ex.Message}");
    }
}

// ===== 步驟 5: 在 EditModal 中使用 Helper 調用 =====
// 在 PurchaseReceivingEditModalComponent.razor 中
private async Task HandlePrint()
{
    try
    {
        // 使用通用 Helper 驗證
        var (isValid, errorMessage) = ReportPrintHelper.ValidateForPrint(
            entity: editModalComponent?.Entity,
            entityId: PurchaseReceivingId,
            isApproved: editModalComponent?.Entity?.IsApproved ?? false,
            entityName: "進貨單",
            requireApproval: true
        );
        
        if (!isValid)
        {
            await NotificationService.ShowWarningAsync(errorMessage);
            return;
        }
        
        await HandleDirectPrint(null);
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync("列印處理時發生錯誤");
    }
}

private async Task HandleDirectPrint(ReportPrintConfiguration? printConfig)
{
    try
    {
        // 驗證配置
        var (isValid, errorMessage) = ReportPrintHelper.ValidateConfiguration(printConfig);
        if (!isValid)
        {
            await NotificationService.ShowWarningAsync($"列印配置無效：{errorMessage}");
            return;
        }

        // 使用 Helper 建立 URL（注意 reportType 改為 "purchase-receiving"）
        var printUrl = ReportPrintHelper.BuildPrintUrl(
            baseUrl: NavigationManager.BaseUri,
            reportType: "purchase-receiving",  // 修改為進貨單的路由
            documentId: PurchaseReceivingId.Value,
            configuration: printConfig,
            autoprint: true
        );

        // 使用 Helper 執行列印
        var success = await ReportPrintHelper.ExecutePrintWithHiddenIframeAsync(
            printUrl: printUrl,
            jsRuntime: JSRuntime,
            iframeId: "printFrame"
        );
        
        if (success)
        {
            var configName = ReportPrintHelper.GetDisplayName(printConfig);
            await NotificationService.ShowSuccessAsync($"已使用 {configName} 送出列印");
        }
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync("列印執行時發生錯誤");
    }
}
```

### 關鍵差異點

不同單據類型只需要修改：
1. **報表服務**: 資料載入邏輯和 HTML 生成內容
2. **Controller 路由**: `reportType` 參數（如 `"purchase-receiving"`、`"sales-order"`）
3. **EditModal**: `entityName` 和 `reportType` 參數

其他所有驗證、URL 建立、列印執行邏輯都由 `ReportPrintHelper` 統一處理！

## 📝 樣式客製化

### 修改表格樣式

編輯 `wwwroot/css/print-styles.css`：

```css
.print-table th {
    background-color: #f0f0f0;  /* 表頭背景色 */
    border: 1px solid #333;     /* 表頭邊框 */
}

.print-table td {
    border: 1px solid #333;     /* 儲存格邊框 */
    padding: 1mm;               /* 儲存格內距 */
}
```

### 修改公司標頭

編輯服務中的 `GenerateHeader` 方法：

```csharp
private void GenerateHeader(StringBuilder html, ...)
{
    html.AppendLine($"<div class='print-company-name'>{company?.CompanyName ?? "公司名稱"}</div>");
    html.AppendLine($"<div class='print-report-title'>採購單</div>");
}
```

## 🐛 疑難排解

### 問題：列印仍有邊距

**解決方案**：
1. 檢查瀏覽器列印設定
2. 確認 `print-styles.css` 已正確載入
3. 清除瀏覽器快取
4. 使用無痕模式測試

### 問題：尺寸不正確

**解決方案**：
1. 檢查 CSS 中的 `@page` 設定
2. 確認瀏覽器縮放比例為 100%
3. 檢查印表機設定是否正確

### 問題：內容被截斷

**解決方案**：
1. 減少內容高度
2. 調整字體大小
3. 使用分頁版面（上下兩部分）

## 📚 相關檔案

- `Controllers/ReportController.cs` - API 控制器
- `Services/Reports/PurchaseOrderReportServiceV2.cs` - 報表服務
- `wwwroot/css/print-styles.css` - 列印樣式
- **`Helpers/ReportPrintHelper.cs` - ⭐ 通用列印輔助工具（重要）**
- `Data/ServiceRegistration.cs` - 服務註冊

## ✅ 已完成項目

- [x] 建立 ReportController API
- [x] 實作 PurchaseOrderReportServiceV2
- [x] 建立通用列印 CSS
- [x] 整合到 PurchaseOrderEditModal
- [x] 完全消除列印邊距
- [x] 支援中一刀格式
- [x] **建立通用 ReportPrintHelper（V2.1）**
- [x] **重構採購單列印使用 Helper**

## 🔜 待辦事項

- [ ] 實作進貨單列印（使用 Helper）
- [ ] 實作進貨退回列印（使用 Helper）
- [ ] 實作銷貨單列印（使用 Helper）
- [ ] 實作銷貨退回列印（使用 Helper）
- [ ] 支援多頁列印
- [ ] 支援 Logo 顯示
- [ ] 支援自訂頁首頁尾
- [ ] 支援列印預覽功能增強

## 💡 最佳實踐

1. **始終使用 ReportPrintHelper** ⭐
   - 所有列印功能都應該使用 Helper 提供的方法
   - 不要自己手寫 JavaScript iframe 程式碼
   - 不要自己手寫驗證邏輯

2. **統一的 URL 命名規則**
   - 使用 kebab-case：`purchase-order`、`purchase-receiving`
   - Controller 路由應該對應 `reportType` 參數

3. **始終使用 mm 單位**
   - 確保尺寸精確

4. **測試多種瀏覽器**
   - Chrome、Edge、Firefox

5. **使用實際印表機測試**
   - 不要只依賴 PDF

6. **保持 CSS 簡潔**
   - 避免過於複雜的樣式

7. **版本控制**
   - 保留舊版服務以便回退

## 🎯 V2.1 版本更新重點

### 新增通用 Helper

**ReportPrintHelper.cs** 現在提供以下通用方法：

1. **ValidateForPrint** - 一站式驗證
   - 檢查實體是否存在
   - 檢查 ID 是否有效
   - 檢查核准狀態（可選）
   
2. **BuildPrintUrl** - 智能 URL 建立
   - 自動處理 baseUrl
   - 自動添加 autoprint 參數
   - 支援列印配置

3. **ExecutePrintWithHiddenIframeAsync** - 標準化列印執行
   - 自動清理舊 iframe
   - 完全隱藏設計
   - 錯誤處理

4. **ValidateConfiguration** - 配置驗證
5. **GetDisplayName** - 友善名稱顯示

### 使用 Helper 的好處

✅ **程式碼重用**: 所有單據使用相同邏輯  
✅ **減少錯誤**: 統一驗證和錯誤處理  
✅ **易於維護**: 修改一處即可影響所有單據  
✅ **一致體驗**: 所有列印功能行為相同  
✅ **快速開發**: 新增單據列印只需幾行程式碼

---

**建立日期**: 2025-01-17  
**最後更新**: 2025-01-17  
**版本**: 2.1 (新增通用 Helper)  
**作者**: GitHub Copilot
