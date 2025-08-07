# Modal 組件轉換指南

本指南說明如何將現有的 Edit UI 轉換為使用 `GenericEditModalComponent` 的新模式。

## 完整範例參考
- **Index 頁面**: `DepartmentIndex.razor`
- **Modal 組件**: `DepartmentEditModalComponent.razor`

## 轉換步驟

### 1. 新增 Modal 組件

在 `Components/Shared/Modals/` 目錄下新增 `{Entity}EditModalComponent.razor` 檔案。

#### 必要結構：

```razor
@* 可重用的{實體}編輯組件 - 可在任何頁面中嵌入 *@
@inject I{Entity}Service {Entity}Service
@inject INotificationService NotificationService

<GenericEditModalComponent TEntity="{Entity}" 
                          TService="I{Entity}Service"
                          @ref="editModalComponent"
                          IsVisible="@IsVisible"
                          IsVisibleChanged="@IsVisibleChanged"
                          Id="@{Entity}Id"
                          Service="@{Entity}Service"
                          EntityName="{實體中文名稱}"
                          EntityNamePlural="{實體中文名稱}"
                          ModalTitle="@({Entity}Id.HasValue ? "編輯{實體中文名稱}" : "新增{實體中文名稱}")"
                          Size="GenericEditModalComponent<{Entity}, I{Entity}Service>.ModalSize.Desktop"
                          UseGenericForm="true"
                          FormFields="@GetFormFields()"
                          FormSections="@formSections"
                          DataLoader="@Load{Entity}Data"
                          AdditionalDataLoader="@LoadAdditionalDataAsync"
                          UseGenericSave="true"
                          SaveSuccessMessage="@({Entity}Id.HasValue ? "{實體中文名稱}更新成功" : "{實體中文名稱}新增成功")"
                          SaveFailureMessage="{實體中文名稱}儲存失敗"
                          RequiredPermission="{Entity}.Read"
                          OnSaveSuccess="@HandleSaveSuccess"
                          OnCancel="@HandleCancel" />

@code {
    // ===== 必要參數 =====
    [Parameter] public bool IsVisible { get; set; } = false;
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
    [Parameter] public int? {Entity}Id { get; set; }
    [Parameter] public EventCallback<{Entity}> On{Entity}Saved { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }

    // ===== 內部狀態 =====
    private GenericEditModalComponent<{Entity}, I{Entity}Service>? editModalComponent;
    private List<FormFieldDefinition> formFields = new();
    private Dictionary<string, string> formSections = new();
    
    // 其他需要的變數...

    // ===== 必要方法 =====
    protected override async Task OnParametersSetAsync() { ... }
    private async Task HandleSaveSuccess() { ... }
    private async Task HandleCancel() { ... }
    private async Task CloseModal() { ... }
    private async Task Load{Entity}Data() { ... }
    private async Task LoadAdditionalDataAsync() { ... }
    private void InitializeFormFields() { ... }
    private List<FormFieldDefinition> GetFormFields() { ... }
}
```

#### 關鍵實作要點：

1. **表單欄位定義** (`InitializeFormFields`)：
   ```csharp
   formFields = new List<FormFieldDefinition>
   {
       new()
       {
           PropertyName = nameof({Entity}.PropertyName),
           Label = "欄位標籤",
           FieldType = FormFieldType.Text, // Text, Select, TextArea, Date 等
           Placeholder = "請輸入...",
           IsRequired = true,
           HelpText = "說明文字"
       }
   };
   ```

2. **表單區段定義** (`formSections`)：
   ```csharp
   formSections = new Dictionary<string, string>
   {
       { nameof({Entity}.Property1), "基本資訊" },
       { nameof({Entity}.Property2), "基本資訊" },
       { nameof({Entity}.Property3), "額外資料" }
   };
   ```

