# 說明引導系統設計總綱

## 更新日期
2026-03-23

---

## 📋 概述

ERPCore2 說明引導系統（Feature Guide）採用**數據驅動的自動渲染模式**，為每個 EditModal 提供右側滑出式操作說明面板。系統支援左側彩色書籤導航、多語系（5 種語言）、深色模式，並透過統一的數據定義實現高度可擴展性。

**核心設計理念：**

- **數據驅動**：每個模組的說明內容以純 C# 數據類別定義，不寫任何 HTML
- **自動渲染**：`FeatureGuideRenderer` 根據章節類型（Description / Steps / FieldList / Tips / FAQ）自動選擇渲染模板
- **書籤自動產生**：左側彩色書籤標籤從 `GuideSection` 定義中自動讀取，點擊平滑滾動到對應章節
- **零 UI 程式碼**：新增模組說明只需建立數據定義檔 + 加入 resx key，不需撰寫 `.razor` 模板
- **i18n 支援**：所有文字皆使用 resx key，支援 zh-TW / en-US / ja-JP / zh-CN / fil 五種語言

---

## 🏗️ 系統架構圖

```
┌─────────────────────────────────────────────────────────────────────┐
│                    Layer 1: EditModal（消費端）                       │
│  SalesOrderEditModalComponent / PurchaseOrderEditModalComponent      │
│  - 只需一行：FeatureGuide="@SalesOrderGuide.Create()"              │
│  - 不需要寫任何 guide HTML                                          │
│  - 書籤從 Sections 自動產生                                         │
└───────────────────────────────┬─────────────────────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────────┐
│                    Layer 2: 通用元件層                                │
│  GenericEditModalComponent                                           │
│  - 接收 FeatureGuide 參數（FeatureGuideDefinition?）                │
│  - 透傳給 BaseModalComponent                                        │
│                                                                      │
│  BaseModalComponent                                                  │
│  - 底部浮動 ❓ 按鈕（有 FeatureGuide 時才顯示）                      │
│  - 右側滑出 Drawer（含透明遮罩、書籤列、標題列、內容區）            │
│  - 點擊遮罩或 ✕ 關閉 Drawer                                        │
│  - 書籤點擊 → JS scrollIntoView 平滑滾動                           │
└───────────────────────────────┬─────────────────────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────────┐
│                    Layer 3: 自動渲染組件                              │
│  FeatureGuideRenderer.razor                                          │
│  - 接收 FeatureGuideDefinition，遍歷 Sections                       │
│  - 根據 GuideSectionType 自動選擇渲染模板：                         │
│    ・Description → <p> 段落                                         │
│    ・Steps       → <ol> 有序列表（自動編號圓圈）                    │
│    ・FieldList   → <dl> 定義列表（dt=欄位名, dd=說明）              │
│    ・Tips        → guide-tip / guide-warning 提示框                 │
│    ・FAQ         → <dl> 定義列表（dt=問題, dd=答案）                │
│  - 注入 IStringLocalizer<SharedResource> 解析 resx key              │
└───────────────────────────────┬─────────────────────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────────┐
│                    Layer 4: 數據定義                                  │
│  Models/FeatureGuides/                                               │
│  ├── FeatureGuideDefinition.cs     核心模型 + 列舉                   │
│  └── GuideDefinitions/                                               │
│      ├── SalesOrderGuide.cs        銷售訂單說明（已實作）            │
│      ├── PurchaseOrderGuide.cs     採購單說明（待新增）              │
│      └── ...                                                         │
│                                                                      │
│  Resources/SharedResource.*.resx   多語系翻譯                        │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 📚 文件導覽

| 文件 | 說明 | 適用場景 |
|------|------|----------|
| **本文件** | 系統總綱、架構圖、快速指南 | 了解整體設計 |
| [README_數據模型與渲染.md](README_數據模型與渲染.md) | 數據模型詳細說明、渲染邏輯、CSS 樣式類別 | 客製化渲染或新增章節類型 |
| [README_新增模組說明指南.md](README_新增模組說明指南.md) | 逐步教學：如何為新模組添加說明 | 實作新的 Feature Guide |

---

## 📁 目錄結構

```
Models/FeatureGuides/
├── FeatureGuideDefinition.cs          # 核心數據模型
│   ├── FeatureGuideDefinition         #   說明定義（含 Sections 列表）
│   ├── GuideSection                   #   章節定義
│   ├── GuideItem                      #   內容項目
│   ├── GuideSectionType (enum)        #   Description / Steps / FieldList / Tips / FAQ
│   └── GuideItemStyle (enum)          #   Normal / Tip / Warning
└── GuideDefinitions/                  # 各模組的說明定義（每個模組一個 .cs）
    └── SalesOrderGuide.cs             #   銷售訂單（範例）

