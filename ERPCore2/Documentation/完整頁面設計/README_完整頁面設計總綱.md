# 完整頁面設計總綱

## 更新日期
2026-02-17

---

## 概述

ERPCore2 採用**五層架構**實現完整的 CRUD 頁面功能。每個業務實體（Entity）遵循統一的設計模式，透過泛型元件與 Helper 類別大幅減少重複程式碼。

**核心原則：**
1. **優先使用 Helper** - 避免重複造輪子，所有通用邏輯已封裝
2. **配置驅動** - 透過 `FormFieldDefinition`、`FieldDefinition<T>` 等配置類別驅動 UI
3. **類型安全** - 使用 Lambda Expression（`e => e.Name`）而非字串
4. **統一錯誤處理** - 全系統使用 `ErrorHandlingHelper`

---

## 系統架構圖

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         五層架構總覽                                     │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ┌──────────────────────────────────────────────────────────────────┐  │
│  │ 第一層：Data 資料層                                               │  │
│  │ → BaseEntity 繼承、AppDbContext、Entity 定義、Migration           │  │
│  │ 📁 Data/Entities/, Data/Context/                                 │  │
│  └──────────────────────┬───────────────────────────────────────────┘  │
│                          ↓                                              │
│  ┌──────────────────────────────────────────────────────────────────┐  │
│  │ 第二層：Service 服務層                                             │  │
│  │ → GenericManagementService<T> 繼承、CRUD、驗證、ServiceResult     │  │
│  │ 📁 Services/                                                      │  │
│  └──────────────────────┬───────────────────────────────────────────┘  │
│                          ↓                                              │
│  ┌──────────────────────────────────────────────────────────────────┐  │
│  │ 第三層：FieldConfiguration 欄位配置層                              │  │
│  │ → BaseFieldConfiguration<T> 繼承、篩選/欄位定義                   │  │
│  │ 📁 Components/FieldConfiguration/                                 │  │
│  └──────────────────────┬───────────────────────────────────────────┘  │
│                          ↓                                              │
│  ┌──────────────────────────────────────────────────────────────────┐  │
│  │ 第四層：Index 列表頁面                                             │  │
│  │ → GenericIndexPageComponent<TEntity, TService> 搜尋/篩選/表格     │  │
│  │ 📁 Components/Pages/{Module}/{Entity}Index.razor                  │  │
│  └──────────────────────┬───────────────────────────────────────────┘  │
│                          ↓                                              │
│  ┌──────────────────────────────────────────────────────────────────┐  │
│  │ 第五層：EditModal 編輯表單                                         │  │
│  │ → GenericEditModalComponent<TEntity, TService> + GenericForm      │  │
│  │ → FormFieldDefinition、FormSectionHelper、Tab 佈局                │  │
│  │ 📁 Components/Pages/{Module}/{Entity}EditModalComponent.razor     │  │
│  └──────────────────────────────────────────────────────────────────┘  │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 文件導覽

本設計系統分為五份詳細文件，各司其職：

| # | 文件 | 說明 | 適用場景 |
|---|------|------|----------|
| 1 | [README_Data資料層設計.md](README_Data資料層設計.md) | Entity、BaseEntity、AppDbContext、Migration | 新增實體、定義資料結構 |
| 2 | [README_Service服務層設計.md](README_Service服務層設計.md) | GenericManagementService、ServiceResult、DI 註冊 | 新增服務、實作業務邏輯 |
| 3 | [README_Index頁面設計.md](README_Index頁面設計.md) | GenericIndexPageComponent、FieldConfiguration、篩選 | 建立列表頁面 |
| 4 | [README_EditModal設計.md](README_EditModal設計.md) | GenericEditModalComponent、Lazy Loading、Modal 管理器、列印整合、自訂 Tab | 建立編輯表單 |
| 5 | [README_FormField表單欄位設計.md](README_FormField表單欄位設計.md) | FormFieldDefinition、GenericFormComponent、Tab 佈局、自訂內容 Tab | 設計表單欄位與佈局 |

### 相關文件（其他位置）

| 文件 | 說明 |
|------|------|
| [README_SeedData管理.md](../專案架構/README_SeedData管理.md) | Seeder 種子資料管理系統 |
| [README_Helpers結構圖.md](../專案架構/README_Helpers結構圖.md) | Helper vs Component 的區別與使用原則 |

---

## 新增完整頁面快速指南

建立一個新的業務實體 CRUD 頁面，依序完成以下步驟：

### 步驟一覽

```
1. Data 資料層        → 建立 Entity、設定 DbContext、執行 Migration
2. Service 服務層     → 建立介面與實作、註冊 DI
3. FieldConfiguration → 定義篩選器與表格欄位
4. Index 頁面         → 建立列表頁面
5. EditModal          → 建立編輯表單（含 FormField 配置）
```

### 步驟 1：Data 資料層