3. **資料載入方法**：
   ```csharp
   private async Task<{Entity}?> Load{Entity}Data()
   {
       if (!{Entity}Id.HasValue) 
       {
           // 新增模式
           return new {Entity}
           {
               // 預設值設定
               Status = EntityStatus.Active
           };
       }
       
       // 編輯模式
       return await {Entity}Service.GetByIdAsync({Entity}Id.Value);
   }
   ```

### 2. 修改 Index 頁面

#### 需要修改的部分：

1. **Modal 組件引用**：
   ```razor
   @* 將舊的 Modal 替換為新的組件 *@
   <{Entity}EditModalComponent IsVisible="@showEditModal"
                              IsVisibleChanged="@((bool visible) => showEditModal = visible)"
                              {Entity}Id="@editing{Entity}Id"
                              On{Entity}Saved="@On{Entity}Saved"
                              OnCancel="@OnModalCancel" />
   ```

2. **Modal 狀態變數**：
   ```csharp
   // Modal 相關狀態
   private bool showEditModal = false;
   private int? editing{Entity}Id = null;
   ```

3. **Modal 相關方法**：
   ```csharp
   private async Task ShowAddModal()
   {
       editing{Entity}Id = null;
       showEditModal = true;
       StateHasChanged();
   }

   private async Task ShowEditModal({Entity} entity)
   {
       if (entity?.Id != null)
       {
           editing{Entity}Id = entity.Id;
           showEditModal = true;
           StateHasChanged();
       }
   }

   private async Task On{Entity}Saved({Entity} saved{Entity})
   {
       showEditModal = false;
       editing{Entity}Id = null;
       
       if (indexComponent != null)
       {
           await indexComponent.Refresh();
       }
       
       StateHasChanged();
   }

   private async Task OnModalCancel()
   {
       showEditModal = false;
       editing{Entity}Id = null;
       StateHasChanged();
   }
   ```

### 3. 需要刪除的舊檔案

- 移除舊的 Modal 組件檔案（如果有獨立的 Modal 檔案）
- 移除舊的 Edit 頁面檔案（如果使用獨立頁面編輯）

### 4. 常見的表單欄位類型

```csharp
// 文字輸入
new() {
    PropertyName = nameof(Entity.Name),
    FieldType = FormFieldType.Text,
    IsRequired = true
}

// 下拉選單
new() {
    PropertyName = nameof(Entity.CategoryId),
    FieldType = FormFieldType.Select,
    Options = categoryOptions // List<SelectOption>
}

// 文字區域
new() {
    PropertyName = nameof(Entity.Description),
    FieldType = FormFieldType.TextArea,
    Rows = 3
}

// 日期選擇
new() {
    PropertyName = nameof(Entity.Date),
    FieldType = FormFieldType.Date
}

// 數字輸入
new() {
    PropertyName = nameof(Entity.Amount),
    FieldType = FormFieldType.Number
}
```

## 優點

1. **統一的UI風格**：所有 Modal 使用相同的樣式和佈局
2. **減少重複程式碼**：共用驗證、儲存、錯誤處理邏輯
3. **易於維護**：核心邏輯集中在 `GenericEditModalComponent`
4. **彈性擴展**：可透過參數客製化不同需求
5. **權限控制**：內建權限檢查機制

## 注意事項

1. 確保 Service 介面有實作必要的方法（GetByIdAsync, CreateAsync, UpdateAsync）
2. 實體類別需要繼承 `BaseEntity`
3. 表單欄位的 `PropertyName` 必須與實體屬性名稱完全一致
4. 記得在 `_Imports.razor` 中包含必要的命名空間

## 疑難排解

1. **組件找不到**：檢查命名空間是否正確引入
2. **表單欄位沒有顯示**：確認 `PropertyName` 拼寫正確
3. **儲存失敗**：檢查 Service 方法是否正確實作
4. **權限錯誤**：確認 `RequiredPermission` 設定正確

## 聯絡資訊

如有問題請參考完整範例或聯絡開發團隊。
