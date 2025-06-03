# IndustryType Services 功能說明

## 📋 概述

IndustryType 服務模組負責管理行業類型的完整生命週期，包括建立、查詢、更新、刪除等操作。此服務繼承自 `GenericManagementService<IndustryType>`，提供標準化的 CRUD 操作和業務特定功能。

## 🏗️ 架構概述

### 服務繼承結構
```
GenericManagementService<IndustryType>
└── IndustryTypeService
```

### 核心元件
- **IndustryTypeService**: 主要服務實作，提供行業類型管理功能
- **IIndustryTypeService**: 服務介面，定義行業類型服務契約
- **IndustryType**: 行業類型實體，包含名稱、代碼等屬性

## 📂 檔案結構

```
Services/Industries/
├── IndustryTypeService.cs           # 主要服務實作
├── Interfaces/
│   └── IIndustryTypeService.cs      # 服務介面定義
└── Readme_IndustryType.md          # 本文檔
```

## 🔧 依賴注入設定

在 `Program.cs` 或 `ServiceCollectionExtensions` 中註冊：

```csharp
// 註冊 IndustryType 服務
builder.Services.AddScoped<IIndustryTypeService, IndustryTypeService>();
```

## 📖 IndustryTypeService 類別詳細說明

### 建構函式

```csharp
public IndustryTypeService(AppDbContext context, ILogger<IndustryTypeService> logger) : base(context)
```

**參數:**
- `context`: 資料庫上下文
- `logger`: 日誌記錄器

### 繼承的基底方法 (來自 GenericManagementService)

#### 基本 CRUD 操作

| 方法 | 回傳型別 | 說明 |
|------|----------|------|
| `GetAllAsync()` | `Task<List<IndustryType>>` | 取得所有未刪除的行業類型 |
| `GetActiveAsync()` | `Task<List<IndustryType>>` | 取得所有啟用的行業類型 |
| `GetByIdAsync(int id)` | `Task<IndustryType?>` | 根據 ID 取得行業類型 |
| `CreateAsync(IndustryType entity)` | `Task<ServiceResult<IndustryType>>` | 建立新行業類型 |
| `UpdateAsync(IndustryType entity)` | `Task<ServiceResult<IndustryType>>` | 更新行業類型 |
| `DeleteAsync(int id)` | `Task<ServiceResult>` | 刪除行業類型（軟刪除） |

#### 批次操作

| 方法 | 回傳型別 | 說明 |
|------|----------|------|
| `CreateBatchAsync(List<IndustryType> entities)` | `Task<ServiceResult<List<IndustryType>>>` | 批次建立行業類型 |
| `UpdateBatchAsync(List<IndustryType> entities)` | `Task<ServiceResult<List<IndustryType>>>` | 批次更新行業類型 |
| `DeleteBatchAsync(List<int> ids)` | `Task<ServiceResult>` | 批次刪除行業類型 |

#### 查詢操作

| 方法 | 回傳型別 | 說明 |
|------|----------|------|
| `GetPagedAsync(int pageNumber, int pageSize, string? searchTerm)` | `Task<(List<IndustryType> Items, int TotalCount)>` | 分頁查詢（含搜尋） |
| `SearchAsync(string searchTerm)` | `Task<List<IndustryType>>` | 搜尋行業類型 |
| `ExistsAsync(int id)` | `Task<bool>` | 檢查是否存在 |
| `GetCountAsync()` | `Task<int>` | 取得總數 |

#### 狀態管理

| 方法 | 回傳型別 | 說明 |
|------|----------|------|
| `SetStatusAsync(int id, EntityStatus status)` | `Task<ServiceResult>` | 設定特定狀態 |
| `ToggleStatusAsync(int id)` | `Task<ServiceResult>` | 切換狀態 (Active ↔ Inactive) |
| `SetStatusBatchAsync(List<int> ids, EntityStatus status)` | `Task<ServiceResult>` | 批次設定狀態 |

