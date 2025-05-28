# Blazor Server ERP 設計指南

## 架構概覽

本指南提供使用 Blazor Server 建構 ERP 系統的系統化方法，採用簡化且高效的架構。架構遵循以下依賴流程：

```
資料庫 ↔ DbContext (EF Core) ↔ Service ↔ Blazor Pages
```

### 核心原則
- **關注分離**：每一層都有單一責任
- **依賴反轉**：依賴抽象而非具體實作
- **SOLID 原則**：維持乾淨且可維護的程式碼
- **簡潔性**：避免不必要的抽象層，因為 Entity Framework Core 已經提供了這些功能
- **一致性**：在所有模組中遵循相同的模式

### 為什麼選擇這個架構？

#### **Entity Framework Core 本身就是完整的資料存取層**
Entity Framework Core 已經實作了所有必要的資料存取模式：
- `DbContext` = 工作單元 (Unit of Work)
- `DbSet<T>` = 實體 T 的資料集合
- 添加額外的資料存取層會產生不必要的複雜性

#### **Service 層的優點**
- **單一責任**：處理業務邏輯和資料存取
- **更好的效能**：直接存取 EF Core 的完整功能集
- **更容易維護**：更少的層級需要除錯和維護
- **現代化方法**：符合目前 .NET 最佳實務

---

## 1. 實體設計

### 目的
實體代表您的資料結構，是 ERP 系統的基礎。

### 設計準則

#### ✅ **最佳實務**
- 使用有意義的業務導向名稱
- 只包含資料屬性（不包含業務邏輯）
- 遵循 C# 命名慣例
- 添加資料註解進行驗證
- 保持實體簡單且專注

#### ❌ **常見陷阱**
- 不要在實體中包含業務方法
- 避免複雜的繼承階層
- 不要引用 UI 特定屬性

### 範本結構
```csharp
public class [實體名稱]
{
    // 主鍵
    public int Id { get; set; }
    
    // 必要屬性
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    // 選擇性屬性
    public string? Description { get; set; }
    
    // 稽核欄位
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    
    // 狀態
    public [實體名稱]Status Status { get; set; }
    
    // 外鍵（如果適用）
    public int? ParentId { get; set; }
}

public enum [實體名稱]Status
{
    Active = 1,
    Inactive = 2,
    Deleted = 3
}
```

### 設計檢查清單
- [ ] 實體名稱反映業務領域
- [ ] 所有必要屬性都有 `[Required]` 屬性
- [ ] 字串屬性都有 `[MaxLength]` 屬性
- [ ] 包含稽核欄位（CreatedDate、ModifiedDate）
- [ ] 如需要則定義狀態列舉
- [ ] 外鍵屬性遵循命名慣例

---

## 2. DbContext 設定

### 目的
DbContext 作為實體和資料庫之間的橋樑，管理連線和實體對應。

### 設計準則

#### ✅ **最佳實務**
- 明確設定實體關係
- 設定適當的欄位約束
- 使用有意義的資料表和欄位名稱
- 為效能設定索引
- 正確處理資料庫連線

#### ❌ **常見陷阱**
- 不要忽略關係設定
- 避免硬編碼連線字串
- 不要跳過約束定義

### 範本結構
```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    // 每個實體的 DbSet
    public DbSet<[實體名稱]> [實體名稱]s { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // 設定實體
        modelBuilder.Entity<[實體名稱]>(entity =>
        {
            // 主鍵
            entity.HasKey(e => e.Id);
            
            // 必要欄位
            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(100);
            
            // 選擇性欄位
            entity.Property(e => e.Description)
                  .HasMaxLength(500);
            
            // 預設值
            entity.Property(e => e.CreatedDate)
                  .HasDefaultValueSql("GETDATE()");
            
            // 索引
            entity.HasIndex(e => e.Name)
                  .IsUnique();
            
            // 關係（如果適用）
            entity.HasOne<ParentEntity>()
                  .WithMany()
                  .HasForeignKey(e => e.ParentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
```

