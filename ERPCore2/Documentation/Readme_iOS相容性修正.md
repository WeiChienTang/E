# iOS 相容性修正計畫

## 問題描述

在 iOS（iPhone / iPad）的 Safari 瀏覽器上，Modal 元件出現以下問題：

- 開啟 Modal 後看不到任何輸入欄位，只顯示標題列（最嚴重）
- 按鈕無法點擊或點擊沒有反應
- 表單無法捲動
- 部分情況下 Modal 完全無法開啟

Android 裝置與桌面瀏覽器皆正常運作。

---

## 根本原因分析

### 原因一：`100vh` 在 iOS Safari 的計算差異（最主要）

iOS Safari 將 `100vh` 定義為「網址列完全收起後」的視窗最大高度（large viewport height），而非實際可見高度。

| 平台 | `100vh` 計算結果 | 實際可見視窗 |
|------|----------------|------------|
| Android Chrome | 等於實際可見高度 | ✅ 兩者一致 |
| iOS Safari | 包含 Safari UI 工具列的全螢幕高度 | ❌ 比可見高度多 50–80px |

**iPhone SE（375×667）實際計算：**

```
iOS 100vh = 667px（Safari 工具列收起時的全螢幕）
實際可見視窗 = 667px - 64px（網址列＋底部工具列）= 約 603px

outer modal（position:fixed, height:100%）= 603px（正確）
modal-dialog 高度 = calc(667px - 48px) = 619px（超出！）
dialog 從 margin-top:24px 開始 → 延伸到 643px
→ overflow:hidden 剪切至 603px，底部 40px 被截掉
```

受影響的 CSS 屬性：
- `BaseModalComponent.razor.css` → `.modal-body { max-height: calc(100vh - 210px) }`
- `BaseModalComponent.razor.css` → `.modal-dialog.modal-desktop { height: calc(100vh - 3rem) }`
- `BaseModalComponent.razor` C# → `GetModalDialogStyle()` 回傳 `max-height: calc(100vh - 3rem)`

---

### 原因二：手機版 modal-action-bar 垂直堆疊過高（造成欄位完全消失）

Modal 的 content 區域是 flex column 佈局：

```
modal-content（flex column, height:100%）
  ├── modal-header        （~60px）← 標題列，永遠可見
  ├── modal-action-bar    （手機上可能高達 200–350px）← 按鈕列
  └── modal-body          （flex: 1 1 auto）← 表單欄位
```

目前 `GenericEditModalComponent.razor.css` 在手機（max-width: 576px）下設定：

```css
.modal-buttons-container {
    flex-direction: column;        /* 每個按鈕群組垂直堆疊 */
    align-items: stretch !important;
}
```

`CustomerEditModalComponent` 的按鈕列包含：
- 自訂業務按鈕（複製至廠商等）
- 狀態 badge
- 審核按鈕（核准 / 駁回）
- 主要操作按鈕（儲存、列印、重新整理、刪除）= 至少 4 個按鈕

加上每個按鈕群組佔一整行，**action-bar 高度可達 200–350px**。

**結果：**

```
modal-content 有效高度 = 603px（實際可見）- 24px（margin）= 579px
modal-header   = 60px
modal-action-bar = 300px（多按鈕堆疊）
合計已用 = 360px

modal-body 開始位置 = 360px
modal-body 最大可見範圍 = 579px
modal-body 可見高度 = 579px - 360px = 219px  →  非常小甚至接近 0
```

若 action-bar 更高（> 400px），modal-body **完全不在可見範圍內**，表單欄位全部消失。
這就是為何 iOS 上標題看得到但表單欄位完全不見。

---

### 原因三：缺少 `-webkit-overflow-scrolling: touch`

`BaseModalComponent.razor.css` 的 `.modal-body` 只有 `overflow-y: auto`，缺少 iOS 慣性捲動支援：

```css
/* 現況（缺少 iOS 捲動） */
::deep .modal-body {
    overflow-y: auto;  /* 沒有 -webkit-overflow-scrolling: touch */
}
```