### 覆寫的基底方法

#### GetAllAsync()
```csharp
public override async Task<List<IndustryType>> GetAllAsync()
```
**功能**: 取得所有未刪除的行業類型，按名稱排序
**回傳**: 行業類型清單
**特殊邏輯**: 按 `IndustryTypeName` 升序排列

#### SearchAsync(string searchTerm)
```csharp
public override async Task<List<IndustryType>> SearchAsync(string searchTerm)
```
**功能**: 根據搜尋條件查詢行業類型
**參數**: `searchTerm` - 搜尋關鍵字
**回傳**: 符合條件的行業類型清單
**搜尋範圍**: 行業類型名稱、行業類型代碼
**特殊邏輯**: 不區分大小寫搜尋

#### ValidateAsync(IndustryType entity)
```csharp
public override async Task<ServiceResult> ValidateAsync(IndustryType entity)
```
**功能**: 驗證行業類型資料
**參數**: `entity` - 要驗證的行業類型實體
**回傳**: 驗證結果

**驗證規則**:
- ✅ 名稱為必填欄位
- ✅ 名稱長度不超過 100 字元
- ✅ 代碼長度不超過 10 字元（可選）
- ✅ 名稱不重複
- ✅ 代碼不重複（如果有提供）

#### DeleteAsync(int id)
```csharp
public override async Task<ServiceResult> DeleteAsync(int id)
```
**功能**: 刪除行業類型（軟刪除）
**參數**: `id` - 行業類型 ID
**回傳**: 操作結果
**特殊邏輯**: 檢查是否有關聯的客戶，如果有則禁止刪除

#### CanDeleteAsync(IndustryType entity)
```csharp
protected override async Task<ServiceResult> CanDeleteAsync(IndustryType entity)
```
**功能**: 檢查是否可以刪除
**參數**: `entity` - 行業類型實體
**回傳**: 檢查結果
**檢查邏輯**: 驗證是否有客戶使用此行業類型

### 業務特定方法

#### IsNameExistsAsync(string name, int? excludeId = null)
```csharp
public override async Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
```
**功能**: 檢查行業類型名稱是否存在
**參數**: 
- `name` - 要檢查的名稱
- `excludeId` - 排除的 ID（用於更新時檢查）
**回傳**: 是否存在

#### IsIndustryTypeNameExistsAsync(string industryTypeName, int? excludeId = null)
```csharp
public async Task<bool> IsIndustryTypeNameExistsAsync(string industryTypeName, int? excludeId = null)
```
**功能**: 檢查行業類型名稱是否存在（業務方法別名）
**參數**: 
- `industryTypeName` - 要檢查的名稱
- `excludeId` - 排除的 ID
**回傳**: 是否存在

#### IsIndustryTypeCodeExistsAsync(string industryTypeCode, int? excludeId = null)
```csharp
public async Task<bool> IsIndustryTypeCodeExistsAsync(string industryTypeCode, int? excludeId = null)
```
**功能**: 檢查行業類型代碼是否存在
**參數**: 
- `industryTypeCode` - 要檢查的代碼
- `excludeId` - 排除的 ID
**回傳**: 是否存在

#### GetPagedAsync(int pageNumber, int pageSize)
```csharp
public async Task<(List<IndustryType> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
```
**功能**: 分頁查詢（無搜尋條件）
**參數**: 
- `pageNumber` - 頁碼
- `pageSize` - 每頁大小
**回傳**: 分頁結果和總數

## 🎯 使用範例

### 基本使用

```csharp
[Inject] IIndustryTypeService IndustryTypeService { get; set; }

// 取得所有行業類型
var allIndustryTypes = await IndustryTypeService.GetAllAsync();

// 取得啟用的行業類型
var activeIndustryTypes = await IndustryTypeService.GetActiveAsync();

// 根據 ID 取得行業類型
var industryType = await IndustryTypeService.GetByIdAsync(1);
```

