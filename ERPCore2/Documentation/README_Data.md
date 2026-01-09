## 命名規範
所有 Data 資料夾下的檔案使用廣域命名，**不包含具體資料夾名稱**：

- `ERPCore2.Data.Entities`

**實際範例**：
- `Data/Entities/Commons/AddressType.cs` → `namespace ERPCore2.Data.Entities`
- `Data/Context/AppDbContext.cs` → `namespace ERPCore2.Data.Context`
- `Data/Enums/CommonEnums.cs` → `namespace ERPCore2.Data.Enums`

## 實體開發模式

### 標準實體結構
所有實體繼承 `BaseEntity`，自動包含基礎屬性（**請勿重複定義**）：
- `Id`, `Status`, `IsDeleted`
- `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`, `Remarks`

### 實體範例
```csharp
using System.ComponentModel.DataAnnotations;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 您的實體類別 - 繼承 BaseEntity
    /// </summary>
    public class YourEntity : BaseEntity
    {
        [Required(ErrorMessage = "名稱為必填")]
        [MaxLength(50, ErrorMessage = "名稱不可超過50個字元")]
        [Display(Name = "名稱")]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(200, ErrorMessage = "描述不可超過200個字元")]
        [Display(Name = "描述")]
        public string? Description { get; set; }
        
        // 外鍵與導航屬性
        public int? RelatedEntityId { get; set; }
        public RelatedEntity? RelatedEntity { get; set; }
        public ICollection<ChildEntity> ChildEntities { get; set; } = new List<ChildEntity>();
    }
}
```

### 關係模式
```csharp
// 一對多關係 - 父實體
public ICollection<ChildEntity> ChildEntities { get; set; } = new List<ChildEntity>();

// 一對多關係 - 子實體
public int ParentEntityId { get; set; }
public ParentEntity ParentEntity { get; set; } = null!;

// 選擇性關係
public int? OptionalEntityId { get; set; }
public OptionalEntity? OptionalEntity { get; set; }
```

## 資料建設流程

### 1. Seeder 資料種子設定

> ⚠️ **重要提醒**：**不需要新增 Seeder，除非設計者明確聲明需要。** Seeder 主要用於初始化系統必要資料（如權限、角色等），一般業務實體不需要建立 Seeder。

#### Seeder 標準結構
```csharp
using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    public class YourEntitySeeder : IDataSeeder
    {
        public int Order => 6; // 設定執行順序
        public string Name => "您的實體資料";

        public async Task SeedAsync(AppDbContext context)
        {
            await SeedYourEntitiesAsync(context);
        }

        private static async Task SeedYourEntitiesAsync(AppDbContext context)
        {
            if (await context.YourEntities.AnyAsync()) return;

            var (createdAt1, createdBy) = SeedDataHelper.GetSystemCreateInfo(30);
            var (createdAt2, _) = SeedDataHelper.GetSystemCreateInfo(25);

            var entities = new[]
            {
                new YourEntity
                {
                    Name = "測試資料 1",
                    Description = "第一筆測試資料",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt1,
                    CreatedBy = createdBy
                },
                new YourEntity
                {
                    Name = "測試資料 2", 
                    Description = "第二筆測試資料",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt2,
                    CreatedBy = createdBy
                }
            };

            await context.YourEntities.AddRangeAsync(entities);
            await context.SaveChangesAsync();
        }
    }
}
```

#### 註冊 Seeder
在 `SeedData.cs` 中新增：
```csharp
private static IEnumerable<IDataSeeder> GetAllSeeders()
{
    return new List<IDataSeeder>
    {
        new PermissionSeeder(),
        new RoleSeeder(),
        new RolePermissionSeeder(),
        new BasicDataSeeder(),
        new CustomerSeeder(),
        // ... 其他既有 Seeder
        new YourEntitySeeder() // 新增
    };
}
```

### 2. Migration 資料庫遷移

#### 執行步驟
```powershell
# 1. 建立 Migration
Add-Migration AddYourEntityTable -Context AppDbContext

# 2. 檢查生成的 Migration 檔案

# 3. 執行 Migration
Update-Database -Context AppDbContext

# 4. 驗證結果
```

#### 命名規範
- 使用英文 Pascal Case
- 清楚描述變更內容
- 範例：`AddCustomerTable`, `UpdateProductEntity`, `AddIndexToCustomerName`

## 開發檢查清單
## 重要特性
- **非同步操作**：所有資料存取使用 `async/await`
- **交易支援**：Seeder 在單一交易中執行
- **重複保護**：避免重複建立資料
- **順序控制**：透過 `Order` 屬性處理相依性
- **版本控制**：所有 Migration 檔案納入版控