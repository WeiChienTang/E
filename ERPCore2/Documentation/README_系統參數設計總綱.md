# 系統參數設計總綱

> 最後更新：2026-02-14
> 專案：ERPCore2

---

## 一、架構總覽

系統參數（SystemParameter）負責管理全域性的營運設定，包含稅率與各模組的簽核流程開關。整體架構遵循 ERPCore2 的分層模式：

```
Entity（資料模型）
  └─ SystemParameter.cs

Service（商業邏輯）
  ├─ ISystemParameterService.cs（介面）
  └─ SystemParameterService.cs（實作，含快取機制）

UI（使用者介面）
  └─ SystemParameterSettingsModal.razor
      └─ Shared/Tabs/
          ├─ BasicSettingsTab.razor（稅率、備註）
          └─ ApprovalSettingsTab.razor（簽核開關）

Seeder（種子資料）
  └─ SystemParameterSeeder.cs

Defaults（預設值定義）← 本次新增
  └─ SystemParameterDefaults.cs
```

---

## 二、現有欄位定義

### SystemParameter Entity

繼承自 `BaseEntity`，包含以下業務欄位：

| 欄位名稱 | 型別 | 預設值 | 說明 |
|---------|------|--------|------|
| TaxRate | decimal | 5.00m | 系統稅率（範圍 0 ~ 100%） |
| EnableQuotationApproval | bool | false | 報價單簽核 |
| EnablePurchaseOrderApproval | bool | false | 採購單簽核 |
| EnablePurchaseReceivingApproval | bool | false | 進貨單簽核 |
| EnablePurchaseReturnApproval | bool | false | 採購退貨單簽核 |
| EnableSalesOrderApproval | bool | false | 銷售單簽核 |
| EnableSalesReturnApproval | bool | false | 銷售退貨單簽核 |
| EnableInventoryTransferApproval | bool | false | 庫存調撥單簽核 |

BaseEntity 繼承欄位：Id、Status、CreatedAt、CreatedBy、UpdatedAt、UpdatedBy、Remarks。

---

## 三、Service 層設計

### 介面 ISystemParameterService

定義 `ApprovalType` 列舉對應七種簽核類型，主要方法包含：

- `GetSystemParameterAsync()` — 取得目前生效的系統參數
- `GetTaxRateAsync()` / `SetTaxRateAsync()` — 稅率存取
- `IsApprovalEnabledAsync(ApprovalType)` — 泛用簽核狀態查詢（含快取）
- 各模組專用簽核查詢方法（如 `IsQuotationApprovalEnabledAsync()`）
- `ClearApprovalConfigCache()` — 手動清除快取

### 快取策略

SystemParameterService 對簽核設定採用 5 分鐘記憶體快取：

- 快取鍵值以 `_cachedParameter` + `_cacheExpiration` 控制
- 任何更新操作後自動呼叫 `ClearApprovalConfigCache()` 使快取失效
- `GetTaxRateAsync()` 讀取不到資料時回退預設值 5%

---

## 四、UI 元件結構

### SystemParameterSettingsModal.razor

採用 Tab 分頁架構的 Modal 對話框：

- **基本設定 Tab**（BasicSettingsTab）：稅率輸入、備註欄位
- **簽核設定 Tab**（ApprovalSettingsTab）：三個區塊（採購流程、銷售流程、庫存流程），各含對應的 Switch 開關與狀態指示器

Modal Footer 目前包含：「取消」、「重整」（重新載入 DB 資料）、「儲存」三個按鈕。

---

## 五、種子資料

`SystemParameterSeeder.cs` 實作 `IDataSeeder`（Order = 2），在資料庫初始化時新增一筆預設記錄：

- TaxRate = 5.00m
- Status = Active
- Remarks = "系統預設稅率設定"
- 簽核開關全部為 false（Entity 預設值）

---

## 六、本次新增：恢復預設功能

### 設計動機

參考 Dashboard 模組已實作的「恢復預設」機制（`DashboardDefaults.cs` + `ResetPanelToDefaultAsync`），系統參數目前缺少將設定回復到出廠狀態的能力。使用者一旦修改後，只能手動逐欄改回，沒有一鍵恢復的途徑。

### 參考範本：Dashboard 恢復預設流程

```
使用者點擊「恢復預設」按鈕
    ↓
彈出 GenericConfirmModalComponent 確認對話框
    ↓
使用者確認
    ↓
呼叫 DashboardService.ResetPanelToDefaultAsync()
    ↓
從 DashboardDefaults.DefaultPanelDefinitions 讀取預設定義
    ↓
清除現有配置 → 依預設定義重建
    ↓
重新載入畫面 + 顯示成功通知
```

### 實作項目

#### 6-1. 新增 SystemParameterDefaults.cs

建立靜態類別，作為系統參數預設值的唯一來源（Single Source of Truth）：

```
位置：Data/Navigation/SystemParameterDefaults.cs（或 Constants 資料夾）

內容：
  - TaxRate = 5.00m
  - EnableQuotationApproval = false
  - EnablePurchaseOrderApproval = false
  - EnablePurchaseReceivingApproval = false
  - EnablePurchaseReturnApproval = false
  - EnableSalesOrderApproval = false
  - EnableSalesReturnApproval = false
  - EnableInventoryTransferApproval = false
  - DefaultRemarks = "系統預設稅率設定"
```

