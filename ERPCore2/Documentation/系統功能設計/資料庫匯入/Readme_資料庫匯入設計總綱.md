# 資料庫匯入工具設計總綱

## 更新日期
2026-03-03（Phase 1 MVP 完成、Phase 2 智慧預設 + 友善錯誤提示完成）

---

## 概述

提供 **SuperAdmin 專用**的資料庫匯入工具，允許從外部 Excel 檔案將資料匯入到系統已有的 Entity 表中。此工具位於「個人化設定 → Debug Tab」，僅 SuperAdmin 可見、不影響一般使用者。

適用場景：
- 從其他系統/資料庫轉移資料到本系統
- 批次補建歷史資料
- 測試環境快速灌入資料

### 目前狀態

| 階段 | 狀態 | 說明 |
|------|------|------|
| Phase 1 — MVP | ✅ 完成 | 5 步精靈、Excel 解析、自動配對、預覽、EF Core Transaction 匯入 |
| Phase 2 — 體驗優化 | � 部分完成 | ✅ 智慧預設按鈕、✅ 友善錯誤提示、📋 進度條、📋 報告匯出 |
| Phase 3 — 進階功能 | 📋 待做 | FK 選取器、衝突偵測、對應 Profile 儲存、CSV 支援 |

---

## 架構圖

```
SuperAdminDebugTab.razor
  └→ [資料庫載入] 按鈕
       └→ DatabaseImportModalComponent.razor（BaseModalComponent 包裹，全寬 Modal）
            │
            ├─ Step 1：選擇目標 Entity（下拉選單，反射取得 DbSet 清單）
            ├─ Step 2：上傳 Excel 檔案 + 解析欄位標頭
            ├─ Step 3：欄位對應表（自動配對 + 手動調整 + 預設值設定）
            ├─ Step 4：預覽 & 驗證（前 10 筆 + 錯誤高亮）
            └─ Step 5：執行匯入（Transaction 包裹 + 進度顯示）

Services/Import/
  └→ IDatabaseImportService / DatabaseImportService
       ├─ GetEntityTableList()          ← 反射取得 AppDbContext 的 DbSet 清單
       ├─ GetEntityProperties(name)     ← 取得 Entity 的屬性資訊（型別/必填/FK）
       ├─ ParseExcelAsync(stream)       ← ClosedXML 讀取 Excel 標頭 + 資料
       ├─ AutoMapColumns(target, source)← 名稱相似度自動配對
       ├─ GetSmartDefaultValue(prop)    ← ✨ Phase 2 新增：依型別產生智慧預設值
       ├─ ValidateMapping(...)          ← 驗證必填欄位是否皆已對應或有預設值
       └─ ExecuteImportAsync(...)       ← 執行匯入（EF Core + Transaction）
```

---

## 設計核心原則

1. **僅限 EF Core 已定義的 Entity**：匯入目標從 `AppDbContext` 的 `DbSet<T>` 反射取得，所有寫入透過 EF Core 執行，享有型別檢查與驗證。
2. **BaseEntity 欄位自動填入**：`Id`（DB 自動生成）、`CreatedAt`（Now）、`CreatedBy`（當前用戶）、`Status`（Active）、`UpdatedAt`/`UpdatedBy`（null）不開放對應，由系統自動處理。
3. **三層防線處理不可 Null 欄位**（詳見下方章節）。
4. **精靈步驟模式（Wizard）**：Step 1→2→3→4→5 循序操作，流程清晰，不易出錯。
5. **Transaction 保護**：整批匯入包在同一個 Transaction 中，任何一筆失敗則全部 Rollback。
6. **SuperAdmin 專用**：此功能僅在 `SuperAdminDebugTab` 中出現，依循現有 `_isSuperAdmin` 機制控制可見性。

---

## 不可 Null 欄位處理策略（三層防線）

這是匯入工具最核心的設計問題：來源 Excel 的欄位一定與目標 Entity 不完全一致，當目標欄位不可 Null 而來源卻沒有對應資料時，如何處理？

