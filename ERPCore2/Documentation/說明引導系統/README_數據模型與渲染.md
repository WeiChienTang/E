# 數據模型與渲染設計說明

## 更新日期
2026-03-23

---

## 📋 設計概述

說明引導系統採用**數據驅動的自動渲染模式**，所有模組共用單一 `FeatureGuideRenderer.razor`，說明 UI 由 `FeatureGuideDefinition` 數據類別驅動自動產生：

- **數據驅動**：純 C# 物件定義章節與內容，不寫 HTML
- **單一渲染器**：`FeatureGuideRenderer.razor` 根據 `GuideSectionType` 自動選擇渲染模板
- **零 UI 程式碼**：新增模組說明不需撰寫 `.razor` 模板
- **i18n 整合**：所有文字透過 `IStringLocalizer<SharedResource>` 解析 resx key

---

## 🔧 數據模型

### 類別關係圖

```
FeatureGuideDefinition
└── List<GuideSection>
    ├── Id                  (string)     HTML anchor id
    ├── TitleKey            (string)     resx key → 章節標題
    ├── Icon                (string)     Bootstrap Icons 類別
    ├── BookmarkLabel       (string)     左側書籤短標籤
    ├── BookmarkColor       (string)     HEX 色碼
    ├── Type                (GuideSectionType)
    └── List<GuideItem>
        ├── TextKey         (string)     resx key → 主要文字
        ├── LabelKey        (string?)    resx key → 標籤文字（FieldList/FAQ）
        └── Style           (GuideItemStyle)
```

### GuideItem 建構子快捷用法

```csharp
// 1. 純文字項目（Description / Steps）
new GuideItem("Guide.SalesOrder.Step1")

// 2. 標籤 + 文字（FieldList / FAQ）
// 參數順序：labelKey, textKey
new GuideItem("Field.CustomerName", "Guide.SalesOrder.Field.Customer")
// FieldList → dt = L["Field.CustomerName"], dd = L["Guide.SalesOrder.Field.Customer"]
// FAQ       → dt = L["Guide.SalesOrder.Faq1Q"],  dd = L["Guide.SalesOrder.Faq1A"]

// 3. 提示框
new GuideItem("Guide.SalesOrder.Tip1", GuideItemStyle.Tip)

// 4. 警告框
new GuideItem("Guide.SalesOrder.Warning1", GuideItemStyle.Warning)
```

---

## 🎨 渲染邏輯

### FeatureGuideRenderer 渲染對照表

| GuideSectionType | HTML 結構 | CSS 類別 | GuideItem 解讀 |
|------------------|-----------|----------|----------------|
| `Description` | `<p>` | `guide-description` | `TextKey` → 段落文字 |
| `Steps` | `<ol><li>` | `guide-steps` | `TextKey` → 步驟文字（自動編號） |
| `FieldList` | `<dl><dt><dd>` | `guide-fields` | `LabelKey` → dt, `TextKey` → dd |
| `Tips` | `<div>` | `guide-tip` / `guide-warning` | `Style` 決定框型，`TextKey` → 內容 |
| `FAQ` | `<dl><dt><dd>` | `guide-fields` | `LabelKey` → 問題, `TextKey` → 答案 |

### 渲染器原始碼摘要

```razor
@foreach (var sec in Definition.Sections)
{
    <div class="guide-section" id="@sec.Id">
        <div class="guide-section-title">
            <i class="bi @sec.Icon"></i> @L[sec.TitleKey]
        </div>

        @switch (sec.Type)
        {
            case GuideSectionType.Steps:
                <ol class="guide-steps">
                    @foreach (var item in sec.Items)
                    {
                        <li>@L[item.TextKey]</li>
                    }
                </ol>
                break;
            // ... 其他類型
        }
    </div>
}
```

---

## 🎨 CSS 樣式類別

所有樣式定義在 `BaseModalComponent.razor.css`，使用 `::deep` 前綴確保 Blazor scoped CSS 穿透。

### Drawer 容器

| 類別 | 說明 |
|------|------|
| `.feature-guide-overlay` | 透明遮罩（z-index: 19），點擊關閉 Drawer |
| `.feature-guide-drawer` | Drawer 外層容器（flex row），transform 動畫 |
| `.feature-guide-drawer.open` | 開啟狀態（translateX(0)） |
| `.feature-guide-main` | 內容面板（340px 寬），含 Header + Body |
| `.feature-guide-header` | 標題列（📖 操作說明 + ✕ 關閉） |
| `.feature-guide-body` | 可滾動內容區（flex: 1, overflow-y: auto） |

### 書籤標籤

| 類別 | 說明 |
|------|------|
| `.feature-guide-bookmarks` | 書籤列容器（flex column） |
| `.feature-guide-bookmark` | 單一書籤標籤（垂直文字、圓角、彩色背景） |
| `.feature-guide-bookmark:hover` | Hover 效果（translateX(-3px)） |
| `.feature-guide-bookmark.active` | Active 狀態（translateX(-4px)、加深陰影） |

### 內容樣式

| 類別 | 用途 | 產生方式 |
|------|------|----------|
| `.guide-section` | 章節容器 | 每個 GuideSection |
| `.guide-section-title` | 章節標題（藍色底線） | TitleKey + Icon |
| `.guide-description` | 描述段落 | Description 類型 |
| `.guide-steps` | 有序步驟列表 | Steps 類型 |
| `.guide-steps li::before` | 圓形編號（藍底白字） | CSS counter |
| `.guide-fields` | 定義列表容器 | FieldList / FAQ 類型 |
| `.guide-fields dt` | 欄位名 / 問題標題 | LabelKey |
| `.guide-fields dd` | 欄位說明 / 答案（左側灰線） | TextKey |
| `.guide-tip` | 藍色提示框（左側藍線） | Tips + GuideItemStyle.Tip |
| `.guide-warning` | 黃色警告框（左側黃線） | Tips + GuideItemStyle.Warning |

### 響應式斷點

| 螢幕寬度 | 行為 |
|-----------|------|
| `> 576px` | Drawer 340px 寬，書籤垂直排列在左側 |
| `≤ 576px` | Drawer 100% 寬，書籤改為頂部水平排列 |

---

## 🔌 元件參數鏈

```
EditModal
  └─ GenericEditModalComponent
       [Parameter] FeatureGuideDefinition? FeatureGuide
       └─ BaseModalComponent
            [Parameter] FeatureGuideDefinition? FeatureGuide
            └─ FeatureGuideRenderer
                 [Parameter] FeatureGuideDefinition? Definition
```

---

## ⚠️ 注意事項

1. **Razor `section` 關鍵字**：`.razor` 檔案中迴圈變數不可命名為 `section`，Razor 編譯器會誤判為 `@section` 指令。渲染器中使用 `sec` 代替
2. **CSS `::deep` 與深色模式**：深色模式 CSS 使用 `[data-bs-theme=dark] .class`（不加 `::deep`），避免 Blazor scope attribute 附加在 `[data-bs-theme=dark]` 上導致永遠不匹配
3. **`overflow: hidden` 必要性**：`.modal-content` 必須設定 `overflow: hidden`，否則 Drawer 的 `translateX(100%)` 隱藏狀態會溢出可見

---

## 相關檔案

- [README_說明引導系統總綱.md](README_說明引導系統總綱.md) - 系統總綱
- [README_新增模組說明指南.md](README_新增模組說明指南.md) - 新增模組逐步教學
