# Customer Services 功能說明

本目錄包含所有與客戶相關的服務類別，這些服務提供完整的客戶資料管理功能。

## 🏗️ 架構概述

所有客戶服務都繼承自 `GenericManagementService<T>`，提供統一的基礎 CRUD 操作，並根據業務需求擴展特定功能。

### 服務繼承結構
```
GenericManagementService<T>
├── CustomerService
├── CustomerTypeService  
├── CustomerContactService
└── CustomerAddressService
```

## 📁 服務檔案清單

### 1. CustomerService.cs
**主要客戶資料管理服務**

#### 基礎功能 (繼承自 GenericManagementService)
- ✅ 覆寫 `GetAllAsync()` - 取得所有客戶 (包含關聯資料)
- ✅ 覆寫 `GetByIdAsync(int id)` - 根據ID取得客戶 (包含聯絡資料和地址)
- ✅ 覆寫 `SearchAsync(string searchTerm)` - 搜尋客戶 (代碼、公司名稱、聯絡人、統編)
- ✅ 覆寫 `ValidateAsync(Customer entity)` - 驗證客戶資料

#### 業務特定功能
- `GetByCustomerCodeAsync(string customerCode)` - 根據客戶代碼查詢
- `GetByCompanyNameAsync(string companyName)` - 根據公司名稱查詢
- `IsCustomerCodeExistsAsync(string customerCode, int? excludeId)` - 檢查代碼重複

#### 關聯資料查詢
- `GetCustomerTypesAsync()` - 取得客戶類型清單
- `GetIndustryTypesAsync()` - 取得行業類型清單
- `GetContactTypesAsync()` - 取得聯絡類型清單
- `GetAddressTypesAsync()` - 取得地址類型清單

#### 聯絡資料管理
- `GetCustomerContactsAsync(int customerId)` - 取得客戶聯絡資料
- `UpdateCustomerContactsAsync(int customerId, List<CustomerContact> contacts)` - 更新聯絡資料

#### 輔助方法
- `InitializeNewCustomer(Customer customer)` - 初始化新客戶
- `GetBasicRequiredFieldsCount()` - 取得基本必填欄位數量
- `GetBasicCompletedFieldsCount(Customer customer)` - 計算已完成欄位數量

### 2. CustomerTypeService.cs
**客戶類型管理服務**

#### 基礎功能 (繼承自 GenericManagementService)
- ✅ 覆寫 `GetAllAsync()` - 取得所有客戶類型
- ✅ 覆寫 `SearchAsync(string searchTerm)` - 搜尋客戶類型 (名稱、描述)
- ✅ 覆寫 `ValidateAsync(CustomerType entity)` - 驗證客戶類型
- ✅ 覆寫 `DeleteAsync(int id)` - 刪除客戶類型 (檢查關聯使用)
- ✅ 覆寫 `IsNameExistsAsync(string name, int? excludeId)` - 檢查名稱重複

#### 業務特定功能
- `IsTypeNameExistsAsync(string typeName, int? excludeId)` - 檢查類型名稱重複
- `GetPagedAsync(int pageNumber, int pageSize)` - 分頁查詢
- `DeleteBatchWithValidationAsync(List<int> ids)` - 批次刪除 (含關聯驗證)

### 3. CustomerContactService.cs
**客戶聯絡資訊管理服務**

#### 基礎功能 (繼承自 GenericManagementService)
- ✅ 覆寫 `GetAllAsync()` - 取得所有聯絡資料 (包含關聯)
- ✅ 覆寫 `GetByIdAsync(int id)` - 根據ID取得聯絡資料
- ✅ 覆寫 `SearchAsync(string searchTerm)` - 搜尋聯絡資料 (值、客戶、類型)
- ✅ 覆寫 `ValidateAsync(CustomerContact entity)` - 驗證聯絡資料

#### 業務特定功能
- `GetContactValue(int customerId, string contactTypeName, ...)` - 取得聯絡資料值
- `UpdateContactValue(int customerId, string contactTypeName, string value, ...)` - 更新聯絡資料
- `GetContactCompletedFieldsCount(List<CustomerContact> customerContacts)` - 計算完成欄位數
- `ValidateCustomerContacts(List<CustomerContact> customerContacts)` - 驗證聯絡資料清單
- `EnsureUniquePrimaryContacts(List<CustomerContact> customerContacts)` - 確保唯一主要聯絡

### 4. CustomerAddressService.cs
**客戶地址管理服務**

#### 基礎功能 (繼承自 GenericManagementService)
- ✅ 覆寫 `GetAllAsync()` - 取得所有地址資料 (包含關聯)
- ✅ 覆寫 `SearchAsync(string searchTerm)` - 搜尋地址 (地址、城市、區域、郵遞區號、客戶)
- ✅ 覆寫 `ValidateAsync(CustomerAddress entity)` - 驗證地址資料

#### 業務特定查詢
- `GetByCustomerIdAsync(int customerId)` - 取得客戶地址清單
- `GetPrimaryAddressAsync(int customerId)` - 取得主要地址
- `GetByAddressTypeAsync(int addressTypeId)` - 根據地址類型查詢

