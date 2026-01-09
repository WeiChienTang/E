# 訊息複製功能設計文件

## 概述

在採購單編輯 Modal 中新增「複製訊息」按鈕，讓使用者可以快速複製一段格式化的採購訊息，方便透過 Line、Email 等方式發送給供應商。

---

## 功能需求

### 使用者操作流程

1. 使用者在採購單編輯 Modal 中填寫完資料
2. 點擊「複製訊息」按鈕
3. 系統依據設定產生格式化訊息並複製到剪貼簿
4. 顯示「已複製到剪貼簿」的提示訊息（使用現有 `NotificationService.ShowSuccessAsync()`）

### 訊息格式範例

```
[第一段 - 問候語]
貴公司您好，我們希望與貴公司採購以下商品：

[第二段 - 商品明細]
1. 商品A x 10 個
2. 商品B x 5 箱
3. 商品C x 20 kg

[第三段 - 結語]
如有任何問題，請與我們聯繫，感謝您。
```

---

## 現有功能分析

### ✅ 可重用的現有功能

| 功能 | 來源 | 用途 |
|------|------|------|
| Toast 通知 | `NotificationService.ShowSuccessAsync()` | 顯示「已複製到剪貼簿」訊息 |
| 數字格式化 | `NumberFormatHelper.FormatSmart()` | 格式化數量、金額 |
| 字串組合 | `string.Join()` | 組合明細文字 |

### ❌ 需要新建的功能

| 功能 | 說明 |
|------|------|
| 剪貼簿 API | JavaScript 端的 `navigator.clipboard.writeText()` 封裝 |
| 文字範本處理 | 通用的變數替換邏輯（可套用到其他場景） |

---

## 系統設計

### 1. 資料表設計

#### TextMessageTemplate（文字訊息範本）

> **命名說明**：使用通用名稱 `TextMessageTemplate`，未來可套用於銷貨單、報價單等其他單據的訊息複製功能。

| 欄位名稱 | 型別 | 說明 | 必填 |
|---------|------|------|------|
| Id | int | 主鍵 | ✅ |
| TemplateCode | string(50) | 範本代碼（如：PurchaseOrder, SalesOrder） | ✅ |
| TemplateName | string(100) | 範本名稱（如：採購單訊息範本） | ✅ |
| HeaderText | string(500) | 第一段文字（問候語） | ✅ |
| FooterText | string(500) | 第三段文字（結語） | ✅ |
| DetailFormatJson | string | 明細格式設定（JSON） | ✅ |
| IsActive | bool | 是否啟用 | ✅ |
| SortOrder | int | 排序順序 | ✅ |
| CreatedAt | DateTime | 建立時間 | ✅ |
| UpdatedAt | DateTime? | 更新時間 | ❌ |
| CreatedBy | string | 建立者 | ❌ |
| UpdatedBy | string | 更新者 | ❌ |

#### DetailFormatConfig（明細格式設定類別）

```csharp
/// <summary>
/// 明細格式設定 - 用於序列化/反序列化 DetailFormatJson
/// </summary>
public class DetailFormatConfig
{
    public bool ShowProductCode { get; set; } = false;     // 顯示商品編號
    public bool ShowProductName { get; set; } = true;      // 顯示商品名稱
    public bool ShowQuantity { get; set; } = true;         // 顯示數量
    public bool ShowUnit { get; set; } = true;             // 顯示單位
    public bool ShowUnitPrice { get; set; } = false;       // 顯示單價
    public bool ShowSubtotal { get; set; } = false;        // 顯示小計
    public bool ShowRemark { get; set; } = false;          // 顯示備註
}
```

---

### 2. 檔案結構規劃

```
ERPCore2/
├── Data/
│   └── Entities/
│       └── Systems/
│           └── TextMessageTemplate.cs          # 新增：資料實體
│
├── Models/
│   └── DetailFormatConfig.cs                   # 新增：明細格式設定類別
│
├── Services/
│   ├── Notifications/
│   │   ├── INotificationService.cs             # 修改：新增 CopyToClipboardAsync 方法
│   │   └── NotificationService.cs              # 修改：實作剪貼簿功能
│   │
│   └── Systems/
│       ├── ITextMessageTemplateService.cs      # 新增：服務介面
│       └── TextMessageTemplateService.cs       # 新增：服務實作
│
├── Helpers/
│   └── Common/
│       └── TextTemplateHelper.cs               # 新增：通用文字範本處理
│
├── wwwroot/
│   └── js/
│       └── clipboard-helper.js                 # 新增：剪貼簿 JavaScript
│
├── Components/
│   └── Pages/
│       └── Systems/
│           └── TextMessageTemplateEditModalComponent.razor  # 新增：設定 Modal
│
└── Documentation/
    └── README_訊息複製設計.md                   # 本文件
```

