# 沖款單關聯方動態欄位實作

## 📋 概述

本文檔說明如何在沖款單編輯組件中實作根據沖款類型動態切換客戶/廠商欄位的功能。

## 🎯 需求分析

### 業務需求
- 當 `SetoffType` = **應收帳款** → 顯示**客戶** AutoComplete 欄位 + 客戶 EditModal
- 當 `SetoffType` = **應付帳款** → 顯示**廠商** AutoComplete 欄位 + 廠商 EditModal
- 支援透過 ActionButtons 快速新增/編輯客戶或廠商
- 選擇客戶或廠商後自動更新 `RelatedPartyId`、`RelatedPartyType`、`RelatedPartyName`

### 技術挑戰
- 原有架構中,每個 AutoComplete 欄位的 `ActionButtons` 只能綁定**單一** `ModalManager`
- 需要根據 `SetoffType` 的值動態切換使用哪個 `ModalManager`
- 需要保持與現有 GenericEditModalComponent 架構的一致性

## ✅ 選擇的解決方案

### 方案 1: 條件式欄位 (採用)

**核心思路:**
- 定義兩個獨立的 AutoComplete 欄位: `CustomerId` 和 `SupplierId`
- 每個欄位有獨立的 `ModalManager` (customerModalManager / supplierModalManager)
- 透過 `IsDisabled` 屬性根據 `SetoffType` 控制欄位是否可用
- 在 `SetoffDocument` 實體中新增兩個 `[NotMapped]` 計算屬性來支援表單綁定

**優點:**
- ✅ 符合現有架構設計
- ✅ 每個欄位有獨立的 ModalManager,邏輯清晰
- ✅ 實作簡單且可維護
- ✅ 型別安全
- ✅ 易於擴展

## 🔧 實作細節

### 1. SetoffDocument 實體擴展

在 `SetoffDocument.cs` 中新增兩個虛擬屬性:

```csharp
/// <summary>
/// 客戶ID (虛擬屬性,用於表單綁定)
/// </summary>
[NotMapped]
public int? CustomerId
{
    get => RelatedPartyType == "Customer" ? (RelatedPartyId > 0 ? RelatedPartyId : null) : null;
    set
    {
        if (value.HasValue && value.Value > 0)
        {
            RelatedPartyId = value.Value;
            RelatedPartyType = "Customer";
        }
    }
}

/// <summary>
/// 廠商ID (虛擬屬性,用於表單綁定)
/// </summary>
[NotMapped]
public int? SupplierId
{
    get => RelatedPartyType == "Supplier" ? (RelatedPartyId > 0 ? RelatedPartyId : null) : null;
    set
    {
        if (value.HasValue && value.Value > 0)
        {
            RelatedPartyId = value.Value;
            RelatedPartyType = "Supplier";
        }
    }
}
```

**設計說明:**
- 使用 `[NotMapped]` 標記,不會寫入資料庫
- Getter: 根據 `RelatedPartyType` 判斷是否返回 `RelatedPartyId`
- Setter: 自動設定 `RelatedPartyId` 和 `RelatedPartyType`

### 2. SetoffDocumentEditModalComponent 修改

#### 2.1 新增 Modal 管理器

```csharp
// 客戶編輯 Modal 相關變數
private CustomerEditModalComponent? customerEditModal;
private RelatedEntityModalManager<Customer> customerModalManager = default!;

// 廠商編輯 Modal 相關變數
private SupplierEditModalComponent? supplierEditModal;
private RelatedEntityModalManager<Supplier> supplierModalManager = default!;
```

#### 2.2 初始化 Modal 管理器