### 建立新行業類型

```csharp
var newIndustryType = new IndustryType
{
    IndustryTypeName = "資訊科技",
    IndustryTypeCode = "IT"
};

var result = await IndustryTypeService.CreateAsync(newIndustryType);
if (result.IsSuccess)
{
    Console.WriteLine($"成功建立行業類型: {result.Data.IndustryTypeName}");
}
else
{
    Console.WriteLine($"建立失敗: {result.ErrorMessage}");
}
```

### 更新行業類型

```csharp
var industryType = await IndustryTypeService.GetByIdAsync(1);
if (industryType != null)
{
    industryType.IndustryTypeName = "資訊技術服務";
    var result = await IndustryTypeService.UpdateAsync(industryType);
    
    if (result.IsSuccess)
    {
        Console.WriteLine("更新成功");
    }
}
```

### 刪除行業類型

```csharp
var result = await IndustryTypeService.DeleteAsync(1);
if (result.IsSuccess)
{
    Console.WriteLine("刪除成功");
}
else
{
    Console.WriteLine($"刪除失敗: {result.ErrorMessage}");
}
```

### 搜尋功能

```csharp
// 搜尋包含 "科技" 的行業類型
var searchResults = await IndustryTypeService.SearchAsync("科技");

// 分頁搜尋
var (items, totalCount) = await IndustryTypeService.GetPagedAsync(1, 10, "服務");
```

### 驗證功能

```csharp
// 檢查名稱是否存在
var nameExists = await IndustryTypeService.IsIndustryTypeNameExistsAsync("資訊科技");

// 檢查代碼是否存在
var codeExists = await IndustryTypeService.IsIndustryTypeCodeExistsAsync("IT");

// 更新時排除自己檢查
var nameExistsForUpdate = await IndustryTypeService.IsIndustryTypeNameExistsAsync("資訊科技", excludeId: 1);
```

### 批次操作

```csharp
// 批次建立
var industryTypes = new List<IndustryType>
{
    new() { IndustryTypeName = "製造業", IndustryTypeCode = "MFG" },
    new() { IndustryTypeName = "服務業", IndustryTypeCode = "SVC" }
};

var batchResult = await IndustryTypeService.CreateBatchAsync(industryTypes);

// 批次刪除
var idsToDelete = new List<int> { 1, 2, 3 };
var deleteResult = await IndustryTypeService.DeleteBatchAsync(idsToDelete);
```

### 狀態管理

```csharp
// 設定為非啟用狀態
await IndustryTypeService.SetStatusAsync(1, EntityStatus.Inactive);

// 切換狀態
await IndustryTypeService.ToggleStatusAsync(1);

// 批次設定狀態
var ids = new List<int> { 1, 2, 3 };
await IndustryTypeService.SetStatusBatchAsync(ids, EntityStatus.Active);
```

## 🔍 錯誤處理

### 常見錯誤訊息

| 錯誤訊息 | 原因 | 解決方案 |
|----------|------|----------|
| "行業類型名稱為必填" | 名稱欄位為空或空白 | 提供有效的名稱 |
| "行業類型名稱不可超過100個字元" | 名稱長度超限 | 縮短名稱長度 |
| "行業類型代碼不可超過10個字元" | 代碼長度超限 | 縮短代碼長度 |
| "行業類型名稱已存在" | 名稱重複 | 使用不同的名稱 |
| "行業類型代碼已存在" | 代碼重複 | 使用不同的代碼 |
| "無法刪除，此行業類型已被客戶使用" | 有關聯的客戶資料 | 先更新或刪除相關客戶 |
| "找不到要刪除的行業類型" | 指定的 ID 不存在 | 確認 ID 正確性 |

### 異常處理模式