---

### 3. Helper 設計

#### TextTemplateHelper（Helpers/Common/）

> **設計原則**：這是一個通用的文字範本處理 Helper，不限定於採購單，未來可套用到：
> - 銷貨單訊息複製
> - 報價單訊息複製
> - Email 範本生成
> - 其他需要變數替換的場景

```csharp
/// <summary>
/// 通用文字範本處理輔助類別
/// 提供變數替換、明細格式化等功能
/// </summary>
public static class TextTemplateHelper
{
    /// <summary>
    /// 替換範本中的變數
    /// 支援變數：{supplierName}, {customerName}, {orderCode}, {orderDate}, {companyName} 等
    /// </summary>
    /// <param name="template">範本文字</param>
    /// <param name="variables">變數字典 (key: 變數名, value: 值)</param>
    /// <returns>替換後的文字</returns>
    public static string ReplaceVariables(string template, Dictionary<string, string?> variables)
    {
        if (string.IsNullOrEmpty(template)) return string.Empty;
        
        var result = template;
        foreach (var variable in variables)
        {
            result = result.Replace($"{{{variable.Key}}}", variable.Value ?? string.Empty);
        }
        return result;
    }
    
    /// <summary>
    /// 格式化明細項目列表
    /// </summary>
    /// <typeparam name="TDetail">明細類型</typeparam>
    /// <param name="details">明細列表</param>
    /// <param name="config">格式設定</param>
    /// <param name="lineFormatter">行格式化函數</param>
    /// <returns>格式化後的明細文字</returns>
    public static string FormatDetailLines<TDetail>(
        IEnumerable<TDetail> details,
        DetailFormatConfig config,
        Func<TDetail, int, DetailFormatConfig, string> lineFormatter)
    {
        var lines = details
            .Select((detail, index) => lineFormatter(detail, index + 1, config))
            .Where(line => !string.IsNullOrWhiteSpace(line));
            
        return string.Join("\n", lines);
    }
    
    /// <summary>
    /// 組合完整訊息（問候語 + 明細 + 結語）
    /// </summary>
    public static string BuildFullMessage(string header, string details, string footer)
    {
        var parts = new List<string>();
        
        if (!string.IsNullOrWhiteSpace(header))
            parts.Add(header.Trim());
            
        if (!string.IsNullOrWhiteSpace(details))
            parts.Add(details.Trim());
            
        if (!string.IsNullOrWhiteSpace(footer))
            parts.Add(footer.Trim());
            
        return string.Join("\n\n", parts);
    }
}
```

---

### 4. NotificationService 擴充

在現有的 `INotificationService` 中新增剪貼簿方法：

```csharp
/// <summary>
/// 複製文字到剪貼簿
/// </summary>
/// <param name="text">要複製的文字</param>
/// <param name="showSuccessMessage">是否顯示成功訊息</param>
/// <returns>是否成功</returns>
Task<bool> CopyToClipboardAsync(string text, bool showSuccessMessage = true);
```

---

### 5. 設定 Modal 設計

#### 基本資訊區
- 範本代碼（下拉選單：採購單、銷貨單...）
- 範本名稱
- 是否啟用

#### 問候語區
- 第一段文字（多行文字輸入）
- 支援變數提示：`{supplierName}` 會被替換為供應商名稱

#### 明細格式區（勾選項目）
- [ ] 顯示商品編號
- [x] 顯示商品名稱
- [x] 顯示數量
- [x] 顯示單位
- [ ] 顯示單價
- [ ] 顯示小計
- [ ] 顯示備註

#### 結語區（FooterText）
- 多行文字輸入

#### 預覽區
- 即時預覽產生的訊息格式

---

## 實作步驟

