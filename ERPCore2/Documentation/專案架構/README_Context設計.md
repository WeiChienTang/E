# AppDbContext 配置說明與設計原則

## 概述
`AppDbContext` 是專案的核心資料庫上下文，遵循 **Entity 優先配置** 原則，僅處理無法透過 Data Annotations 表達的 Entity Framework Core 特定配置。

## 核心設計原則

### 1. **Entity 優先配置原則**
- 所有能透過 Data Annotations 標籤配置的屬性，都應在 Entity 類別中定義
- AppDbContext 只負責無法透過標籤表達的複雜配置
- 避免任何形式的重複設定

### 2. **責任分離原則**
- **Entity 層**: 屬性驗證、索引、外鍵標示、顯示設定、預設值
- **AppDbContext 層**: 關聯刪除行為、複雜查詢配置、實體間關係行為

### 3. **配置檢查原則**
每次修改 AppDbContext 後，必須檢查：
- 是否有配置可移至 Entity 層的 Data Annotations
- 是否存在重複的設定
- 是否有空的實體配置區塊可移除

## AppDbContext 專責配置項目

### **實體關聯刪除行為**
```csharp
modelBuilder.Entity<Customer>(entity =>
{
    entity.HasOne(e => e.CustomerType)
          .WithMany(ct => ct.Customers)
          .OnDelete(DeleteBehavior.SetNull);
});
```

**適用場景**:
- `DeleteBehavior.SetNull`: 參考實體刪除時，外鍵設為 null
- `DeleteBehavior.Cascade`: 主實體刪除時，相關實體也一併刪除
- `DeleteBehavior.Restrict`: 存在相關實體時，禁止刪除主實體

## Entity 層標籤配置

### **基本屬性驗證**
```csharp
[Required(ErrorMessage = "客戶名稱為必填")]
[MaxLength(100, ErrorMessage = "客戶名稱長度不能超過100字元")]
[Display(Name = "客戶名稱")]
public string CustomerName { get; set; } = string.Empty;
```

### **索引設定**
```csharp
[Index(nameof(CustomerCode), IsUnique = true)]
public class Customer : BaseEntity
{
    // ...屬性定義
}
```

### **外鍵關聯**
```csharp
[ForeignKey(nameof(CustomerType))]
public int? CustomerTypeId { get; set; }
public virtual CustomerType? CustomerType { get; set; }
```

### **預設值設定**
```csharp
// 在 BaseEntity 中已設定
public EntityStatus Status { get; set; } = EntityStatus.Active;
```

## 配置遷移指導

### **從 AppDbContext 遷移至 Entity 的配置**

#### 需要移除的重複配置
```csharp
// ❌ 在 AppDbContext 中移除
entity.Property(e => e.Status).HasDefaultValue(EntityStatus.Active);
entity.HasIndex(e => e.CustomerCode).IsUnique();
entity.HasOne(e => e.CustomerType).WithMany().HasForeignKey(e => e.CustomerTypeId);

// ✅ 在 Entity 中使用標籤
[Index(nameof(CustomerCode), IsUnique = true)]
public class Customer : BaseEntity // BaseEntity 已有 Status 預設值
{
    [ForeignKey(nameof(CustomerType))]
    public int? CustomerTypeId { get; set; }
}
```

#### 空配置區塊移除
```csharp
// ❌ 移除空的實體配置
modelBuilder.Entity<CustomerType>(entity => { });
modelBuilder.Entity<IndustryType>(entity => { });
```

## 實作規範

### **Entity 類別設計規範**
1. 繼承 `BaseEntity` 取得共通欄位（Id, Status, 審計欄位）
2. 使用 Data Annotations 進行屬性驗證與配置
3. 實作導航屬性以支援關聯查詢
4. 避免在 Entity 中放置業務邏輯

### **AppDbContext 實作規範**
1. 只配置關聯刪除行為
2. 避免重複設定已在 Entity 中定義的配置
3. 每個實體配置前檢查是否真的需要
4. 保持配置區塊的簡潔性

### **配置驗證檢查清單**
配置完成後檢查：
- [ ] 是否有重複的屬性配置
- [ ] 是否有空的實體配置區塊
- [ ] 是否所有索引都使用 `[Index]` 標籤
- [ ] 是否所有外鍵都使用 `[ForeignKey]` 標籤
- [ ] 是否刪除行為配置正確且必要

## 技術考量

### **EF Core 版本依賴**
- `[Index]` 標籤：需要 EF Core 5.0+
- `[ForeignKey]` 標籤：EF Core 所有版本支援
- Data Annotations：建議優先使用，Fluent API 為輔

### **效能考量**
- Entity 層配置在編譯時處理，效能較佳
- AppDbContext 配置在運行時處理，適合動態配置
- 複雜查詢配置考慮使用 Query Types 或 Raw SQL

### **維護性考量**
- 配置集中在 Entity 層，降低維護複雜度
- 關聯行為集中在 AppDbContext，便於統一管理
- 避免配置分散造成的不一致問題

## 最佳實踐總結

1. **優先使用 Entity Data Annotations**：提高可讀性和維護性
2. **AppDbContext 專注關聯行為**：只處理無法標籤化的配置
3. **避免任何重複設定**：遵循 DRY (Don't Repeat Yourself) 原則
4. **定期重構檢查**：移除不必要的配置和空區塊
5. **配置文件化**：在程式碼中加入適當的註解說明
