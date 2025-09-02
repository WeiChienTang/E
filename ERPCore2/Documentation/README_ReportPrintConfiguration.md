# 報表列印配置管理系統實作指南

## ✅ 實作狀態總覽

**最後更新**：2025年9月2日  
**專案狀態**：階段一、二、三已完成 ✅  
**建置狀態**：成功通過建置測試 ✅

## 🎯 專案目標

實作一個彈性的報表列印配置管理系統，讓系統管理者可以透過網頁介面設定不同報表的印表機和紙張配置，而無需程式設計師修改程式碼。

## 📋 系統架構概覽

### 方案選擇：方案一 - 報表配置實體
- **優點**：使用者可透過介面靈活設定，符合現有實體管理模式
- **核心概念**：建立 `ReportPrintConfiguration` 實體，管理報表類型與列印設定的對應關係
- **設定優先順序**：使用者選擇 > 報表專用設定 > 系統預設設定

## 🗂️ 實作清單

### ✅ 階段一：資料庫結構建立（已完成）

#### ✅ 1. 建立報表列印配置實體
**檔案路徑**：`Data/Entities/Systems/ReportPrintConfiguration.cs`  
**狀態**：✅ 已完成並簡化實體結構

```csharp
/// <summary>
/// 報表列印配置實體 - 管理不同報表的列印設定
/// </summary>
public class ReportPrintConfiguration : BaseEntity
{
    /// <summary>
    /// 報表類型識別碼（如：PurchaseOrder, Invoice, Receipt）
    /// </summary>
    [Required(ErrorMessage = "報表類型為必填")]
    [MaxLength(50, ErrorMessage = "報表類型不可超過50個字元")]
    [Display(Name = "報表類型")]
    public string ReportType { get; set; } = string.Empty;

    /// <summary>
    /// 報表顯示名稱
    /// </summary>
    [Required(ErrorMessage = "報表名稱為必填")]
    [MaxLength(100, ErrorMessage = "報表名稱不可超過100個字元")]
    [Display(Name = "報表名稱")]
    public string ReportName { get; set; } = string.Empty;

    /// <summary>
    /// 印表機設定ID
    /// </summary>
    [Display(Name = "印表機設定")]
    public int? PrinterConfigurationId { get; set; }

    /// <summary>
    /// 紙張設定ID
    /// </summary>
    [Display(Name = "紙張設定")]
    public int? PaperSettingId { get; set; }

    // 導航屬性
    /// <summary>
    /// 印表機設定
    /// </summary>
    public PrinterConfiguration? PrinterConfiguration { get; set; }

    /// <summary>
    /// 紙張設定
    /// </summary>
    public PaperSetting? PaperSetting { get; set; }
}
```

#### ✅ 2. 建立資料庫移植檔案
**檔案路徑**：`Migrations/20250902015228_AddReportPrintConfiguration.cs`  
**狀態**：✅ 已完成並成功應用到資料庫

```bash
# ✅ 已執行完成
dotnet ef migrations add AddReportPrintConfiguration
dotnet ef database update
```

#### ✅ 3. 更新 DbContext
**檔案路徑**：`Data/Context/AppDbContext.cs`  
**狀態**：✅ 已完成

```csharp
public DbSet<ReportPrintConfiguration> ReportPrintConfigurations { get; set; }

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // ✅ 已新增配置
    modelBuilder.Entity<ReportPrintConfiguration>(entity =>
    {
        entity.Property(e => e.Id).ValueGeneratedOnAdd();

        // 外鍵關係設定
        entity.HasOne(e => e.PrinterConfiguration)
              .WithMany()
              .HasForeignKey(e => e.PrinterConfigurationId)
              .OnDelete(DeleteBehavior.SetNull);

        entity.HasOne(e => e.PaperSetting)
              .WithMany()
              .HasForeignKey(e => e.PaperSettingId)
              .OnDelete(DeleteBehavior.SetNull);

        // 建立複合索引確保報表類型的唯一性
        entity.HasIndex(e => e.ReportType)
              .IsUnique();
    });
}
```

### ✅ 階段二：服務層實作（已完成）

#### ✅ 4. 建立報表列印配置服務介面
**檔案路徑**：`Services/Systems/IReportPrintConfigurationService.cs`  
**狀態**：✅ 已完成

```csharp
/// <summary>
/// 報表列印配置服務介面
/// </summary>
public interface IReportPrintConfigurationService : IGenericManagementService<ReportPrintConfiguration>
{
    /// <summary>
    /// 根據報表類型取得列印配置
    /// </summary>
    Task<ReportPrintConfiguration?> GetByReportTypeAsync(string reportType);

    /// <summary>
    /// 檢查報表類型是否已存在
    /// </summary>
    Task<bool> IsReportTypeExistsAsync(string reportType, int? excludeId = null);

    /// <summary>
    /// 取得所有啟用的報表列印配置
    /// </summary>
    Task<List<ReportPrintConfiguration>> GetActiveConfigurationsAsync();

    /// <summary>
    /// 取得完整的報表列印配置（包含印表機和紙張設定）
    /// </summary>
    Task<ReportPrintConfiguration?> GetCompleteConfigurationAsync(string reportType);

    /// <summary>
    /// 批量更新報表列印配置
    /// </summary>
    Task<bool> BatchUpdateAsync(List<ReportPrintConfiguration> configurations);

    /// <summary>
    /// 複製報表列印配置
    /// </summary>
    Task<bool> CopyConfigurationAsync(string sourceReportType, string targetReportType, string targetReportName);
}
```

