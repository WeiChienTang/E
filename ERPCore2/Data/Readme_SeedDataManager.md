# SeedDataManager 種子資料管理系統

## 概述

SeedDataManager 是 ERPCore2 專案中的種子資料管理系統，負責初始化系統所需的基礎資料。該系統採用模組化設計，支援分層級、按順序執行的資料初始化流程。

## 系統架構

```
SeedDataManager/
├── Interfaces/
│   └── IDataSeeder.cs          # 種子器介面定義
├── Helpers/
│   └── SeedDataHelper.cs       # 種子資料共用工具
└── Seeders/
    ├── PermissionSeeder.cs     # 權限種子器 (Order: 0)
    ├── RoleSeeder.cs           # 角色種子器 (Order: 1)
    ├── RolePermissionSeeder.cs # 角色權限關聯種子器 (Order: 2)
    ├── BasicDataSeeder.cs      # 基礎資料種子器 (Order: 3)
    ├── CustomerSeeder.cs       # 客戶資料種子器 (Order: 4)
    ├── SupplierSeeder.cs       # 供應商資料種子器 (Order: 5)
    └── ProductSeeder.cs        # 產品資料種子器 (Order: 6)
```

## 核心組件

### 1. IDataSeeder 介面

定義所有種子器必須實現的標準介面：

```csharp
public interface IDataSeeder
{
    int Order { get; }              // 執行順序（數字越小越早執行）
    string Name { get; }            // 種子器名稱
    Task SeedAsync(AppDbContext context);  // 執行資料種子初始化
}
```

**設計特點：**
- **順序控制**：透過 `Order` 屬性確保種子器按正確順序執行
- **命名識別**：透過 `Name` 屬性提供人性化的識別名稱
- **非同步操作**：支援非同步資料庫操作以提升效能

### 2. SeedDataHelper 工具類別

提供種子資料初始化過程中的共用功能：

```csharp
public static class SeedDataHelper
{
    // 密碼雜湊功能
    public static string HashPassword(string password)
    
    // 生成統一的建立時間和建立者資訊
    public static (DateTime CreatedAt, string CreatedBy) GetSystemCreateInfo(int daysAgo = 0)
}
```

**功能說明：**
- **密碼安全**：提供 SHA256 + Salt 的密碼雜湊功能
- **審計資訊**：統一生成系統建立的審計資訊，支援回溯日期設定

## 種子器詳細說明

### PermissionSeeder (Order: 0) - 權限種子器

**職責：** 初始化系統所有功能權限定義

**初始化內容：**
- **權限資料 (Permissions)**：系統所有功能權限定義
  - 系統管理權限
  - 使用者管理權限 (CRUD)
  - 客戶管理權限 (CRUD)
  - 供應商管理權限 (CRUD)
  - 員工管理權限 (CRUD)
  - 產品管理權限 (CRUD)
  - 產品分類管理權限 (CRUD)
  - 基礎資料管理權限 (材質、天氣、顏色、尺寸、單位等)
  - 權限與角色管理權限 (CRUD)
  - 倉庫管理權限 (CRUD)
  - 庫存管理權限 (CRUD)
  - 系統控制權限

**執行順序考量：** 權限定義必須在角色建立之前完成，為最基礎的資料。

### RoleSeeder (Order: 1) - 角色種子器

**職責：** 初始化系統角色定義

**初始化內容：**
- **角色資料 (Roles)**：系統角色定義
  - 系統管理員 (Administrator)

**設計特點：**
- 簡化角色結構，僅保留系統管理員角色
- 檢查資料是否已存在，避免重複初始化
- 使用統一的審計資訊設定

**執行順序考量：** 依賴權限資料，在權限建立後、角色權限關聯前執行。

### RolePermissionSeeder (Order: 2) - 角色權限關聯種子器

**職責：** 建立角色與權限的關聯關係，並建立預設系統管理員帳號

**初始化內容：**
- **角色權限關聯 (RolePermissions)**：系統管理員擁有所有權限
- **預設管理員帳號 (Users)**：系統初始管理員帳號
  - 帳號：admin
  - 密碼：admin123 (已雜湊)
  - Email 聯絡資料

**設計特點：**
- 系統管理員自動獲得所有權限
- 包含預設管理員帳號建立
- 為管理員建立基本聯絡資料

**執行順序考量：** 必須在權限和角色都建立完成後執行，為認證系統的最後一步。

### BasicDataSeeder (Order: 3) - 基礎資料種子器

**職責：** 初始化系統運作所需的基礎主檔資料