```csharp
private void InitializeCustomerModalManager()
{
    customerModalManager = new RelatedEntityManagerBuilder<Customer>(NotificationService, "客戶")
        .WithPropertyName("CustomerId")
        .WithReloadCallback(LoadAdditionalDataAsync)
        .WithStateChangedCallback(StateHasChanged)
        .WithAutoSelectCallback(customerId => 
        {
            if (editModalComponent?.Entity != null)
            {
                editModalComponent.Entity.RelatedPartyId = customerId;
                editModalComponent.Entity.RelatedPartyType = "Customer";
                
                // 更新 RelatedPartyName
                var customer = customers.FirstOrDefault(c => c.Id == customerId);
                if (customer != null)
                {
                    editModalComponent.Entity.RelatedPartyName = customer.CompanyName;
                }
            }
        })
        .WithCustomPostProcess(async customer => 
        {
            await InitializeFormFieldsAsync();
        })
        .Build();
}

private void InitializeSupplierModalManager()
{
    // 類似 customerModalManager 的實作
}
```

#### 2.3 表單欄位定義

```csharp
new()
{
    PropertyName = "CustomerId",
    Label = "客戶",
    FieldType = FormFieldType.AutoComplete,
    Placeholder = "請輸入或選擇客戶",
    MinSearchLength = 0,
    IsRequired = false,
    HelpText = "選擇應收帳款的客戶",
    ActionButtons = await GetCustomerActionButtonsAsync(),
    ContainerCssClass = "col-md-6",
    IsDisabled = editModalComponent?.Entity?.SetoffType != SetoffType.AccountsReceivable
},
new()
{
    PropertyName = "SupplierId",
    Label = "廠商",
    FieldType = FormFieldType.AutoComplete,
    Placeholder = "請輸入或選擇廠商",
    MinSearchLength = 0,
    IsRequired = false,
    HelpText = "選擇應付帳款的廠商",
    ActionButtons = await GetSupplierActionButtonsAsync(),
    ContainerCssClass = "col-md-6",
    IsDisabled = editModalComponent?.Entity?.SetoffType != SetoffType.AccountsPayable
}
```

**設計說明:**
- 使用 `IsDisabled` 而非 `IsVisible` 來保持表單佈局一致
- 當 SetoffType 不匹配時,欄位會被停用(顯示為灰色)

#### 2.4 AutoComplete 配置

```csharp
private Dictionary<string, IEnumerable<object>> GetAutoCompleteCollections()
{
    return new Dictionary<string, IEnumerable<object>>
    {
        { nameof(SetoffDocument.CompanyId), companies.Cast<object>() },
        { "CustomerId", customers.Cast<object>() },
        { "SupplierId", suppliers.Cast<object>() }
    };
}

private Dictionary<string, string> GetAutoCompleteDisplayProperties()
{
    return new Dictionary<string, string>
    {
        { nameof(SetoffDocument.CompanyId), "CompanyName" },
        { "CustomerId", "CompanyName" },
        { "SupplierId", "CompanyName" }
    };
}

private Dictionary<string, object> GetModalManagers()
{
    return new Dictionary<string, object>
    {
        { nameof(SetoffDocument.CompanyId), companyModalManager },
        { "CustomerId", customerModalManager },
        { "SupplierId", supplierModalManager }
    };
}
```

#### 2.5 欄位變更事件處理

```csharp
private async Task OnFieldValueChanged((string PropertyName, object? Value) fieldChange)
{
    try
    {
        // 當沖款類型變更時,清除關聯方資訊並重新初始化表單欄位
        if (fieldChange.PropertyName == nameof(SetoffDocument.SetoffType) && editModalComponent?.Entity != null)
        {
            editModalComponent.Entity.RelatedPartyId = 0;
            editModalComponent.Entity.RelatedPartyName = string.Empty;
            editModalComponent.Entity.RelatedPartyType = string.Empty;
            
            // 重新初始化表單欄位以更新客戶/廠商欄位的 IsDisabled 狀態
            await InitializeFormFieldsAsync();
            StateHasChanged();
        }
        
        // 處理客戶欄位變更
        if (fieldChange.PropertyName == "CustomerId" && editModalComponent?.Entity != null)
        {
            if (fieldChange.Value != null && int.TryParse(fieldChange.Value.ToString(), out int customerId))
            {
                editModalComponent.Entity.RelatedPartyId = customerId;
                editModalComponent.Entity.RelatedPartyType = "Customer";
                
                // 更新 RelatedPartyName
                var customer = customers.FirstOrDefault(c => c.Id == customerId);
                if (customer != null)
                {
                    editModalComponent.Entity.RelatedPartyName = customer.CompanyName;
                }
                
                // 更新 ActionButtons
                await ActionButtonHelper.UpdateFieldActionButtonsAsync(
                    customerModalManager, formFields, fieldChange.PropertyName, fieldChange.Value);
            }
        }
        
        // 處理廠商欄位變更 (類似客戶的邏輯)
    }
    catch (Exception)
    {
        // 忽略錯誤
    }
}
```