#### ✅ 5. 實作報表列印配置服務
**檔案路徎**：`Services/Systems/ReportPrintConfigurationService.cs`  
**狀態**：✅ 已完成，包含完整的 CRUD 操作和驗證邏輯

#### ✅ 6. 服務註冊
**檔案路徑**：`Data/ServiceRegistration.cs`  
**狀態**：✅ 已完成服務註冊到 DI 容器

### ✅ 階段三：前端介面建立（已完成）

#### ✅ 7. 建立報表列印配置欄位配置
**檔案路徑**：`Components/FieldConfiguration/ReportPrintConfigurationFieldConfiguration.cs`  
**狀態**：✅ 已完成

#### ✅ 8. 建立報表列印配置管理頁面
**檔案路徑**：`Components/Pages/Systems/ReportPrintConfigurationIndex.razor`  
**狀態**：✅ 已完成，可通過 `/report-print-configurations` 訪問

#### ✅ 9. 建立報表列印配置編輯組件
**檔案路徑**：`Components/Pages/Systems/ReportPrintConfigurationEditModalComponent.razor`  
**狀態**：✅ 已完成，包含完整的新增/編輯功能

## 🚀 可立即使用的功能

### 已實現的核心功能
- ✅ **完整的 CRUD 操作**：創建、讀取、更新、刪除報表列印配置
- ✅ **搜尋和篩選**：支援多欄位搜尋和狀態篩選
- ✅ **關聯管理**：印表機設定和紙張設定的關聯配置
- ✅ **驗證機制**：報表類型唯一性驗證
- ✅ **狀態管理**：啟用/停用狀態控制
- ✅ **批量操作**：批量更新和配置複製功能

### 訪問方式
- **URL**：`/report-print-configurations`
- **導航**：系統管理 > 報表列印配置

## 📋 下階段規劃

### 🔄 階段四：整合與擴展（待實作）

#### 10. 更新現有報表服務
  - 列印失敗的錯誤處理機制
  - 不同印表機類型的相容性處理
  - 服務方法的標準結構和異常處理模式

#### 6. 更新現有報表服務
**需要修改的檔案**：
- `Services/Reports/IReportService.cs` 
- `Services/Reports/ReportService.cs`
- `Services/Reports/IPurchaseOrderReportService.cs`
- `Services/Reports/PurchaseOrderReportService.cs`

**修改重點**：
```csharp
// 新增方法簽名，支援列印配置參數
Task<string> GeneratePurchaseOrderReportAsync(
    int purchaseOrderId, 
    ReportFormat format = ReportFormat.Html,
    PrinterConfiguration? printerConfig = null,
    PaperSetting? paperSetting = null);
```

#### 11. 建立報表列印配置控制器
**檔案路徑**：`Controllers/ReportPrintConfigurationController.cs`  
**狀態**：❌ 待實作

#### 12. 更新報表控制器
**檔案路徑**：`Controllers/ReportController.cs`  
**狀態**：❌ 待實作

**修改重點**：
```csharp
[HttpGet("purchase-order/{id}")]
public async Task<IActionResult> GetPurchaseOrderReport(
    int id, 
    [FromQuery] string format = "html",
    [FromQuery] int? printerConfigId = null, 
    [FromQuery] int? paperSettingId = null)
```

### 📋 階段五：前端擴展（待實作）

#### 13. 建立列印設定選擇組件
**檔案路徑**：`Components/Shared/PrintSettingsSelectionComponent.razor`  
**狀態**：❌ 待實作

```razor
@* 列印設定選擇組件 - 可重用於各種報表 *@
<div class="modal fade" id="printSettingsModal" tabindex="-1">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">@Title 列印設定</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <!-- 列印配置選擇 -->
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">取消</button>
                <button type="button" class="btn btn-primary" @onclick="ConfirmPrint">確定列印</button>
            </div>
        </div>
    </div>
</div>
```

### 📋 階段六：現有功能修改（待實作）

#### 14. 修改採購單編輯組件的列印功能
**檔案路徑**：`Components/Pages/Purchase/PurchaseOrderEditModalComponent.razor`  
**狀態**：❌ 待實作

**修改重點**：
```csharp
// 將 HandlePrint 方法改為使用配置化列印
private async Task HandlePrint()
{
    // 1. 檢查資料完整性
    // 2. 取得可用列印配置
    // 3. 讓使用者選擇配置或使用預設
    // 4. 執行列印
}
```

#### 15. 建立列印輔助類別
**檔案路徑**：`Helpers/ReportPrintHelper.cs`  
**狀態**：❌ 待實作