Components/Shared/
├── FeatureGuide/
│   └── FeatureGuideRenderer.razor     # 自動渲染組件（根據 Type 選擇模板）
└── Modal/
    ├── BaseModalComponent.razor       # Drawer 容器、書籤、遮罩、浮動按鈕
    ├── BaseModalComponent.razor.css   # Drawer/書籤/遮罩 CSS + 響應式 + 深色模式
    └── GenericEditModalComponent.razor # FeatureGuide 參數透傳

Resources/
├── SharedResource.resx                # zh-TW（預設）
├── SharedResource.en-US.resx          # English
├── SharedResource.ja-JP.resx          # 日本語
├── SharedResource.zh-CN.resx          # 简体中文
└── SharedResource.fil.resx            # Filipino
```

---

## 🔧 核心模型

### FeatureGuideDefinition（說明定義）

```csharp
public class FeatureGuideDefinition
{
    public List<GuideSection> Sections { get; set; } = new();
}
```

### GuideSection（章節定義）

| 屬性 | 類型 | 說明 | 範例 |
|------|------|------|------|
| `Id` | `string` | HTML anchor id（書籤滾動目標） | `"guide-so-overview"` |
| `TitleKey` | `string` | 章節標題的 resx key | `"Guide.Overview"` |
| `Icon` | `string` | Bootstrap Icons 類別 | `"bi-info-circle"` |
| `BookmarkLabel` | `string` | 左側書籤短標籤文字 | `"概述"` |
| `BookmarkColor` | `string` | 書籤顏色（HEX） | `"#3B82F6"` |
| `Type` | `GuideSectionType` | 章節類型（決定渲染方式） | `Description` |
| `Items` | `List<GuideItem>` | 章節內容項目 | — |

### GuideItem（內容項目）

| 建構方式 | 參數 | 適用場景 |
|----------|------|----------|
| `new GuideItem("textKey")` | 單一文字 | Description / Steps |
| `new GuideItem("labelKey", "textKey")` | 標籤 + 文字 | FieldList / FAQ |
| `new GuideItem("textKey", GuideItemStyle.Tip)` | 文字 + 樣式 | Tips（提示框） |
| `new GuideItem("textKey", GuideItemStyle.Warning)` | 文字 + 樣式 | Tips（警告框） |

### GuideSectionType（章節類型）

| 類型 | 渲染結果 | 適用場景 |
|------|----------|----------|
| `Description` | `<p>` 段落文字 | 功能概述、簡要說明 |
| `Steps` | `<ol>` 有序列表（自動編號圓圈） | 操作步驟、流程說明 |
| `FieldList` | `<dl>` 定義列表（dt=標題, dd=說明） | 欄位說明、參數解釋 |
| `Tips` | `guide-tip` / `guide-warning` 提示框 | 提示、注意事項、警告 |
| `FAQ` | `<dl>` 定義列表（dt=問題, dd=答案） | 常見問題 |

### 各書籤內容建議（以銷售訂單為範例）

每個 EditModal 建議提供以下書籤，可依模組需要增減：

#### 📘 概述（Description）— 必備
讓使用者 30 秒內了解「這個功能是做什麼的」。

| 應包含 | 不應包含 |
|--------|----------|
| 一句話說明此功能的用途 | 技術實作細節 |
| 本頁面有哪些分頁及用途 | 與使用者無關的架構資訊 |
| 與其他功能的關聯（如：訂單→出貨→退貨） | 過長的歷史沿革 |

#### 📗 步驟（Steps）— 必備
從零到完成的操作流程，**以新使用者視角撰寫**。

| 應包含 | 不應包含 |
|--------|----------|
| 每個步驟的動作（動詞開頭） | 重複欄位說明的內容 |
| 步驟間的因果關係 | 超過 8 個步驟（太長請拆分章節） |
| 最後一步提示「後續可做什麼」 | 例外情境處理（放到 FAQ） |

#### 📙 欄位（FieldList）— 必備
每個**使用者可見欄位**都應有說明，使用 `LabelKey` 引用 resx 中已存在的 `Field.*` key 作為標題。

| 應包含 | 不應包含 |
|--------|----------|
| 欄位用途與填寫方式 | 資料庫欄位名稱或型別 |
| 特殊格式說明（如「系統自動產生」） | 與開發有關的 Entity 對應 |
| 欄位間的連動關係（如「選客戶後帶入地址」） | 已在欄位 Placeholder 中說明的重複內容 |

> **重要**：FieldList 的 `LabelKey` 必須引用 resx 中已存在的 key（如 `Field.Customer`、`Field.SalesOrderDate`），
> 不可自行猜測（如 `Field.CustomerName`），否則會顯示原始 key 字串。

#### 📕 金額（FieldList）— 依模組需要
如果頁面有金額相關欄位，獨立為一個書籤讓使用者理解計算邏輯。

| 應包含 | 不應包含 |
|--------|----------|
| 每個金額欄位的計算公式 | 會計科目對應 |
| 欄位間的加減關係 | 小數位數等技術規格 |
| 含稅/未稅模式的差異 | — |

#### 📒 明細（Steps）— 有明細表格時
說明如何操作明細表格（新增、編輯、刪除品項等）。

| 應包含 | 不應包含 |
|--------|----------|
| 如何新增/刪除品項 | 每個明細欄位的詳細定義（放欄位書籤） |
| 特殊操作（如拖曳排序、智能搜尋） | 批次操作的技術限制 |
| 鎖定規則（如已出貨品項不可刪除） | — |

#### 📓 功能（FieldList）— 有特殊功能按鈕時
說明 Modal 上方的操作按鈕群組。

| 應包含 | 不應包含 |
|--------|----------|
| 每個按鈕的名稱與用途 | 按鈕的 CSS 樣式說明 |
| 按鈕的前置條件（如「需審核後才可用」） | 權限設定方式（放管理員手冊） |
| 按鈕狀態變化的含義（如黃色=庫存不足） | — |

#### 📕 審核（Steps）— 有審核流程時
說明審核的完整生命週期。

| 應包含 | 不應包含 |
|--------|----------|
| 審核狀態的意義（待審核/已核准/已駁回） | 審核的權限設定步驟 |
| 如何送審、核准、駁回 | API 層面的實作 |
| 審核後的限制（如無法修改） | — |
| 系統參數如何切換自動/手動審核 | — |

#### 💡 提示（Tips）— 建議提供
實用技巧 + 注意事項，用不同色塊區分重要性。

| Tip（藍色提示框） | Warning（黃色警告框） |
|-------------------|----------------------|
| 提高效率的操作技巧 | 操作後無法復原的動作 |
| 隱藏功能或快捷方式 | 資料一致性的注意事項 |
| 與其他功能的便捷連動 | 系統自動行為的提醒 |

#### ❓ FAQ（FAQ）— 建議提供
以使用者常見困惑為主，**問題用疑問句、答案用操作指引**。

| 好的 FAQ | 不好的 FAQ |
|----------|-----------|
| 「為什麼部分欄位無法編輯？」 | 「系統架構說明」 |
| 「如何查看出貨進度？」 | 「Entity 關聯圖」 |
| 「草稿和正式訂單有什麼不同？」 | 「版本更新紀錄」 |

---

## 🎨 UI 元件說明

### 浮動按鈕

位於 Modal 右下角，與審計資訊按鈕（🕐）並排。有 `FeatureGuide` 時才顯示 ❓ 按鈕。

### 左側書籤標籤

- 垂直文字排列，像實體書的側邊彩色標籤
- 每個書籤對應一個 `GuideSection`，顏色由 `BookmarkColor` 決定
- Hover 時往左突出，Active 狀態有陰影效果
- 點擊後平滑滾動到對應章節
- 手機版自動轉為頂部水平標籤列

### 右側 Drawer

- 寬度 340px（手機版 100%）
- 從右側滑出（CSS transform 動畫 0.3s）
- 點擊外部透明遮罩或 ✕ 按鈕關閉
- 內容區可滾動，支援自訂滾動條樣式

### 深色模式

- 使用 `[data-bs-theme=dark]` 選擇器（非 `@media prefers-color-scheme`）
- Drawer 陰影加深、提示框背景微調

---

## 📖 新增模組說明步驟（快速指南）

### 步驟 1：建立數據定義

在 `Models/FeatureGuides/GuideDefinitions/` 建立新檔案：

```csharp
// Models/FeatureGuides/GuideDefinitions/PurchaseOrderGuide.cs
namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

