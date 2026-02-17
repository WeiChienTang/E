# Data 資料層設計

## 更新日期
2026-02-17

---

## 概述

資料層是五層架構的基礎，負責定義實體（Entity）、資料庫上下文（AppDbContext）、以及資料庫遷移（Migration）。所有實體繼承 `BaseEntity`，確保統一的審計欄位與狀態管理。

---

## 命名規範

所有 `Data/` 資料夾下的檔案使用**廣域命名**，不包含具體子資料夾名稱：

```csharp
// 正確
namespace ERPCore2.Data.Entities
namespace ERPCore2.Data.Context
namespace ERPCore2.Data.Enums

// 錯誤（不要包含子資料夾名稱）
namespace ERPCore2.Data.Entities.Customers    // ❌
namespace ERPCore2.Data.Entities.Commons      // ❌
```

---

## BaseEntity 基底類別

所有實體繼承 `BaseEntity`，自動包含以下屬性（**請勿重複定義**）：

| 屬性 | 類型 | 說明 |
|------|------|------|
| Id | int | 主鍵，自動遞增 |
| Status | EntityStatus | 狀態（Active/Inactive） |
| IsDeleted | bool | 軟刪除標記 |
| CreatedAt | DateTime | 建立時間 |
| UpdatedAt | DateTime? | 最後更新時間 |
| CreatedBy | string? | 建立者 |
| UpdatedBy | string? | 最後修改者 |
| Remarks | string? | 備註 |

---

## 實體開發模式

### 標準實體結構

```csharp
using System.ComponentModel.DataAnnotations;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    public class YourEntity : BaseEntity
    {
        // 基本屬性
        [Required(ErrorMessage = "編號為必填")]
        [MaxLength(20, ErrorMessage = "編號不可超過20個字元")]
        [Display(Name = "編號")]
        public string Code { get; set; } = string.Empty;

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

        // 一對多子集合
        public ICollection<ChildEntity> ChildEntities { get; set; } = new List<ChildEntity>();
    }
}
```

### 關係模式

```csharp
// 一對多 - 父實體
public ICollection<ChildEntity> ChildEntities { get; set; } = new List<ChildEntity>();

// 一對多 - 子實體（必填外鍵）
public int ParentEntityId { get; set; }
public ParentEntity ParentEntity { get; set; } = null!;

// 選擇性關係（可為 null）
public int? OptionalEntityId { get; set; }
public OptionalEntity? OptionalEntity { get; set; }
```

---

## 實體檔案位置

實體依模組分類存放於 `Data/Entities/` 子資料夾：

| 資料夾 | 說明 | 範例 |
|--------|------|------|
| Commons/ | 通用實體 | AddressType, Currency, Unit, Size |
| Customers/ | 客戶相關 | Customer |
| Employees/ | 員工相關 | Employee, Department, EmployeePosition, Role |
| Products/ | 產品相關 | Product, ProductCategory, ProductComposition |
| Purchase/ | 採購相關 | PurchaseOrder, PurchaseOrderDetail, PurchaseReceiving |
| Sales/ | 銷售相關 | Quotation, SalesOrder, SalesDelivery, SalesReturn |
| FinancialManagement/ | 財務相關 | SetoffDocument |
| Warehouses/ | 倉庫相關 | Warehouse, WarehouseLocation |
| Systems/ | 系統相關 | SystemParameter, ErrorLog, Permission |

---

## AppDbContext 設定

在 `Data/Context/AppDbContext.cs` 中註冊 DbSet：

```csharp
public class AppDbContext : DbContext
{
    public DbSet<YourEntity> YourEntities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 設定外鍵關係（選擇性）
        modelBuilder.Entity<YourEntity>(entity =>
        {
            entity.HasOne(e => e.RelatedEntity)
                  .WithMany()
                  .HasForeignKey(e => e.RelatedEntityId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
```

---

## 資料庫遷移

### 執行步驟

```powershell
# 1. 建立 Migration
Add-Migration AddYourEntityTable -Context AppDbContext

# 2. 檢查生成的 Migration 檔案

# 3. 執行 Migration
Update-Database -Context AppDbContext
```

### 命名規範
- 使用英文 Pascal Case
- 清楚描述變更內容
- 範例：`AddCustomerTable`、`UpdateProductEntity`、`AddIndexToCustomerName`

---

## Seeder 資料種子

> **注意**：不需要新增 Seeder，除非設計者明確聲明需要。Seeder 主要用於初始化系統必要資料（如權限、角色等）。

如需建立 Seeder，請參考 [Readme_SeedDataManager.md](../Readme_SeedDataManager.md)。

---

## 開發檢查清單

- [ ] 實體繼承 `BaseEntity`
- [ ] 不重複定義 `Id`、`Status`、`CreatedAt` 等基底屬性
- [ ] 使用 `[Required]`、`[MaxLength]` 等 Data Annotation
- [ ] 外鍵屬性命名為 `{RelatedEntity}Id`
- [ ] 導航屬性使用正確的可空性（必填 `= null!`，選填 `?`）
- [ ] 在 `AppDbContext` 中新增 `DbSet<>`
- [ ] 執行 Migration 並驗證資料庫結構

---

## 相關文件

- [README_完整頁面設計總綱.md](README_完整頁面設計總綱.md) - 總綱
- [README_SeedData管理.md](../專案架構/README_SeedData管理.md) - Seeder 管理系統
