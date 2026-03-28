# EBC 可配置化設計

> 專案：ERPCore2
> 建立日期：2026-03-27
> 核心問題：不同廠商的業務流程不同，無法用統一寫死的方式滿足所有人
> 解決方向：讓使用者自己定義參數和流程，系統根據設定動態運作

### 相關文件

| 文件 | 定位 | 說明 |
|------|------|------|
| [Readme_ERP+EBC規劃.md](../Readme_ERP+EBC規劃.md) | **策略層（Why + What）** | ERP 與 EBC 的差異、Gartner 五大平台框架、七階段演進路線、業界案例。先讀此文件理解為什麼要做 EBC |
| **本文件** | **實作層（How）** | 四個可配置化層次的具體設計方案，對應演進路線的「階段二：可配置化核心」 |

**本文件在演進路線中的位置**：

```
階段一：穩固 ERP 核心 ← 已完成大部分
階段二：可配置化核心  ← ★ 本文件的設計範圍 ★
階段三：API 化與模組解耦
階段四：客戶體驗平台（CX）
階段五：夥伴生態平台（PX）
階段六：數據與智慧分析（Data & AI）
階段七：員工賦能與物聯網
```

### 本文件涵蓋的四個層次

| 章節 | 層次 | 核心問題 | 對應 EBC 概念 |
|------|------|----------|--------------|
| §三 | Level 1：自訂資料表 ✅ | 不同廠商需要記錄不同資料 | PBC 的資料可組合性 |
| §四 | Level 2：業務規則 | 不同廠商的審核/通知/驗證規則不同 | PBC 的邏輯可組合性 |
| §五 | Level 3：流程配置 | 不同廠商的單據流轉順序不同 | PBC 的流程可組合性 |
| §六 | Level 4：畫面配置 ✅ | 不同廠商想隱藏/必填不同欄位 | PBC 的介面可組合性 |

---

## 一、現狀分析：目前已經可配置的東西

你目前已經有三層配置能力，這是很好的基礎：

```
第一層：模組開關（CompanyModule）
  → 「要不要用這個功能？」

第二層：行為參數（SystemParameter）
  → 「這個功能的行為方式是什麼？」（審核模式、稅率、薪資規則）

第三層：格式設定（CodeSetting）
  → 「單據編號長什麼樣子？」
```

### 現狀的問題

| 現在能做的 | 使用者想做但做不到的 |
|---|---|
| 採購單要不要人工審核 | 採購單超過 10 萬才要主管審核 |
| 模組開或關 | 某些欄位對我沒用，想隱藏 |
| 單據編號格式 | 我想在表單上多加一個自訂欄位 |
| 全公司統一稅率 | 不同客戶適用不同稅率規則 |
| — | 儲存後自動通知相關人員 |
| — | 採購單審核後自動產生進貨單 |

**一句話：你現在的配置是「開關型」的，使用者需要的是「規則型」的。**

---

## 二、EBC 可配置化的四個層次

從簡單到複雜，建議按順序逐步實作：

```
Level 1：自訂資料表    ← ✅ 已完成，使用者可建立自訂資料表與欄位（EAV 模式）
Level 2：規則可配置    ← 解決「不同廠商規則不同」的核心問題
Level 3：流程可配置    ← 真正的 EBC 差異化能力
Level 4：畫面可配置    ← ✅ 已完成，使用者可自訂欄位顯示/隱藏/必填/名稱
```

---

## 三、Level 1：自訂資料表 ✅ 已實作

> **實作狀態**：已完成並整合至系統（2026-03-28）。使用者可透過「系統管理 → 自訂資料表」建立自訂資料表及欄位定義。

### 問題
不同廠商需要記錄不同的自訂資料。傳統做法是為每個需求修改 Entity、建立 Migration、修改 UI，成本高且無法快速回應。

### 實作成果：自訂資料表系統（Custom Tables）

採用 **EAV（Entity-Attribute-Value）** 模式，使用者可自行建立資料表並定義欄位，所有資料值統一以字串儲存，依欄位類型在顯示時轉換。

#### 1a. 資料庫架構（4 張表）

```
CustomTableDefinitions        CustomFieldDefinitions
┌──────────────────┐         ┌──────────────────────┐
│ Id (PK)          │────┐    │ Id (PK)              │
│ TableName        │    │    │ CustomTableDefinitionId│◄─┐
│ Description      │    ├───▶│ FieldName             │   │
│ IconClass        │    │    │ DisplayName           │   │
│ CodePrefix       │    │    │ FieldType (enum)      │   │
│ (BaseEntity)     │    │    │ IsRequired            │   │
└──────────────────┘    │    │ SortOrder             │   │
                        │    │ ShowInList / ShowInForm│   │
                        │    │ Options (JSON)        │   │
                        │    │ DefaultValue          │   │
                        │    │ Placeholder           │   │
                        │    │ (BaseEntity)          │   │
                        │    └──────────────────────┘   │
                        │                               │
CustomTableRows          │    CustomFieldValues          │
┌──────────────────┐    │    ┌──────────────────────┐   │
│ Id (PK)          │────┘    │ Id (PK)              │   │
│ CustomTableDefId │         │ CustomTableRowId (FK) │───┘
│ (BaseEntity)     │────────▶│ CustomFieldDefId (FK) │
└──────────────────┘         │ Value (string)        │
                             └──────────────────────┘
```

