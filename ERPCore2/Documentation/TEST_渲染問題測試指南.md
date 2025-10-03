# 測試渲染問題修正

## 快速測試

### 1. 啟動應用程式
```powershell
cd c:\Users\cses3\source\repos\ERPCore2\ERPCore2
dotnet run
```

### 2. 訪問測試頁面
在瀏覽器中打開：
```
https://localhost:5001/test/setoff-prepayment-render
```
或
```
http://localhost:5000/test/setoff-prepayment-render
```

### 3. 執行測試

#### 測試 A：基本輸入測試
1. 在第一列的「本次金額」輸入：`600`
2. 🔍 **立即觀察「原始金額」欄位**
   - ✅ 應該顯示：`600.00`（綠色）
   - ❌ 如果顯示：`6.00` 或不完整數字，表示還有問題

#### 測試 B：連續輸入測試
1. 繼續輸入：`6001`
2. 🔍 觀察每次按鍵後的變化
   - 輸入 `6` → 顯示 `6.00`
   - 輸入 `60` → 顯示 `60.00`
   - 輸入 `600` → 顯示 `600.00`
   - 輸入 `6001` → 顯示 `6,001.00`

#### 測試 C：負數測試
1. 清空欄位
2. 輸入：`-500`
3. 🔍 觀察「原始金額」
   - ✅ 應該顯示：`-500.00`（紅色）

#### 測試 D：渲染次數檢查
1. 觀察「渲染次數」欄位的變化
2. 🔍 輸入時該數字應該增加
   - ✅ 正常：每次輸入增加 1-2 次
   - ❌ 異常：頻繁跳動或增加過快

#### 測試 E：無需手動觸發
1. 輸入數字後，**不要點擊其他欄位**
2. 🔍 「原始金額」應該立即顯示正確數字
   - ✅ 不需要圈選其他欄位
   - ✅ 不需要點擊其他地方

### 4. 測試實際組件

#### 測試應收沖款明細
1. 前往應收帳款沖款頁面（或應付帳款）
2. 選擇一個客戶
3. 在明細表格中的「本次沖款」欄位輸入數字
4. 🔍 觀察「已沖款」和「待沖款」欄位是否立即更新

#### 測試預收預付款
1. 在沖款頁面中找到預收/預付款區塊
2. 在「本次金額」輸入數字
3. 🔍 觀察「原始金額」、「可用金額」是否立即更新

## 問題排查

### 如果測試失敗

#### 症狀：輸入後只顯示第一個數字
**可能原因：**
- CustomTemplate 沒有設定 PropertyName
- 沒有調用 StateHasChanged()

**檢查：**
```powershell
# 檢查是否有編譯錯誤
dotnet build

# 檢查瀏覽器控制台是否有 JavaScript 錯誤
# 按 F12 打開開發者工具
```

#### 症狀：需要點擊其他欄位才能更新
**可能原因：**
- StateHasChanged() 沒有被調用
- 事件處理有 bug

**解決：**
1. 檢查 `HandleAddAmountChanged` 方法
2. 確認 `NotifyChanges()` 有被調用
3. 確認 `NotifyChanges()` 內有 `StateHasChanged()`

#### 症狀：畫面閃爍或渲染過度
**可能原因：**
- 重複調用 StateHasChanged()

**解決：**
1. 檢查是否有兩次 `StateHasChanged()` 調用
2. 移除重複的調用

## 修正清單

已修正的檔案：
- ✅ `SetoffPrepaymentManagerComponent.razor`
  - 原始金額欄位：加上 PropertyName
  - 已用金額欄位：加上 PropertyName
  - 可用金額欄位：加上 PropertyName
  - HandleAddAmountChanged：移除重複 StateHasChanged

- ✅ `SetoffDetailManagerComponent.razor`
  - HandleAmountChanged：移除重複 StateHasChanged
  - HandleDiscountAmountChanged：移除重複 StateHasChanged

## 成功標準

✅ **測試通過標準：**
1. 輸入數字時，相關欄位**立即**顯示正確值
2. **不需要**手動觸發（點擊、圈選其他欄位）
3. 負數、千分位格式正確
4. 渲染次數合理（不會過度渲染）
5. 多筆資料同時編輯無問題

---

**測試建議：** 建議在不同瀏覽器中測試（Chrome, Edge, Firefox）