### 第一層：反射自動偵測

系統透過反射分析 Entity 屬性，自動判斷每個欄位的必填狀態：

| 條件 | 判定 |
|------|------|
| 型別為 `Nullable<T>`（如 `int?`, `DateTime?`） | 可空 |
| 型別為 `string?`（reference type nullable） | 可空 |
| 有 `[Required]` Attribute | 必填 |
| 為 value type 且非 Nullable（如 `int`, `decimal`, `DateTime`） | 必填 |

### 第二層：使用者手動處理

在 Step 3 欄位對應表中，每個未對應的**必填欄位**旁邊會顯示「預設值」輸入欄：

```
┌──────────────┬──────────┬────────┬─────────────┬────────────────┬──────────┐
│  目標欄位     │ C# 型別   │ 必填？ │ 對應來源欄位  │ 預設值          │ 狀態     │
├──────────────┼──────────┼────────┼─────────────┼────────────────┼──────────┤
│ Code         │ string?  │        │ [▼ 編號]    │                │ ✅ 已對應 │
│ Name         │ string   │ ● 必填 │ [▼ 品名]    │                │ ✅ 已對應 │
│ Price        │ decimal  │ ● 必填 │ [▼ 售價]    │                │ ✅ 已對應 │
│ CategoryId   │ int      │ ● 必填 │ [▼ —]      │ [         ]    │ ❌ 需處理 │
│ UnitName     │ string   │ ● 必填 │ [▼ —]      │ [個        ]   │ ✅ 有預設 │
└──────────────┴──────────┴────────┴─────────────┴────────────────┴──────────┘
```

- 使用者可從下拉選單配對來源欄位
- 或在預設值欄位手動輸入固定值
- 必填欄位未做任何處理 → 紅色警告，**匯入按鈕不可按**

### 第三層：型別智慧預設（最終攞截）✅ 已實作

當必填欄位既沒有對應來源、使用者也未填預設值時，Step 3 頂部顯示「套用智慧預設（N 個必填欄位）」按鈕，僅在有未解決的必填欄位時出現。點擊後依型別自動填入：

| C# 型別 | 智慧預設值 |
|---------|-----------|
| `string` | `""`（空字串） |
| `int` / `long` | `0` |
| `decimal` / `double` / `float` | `0` |
| `DateTime` | `DateTime.Now` |
| `bool` | `false` |
| `enum` | 該 enum 的第一個值 |

> ⚠ **FK 欄位注意**：如 `CategoryId = 0` 可能違反外鍵約束。系統會在 Step 4 預覽階段嘗試偵測 FK 關聯，並在驗證結果中警告。

---

## 自動欄位配對演算法

Step 3 在載入完成後，系統嘗試自動建議欄位對應，使用者可手動調整：

```
優先度（由高到低）：

1. 完全相符（忽略大小寫）： "Code" ↔ "Code"              → 100%
2. 去底線/空格後相符：      "product_code" ↔ "ProductCode" → 90%
3. 包含關係：               "ProductName" ↔ "Name"         → 70%
4. 去除常見前綴後相符：     "prod_code" ↔ "Code"           → 60%
   （常見前綴：表名縮寫如 prod_, cust_, emp_）
```

僅在相似度 ≥ 70% 時自動配對，低於此閾值需手動選擇。

---

## 技術注意事項

