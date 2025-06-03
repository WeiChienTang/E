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
- ✅ `GetAllAsync()` - 取得所有客戶 (包含關聯資料)
- ✅ `GetByIdAsync(int id)` - 根據ID取得客戶 (包含聯絡資料和地址)
- ✅ `SearchAsync(string searchTerm)` - 搜尋客戶 (代碼、公司名稱、聯絡人、統編)
- ✅ `CreateAsync(Customer entity)` - 新增客戶
- ✅ `UpdateAsync(Customer entity)` - 更新客戶
- ✅ `DeleteAsync(int id)` - 刪除客戶
- ✅ `ValidateAsync(Customer entity)` - 驗證客戶資料

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
- ✅ `GetAllAsync()` - 取得所有客戶類型
- ✅ `SearchAsync(string searchTerm)` - 搜尋客戶類型 (名稱、描述)
- ✅ `ValidateAsync(CustomerType entity)` - 驗證客戶類型
- ✅ `DeleteAsync(int id)` - 刪除客戶類型 (檢查關聯使用)
- ✅ `IsNameExistsAsync(string name, int? excludeId)` - 檢查名稱重複

#### 業務特定功能
- `IsTypeNameExistsAsync(string typeName, int? excludeId)` - 檢查類型名稱重複
- `GetPagedAsync(int pageNumber, int pageSize)` - 分頁查詢
- `DeleteBatchWithValidationAsync(List<int> ids)` - 批次刪除 (含關聯驗證)

### 3. CustomerContactService.cs
**客戶聯絡資訊管理服務**

#### 基礎功能 (繼承自 GenericManagementService)
- ✅ `GetAllAsync()` - 取得所有聯絡資料 (包含關聯)
- ✅ `GetByIdAsync(int id)` - 根據ID取得聯絡資料
- ✅ `SearchAsync(string searchTerm)` - 搜尋聯絡資料 (值、客戶、類型)
- ✅ `ValidateAsync(CustomerContact entity)` - 驗證聯絡資料

#### 業務特定功能
- `GetContactValue(int customerId, string contactTypeName, ...)` - 取得聯絡資料值
- `UpdateContactValue(int customerId, string contactTypeName, string value, ...)` - 更新聯絡資料
- `GetContactCompletedFieldsCount(List<CustomerContact> customerContacts)` - 計算完成欄位數
- `ValidateCustomerContacts(List<CustomerContact> customerContacts)` - 驗證聯絡資料清單
- `EnsureUniquePrimaryContacts(List<CustomerContact> customerContacts)` - 確保唯一主要聯絡

### 4. CustomerAddressService.cs
**客戶地址管理服務**

#### 基礎功能 (繼承自 GenericManagementService)
- ✅ `GetAllAsync()` - 取得所有地址資料 (包含關聯)
- ✅ `SearchAsync(string searchTerm)` - 搜尋地址 (地址、城市、區域、郵遞區號、客戶)
- ✅ `ValidateAsync(CustomerAddress entity)` - 驗證地址資料

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

## 🔄 與 GenericManagementModal.razor 功能比對

### 重複功能 (已由 GenericManagementModal 提供)

| 功能 | GenericManagementModal | Customer Services | 重複程度 |
|------|----------------------|-------------------|----------|
| **基礎 CRUD 操作** | | | |
| 新增實體 | ✅ `CreateEntity()` | ✅ 繼承 `CreateAsync()` | 🔴 **完全重複** |
| 讀取實體清單 | ✅ `LoadEntities()` | ✅ 繼承 `GetAllAsync()` | 🔴 **完全重複** |
| 刪除實體 | ✅ `DeleteEntity()` | ✅ 繼承 `DeleteAsync()` | 🔴 **完全重複** |
| 狀態切換 | ✅ `ToggleEntityStatus()` | ✅ 繼承 `ToggleStatusAsync()` | 🔴 **完全重複** |
| **UI 管理功能** | | | |
| 載入狀態管理 | ✅ `isLoading` | ❌ 無 | 🟡 **Modal 專用** |
| 錯誤訊息處理 | ✅ `errorMessage` | ❌ 無 | 🟡 **Modal 專用** |
| 成功訊息處理 | ✅ `successMessage` | ❌ 無 | 🟡 **Modal 專用** |
| 表單驗證 | ✅ `DataAnnotationsValidator` | ✅ `ValidateAsync()` | 🟠 **部分重複** |
| **操作同步** | | | |
| 防重複提交 | ✅ `SemaphoreSlim` | ❌ 無 | 🟡 **Modal 專用** |
| 確認對話框 | ✅ `JSRuntime.confirm` | ❌ 無 | 🟡 **Modal 專用** |

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

## 📋 建議和最佳實務

### 1. 避免功能重複
- ✅ **建議使用**: `GenericManagementModal` 處理基礎 CRUD UI
- ✅ **建議保留**: Customer Services 的業務邏輯和複雜查詢
- ⚠️ **注意**: 避免在 Service 層重複實作 UI 相關的狀態管理

### 2. 職責分離
```
GenericManagementModal.razor  ← UI 操作、狀態管理、用戶互動
                ↕
IGenericManagementService     ← 基礎 CRUD 介面
                ↕  
Customer Services            ← 業務邏輯、複雜查詢、資料驗證
                ↕
AppDbContext                 ← 資料存取
```

### 3. 擴展建議
- 考慮將複雜的記憶體操作方法 (如地址清單管理) 移至獨立的 Helper 類別
- 將驗證邏輯考慮提取為獨立的 Validator 服務
- 批次操作可考慮提取為通用的批次處理服務

### 4. 效能最佳化
- 使用分頁查詢避免大量資料載入
- 考慮實作快取機制於頻繁查詢的關聯資料
- 複雜查詢考慮使用投影 (Select) 減少資料傳輸

## 🔗 相關檔案
- `Services/GenericManagementService/GenericManagementService.cs` - 基礎服務實作
- `Components/Shared/Modals/GenericManagementModal.razor` - 通用管理 UI
- `Data/Entities/Customers/` - 客戶實體定義
- `Services/Customers/Interfaces/` - 服務介面定義

---
*最後更新: 2025年6月3日*
