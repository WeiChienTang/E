# 系統代碼自動產生設計

> 最後更新：2026-03-12
> 專案：ERPCore2

---

## 一、設計背景

目前系統各模組在新增實體時，代碼（Code）的產生方式分散於各 Service 或 Helper 中，使用者無法自訂格式，也無法決定是否啟用自動產生。常見問題包含：

- 各模組格式不一致，難以維護
- 使用者無法調整前綴（Prefix）或流水號位數
- 無序號重置機制（按年、按月）
- 高並發下可能產生重複代碼（競爭條件）

本設計目標：**提供統一的代碼自動產生機制，支援使用者透過系統設定介面自訂各模組的代碼格式，並具備良好的並發安全性。**

---

## 二、架構總覽

```
Entity（資料模型）
  └─ CodeSetting.cs               -- 每個模組一筆設定

Service（商業邏輯）
  ├─ ICodeSettingService.cs        -- 介面
  ├─ CodeSettingService.cs         -- 實作（含並發安全邏輯）
  └─ CodeGenerationHelper.cs       -- 格式樣板解析、預覽工具

UI（使用者介面）
  └─ SystemParameterSettingsModal.razor
      └─ Tabs/
          └─ CodeSettingsTab.razor  -- 新 Tab：自動編號設定

Migration
  └─ AddCodeSettings               -- 新增 CodeSettings 資料表

Seeder
  └─ CodeSettingSeeder.cs          -- 預設各模組設定
```

---

## 三、Entity 設計

### `CodeSetting.cs`

繼承自 `BaseEntity`。

| 欄位 | 型別 | 說明 |
|------|------|------|
| `ModuleKey` | string | 模組識別鍵，例如 `"Customer"`、`"PurchaseOrder"` |
| `ModuleDisplayName` | string | UI 顯示名稱，例如 `"客戶"`（僅顯示用，不影響邏輯） |
| `IsAutoCode` | bool | 是否啟用自動產生代碼 |
| `Prefix` | string | 前綴字串，例如 `"CUS"`、`"PO"` |
| `FormatTemplate` | string | 格式樣板字串，例如 `"{Prefix}-{Year:yy}{Seq:4}"` |
| `CurrentSeq` | int | 目前流水號（不包含日期部分） |
| `CurrentYear` | int? | 目前已使用的年份（用於判斷是否需要年度重置） |
| `CurrentMonth` | int? | 目前已使用的月份（用於判斷是否需要月度重置） |
| `RowVersion` | byte[] | 樂觀鎖版本戳記（EF Core concurrency token） |

> `BaseEntity` 繼承欄位：`Id`、`Status`、`CreatedAt`、`CreatedBy`、`UpdatedAt`、`UpdatedBy`、`Remarks`

---

## 四、格式樣板（FormatTemplate）設計

### 支援的 Token

| Token | 說明 | 範例（2026年3月12日） |
|-------|------|----------------------|
| `{Prefix}` | 來自 `Prefix` 欄位的值 | `CUS` |
| `{Year:yy}` | 2 位西元年 | `26` |
| `{Year:yyyy}` | 4 位西元年 | `2026` |
| `{Month:MM}` | 2 位月份（補零） | `03` |
| `{Day:dd}` | 2 位日期（補零） | `12` |
| `{Seq:N}` | N 位流水號（補零），N 為 1–9 的數字 | `{Seq:4}` → `0001` |

### 格式範例

| 樣板 | 產生結果 |
|------|---------|
| `{Prefix}-{Seq:5}` | `CUS-00001` |
| `{Prefix}-{Year:yy}{Seq:4}` | `CUS-260001` |
| `{Prefix}-{Year:yyyy}-{Seq:4}` | `CUS-2026-0001` |
| `{Prefix}-{Year:yy}{Month:MM}{Seq:3}` | `CUS-2603001` |
| `{Prefix}-{Year:yyyy}{Month:MM}{Day:dd}-{Seq:3}` | `CUS-20260312-001` |

### 重置規則（自動推斷）

從 `FormatTemplate` 解析，**不需要額外設定欄位**：