| 項目 | 說明 | 處理方式 |
|------|------|---------|
| **外鍵約束** | CategoryId 等必須是 DB 中已存在的值 | Step 4 預覽時驗證 FK 存在性，不存在的標紅 |
| **唯一約束** | Code 欄位常有 Unique Index | 預覽階段查詢現有資料檢查重複，標記衝突行 |
| **型別轉換** | Excel 數值列可能讀成 string、日期格式不一 | 每欄位自動嘗試型別轉換，失敗時顯示友善提示（含期望格式範例） |
| **大量資料** | 逐筆 `Add()` + `SaveChanges()` 效能差 | 分批處理（每 100 筆 `SaveChangesAsync()`），顯示進度條 |
| **Enum 對應** | Excel 可能是中文或數字 | 支援數值直接轉 + 文字比對 enum 成員名稱 |
| **導航屬性** | 反射會取到 `Category`、`Department` 等導航屬性 | 過濾：僅保留 primitive / enum / DateTime / string 型別的可直接賦值欄位 |
| **BaseEntity 欄位** | Id、CreatedAt、CreatedBy 等 | 排除在對應清單外，系統自動處理 |
| **InputFile + IsLoading** | BaseModalComponent 的 IsLoading 會銷毀 BodyContent | 必須先讀完 Stream 再設 IsLoading（見 BUG-001） |

---

## 檔案結構

```
Components/Pages/Employees/PersonalPreference/
├── SuperAdminDebugTab.razor                  ← 修改：加入 [資料庫載入] 按鈕 + DatabaseImportModalComponent 觸發
└── DatabaseImportModalComponent.razor        ← 新增：匯入精靈 Modal（5 步 Wizard）
    └── DatabaseImportModalComponent.razor.css ← 新增：scoped CSS（步驟指示器、表選取、對應表格、預覽表格樣式）

Models/Import/
├── EntityTableInfo.cs                        ← DbSet 資訊 DTO（DbSetName / EntityShortName / DisplayName）
├── EntityPropertyInfo.cs                     ← 屬性資訊 DTO（型別/必填/Nullable/Enum/FK/MaxLength）
├── ColumnMapping.cs                          ← 單一欄位的對應定義 + MappingStatus enum
├── ExcelParseResult.cs                       ← Excel 解析結果 DTO（Headers / Rows / WorksheetName）
├── ImportPreviewRow.cs                       ← 預覽行 DTO + ImportCellValue（轉換結果/錯誤標記）
└── ImportResult.cs                           ← 匯入結果 DTO + ImportRowError（行錯誤明細）

Services/Import/
├─ IDatabaseImportService.cs                 ← 介面（7 個方法，含 GetSmartDefaultValue）
└─ DatabaseImportService.cs                  ← 實作（反射/ClosedXML/自動配對/型別轉換/智慧預設/EF Core Transaction）

Data/
└── ServiceRegistration.cs                    ← 修改：註冊 IDatabaseImportService / DatabaseImportService
```

---

## 精靈步驟詳細設計

### Step 1：選擇目標 Entity

- 下拉選單顯示 `AppDbContext` 中所有 `DbSet<T>` 的名稱（如 "Customers"、"Products"）
- 透過反射 `typeof(AppDbContext).GetProperties()` 取得型別為 `DbSet<>` 的屬性
- 選取後，自動解析出該 Entity 的所有可對應屬性（排除 BaseEntity 共用欄位與導航屬性）

### Step 2：上傳 Excel 檔案

- 使用 Blazor 內建 `InputFile` 元件
- 限制 `.xlsx` 格式，最大 20MB
- ClosedXML 讀取第一個 Worksheet
- **第一行視為欄位標頭**，後續行為資料
- 解析結果：來源欄位清單 + 資料陣列

### Step 3：欄位對應

- 表格化 UI，每一行是一個目標 Entity 屬性
- 每行包含：屬性名稱、C# 型別、必填標記、來源欄位下拉、預設值輸入、狀態指示
- 頁面載入時執行自動配對演算法，預填匹配結果
- 使用者可手動調整每個對應
- ✨ **「套用智慧預設」按鈕**（Phase 2）：僅在有未解決的必填欄位時顯示，點擊後依型別自動填入預設值
- 未對應的必填欄位以紅色標記，**所有必填欄位都有對應或預設值後，才能進入 Step 4**

### Step 4：預覽 & 驗證