**Cascade 策略**：
- `CustomTableDefinition` → `FieldDefinitions`：Cascade Delete
- `CustomTableDefinition` → `Rows`：Cascade Delete
- `CustomTableRow` → `FieldValues`：Cascade Delete
- `CustomFieldValue` → `CustomFieldDefinition`：**NoAction**（避免 SQL Server 多重 cascade 路徑，由 Service 層手動處理）

**唯一索引**：
- `CustomFieldDefinition`：`(CustomTableDefinitionId, FieldName)` — 同表內欄位名稱不重複
- `CustomFieldValue`：`(CustomTableRowId, CustomFieldDefinitionId)` — 同列內每欄位只有一個值

#### 1b. Entity 定義（已實作）

**檔案**：`Data/Entities/CustomTables/` 目錄下 4 個檔案

| Entity | 繼承 | 說明 |
|--------|------|------|
| `CustomTableDefinition` | `BaseEntity` | 表定義（TableName, Description, IconClass, CodePrefix） |
| `CustomFieldDefinition` | `BaseEntity` | 欄位定義（FieldName, DisplayName, FieldType, IsRequired, Options...） |
| `CustomTableRow` | `BaseEntity` | 資料列（繼承 BaseEntity 獲得 Id, Code, Status, 審計欄位, 草稿支援） |
| `CustomFieldValue` | —（僅有 Id） | 欄位值（不繼承 BaseEntity，純粹的值儲存容器） |

**欄位類型**（`Models/Enums/CustomFieldType.cs`）：

```csharp
public enum CustomFieldType
{
    Text,       // 單行文字
    TextArea,   // 多行文字
    Number,     // 數字（decimal 驗證）
    Date,       // 日期
    DateTime,   // 日期時間
    Boolean,    // 是/否
    Select      // 下拉選單（選項從 Options JSON 讀取）
}
```

#### 1c. Service 層（已實作）

**兩個 Service 分工**：

| Service | 檔案 | 職責 |
|---------|------|------|
| `ICustomTableDefinitionService` | `Services/CustomTables/` | 管理表定義 + 欄位定義（父子關係） |
| `ICustomTableRowService` | `Services/CustomTables/` | 管理資料列 + 欄位值（EAV 模式） |

**CustomTableDefinitionService 關鍵方法**：

| 方法 | 用途 |
|------|------|
| `GetByIdWithFieldsAsync(id)` | 取得表定義 + 欄位定義（按 SortOrder 排序） |
| `IsTableNameExistsAsync(name, excludeId?)` | TableName 唯一性檢查 |
| `AddFieldDefinitionAsync(field)` | 新增欄位定義 |
| `UpdateFieldDefinitionAsync(field)` | 更新欄位定義 |
| `DeleteFieldDefinitionAsync(fieldId)` | 刪除欄位定義（**transaction 內先刪 CustomFieldValues 再刪定義**，因 NoAction FK） |
| `ReorderFieldsAsync(tableId, orderedIds)` | 重新排序欄位 |

**CustomTableRowService 關鍵方法**：

| 方法 | 用途 |
|------|------|
| `GetRowsByTableIdAsync(tableId)` | 取得某表所有資料列 |
| `CreateRowWithValuesAsync(row, values)` | Transaction 內建列 + 存值 |
| `UpdateRowWithValuesAsync(row, values)` | Diff update（比對 CustomFieldDefinitionId） |
| `ValidateFieldValuesAsync(tableId, values)` | 型別驗證 + 必填檢查 |
| `GetPagedByTableIdAsync(tableId, page, size, search?)` | 分頁查詢 |

#### 1d. UI 元件（已實作）

**檔案**：`Components/Pages/Systems/CustomTables/CustomTableManagementModal.razor`

使用 `BaseModalComponent` 呈現，透過 Action+Modal 模式開啟（非路由）。

