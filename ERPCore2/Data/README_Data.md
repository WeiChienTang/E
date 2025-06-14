# Data 資料夾說明文檔

## 概述
Data 資料夾包含了 ERPCore2 系統的資料存取層相關檔案，主要負責資料模型定義、資料庫上下文設定以及初始資料種子。

## 命名規範

### 命名空間命名方式
所有 Data 資料夾下的檔案均使用廣域命名方式，**不包含具體的資料夾名稱**：

- ✅ **正確範例**：`ERPCore2.Data.Entities`
- ❌ **錯誤範例**：`ERPCore2.Data.Entities.Commons`

### 實際命名範例
- `Data/Entities/Commons/AddressType.cs` → `namespace ERPCore2.Data.Entities`
- `Data/Entities/Customers/Customer.cs` → `namespace ERPCore2.Data.Entities`
- `Data/Entities/Industries/Industry.cs` → `namespace ERPCore2.Data.Entities`
- `Data/Context/AppDbContext.cs` → `namespace ERPCore2.Data.Context`
- `Data/Enums/CommonEnums.cs` → `namespace ERPCore2.Data.Enums`
---

### 標準實體結構

每個實體都應包含以下標準結構：
1. 包含必要標籤
2. 繼承 BaseEntity 檔案位置 : `Data/BaseEntity`

#### BaseEntity 已包含的屬性
所有繼承 `BaseEntity` 的實體都會自動擁有以下屬性，**請勿重複定義**：

- `Id` (int) - 主鍵 ID
- `Status` (EntityStatus) - 實體狀態
- `IsDeleted` (bool) - 軟刪除標記
- `CreatedAt` (DateTime) - 建立時間
- `UpdatedAt` (DateTime?) - 最後更新時間
- `CreatedBy` (string?) - 建立者 ID
- `UpdatedBy` (string?) - 最後更新者 ID

#### 新實體範例
```csharp
using System.ComponentModel.DataAnnotations;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 您的實體類別 - 繼承 BaseEntity 後自動包含基礎屬性
    /// </summary>
    public class YourEntity : BaseEntity
    {
        // ⚠️ 注意：不要重複定義 Id、Status、IsDeleted、CreatedAt、UpdatedAt、CreatedBy、UpdatedBy
        
        // 必要屬性
        [Required(ErrorMessage = "名稱為必填")]
        [MaxLength(50, ErrorMessage = "名稱不可超過50個字元")]
        [Display(Name = "名稱")]
        public string Name { get; set; } = string.Empty;
        
        // 選擇性屬性
        [MaxLength(200, ErrorMessage = "描述不可超過200個字元")]
        [Display(Name = "描述")]
        public string? Description { get; set; }
        
        // 外鍵（如果適用）
        [Display(Name = "相關實體")]
        public int? RelatedEntityId { get; set; }
        
        // 導航屬性
        public RelatedEntity? RelatedEntity { get; set; }
        public ICollection<ChildEntity> ChildEntities { get; set; } = new List<ChildEntity>();
    }
}
```

### 屬性長度建議

| 欄位類型 | 建議長度 | 用途 |
|----------|----------|------|
| 代碼類 | 10-20 | 客戶代碼、產品代碼等 |
| 名稱類 | 50-100 | 實體名稱、公司名稱等 |
| 描述類 | 100-500 | 描述、備註等 |
| 地址類 | 200 | 完整地址 |
| 聯絡類 | 100 | 電話、Email等 |
| 人員類 | 50 | 聯絡人、建立者等 |

### 導航屬性設計

#### 一對多關係
```csharp
// 父實體
public ICollection<ChildEntity> ChildEntities { get; set; } = new List<ChildEntity>();

// 子實體
public int ParentEntityId { get; set; }
public ParentEntity ParentEntity { get; set; } = null!;
```

#### 選擇性關係
```csharp
// 主實體
public int? OptionalEntityId { get; set; }
public OptionalEntity? OptionalEntity { get; set; }
```

## 開發檢查清單

建立新實體時，請確認以下項目：

### 基本結構
- [ ] 使用正確的命名空間 `ERPCore2.Data.Entities`
- [ ] 繼承 `BaseEntity` 類別
- [ ] **不可重複定義** BaseEntity 已包含的屬性（Id、Status、IsDeleted、CreatedAt、UpdatedAt、CreatedBy、UpdatedBy）
- [ ] 包含 `using System.ComponentModel.DataAnnotations;`
- [ ] 包含 `using ERPCore2.Data.Enums;`

### 屬性標籤
- [ ] 所有必要欄位都有 `[Required]` 標籤
- [ ] 所有字串欄位都有 `[MaxLength]` 標籤
- [ ] 所有屬性都有 `[Display(Name = "中文名稱")]` 標籤
- [ ] 錯誤訊息使用繁體中文
- [ ] 字串屬性初始化為 `string.Empty` 或可空類型

### 導航屬性
- [ ] 正確設定父子關係的導航屬性
- [ ] 集合屬性初始化為空集合 `new List<>()`
- [ ] 必要關係標記為 `null!`，選擇性關係為可空類型

---

## 相關檔案說明

- `Context/AppDbContext.cs`：Entity Framework 資料庫上下文設定
- `Enums/CommonEnums.cs`：系統共用的列舉定義（包含 EntityStatus）
- `SeedData.cs`：資料庫初始化資料種子檔案

---

## 資料建設流程

### 1. Seeder 資料種子設定

在完成實體定義後，必須在 `SeedDataManager` 資料夾中建立對應的 Seeder 類別，用於提供測試資料：