public static class PurchaseOrderGuide
{
    public static FeatureGuideDefinition Create() => new()
    {
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-po-overview",
                TitleKey = "Guide.Overview",           // 共用 key
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items = { new("Guide.PurchaseOrder.Description") }
            },
            new GuideSection
            {
                Id = "guide-po-steps",
                TitleKey = "Guide.Steps",              // 共用 key
                Icon = "bi-list-ol",
                BookmarkLabel = "步驟",
                BookmarkColor = "#10B981",
                Type = GuideSectionType.Steps,
                Items =
                {
                    new("Guide.PurchaseOrder.Step1"),
                    new("Guide.PurchaseOrder.Step2"),
                    new("Guide.PurchaseOrder.Step3"),
                }
            },
            // ... 更多章節
        }
    };
}
```

### 步驟 2：新增 resx key

在所有 5 個 `SharedResource.*.resx` 檔案中加入對應的翻譯 key：

```xml
<!-- SharedResource.resx (zh-TW) -->
<data name="Guide.PurchaseOrder.Description" xml:space="preserve">
  <value>採購單用於記錄向廠商採購的品項、數量及金額...</value>
</data>
<data name="Guide.PurchaseOrder.Step1" xml:space="preserve">
  <value>選擇廠商：從廠商欄位搜尋並選擇供應廠商</value>