```
系統管理 → 自訂資料表
  ┌──────────────────────────────────────────────────┐
  │ 列表模式                                          │
  │                                                   │
  │   表名稱          說明              操作           │
  │   ──────────────────────────────────────          │
  │   設備維護紀錄    廠區設備維護記錄     [編輯] [刪除] │
  │   客戶分類標籤    客戶自訂分類         [編輯] [刪除] │
  │                                                   │
  │                                        [新增資料表] │
  └──────────────────────────────────────────────────┘

  ┌──────────────────────────────────────────────────┐
  │ 編輯模式                                          │
  │                                                   │
  │  資料表名稱：[設備維護紀錄    ]                     │
  │  說明：      [廠區設備維護記錄]                     │
  │  圖示：      [bi bi-wrench    ]                   │
  │  編號前綴：  [EQ              ]                   │
  │                                                   │
  │  欄位定義：                                        │
  │   顯示名稱    欄位名稱    類型    必填  操作        │
  │   ──────────────────────────────────────          │
  │   設備名稱    DeviceName  文字     ☑   [↑][↓][✕]  │
  │   維護日期    MaintDate   日期     ☑   [↑][↓][✕]  │
  │   維護人員    Maintainer  文字     ☐   [↑][↓][✕]  │
  │   費用        Cost        數字     ☐   [↑][↓][✕]  │
  │                                        [新增欄位]  │
  │                                                   │
  │                           [取消] [儲存]            │
  └──────────────────────────────────────────────────┘
```

#### 1e. 導覽與權限（已實作）

- **導覽**：`Data/Navigation/NavigationConfig.cs` 使用 `NavigationActionHelper.CreateActionItem`，actionId = `OpenCustomTableManagement`
- **權限**：`System.CustomTable`（PermissionLevel.Sensitive），定義於 `Models/PermissionRegistry.cs`
- **MainLayout**：註冊 action handler，開啟 `CustomTableManagementModal`

#### 1f. 實作檔案清單

| 檔案 | 類型 | 說明 |
|------|------|------|
| `Data/Entities/CustomTables/CustomTableDefinition.cs` | Entity | 自訂資料表定義 |
| `Data/Entities/CustomTables/CustomFieldDefinition.cs` | Entity | 自訂欄位定義 |
| `Data/Entities/CustomTables/CustomTableRow.cs` | Entity | 資料列（繼承 BaseEntity） |
| `Data/Entities/CustomTables/CustomFieldValue.cs` | Entity | 欄位值（EAV，不繼承 BaseEntity） |
| `Models/Enums/CustomFieldType.cs` | Enum | 欄位類型定義 |
| `Services/CustomTables/ICustomTableDefinitionService.cs` | Interface | 表定義服務介面 |
| `Services/CustomTables/CustomTableDefinitionService.cs` | Service | 表定義服務實作 |
| `Services/CustomTables/ICustomTableRowService.cs` | Interface | 資料列服務介面 |
| `Services/CustomTables/CustomTableRowService.cs` | Service | 資料列服務實作 |
| `Components/Pages/Systems/CustomTables/CustomTableManagementModal.razor` | UI | 管理介面 |
| `Data/Context/AppDbContext.cs` | DB（修改） | 新增 4 個 DbSet + 索引 + FK 設定 |
| `Data/ServiceRegistration.cs` | DI（修改） | 註冊 2 個 Service |
| `Data/Navigation/NavigationConfig.cs` | Nav（修改） | 新增導覽項目 |
| `Components/Layout/MainLayout.razor` | Layout（修改） | 新增 action handler + modal |
| `Models/PermissionRegistry.cs` | Auth（修改） | 新增 System.CustomTable 權限 |
| `Migrations/AddCustomTables` | Migration | EF Core 資料庫遷移 |

#### 1g. 後續擴展方向

Level 1 目前為獨立的自訂資料表系統。未來可擴展：
- **從屬表支援**：自訂資料表可掛在主表下，形成主從關係
- **與既有模組整合**：將自訂欄位掛到現有 Entity（如品項、客戶），類似 TargetModule 模式
- **資料列管理 UI**：在 CustomTableManagementModal 內嵌資料列的 CRUD 介面

### 這能解決什麼

- 使用者可自行建立任意結構的資料表，**不需要改程式碼、不需要跑 Migration**
- 每個廠商可以有自己獨特的資料表（設備紀錄、自訂分類、特殊流水帳...）
- 資料列繼承 BaseEntity，享有完整的審計追蹤、狀態管理、草稿支援

---

## 四、Level 2：規則可配置

### 問題
A 廠商：採購單不用審核，直接存就好
B 廠商：採購單超過 5 萬要主管審核
C 廠商：採購單一律要老闆審核，而且審核後要自動通知倉管

目前的 `SystemParameter.PurchaseOrderManualApproval = true/false` 只能選「要或不要」，無法設定條件。

### 設計方案：業務規則引擎（Business Rules）

#### 2a. 規則定義表