### Phase 1: 資料層（需新建）
1. [x] 建立 `TextMessageTemplate` 實體類別
2. [x] 建立 `DetailFormatConfig` 設定類別
3. [x] 更新 DbContext 加入新資料表
4. [x] 建立 Migration
5. [x] 建立 `ITextMessageTemplateService` 介面與 `TextMessageTemplateService` 實作

### Phase 2: Helper 層
6. [x] 建立 `TextTemplateHelper` 類別（通用文字範本處理）
7. [x] 擴充 `INotificationService` 新增 `CopyToClipboardAsync` 方法
8. [x] 建立 `clipboard-helper.js` JavaScript 檔案

### Phase 3: UI 層
9. [x] 建立 `TextMessageTemplateEditModalComponent` 設定 Modal
10. [x] ~~在系統設定頁面加入範本管理入口~~ （改為直接從採購單設定齒輪開啟）
11. [x] 在採購單 Modal 加入「複製訊息」按鈕

### Phase 4: 整合與測試
12. [x] 加入 SeedData 預設範本
13. [ ] 測試各種情境

---

## 討論問題（已確認）

### Q1: 範本是否需要支援多個？
**決定**：每種單據類型一個範本（簡單優先），未來可擴充。

### Q2: 問候語的變數支援範圍？
**決定**：
- `{supplierName}` - 供應商/客戶名稱
- `{orderCode}` - 單據編號
- `{orderDate}` - 單據日期
- `{companyName}` - 本公司名稱

### Q3: 明細格式的自訂程度？
**決定**：只提供勾選顯示/隱藏欄位，預設格式為 `{index}. {productName} x {quantity} {unit}`

### Q4: 是否需要設定 Modal？
**決定**：需要，讓使用者可自行調整。

### Q5: 按鈕顯示時機？
**決定**：
- ✅ 按鈕**始終顯示**
- ⛔ 新增模式：按鈕 disabled（需先儲存）
- ⛔ 編輯模式但有未儲存變更：按鈕 disabled
- ✅ 編輯模式且已儲存：按鈕可點擊

> **實作方式**：與「列印」按鈕相同邏輯，使用 `canPrint` 類似的狀態變數 `canCopyMessage` 來控制。

### Q6: 設定 Modal 的 UI 簡化？
**決定**：保留資料表欄位，只簡化 UI。

**評估分析**：

| 方案 | 說明 | 優勢 | 劣勢 |
|------|------|------|------|
| A. 修改資料表 | 移除不必要欄位 | 資料表更簡潔 | 需重建 Migration、破壞通用性、未來擴展困難 |
| B. 只修改 UI | 保留欄位，隱藏 UI | 保持通用性、擴展容易、修改量小 | 資料表有冗餘欄位（可接受） |

**採用方案 B**：保留資料表欄位，簡化 UI。

**UI 簡化內容**：
- 移除顯示：範本編號、適用單據、範本名稱、是否啟用、排序
- 保留顯示：問候語、結語、明細格式設定、預覽
- Modal 標題改為：「採購單訊息設定」

**理由**：
1. 設計文件已規劃未來套用到銷貨單、報價單
2. SeedData 已建立多種範本可直接使用
3. 採購單透過 `TemplateCode = "PurchaseOrder"` 查詢固定範本
4. 修改量最小，風險最低

---

## 預設範本內容（SeedData）

### 採購單預設範本

```csharp
new TextMessageTemplate
{
    TemplateCode = "PurchaseOrder",
    TemplateName = "採購單訊息範本",
    HeaderText = "{supplierName}您好，\n\n我們希望與貴公司採購以下商品：",
    FooterText = "如有任何問題，請與我們聯繫，感謝您。\n\n{companyName}",
    DetailFormatJson = JsonSerializer.Serialize(new DetailFormatConfig
    {
        ShowProductName = true,
        ShowQuantity = true,
        ShowUnit = true
    }),
    IsActive = true,
    SortOrder = 1
}
```

---

## 相關文件

- [Helpers 結構說明](../Helpers/README_Helpers結構圖.md)
- [Shared 元件結構說明](../Components/Shared/README_Shared結構圖.md)
- [BaseModalComponent 套用說明](../Components/Shared/Base/README_套用基礎Modal.md)

---

## 實作檔案清單

### 新增檔案