### 設定檢查清單
- [ ] 所有實體都有 DbSet 屬性
- [ ] 主鍵明確設定
- [ ] 必要屬性標記為 IsRequired()
- [ ] 字串長度正確設定
- [ ] 為常查詢欄位建立索引
- [ ] 外鍵關係正確設定
- [ ] 適當的地方設定預設值

## 3. Service 實作

### 目的
Service 包含業務邏輯、資料存取操作、驗證，並直接使用 Entity Framework Core 協調複雜的工作流程。

### 設計準則

#### ✅ **最佳實務**
- 實作完整的業務驗證
- 回傳結構化結果（ServiceResult 模式）
- 處理複雜的業務工作流程
- 直接使用 EF Core 進行資料操作
- 包含適當的錯誤訊息
- 需要時實作交易管理

#### ❌ **常見陷阱**
- 不要繞過業務規則
- 避免與 UI 關注點緊密耦合
- 不要忽略驗證
- 不要在 EF Core 上建立不必要的抽象

### 範本結構
```csharp
// 請求/回應模型
public class Create[實體名稱]Request
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
}

public class Update[實體名稱]Request
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
}

// Service 結果模式
public class ServiceResult
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public List<string> ValidationErrors { get; set; } = new();
    
    public static ServiceResult Success() => new() { IsSuccess = true };
    public static ServiceResult Failure(string error) => new() { IsSuccess = false, ErrorMessage = error };
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; set; }
    
    public static ServiceResult<T> Success(T data) => new() { IsSuccess = true, Data = data };
    public static new ServiceResult<T> Failure(string error) => new() { IsSuccess = false, ErrorMessage = error };
}

// Service 介面
public interface I[實體名稱]Service
{
    Task<List<[實體名稱]>> GetAllAsync();
    Task<[實體名稱]?> GetByIdAsync(int id);
    Task<ServiceResult<[實體名稱]>> CreateAsync(Create[實體名稱]Request request);
    Task<ServiceResult<[實體名稱]>> UpdateAsync(Update[實體名稱]Request request);
    Task<ServiceResult> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<[實體名稱]?> GetByNameAsync(string name);
    Task<(List<[實體名稱]> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);
}

// Service 實作 - 直接使用 EF Core
public class [實體名稱]Service : I[實體名稱]Service
{
    private readonly AppDbContext _context;
    private readonly ILogger<[實體名稱]Service> _logger;
    
    public [實體名稱]Service(AppDbContext context, ILogger<[實體名稱]Service> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<List<[實體名稱]>> GetAllAsync()
    {
        try
        {
            return await _context.[實體名稱]s
                .Where(e => e.Status != [實體名稱]Status.Deleted)
                .OrderBy(e => e.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得所有 {EntityName} 時發生錯誤", typeof([實體名稱]).Name);
            throw;
        }
    }
    
    public async Task<[實體名稱]?> GetByIdAsync(int id)
    {
        if (id <= 0)
            return null;
        
        try
        {
            return await _context.[實體名稱]s
                .FirstOrDefaultAsync(e => e.Id == id && e.Status != [實體名稱]Status.Deleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得 ID 為 {Id} 的 {EntityName} 時發生錯誤", id, typeof([實體名稱]).Name);
            throw;
        }
    }
    
    public async Task<ServiceResult<[實體名稱]>> CreateAsync(Create[實體名稱]Request request)
    {
        try
        {
            // 業務驗證
            var validationResult = await ValidateCreateRequestAsync(request);
            if (!validationResult.IsSuccess)
                return ServiceResult<[實體名稱]>.Failure(validationResult.ErrorMessage);
            
            // 業務規則 - 檢查重複
            var existingEntity = await _context.[實體名稱]s
                .FirstOrDefaultAsync(e => e.Name == request.Name && e.Status != [實體名稱]Status.Deleted);
            
            if (existingEntity != null)
                return ServiceResult<[實體名稱]>.Failure("名稱已存在");
            
            // 建立實體
            var entity = new [實體名稱]
            {
                Name = request.Name,
                Description = request.Description,
                Status = [實體名稱]Status.Active,
                CreatedDate = DateTime.UtcNow
            };
            
            // 儲存到資料庫
            _context.[實體名稱]s.Add(entity);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("已建立 ID 為 {Id} 的 {EntityName}", entity.Id, typeof([實體名稱]).Name);
            return ServiceResult<[實體名稱]>.Success(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "建立 {EntityName} 時發生錯誤", typeof([實體名稱]).Name);
            return ServiceResult<[實體名稱]>.Failure("建立記錄時發生錯誤");
        }
    }
    
    public async Task<ServiceResult<[實體名稱]>> UpdateAsync(Update[實體名稱]Request request)
    {
        try
        {
            // 取得現有實體
            var entity = await _context.[實體名稱]s
                .FirstOrDefaultAsync(e => e.Id == request.Id && e.Status != [實體名稱]Status.Deleted);
            
            if (entity == null)
                return ServiceResult<[實體名稱]>.Failure("找不到實體");
            
            // 業務規則 - 檢查重複（排除目前實體）
            var existingEntity = await _context.[實體名稱]s
                .FirstOrDefaultAsync(e => e.Name == request.Name && 
                                        e.Id != request.Id && 
                                        e.Status != [實體名稱]Status.Deleted);
            
            if (existingEntity != null)
                return ServiceResult<[實體名稱]>.Failure("名稱已存在");
            
            // 更新實體
            entity.Name = request.Name;
            entity.Description = request.Description;
            entity.ModifiedDate = DateTime.UtcNow;
            
            // 儲存到資料庫
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("已更新 ID 為 {Id} 的 {EntityName}", entity.Id, typeof([實體名稱]).Name);
            return ServiceResult<[實體名稱]>.Success(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新 ID 為 {Id} 的 {EntityName} 時發生錯誤", request.Id, typeof([實體名稱]).Name);
            return ServiceResult<[實體名稱]>.Failure("更新記錄時發生錯誤");
        }
    }
    
    public async Task<ServiceResult> DeleteAsync(int id)
    {
        try
        {
            var entity = await _context.[實體名稱]s
                .FirstOrDefaultAsync(e => e.Id == id && e.Status != [實體名稱]Status.Deleted);
            
            if (entity == null)
                return ServiceResult.Failure("找不到實體");
            
            // 業務規則 - 檢查相依性
            // 範例：檢查實體是否被其他地方使用
            var hasReferences = await _context.SomeRelatedEntities
                .AnyAsync(r => r.[實體名稱]Id == id);
            
            if (hasReferences)
                return ServiceResult.Failure("無法刪除：此實體正被其他記錄使用");
            
            // 軟刪除
            entity.Status = [實體名稱]Status.Deleted;
            entity.ModifiedDate = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("已刪除 ID 為 {Id} 的 {EntityName}", id, typeof([實體名稱]).Name);
            return ServiceResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刪除 ID 為 {Id} 的 {EntityName} 時發生錯誤", id, typeof([實體名稱]).Name);
            return ServiceResult.Failure("刪除記錄時發生錯誤");
        }
    }
    
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.[實體名稱]s
            .AnyAsync(e => e.Id == id && e.Status != [實體名稱]Status.Deleted);
    }
    
    public async Task<[實體名稱]?> GetByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;
            
        return await _context.[實體名稱]s
            .FirstOrDefaultAsync(e => e.Name == name && e.Status != [實體名稱]Status.Deleted);
    }
    
    public async Task<(List<[實體名稱]> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
    {
        var query = _context.[實體名稱]s
            .Where(e => e.Status != [實體名稱]Status.Deleted);
        
        var totalCount = await query.CountAsync();
        
        var items = await query
            .OrderBy(e => e.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return (items, totalCount);
    }
    
    // 複雜業務操作範例
    public async Task<ServiceResult> ProcessComplexBusinessOperationAsync(int entityId, SomeBusinessRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 步驟 1：取得並驗證實體
            var entity = await GetByIdAsync(entityId);
            if (entity == null)
                return ServiceResult.Failure("找不到實體");
            
            // 步驟 2：業務驗證
            if (entity.Status != [實體名稱]Status.Active)
                return ServiceResult.Failure("非啟用狀態的實體不允許此操作");
            
            // 步驟 3：更新多個相關實體
            var relatedEntities = await _context.RelatedEntities
                .Where(r => r.[實體名稱]Id == entityId)
                .ToListAsync();
            
            foreach (var related in relatedEntities)
            {
                // 套用業務邏輯
                related.SomeProperty = CalculateSomeValue(related, request);
                related.ModifiedDate = DateTime.UtcNow;
            }
            
            // 步驟 4：更新主要實體
            entity.SomeCalculatedField = CalculateMainValue(entity, relatedEntities);
            entity.ModifiedDate = DateTime.UtcNow;
            
            // 步驟 5：儲存所有變更
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            return ServiceResult.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "實體 {Id} 的複雜業務操作發生錯誤", entityId);
            return ServiceResult.Failure("業務操作失敗");
        }
    }
    
    private async Task<ServiceResult> ValidateCreateRequestAsync(Create[實體名稱]Request request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return ServiceResult.Failure("名稱為必填欄位");
        
        if (request.Name.Length > 100)
            return ServiceResult.Failure("名稱不能超過 100 個字元");
        
        // 依需要添加更多業務驗證規則
        
        return ServiceResult.Success();
    }
}
```