```csharp
/// <summary>
/// 業務規則定義 - 使用者可配置的條件式規則
/// </summary>
public class BusinessRule : BaseEntity
{
    /// <summary>規則名稱（顯示用）</summary>
    [Required, MaxLength(100)]
    public string RuleName { get; set; } = string.Empty;

    /// <summary>目標模組（如 "PurchaseOrder"）</summary>
    [Required, MaxLength(50)]
    public string TargetModule { get; set; } = string.Empty;

    /// <summary>觸發時機</summary>
    public RuleTrigger Trigger { get; set; }

    /// <summary>是否啟用</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>優先順序（數字小的先執行）</summary>
    public int Priority { get; set; } = 100;

    /// <summary>規則條件（JSON 格式）</summary>
    public string ConditionsJson { get; set; } = "[]";

    /// <summary>規則動作（JSON 格式）</summary>
    public string ActionsJson { get; set; } = "[]";
}

/// <summary>規則觸發時機</summary>
public enum RuleTrigger
{
    BeforeSave,         // 儲存前（可用於驗證、攔截）
    AfterSave,          // 儲存後（可用於通知、產生關聯單據）
    BeforeApproval,     // 審核前
    AfterApproval,      // 審核後（可用於自動轉單）
    OnStatusChange,     // 狀態變更時
    OnCreate,           // 新增時
    OnDelete            // 刪除時
}
```

#### 2b. 條件與動作的 JSON 結構

條件（Conditions）— 用簡單的比較運算子：

```json
[
    {
        "field": "TotalAmount",
        "operator": "GreaterThan",
        "value": "50000",
        "logic": "And"
    },
    {
        "field": "SupplierId",
        "operator": "NotEquals",
        "value": "0",
        "logic": "And"
    }
]
```

動作（Actions）— 系統預定義的動作清單：

```json
[
    {
        "type": "RequireApproval",
        "params": { "approverRole": "Manager" }
    },
    {
        "type": "SendNotification",
        "params": {
            "to": "WarehouseStaff",
            "template": "採購單 {Code} 已核准，金額 {TotalAmount}，請準備收貨"
        }
    },
    {
        "type": "SetFieldValue",
        "params": { "field": "Priority", "value": "High" }
    }
]
```

#### 2c. 預定義動作類型

| 動作類型 | 說明 | 範例 |
|----------|------|------|
| `RequireApproval` | 要求指定角色審核 | 金額 > 5 萬 → 需主管審核 |
| `SendNotification` | 發送通知 | 審核後 → 通知倉管 |
| `SetFieldValue` | 自動填入欄位值 | 客戶類型 = 政府 → 稅率 = 0 |
| `CreateRelatedDocument` | 自動產生關聯單據 | 採購單審核 → 自動建立進貨單草稿 |
| `ValidateField` | 自訂驗證規則 | 數量 > 庫存量 → 顯示警告 |
| `BlockAction` | 阻止操作 | 客戶餘額 > 信用額度 → 不可出貨 |
| `UpdateRelatedEntity` | 更新關聯實體 | 出貨完成 → 更新訂單狀態 |

#### 2d. 使用情境

```
系統管理 → 業務規則設定
  ┌──────────────────────────────────────────────────┐
  │ 模組：[採購訂單 ▾]                                │
  │                                                   │
  │ 規則 1：大額採購需主管審核                    [啟用] │
  │   觸發：儲存前                                     │
  │   條件：總金額 > 50,000                            │
  │   動作：要求「主管」角色審核                        │
  │                                                   │
  │ 規則 2：審核後通知倉管                        [啟用] │
  │   觸發：審核後                                     │
  │   條件：（無條件，一律觸發）                        │
  │   動作：發送通知給「倉管」角色                      │
  │         內容：「採購單 {Code} 已核准，請準備收貨」   │
  │                                                   │
  │ 規則 3：VIP 客戶自動優先處理                  [停用] │
  │   ...                                             │
  │                                          [新增規則] │
  └──────────────────────────────────────────────────┘
```

#### 2e. 與現有 ApprovalConfigHelper 的共存

**不需要砍掉現有的 SystemParameter 審核設定**，而是疊加：

```
判斷是否需要審核：
  1. 先看 SystemParameter.PurchaseOrderManualApproval
     → false：系統自動審核（現有行為不變）
     → true：進入規則引擎

  2. 規則引擎檢查 BusinessRule（Trigger = BeforeSave）
     → 有 RequireApproval 規則？→ 檢查條件
       → 條件符合 → 需要指定角色審核
       → 條件不符 → 一般審核流程
     → 沒有規則 → 走原本 ApprovalConfigHelper 邏輯
```

---

## 五、Level 3：流程可配置

### 問題
A 廠商：報價 → 訂單 → 出貨（標準流程）
B 廠商：訂單 → 直接出貨（不需要報價）
C 廠商：報價 → 訂單 → 審核 → 備料 → 排程 → 出貨（完整流程）

目前的轉單邏輯（如「採購單轉進貨單」）是寫死在各個 Service 裡的。

### 設計方案：單據流程設定（Document Flow Configuration）

#### 3a. 流程定義表