**結果：** Modal 內部在 iOS 上無法流暢捲動，感覺卡住無法操作。

---

### 原因四：`position: fixed` + 鍵盤彈出造成版面位移

iOS Safari 的已知問題：鍵盤彈出時，`position: fixed` 的元素不會自動避開鍵盤，而是維持原位，導致：

- 輸入框被鍵盤遮住，看不到也點不到
- Modal 底部按鈕（儲存、取消）被推到鍵盤後方
- Android Chrome 對此有額外處理，所以 Android 正常

---

### 原因五：Input font-size 自動縮放（次要）

iOS Safari 規定：當 `<input>` 的 `font-size < 16px` 時，自動 zoom in，導致版面跳動。

`GenericFormComponent.razor.css` 已針對手機版加入 `font-size: 16px !important`（line 374），此問題在表單欄位上已處理，但其他地方（如搜尋框等）需留意。

---

## 修正計畫

### 修正一：`100vh` → `dvh` / `svh`（解決 modal 溢出問題）

**影響檔案：**
- `Components/Shared/Modal/BaseModalComponent.razor.css`
- `Components/Shared/Modal/BaseModalComponent.razor`（`GetModalDialogStyle()` 方法）

**說明：**

CSS 支援同一屬性的多個宣告作為 fallback（瀏覽器採用最後一個它認識的值）：

| 單位 | 說明 | 支援度 |
|------|------|--------|
| `100vh` | 大視窗高度（含 Safari UI）| 所有瀏覽器（但 iOS 有問題）|
| `100svh` | 小視窗高度（Safari UI 展開時）| iOS 15.4+ / Chrome 108+ |
| `100dvh` | 動態視窗高度（隨 Safari UI 自動調整）| iOS 16+ / Chrome 108+ |

`svh` 是 Modal 的最佳選擇：永遠等於 Safari UI 展開時的可見高度，確保 Modal 不超出畫面。

**修改範圍：**

```css
/* 修改前 */
height: calc(100vh - 3rem);

/* 修改後（三層 fallback） */
height: calc(100vh - 3rem);    /* 舊瀏覽器 fallback */
height: calc(100svh - 3rem);   /* iOS 15.4+ 修正 */
height: calc(100dvh - 3rem);   /* 現代瀏覽器：動態調整 */
```

需要修改的 CSS 位置：
1. `.modal-body { max-height: calc(100vh - 210px) }`
2. `@media (max-width: 576px)` 內 `.modal-body { max-height: calc(100vh - 180px) }`
3. `.modal-dialog.modal-desktop { height: calc(100vh - 3rem) }`

`GetModalDialogStyle()` 的 inline style 無法使用 CSS fallback 機制，需將 `max-height` 從 inline style 移至 CSS，inline style 只保留 `position: relative` 和 `margin: 1.5rem auto`。

---

### 修正二：手機版 action bar 改為水平捲動（解決欄位消失問題）

**影響檔案：**
- `Components/Shared/Modal/BaseModalComponent.razor.css`
- `Components/Shared/Modal/GenericEditModalComponent.razor.css`

**說明：**

將手機版按鈕列從「垂直堆疊（每組一行）」改為「水平單行 + overflow 捲動」，確保 action-bar 固定只佔一行高度（約 50–60px）。

**修改概念：**

```
修改前（垂直堆疊）：          修改後（水平捲動）：
┌─────────────────────┐     ┌───────────────────────────────────→
│ [自訂按鈕群組]       │     │ [自訂] [狀態] [核准][駁回] [儲存][列印][刪除] ···
│ [狀態 badge]         │     └───────────────────────────────────→
│ [核准] [駁回]        │       可向右滑動看到更多按鈕
│ [儲存] [列印] [刪除] │
└─────────────────────┘
  高度：~300px                高度：~55px（固定）
```

**修改範圍（BaseModalComponent.razor.css）：**

