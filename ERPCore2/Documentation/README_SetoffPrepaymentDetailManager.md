# SetoffPrepaymentDetailManagerComponent 實作說明

## 概述
`SetoffPrepaymentDetailManagerComponent.razor` 是一個用於管理沖款單中預收付款項使用記錄的組件，參考 `SetoffPaymentDetailManagerComponent.razor` 和 `AddressManagerComponent.razor` 的設計模式實作。

## 主要功能

### 1. 自動保持一行空行輸入
- 使用 `AutoEmptyRowHelper` 工具類別
- 當使用者選擇來源預收付款項後，自動新增一行空行供繼續輸入
- 核心判斷欄位：`SourcePrepaymentId`（來源預收付款項ID）

### 2. 動態載入可用預收付款項
- 根據沖款單的類型（應收/應付）和關聯對象（客戶/供應商）載入對應的可用預收付款項
- 應收帳款沖款：載入客戶的可用預收款項
- 應付帳款沖款：載入供應商的可用預付款項
- 僅顯示尚有可用餘額的預收付款項

### 3. 互動式表格顯示
使用 `InteractiveTableComponent` 顯示以下欄位：

| 欄位名稱 | 類型 | 說明 |
|---------|------|------|
| 來源單號 | 下拉選單 | 選擇可用的預收付款項，顯示來源單號和可用餘額 |
| 預收付類型 | 唯讀顯示 | 自動帶入選擇的預收付款項類型 |
| 總金額 | 唯讀顯示 | 該筆預收付款項的總金額 |
| 可用餘額 | 唯讀顯示 | 該筆預收付款項的可用餘額（以藍色粗體顯示） |
| 本次使用金額 | 數字輸入 | 本次沖款要使用的金額，不可超過可用餘額 |

### 4. 資料驗證
- 使用金額必須大於 0
- 使用金額不能超過可用餘額
- 選擇來源預收付款項時自動帶入可用餘額作為預設使用金額
- 使用者修改金額時即時檢查並提示錯誤

### 5. 資料同步
- 透過 `OnPrepaymentsChanged` 事件通知父組件資料變更
- 僅傳遞非空行的有效資料
- 自動設定 `SetoffDocumentId` 關聯

## 核心方法

### 生命週期方法
- `OnInitializedAsync()`: 初始化時載入可用預收付款項
- `OnParametersSetAsync()`: 參數變更時重新載入資料並確保空行

### AutoEmptyRowHelper 相關
- `IsEmptyRow()`: 判斷是否為空行（檢查 SourcePrepaymentId）
- `CreateEmptyItem()`: 創建空的預收付款項項目
- `EnsureOneEmptyRow()`: 確保總是有一行空行可輸入

### 資料處理
- `LoadAvailablePrepaymentsAsync()`: 根據客戶/供應商載入可用預收付款項
- `LoadExistingPrepayments()`: 載入已存在的使用記錄（編輯模式）
- `NotifyPrepaymentsChanged()`: 通知父組件資料變更

### 事件處理
- `OnUsedAmountInput()`: 處理使用金額輸入，驗證金額範圍
- `HandleRemovePrepaymentRecord()`: 處理刪除記錄

### 公開方法（供父組件調用）
- `GetTotalUsedAmount()`: 取得總使用金額
- `ValidateAsync()`: 驗證預收付款項記錄的有效性

## 套用到 SetoffDocumentEditModalComponent

### 1. 參數傳遞
```razor
<SetoffPrepaymentDetailManagerComponent @ref="setoffPrepaymentDetailManager"
    SetoffDocumentId="@SetoffDocumentId"
    CustomerId="@(editModalComponent.Entity.SetoffType == SetoffType.AccountsReceivable ? editModalComponent.Entity.RelatedPartyId : null)"
    SupplierId="@(editModalComponent.Entity.SetoffType == SetoffType.AccountsPayable ? editModalComponent.Entity.RelatedPartyId : null)"
    ExistingPrepayments="@setoffPrepayments"
    OnPrepaymentsChanged="@HandlePrepaymentsChanged"
    IsReadOnly="@IsReadOnly" />
```

### 2. 驗證整合
在 `ValidateBeforeSave` 方法中加入預收付款項驗證：
```csharp
// 驗證預收付款項（如果有填寫的話）
if (setoffPrepaymentDetailManager != null)
{
    var prepaymentValidationResult = await setoffPrepaymentDetailManager.ValidateAsync();
    if (!prepaymentValidationResult)
    {
        return false;
    }
}
```

### 3. 儲存整合
`HandleSaveSuccess` 方法已包含預收付款項的儲存邏輯：
- 過濾使用金額大於 0 的記錄
- 刪除舊的記錄（編輯模式）
- 新增新的記錄
- 自動設定 SetoffDocumentId

## 資料流程

### 新增模式
1. 使用者選擇沖款類型（應收/應付）
2. 選擇客戶/供應商
3. 組件自動載入該客戶/供應商的可用預收付款項
4. 使用者從下拉選單選擇要使用的預收付款項
5. 系統自動帶入可用餘額作為預設使用金額
6. 使用者可調整使用金額（不可超過可用餘額）
7. 自動新增一行空行供繼續輸入
8. 儲存時驗證並寫入資料庫

### 編輯模式
1. 載入沖款單時同時載入已使用的預收付款項記錄
2. 顯示已選擇的預收付款項和使用金額
3. 使用者可修改、刪除或新增記錄
4. 儲存時更新資料庫

## 技術特點

1. **智能空行管理**: 使用 `AutoEmptyRowHelper` 統一處理空行邏輯
2. **即時驗證**: 輸入金額時即時檢查並提示錯誤
3. **自動帶值**: 選擇預收付款項後自動帶入相關資訊
4. **響應式更新**: 客戶/供應商變更時自動重新載入可用預收付款項
5. **資料安全**: 僅允許使用可用餘額內的金額

## 相依服務
- `ISetoffPrepaymentService`: 預收付款項服務
- `INotificationService`: 通知服務

## 資料實體
- `SetoffPrepayment`: 預收付款項實體
- `SetoffPrepaymentItem`: 內部資料項目類別（用於 UI 綁定）

## 注意事項
1. 預收付款項是可選的，不一定要填寫
2. 使用金額不能超過可用餘額
3. 同一筆預收付款項可以分多次使用（只要餘額足夠）
4. 客戶和供應商不能同時存在，根據沖款類型自動判斷
