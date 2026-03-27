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
| §三 | Level 1：自訂欄位 | 不同廠商需要記錄不同資料 | PBC 的資料可組合性 |
| §四 | Level 2：業務規則 | 不同廠商的審核/通知/驗證規則不同 | PBC 的邏輯可組合性 |
| §五 | Level 3：流程配置 | 不同廠商的單據流轉順序不同 | PBC 的流程可組合性 |
| §六 | Level 4：畫面配置 | 不同廠商想隱藏/必填不同欄位 | PBC 的介面可組合性 |

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
Level 1：欄位可配置    ← 最基本，投資報酬率最高
Level 2：規則可配置    ← 解決「不同廠商規則不同」的核心問題
Level 3：流程可配置    ← 真正的 EBC 差異化能力
Level 4：畫面可配置    ← 進階，讓使用者自訂介面
```

---

## 三、Level 1：欄位可配置（建議優先做）

### 問題
A 廠商需要在品項上記錄「耐熱溫度」，B 廠商需要記錄「保存期限」。
你不可能為每個廠商加欄位，也不該在 Entity 上塞一堆用不到的欄位。

### 設計方案：自訂欄位系統（Custom Fields）

#### 1a. 欄位定義表

```csharp
/// <summary>
/// 自訂欄位定義 - 使用者可以為任何模組新增自訂欄位
/// </summary>
public class CustomFieldDefinition : BaseEntity
{
    /// <summary>目標模組（如 "Item", "Customer", "PurchaseOrder"）</summary>
    [Required, MaxLength(50)]
    public string TargetModule { get; set; } = string.Empty;

    /// <summary>欄位名稱（顯示用）</summary>
    [Required, MaxLength(100)]
    public string FieldName { get; set; } = string.Empty;

    /// <summary>欄位識別鍵（程式用，如 "heat_resistance"）</summary>
    [Required, MaxLength(50)]
    public string FieldKey { get; set; } = string.Empty;

    /// <summary>欄位類型</summary>
    public CustomFieldType FieldType { get; set; } = CustomFieldType.Text;

    /// <summary>是否必填</summary>
    public bool IsRequired { get; set; } = false;

    /// <summary>預設值</summary>
    [MaxLength(500)]
    public string? DefaultValue { get; set; }

    /// <summary>下拉選項（FieldType 為 Dropdown 時使用，JSON 格式）</summary>
    public string? OptionsJson { get; set; }

    /// <summary>排序順序</summary>
    public int SortOrder { get; set; }

    /// <summary>是否在列表頁顯示此欄位</summary>
    public bool ShowInList { get; set; } = false;

    /// <summary>是否啟用</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>欄位提示說明</summary>
    [MaxLength(200)]
    public string? HelpText { get; set; }
}

public enum CustomFieldType
{
    Text,           // 單行文字
    TextArea,       // 多行文字
    Number,         // 數字
    Decimal,        // 小數
    Date,           // 日期
    DateTime,       // 日期時間
    Boolean,        // 是/否
    Dropdown,       // 下拉選單（選項從 OptionsJson 讀取）
    MultiSelect     // 多選（選項從 OptionsJson 讀取）
}
```

#### 1b. 欄位值儲存表

```csharp
/// <summary>
/// 自訂欄位值 - EAV（Entity-Attribute-Value）模式儲存
/// </summary>
public class CustomFieldValue : BaseEntity
{
    /// <summary>對應的欄位定義</summary>
    public int CustomFieldDefinitionId { get; set; }
    public CustomFieldDefinition CustomFieldDefinition { get; set; } = null!;

    /// <summary>目標實體的 Id</summary>
    public int EntityId { get; set; }

    /// <summary>欄位值（統一用字串儲存，依 FieldType 轉型）</summary>
    [MaxLength(2000)]
    public string? Value { get; set; }
}
```

#### 1c. 使用情境

```
系統管理 → 自訂欄位設定
  ┌────────────────────────────────────────────┐
  │ 模組：[品項 ▾]                              │
  │                                             │
  │  欄位名稱        類型      必填  列表顯示    │
  │  ─────────────────────────────────────      │
  │  耐熱溫度(°C)    數字       ☐     ☑         │
  │  材質認證        下拉選單    ☑     ☑         │
  │  保存期限(天)    數字       ☐     ☐         │
  │  特殊備註        多行文字    ☐     ☐         │
  │                                    [新增]   │
  └────────────────────────────────────────────┘
```

#### 1d. 與現有架構的整合方式

```
GenericEditModalComponent
  └─ 原有固定欄位區塊
  └─ 【新增】CustomFieldSection 元件
       → 讀取該模組的 CustomFieldDefinition
       → 動態渲染對應的輸入元件
       → 儲存時寫入 CustomFieldValue

GenericIndexPageComponent
  └─ 原有固定欄位
  └─ 【新增】ShowInList = true 的自訂欄位動態加入表格
```

### 這能解決什麼

- A 工廠：品項加「耐熱溫度」「模具編號」
- B 貿易商：客戶加「信用評等備註」「年度採購預算」
- C 食品廠：品項加「保存期限」「過敏原標示」
- **不需要改程式碼，使用者自己在後台設定**

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

## 六、Level 4：畫面可配置

### 問題
A 廠商：客戶表單上「傳真」欄位根本沒人用，想隱藏
B 廠商：採購單上想把「備註」欄位設為必填

### 設計方案：欄位顯示設定（Field Display Configuration）

#### 4a. 欄位設定表

```csharp
/// <summary>
/// 欄位顯示設定 - 控制既有欄位在表單和列表中的顯示行為
/// </summary>
public class FieldDisplaySetting : BaseEntity
{
    /// <summary>目標模組</summary>
    [Required, MaxLength(50)]
    public string TargetModule { get; set; } = string.Empty;