#### Seeder 建立步驟
1. 在 `Data/SeedDataManager/Seeders/` 資料夾下建立新的 Seeder 類別
2. 實作 `IDataSeeder` 介面
3. 設定適當的執行順序 (`Order` 屬性)
4. 實作非同步的資料初始化邏輯
5. 在 `SeedData.cs` 中註冊新的 Seeder

#### Seeder 標準結構
```csharp
using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Data.SeedDataManager.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 您的實體種子器
    /// </summary>
    public class YourEntitySeeder : IDataSeeder
    {
        public int Order => 6; // 設定執行順序（參考現有 Seeder 的順序）
        public string Name => "您的實體資料";

        public async Task SeedAsync(AppDbContext context)
        {
            await SeedYourEntitiesAsync(context);
        }

        /// <summary>
        /// 初始化您的實體資料
        /// </summary>
        private static async Task SeedYourEntitiesAsync(AppDbContext context)
        {
            // 檢查資料是否已存在
            if (await context.YourEntities.AnyAsync())
                return;

            // 取得建立時間和建立者資訊
            var (createdAt1, createdBy) = SeedDataHelper.GetSystemCreateInfo(30);
            var (createdAt2, _) = SeedDataHelper.GetSystemCreateInfo(25);
            var (createdAt3, _) = SeedDataHelper.GetSystemCreateInfo(20);

            var entities = new[]
            {
                new YourEntity
                {
                    Name = "測試資料 1",
                    Description = "這是第一筆測試資料",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt1,
                    CreatedBy = createdBy
                },
                new YourEntity
                {
                    Name = "測試資料 2",
                    Description = "這是第二筆測試資料",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt2,
                    CreatedBy = createdBy
                },
                new YourEntity
                {
                    Name = "測試資料 3",
                    Description = "這是第三筆測試資料",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt3,
                    CreatedBy = createdBy
                }
                // 建議至少建立 3-5 筆測試資料
            };

            await context.YourEntities.AddRangeAsync(entities);
            await context.SaveChangesAsync();
        }
    }
}
```

新增的 Seeder 應根據相依性選擇適當的順序。

#### 註冊 Seeder
在 `SeedData.cs` 的 `GetAllSeeders()` 方法中加入新的 Seeder
```csharp
private static IEnumerable<IDataSeeder> GetAllSeeders()
{
    return new List<IDataSeeder>
    {
        new AuthSeeder(),
        new BasicDataSeeder(),
        new CustomerSeeder(),
        new SupplierSeeder(),
        new ProductSeeder(),
        new InventorySeeder(),
        new YourEntitySeeder() // 新增您的 Seeder
    };
}
```

#### Seeder 重要特性

1. **非同步執行**：所有 Seeder 都使用 `async/await` 模式以提升效能
2. **重複執行保護**：使用 `AnyAsync()` 檢查避免重複建立資料
3. **交易支援**：整個初始化過程在單一交易中執行，確保資料一致性
4. **順序控制**：透過 `Order` 屬性確保相依性正確的執行順序
5. **工具類別支援**：使用 `SeedDataHelper` 產生一致的時間和使用者資訊

### 2. Migrations 資料庫遷移

完成實體定義和 Seeder 設定後，需要進行資料庫遷移：

#### Migration 執行步驟

1. **開啟 Package Manager Console** 或使用命令列工具
2. **建立 Migration** ：
   ```powershell
   Add-Migration [MigrationName] -Context AppDbContext
   ```
   範例：
   ```powershell
   Add-Migration AddYourEntityTable -Context AppDbContext
   ```

3. **檢查 Migration 檔案** ：
   - 檢查生成的 Migration 檔案是否正確
   - 確認 Up() 和 Down() 方法的內容
   - 驗證外鍵關係和索引設定

4. **執行 Migration** ：
   ```powershell
   Update-Database -Context AppDbContext
   ```

5. **驗證結果** ：
   - 檢查資料庫是否正確建立新表格
   - 確認 Seed 資料是否正確插入
   - 測試基本的 CRUD 操作

#### Migration 命名規範

- 使用英文命名，採用 Pascal Case
- 命名應清楚描述變更內容
- 範例：
  - `AddCustomerTable`
  - `UpdateProductEntity`
  - `AddIndexToCustomerName`
  - `ModifyOrderStatusEnum`

#### Migration 注意事項

1. **資料備份**：執行 Migration 前請先備份資料庫
2. **分段執行**：大型變更建議分多個 Migration 執行
3. **回滾準備**：確保 Down() 方法能正確回滾變更
4. **測試優先**：在開發環境完整測試後再部署到正式環境
5. **文檔記錄**：重要的 Migration 應在專案文檔中記錄說明

---

## 注意事項

1. **命名空間一致性**：所有新增的實體類別都必須遵循廣域命名規範
2. **資料夾分類**：新增實體時請根據業務領域建立適當的資料夾分類
3. **屬性驗證**：每個實體類別都應包含完整的資料驗證屬性
4. **中文錯誤訊息**：所有錯誤訊息都使用繁體中文
5. **文檔維護**：新增實體分類時請同步更新此 README 文檔
6. **Seeder 實作要求**：
   - 每個新實體都必須實作對應的 Seeder 類別
   - 必須實作 `IDataSeeder` 介面
   - 使用 `SeedDataHelper` 產生一致的測試資料
   - 提供至少 5-10 筆有意義的測試資料
   - 正確設定執行順序以處理相依性
7. **Migration 版本控制**：所有 Migration 檔案都應納入版本控制系統
8. **非同步操作**：所有資料存取操作都應使用非同步方法以提升效能