```csharp
try
{
    var result = await IndustryTypeService.CreateAsync(industryType);
    if (!result.IsSuccess)
    {
        // 處理業務邏輯錯誤
        LogError(result.ErrorMessage);
        ShowUserMessage(result.ErrorMessage);
    }
}
catch (Exception ex)
{
    // 處理系統層級異常
    LogException(ex);
    ShowUserMessage("系統發生錯誤，請稍後再試");
}
```

## 🧪 測試建議

### 單元測試

```csharp
[Test]
public async Task CreateAsync_ValidIndustryType_ShouldSucceed()
{
    // Arrange
    var industryType = new IndustryType
    {
        IndustryTypeName = "測試行業",
        IndustryTypeCode = "TEST"
    };

    // Act
    var result = await _industryTypeService.CreateAsync(industryType);

    // Assert
    Assert.IsTrue(result.IsSuccess);
    Assert.IsNotNull(result.Data);
}

[Test]
public async Task CreateAsync_DuplicateName_ShouldFail()
{
    // Arrange & Act
    var result = await _industryTypeService.CreateAsync(new IndustryType 
    { 
        IndustryTypeName = "已存在的名稱" 
    });

    // Assert
    Assert.IsFalse(result.IsSuccess);
    Assert.Contains("已存在", result.ErrorMessage);
}
```

### 整合測試

```csharp
[Test]
public async Task DeleteAsync_WithRelatedCustomers_ShouldFail()
{
    // Arrange
    var industryTypeId = await CreateIndustryTypeWithCustomer();

    // Act
    var result = await _industryTypeService.DeleteAsync(industryTypeId);

    // Assert
    Assert.IsFalse(result.IsSuccess);
    Assert.Contains("已被客戶使用", result.ErrorMessage);
}
```

## 📊 效能考量

### 查詢優化
- 資料庫索引建議：在 `IndustryTypeName` 和 `IndustryTypeCode` 欄位建立索引
- 大量資料時使用分頁查詢
- 搜尋時避免使用 `Contains` 開頭的模糊查詢

### 快取策略
- 考慮對經常查詢的行業類型清單實作快取
- 使用 `IMemoryCache` 或 `IDistributedCache`

```csharp
// 快取實作範例
public async Task<List<IndustryType>> GetActiveWithCacheAsync()
{
    const string cacheKey = "ActiveIndustryTypes";
    
    if (_cache.TryGetValue(cacheKey, out List<IndustryType> cachedTypes))
    {
        return cachedTypes;
    }

    var types = await GetActiveAsync();
    _cache.Set(cacheKey, types, TimeSpan.FromMinutes(30));
    
    return types;
}
```

## 🔄 版本歷史

| 版本 | 日期 | 變更內容 |
|------|------|----------|
| 1.0.0 | 2024-12-XX | 初版發布，繼承 GenericManagementService |
| 1.0.1 | 2024-12-XX | 新增業務特定驗證邏輯 |
| 1.0.2 | 2024-12-XX | 優化搜尋功能，支援代碼搜尋 |

## 📝 注意事項

1. **資料庫主鍵**: 使用標準的 `Id` 欄位作為主鍵，而非自定義的 `IndustryTypeId`
2. **軟刪除**: 所有刪除操作都是軟刪除，資料仍保留在資料庫中
3. **關聯檢查**: 刪除前會檢查是否有客戶使用此行業類型
4. **稽核欄位**: 自動設定 `CreatedAt`、`UpdatedAt` 等稽核欄位
5. **狀態管理**: 支援 Active、Inactive 等狀態切換
6. **日誌記錄**: 重要操作都有詳細的日誌記錄

## 🔗 相關連結

- [GenericManagementService 說明](../GenericManagementService/README.md)
- [Customer Services 說明](../Customers/Readme_CustomerService.md)
- [Entity Framework Core 文檔](https://docs.microsoft.com/en-us/ef/core/)
- [ServiceResult 模式說明](../ServiceResults/README.md)