</data>
```

> **共用 key 不需重複添加**：`Guide.Overview`、`Guide.Steps`、`Guide.FieldDescriptions`、`Guide.Tips` 等通用標題 key 已存在，直接使用即可。

### 步驟 3：在 EditModal 加入一行

```razor
@using ERPCore2.Models.FeatureGuides.GuideDefinitions

<GenericEditModalComponent ...
    FeatureGuide="@PurchaseOrderGuide.Create()">
</GenericEditModalComponent>
```

完成。不需要寫任何 HTML、CSS 或書籤定義。

---

## 🏷️ resx key 命名規則

### 共用 key（所有模組共用）

| Key | zh-TW | 用途 |
|-----|-------|------|
| `Guide.Overview` | 功能概述 | 章節標題 |
| `Guide.Steps` | 操作步驟 | 章節標題 |
| `Guide.FieldDescriptions` | 欄位說明 | 章節標題 |
| `Guide.Tips` | 提示與注意事項 | 章節標題 |
| `Modal.ShowFeatureGuide` | 關於此功能 | 按鈕 tooltip |
| `Modal.HideFeatureGuide` | 關閉功能說明 | 按鈕 tooltip |
| `Modal.FeatureGuideTitle` | 操作說明 | Drawer 標題 |

### 模組專用 key 命名規則

```
Guide.{ModuleName}.{ContentType}
Guide.{ModuleName}.{ContentType}{Number}
```

| 模式 | 範例 | 說明 |
|------|------|------|
| `Guide.{Module}.Description` | `Guide.SalesOrder.Description` | 功能概述文字 |
| `Guide.{Module}.Step{N}` | `Guide.SalesOrder.Step1` | 第 N 個操作步驟 |
| `Guide.{Module}.Field.{Name}` | `Guide.SalesOrder.Field.Customer` | 欄位說明 |
| `Guide.{Module}.Tip{N}` | `Guide.SalesOrder.Tip1` | 第 N 個提示 |
| `Guide.{Module}.Warning{N}` | `Guide.SalesOrder.Warning1` | 第 N 個警告 |
| `Guide.{Module}.Faq{N}Q` | `Guide.SalesOrder.Faq1Q` | 第 N 個 FAQ 問題 |
| `Guide.{Module}.Faq{N}A` | `Guide.SalesOrder.Faq1A` | 第 N 個 FAQ 答案 |
| `Guide.{Module}.{Custom}Title` | `Guide.SalesOrder.DetailTitle` | 自訂章節標題 |

---

## ✅ 已實作項目

### 說明定義

| 模組 | 定義檔案 | 章節數 | 狀態 |
|------|----------|--------|------|
| 銷售訂單 | `SalesOrderGuide.cs` | 9（概述、步驟、基本欄位、金額欄位、明細、功能按鈕、審核、提示、FAQ） | ✅ 完成 |

### resx key 統計

| 分類 | key 數量 | 說明 |
|------|----------|------|
| 共用 UI key | 7 | Modal 按鈕 + Drawer 標題 + 章節通用標題 |
| SalesOrder 專用 | 60 | Description×2 + Step×6 + Field×15 + Detail×6 + Action×10 + Approval×4 + Tip×3 + Warning×3 + FAQ×10 + Title×4 |
| 共用 Field key（新增） | 2 | Field.DiscountAmount, Field.TotalAmountWithTax |
| **合計** | 69 key × 5 語言 |

---

## 🔄 完整渲染流程

```
1. EditModal 傳入 FeatureGuide
   ↓ GenericEditModalComponent 透傳
   ↓ BaseModalComponent 接收 FeatureGuideDefinition