| 樣板包含 | 推斷重置規則 |
|---------|------------|
| `{Month}` | 每月1日序號重置為 1 |
| `{Year}` 但不含 `{Month}` | 每年1月1日序號重置為 1 |
| 皆不含 | 永不重置，持續累加 |

### 驗證規則

- 格式字串**必須**包含 `{Seq:N}`，否則無法保證唯一性
- `{Seq:N}` 中的 N 必須為 1～9 的整數
- `{Prefix}` 若出現於樣板，則 `Prefix` 欄位不得為空

---

## 五、Service 層設計

### 並發安全機制

代碼產生是**寫多讀少**的敏感操作，使用**樂觀鎖 + 重試**方案：

```
1. 讀取 CodeSetting（含 RowVersion）
2. 判斷是否需要重置序號（年/月跨期）
3. 遞增 CurrentSeq
4. SaveChangesAsync()
   ├─ 成功 → 代入樣板產生代碼，回傳
   └─ DbUpdateConcurrencyException → 重試（最多 3 次）
5. 3 次皆失敗 → 回傳 ServiceResult 失敗，由呼叫端決定處理
```

### `ICodeSettingService` 主要方法

```csharp
// 取得所有模組設定（用於 UI 列表）
Task<List<CodeSetting>> GetAllAsync();

// 取得特定模組設定
Task<CodeSetting?> GetByModuleKeyAsync(string moduleKey);

// 產生並更新代碼（含樂觀鎖重試）
Task<ServiceResult<string>> GenerateCodeAsync(string moduleKey);

// 儲存設定（單筆）
Task<ServiceResult<CodeSetting>> SaveAsync(CodeSetting setting);

// 重置序號（SuperAdmin 操作）
Task<ServiceResult> ResetSeqAsync(string moduleKey);

// 預覽代碼（不實際遞增序號，用於 UI 即時預覽）
string PreviewCode(string prefix, string formatTemplate, int seq = 1);
```

### 格式樣板解析邏輯（`CodeGenerationHelper`）

```csharp
public static string Apply(string formatTemplate, string prefix, int seq, DateTime now)
{
    return formatTemplate
        .Replace("{Prefix}",       prefix)
        .Replace("{Year:yyyy}",    now.Year.ToString("D4"))
        .Replace("{Year:yy}",      (now.Year % 100).ToString("D2"))
        .Replace("{Month:MM}",     now.Month.ToString("D2"))
        .Replace("{Day:dd}",       now.Day.ToString("D2"))
        .Replace(Regex("{Seq:(\\d)}"), m => seq.ToString($"D{m.Groups[1].Value}"));
}

public static ResetMode InferResetMode(string formatTemplate)
{
    if (formatTemplate.Contains("{Month:")) return ResetMode.Monthly;
    if (formatTemplate.Contains("{Year:"))  return ResetMode.Yearly;
    return ResetMode.Never;
}
```

---

## 六、UI 設計（CodeSettingsTab）

### Tab 可見條件

在 `SystemParameterSettingsModal.razor` 的 `OnInitializedAsync` 中，依權限動態加入：

```csharp
// 有 System.CodeSettings 權限 或 SuperAdmin 才加入
if (hasCodeSettings || isSuperAdmin)
{
    tabDefinitions.Add(new FormTabDefinition
    {
        Label = L["Tab.CodeSettings"],
        Icon = "bi bi-hash",
        CustomContent = CreateCodeSettingsTabContent(),
        DebugComponentName = nameof(CodeSettingsTab)
    });
}
```

### Tab 介面設計

每個模組以一列呈現，包含：

| 欄位 | 控制項 | 說明 |
|------|--------|------|
| 模組名稱 | 文字標籤 | 不可編輯，顯示用 |
| 啟用自動編號 | Toggle | 關閉時該列其他設定 disabled |
| 前綴（Prefix） | 短文字輸入 | 最多 10 字元 |
| 格式樣板 | 文字輸入 + 即時預覽 | 輸入後即時計算預覽值 |
| 重置規則 | 唯讀推斷標籤 | 從樣板自動推斷，顯示說明文字 |
| 重置序號 | 按鈕（僅 SuperAdmin） | 需二次確認 |

**格式參數說明區**（固定顯示於列表下方）：

