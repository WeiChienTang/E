# 移除全部 ActionButton 指南

本指南以 `SetoffDocumentEditModalComponent.razor` 為參考範本，記錄從 EditModal 組件中完整移除 ActionButton 功能的標準步驟。

---

## 需要移除的項目清單

### 1. `@inject` 宣告

移除 `ActionButtonHelper` 的 inject：

```diff
- @inject ActionButtonHelper ActionButtonHelper
```

---

### 2. HTML 區塊：三個關聯實體的 EditModalComponent

移除公司、客戶（或廠商）的子 Modal 組件，這些是 ActionButton 點擊後開啟的編輯視窗：

```diff
- <CompanyEditModalComponent @ref="companyEditModal"
-                           IsVisible="@companyModalManager.IsModalVisible"
-                           IsVisibleChanged="@companyModalManager.HandleModalVisibilityChangedAsync"
-                           CompanyId="@companyModalManager.SelectedEntityId"
-                           OnCompanySaved="@companyModalManager.OnSavedAsync"
-                           OnCancel="@companyModalManager.HandleModalCancelAsync" />
-
- <CustomerEditModalComponent @ref="customerEditModal"
-                            IsVisible="@customerModalManager.IsModalVisible"
-                            IsVisibleChanged="@customerModalManager.HandleModalVisibilityChangedAsync"
-                            CustomerId="@customerModalManager.SelectedEntityId"
-                            OnCustomerSaved="@customerModalManager.OnSavedAsync"
-                            OnCancel="@customerModalManager.HandleModalCancelAsync" />
-
- <SupplierEditModalComponent @ref="supplierEditModal"
-                            IsVisible="@supplierModalManager.IsModalVisible"
-                            IsVisibleChanged="@supplierModalManager.HandleModalVisibilityChangedAsync"
-                            SupplierId="@supplierModalManager.SelectedEntityId"
-                            OnSupplierSaved="@supplierModalManager.OnSavedAsync"
-                            OnCancel="@supplierModalManager.HandleModalCancelAsync" />
```

> 各檔案的子 Modal 組件名稱不同，請依實際內容調整（例如 ProductEditModalComponent、WarehouseEditModalComponent 等）。

---

### 3. `GenericEditModalComponent` 的 `ModalManagers` 參數

```diff
  <GenericEditModalComponent ...
-                           ModalManagers="@modalManagers?.AsDictionary()"
                            DataLoader="@LoadXxxData"
```

---

### 4. `@code` 區塊：私有欄位

移除所有 Modal Manager 相關的私有欄位：

```diff
- // 公司編輯 Modal 相關變數 - 使用泛型管理器
- private CompanyEditModalComponent? companyEditModal;
- private RelatedEntityModalManager<Company> companyModalManager = default!;
-
- // 客戶編輯 Modal 相關變數 - 使用泛型管理器
- private CustomerEditModalComponent? customerEditModal;
- private RelatedEntityModalManager<Customer> customerModalManager = default!;
-
- // 廠商編輯 Modal 相關變數 - 使用泛型管理器
- private SupplierEditModalComponent? supplierEditModal;
- private RelatedEntityModalManager<Supplier> supplierModalManager = default!;
-
- private ModalManagerCollection modalManagers = default!;
```

> 欄位名稱依各檔案實際內容調整。

---

### 5. `OnInitializedAsync` 方法

若 `OnInitializedAsync` 的內容只有 `ModalManagerInitHelper.CreateBuilder(...)` 相關程式碼，則整個方法都可移除：

```diff
- protected override async Task OnInitializedAsync()
- {
-     try
-     {
-         modalManagers = ModalManagerInitHelper.CreateBuilder<XxxEntity, IXxxService>(
-                 () => editModalComponent,
-                 NotificationService,
-                 StateHasChanged,
-                 LoadAdditionalDataAsync,
-                 InitializeFormFieldsAsync)
-             .AddManager<Company>(nameof(XxxEntity.CompanyId), "公司")
-             .AddManager<Customer>("CustomerId", "客戶")
-             .AddManager<Supplier>("SupplierId", "廠商")
-             .Build();
-         companyModalManager = modalManagers.Get<Company>(nameof(XxxEntity.CompanyId));
-         customerModalManager = modalManagers.Get<Customer>("CustomerId");
-         supplierModalManager = modalManagers.Get<Supplier>("SupplierId");
-     }
-     catch (Exception)
-     {
-         await NotificationService.ShowErrorAsync("初始化...編輯組件時發生錯誤");
-     }
- }
```

> 若 `OnInitializedAsync` 除了 ModalManager 初始化之外還有其他邏輯，則只移除 ModalManager 相關的部分，保留其餘程式碼。

---

### 6. `OnAfterRenderAsync` 方法（若存在）

若 `OnAfterRenderAsync` 是為了修正 ActionButton 首次渲染問題而加入的，整個移除：

```diff
- protected override async Task OnAfterRenderAsync(bool firstRender)
- {
-     if (firstRender && IsVisible && isDataLoaded)
-     {
-         await InitializeFormFieldsAsync();
-         StateHasChanged();
-     }
- }
```

---

### 7. `InitializeFormFieldsAsync` 中的 `ActionButtons` 屬性

移除每個 `FormFieldDefinition` 中的 `ActionButtons` 屬性：