```csharp
/// <summary>
/// 單據流程定義 - 定義單據之間的轉換關係
/// </summary>
public class DocumentFlowStep : BaseEntity
{
    /// <summary>流程名稱</summary>
    [Required, MaxLength(100)]
    public string FlowName { get; set; } = string.Empty;

    /// <summary>來源模組（如 "Quotation"）</summary>
    [Required, MaxLength(50)]
    public string SourceModule { get; set; } = string.Empty;

    /// <summary>目標模組（如 "SalesOrder"）</summary>
    [Required, MaxLength(50)]
    public string TargetModule { get; set; } = string.Empty;

    /// <summary>步驟順序</summary>
    public int StepOrder { get; set; }

    /// <summary>是否為必要步驟（false = 可跳過）</summary>
    public bool IsRequired { get; set; } = true;

    /// <summary>轉換後是否自動建立目標單據草稿</summary>
    public bool AutoCreateDraft { get; set; } = false;

    /// <summary>是否啟用</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>說明</summary>
    [MaxLength(200)]
    public string? Description { get; set; }
}
```

#### 3b. 預設流程 vs 自訂流程

```
銷售流程（預設）：
  報價(可跳) → 訂單(必要) → 出貨(必要) → 退貨(可跳)

銷售流程（某些廠商自訂）：
  訂單(必要) → 出貨(必要)              ← 跳過報價和退貨

採購流程（預設）：
  採購單(必要) → 進貨(必要) → 退貨(可跳)

生產流程（完整版）：
  訂單 → 排程(必要) → 領料(必要) → 完工(必要) → 出貨

生產流程（簡化版）：
  訂單 → 出貨                         ← 不走生產排程
```

#### 3c. 使用情境

```
系統管理 → 單據流程設定
  ┌────────────────────────────────────────────────┐
  │ 銷售流程：                                      │
  │                                                 │
  │   [報價] ──→ [訂單] ──→ [出貨] ──→ [退貨]       │
  │    可跳過     必要      必要      可跳過         │
  │    ☑啟用     ☑啟用     ☑啟用     ☑啟用          │
  │                                                 │
  │ 採購流程：                                      │
  │                                                 │
  │   [採購單] ──→ [進貨] ──→ [退貨]                │
  │    必要        必要      可跳過                  │
  │    ☑啟用      ☑啟用     ☑啟用                   │
  │                                                 │
  │                               [恢復預設] [儲存] │
  └────────────────────────────────────────────────┘
```

---

## 六、Level 4：畫面可配置 ✅ 已實作

> **實作狀態**：已完成並整合至系統，所有使用 `GenericEditModalComponent` 的模組自動具備此功能。

### 問題
A 廠商：客戶表單上「傳真」欄位根本沒人用，想隱藏
B 廠商：採購單上想把「備註」欄位設為必填

### 實作成果：欄位顯示設定（Field Display Configuration）

#### 4a. 欄位設定表（已實作）

**檔案**：`Data/Entities/Systems/FieldDisplaySetting.cs`

```csharp
public class FieldDisplaySetting : BaseEntity
{
    [Required, MaxLength(50)]
    public string TargetModule { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string FieldName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? DisplayNameOverride { get; set; }

    public bool? ShowInForm { get; set; }       // null = 使用程式碼預設值
    public bool? ShowInList { get; set; }
    public bool? IsRequiredOverride { get; set; } // null = 使用預設, true = 必填, false = 選填

    public int? SortOrder { get; set; }

    [MaxLength(500)]
    public string? HelpTextOverride { get; set; }
}
```

**設計特色**：所有覆蓋欄位皆為 `nullable`，`null` 代表「使用程式碼預設值」。當使用者將所有設定恢復為預設時，資料庫列會自動刪除（`IsDefaultSetting()` 檢查），避免累積無用資料。

**資料庫索引**：`(TargetModule, FieldName)` 複合唯一索引。

#### 4b. 服務層（已實作）

**檔案**：`Services/Systems/FieldDisplaySettingService.cs`（實作 `IFieldDisplaySettingService`）

| 方法 | 用途 |
|------|------|
| `GetByModuleAsync(module)` | 讀取模組設定（30 分鐘 IMemoryCache） |
| `SaveModuleSettingsAsync(module, settings, updatedBy)` | 批次儲存（新增/更新/刪除一次完成） |
| `ResetModuleSettingsAsync(module)` | 恢復預設（刪除全部覆蓋設定） |
| `ClearCache(module?)` | 手動清除快取 |

**快取策略**：`IDbContextFactory` + `IMemoryCache`，每模組獨立快取 30 分鐘，儲存/重設時自動清除。

#### 4c. UI 元件（已實作）

**檔案**：`Components/Shared/Modal/FieldSettingsPanel.razor` + `.razor.css`

入口位置：`GenericEditModalComponent` 標題列的齒輪按鈕（⚙），受 `System.FieldSettings` 權限控制。

面板以右側滑入方式開啟，包含：
- 欄位列表表格（欄位名稱 / 顯示開關 / 必填設定 / 自訂名稱）
- 恢復預設、取消、儲存按鈕（使用 `GenericButtonComponent`）
- 字型遵循 `typography.css` 變數（標題 `--fs-md`，內文 `1rem`）

