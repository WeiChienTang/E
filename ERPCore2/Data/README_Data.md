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

## 注意事項

1. **命名空間一致性**：所有新增的實體類別都必須遵循廣域命名規範
2. **資料夾分類**：新增實體時請根據業務領域建立適當的資料夾分類
3. **屬性驗證**：每個實體類別都應包含完整的資料驗證屬性
4. **中文錯誤訊息**：所有錯誤訊息都使用繁體中文
5. **文檔維護**：新增實體分類時請同步更新此 README 文檔