- 顯示前 10 筆轉換後資料
- 紅色高亮：型別轉換失敗的儲存格
- 顯示摘要：總筆數、預計成功數、有問題數
- 驗證通過後啟用 [執行匯入] 按鈕

### Step 5：執行匯入

- 透過 EF Core `context.Set<TEntity>().Add()` 寫入
- 整體包在 `using var transaction = await context.Database.BeginTransactionAsync()` 中
- 分批處理：每 100 筆 `SaveChangesAsync()` + 進度回報
- 完成後顯示結果：成功 N 筆、失敗 N 筆（含失敗原因）
- 任一筆失敗 → 全部 Rollback + 顯示錯誤明細

---

## 實作階段規劃

### Phase 1 — MVP（核心功能）✅ 已完成

1. ✅ Models/Import DTO 類別建立（6 個 DTO）
2. ✅ IDatabaseImportService 介面 + DatabaseImportService 實作
3. ✅ DatabaseImportModalComponent.razor 精靈 UI（Step 1~5）
4. ✅ SuperAdminDebugTab 加入按鈕
5. ✅ ServiceRegistration 註冊

### Phase 2 — 體驗優化（部分完成）

6. ✅ 型別轉換錯誤的友善提示（錯誤訊息包含期望格式範例，如「期望格式：整數，如 123」）
7. ✅ Step 3 增加「套用智慧預設」一鍵按鈕（自動填入所有未解決必填欄位的型別預設值）
8. Step 5 匯入進度條 UI（目前僅在 Loading 訊息中顯示百分比文字）
9. 匯入結果報告可匯出（匯入失敗時的錯誤明細匯出為 Excel）

### Phase 3 — 進階功能（待做）

10. FK 欄位的「選取現有資料」選取器（點擊後彈出對應 Entity 的選取清單）
11. 唯一約束衝突檢測 + 跳過/覆蓋策略（Step 4 預覽時查重）
12. 欄位對應 Profile 儲存/載入（重複匯入同類表可沿用上次設定）
13. 支援 CSV 格式
14. 匯入操作日誌記錄（寫入 ErrorLogs 或獨立匯入日誌表）

---

## 已知問題與修正記錄

### BUG-001：InputFile 讀檔時 `_blazorFilesById` 為 null（已修正）

**現象**：Step 2 上傳 Excel 時拋出 `Cannot read properties of null (reading '_blazorFilesById')`

**原因**：`BaseModalComponent` 在 `IsLoading=true` 時以 loading spinner 完全取代 `BodyContent`（`@if (IsLoading) ... else if (BodyContent != null)`），導致 `InputFile` 元件從 DOM 移除，Blazor JS 端的檔案參考遺失。原程式碼在呼叫 `file.OpenReadStream()` 之前先設定了 `isLoading = true` + `StateHasChanged()`。

**修正**：將 `file.OpenReadStream().CopyToAsync(stream)` 移到 `isLoading = true` **之前**執行。先把檔案內容完整讀入 `MemoryStream`，再設定 loading 狀態進行解析。

```csharp
// ⚠ 必須先讀完檔案 Stream，再設定 isLoading。
using var stream = new MemoryStream();
await file.OpenReadStream(maxAllowedSize: 20 * 1024 * 1024).CopyToAsync(stream);
stream.Position = 0;

// 檔案已讀入記憶體，現在才可以安全地顯示 Loading
isLoading = true;
loadingMessage = "解析 Excel 中...";
StateHasChanged();
```

> ⚠ **通用注意**：所有使用 `BaseModalComponent` + `InputFile` 的元件，都必須遵守「先讀完 Stream，再設 IsLoading」的順序，否則會觸發同樣問題。

---

## 相關文件

- [README_個人化設定總綱.md](個人化設定/README_個人化設定總綱.md)（SuperAdminDebugTab 所在模組）
- [README_個人化設定_UI框架.md](個人化設定/README_個人化設定_UI框架.md)（Tab 架構與 Modal 開啟方式）