**初始化內容：**
- **聯絡類型 (ContactTypes)**：電話、手機、Email、傳真等
- **地址類型 (AddressTypes)**：公司地址、通訊地址、帳單地址等
- **客戶類型 (CustomerTypes)**：企業客戶、個人客戶等
- **行業類型 (IndustryTypes)**：製造業、服務業、零售業等
- **供應商類型 (SupplierTypes)**：原料供應商、設備供應商、服務供應商等

**設計特點：**
- 檢查資料是否已存在，避免重複初始化
- 使用統一的審計資訊設定
- 支援多語系描述

### CustomerSeeder (Order: 4) - 客戶資料種子器

**職責：** 初始化示例客戶資料供系統測試與展示使用

**初始化內容：**
- 示例客戶主檔資料
- 客戶聯絡資訊
- 客戶地址資訊
- 客戶與類型、行業的關聯

**資料特點：**
- 依賴 BasicDataSeeder 的基礎資料
- 提供真實的業務場景示例
- 包含完整的客戶資訊結構

### SupplierSeeder (Order: 5) - 供應商資料種子器

**職責：** 初始化示例供應商資料

**初始化內容：**
- 供應商主檔資料
- 供應商聯絡資訊
- 供應商地址資訊
- 供應商類型關聯

**設計考量：**
- 與客戶資料結構保持一致性
- 支援供應商分類管理
- 提供豐富的測試資料

### ProductSeeder (Order: 6) - 產品資料種子器

**職責：** 初始化產品相關資料（目前為預留結構）

**當前狀態：** 預留給未來產品資料的種子初始化

**未來規劃：**
- 產品主檔資料
- 產品分類
- 產品供應商關聯
- 產品價格資訊

## 執行流程

### 1. 初始化調用

系統啟動時透過 `SeedData.InitializeAsync()` 方法執行：

```csharp
// 在 Program.cs 中調用
await SeedData.InitializeAsync(app.Services);
```

### 2. 執行步驟

1. **資料庫準備**：確保資料庫已建立
2. **交易開始**：開啟資料庫交易以確保資料一致性
3. **種子器排序**：按 Order 屬性對種子器進行排序
4. **順序執行**：依序執行每個種子器的 `SeedAsync` 方法
5. **交易提交**：所有種子器執行成功後提交交易
6. **例外處理**：發生錯誤時回滾交易並記錄錯誤

### 3. 執行順序

```
0. PermissionSeeder     → 權限定義
1. RoleSeeder          → 角色定義
2. RolePermissionSeeder → 角色權限關聯與預設管理員帳號
3. BasicDataSeeder     → 系統基礎主檔資料
4. CustomerSeeder      → 客戶示例資料
5. SupplierSeeder      → 供應商示例資料
6. ProductSeeder       → 產品資料（預留）
```

## 設計原則

### 1. 單一職責原則
每個種子器只負責特定領域的資料初始化，職責明確分離。

### 2. 依賴關係管理
透過 Order 屬性確保種子器按正確的依賴順序執行。

### 3. 冪等性設計
所有種子器都會檢查資料是否已存在，支援重複執行而不會產生重複資料。

### 4. 交易一致性
使用資料庫交易確保所有資料初始化過程的原子性。

### 5. 錯誤處理
提供完整的錯誤處理機制，失敗時能夠完整回滾。

## 擴充指南

### 新增種子器

1. **實現 IDataSeeder 介面**
```csharp
public class NewDataSeeder : IDataSeeder
{
    public int Order => 5;  // 設定適當的執行順序
    public string Name => "新資料模組";

    public async Task SeedAsync(AppDbContext context)
    {
        // 實現資料初始化邏輯
    }
}
```

2. **註冊種子器**
在 `SeedData.GetAllSeeders()` 方法中加入新的種子器實例。

3. **考慮依賴關係**
確保 Order 值反映正確的執行順序，避免依賴關係問題。

### 最佳實踐

1. **資料檢查**：總是先檢查資料是否已存在
2. **統一審計**：使用 `SeedDataHelper.GetSystemCreateInfo()` 設定審計資訊
3. **錯誤處理**：妥善處理可能的資料庫例外
4. **效能考量**：對於大量資料使用批次操作
5. **測試資料**：提供有意義的測試資料，避免假資料

## 注意事項

1. **執行環境**：種子資料主要用於開發和測試環境，生產環境需謹慎使用
2. **資料安全**：預設密碼等敏感資訊應該使用安全的預設值
3. **版本控制**：種子資料的變更應該配合資料庫 Migration 進行版本控制
4. **效能影響**：大量種子資料可能影響系統啟動時間，需要適當平衡

## 相關檔案

- `Data/SeedData.cs` - 種子資料協調器
- `Data/Context/AppDbContext.cs` - 資料庫上下文
- `Data/Entities/` - 實體定義
- `Program.cs` - 應用程式啟動點