#### 資料庫操作
- `UpdateCustomerAddressesAsync(int customerId, List<CustomerAddress> addresses)` - 更新地址
- `SetPrimaryAddressAsync(int customerId, int addressId)` - 設定主要地址
- `EnsurePrimaryAddressAsync(int customerId)` - 確保有主要地址

#### 記憶體操作 (用於UI編輯)
- `CreateNewAddress(int customerId, int addressCount)` - 建立新地址
- `InitializeDefaultAddresses(List<CustomerAddress> addressList, List<AddressType> addressTypes)` - 初始化預設地址
- `AddAddress(List<CustomerAddress> addressList, CustomerAddress newAddress)` - 新增地址
- `RemoveAddress(List<CustomerAddress> addressList, int index)` - 移除地址
- `SetPrimaryAddress(List<CustomerAddress> addressList, int index)` - 設定主要地址
- `CopyAddressFromFirst(List<CustomerAddress> addressList, int targetIndex)` - 複製地址

#### 欄位更新方法
- `UpdateAddressType(List<CustomerAddress> addressList, int index, int? addressTypeId)`
- `UpdatePostalCode(List<CustomerAddress> addressList, int index, string? postalCode)`
- `UpdateCity(List<CustomerAddress> addressList, int index, string? city)`
- `UpdateDistrict(List<CustomerAddress> addressList, int index, string? district)`
- `UpdateAddress(List<CustomerAddress> addressList, int index, string? address)`

#### 驗證方法
- `ValidateAddressList(List<CustomerAddress> addresses)` - 驗證地址清單
- `GetCompletedAddressCount(List<CustomerAddress> addresses)` - 計算完成地址數

## 🔄 功能比對分析

### 與 GenericManagementService.cs 功能比對

#### 基礎 CRUD 功能 (由 GenericManagementService 提供)

| 功能類別 | GenericManagementService | Customer Services | 實作方式 |
|----------|-------------------------|-------------------|----------|
| **基礎查詢** | | | |
| 取得所有資料 | ✅ `GetAllAsync()` | ✅ 覆寫以包含關聯資料 | 🟠 **覆寫擴展** |
| 取得啟用資料 | ✅ `GetActiveAsync()` | ✅ 繼承使用 | 🟢 **直接繼承** |
| 根據ID查詢 | ✅ `GetByIdAsync(int id)` | ✅ 覆寫以包含關聯資料 | 🟠 **覆寫擴展** |
| 搜尋功能 | ✅ `SearchAsync(string)` (抽象) | ✅ 各服務實作業務邏輯 | 🔴 **必須實作** |
| **CRUD 操作** | | | |
| 建立資料 | ✅ `CreateAsync(T entity)` | ✅ 繼承使用 | 🟢 **直接繼承** |
| 更新資料 | ✅ `UpdateAsync(T entity)` | ✅ 繼承使用 | 🟢 **直接繼承** |
| 刪除資料 | ✅ `DeleteAsync(int id)` | ✅ CustomerType 覆寫檢查關聯 | 🟠 **部分覆寫** |
| 驗證資料 | ✅ `ValidateAsync(T)` (抽象) | ✅ 各服務實作業務驗證 | 🔴 **必須實作** |
| **批次操作** | | | |
| 批次建立 | ✅ `CreateBatchAsync(List<T>)` | ✅ 繼承使用 | 🟢 **直接繼承** |
| 批次更新 | ✅ `UpdateBatchAsync(List<T>)` | ✅ 繼承使用 | 🟢 **直接繼承** |
| 批次刪除 | ✅ `DeleteBatchAsync(List<int>)` | ✅ CustomerType 擴展關聯驗證 | 🟠 **擴展功能** |
| **分頁查詢** | | | |
| 分頁查詢 | ✅ `GetPagedAsync(int, int, string?)` | ✅ CustomerType 另外實作 | 🟡 **重複實作** |
| **狀態管理** | | | |
| 設定狀態 | ✅ `SetStatusAsync(int, EntityStatus)` | ✅ 繼承使用 | 🟢 **直接繼承** |
| 切換狀態 | ✅ `ToggleStatusAsync(int)` | ✅ 繼承使用 | 🟢 **直接繼承** |
| 批次狀態設定 | ✅ `SetStatusBatchAsync(List<int>, EntityStatus)` | ✅ 繼承使用 | 🟢 **直接繼承** |
| **輔助功能** | | | |
| 檢查存在 | ✅ `ExistsAsync(int id)` | ✅ 繼承使用 | 🟢 **直接繼承** |
| 取得總數 | ✅ `GetCountAsync()` | ✅ 繼承使用 | 🟢 **直接繼承** |
| 名稱重複檢查 | ✅ `IsNameExistsAsync(string, int?)` | ✅ CustomerType 覆寫實作 | 🟠 **覆寫實作** |

### 與 GenericManagementModal.razor 功能比對

#### UI 操作功能 (由 GenericManagementModal 提供)

