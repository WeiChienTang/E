# 報表檔案重構說明

## 變更日期
2025-01-XX

## 重構目的
將分散在多個目錄的報表相關檔案整合至統一的目錄結構，提升程式碼可維護性和可讀性。

## 新目錄結構

### 📁 Services/Reports/Interfaces/
所有報表服務介面的集中位置：
- `IReportService.cs` - 通用報表服務介面
- `IPurchaseOrderReportService.cs` - 採購單報表服務介面
- `IPurchaseReceivingReportService.cs` - 進貨單報表服務介面
- `IPurchaseReturnReportService.cs` - 進貨退出單報表服務介面
- `IQuotationReportService.cs` - 報價單報表服務介面
- `ISalesOrderReportService.cs` - 銷貨單報表服務介面
- `ISalesReturnReportService.cs` - 銷貨退回單報表服務介面
- `IProductBarcodeReportService.cs` - 商品條碼報表服務介面

### 📁 Services/Reports/Configuration/
報表列印配置服務：
- `IReportPrintConfigurationService.cs` - 列印配置服務介面
- `ReportPrintConfigurationService.cs` - 列印配置服務實作

### 📁 Models/Reports/
報表相關模型：
- `ReportModels.cs` - 報表配置類別（ReportConfiguration、ReportField 等）
- `ReportDefinition.cs` - 報表定義類別
- `BatchPrintCriteria.cs` - 批次列印條件類別

## 向後相容性

為確保現有程式碼無需修改，已保留舊檔案並添加重導向：

### 舊介面檔案（Services/Reports/*.cs）
使用 `global using` 語句重導向至新的 Interfaces 目錄：
```csharp
global using ERPCore2.Services.Reports.Interfaces;
```

### 舊模型檔案（Models/ReportModels.cs）
- 使用 `global using` 導入 `ERPCore2.Models.Reports` 命名空間
- 保留 `SortDirection` 枚舉在 `ERPCore2.Models` 命名空間，以支援 `Models.SortDirection` 語法

### 舊服務檔案（Services/Systems/ReportPrintConfigurationService.cs）
使用 `global using` 語句重導向至新的 Configuration 目錄：
```csharp
global using ERPCore2.Services.Reports.Configuration;
```

## 命名空間對照

| 舊命名空間 | 新命名空間 |
|-----------|-----------|
| `ERPCore2.Services.Reports` (介面) | `ERPCore2.Services.Reports.Interfaces` |
| `ERPCore2.Services` (ReportPrintConfigurationService) | `ERPCore2.Services.Reports.Configuration` |
| `ERPCore2.Models` (報表模型) | `ERPCore2.Models.Reports` |

## ServiceRegistration.cs 更新

已更新服務註冊以使用完整命名空間：
```csharp
using ERPCore2.Services.Reports.Configuration;
using ERPCore2.Services.Reports.Interfaces;

// 報表列印配置服務
services.AddScoped<ERPCore2.Services.Reports.Configuration.IReportPrintConfigurationService, 
                  ERPCore2.Services.Reports.Configuration.ReportPrintConfigurationService>();

// 報表服務介面
services.AddScoped<ERPCore2.Services.Reports.Interfaces.IReportService, ReportService>();
services.AddScoped<ERPCore2.Services.Reports.Interfaces.IPurchaseOrderReportService, PurchaseOrderReportService>();
// ... 其他報表服務
```

## 新程式碼建議使用方式

### 使用介面時
```csharp
using ERPCore2.Services.Reports.Interfaces;

public class MyComponent
{
    [Inject] private IPurchaseOrderReportService ReportService { get; set; }
}
```

### 使用模型時
```csharp
using ERPCore2.Models.Reports;

var config = new ReportConfiguration
{
    Title = "報表標題"
};
```

### 使用配置服務時
```csharp
using ERPCore2.Services.Reports.Configuration;

public class MyService
{
    private readonly IReportPrintConfigurationService _configService;
}
```

## 注意事項

1. **SortDirection 枚舉位置**：保留在 `ERPCore2.Models` 命名空間，因為許多現有程式碼使用 `Models.SortDirection` 語法。

2. **逐步遷移**：現有程式碼可繼續使用舊的命名空間，建議在修改相關檔案時逐步更新至新命名空間。

3. **未來清理**：當所有程式碼都更新至新命名空間後，可移除舊的重導向檔案。