### Service 檢查清單
- [ ] 介面定義包含所有必要方法
- [ ] 所有方法都是非同步且正確處理例外
- [ ] Service 直接使用 EF Core DbContext
- [ ] 實作業務驗證
- [ ] 實作軟刪除（使用 Status 欄位）
- [ ] 實作重複檢查
- [ ] 複雜操作的交易管理
- [ ] 實作適當的日誌記錄
- [ ] 一致使用 ServiceResult 模式
- [ ] 錯誤訊息對使用者友善

---

## 4. Blazor Pages 實作

### 目的
Pages 處理使用者介面、使用者輸入，並顯示來自 Service 的資料。

### 設計準則

#### ✅ **最佳實務**
- 保持 code-behind 專注於 UI 邏輯
- 使用適當的錯誤處理和使用者回饋
- 實作載入狀態
- 遵循一致的 UI 模式
- 有效使用資料繫結

#### ❌ **常見陷阱**
- 不要在頁面中包含業務邏輯
- 避免直接存取資料庫
- 不要忽略錯誤處理

### 範本結構
```razor
@page "/[實體名稱小寫]"
@inject I[實體名稱]Service [實體名稱]Service
@inject IJSRuntime JSRuntime

<PageTitle>[實體名稱] 管理</PageTitle>

<div class="container-fluid">
    <div class="row">
        <div class="col-12">
            <div class="d-flex justify-content-between align-items-center mb-3">
                <h3>[實體名稱] 管理</h3>
                <button class="btn btn-primary" @onclick="ShowCreateModal">
                    <i class="fas fa-plus"></i> 新增 [實體名稱]
                </button>
            </div>
        </div>
    </div>

    <!-- 載入狀態 -->
    @if (isLoading)
    {
        <div class="text-center">
            <div class="spinner-border" role="status">
                <span class="visually-hidden">載入中...</span>
            </div>
        </div>
    }
    
    <!-- 錯誤訊息 -->
    @if (!string.IsNullOrEmpty(errorMessage))
    {
        <div class="alert alert-danger alert-dismissible fade show" role="alert">
            @errorMessage
            <button type="button" class="btn-close" @onclick="ClearError"></button>
        </div>
    }
    
    <!-- 成功訊息 -->
    @if (!string.IsNullOrEmpty(successMessage))
    {
        <div class="alert alert-success alert-dismissible fade show" role="alert">
            @successMessage
            <button type="button" class="btn-close" @onclick="ClearSuccess"></button>
        </div>
    }

    <!-- 資料表格 -->
    @if (entities != null && entities.Any())
    {
        <div class="table-responsive">
            <table class="table table-striped">
                <thead>
                    <tr>
                        <th>名稱</th>
                        <th>描述</th>
                        <th>狀態</th>
                        <th>建立日期</th>
                        <th>操作</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var entity in entities)
                    {
                        <tr>
                            <td>@entity.Name</td>
                            <td>@entity.Description</td>
                            <td>
                                <span class="badge bg-@GetStatusColor(entity.Status)">
                                    @GetStatusText(entity.Status)
                                </span>
                            </td>
                            <td>@entity.CreatedDate.ToString("yyyy-MM-dd")</td>
                            <td>
                                <button class="btn btn-sm btn-outline-primary me-1" 
                                        @onclick="() => EditEntity(entity)">
                                    <i class="fas fa-edit"></i>
                                </button>
                                <button class="btn btn-sm btn-outline-danger" 
                                        @onclick="() => DeleteEntity(entity)">
                                    <i class="fas fa-trash"></i>
                                </button>
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>
    }
    else if (!isLoading)
    {
        <div class="text-center">
            <p>找不到記錄。</p>
        </div>
    }
</div>

<!-- 建立/編輯模態視窗 -->
<div class="modal fade" id="entityModal" tabindex="-1">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">@(isEditMode ? "編輯" : "建立") [實體名稱]</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <EditForm Model="currentEntity" OnValidSubmit="SaveEntity">
                    <DataAnnotationsValidator />
                    
                    <div class="mb-3">
                        <label class="form-label">名稱 *</label>
                        <InputText class="form-control" @bind-Value="currentEntity.Name" />
                        <ValidationMessage For="() => currentEntity.Name" />
                    </div>
                    
                    <div class="mb-3">
                        <label class="form-label">描述</label>
                        <InputTextArea class="form-control" @bind-Value="currentEntity.Description" rows="3" />
                        <ValidationMessage For="() => currentEntity.Description" />
                    </div>
                    
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">取消</button>
                        <button type="submit" class="btn btn-primary" disabled="@isSaving">
                            @if (isSaving)
                            {
                                <span class="spinner-border spinner-border-sm me-2"></span>
                            }
                            @(isEditMode ? "更新" : "建立")
                        </button>
                    </div>
                </EditForm>
            </div>
        </div>
    </div>
</div>

@code {
    private List<[實體名稱]>? entities;
    private Create[實體名稱]Request currentEntity = new();
    private bool isLoading = false;
    private bool isSaving = false;
    private bool isEditMode = false;
    private int editingId = 0;
    
    private string errorMessage = string.Empty;
    private string successMessage = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await LoadEntitiesAsync();
    }

    private async Task LoadEntitiesAsync()
    {
        try
        {
            isLoading = true;
            entities = await [實體名稱]Service.GetAllAsync();
        }
        catch (Exception ex)
        {
            errorMessage = "載入資料失敗，請重試。";
        }
        finally
        {
            isLoading = false;
        }
    }

    private void ShowCreateModal()
    {
        currentEntity = new Create[實體名稱]Request();
        isEditMode = false;
        editingId = 0;
        ShowModal();
    }

    private void EditEntity([實體名稱] entity)
    {
        currentEntity = new Create[實體名稱]Request
        {
            Name = entity.Name,
            Description = entity.Description
        };
        isEditMode = true;
        editingId = entity.Id;
        ShowModal();
    }

    private async Task SaveEntity()
    {
        try
        {
            isSaving = true;
            ClearMessages();

            ServiceResult result;

            if (isEditMode)
            {
                var updateRequest = new Update[實體名稱]Request
                {
                    Id = editingId,
                    Name = currentEntity.Name,
                    Description = currentEntity.Description
                };
                result = await [實體名稱]Service.UpdateAsync(updateRequest);
            }
            else
            {
                var createResult = await [實體名稱]Service.CreateAsync(currentEntity);
                result = createResult;
            }

            if (result.IsSuccess)
            {
                successMessage = isEditMode ? "更新成功！" : "建立成功！";
                await HideModal();
                await LoadEntitiesAsync();
            }
            else
            {
                errorMessage = result.ErrorMessage;
            }
        }
        catch (Exception ex)
        {
            errorMessage = "發生錯誤，請重試。";
        }
        finally
        {
            isSaving = false;
        }
    }

    private async Task DeleteEntity([實體名稱] entity)
    {
        if (await JSRuntime.InvokeAsync<bool>("confirm", $"您確定要刪除 '{entity.Name}' 嗎？"))
        {
            try
            {
                var result = await [實體名稱]Service.DeleteAsync(entity.Id);
                
                if (result.IsSuccess)
                {
                    successMessage = "刪除成功！";
                    await LoadEntitiesAsync();
                }
                else
                {
                    errorMessage = result.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                errorMessage = "刪除失敗，請重試。";
            }
        }
    }

    private async Task ShowModal()
    {
        await JSRuntime.InvokeVoidAsync("bootstrap.Modal.getOrCreateInstance", 
            await JSRuntime.InvokeAsync<object>("document.getElementById", "entityModal"))
            .InvokeVoidAsync("show");
    }

    private async Task HideModal()
    {
        await JSRuntime.InvokeVoidAsync("bootstrap.Modal.getInstance", 
            await JSRuntime.InvokeAsync<object>("document.getElementById", "entityModal"))
            .InvokeVoidAsync("hide");
    }

    private void ClearMessages()
    {
        errorMessage = string.Empty;
        successMessage = string.Empty;
    }

    private void ClearError() => errorMessage = string.Empty;
    private void ClearSuccess() => successMessage = string.Empty;

    private string GetStatusColor([實體名稱]Status status)
    {
        return status switch
        {
            [實體名稱]Status.Active => "success",
            [實體名稱]Status.Inactive => "warning",
            [實體名稱]Status.Deleted => "danger",
            _ => "secondary"
        };
    }
    
    private string GetStatusText([實體名稱]Status status)
    {
        return status switch
        {
            [實體名稱]Status.Active => "啟用",
            [實體名稱]Status.Inactive => "停用",
            [實體名稱]Status.Deleted => "已刪除",
            _ => "未知"
        };
    }
}
```