```css
@media (max-width: 576px) {
    ::deep .modal-action-bar {
        flex-wrap: nowrap;                    /* 不換行 */
        overflow-x: auto;                     /* 水平捲動 */
        overflow-y: hidden;
        -webkit-overflow-scrolling: touch;    /* iOS 慣性捲動 */
        justify-content: flex-start;
        scrollbar-width: none;
        -ms-overflow-style: none;
    }
    ::deep .modal-action-bar::-webkit-scrollbar { display: none; }
    ::deep .modal-action-bar > .d-flex {
        flex-shrink: 0;   /* 按鈕群組不壓縮 */
        flex-wrap: nowrap;
    }
}
```

**修改範圍（GenericEditModalComponent.razor.css）：**

```css
@media (max-width: 576px) {
    .modal-buttons-container {
        flex-direction: row;      /* 改回水平排列 */
        flex-wrap: nowrap;
        align-items: center !important;
        overflow-x: auto;
        -webkit-overflow-scrolling: touch;
    }
    /* 移除強制全寬 */
    .custom-buttons-section,
    .modal-buttons-container > .d-flex:last-child,
    .modal-buttons-container > .d-flex.me-3 {
        width: auto;
        flex-shrink: 0;
        justify-content: flex-start;
        margin-right: 0.25rem !important;
    }
}
```

---

### 修正三：補上 iOS 慣性捲動（解決 modal 內無法捲動）

**影響檔案：**
- `Components/Shared/Modal/BaseModalComponent.razor.css`

```css
/* 修改前 */
::deep .modal-body {
    overflow-y: auto;
}

/* 修改後 */
::deep .modal-body {
    overflow-y: auto;
    -webkit-overflow-scrolling: touch;   /* 補上 iOS 慣性捲動 */
}
```

---

## 修正優先序

| 優先 | 修正 | 解決症狀 | 修改難度 |
|------|------|---------|---------|
| 🔴 最高 | 修正二：action bar 水平化 | 表單欄位完全消失 | 低（僅 CSS）|
| 🔴 最高 | 修正一：`100vh` → `dvh/svh` | Modal 溢出、按鈕被截掉 | 低（CSS + 小幅 C#）|
| 🟡 中 | 修正三：`-webkit-overflow-scrolling` | Modal 內無法捲動 | 最低（一行 CSS）|

---

## 影響範圍

以上修正皆為 **CSS 層級調整**，不修改任何業務邏輯、Blazor 元件參數或 C# 後端程式碼（除 `GetModalDialogStyle()` 的 inline style 搬移至 CSS 外）。

所有使用 `BaseModalComponent` 的 Modal 均受益，包含：
- `GenericEditModalComponent`（所有 Edit Modal）
- `GenericConfirmModalComponent`
- `ReportPreviewModalComponent`
- `GenericReportFilterModalComponent`
- 其他所有繼承 `BaseModalComponent` 的元件

---

## 已知不在本次修正範圍內的問題

| 問題 | 說明 |
|------|------|
| `position: fixed` + 鍵盤位移 | iOS Safari 的 OS 層級 bug，需要 JS 偵測鍵盤高度動態調整，複雜度較高，列為後續改善 |
| SignalR WebSocket 斷線 | iOS 背景限制導致 Blazor 連線中斷，與 Modal 問題無直接關係，屬於另一個議題 |

---

## 測試建議

修正後建議在以下環境驗證：

- **iOS 15** — 驗證 `svh` fallback 是否生效
- **iOS 16+** — 驗證 `dvh` 動態調整
- **iOS 14 以下** — 驗證 `100vh` fallback 不破壞現有顯示
- **iPhone SE（小螢幕 375px）** — 最嚴苛條件
- **iPhone 14 Pro（Dynamic Island）** — 驗證大螢幕正常
- **iPad** — 驗證平板模式不受影響
- **Android Chrome** — 確認現有功能不退步
- **桌面 Chrome / Firefox** — 確認桌面版無變化
