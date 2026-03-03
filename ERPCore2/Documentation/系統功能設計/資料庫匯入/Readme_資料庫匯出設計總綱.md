# 資料庫匯出工具設計總綱

## 更新日期
2026-03-03（Phase 1 MVP 完成）

---

## 概述

提供 **SuperAdmin 專用**的資料庫匯出工具，允許將系統 Entity 表中的資料匯出為 Excel 檔案。此工具位於「個人化設定 → Debug Tab」，僅 SuperAdmin 可見、不影響一般使用者。

適用場景：
- 資料備份與封存
- 跨系統資料遷移（匯出後提供給其他系統或人員）
- 資料分析（匯出後在 Excel 中進行進階篩選/統計）
- 除錯用途（檢視特定資料表的完整資料）

### 目前狀態

| 階段 | 狀態 | 說明 |
|------|------|------|
| Phase 1 — MVP | ✅ 完成 | 3 步精靈、多選/全選資料表、預覽確認、ClosedXML 產生 Excel、瀏覽器下載 |
| Phase 2 — 體驗優化 | 📋 待做 | 欄位篩選、資料條件篩選、匯出格式選擇（CSV）、進度條 UI |
| Phase 3 — 進階功能 | 📋 待做 | 排程匯出、匯出模板儲存/載入、資料脫敏選項 |

---

## 架構圖

```
SuperAdminDebugTab.razor
  └→ [資料庫匯出] 按鈕
       └→ DatabaseExportModalComponent.razor（BaseModalComponent 包裹，全寬 Modal）
            │
            ├─ Step 1：選擇匯出資料表（多選清單 + 全選/取消全選 + 搜尋過濾）
            ├─ Step 2：預覽確認（各表資料筆數/欄位數摘要 + 大資料量警告）
            └─ Step 3：匯出結果（成功/失敗 + 各表摘要 + 下載按鈕）

Services/Export/
  └→ IDatabaseExportService / DatabaseExportService
       ├─ GetEntityTableList()              ← 反射取得 AppDbContext 的 DbSet 清單
       ├─ GetExportableProperties(name)     ← 取得 Entity 的所有可匯出屬性（含 BaseEntity 欄位）
       ├─ GetTableRowCountAsync(name)       ← 查詢指定表的資料筆數
       ├─ ExportSingleTableAsync(name)      ← 匯出單一表為 Excel
       ├─ ExportMultipleTablesAsync(names)  ← 匯出多表為 Excel（每表一個 Worksheet）
       └─ ExportAllTablesAsync()            ← 匯出全部表
```

---

## 設計核心原則

1. **僅限 EF Core 已定義的 Entity**：匯出目標從 `AppDbContext` 的 `DbSet<T>` 反射取得，所有讀取透過 EF Core `AsNoTracking()` 執行。
2. **所有欄位皆匯出**：與匯入工具不同，匯出包含所有可讀取的 primitive 屬性（含 Id、CreatedAt 等 BaseEntity 欄位），排除導航屬性。
3. **多表合併為一個 Excel**：選取多個表時，每個表會在同一個 Excel 檔案中各佔一個 Worksheet。
4. **精靈步驟模式（Wizard）**：Step 1→2→3 循序操作。
5. **瀏覽器端下載**：透過既有的 `downloadFileFromBase64` JS 函式觸發瀏覽器下載，不在伺服器端寫入暫存檔。
6. **SuperAdmin 專用**：此功能僅在 `SuperAdminDebugTab` 中出現，依循現有 `_isSuperAdmin` 機制控制可見性。

---

## 與匯入工具的關係

匯出工具複用了匯入工具的部分 DTO：

| DTO | 來源 | 用途 |
|-----|------|------|
| `EntityTableInfo` | `Models/Import/` | DbSet 清單（匯出 Step 1 選表用） |
| `EntityPropertyInfo` | `Models/Import/` | Entity 屬性資訊（取得欄位數、型別顯示） |

匯出專屬 DTO 放在 `Models/Export/`：

| DTO | 說明 |
|-----|------|
| `ExportResult` | 匯出結果（成功/失敗、檔案內容、摘要） |
| `ExportTableSummary` | 單一表的匯出摘要（表名、筆數、欄位數） |

---

## Excel 格式規範

| 項目 | 說明 |
|------|------|
| **格式** | .xlsx（ClosedXML） |
| **標頭行** | 第一行為 Entity 的 C# 屬性名稱，粗體白字藍底 |
| **欄位排序** | `Id` 欄位排第一，其餘依字母排序 |
| **自動篩選** | 每個 Worksheet 啟用 AutoFilter |
| **凍結首行** | 標頭行凍結（FreezeRows(1)） |
| **欄寬自適應** | `AdjustToContents` 依前 100 行自動調整 |
| **DateTime 格式** | `yyyy-MM-dd HH:mm:ss` |
| **DateOnly 格式** | `yyyy-MM-dd` |
| **Null 值** | 空白儲存格，字體設為淺灰色 |
| **Bool 值** | 原生 True/False |
| **Enum 值** | 以名稱字串輸出（如 "Active"） |
| **Worksheet 名稱** | 等同 DbSet 名稱（如 "Customers"），超過 31 字元截斷 |

