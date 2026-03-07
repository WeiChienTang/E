# 詢問訊息操作指南

## 更新日期
2026-03-07

---

## 概述

`GenericConfirmModalComponent` 是通用確認對話框元件，取代原生 JavaScript `confirm()`，提供可自訂標題、條件清單、摘要訊息、按鈕樣式的確認 Modal。

元件路徑：`Components/Shared/UI/GenericConfirmModalComponent.razor`

---

## GenericConfirmModalComponent 參數

| 參數 | 型別 | 預設值 | 說明 |
|------|------|--------|------|
| `IsVisible` | `bool` | `false` | Modal 是否顯示 |
| `IsVisibleChanged` | `EventCallback<bool>` | — | 顯示狀態變更事件 |
| `Title` | `string` | `"確認"` | Modal 標題 |
| `Icon` | `string` | `"bi-question-circle"` | 標題圖示（Bootstrap Icon class） |
| `Message` | `string?` | `null` | 主訊息文字 |
| `Conditions` | `List<string>?` | `null` | 條件清單（每項前顯示勾選圖示） |
| `SummaryMessage` | `string?` | `null` | 摘要訊息（粗體藍色顯示，置於條件清單之後） |
| `ConfirmButtonText` | `string` | `"確認"` | 確認按鈕文字 |
| `CancelButtonText` | `string` | `"取消"` | 取消按鈕文字 |
| `ConfirmButtonVariant` | `ButtonVariant` | `ButtonVariant.Blue` | 確認按鈕樣式 |
| `HeaderColor` | `HeaderVariant` | `HeaderVariant.Primary` | 標題列顏色 |
| `OnConfirm` | `EventCallback` | — | 確認按鈕點擊事件 |
| `OnCancel` | `EventCallback` | — | 取消按鈕點擊事件 |
| `ChildContent` | `RenderFragment?` | `null` | 自訂內容區塊（可放置表單控制項） |

---

## HeaderVariant 選項

| 值 | 說明 |
|----|------|
| `Default` | 白底黑字 |
| `Primary` | Bootstrap 藍色 |
| `Secondary` | Bootstrap 灰色 |
| `Success` | Bootstrap 綠色 |
| `Danger` | Bootstrap 紅色 |
| `Warning` | Bootstrap 黃色 |
| `Info` | Bootstrap 淺藍色 |
| `Dark` | Bootstrap 深色 |
| `ProjectPrimary` | 專案主色 #1F2937 |

---

## ButtonVariant 選項

| 值 | 說明 |
|----|------|
| `Blue` | 藍色（主要操作） |
| `Green` | 綠色（成功 / 確認） |
| `Red` | 紅色（危險 / 刪除） |
| `Gray` | 灰色（取消 / 次要） |
| `DarkBlue` | 深藍色 |
| `Cyan` | 青色 |
| `Purple` | 紫色 |
| `Pink` | 粉紅色 |
| `Orange` | 橙色 |
| `Black` | 黑色 |
| `White` | 白色 |
| `Outline*` | 以上所有顏色的輪廓版本（如 `OutlineBlue`、`OutlineCyan`） |

---

## 重要設計規則

### 1. Modal 自動關閉

確認或取消後 Modal 會自動關閉，`OnConfirm` / `OnCancel` 中不需手動設定 `IsVisible = false`。

### 2. 條件清單於顯示前動態計算

`Conditions` 和 `SummaryMessage` 應在開啟 Modal 之前完成計算並賦值，確保使用者看到的資訊準確。

### 3. 顯示前先驗證條件

開啟 Modal 前應先驗證必要條件，不符合時以 `NotificationService` 顯示警告並直接返回，不進入確認流程。

### 4. 長時間操作需防重複提交

`OnConfirm` 中若執行耗時操作，應在開始時設定 loading 狀態並停用按鈕，避免重複觸發。

---

## 使用時機

- 需要使用者明確確認「是 / 否」的操作（刪除、重置、批次處理等）
- 需要列出操作條件讓使用者核對（載入確認、批次確認）
- 需要在確認前讓使用者選擇選項（透過 `ChildContent` 放置表單控制項）

---

## 相關檔案

- 元件：`Components/Shared/UI/GenericConfirmModalComponent.razor`
- 基礎 Modal：`Components/Shared/Base/BaseModalComponent.razor`
- 按鈕元件：`Components/Shared/UI/Button/GenericButtonComponent.razor`
- [README_共用元件設計總綱.md](README_共用元件設計總綱.md)
