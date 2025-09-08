# 手機版下拉選單修復測試

## 問題描述
商品選擇下拉選單在手機模式下無法正確顯示，可能的原因：
1. 使用 `position-fixed` 但沒有設定正確的 `top` 和 `left` 位置
2. 下拉選單出現在頁面左上角而不是輸入框下方
3. 手機版的觸控區域可能不夠大

## 修復內容

### 1. 修復下拉選單定位問題
**檔案**: `InteractiveTableComponent.razor` (第 182-183 行)

**修改前**:
```html
<div class="dropdown-menu show position-fixed w-auto shadow" 
     style="z-index: 9999; max-height: @column.DropdownMaxHeight; overflow-y: auto; overflow-x: hidden; border: 1px solid #dee2e6; min-width: @column.DropdownMinWidth; max-width: @column.DropdownMaxWidth;"
```

**修改後**:
```html
<div class="dropdown-menu show position-absolute w-auto shadow" 
     style="top: 100%; left: 0; z-index: 9999; max-height: @column.DropdownMaxHeight; overflow-y: auto; overflow-x: hidden; border: 1px solid #dee2e6; min-width: @column.DropdownMinWidth; max-width: @column.DropdownMaxWidth;"
```

**變更說明**:
- 將 `position-fixed` 改為 `position-absolute`
- 添加 `top: 100%` 讓下拉選單出現在輸入框下方
- 添加 `left: 0` 讓下拉選單與輸入框左對齊

### 2. 添加手機版下拉選單樣式優化
**檔案**: `InteractiveTableComponent.razor.css`

添加以下 CSS：
```css
/* 下拉選單在手機版的優化 */
@media (max-width: 767px) {
    /* 下拉選單在手機版的寬度調整 */
    .dropdown-menu {
        min-width: 250px !important;
        max-width: calc(100vw - 40px) !important; /* 確保不超出螢幕寬度 */
        font-size: 0.875rem; /* 手機版稍小的字體 */
    }
    
    /* 下拉選項在手機版的觸控優化 */
    .dropdown-item {
        padding: 12px 16px !important; /* 增大觸控區域 */
        line-height: 1.4;
    }
    
    /* 下拉選單項目選中狀態在手機版的顯示優化 */
    .dropdown-item.active {
        background-color: #007bff !important;
    }
}
```

## 測試步驟

1. **桌面版測試**:
   - 打開採購單頁面
   - 選擇供應商
   - 點擊商品選擇欄位
   - 輸入商品名稱或代碼
   - 確認下拉選單出現在輸入框正下方

2. **手機版測試**:
   - 使用瀏覽器開發者工具切換到手機模式
   - 或使用手機實際測試
   - 重複上述步驟
   - 確認下拉選單：
     - 出現在正確位置（輸入框下方）
     - 不會超出螢幕寬度
     - 觸控區域足夠大
     - 選中項目有明顯的視覺回饋

## 預期結果

- 下拉選單應該出現在輸入框正下方
- 手機版下拉選單不會超出螢幕邊界
- 觸控操作更加友善
- 在所有裝置上都能正常使用商品搜尋功能

## 回滾計劃

如果修復造成其他問題，可以還原：
1. 將 `position-absolute` 改回 `position-fixed`
2. 移除 `top: 100%; left: 0;`
3. 移除新增的手機版 CSS 樣式