| 檔案路徑 | 說明 |
|---------|------|
| `Data/Entities/Systems/TextMessageTemplate.cs` | 文字訊息範本實體類別 |
| `Models/DetailFormatConfig.cs` | 明細格式設定類別（JSON 序列化用） |
| `Services/Systems/ITextMessageTemplateService.cs` | 範本服務介面 |
| `Services/Systems/TextMessageTemplateService.cs` | 範本服務實作 |
| `Helpers/Common/TextTemplateHelper.cs` | 通用文字範本處理 Helper |
| `wwwroot/js/clipboard-helper.js` | 剪貼簿 JavaScript 封裝 |
| `Components/Pages/Systems/TextMessageTemplateEditModalComponent.razor` | 範本設定 Modal 元件 |
| `Data/SeedDataManager/Seeders/TextMessageTemplateSeeder.cs` | 預設範本 SeedData |
| `Migrations/xxxxxx_AddTextMessageTemplate.cs` | 資料庫遷移檔案 |

### 修改檔案

| 檔案路徑 | 修改內容 |
|---------|----------|
| `Data/Context/AppDbContext.cs` | 加入 `DbSet<TextMessageTemplate>` |
| `Data/ServiceRegistration.cs` | 註冊 `ITextMessageTemplateService` |
| `Services/Notifications/INotificationService.cs` | 新增 `CopyToClipboardAsync()` 方法簽章 |
| `Services/Notifications/NotificationService.cs` | 實作 `CopyToClipboardAsync()` 方法 |
| `Components/App.razor` | 引入 `clipboard-helper.js` script |
| `Components/Pages/Purchase/PurchaseOrderEditModalComponent.razor` | 整合「複製訊息」按鈕與設定功能 |

---

## PurchaseOrderEditModalComponent 修改細節

### 新增 inject
```csharp
@inject ITextMessageTemplateService TextMessageTemplateService
```

### 新增狀態變數
```csharp
private bool canCopyMessage = false;
private bool showTemplateSettingModal = false;
private int? currentTemplateId = null;
```

### 新增按鈕（CustomActionButtons）
```razor
<div class="btn-group me-2" role="group">
    <GenericButtonComponent Text="複製訊息" 
                          Variant="ButtonVariant.Gray" 
                          IconClass="bi-clipboard"
                          IsDisabled="@(!canCopyMessage)"
                          OnClick="HandleCopyMessage" />
    <button type="button" 
            class="btn btn-outline-secondary btn-sm" 
            title="訊息範本設定"
            @onclick="HandleOpenTemplateSettings">
        <i class="bi bi-gear"></i>
    </button>
</div>
```

### 新增方法
- `HandleCopyMessage()` - 複製格式化訊息到剪貼簿
- `HandleOpenTemplateSettings()` - 開啟範本設定 Modal
- `HandleTemplateSaved()` - 處理範本儲存完成事件

### canCopyMessage 狀態同步位置
與 `canPrint` 相同邏輯，在以下位置設定：
1. `LoadPurchaseOrderData()` - 新增模式設為 false，編輯模式設為 true
2. `HandleEntityLoaded()` - 上下筆切換時設為 true
3. `SavePurchaseOrderWrapper()` - 儲存成功後設為 true

---

## 變更記錄

| 日期 | 變更內容 |
|------|----------|
| 2026-01-09 | 初版設計 |
| 2026-01-09 | 更新：使用通用命名 `TextMessageTemplate`、`TextTemplateHelper`，重用現有 `NotificationService` |
| 2026-01-09 | Phase 1 完成：建立實體、Model、DbContext、Migration、Service |
| 2026-01-09 | Phase 2 完成：建立 TextTemplateHelper、擴充 NotificationService、建立 clipboard-helper.js |
| 2026-01-09 | Phase 3 完成：建立 TextMessageTemplateEditModalComponent、整合到 PurchaseOrderEditModalComponent |
| 2026-01-09 | Phase 4 部分完成：建立 TextMessageTemplateSeeder、資料庫遷移已應用 |
| 2026-01-09 | 修正：FormFieldDefinition 使用 `GroupName` 而非 `Section`；ButtonVariant 使用 `Gray` 而非 `Secondary` |
| 2026-01-09 | 修正：`detail.Product?.Unit?.Name` 取得單位名稱字串 |
| 2026-01-09 | 修正：formSections 格式錯誤（Key 應為 PropertyName，Value 為 SectionTitle） |
| 2026-01-09 | 決策：Q6 UI 簡化 - 採用方案 B（保留資料表欄位，只簡化 UI） |
| 2026-01-09 | UI 簡化：移除基本資訊區段（範本編號、適用單據、範本名稱、是否啟用、排序），只保留訊息內容設定 |
| 2026-01-09 | 新增 ShowRefreshButton 參數到 GenericEditModalComponent，移除「新增」和「重整」按鈕 |
| 2026-01-09 | 新增「載入預設範本」按鈕，可一鍵載入預設的問候語、結語和明細格式設定 |
| 2026-01-09 | 修正訊息顯示兩次問題：ShowSuccessMessage 方法新增空字串檢查 |
| 2026-01-09 | 驗證邏輯調整：問候語和結語可以為空白 |
| 2026-01-09 | 設計檢查完成：所有檔案與設計文件一致 |