#### 6-2. 擴充 ISystemParameterService 介面

新增方法：

```
Task<OperationResult> ResetToDefaultAsync()
```

#### 6-3. 實作 SystemParameterService.ResetToDefaultAsync()

處理邏輯：

1. 取得現有的 SystemParameter 記錄
2. 將所有業務欄位覆寫為 `SystemParameterDefaults` 中的預設值
3. 保留不可重置欄位（Id、CreatedAt、CreatedBy）
4. 更新 UpdatedAt、UpdatedBy
5. 呼叫 `UpdateAsync()` 儲存
6. 呼叫 `ClearApprovalConfigCache()` 清除快取
7. 回傳 `OperationResult`

#### 6-4. 修改 SystemParameterSettingsModal.razor

在 Modal Footer 加入「恢復預設」按鈕，流程：

1. 按鈕樣式採用 `btn-outline-warning`，與「重整」、「儲存」區隔
2. 點擊後觸發 `HandleResetToDefault()` 方法
3. 彈出 `GenericConfirmModalComponent` 確認對話框
4. 確認訊息：「確定要將系統參數重置為預設配置嗎？現有的自訂配置將被清除。」
5. 使用者確認後呼叫 `SystemParameterService.ResetToDefaultAsync()`
6. 成功 → 重新載入表單資料 + `ShowSuccessAsync("已重置為預設配置")`
7. 失敗 → `ShowErrorAsync()` 顯示錯誤訊息

#### 6-5. 修改 SystemParameterSeeder.cs

改為從 `SystemParameterDefaults` 讀取預設值，確保種子資料與恢復預設共用同一組定義。

---

## 七、按鈕配置對照

修改後 Modal Footer 的按鈕配置：

| 按鈕 | 樣式 | 功能 | 確認機制 |
|------|------|------|---------|
| 取消 | btn-secondary | 關閉 Modal，不儲存 | 無 |
| 恢復預設 | btn-outline-warning | 所有欄位回復出廠值 | GenericConfirmModal |
| 重整 | btn-outline-info | 重新載入 DB 現有值 | 無 |
| 儲存 | btn-primary | 儲存當前表單至 DB | 無 |

---

## 八、資料流程圖

### 恢復預設完整流程

```
[使用者] 點擊「恢復預設」
    │
    ▼
[Modal] HandleResetToDefault()
    │
    ▼
[GenericConfirmModalComponent] 顯示確認對話框
    │
    ▼ 使用者確認
[Modal] ConfirmResetToDefault()
    │
    ▼
[SystemParameterService] ResetToDefaultAsync()
    │
    ├─ 讀取現有 SystemParameter
    ├─ 從 SystemParameterDefaults 取得預設值
    ├─ 覆寫業務欄位（保留 Id、CreatedAt、CreatedBy）
    ├─ UpdateAsync() 儲存至 DB
    └─ ClearApprovalConfigCache() 清除快取
    │
    ▼
[Modal] LoadSystemParameterAsync() 重新載入
    │
    ▼
[NotificationService] ShowSuccessAsync("已重置為預設配置")
```

### 既有儲存流程（不變）

```
[使用者] 修改表單欄位 → 點擊「儲存」
    │
    ▼
[Modal] HandleValidSubmit()
    │
    ▼
[SystemParameterService] UpdateAsync() / CreateAsync()
    │
    ▼
ClearApprovalConfigCache() → ShowSuccessAsync()
```

---

## 九、預設值維護原則

1. **單一來源**：所有預設值統一定義在 `SystemParameterDefaults.cs`，禁止在其他位置硬編碼
2. **Entity 預設值同步**：`SystemParameter.cs` 的屬性初始值應與 `SystemParameterDefaults` 保持一致
3. **Seeder 引用**：`SystemParameterSeeder.cs` 直接引用 `SystemParameterDefaults` 的常數
4. **新增欄位時**：同步更新 Entity、Defaults、Seeder、對應的 Tab 元件，以及 `ResetToDefaultAsync()` 邏輯
5. **快取一致性**：任何修改預設值的操作完成後，必須呼叫 `ClearApprovalConfigCache()`

---

## 十、檔案異動清單

| 檔案 | 異動類型 | 說明 |
|------|---------|------|
| SystemParameterDefaults.cs | 新增 | 預設值靜態類別 |
| ISystemParameterService.cs | 修改 | 新增 ResetToDefaultAsync 方法簽章 |
| SystemParameterService.cs | 修改 | 實作 ResetToDefaultAsync |
| SystemParameterSettingsModal.razor | 修改 | 新增恢復預設按鈕與確認流程 |
| SystemParameterSeeder.cs | 修改 | 改用 SystemParameterDefaults 取值 |
| SystemParameter.cs | 不變 | 既有 Entity 結構維持 |
| BasicSettingsTab.razor | 不變 | 既有 Tab 元件維持 |
| ApprovalSettingsTab.razor | 不變 | 既有 Tab 元件維持 |
