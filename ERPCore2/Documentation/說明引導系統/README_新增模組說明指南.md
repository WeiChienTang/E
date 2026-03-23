# 新增模組說明指南

## 更新日期
2026-03-23

---

## 📋 概述

本文件以逐步教學方式，說明如何為一個新的 EditModal 模組添加功能說明（Feature Guide）。整個過程只需三個步驟，不需要撰寫任何 HTML 或 CSS。

---

## 🚀 完整範例：為採購單新增說明

### 步驟 1：建立數據定義檔

在 `Models/FeatureGuides/GuideDefinitions/` 建立 `PurchaseOrderGuide.cs`：

```csharp
namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

public static class PurchaseOrderGuide
{
    public static FeatureGuideDefinition Create() => new()
    {
        Sections = new()
        {
            // === 功能概述 ===
            new GuideSection
            {
                Id = "guide-po-overview",
                TitleKey = "Guide.Overview",          // 共用 key，已存在
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",            // 藍色
                Type = GuideSectionType.Description,
                Items = { new("Guide.PurchaseOrder.Description") }
            },

            // === 操作步驟 ===
            new GuideSection
            {
                Id = "guide-po-steps",
                TitleKey = "Guide.Steps",             // 共用 key，已存在
                Icon = "bi-list-ol",
                BookmarkLabel = "步驟",
                BookmarkColor = "#10B981",            // 綠色
                Type = GuideSectionType.Steps,
                Items =
                {
                    new("Guide.PurchaseOrder.Step1"),
                    new("Guide.PurchaseOrder.Step2"),
                    new("Guide.PurchaseOrder.Step3"),
                    new("Guide.PurchaseOrder.Step4"),
                }
            },

            // === 欄位說明 ===
            new GuideSection
            {
                Id = "guide-po-fields",
                TitleKey = "Guide.FieldDescriptions",  // 共用 key，已存在
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",            // 橙色
                Type = GuideSectionType.FieldList,
                Items =
                {
                    // 參數順序：labelKey（dt 欄位名）, textKey（dd 說明）
                    new("Field.SupplierName", "Guide.PurchaseOrder.Field.Supplier"),
                    new("Field.OrderDate", "Guide.PurchaseOrder.Field.OrderDate"),
                }
            },

            // === 提示與警告 ===
            new GuideSection
            {
                Id = "guide-po-tips",
                TitleKey = "Guide.Tips",              // 共用 key，已存在
                Icon = "bi-lightbulb",
                BookmarkLabel = "提示",
                BookmarkColor = "#06B6D4",            // 青色
                Type = GuideSectionType.Tips,
                Items =
                {
                    new("Guide.PurchaseOrder.Tip1", GuideItemStyle.Tip),
                    new("Guide.PurchaseOrder.Warning1", GuideItemStyle.Warning),
                }
            },

            // === 常見問題 ===
            new GuideSection
            {
                Id = "guide-po-faq",
                TitleKey = "Guide.PurchaseOrder.FaqTitle",  // 模組專用 key
                Icon = "bi-question-diamond",
                BookmarkLabel = "FAQ",
                BookmarkColor = "#6366F1",            // 靛色
                Type = GuideSectionType.FAQ,
                Items =
                {
                    // 參數順序：labelKey（dt 問題）, textKey（dd 答案）
                    new("Guide.PurchaseOrder.Faq1Q", "Guide.PurchaseOrder.Faq1A"),
                }
            },
        }
    };
}
```

### 步驟 2：新增 resx key

在所有 5 個 `SharedResource.*.resx` 中加入翻譯：

```xml
<!-- SharedResource.resx (zh-TW) -->
<data name="Guide.PurchaseOrder.Description" xml:space="preserve">
  <value>採購單用於記錄向廠商採購的品項、數量及金額。建立後可進行審核、列印採購單，並轉為進貨作業。</value>
</data>
<data name="Guide.PurchaseOrder.Step1" xml:space="preserve">
  <value>選擇廠商：從廠商欄位搜尋並選擇供應廠商</value>
</data>
<data name="Guide.PurchaseOrder.Step2" xml:space="preserve">
  <value>填寫採購資訊：設定採購日期、稅別等基本資料</value>
</data>
<data name="Guide.PurchaseOrder.Step3" xml:space="preserve">
  <value>新增採購明細：在明細分頁中加入品項、數量與單價</value>
</data>
<data name="Guide.PurchaseOrder.Step4" xml:space="preserve">
  <value>儲存或送審：點擊儲存按鈕，或提交審核流程</value>
</data>
<data name="Guide.PurchaseOrder.Field.Supplier" xml:space="preserve">
  <value>選擇此採購單的廠商，系統會自動帶入廠商相關資料</value>
</data>
<data name="Guide.PurchaseOrder.Field.OrderDate" xml:space="preserve">
  <value>採購日期，預設為今天，可手動修改</value>
</data>
<data name="Guide.PurchaseOrder.Tip1" xml:space="preserve">
  <value>採購單可批次列印，從列表頁點擊列印按鈕即可篩選並批次產出。</value>
</data>
<data name="Guide.PurchaseOrder.Warning1" xml:space="preserve">
  <value>審核通過後將無法修改內容，請確認所有資料正確後再提交審核。</value>
</data>
<data name="Guide.PurchaseOrder.FaqTitle" xml:space="preserve">
  <value>常見問題</value>
</data>
<data name="Guide.PurchaseOrder.Faq1Q" xml:space="preserve">
  <value>如何批次列印採購單？</value>
</data>
<data name="Guide.PurchaseOrder.Faq1A" xml:space="preserve">
  <value>在採購單列表頁點擊「列印」按鈕，設定篩選條件後即可批次預覽與列印。</value>
</data>
```