---

## 技術注意事項

| 項目 | 說明 | 處理方式 |
|------|------|---------|
| **大量資料** | 表中可能有上萬筆資料 | Step 2 預先查詢筆數並警告；EF Core `AsNoTracking()` 讀取 |
| **記憶體** | 整個 Excel 在記憶體中產生 | 使用 `MemoryStream` → `byte[]`，匯出完成後即釋放 |
| **Worksheet 名稱限制** | Excel Worksheet 名稱最多 31 字元 | 超過時截斷處理，重複時加後綴 `_1`、`_2` |
| **導航屬性** | 反射會取到導航屬性 | 過濾：僅保留 primitive / enum / DateTime / string / Guid 型別 |
| **Decimal 精度** | ClosedXML 使用 double | `decimal` 轉 `double` 輸出（注意精度限制） |
| **檔案下載** | Blazor Server 無法直接推送檔案 | 轉 Base64 後呼叫 `downloadFileFromBase64` JS 函式 |
| **大檔案下載** | Base64 會使大小增加約 33% | 超過 10,000 筆顯示警告提示 |

---

## 精靈步驟詳細設計

### Step 1：選擇匯出資料表

- 多選清單顯示 `AppDbContext` 中所有 `DbSet<T>`
- 提供「全選」和「取消全選」按鈕
- 搜尋框可過濾表名（即時篩選，支援 DbSetName 和 EntityShortName）
- 每個項目顯示 Checkbox + DbSetName + EntityShortName
- 至少選擇 1 個表才能進入下一步

### Step 2：預覽確認

- 表格顯示已選表的摘要：序號、DbSetName、Entity 名稱、欄位數、資料筆數
- 底部顯示合計筆數
- 超過 10,000 筆總量時顯示黃色警告
- 點擊「執行匯出」開始產生 Excel

### Step 3：匯出結果

- 成功：顯示成功圖示、匯出統計（表數/總筆數/檔案大小/耗時）、各表匯出摘要表格、「下載 Excel」按鈕
- 失敗：顯示失敗圖示和錯誤訊息
- 「完成」按鈕關閉 Modal

---

## 檔案結構

```
Components/Pages/Employees/PersonalPreference/
├── SuperAdminDebugTab.razor                   ← 修改：加入 [資料庫匯出] 按鈕 + DatabaseExportModalComponent 觸發
├── DatabaseImportModalComponent.razor         ← 既有（匯入工具）
├── DatabaseImportModalComponent.razor.css     ← 既有
├── DatabaseExportModalComponent.razor         ← 新增：匯出精靈 Modal（3 步 Wizard）
└── DatabaseExportModalComponent.razor.css     ← 新增：scoped CSS

Models/Export/
└── ExportResult.cs                            ← 新增：匯出結果 DTO + ExportTableSummary

Models/Import/
├── EntityTableInfo.cs                         ← 既有（匯出複用）
└── EntityPropertyInfo.cs                      ← 既有（匯出複用）

Services/Export/
├── IDatabaseExportService.cs                  ← 新增：匯出服務介面（6 個方法）
└── DatabaseExportService.cs                   ← 新增：匯出服務實作（反射/ClosedXML/EF Core AsNoTracking）

Data/
└── ServiceRegistration.cs                     ← 修改：註冊 IDatabaseExportService / DatabaseExportService
```

---

## 實作階段規劃

### Phase 1 — MVP（核心功能）✅ 已完成

1. ✅ Models/Export DTO 類別建立（ExportResult、ExportTableSummary）
2. ✅ IDatabaseExportService 介面 + DatabaseExportService 實作
3. ✅ DatabaseExportModalComponent.razor 精靈 UI（Step 1~3）
4. ✅ DatabaseExportModalComponent.razor.css scoped 樣式
5. ✅ SuperAdminDebugTab 加入匯出按鈕
6. ✅ ServiceRegistration 註冊

### Phase 2 — 體驗優化（待做）

7. 欄位篩選（Step 1 選完表後可勾選要匯出的欄位）
8. 資料條件篩選（如只匯出 Status = Active 的資料）
9. 支援 CSV 格式匯出
10. Step 3 匯出進度條 UI（目前使用 Loading 訊息百分比文字）

### Phase 3 — 進階功能（待做）

11. 匯出模板儲存/載入（常用的表+欄位組合可儲存為模板）
12. 資料脫敏選項（匯出時將敏感欄位遮罩/雜湊處理）
13. 匯出操作日誌記錄
14. 排程匯出（定期自動產生匯出檔並通知）

---

## 相關文件

- [Readme_資料庫匯入設計總綱.md](Readme_資料庫匯入設計總綱.md)（匯入工具設計）
- [README_個人化設定總綱.md](../個人化設定/README_個人化設定總綱.md)（SuperAdminDebugTab 所在模組）
- [README_個人化設定_UI框架.md](../個人化設定/README_個人化設定_UI框架.md)（Tab 架構與 Modal 開啟方式）