### 3. Razor 標記擴展

```razor
<CustomerEditModalComponent @ref="customerEditModal"
                           IsVisible="@customerModalManager.IsModalVisible"
                           IsVisibleChanged="@customerModalManager.HandleModalVisibilityChangedAsync"
                           CustomerId="@customerModalManager.SelectedEntityId"
                           OnCustomerSaved="@OnCustomerSavedWrapper"
                           OnCancel="@customerModalManager.HandleModalCancelAsync" />

<SupplierEditModalComponent @ref="supplierEditModal"
                           IsVisible="@supplierModalManager.IsModalVisible"
                           IsVisibleChanged="@supplierModalManager.HandleModalVisibilityChangedAsync"
                           SupplierId="@supplierModalManager.SelectedEntityId"
                           OnSupplierSaved="@OnSupplierSavedWrapper"
                           OnCancel="@supplierModalManager.HandleModalCancelAsync" />
```

## 🔄 執行流程

### 使用者操作流程

1. **選擇沖款類型**
   - 使用者選擇「應收帳款」或「應付帳款」
   - 觸發 `OnFieldValueChanged` 事件
   - 清除現有的關聯方資訊
   - 重新初始化表單欄位,更新 `IsDisabled` 狀態

2. **選擇客戶/廠商**
   - 根據沖款類型,對應的欄位變為可用
   - 使用者從 AutoComplete 選擇客戶或廠商
   - 觸發 `OnFieldValueChanged` 事件
   - 自動設定 `RelatedPartyId`、`RelatedPartyType`、`RelatedPartyName`

3. **使用 ActionButtons**
   - 點擊「新增」按鈕 → 開啟對應的 EditModal (客戶或廠商)
   - 儲存後自動選擇新建的實體
   - 點擊「編輯」按鈕 → 開啟編輯視窗
   - 點擊「清除」按鈕 → 清空選擇

### 資料同步流程

```
使用者選擇客戶
    ↓
CustomerId setter 被呼叫
    ↓
RelatedPartyId = customerId
RelatedPartyType = "Customer"
    ↓
OnFieldValueChanged 事件
    ↓
更新 RelatedPartyName
更新 ActionButtons
```

## 📊 資料結構

### SetoffDocument 資料欄位對應

| 欄位 | 型別 | 說明 | 範例 |
|------|------|------|------|
| `RelatedPartyId` | `int` | 關聯方ID (客戶或廠商的ID) | 123 |
| `RelatedPartyType` | `string` | 關聯方類型 | "Customer" 或 "Supplier" |
| `RelatedPartyName` | `string` | 關聯方名稱 (NotMapped) | "ABC公司" |
| `CustomerId` | `int?` | 客戶ID (NotMapped, 虛擬屬性) | 123 或 null |
| `SupplierId` | `int?` | 廠商ID (NotMapped, 虛擬屬性) | 456 或 null |

### 範例資料流

**應收帳款情境:**
```json
{
  "SetoffType": "AccountsReceivable",
  "RelatedPartyId": 123,
  "RelatedPartyType": "Customer",
  "RelatedPartyName": "ABC客戶公司",
  "CustomerId": 123,  // 虛擬屬性,從 RelatedPartyId 計算得出
  "SupplierId": null  // 虛擬屬性,因為 RelatedPartyType != "Supplier"
}
```