### Pages 檢查清單
- [ ] 實作適當的錯誤處理
- [ ] 向使用者顯示載入狀態
- [ ] 顯示成功/錯誤訊息
- [ ] 包含表單驗證
- [ ] 對破壞性動作提供確認對話框
- [ ] 考慮響應式設計
- [ ] 遵循一致的 UI 模式

---

## 5. 依賴注入註冊

### 目的
在應用程式啟動時設定所有服務和相依性。

### 範本結構
```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// 資料庫設定
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Service 註冊
builder.Services.AddScoped<I[實體名稱]Service, [實體名稱]Service>();

// Blazor Server
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var app = builder.Build();

// 設定 HTTP 請求管線
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
```

### 註冊檢查清單
- [ ] DbContext 已使用連線字串註冊
- [ ] 所有服務都已使用其介面註冊
- [ ] 選擇適當的服務生命週期（大多數情況下使用 Scoped）

---

## 測試策略

### 使用 EF Core In-Memory 資料庫進行單元測試
```csharp
[TestFixture]
public class [實體名稱]ServiceTests
{
    private AppDbContext _context;
    private [實體名稱]Service _service;
    private ILogger<[實體名稱]Service> _logger;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _logger = new Mock<ILogger<[實體名稱]Service>>().Object;
        _service = new [實體名稱]Service(_context, _logger);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task CreateAsync_應該回傳成功_當資料有效時()
    {
        // Arrange
        var request = new Create[實體名稱]Request
        {
            Name = "測試實體",
            Description = "測試描述"
        };

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual("測試實體", result.Data.Name);
    }

    [Test]
    public async Task CreateAsync_應該回傳失敗_當名稱重複時()
    {
        // Arrange
        var existingEntity = new [實體名稱]
        {
            Name = "現有實體",
            Status = [實體名稱]Status.Active,
            CreatedDate = DateTime.UtcNow
        };
        _context.[實體名稱]s.Add(existingEntity);
        await _context.SaveChangesAsync();

        var request = new Create[實體名稱]Request
        {
            Name = "現有實體",
            Description = "測試描述"
        };

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("名稱已存在", result.ErrorMessage);
    }
}
```