```csharp
// Data/Entities/{Category}/YourEntity.cs
public class YourEntity : BaseEntity
{
    // Code 已由 BaseEntity 提供（string?, MaxLength 50）
    // 如需必填則加 [Required]

    [Required] [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    public int? RelatedEntityId { get; set; }
    public RelatedEntity? RelatedEntity { get; set; }
}
```

> 詳見 [README_Data資料層設計.md](README_Data資料層設計.md)

### 步驟 2：Service 服務層

```csharp
// Services/IYourEntityService.cs
public interface IYourEntityService : IGenericManagementService<YourEntity> { }

// Services/YourEntityService.cs
public class YourEntityService : GenericManagementService<YourEntity>, IYourEntityService
{
    public YourEntityService(IDbContextFactory<AppDbContext> contextFactory,
        ILogger<GenericManagementService<YourEntity>> logger) : base(contextFactory, logger) { }
}
```

> 詳見 [README_Service服務層設計.md](README_Service服務層設計.md)

### 步驟 3：FieldConfiguration

```csharp
// Components/FieldConfiguration/YourEntityFieldConfiguration.cs
public class YourEntityFieldConfiguration : BaseFieldConfiguration<YourEntity>
{
    public override Dictionary<string, FieldDefinition<YourEntity>> GetFieldDefinitions()
    {
        return new Dictionary<string, FieldDefinition<YourEntity>>
        {
            { nameof(YourEntity.Code), new FieldDefinition<YourEntity> { ... } }
        };
    }
}
```

> 詳見 [README_Index頁面設計.md](README_Index頁面設計.md)

### 步驟 4：Index 頁面

```razor
<GenericIndexPageComponent TEntity="YourEntity" TService="IYourEntityService"
    Service="@YourEntityService" PageTitle="實體管理" ... />
```

> 詳見 [README_Index頁面設計.md](README_Index頁面設計.md)

### 步驟 5：EditModal

```razor
<GenericEditModalComponent TEntity="YourEntity" TService="IYourEntityService"
    @bind-Id="@YourEntityId"
    FormFields="@GetFormFields()" FormSections="@formSections"
    TabDefinitions="@tabDefinitions"
    OnEntityLoaded="@HandleEntityLoaded" ... />
```

> 詳見 [README_EditModal設計.md](README_EditModal設計.md) 與 [README_FormField表單欄位設計.md](README_FormField表單欄位設計.md)

---

## 目錄結構總覽

```
Data/
├── Entities/                          # 實體定義（按模組分類）
│   ├── Commons/                       # 通用實體（AddressType, Currency 等）
│   ├── Customers/                     # 客戶相關
│   ├── Employees/                     # 員工相關
│   ├── Products/                      # 產品相關
│   ├── Purchase/                      # 採購相關
│   ├── Sales/                         # 銷售相關
│   └── ...
├── Context/
│   └── AppDbContext.cs                # EF Core 資料庫上下文
├── Enums/
│   └── CommonEnums.cs                 # 共用列舉（EntityStatus 等）
├── Navigation/
│   └── NavigationConfig.cs            # 導航選單配置
└── ServiceRegistration.cs             # DI 服務註冊

Services/
├── Interfaces/
│   └── IGenericManagementService.cs   # 泛型服務介面
├── GenericManagementService.cs        # 泛型服務基底類別
├── I{Entity}Service.cs               # 各實體服務介面
└── {Entity}Service.cs                 # 各實體服務實作

Components/
├── FieldConfiguration/                # 欄位配置（篩選 + 表格）
│   ├── BaseFieldConfiguration.cs      # 基底配置類別
│   ├── FieldDefinition.cs             # 欄位定義
│   ├── FormFieldConfigurationHelper.cs # 常用表單欄位 Helper
│   └── {Entity}FieldConfiguration.cs  # 各實體欄位配置
├── Shared/
│   ├── Page/
│   │   └── GenericIndexPageComponent.razor    # 通用列表頁面元件
│   ├── Modal/
│   │   ├── GenericEditModalComponent.razor    # 通用編輯 Modal 元件
│   │   └── BaseModalComponent.razor           # Modal 基底元件
│   └── UI/Form/
│       ├── GenericFormComponent.razor          # 通用表單元件
│       ├── GenericFormComponent.razor.cs       # 表單邏輯（code-behind）
│       ├── GenericFormComponent.razor.css      # 表單樣式
│       ├── FormFieldDefinition.cs             # 表單欄位定義
│       ├── FormConstants.cs                   # 常數定義
│       ├── FormTextField.razor                # 文字欄位子組件
│       ├── FormNumberField.razor              # 數字欄位子組件
│       ├── FormSelectField.razor              # 下拉選單子組件
│       ├── FormAutoCompleteField.razor        # 自動完成子組件
│       └── ...                                # 其他欄位子組件
└── Pages/{Module}/
    ├── {Entity}Index.razor                    # 各實體列表頁面
    └── {Entity}EditModalComponent.razor       # 各實體編輯 Modal

Helpers/
├── IndexHelpers/
│   ├── BreadcrumbHelper.cs            # 麵包屑導航
│   └── DataLoaderHelper.cs            # 資料載入
├── EditModal/
│   ├── FormSectionHelper.cs           # 表單區段 + Tab 定義
│   ├── ActionButtonHelper.cs          # 欄位操作按鈕
│   ├── AutoCompleteConfigHelper.cs    # AutoComplete 配置
│   ├── ModalManagerInitHelper.cs      # Modal 管理器初始化
│   ├── EntityCodeGenerationHelper.cs  # 實體編號生成
│   ├── FormFieldLockHelper.cs         # 表單欄位鎖定
│   ├── ApprovalConfigHelper.cs        # 審核流程配置
│   ├── TaxCalculationHelper.cs        # 稅額計算
│   ├── DocumentConversionHelper.cs    # 單據轉換
│   ├── PrefilledValueHelper.cs        # 預填值處理
│   ├── ChildDocumentRefreshHelper.cs  # 子文件刷新
│   └── CodeGenerationHelper.cs        # 編號生成邏輯
├── ErrorHandlingHelper.cs             # 統一錯誤處理
├── FilterHelper.cs                    # 篩選邏輯
├── ModalHelper.cs                     # Modal 狀態管理
├── CurrentUserHelper.cs               # 當前使用者
├── DependencyCheckHelper.cs           # 依賴關係檢查
├── EntityStatusHelper.cs              # 實體狀態管理
└── NumberFormatHelper.cs              # 數字格式化
```