**應付帳款情境:**
```json
{
  "SetoffType": "AccountsPayable",
  "RelatedPartyId": 456,
  "RelatedPartyType": "Supplier",
  "RelatedPartyName": "XYZ廠商公司",
  "CustomerId": null,   // 虛擬屬性,因為 RelatedPartyType != "Customer"
  "SupplierId": 456     // 虛擬屬性,從 RelatedPartyId 計算得出
}
```

## ⚠️ 注意事項

### 1. IsDisabled vs IsVisible
- 使用 `IsDisabled` 而非 `IsVisible`
- 保持兩個欄位都在 DOM 中,避免佈局跳動
- 使用者可以看到所有欄位,但只能編輯對應的欄位

### 2. 虛擬屬性的限制
- `CustomerId` 和 `SupplierId` 只用於表單綁定
- 不會寫入資料庫 (標記為 `[NotMapped]`)
- 實際資料儲存在 `RelatedPartyId` 和 `RelatedPartyType`

### 3. ActionButtons 更新
- 當沖款類型變更時,需要重新初始化表單欄位
- 確保 ActionButtons 顯示正確的狀態 (新增/編輯/清除)

### 4. 資料驗證
- 在儲存前應驗證 `RelatedPartyId` 與 `RelatedPartyType` 的一致性
- 確保 SetoffType 與 RelatedPartyType 相符

## 🧪 測試建議

### 測試案例

1. **新增沖款單 - 應收帳款**
   - 選擇「應收帳款」
   - 確認客戶欄位可用,廠商欄位停用
   - 選擇客戶
   - 確認 RelatedPartyId、RelatedPartyType、RelatedPartyName 正確設定

2. **新增沖款單 - 應付帳款**
   - 選擇「應付帳款」
   - 確認廠商欄位可用,客戶欄位停用
   - 選擇廠商
   - 確認資料正確設定

3. **切換沖款類型**
   - 先選擇「應收帳款」並選擇客戶
   - 切換為「應付帳款」
   - 確認客戶資料被清除
   - 選擇廠商
   - 確認資料正確設定

4. **ActionButtons 功能**
   - 測試「新增」按鈕 → 開啟對應 Modal
   - 測試「編輯」按鈕 → 正確載入選擇的實體
   - 測試「清除」按鈕 → 清空選擇

5. **編輯現有沖款單**
   - 載入現有的應收帳款沖款單
   - 確認客戶欄位正確顯示
   - 載入現有的應付帳款沖款單
   - 確認廠商欄位正確顯示

## 📚 參考資料

- `CustomerEditModalComponent.razor` - 客戶編輯組件參考
- `ActionButtonHelper.cs` - ActionButtons 統一處理
- `RelatedEntityModalManager.cs` - Modal 管理器
- `GenericEditModalComponent.razor` - 通用編輯組件

## 🎓 學習要點

### 1. 虛擬屬性模式
- 使用計算屬性實現表單綁定與實際資料模型的分離
- `[NotMapped]` 標記避免 EF Core 嘗試映射虛擬欄位

### 2. 條件式表單欄位
- 透過 `IsDisabled` 實現動態欄位切換
- 保持一致的表單佈局體驗

### 3. Modal 管理器模式
- 每個 AutoComplete 欄位配對一個 ModalManager
- 使用 RelatedEntityManagerBuilder 建構器模式初始化

### 4. 統一的 ActionButtons 處理
- 使用 ActionButtonHelper 統一處理按鈕生成和更新
- 減少各個組件中的重複代碼

## 📝 更新歷史

| 日期 | 版本 | 說明 |
|------|------|------|
| 2025-10-06 | 1.0 | 初始版本 - 實作沖款單關聯方動態欄位功能 |

---

**文件建立日期:** 2025年10月6日  
**最後更新:** 2025年10月6日  
**維護人員:** GitHub Copilot  
**相關需求:** 沖款單編輯功能優化