```csharp
/// <summary>
/// 報表列印輔助類別
/// </summary>
public static class ReportPrintHelper
{
    /// <summary>
    /// 取得可用的列印配置並處理使用者選擇
    /// </summary>
    public static async Task<ReportPrintConfiguration?> SelectPrintConfigurationAsync(
        string reportType,
        IReportPrintConfigurationService configService,
        IJSRuntime jsRuntime);

    /// <summary>
    /// 執行列印動作
    /// </summary>
    public static async Task ExecutePrintAsync(
        string reportUrl,
        IJSRuntime jsRuntime);
}
```

### 📋 階段七：系統整合（待實作）

#### 16. 新增導航功能表項目
**檔案路徑**：`Models/NavigationMenuItem.cs` 或相關導航配置  
**狀態**：❌ 待實作

#### 17. 測試整合功能
**檔案路徑**：專案完整測試  
**狀態**：❌ 待實作

### 🚀 實作進度總結

#### ✅ 已完成項目（2025-01-01）
- **資料庫結構**：ReportPrintConfiguration 實體類別及遷移檔案
- **服務層**：完整的 IReportPrintConfigurationService 介面及實作
- **前端元件**：管理頁面、編輯組件及欄位配置
- **系統註冊**：服務依賴注入及資料庫內容註冊

#### ❌ 待完成項目
- **報表服務整合**：修改現有報表服務以支援列印配置
- **控制器擴展**：建立專用 API 控制器
- **前端工具組件**：列印設定選擇組件
- **既有功能修改**：採購單列印功能升級
- **輔助工具類別**：ReportPrintHelper 實作
- **導航整合**：加入系統功能表
- **完整測試**：端到端功能驗證

### 📌 快速存取資訊

- **🌐 管理頁面網址**：`/report-print-configurations`
- **🗄️ 資料表名稱**：`ReportPrintConfigurations`
- **📁 核心檔案位置**：
  - 實體：`Data/Entities/ReportPrintConfiguration.cs`
  - 服務：`Services/ReportPrintConfigurationService.cs`
  - 頁面：`Components/Pages/Reports/ReportPrintConfigurationIndex.razor`

---

## 🔧 技術細節

### 實作優先順序（已調整）

#### ✅ 第一階段：核心架構（已完成）
1. ✅ 建立 `ReportPrintConfiguration` 實體
2. ✅ 資料庫遷移
3. ✅ 建立基礎服務介面和實作
4. ✅ 建立管理介面和編輯組件

#### ❌ 第二階段：服務層整合（待實作）
5. 修改現有報表服務
6. 更新控制器
7. 建立輔助類別

#### ❌ 第三階段：前端擴展（待實作）
8. 建立選擇組件
9. 修改現有編輯組件
10. 導航整合

#### ❌ 第四階段：測試與優化（待實作）
11. 整合測試
12. 使用者體驗優化
13. 文件完善

### 使用模式參考

### 管理者可進行的設定

1. **新增報表配置**
   ```
   報表類型：PurchaseOrder
   報表名稱：採購單
   配置名稱：A4標準列印
   印表機：辦公室雷射印表機
   紙張：A4橫向
   ```

2. **多重配置**
   ```
   配置1：A4標準列印（辦公室使用）
   配置2：A5簡易列印（現場使用）
   配置3：熱感印表機列印（倉庫使用）
   ```

### 使用者體驗流程

1. **使用者點擊列印按鈕**
2. **系統自動檢查可用配置**
   - 如果只有一個配置：直接使用
   - 如果有多個配置：顯示選擇對話框
   - 如果沒有配置：使用系統預設
3. **執行列印**

## 🚀 擴展計劃

### 未來功能
- 部門別列印配置
- 使用者個人偏好設定
- 列印歷史記錄
- 批量列印設定
- 列印預覽功能

### 支援更多報表類型
- 發票（Invoice）
- 收據（Receipt）
- 庫存報表（Inventory）
- 財務報表（Financial）

## 🔍 測試計劃

### 功能測試
1. 配置 CRUD 操作
2. 預設配置切換
3. 多配置選擇
4. 列印執行

### 整合測試
1. 與現有報表系統整合
2. 權限控制測試
3. 錯誤處理測試

## 📋 檢查清單

- [ ] 資料庫實體建立
- [ ] 資料庫遷移完成
- [ ] 服務層實作完成
- [ ] 控制器更新完成
- [ ] 管理介面建立
- [ ] 選擇組件建立
- [ ] 現有功能修改
- [ ] 服務註冊更新
- [ ] 導航選單更新
- [ ] 種子資料建立
- [ ] 功能測試完成
- [ ] 整合測試完成
- [ ] 文件撰寫完成

## 💡 重要注意事項

1. **向後相容性**：確保現有沒有配置的報表仍能正常列印
2. **錯誤處理**：當配置的印表機或紙張不可用時的處理邏輯
3. **權限控制**：確保只有授權使用者可以管理列印配置
4. **效能考量**：避免每次列印都查詢資料庫，考慮快取機制
5. **使用者體驗**：讓配置選擇過程簡單直觀

---

**實作完成後，系統管理者就能透過網頁介面靈活設定各種報表的列印選項，而無需程式設計師修改程式碼！**