    /// <summary>欄位名稱（對應 Entity Property Name）</summary>
    [Required, MaxLength(100)]
    public string FieldName { get; set; } = string.Empty;

    /// <summary>欄位顯示名稱（覆蓋預設 Display Name）</summary>
    [MaxLength(100)]
    public string? DisplayNameOverride { get; set; }

    /// <summary>是否在表單中顯示（預設 true）</summary>
    public bool ShowInForm { get; set; } = true;

    /// <summary>是否在列表中顯示（預設跟隨系統設定）</summary>
    public bool? ShowInList { get; set; }

    /// <summary>是否為必填（覆蓋預設驗證）</summary>
    public bool? IsRequiredOverride { get; set; }

    /// <summary>表單中的排序順序</summary>
    public int? SortOrder { get; set; }
}
```

#### 4b. 使用情境

```
系統管理 → 欄位顯示設定
  ┌──────────────────────────────────────────────────┐
  │ 模組：[客戶 ▾]                                    │
  │                                                   │
  │  欄位名稱      顯示名稱(自訂)  表單顯示  必填覆蓋  │
  │  ──────────────────────────────────────────       │
  │  CompanyName    公司名稱        ☑        ☑ 必填   │
  │  ContactPerson  聯絡窗口        ☑        — 預設   │
  │  Fax            傳真            ☐ 隱藏   — 預設   │  ← 不需要的欄位
  │  Email          電子郵件        ☑        ☑ 必填   │  ← 改為必填
  │  CreditLimit    信用額度        ☑        — 預設   │
  │  Website        官方網站        ☐ 隱藏   — 預設   │
  │                                                   │
  │                                [恢復預設] [儲存]   │
  └──────────────────────────────────────────────────┘
```

---

## 七、實作優先順序與現有架構的對應

### 建議實作順序

```
Phase 1（近期）
  ├─ Level 1：自訂欄位        ← 立即解決「每家都不一樣」的問題
  └─ Level 4：欄位顯示設定    ← 搭配一起做，讓表單更靈活

Phase 2（中期）
  └─ Level 2：業務規則引擎    ← 讓審核/通知/驗證可配置

Phase 3（長期）
  └─ Level 3：流程可配置      ← 真正的 EBC 能力
```

### 與現有架構的對應

| 現有元件 | 擴展方式 | 影響範圍 |
|---------|---------|---------|
| `SystemParameter` | 保持不變，作為「全域基礎設定」 | 無 |
| `ApprovalConfigHelper` | 加入規則引擎判斷，作為第二層邏輯 | 小改 |
| `GenericEditModalComponent` | 加入 CustomFieldSection 渲染區 | 新增區塊 |
| `GenericIndexPageComponent` | 支援動態欄位顯示 | 小改 |
| `CodeSetting` | 保持不變 | 無 |
| `CompanyModule` | 保持不變 | 無 |
| 各模組 Service | 加入 RuleEngine 呼叫點 | 小改 |

### 新增的資料表

| Entity | 用途 | Phase |
|--------|------|-------|
| `CustomFieldDefinition` | 自訂欄位定義 | Phase 1 |
| `CustomFieldValue` | 自訂欄位值儲存 | Phase 1 |
| `FieldDisplaySetting` | 既有欄位顯示控制 | Phase 1 |
| `BusinessRule` | 業務規則定義 | Phase 2 |
| `DocumentFlowStep` | 單據流程定義 | Phase 3 |

### 新增的 Service

| Service | 用途 | Phase |
|---------|------|-------|
| `ICustomFieldService` | 自訂欄位 CRUD + 動態渲染資料 | Phase 1 |
| `IFieldDisplayService` | 欄位顯示設定管理 | Phase 1 |
| `IRuleEngineService` | 規則評估引擎 | Phase 2 |
| `IDocumentFlowService` | 流程設定管理 | Phase 3 |

### 新增的 UI 元件

| 元件 | 用途 | Phase |
|------|------|-------|
| `CustomFieldSection.razor` | EditModal 內動態渲染自訂欄位 | Phase 1 |
| `CustomFieldSettingPage` | 系統管理 → 自訂欄位設定 | Phase 1 |
| `FieldDisplaySettingPage` | 系統管理 → 欄位顯示設定 | Phase 1 |
| `BusinessRuleSettingPage` | 系統管理 → 業務規則設定 | Phase 2 |
| `DocumentFlowSettingPage` | 系統管理 → 單據流程設定 | Phase 3 |

---

## 八、配置層級架構總覽

完成後，ERPCore2 的可配置化架構會是這樣：

```
┌─────────────────────────────────────────────────────────┐
│                    使用者可配置層                         │
│                                                         │
│  ┌───────────────┐  ┌───────────────┐  ┌─────────────┐ │
│  │ 模組開關       │  │ 欄位設定       │  │ 業務規則    │ │
│  │ CompanyModule  │  │ CustomField   │  │ BusinessRule│ │
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