```diff
  formFields.Add(new()
  {
      PropertyName = "CustomerId",
      Label = "客戶",
      FieldType = FormFieldType.AutoComplete,
      IsRequired = true,
      HelpText = "選擇客戶",
-     ActionButtons = GetCustomerActionButtonsAsync()   // 或 GetCustomerActionButtons()
  });
```

---

### 8. `OnFieldValueChanged` 中的 ActionButton 更新邏輯

移除每個欄位變更區塊中，用來更新 ActionButton 的程式碼片段：

```diff
  if (fieldChange.PropertyName == "CustomerId" && editModalComponent?.Entity != null)
  {
      if (fieldChange.Value != null && int.TryParse(..., out int customerId))
      {
          editModalComponent.Entity.RelatedPartyId = customerId;
          editModalComponent.Entity.RelatedPartyType = "Customer";
          var customer = customers.FirstOrDefault(c => c.Id == customerId);
          if (customer != null)
              editModalComponent.Entity.RelatedPartyName = customer.CompanyName ?? "";
-
-         // 更新 ActionButtons
-         await ActionButtonHelper.UpdateFieldActionButtonsAsync(
-             customerModalManager, formFields, fieldChange.PropertyName, fieldChange.Value);
-         // 或：
-         var customerField = formFields.FirstOrDefault(f => f.PropertyName == "CustomerId");
-         if (customerField != null)
-         {
-             customerField.ActionButtons = GetCustomerActionButtons();
-             StateHasChanged();
-         }
      }
  }
```

另外，若整個 if 區塊只有 ActionButton 更新（例如公司欄位只做按鈕更新，沒有其他邏輯），則整個 if 區塊都移除：

```diff
- // 使用統一 Helper 處理公司欄位變更
- if (fieldChange.PropertyName == nameof(XxxEntity.CompanyId))
- {
-     await ActionButtonHelper.UpdateFieldActionButtonsAsync(
-         companyModalManager, formFields, fieldChange.PropertyName, fieldChange.Value);
- }
```

---

### 9. ActionButton 方法區塊

移除 `// ===== Action Buttons =====` 區塊及其中所有方法：

```diff
- // ===== Action Buttons =====
- private async Task<List<FieldActionButton>> GetCompanyActionButtonsAsync()
- {
-     return await ActionButtonHelper.GenerateFieldActionButtonsAsync(
-         editModalComponent,
-         companyModalManager,
-         nameof(XxxEntity.CompanyId)
-     );
- }
-
- private Task<List<FieldActionButton>> GetCustomerActionButtonsAsync()  // 或 sync 版本
- {
-     int? currentCustomerId = null;
-     if (editModalComponent?.Entity?.RelatedPartyType == "Customer")
-         currentCustomerId = editModalComponent.Entity.RelatedPartyId > 0
-             ? editModalComponent.Entity.RelatedPartyId : (int?)null;
-     return Task.FromResult(customerModalManager.GenerateActionButtons(currentCustomerId));
- }
-
- private Task<List<FieldActionButton>> GetSupplierActionButtonsAsync()
- {
-     ...
- }
```

---

### 10. `OnFieldValueChanged` 方法簽章調整

移除所有 `await` 後，若方法不再使用 `async`，需調整簽章並加上 `return Task.CompletedTask`：

```diff
- private async Task OnFieldValueChanged((string PropertyName, object? Value) fieldChange)
+ private Task OnFieldValueChanged((string PropertyName, object? Value) fieldChange)
  {
      try { ... }
      catch (Exception) { }
+
+     return Task.CompletedTask;
  }
```

> 若方法內仍有其他 `await`（例如廠商/客戶選擇後需非同步處理），則保留 `async Task`，不需加 `return Task.CompletedTask`。

---

## 完成後的檢查清單

- [ ] 專案可正常編譯（無參考 `ActionButtonHelper`、`modalManagers`、`companyModalManager` 等未定義變數的錯誤）
- [ ] 移除 `ActionButtonHelper` inject 後，確認其他地方沒有使用到
- [ ] 確認子 Modal 組件（CompanyEditModal 等）已從 HTML 區塊移除
- [ ] 確認 `GenericEditModalComponent` 的 `ModalManagers` 參數已移除
- [ ] 確認 `InitializeFormFieldsAsync` 中沒有殘留 `ActionButtons = ...` 賦值
- [ ] 確認 `OnFieldValueChanged` 中沒有殘留 ActionButton 更新程式碼
- [ ] 確認 `OnInitializedAsync` 若變空可整個移除
- [ ] 確認 `OnAfterRenderAsync` 若只為 ActionButton 而存在可整個移除

---

## 注意事項

1. **不需移除** `customers`、`suppliers`、`companies` 等選項清單，AutoComplete 欄位仍需這些資料。
2. **不需移除** `LoadAdditionalDataAsync` 方法，該方法也負責載入 AutoComplete 所需的資料。
3. **不需移除** `OnFieldValueChanged` 中對 `RelatedPartyId`、`RelatedPartyType`、`RelatedPartyName` 的賦值邏輯，這些是業務邏輯的一部分。
4. 若某個欄位的 `ActionButtons` 是用 `await` 呼叫（`async`），移除後需重新確認 `InitializeFormFieldsAsync` 是否還需要 `async` 關鍵字（看 catch 區塊是否有 `await`）。
