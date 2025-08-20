# 泛型報表系統

這是一個可重複使用的報表系統，設計用於生成各種類型的業務報表（如採購單、出貨單、進貨單等）。

## 系統架構

### 核心組件

1. **ReportModels.cs** - 報表配置模型
   - `ReportConfiguration` - 報表基本設定
   - `ReportHeaderSection` - 頁首區段
   - `ReportColumnDefinition` - 表格欄位定義
   - `ReportFooterSection` - 頁尾區段
   - `ReportData<TMainEntity, TDetailEntity>` - 報表資料容器

2. **IReportService.cs** - 報表服務介面
   - `GenerateHtmlReportAsync` - 生成 HTML 報表
   - `GeneratePdfReportAsync` - 生成 PDF 報表（待實作）
   - `GeneratePrintableReportAsync` - 生成可列印報表

3. **ReportService.cs** - 報表服務實作
   - 負責將配置和資料轉換為實際的報表內容

4. **PurchaseOrderReportService.cs** - 採購單報表服務
   - 採購單的具體報表實作

## 使用方式

### 1. 基本設定

在 `ServiceRegistration.cs` 中已註冊相關服務：

```csharp
services.AddScoped<IReportService, ReportService>();
services.AddScoped<IPurchaseOrderReportService, PurchaseOrderReportService>();
```

### 2. 在組件中使用

在採購單編輯組件中，列印按鈕會：

```csharp
private async Task HandlePrint()
{
    var reportUrl = $"/api/report/purchase-order/{PurchaseOrderId.Value}/preview";
    await JSRuntime.InvokeVoidAsync("window.open", reportUrl, "_blank");
}
```

### 3. API 端點

- `GET /api/report/purchase-order/{id}` - 生成採購單報表
- `GET /api/report/purchase-order/{id}/preview` - 預覽採購單報表
- `GET /api/report/purchase-order/{id}/print` - 可列印的採購單報表

### 4. 測試頁面

訪問 `/test-report` 頁面來測試報表功能。

## 擴展指南

### 新增其他類型的報表

1. **創建專門的報表服務**

```csharp
public interface ISalesOrderReportService
{
    Task<string> GenerateSalesOrderReportAsync(int salesOrderId, ReportFormat format = ReportFormat.Html);
    ReportConfiguration GetSalesOrderReportConfiguration();
}

public class SalesOrderReportService : ISalesOrderReportService
{
    // 實作銷貨單報表邏輯
}
```

2. **定義報表配置**

```csharp
public ReportConfiguration GetSalesOrderReportConfiguration()
{
    return new ReportConfiguration
    {
        Title = "銷貨單",
        // ... 其他配置
        HeaderSections = new List<ReportHeaderSection>
        {
            // 定義頁首區段
        },
        Columns = new List<ReportColumnDefinition>
        {
            // 定義表格欄位
        },
        FooterSections = new List<ReportFooterSection>
        {
            // 定義頁尾區段
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

## 功能特色

### 1. 彈性的配置系統
- 可自訂頁首、表格、頁尾區段
- 支援多種資料格式化選項
- 可設定欄位對齊、寬度等樣式

### 2. 智能資料解析
- 自動序號生成
- 關聯資料查詢（如商品名稱）
- 支援自訂值產生器

### 3. 多種輸出格式
- HTML（已完成）
- PDF（規劃中）
- Excel（規劃中）

### 4. 列印友善
- 自動分頁
- 列印專用樣式
- 列印預覽功能

## 待實作功能

1. **PDF 生成** - 使用 PuppeteerSharp 或 iTextSharp
2. **Excel 輸出** - 使用 EPPlus 或 NPOI
3. **範本系統** - 支援自訂報表範本
4. **批次報表** - 支援批次生成多個報表
5. **報表快取** - 提升大型報表的生成效能

## 技術說明

### 樣式系統
報表使用內嵌 CSS，確保列印時樣式正確顯示。主要樣式包括：
- 響應式布局
- 列印專用樣式（@media print）
- 統一的視覺風格

### 資料處理
- 使用反射機制動態取得屬性值
- 支援格式化字串（日期、數字等）
- 提供額外資料字典機制

### 錯誤處理
- 完整的例外處理機制
- 友善的錯誤訊息
- 日誌記錄功能

## 範例

檢視 `TestReport.razor` 頁面以瞭解完整的使用範例。
