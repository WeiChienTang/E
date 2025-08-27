# 泛型報表系統

這是一個可重複使用的報表系統，設計用於生成各種類型的業務報表（如採購單、出貨單、進貨單等）。系統已整合公司資訊傳遞、優化列印體驗，並提供無預覽視窗的直接列印功能。

## 系統架構

### 核心組件

1. **ReportModels.cs** - 報表配置模型
   - `ReportConfiguration` - 報表基本設定（支援公司資訊傳遞）
   - `ReportHeaderSection` - 頁首區段
   - `ReportColumnDefinition` - 表格欄位定義
   - `ReportFooterSection` - 頁尾區段（已移除報表生成時間）
   - `ReportData<TMainEntity, TDetailEntity>` - 報表資料容器

2. **IReportService.cs** - 報表服務介面
   - `GenerateHtmlReportAsync` - 生成 HTML 報表（含列印優化）
   - `GeneratePdfReportAsync` - 生成 PDF 報表（待實作）
   - `GeneratePrintableReportAsync` - 生成可列印報表

3. **ReportService.cs** - 報表服務實作
   - 負責將配置和資料轉換為實際的報表內容
   - 包含列印專用 CSS 樣式和 JavaScript 自動化

4. **PurchaseOrderReportService.cs** - 採購單報表服務
   - 採購單的具體報表實作
   - 已整合 ICompanyService 用於公司資訊載入
   - 支援從 PurchaseOrder.CompanyId 載入公司資料

## 使用方式

### 1. 基本設定

在 `ServiceRegistration.cs` 中已註冊相關服務：

```csharp
services.AddScoped<IReportService, ReportService>();
services.AddScoped<IPurchaseOrderReportService, PurchaseOrderReportService>();
```

### 2. 在組件中使用（最新版本 - 無預覽視窗）

在採購單編輯組件中，列印按鈕會使用隱藏 iframe 直接觸發列印：

```csharp
private async Task HandlePrint()
{
    // 檢查採購單狀態...
    
    // 使用 JavaScript 直接觸發列印，不開啟新視窗
    var reportUrl = $"/api/report/purchase-order/{PurchaseOrderId.Value}?format=html";
    
    // 建立隱藏的 iframe 來載入報表，然後觸發列印
    await JSRuntime.InvokeVoidAsync("eval", $@"
        var iframe = document.createElement('iframe');
        iframe.style.position = 'absolute';
        iframe.style.left = '-9999px';
        iframe.style.width = '1px';
        iframe.style.height = '1px';
        iframe.onload = function() {{
            try {{
                iframe.contentWindow.print();
                setTimeout(function() {{
                    document.body.removeChild(iframe);
                }}, 1000);
            }} catch(e) {{
                console.error('列印失敗:', e);
                document.body.removeChild(iframe);
            }}
        }};
        iframe.src = '{reportUrl}';
        document.body.appendChild(iframe);
    ");
}
```

### 3. API 端點

- `GET /api/report/purchase-order/{id}` - 生成採購單報表（支援 format 參數：html/pdf）
- `GET /api/report/purchase-order/{id}/preview` - 預覽採購單報表（含預覽按鈕）
- `GET /api/report/purchase-order/{id}/print` - 可列印的採購單報表（自動觸發列印對話框）

### 4. 測試頁面

訪問 `/test-report` 頁面來測試報表功能。

## 最新功能特色

### 1. 公司資訊整合
- **自動載入公司資料**：從 PurchaseOrder.CompanyId 屬性自動載入對應的公司資訊
- **服務整合**：PurchaseOrderReportService 已注入 ICompanyService
- **配置傳遞**：GetPurchaseOrderReportConfiguration 方法支援 Company 參數
- **完整資訊顯示**：報表中正確顯示公司名稱、地址、電話等資訊

### 2. 優化的列印體驗
- **無預覽視窗**：使用隱藏 iframe 技術，直接觸發列印對話框
- **列印專用 CSS**：包含 @media print 樣式，移除瀏覽器頁首頁尾
- **自動化處理**：列印完成後自動清理資源
- **錯誤處理**：完整的例外處理和資源清理機制

### 3. 客製化報表內容
- **移除狀態顯示**：報表中不再顯示採購單狀態
- **移除採購備註**：簡化報表內容，移除備註欄位
- **移除生成時間**：頁尾不再顯示報表生成時間
- **清潔版面**：專注於核心業務資訊

### 4. 智能資料處理
- **彈性的配置系統**：可自訂頁首、表格、頁尾區段
- **自動序號生成**：表格明細自動編號
- **關聯資料查詢**：自動載入商品名稱、供應商資訊等
- **支援自訂值產生器**：可擴展資料處理邏輯