2. BaseModalComponent 渲染
   ↓ 右下角顯示 ❓ 浮動按鈕
   ↓ 使用者點擊 → _showFeatureGuide = true

3. Drawer 滑出
   ↓ 透明遮罩覆蓋 Modal 其餘區域
   ↓ 左側書籤列：遍歷 FeatureGuide.Sections 產生彩色標籤
   ↓ 右側主面板：Header + Body

4. FeatureGuideRenderer 渲染內容
   ↓ 遍歷 Sections
   ↓ 每個 Section：渲染標題（Icon + L[TitleKey]）
   ↓ 根據 Type 選擇渲染模板：
     ・Description → <p class="guide-description">
     ・Steps       → <ol class="guide-steps"><li>
     ・FieldList   → <dl class="guide-fields"><dt><dd>
     ・Tips        → <div class="guide-tip"> / <div class="guide-warning">
     ・FAQ         → <dl class="guide-fields"><dt><dd>

5. 書籤導航
   ↓ 使用者點擊書籤 → ScrollToBookmark(sectionId)
   ↓ JS: document.getElementById(id).scrollIntoView({ behavior: 'smooth' })
   ↓ _activeBookmarkId 更新 → 書籤 active 狀態變化

6. 關閉
   ↓ 點擊遮罩 / ✕ 按鈕 / Modal 關閉 → _showFeatureGuide = false
   ↓ Drawer 滑回（CSS transition）
```

---

## ⚠️ 注意事項

1. **Razor `section` 關鍵字衝突**：在 `.razor` 檔案中 `foreach` 迴圈的變數名稱不可使用 `section`（Razor 會誤判為 `@section` 指令），應使用 `sec` 或其他名稱
2. **resx XML 跳脫**：`&` 必須寫成 `&amp;`，否則 resx 解析會失敗（例如 `Approval & Printing` → `Approval &amp; Printing`）
3. **BookmarkLabel 語言**：目前書籤標籤文字直接寫在數據定義中（非 resx key），因為書籤空間有限，短標籤通常不需翻譯。如需多語系書籤，可改為 resx key 並在 BaseModalComponent 中解析
4. **`overflow: hidden` 在 `modal-content`**：Drawer 使用 `transform: translateX(100%)` 隱藏，需要父容器 `overflow: hidden` 才能正確裁切。此屬性已加在 `BaseModalComponent.razor.css` 的 `.modal-content` 上
5. **深色模式選擇器**：使用 `[data-bs-theme=dark] .class`（無 `::deep`），不使用 `@media (prefers-color-scheme: dark)`
6. **手機版佈局**：`≤ 576px` 時 Drawer 佔滿 100% 寬度，書籤改為頂部水平排列，`.feature-guide-main` 需設定 `flex: 1` 確保內容區填滿剩餘高度

---

## 📋 新增模組說明 Checklist

1. ☐ 在 `Models/FeatureGuides/GuideDefinitions/` 建立 `{Module}Guide.cs`
   - 定義 `Sections`（每個章節設定 Id、TitleKey、Icon、BookmarkLabel、BookmarkColor、Type）
   - 使用 `GuideItem` 建構子填入 resx key
2. ☐ 在所有 5 個 `SharedResource.*.resx` 新增翻譯 key
   - 共用 key（`Guide.Overview` 等）已存在，不需重複
   - 模組專用 key 遵循 `Guide.{Module}.{Type}` 命名規則
3. ☐ 在 EditModal 的 `<GenericEditModalComponent>` 加入 `FeatureGuide="@{Module}Guide.Create()"`
4. ☐ 加入 `@using ERPCore2.Models.FeatureGuides.GuideDefinitions`

---

## 相關檔案

- [README_數據模型與渲染.md](README_數據模型與渲染.md) - 數據模型詳細說明
- [README_新增模組說明指南.md](README_新增模組說明指南.md) - 新增模組逐步教學