---

## Helper 總覽

### IndexHelpers（`Helpers/IndexHelpers/`）

| Helper | 功能 | 使用時機 |
|--------|------|---------|
| BreadcrumbHelper | 麵包屑導航初始化 | 所有 Index 頁面 |
| DataLoaderHelper | 統一資料載入與錯誤處理 | 所有需要載入資料的頁面 |

### EditModal Helpers（`Helpers/EditModal/`）

| Helper | 功能 | 使用時機 |
|--------|------|---------|
| FormSectionHelper | 表單區段 + Tab 頁籤定義 | 所有 EditModal |
| ActionButtonHelper | 欄位操作按鈕產生與更新 | 有 AutoComplete 關聯實體的欄位 |
| AutoCompleteConfigHelper | AutoComplete 配置建立 | 所有 AutoComplete 欄位 |
| ModalManagerInitHelper | Modal 管理器初始化 | 有關聯實體編輯的 Modal |
| EntityCodeGenerationHelper | 實體編號生成（泛型） | 新增模式時自動產生編號 |
| FormFieldLockHelper | 表單欄位鎖定控制 | 需要根據狀態鎖定欄位 |
| ApprovalConfigHelper | 審核流程配置 | 需要審核機制的單據 |
| TaxCalculationHelper | 稅額計算 | 有稅額計算的單據 |
| DocumentConversionHelper | 單據轉換（A 單轉 B 單） | 轉單功能 |
| PrefilledValueHelper | 預填值處理 | 需要從其他實體複製值 |
| ChildDocumentRefreshHelper | 子文件刷新處理 | 有明細資料的主檔單據 |
| CodeGenerationHelper | 編號生成邏輯 | 需要自動產生編號的實體 |

### 通用 Helpers（`Helpers/`）

| Helper | 功能 | 使用時機 |
|--------|------|---------|
| ErrorHandlingHelper | 統一錯誤處理與記錄 | 所有需要錯誤處理的地方 |
| FilterHelper | 篩選邏輯處理 | FieldConfiguration 中定義篩選 |
| ModalHelper | Modal 狀態管理 | Index 頁面的 Modal 處理 |
| CurrentUserHelper | 當前使用者資訊 | 需要取得當前使用者 |
| DependencyCheckHelper | 依賴關係檢查 | 刪除前檢查是否有關聯資料 |
| EntityStatusHelper | 實體狀態管理 | 需要處理啟用/停用狀態 |
| NumberFormatHelper | 數字格式化 | 顯示金額、數量等數值 |

---

## 注意事項

1. **BaseEntity 屬性不要重複定義** - `Id`、`Code`、`Status`、`CreatedAt` 等已由基底類別提供
2. **Lazy Loading 模式** - EditModal 資料只在 `IsVisible = true` 時才載入
3. **使用 `IDbContextFactory`** - Blazor Server 必須使用 Factory 模式建立 DbContext
4. **非同步操作** - 所有資料存取使用 `async/await`
5. **安全回傳值** - `catch` 區塊回傳空列表、null 或 `ServiceResult.Failure()`
6. **Tab 佈局** - 使用 `FormSectionHelper.GroupIntoTab().BuildAll()` 啟用 Tab 模式
7. **自訂 Tab** - 使用 `GroupIntoCustomTab()` 嵌入子表格等非表單內容
8. **Column 分組** - 使用 `.AssignColumn(column, sectionNames)` 讓多個 Section 共用同一欄位，減少表單高度
8. **上下筆導航** - 使用 `@bind-Id` 和 `OnEntityLoaded` 確保資料同步
9. **巢狀 Modal** - 使用 `@if` 條件式渲染避免循環實例化