### 整合測試
- 測試完整的端到端工作流程
- 使用真實資料庫測試資料庫操作
- 如適用，測試 API 端點

---

## 一致性準則

### 命名慣例
- **實體**：單數名詞（Customer、Order、Product）
- **DbSets**：複數名詞（Customers、Orders、Products）  
- **Services**：[實體]Service
- **介面**：I[實體]Service

### 檔案組織
```
/Data
  /Entities
    Customer.cs
    Order.cs
  /Context
    AppDbContext.cs
/Services
  /Models
    CustomerModels.cs
  /Interfaces
    ICustomerService.cs
  CustomerService.cs
/Pages
  /Customers
    Index.razor
    Create.razor
    Edit.razor
```

### 簡化架構的優點

#### **效能**
- 直接存取 EF Core 提供最佳效能
- 沒有不必要的層級間資料對應
- 完全存取 EF Core 的進階功能（Include、Select 等）

#### **可維護性**
- 更少的層級 = 更容易除錯
- 更少的樣板程式碼需要維護
- 業務邏輯的單一真相來源

#### **可測試性**
- 使用 EF Core In-Memory 資料庫進行單元測試
- 容易模擬 DbContext 進行隔離測試
- 不需要複雜的模擬

#### **現代化方法**
- 符合微軟的建議
- 善用 EF Core 的內建模式
- 減少過度工程