```
  GenericEditModalComponent 標題列
  ┌──────────────────────────────────────────┐
  │  [模組名稱]              [⚙] [✕]        │ ← 齒輪按鈕開啟欄位設定
  └──────────────────────────────────────────┘

  FieldSettingsPanel（右側滑入面板）
  ┌──────────────────────────────────────────┐
  │ ⚙ 欄位設定 — 客戶                        │
  │ ℹ 此設定將套用到所有使用者                 │
  │                                          │
  │  欄位        顯示    必填    自訂名稱      │
  │  ──────────────────────────────────      │
  │  CompanyName  ☑      預設    [        ]  │
  │  Fax          ☐      —      [        ]  │ ← 隱藏
  │  Email        ☑      必填    [        ]  │ ← 改為必填
  │                                          │
  │  [恢復預設]              [取消] [儲存設定] │
  └──────────────────────────────────────────┘
```

#### 4d. 覆蓋機制（已實作）

**檔案**：`Components/Shared/Modal/GenericEditModalComponent.AutoComplete.cs` → `ApplyFieldDisplaySettings()`

在 `GetProcessedFormFields()` 的最後階段套用覆蓋：

| 資料庫欄位 | 覆蓋目標 | 規則 |
|-----------|---------|------|
| `ShowInForm` | `field.IsVisible` | `null` = 不覆蓋, `false` = 隱藏 |
| `IsRequiredOverride` | `field.IsRequired` | `null` = 不覆蓋, `true`/`false` = 覆蓋 |
| `DisplayNameOverride` | `field.Label` | 空字串 = 不覆蓋 |
| `HelpTextOverride` | `field.HelpText` | 空字串 = 不覆蓋 |

#### 4e. 權限控制（已實作）

**檔案**：`Models/PermissionRegistry.cs`

```
System.FieldSettings — 欄位顯示設定（PermissionLevel.Sensitive）
  → 管理各模組欄位的顯示、隱藏、必填與名稱覆蓋（全公司適用）
```

齒輪按鈕以 `PermissionCheck` 包裹，無權限者不可見。

#### 4f. 欄位保護等級（已實作）

**檔案**：`Components/Shared/UI/Form/FormFieldDefinition.cs` → `FieldProtectionLevel` enum

為避免使用者在設定面板中將系統必要欄位設為選填或隱藏（導致 Service 層 ValidateAsync 報錯的困惑體驗），
每個欄位都有一個保護等級，控制設定面板中的可操作範圍：

| 等級 | 說明 | 可隱藏？ | 可改必填？ | 可改名稱？ | 範例 |
|------|------|---------|----------|----------|------|
| `SystemRequired` | 系統必要，沒有它系統會壞 | 不可 | 不可 | 不可 | Code、外鍵（SupplierId） |
| `BusinessRequired` | 業務必要，不同公司可能不同 | 不可 | 可覆蓋 | 可改 | Name、CompanyName、OrderDate |
| `Normal` | 一般欄位 | 可隱藏 | 可覆蓋 | 可改 | 備註、傳真、統編 |

**自動推斷**：`FormFieldConfigurationHelper.ApplyProtectionLevels()` 在 `BuildProcessedFormFields()` 中自動呼叫，根據慣例推斷：

```
PropertyName == "Code"              → SystemRequired
PropertyName 以 "Id" 結尾 + 必填    → SystemRequired
其餘必填欄位                         → BusinessRequired
其餘                                → Normal
```

個別模組仍可手動設定 `ProtectionLevel` 屬性覆蓋自動推斷結果。

**覆蓋套用防護**：`ApplyFieldDisplaySettings()` 在套用資料庫覆蓋值前檢查保護等級：
- `SystemRequired` → 完全跳過，不套用任何覆蓋
- `BusinessRequired` → 跳過顯示/隱藏覆蓋，允許必填與名稱覆蓋
- `Normal` → 全部允許

#### 4g. Service 層 EBC 驗證（已實作）

**檔案**：`Services/GenericManagementService/GenericManagementService.cs`

為確保 Form 層和 Service 層的驗證一致，GenericManagementService 提供兩個 EBC 驗證機制：

**1. 自動驗證**：`CreateAsync` / `UpdateAsync` 在呼叫 `ValidateAsync` 前，自動執行 `ValidateEbcRequiredFieldsAsync()`。
如果 FieldDisplaySetting 將某欄位設為 `IsRequiredOverride = true`，Service 層也會檢查該欄位不為空。

**2. 覆蓋放寬**：`IsFieldRelaxedByEbcAsync(fieldName)` 供子類別在 ValidateAsync 中使用。
BusinessRequired 欄位被 EBC 設為選填時，子類別可跳過原本寫死的必填檢查。