---

## 待測試項目

- [ ] 新增採購單後，按鈕從 disabled 變為可點擊
- [ ] 點擊「複製訊息」成功複製並顯示 Toast
- [ ] 點擊齒輪圖示開啟設定 Modal
- [ ] 設定 Modal 修改後儲存成功
- [ ] 變數替換正確（{supplierName}, {companyName}, {orderDate}, {orderCode}）
- [ ] 明細格式設定生效（勾選/取消勾選欄位）
- [ ] SeedData 初次執行時正確建立預設範本
- [ ] 「載入預設範本」按鈕功能正常
- [ ] 問候語和結語可以為空白儲存

---

## 設計與實作一致性檢查

### ✅ 新增檔案（全部完成）

| 檔案路徑 | 狀態 | 說明 |
|---------|------|------|
| `Data/Entities/Systems/TextMessageTemplate.cs` | ✅ | 文字訊息範本實體類別 |
| `Models/DetailFormatConfig.cs` | ✅ | 明細格式設定類別 |
| `Services/Systems/ITextMessageTemplateService.cs` | ✅ | 服務介面 |
| `Services/Systems/TextMessageTemplateService.cs` | ✅ | 服務實作 |
| `Helpers/Common/TextTemplateHelper.cs` | ✅ | 通用文字範本處理 |
| `wwwroot/js/clipboard-helper.js` | ✅ | 剪貼簿 JavaScript |
| `Components/Pages/Systems/TextMessageTemplateEditModalComponent.razor` | ✅ | 範本設定 Modal |
| `Data/SeedDataManager/Seeders/TextMessageTemplateSeeder.cs` | ✅ | 預設範本 SeedData |

### ✅ 修改檔案（全部完成）

| 檔案路徑 | 狀態 | 修改內容 |
|---------|------|----------|
| `Data/Context/AppDbContext.cs` | ✅ | 加入 `DbSet<TextMessageTemplate>` |
| `Data/ServiceRegistration.cs` | ✅ | 註冊 `ITextMessageTemplateService` |
| `Services/Notifications/INotificationService.cs` | ✅ | 新增 `CopyToClipboardAsync()` 方法簽章 |
| `Services/Notifications/NotificationService.cs` | ✅ | 實作 `CopyToClipboardAsync()` 方法 |
| `Components/App.razor` | ✅ | 引入 `clipboard-helper.js` script |
| `Components/Pages/Purchase/PurchaseOrderEditModalComponent.razor` | ✅ | 整合「複製訊息」按鈕與設定功能 |

### ✅ PurchaseOrderEditModalComponent 整合項目

| 項目 | 狀態 |
|------|------|
| 新增 `@inject ITextMessageTemplateService` | ✅ |
| 新增 `canCopyMessage` 狀態變數 | ✅ |
| 新增 `showTemplateSettingModal` 狀態變數 | ✅ |
| 新增 `currentTemplateId` 狀態變數 | ✅ |
| 新增「複製訊息」按鈕（含齒輪設定圖示） | ✅ |
| 新增 `HandleCopyMessage()` 方法 | ✅ |
| 新增 `HandleOpenTemplateSettings()` 方法 | ✅ |
| 新增 `HandleTemplateSaved()` 方法 | ✅ |
| `canCopyMessage` 與 `canPrint` 同步邏輯 | ✅ |