```
支援的格式參數：
{Prefix} 自訂前綴  {Year:yy} 2位年份  {Year:yyyy} 4位年份
{Month:MM} 月份    {Day:dd} 日期       {Seq:N} 流水號（N=位數，如 {Seq:4}）
```

---

## 七、支援模組清單

以下為計畫支援自動編號的模組，於 `CodeSettingSeeder` 初始化：

| `ModuleKey` | 顯示名稱 | 預設前綴 | 預設格式 |
|-------------|---------|---------|---------|
| `Customer` | 客戶 | `CUS` | `{Prefix}-{Year:yy}{Seq:4}` |
| `Supplier` | 供應商 | `SUP` | `{Prefix}-{Year:yy}{Seq:4}` |
| `Employee` | 員工 | `EMP` | `{Prefix}-{Seq:5}` |
| `Product` | 產品 | `PRD` | `{Prefix}-{Seq:5}` |
| `Quotation` | 報價單 | `QT` | `{Prefix}-{Year:yy}{Month:MM}{Seq:3}` |
| `PurchaseOrder` | 採購單 | `PO` | `{Prefix}-{Year:yy}{Month:MM}{Seq:3}` |
| `PurchaseReceiving` | 進貨單 | `PR` | `{Prefix}-{Year:yy}{Month:MM}{Seq:3}` |
| `PurchaseReturn` | 採購退貨單 | `PRT` | `{Prefix}-{Year:yy}{Month:MM}{Seq:3}` |
| `SalesOrder` | 銷貨單 | `SO` | `{Prefix}-{Year:yy}{Month:MM}{Seq:3}` |
| `SalesReturn` | 銷貨退貨單 | `SRT` | `{Prefix}-{Year:yy}{Month:MM}{Seq:3}` |
| `InventoryTransfer` | 庫存調撥單 | `IT` | `{Prefix}-{Year:yy}{Month:MM}{Seq:3}` |
| `WasteRecord` | 廢棄記錄 | `WR` | `{Prefix}-{Year:yy}{Month:MM}{Seq:3}` |

---

## 八、現有代碼產生邏輯的遷移

目前系統使用 `EntityCodeGenerationHelper.GenerateForEntity<T>()` 靜態方法（各 Service 呼叫）。

### 遷移策略

1. **Phase 1（當前）**：建立 `CodeSetting` Entity、`ICodeSettingService`、Seeder、`CodeSettingsTab` UI
2. **Phase 2**：`ICodeSettingService.GenerateCodeAsync()` 完成後，**逐一**將各 Service 的 `EntityCodeGenerationHelper` 呼叫替換為 `CodeSettingService`
3. **Phase 3**：確認所有模組切換完畢後，標記 `EntityCodeGenerationHelper` 為 `[Obsolete]`

> **注意**：`IsAutoCode = false` 時，`GenerateCodeAsync` 回傳 `null`，呼叫端保留原本的手動輸入邏輯，不影響現有行為。

---

## 九、權限設計

| 操作 | 所需權限 |
|------|---------|
| 查看 CodeSettings Tab | `System.CodeSettings`（或 SuperAdmin） |
| 啟用/停用、修改格式 | `System.CodeSettings` |
| 重置序號 | SuperAdmin only |

`PermissionRegistry.System.CodeSettings` 需新增至 `PermissionRegistry.cs`。

---

## 十、實作順序

| 步驟 | 項目 | 備註 |
|------|------|------|
| 1 | `CodeSetting.cs` Entity | 繼承 BaseEntity，加 RowVersion |
| 2 | EF Core Migration | 新增 CodeSettings 資料表 |
| 3 | `ICodeSettingService` + `CodeSettingService` | 含 GenerateCodeAsync 樂觀鎖邏輯 |
| 4 | `CodeGenerationHelper` | 格式解析、預覽、重置推斷 |
| 5 | `CodeSettingSeeder` | 12 個模組預設值 |
| 6 | `PermissionRegistry` 新增 `System.CodeSettings` | |
| 7 | `CodeSettingsTab.razor` | UI 元件 |
| 8 | `SystemParameterSettingsModal` 加入 Tab | 依權限動態加入 |
| 9 | 各模組 Service 切換至 `CodeSettingService` | Phase 2，逐步進行 |