```
驗證流程（CreateAsync / UpdateAsync）：
    ├─ ValidateEbcRequiredFieldsAsync(entity)  ← EBC 新增的必填欄位
    ├─ ValidateAsync(entity)                   ← 子類別的結構/業務驗證
    └─ 合併兩層錯誤 → 回傳
```

**子類別遷移範例**（DepartmentService）：

```csharp
// 結構驗證 — SystemRequired，永遠執行
if (string.IsNullOrWhiteSpace(entity.Code))
    errors.Add("部門編號不能為空");

// 業務必填 — BusinessRequired，可被 EBC 覆蓋為選填
if (!await IsFieldRelaxedByEbcAsync(nameof(entity.Name))
    && string.IsNullOrWhiteSpace(entity.Name))
    errors.Add("部門名稱不能為空");

// 唯一性 — 結構驗證，永遠執行
if (await IsDepartmentCodeExistsAsync(entity.Code, ...))
    errors.Add("部門編號已存在");
```

**遷移策略**：各 Service 可逐步遷移，未遷移的 Service 行為不受影響（`_fieldDisplaySettingService` 為 null 時所有 EBC 方法直接回傳空/false）。

#### 4h. 實作檔案清單

| 檔案 | 類型 | 說明 |
|------|------|------|
| `Data/Entities/Systems/FieldDisplaySetting.cs` | Entity | 欄位顯示設定資料模型 |
| `Services/Systems/IFieldDisplaySettingService.cs` | Interface | 服務介面 |
| `Services/Systems/FieldDisplaySettingService.cs` | Service | 服務實作（含快取） |
| `Components/Shared/Modal/FieldSettingsPanel.razor` | UI | 設定面板元件（含保護等級 UI 鎖定） |
| `Components/Shared/Modal/FieldSettingsPanel.razor.css` | CSS | 面板樣式（含系統必要欄位灰色行） |
| `Components/Shared/Modal/GenericEditModalComponent.razor` | UI（修改） | 新增齒輪按鈕 + 面板嵌入 |
| `Components/Shared/Modal/GenericEditModalComponent.AutoComplete.cs` | Logic（修改） | 覆蓋套用邏輯 + 保護等級檢查 + 自動推斷 |
| `Components/Shared/Modal/GenericEditModalComponent.Save.cs` | Logic（修改） | ValidateRequiredFormFields 改用 processed fields |
| `Components/Shared/Modal/GenericEditModalComponent.Lifecycle.cs` | Logic（修改） | 載入時讀取設定 |
| `Components/Shared/UI/Form/FormFieldDefinition.cs` | Model（修改） | 新增 ProtectionLevel 屬性 + enum |
| `Components/FieldConfiguration/FormFieldConfigurationHelper.cs` | Helper（修改） | 新增 ApplyProtectionLevels 自動推斷 |
| `Services/GenericManagementService/GenericManagementService.cs` | Base（修改） | 新增 EBC 驗證方法 + 自動驗證 |
| `Data/Context/AppDbContext.cs` | DB（修改） | 新增 DbSet + 唯一索引 |
| `Data/ServiceRegistration.cs` | DI（修改） | 註冊服務 |
| `Models/PermissionRegistry.cs` | Auth（修改） | 新增權限定義 |

---

## 七、實作優先順序與現有架構的對應

### 實作進度

```
Phase 1（近期）
  ├─ Level 1：自訂資料表      ← ✅ 已完成（2026-03-28）
  └─ Level 4：欄位顯示設定    ← ✅ 已完成（2026-03-27）

Phase 2（中期）
  └─ Level 2：業務規則引擎    ← 📋 待實作

Phase 3（長期）
  └─ Level 3：流程可配置      ← 📋 待實作
```

### 與現有架構的對應

| 現有元件 | 擴展方式 | 影響範圍 |
|---------|---------|---------|
| `SystemParameter` | 保持不變，作為「全域基礎設定」 | 無 |
| `ApprovalConfigHelper` | 加入規則引擎判斷，作為第二層邏輯 | 小改 |
| `GenericEditModalComponent` | 已整合欄位設定面板（Level 4） | 已完成 |
| `MainLayout` | 已整合 CustomTableManagementModal（Level 1） | 已完成 |
| `CodeSetting` | 保持不變 | 無 |
| `CompanyModule` | 保持不變 | 無 |
| 各模組 Service | 加入 RuleEngine 呼叫點 | 小改 |

### 新增的資料表

| Entity | 用途 | Phase | 狀態 |
|--------|------|-------|------|
| `CustomTableDefinition` | 自訂資料表定義 | Phase 1 | ✅ 已完成 |
| `CustomFieldDefinition` | 自訂欄位定義 | Phase 1 | ✅ 已完成 |
| `CustomTableRow` | 自訂資料列 | Phase 1 | ✅ 已完成 |
| `CustomFieldValue` | 自訂欄位值儲存（EAV） | Phase 1 | ✅ 已完成 |
| `FieldDisplaySetting` | 既有欄位顯示控制 | Phase 1 | ✅ 已完成 |
| `BusinessRule` | 業務規則定義 | Phase 2 | 📋 待實作 |
| `DocumentFlowStep` | 單據流程定義 | Phase 3 | 📋 待實作 |