### 程式碼品質標準
- 一致使用 async/await
- 實作適當的錯誤處理
- 遵循 C# 編碼慣例
- 為公共 API 添加 XML 文件
- 使用有意義的變數和方法名稱
- 保持方法專注且單一目的

---

### 要遵循的常見模式

### 錯誤處理
- 對業務操作使用 ServiceResult 模式
- 提供對使用者友善的錯誤訊息
- 記錄技術錯誤以供除錯
- 優雅地處理資料庫例外

### 驗證
- 在實體和請求模型上使用資料註解
- 在服務中實作業務驗證
- 向使用者提供清晰的驗證回饋

### 交易管理
- 對複雜操作使用 EF Core 交易
- 確保多個操作間的資料一致性
- 實作適當的回滾機制

### 安全性考量
- 驗證所有使用者輸入
- 實作適當的授權
- 在資料庫操作前清理資料
- 使用參數化查詢（EF Core 自動處理）

### 效能最佳化
- 一致使用 async/await
- 在資料庫中實作適當的索引
- 對大型資料集使用 Select 投影
- 對大型結果集實作分頁
- 謹慎使用 Include() 以避免 N+1 查詢

---

本指南提供了使用 Blazor Server 和 Entity Framework Core 建構 ERP 系統的現代、高效基礎。通過簡化架構，您可以獲得更好的效能、更容易的維護和更乾淨的程式碼，同時仍然遵循 SOLID 原則並保持可測試性。