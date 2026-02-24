# 個人化設定 — 資料服務層設計

## 更新日期
2026-02-25

---

## Entity 設計

### EmployeePreference

```csharp
// Data/Entities/Employees/EmployeePreference.cs
[Index(nameof(EmployeeId), IsUnique = true)]
public class EmployeePreference : BaseEntity
{
    [Required]
    [ForeignKey(nameof(Employee))]
    public int EmployeeId { get; set; }

    public UILanguage Language { get; set; } = UILanguage.ZhTW;

    public Employee? Employee { get; set; }
}

public enum UILanguage
{
    ZhTW = 1,   // 繁體中文（預設）
    EnUS = 2    // English
}
```

**新增偏好設定欄位時**，在此加入對應的屬性（並更新 `SavePreferenceAsync`）。

---

## AppDbContext 1-to-1 關係

```csharp
// Data/Context/AppDbContext.cs — OnModelCreating
modelBuilder.Entity<EmployeePreference>(entity =>
{
    entity.Property(e => e.Id).ValueGeneratedOnAdd();

    entity.HasOne(ep => ep.Employee)
          .WithOne(e => e.Preference)
          .HasForeignKey<EmployeePreference>(ep => ep.EmployeeId)
          .OnDelete(DeleteBehavior.Cascade); // 員工刪除時設定一併刪除
});
```

`Employee.cs` 對應的導航屬性：

```csharp
public EmployeePreference? Preference { get; set; }
```

> ⚠️ 不要直接讀取 `Employee.Preference`，除非已在 EF 查詢中 `Include()` 此屬性，否則為 `null`。請改用 `IEmployeePreferenceService.GetByEmployeeIdAsync()`。

---

## 服務層設計

### IEmployeePreferenceService

```csharp
public interface IEmployeePreferenceService : IGenericManagementService<EmployeePreference>
{
    // 不存在時回傳預設值（Language = ZhTW），不寫入 DB
    Task<EmployeePreference> GetByEmployeeIdAsync(int employeeId);

    // 不存在則 INSERT，存在則 UPDATE（Upsert）
    Task<ServiceResult> SavePreferenceAsync(int employeeId, EmployeePreference preference);
}
```

### SavePreferenceAsync — Upsert 核心邏輯

```csharp
var existing = await context.EmployeePreferences
    .FirstOrDefaultAsync(p => p.EmployeeId == employeeId);

if (existing == null)
{
    preference.EmployeeId = employeeId;
    preference.CreatedAt = DateTime.Now;
    context.EmployeePreferences.Add(preference);     // 新增
}
else
{
    existing.Language = preference.Language;          // 只更新設定欄位
    // existing.FontSize = preference.FontSize;       // 新增欄位時在此補上
    existing.UpdatedAt = DateTime.Now;
}
```

**每新增一個偏好欄位，都需在 `else` 區塊補上對應的更新行。**

### IEmployeeService.UpdateSelfProfileAsync — 個人資料自助更新

```csharp
// 允許修改：Name、Mobile、Email、Password
// 不允許修改：Account、RoleId、DepartmentId 等（由管理員控制）
Task<ServiceResult> UpdateSelfProfileAsync(
    int employeeId,
    string name,
    string? mobile,
    string? email,
    string? newPassword  // 傳 null 或空白 = 不變更密碼
);
```

**實作重點：**
- 密碼雜湊在此方法內部執行（`SeedDataHelper.HashPassword`），UI 層無需處理
- `EmployeeEditModalComponent` 繼續使用 `UpdateAsync`（完整欄位），兩者邏輯不重疊

---

## 服務註冊

```csharp
// Data/ServiceRegistration.cs
services.AddScoped<IEmployeePreferenceService, EmployeePreferenceService>();
```

---

## Migration

每次加入新欄位後需執行：

```bash
dotnet ef migrations add AddEmployeePreference[FieldName]
dotnet ef database update
```

---

## 相關文件

- [README_個人化設定總綱.md](README_個人化設定總綱.md)
- [README_個人化設定_UI框架.md](README_個人化設定_UI框架.md)
- [README_個人化設定_語言切換.md](README_個人化設定_語言切換.md)