> **重要**：`&` 必須寫成 `&amp;`（XML 跳脫）

### 步驟 3：在 EditModal 掛載

```razor
@using ERPCore2.Models.FeatureGuides.GuideDefinitions

<GenericEditModalComponent TEntity="PurchaseOrder"
                          TService="IPurchaseOrderService"
                          ...
                          FeatureGuide="@PurchaseOrderGuide.Create()">
</GenericEditModalComponent>
```

完成！打開採購單 EditModal 即可看到右下角的 ❓ 按鈕。

---

## 🎨 書籤顏色建議

為保持視覺一致性，建議使用以下 Tailwind 色板：

| 用途 | 色碼 | 色名 | 適合章節 |
|------|------|------|----------|
| 概述 / 總覽 | `#3B82F6` | Blue 500 | Description |
| 步驟 / 流程 | `#10B981` | Emerald 500 | Steps |
| 欄位 / 參數 | `#F59E0B` | Amber 500 | FieldList |
| 明細 / 表格 | `#8B5CF6` | Violet 500 | Steps（明細操作） |
| 審核 / 重要 | `#EF4444` | Red 500 | Steps（審核流程） |
| 提示 / 注意 | `#06B6D4` | Cyan 500 | Tips |
| FAQ | `#6366F1` | Indigo 500 | FAQ |
| 進階 / 設定 | `#EC4899` | Pink 500 | 自訂 |
| 範例 / 示範 | `#14B8A6` | Teal 500 | 自訂 |

---

## 📝 章節規劃建議

### 基本模組（3-5 個章節）

適合簡單的 CRUD 模組（如：系統設定、基本資料維護）：

```csharp
Sections = new()
{
    // 概述（Description）     — 這個功能是什麼
    // 操作步驟（Steps）       — 如何使用
    // 欄位說明（FieldList）   — 重要欄位解釋
    // 提示（Tips）            — 注意事項
}
```

### 完整業務模組（5-7 個章節）

適合有明細、審核、列印的複雜模組（如：銷售訂單、採購單）：

```csharp
Sections = new()
{
    // 概述（Description）     — 功能說明 + 業務流程定位
    // 操作步驟（Steps）       — 基本操作流程
    // 欄位說明（FieldList）   — 重要欄位解釋
    // 明細操作（Steps）       — 明細增刪改操作
    // 審核與列印（Steps）     — 審核流程 + 列印方式
    // 提示與警告（Tips）      — 注意事項
    // 常見問題（FAQ）         — 使用者常遇到的問題
}
```

---

## ⚠️ 常見問題

### Q: 為什麼 ❓ 按鈕沒有出現？

A: 確認以下條件：
1. `FeatureGuide` 參數有傳入（非 null）
2. `ShowFooter="@isEditMode"` — 浮動按鈕只在有 Footer 時顯示（即實體已儲存，Id > 0）
3. 新建模式下不會顯示按鈕，需要先儲存一次

### Q: 書籤點擊沒有滾動效果？

A: 確認 `GuideSection.Id` 與內容中的 `id` 屬性一致。渲染器會自動將 `sec.Id` 設為 `<div id="@sec.Id">`，書籤點擊時會呼叫 `document.getElementById(id).scrollIntoView()`。

### Q: 如何新增自訂章節類型？

A: 目前支援 5 種 `GuideSectionType`。如需新增：
1. 在 `GuideSectionType` enum 加入新值
2. 在 `FeatureGuideRenderer.razor` 的 `switch` 區塊加入對應的渲染邏輯
3. 在 `BaseModalComponent.razor.css` 加入對應的 CSS 樣式

### Q: 可以在說明中嵌入圖片嗎？

A: 目前不支援。說明內容為純文字（透過 resx key）。如需圖片支援，可考慮新增 `GuideSectionType.Image` 類型或在 `GuideItem` 中加入 `ImageUrl` 屬性。

---

## 相關檔案

- [README_說明引導系統總綱.md](README_說明引導系統總綱.md) - 系統總綱
- [README_數據模型與渲染.md](README_數據模型與渲染.md) - 數據模型詳細說明