## 擴展指南

### 新增其他類型的報表

1. **創建專門的報表服務**

```csharp
public interface ISalesOrderReportService
{
    Task<string> GenerateSalesOrderReportAsync(int salesOrderId, ReportFormat format = ReportFormat.Html);
    ReportConfiguration GetSalesOrderReportConfiguration(Company? company = null);
}

public class SalesOrderReportService : ISalesOrderReportService
{
    private readonly ISalesOrderService _salesOrderService;
    private readonly ICompanyService _companyService; // 整合公司服務
    private readonly IReportService _reportService;
    
    // 實作銷貨單報表邏輯，參考 PurchaseOrderReportService 的公司資訊整合方式
}
```

2. **定義報表配置**

```csharp
public ReportConfiguration GetSalesOrderReportConfiguration(Company? company = null)
{
    return new ReportConfiguration
    {
        Title = "銷貨單",
        CompanyInfo = company, // 傳遞公司資訊
        // ... 其他配置
        HeaderSections = new List<ReportHeaderSection>
        {
            // 定義頁首區段（包含公司資訊）
        },
        Columns = new List<ReportColumnDefinition>
        {
            // 定義表格欄位（移除不需要的欄位）
        },
        FooterSections = new List<ReportFooterSection>
        {
            // 定義頁尾區段（不包含生成時間）
        }
    };
}
```

3. **註冊服務**

在 `ServiceRegistration.cs` 中註冊新的報表服務：

```csharp
services.AddScoped<ISalesOrderReportService, SalesOrderReportService>();
```

4. **新增 API 端點**

在 `ReportController.cs` 中新增對應的 API 方法。

5. **前端整合**

使用隱藏 iframe 的列印方式：

```csharp
private async Task HandlePrint()
{
    var reportUrl = $"/api/report/sales-order/{SalesOrderId.Value}?format=html";
    // 使用相同的隱藏 iframe 技術
}
```

## 技術實作細節

### 公司資訊整合流程
1. **資料載入**：從 PurchaseOrder.CompanyId 載入公司資料
2. **服務注入**：PurchaseOrderReportService 注入 ICompanyService
3. **配置傳遞**：將 Company 物件傳遞給報表配置
4. **範本渲染**：在報表 HTML 中正確顯示公司資訊

### 列印優化技術
1. **隱藏 iframe**：建立不可見的 iframe 載入報表
2. **自動觸發**：iframe 載入完成後自動呼叫 window.print()
3. **資源清理**：列印完成後自動移除 iframe
4. **錯誤處理**：包含 try-catch 和 timeout 機制

### 樣式系統
- **響應式布局**：適應不同螢幕尺寸
- **列印專用樣式**：@media print 規則，設定邊距為 0
- **統一視覺風格**：一致的表格和版面設計
- **瀏覽器相容性**：支援主流瀏覽器的列印功能

### 資料處理機制
- **反射取值**：使用反射機制動態取得屬性值
- **格式化支援**：日期、數字等格式化
- **額外資料字典**：支援自訂資料傳遞
- **關聯資料載入**：自動載入相關實體資料

### 錯誤處理策略
- **完整例外處理**：所有關鍵方法都包含 try-catch
- **友善錯誤訊息**：使用者友善的錯誤提示
- **日誌記錄**：使用 ErrorHandlingHelper 記錄錯誤
- **降級處理**：關鍵功能失敗時的備用方案

## 版本歷程

### v2.0 (最新版本)
- ✅ 整合公司資訊傳遞功能
- ✅ 實作隱藏 iframe 直接列印
- ✅ 移除報表中的狀態、備註、生成時間
- ✅ 優化列印 CSS，移除瀏覽器頁首頁尾
- ✅ 完善錯誤處理和資源管理

### v1.0 (基礎版本)
- ✅ 基本報表生成功能
- ✅ HTML 格式輸出
- ✅ 泛型報表配置系統
- ✅ 採購單報表實作

## 待實作功能

1. **PDF 生成** - 使用 PuppeteerSharp 或 iTextSharp
2. **Excel 輸出** - 使用 EPPlus 或 NPOI
3. **範本系統** - 支援自訂報表範本
4. **批次報表** - 支援批次生成多個報表
5. **報表快取** - 提升大型報表的生成效能
6. **多語系支援** - 國際化報表內容
7. **數位簽章** - PDF 報表數位簽章功能

## 範例

檢視 `TestReport.razor` 頁面以瞭解完整的使用範例，或參考 `PurchaseOrderEditModalComponent.razor` 中的 `HandlePrint` 方法查看最新的列印實作。