### 新增的 Service

| Service | 用途 | Phase | 狀態 |
|---------|------|-------|------|
| `ICustomTableDefinitionService` | 表定義 + 欄位定義管理 | Phase 1 | ✅ 已完成 |
| `ICustomTableRowService` | 資料列 + 欄位值管理（EAV） | Phase 1 | ✅ 已完成 |
| `IFieldDisplaySettingService` | 欄位顯示設定管理 | Phase 1 | ✅ 已完成 |
| `IRuleEngineService` | 規則評估引擎 | Phase 2 | 📋 待實作 |
| `IDocumentFlowService` | 流程設定管理 | Phase 3 | 📋 待實作 |

### 新增的 UI 元件

| 元件 | 用途 | Phase | 狀態 |
|------|------|-------|------|
| `CustomTableManagementModal.razor` | 自訂資料表管理（表定義 + 欄位定義） | Phase 1 | ✅ 已完成 |
| `FieldSettingsPanel.razor` | EditModal 內嵌欄位設定面板 | Phase 1 | ✅ 已完成 |
| `BusinessRuleSettingPage` | 系統管理 → 業務規則設定 | Phase 2 | 📋 待實作 |
| `DocumentFlowSettingPage` | 系統管理 → 單據流程設定 | Phase 3 | 📋 待實作 |

---

## 八、配置層級架構總覽

完成後，ERPCore2 的可配置化架構會是這樣：

```
┌─────────────────────────────────────────────────────────┐
│                    使用者可配置層                         │
│                                                         │
│  ┌───────────────┐  ┌───────────────┐  ┌─────────────┐ │
│  │ 模組開關       │  │ 欄位設定 ✅    │  │ 業務規則    │ │
│  │ CompanyModule  │  │ CustomTable   │  │ BusinessRule│ │
│  │ 「要不要」     │  │ FieldDisplay  │  │ 「什麼條件  │ │
│  │               │  │ 「長什麼樣」   │  │  做什麼事」 │ │
│  └───────────────┘  └───────────────┘  └─────────────┘ │
│                                                         │
│  ┌───────────────┐  ┌───────────────┐  ┌─────────────┐ │
│  │ 行為參數       │  │ 編號格式       │  │ 流程設定    │ │
│  │ SystemParam   │  │ CodeSetting   │  │ DocFlow     │ │
│  │ 「全域規則」   │  │ 「編號樣式」   │  │ 「怎麼流轉」│ │
│  └───────────────┘  └───────────────┘  └─────────────┘ │
│                                                         │
│═════════════════════════════════════════════════════════│
│                                                         │
│                    系統核心層（不變）                     │
│                                                         │
│  ┌───────────────────────────────────────────────────┐  │
│  │ GenericManagementService / ApprovalConfigHelper   │  │
│  │ Entity Models / AppDbContext / Permission System  │  │
│  └───────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
```

---

## 九、這就是 EBC 的核心差異

| 傳統 ERP 做法 | EBC 做法 |
|---|---|
| 客戶要加欄位 → 工程師改 Entity、改 UI、改 Migration | 客戶自己在「自訂欄位」頁面新增 |
| 審核規則不同 → 寫 if-else 分支或客製版本 | 客戶自己在「業務規則」頁面設定條件 |
| 流程不同 → fork 程式碼改流程 | 客戶自己在「流程設定」頁面調整步驟 |
| 欄位顯示不同 → 做多套 UI | 客戶自己在「欄位設定」頁面隱藏/必填 |

**EBC 的本質不是做更多功能，而是讓使用者自己組合出他需要的功能。**

你不需要知道每個廠商的流程是什麼 — 你只需要提供工具讓他們自己定義。

---

## 十、下一步：完成可配置化後的演進方向

本文件涵蓋的是 EBC 演進路線中的「階段二：可配置化核心」。
完成後，後續階段請參考 [Readme_ERP+EBC規劃.md](../Readme_ERP+EBC規劃.md)：

| 後續階段 | 說明 | 前置條件 |
|----------|------|----------|
| 階段三：API 化與模組解耦 | 開放 RESTful API、定義模組邊界、事件驅動 | Level 2 業務規則完成（規則引擎可驅動事件） |
| 階段四：客戶體驗平台（CX） | 客戶入口、線上下單、即時通知 | 階段三 API 完成 |
| 階段五：夥伴生態平台（PX） | 供應商入口、協作、自助對帳 | 階段三 API 完成 |
| 階段六：數據與智慧分析 | 銷售預測、客戶流失預警、智慧採購 | 階段三事件驅動完成（資料可蒐集） |
| 階段七：員工賦能與物聯網 | 行動端、協作平台、IoT 框架 | 階段三 API 完成 |