| 功能類別 | GenericManagementModal | Customer Services | 重複程度 |
|----------|----------------------|-------------------|----------|
| **UI 狀態管理** | | | |
| 載入狀態 | ✅ `isLoading` 狀態管理 | ❌ 無 | 🟡 **Modal 專用** |
| 錯誤訊息 | ✅ `errorMessage` 處理 | ❌ 無 | 🟡 **Modal 專用** |
| 成功訊息 | ✅ `successMessage` 處理 | ❌ 無 | 🟡 **Modal 專用** |
| **操作控制** | | | |
| 防重複提交 | ✅ `SemaphoreSlim` 控制 | ❌ 無 | 🟡 **Modal 專用** |
| 確認對話框 | ✅ `JSRuntime.confirm` | ❌ 無 | 🟡 **Modal 專用** |
| **CRUD 介面** | | | |
| 新增表單 | ✅ `CreateEntity()` 呼叫服務 | ✅ 提供服務方法 | 🟢 **正確分工** |
| 資料列表 | ✅ `LoadEntities()` 呼叫服務 | ✅ 提供服務方法 | 🟢 **正確分工** |
| 刪除操作 | ✅ `DeleteEntity()` 呼叫服務 | ✅ 提供服務方法 | 🟢 **正確分工** |
| 狀態切換 | ✅ `ToggleEntityStatus()` 呼叫服務 | ✅ 提供服務方法 | 🟢 **正確分工** |

### 獨特功能 (Customer Services 專有)

| 功能類別 | 功能 | 重複程度 |
|----------|------|----------|
| **業務邏輯** | 客戶代碼重複檢查 | 🟢 **無重複** |
| **關聯查詢** | 多層級 Include 查詢 | 🟢 **無重複** |
| **資料關聯** | 聯絡資料管理 | 🟢 **無重複** |
| **複雜驗證** | 地址清單驗證 | 🟢 **無重複** |
| **記憶體操作** | UI 編輯支援方法 | 🟢 **無重複** |
| **批次操作** | 批次刪除含關聯驗證 | 🟢 **無重複** |
| **欄位計算** | 完成度計算 | 🟢 **無重複** |

## 📋 分析結果與建議

### 🔍 **重複功能分析**

#### 1. 需要檢視的重複功能 🟡
- **CustomerTypeService.GetPagedAsync()** - 與 GenericManagementService.GetPagedAsync() 功能重複
  - **建議**: 移除自定義實作，直接使用基底類別方法

#### 2. 正確的功能覆寫 🟢
- **GetAllAsync()、GetByIdAsync()** - 加入關聯資料載入，符合業務需求
- **DeleteAsync() (CustomerType)** - 加入關聯驗證邏輯，防止意外刪除
- **SearchAsync()、ValidateAsync()** - 必須實作的抽象方法

### 🏗️ **架構層級職責**

```
┌─────────────────────────────┐
│    GenericManagementModal   │ ← UI層：操作介面、狀態管理、用戶互動
│         (Razor)             │
└─────────────────────────────┘
              ↕ 呼叫服務方法
┌─────────────────────────────┐
│  IGenericManagementService  │ ← 介面層：標準化服務契約
│        (Interface)          │
└─────────────────────────────┘
              ↕ 實作介面
┌─────────────────────────────┐
│   GenericManagementService  │ ← 基礎層：通用CRUD、批次操作、狀態管理
│       (Base Class)          │
└─────────────────────────────┘
              ↕ 繼承並擴展
┌─────────────────────────────┐
│    Customer Services        │ ← 業務層：專業邏輯、關聯查詢、業務驗證
│   (Specific Services)       │
└─────────────────────────────┘
              ↕ 資料存取
┌─────────────────────────────┐
│       AppDbContext          │ ← 資料層：資料庫操作
└─────────────────────────────┘
```

### 3. 最佳實務建議

#### ✅ **保持現有的良好設計**
- Customer Services 專注於業務邏輯和複雜查詢
- GenericManagementModal 處理 UI 操作和狀態管理
- GenericManagementService 提供標準化的基礎功能

#### ⚠️ **需要調整的部分**
- 移除重複的分頁查詢實作
- 統一使用基底類別提供的批次操作
- 確保驗證邏輯只在必要時覆寫

#### 🚀 **進階最佳化建議**
- 考慮將複雜的記憶體操作方法 (如地址清單管理) 提取為獨立的 Helper 類別
- 實作快取機制於頻繁查詢的關聯資料 (如客戶類型、行業類型)
- 使用投影查詢 (Select) 減少不必要的資料傳輸

### 4. 效能最佳化
- 使用基底類別的 `GetPagedAsync()` 方法進行分頁查詢
- 實作快取機制於頻繁查詢的關聯資料
- 複雜查詢使用投影 (Select) 減少資料傳輸量
- 適當使用 AsNoTracking() 於唯讀查詢

## 🔗 相關檔案
- `Services/GenericManagementService/GenericManagementService.cs` - 基礎服務實作
- `Components/Shared/Modals/GenericManagementModal.razor` - 通用管理 UI
- `Data/Entities/Customers/` - 客戶實體定義
- `Services/Customers/Interfaces/` - 服務介面定義

---
*最後更新: 2025年6月3日*
