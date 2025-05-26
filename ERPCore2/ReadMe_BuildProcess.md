# Blazor Server ERP Design Guide

## Architecture Overview

This guide provides a systematic approach to building ERP systems using Blazor Server with a clean, layered architecture. The architecture follows the dependency flow:

```
Database ↔ DbContext ↔ Repository ↔ Service ↔ Blazor Pages
```

### Core Principles
- **Separation of Concerns**: Each layer has a single responsibility
- **Dependency Inversion**: Depend on abstractions, not concrete implementations
- **SOLID Principles**: Maintain clean, maintainable code
- **Consistency**: Follow the same patterns across all modules

---

## 1. Entity Design

### Purpose
Entities represent your data structure and serve as the foundation of your ERP system.

### Design Guidelines

#### ✅ **Best Practices**
- Use meaningful, business-oriented names
- Include only data properties (no business logic)
- Follow C# naming conventions
- Add data annotations for validation
- Keep entities simple and focused

#### ❌ **Common Pitfalls**
- Don't include business methods in entities
- Avoid complex inheritance hierarchies
- Don't reference UI-specific properties

### Template Structure
```csharp
public class [EntityName]
{
    // Primary Key
    public int Id { get; set; }
    
    // Required Properties
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    // Optional Properties
    public string? Description { get; set; }
    
    // Audit Fields
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    
    // Status/State
    public [EntityName]Status Status { get; set; }
    
    // Foreign Keys (if applicable)
    public int? ParentId { get; set; }
}

public enum [EntityName]Status
{
    Active = 1,
    Inactive = 2,
    Deleted = 3
}
```

### Design Checklist
- [ ] Entity name reflects business domain
- [ ] All required properties have `[Required]` attribute
- [ ] String properties have `[MaxLength]` attribute
- [ ] Audit fields (CreatedDate, ModifiedDate) included
- [ ] Status enum defined if needed
- [ ] Foreign key properties follow naming convention

---

## 2. DbContext Configuration

### Purpose
DbContext serves as the bridge between your entities and the database, managing connections and entity mapping.

### Design Guidelines

#### ✅ **Best Practices**
- Configure entity relationships explicitly
- Set appropriate field constraints
- Use meaningful table and column names
- Configure indexes for performance
- Handle database connection properly

#### ❌ **Common Pitfalls**
- Don't ignore relationship configurations
- Avoid hard-coding connection strings
- Don't skip constraint definitions

### Template Structure
```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    // DbSets for each entity
    public DbSet<[EntityName]> [EntityName]s { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure Entity
        modelBuilder.Entity<[EntityName]>(entity =>
        {
            // Primary Key
            entity.HasKey(e => e.Id);
            
            // Required Fields
            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(100);
            
            // Optional Fields
            entity.Property(e => e.Description)
                  .HasMaxLength(500);
            
            // Default Values
            entity.Property(e => e.CreatedDate)
                  .HasDefaultValueSql("GETDATE()");
            
            // Indexes
            entity.HasIndex(e => e.Name)
                  .IsUnique();
            
            // Relationships (if applicable)
            entity.HasOne<ParentEntity>()
                  .WithMany()
                  .HasForeignKey(e => e.ParentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
```

### Configuration Checklist
- [ ] All entities have DbSet properties
- [ ] Primary keys explicitly configured
- [ ] Required properties marked as IsRequired()
- [ ] String lengths properly set
- [ ] Indexes created for frequently queried fields
- [ ] Foreign key relationships properly configured
- [ ] Default values set where appropriate

---

## 3. Repository Implementation

### Purpose
Repository handles all data access operations, providing a clean interface between the service layer and the database.

### Design Guidelines

#### ✅ **Best Practices**
- Keep methods simple and focused on data access
- Use async/await for all database operations
- Follow consistent naming conventions
- Implement interface for testability
- Handle common query patterns

#### ❌ **Common Pitfalls**
- Don't include business logic in repositories
- Avoid exposing IQueryable directly
- Don't ignore error handling

### Template Structure
```csharp
// Interface Definition
public interface I[EntityName]Repository
{
    // Basic CRUD Operations
    Task<List<[EntityName]>> GetAllAsync();
    Task<[EntityName]?> GetByIdAsync(int id);
    Task<[EntityName]> AddAsync([EntityName] entity);
    Task<[EntityName]> UpdateAsync([EntityName] entity);
    Task DeleteAsync(int id);
    
    // Common Query Patterns
    Task<bool> ExistsAsync(int id);
    Task<[EntityName]?> GetByNameAsync(string name);
    Task<List<[EntityName]>> GetActiveAsync();
    Task<List<[EntityName]>> GetByStatusAsync([EntityName]Status status);
    
    // Pagination Support
    Task<(List<[EntityName]> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);
}

// Implementation
public class [EntityName]Repository : I[EntityName]Repository
{
    private readonly AppDbContext _context;
    
    public [EntityName]Repository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<[EntityName]>> GetAllAsync()
    {
        return await _context.[EntityName]s
            .Where(e => e.Status != [EntityName]Status.Deleted)
            .OrderBy(e => e.Name)
            .ToListAsync();
    }
    
    public async Task<[EntityName]?> GetByIdAsync(int id)
    {
        return await _context.[EntityName]s
            .FirstOrDefaultAsync(e => e.Id == id && e.Status != [EntityName]Status.Deleted);
    }
    
    public async Task<[EntityName]> AddAsync([EntityName] entity)
    {
        entity.CreatedDate = DateTime.UtcNow;
        entity.Status = [EntityName]Status.Active;
        
        _context.[EntityName]s.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    
    public async Task<[EntityName]> UpdateAsync([EntityName] entity)
    {
        entity.ModifiedDate = DateTime.UtcNow;
        
        _context.[EntityName]s.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    
    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            entity.Status = [EntityName]Status.Deleted;
            entity.ModifiedDate = DateTime.UtcNow;
            await UpdateAsync(entity);
        }
    }
    
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.[EntityName]s
            .AnyAsync(e => e.Id == id && e.Status != [EntityName]Status.Deleted);
    }
}
```

### Repository Checklist
- [ ] Interface defined with all necessary methods
- [ ] All methods are async
- [ ] Soft delete implemented (using Status field)
- [ ] Common query methods included
- [ ] Proper error handling implemented
- [ ] Pagination support added if needed

---

## 4. Service Implementation

### Purpose
Services contain business logic, validation, and orchestrate operations across multiple repositories.

### Design Guidelines

#### ✅ **Best Practices**
- Implement comprehensive business validation
- Return structured results (ServiceResult pattern)
- Handle complex business workflows
- Coordinate multiple repositories when needed
- Include proper error messages

#### ❌ **Common Pitfalls**
- Don't bypass business rules
- Avoid tight coupling to UI concerns
- Don't ignore validation

### Template Structure
```csharp
// Request/Response Models
public class Create[EntityName]Request
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
}

public class Update[EntityName]Request
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
}

// Service Result Pattern
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

// Service Implementation
public class [EntityName]Service
{
    private readonly I[EntityName]Repository _repository;
    
    public [EntityName]Service(I[EntityName]Repository repository)
    {
        _repository = repository;
    }
    
    public async Task<List<[EntityName]>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }
    
    public async Task<[EntityName]?> GetByIdAsync(int id)
    {
        if (id <= 0)
            return null;
            
        return await _repository.GetByIdAsync(id);
    }
    
    public async Task<ServiceResult<[EntityName]>> CreateAsync(Create[EntityName]Request request)
    {
        // Business Validation
        var validationResult = await ValidateCreateRequestAsync(request);
        if (!validationResult.IsSuccess)
            return ServiceResult<[EntityName]>.Failure(validationResult.ErrorMessage);
        
        // Business Rules
        var existingEntity = await _repository.GetByNameAsync(request.Name);
        if (existingEntity != null)
            return ServiceResult<[EntityName]>.Failure("Name already exists");
        
        // Create Entity
        var entity = new [EntityName]
        {
            Name = request.Name,
            Description = request.Description,
            Status = [EntityName]Status.Active
        };
        
        // Save
        var result = await _repository.AddAsync(entity);
        
        return ServiceResult<[EntityName]>.Success(result);
    }
    
    public async Task<ServiceResult<[EntityName]>> UpdateAsync(Update[EntityName]Request request)
    {
        // Business Validation
        var entity = await _repository.GetByIdAsync(request.Id);
        if (entity == null)
            return ServiceResult<[EntityName]>.Failure("Entity not found");
        
        // Business Rules
        var existingEntity = await _repository.GetByNameAsync(request.Name);
        if (existingEntity != null && existingEntity.Id != request.Id)
            return ServiceResult<[EntityName]>.Failure("Name already exists");
        
        // Update Entity
        entity.Name = request.Name;
        entity.Description = request.Description;
        
        // Save
        var result = await _repository.UpdateAsync(entity);
        
        return ServiceResult<[EntityName]>.Success(result);
    }
    
    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return ServiceResult.Failure("Entity not found");
        
        // Business Rules - Check for dependencies
        // Add your business logic here
        
        await _repository.DeleteAsync(id);
        
        return ServiceResult.Success();
    }
    
    private async Task<ServiceResult> ValidateCreateRequestAsync(Create[EntityName]Request request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return ServiceResult.Failure("Name is required");
        
        // Add more validation rules as needed
        
        return ServiceResult.Success();
    }
}
```

### Service Checklist
- [ ] Request/Response models defined
- [ ] ServiceResult pattern implemented
- [ ] Business validation included
- [ ] Duplicate check implemented
- [ ] Error messages are user-friendly
- [ ] Complex business rules handled
- [ ] Dependencies properly injected

---

## 5. Blazor Pages Implementation

### Purpose
Pages handle user interface, user input, and display data from services.

### Design Guidelines

#### ✅ **Best Practices**
- Keep code-behind focused on UI logic
- Use proper error handling and user feedback
- Implement loading states
- Follow consistent UI patterns
- Use data binding effectively

#### ❌ **Common Pitfalls**
- Don't include business logic in pages
- Avoid direct database access
- Don't ignore error handling

### Template Structure
```razor
@page "/[entityname]"
@inject [EntityName]Service [EntityName]Service
@inject IJSRuntime JSRuntime

<PageTitle>[EntityName] Management</PageTitle>

<div class="container-fluid">
    <div class="row">
        <div class="col-12">
            <div class="d-flex justify-content-between align-items-center mb-3">
                <h3>[EntityName] Management</h3>
                <button class="btn btn-primary" @onclick="ShowCreateModal">
                    <i class="fas fa-plus"></i> Add [EntityName]
                </button>
            </div>
        </div>
    </div>

    <!-- Loading State -->
    @if (isLoading)
    {
        <div class="text-center">
            <div class="spinner-border" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
        </div>
    }
    
    <!-- Error Message -->
    @if (!string.IsNullOrEmpty(errorMessage))
    {
        <div class="alert alert-danger alert-dismissible fade show" role="alert">
            @errorMessage
            <button type="button" class="btn-close" @onclick="ClearError"></button>
        </div>
    }
    
    <!-- Success Message -->
    @if (!string.IsNullOrEmpty(successMessage))
    {
        <div class="alert alert-success alert-dismissible fade show" role="alert">
            @successMessage
            <button type="button" class="btn-close" @onclick="ClearSuccess"></button>
        </div>
    }

    <!-- Data Table -->
    @if (entities != null && entities.Any())
    {
        <div class="table-responsive">
            <table class="table table-striped">
                <thead>
                    <tr>
                        <th>Name</th>
                        <th>Description</th>
                        <th>Status</th>
                        <th>Created Date</th>
                        <th>Actions</th>
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
                                    @entity.Status
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
            <p>No records found.</p>
        </div>
    }
</div>

<!-- Create/Edit Modal -->
<div class="modal fade" id="entityModal" tabindex="-1">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">@(isEditMode ? "Edit" : "Create") [EntityName]</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <EditForm Model="currentEntity" OnValidSubmit="SaveEntity">
                    <DataAnnotationsValidator />
                    
                    <div class="mb-3">
                        <label class="form-label">Name *</label>
                        <InputText class="form-control" @bind-Value="currentEntity.Name" />
                        <ValidationMessage For="() => currentEntity.Name" />
                    </div>
                    
                    <div class="mb-3">
                        <label class="form-label">Description</label>
                        <InputTextArea class="form-control" @bind-Value="currentEntity.Description" rows="3" />
                        <ValidationMessage For="() => currentEntity.Description" />
                    </div>
                    
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                        <button type="submit" class="btn btn-primary" disabled="@isSaving">
                            @if (isSaving)
                            {
                                <span class="spinner-border spinner-border-sm me-2"></span>
                            }
                            @(isEditMode ? "Update" : "Create")
                        </button>
                    </div>
                </EditForm>
            </div>
        </div>
    </div>
</div>

@code {
    private List<[EntityName]>? entities;
    private Create[EntityName]Request currentEntity = new();
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
            entities = await [EntityName]Service.GetAllAsync();
        }
        catch (Exception ex)
        {
            errorMessage = "Failed to load data. Please try again.";
        }
        finally
        {
            isLoading = false;
        }
    }

    private void ShowCreateModal()
    {
        currentEntity = new Create[EntityName]Request();
        isEditMode = false;
        editingId = 0;
        ShowModal();
    }

    private void EditEntity([EntityName] entity)
    {
        currentEntity = new Create[EntityName]Request
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
                var updateRequest = new Update[EntityName]Request
                {
                    Id = editingId,
                    Name = currentEntity.Name,
                    Description = currentEntity.Description
                };
                result = await [EntityName]Service.UpdateAsync(updateRequest);
            }
            else
            {
                var createResult = await [EntityName]Service.CreateAsync(currentEntity);
                result = createResult;
            }

            if (result.IsSuccess)
            {
                successMessage = isEditMode ? "Updated successfully!" : "Created successfully!";
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
            errorMessage = "An error occurred. Please try again.";
        }
        finally
        {
            isSaving = false;
        }
    }

    private async Task DeleteEntity([EntityName] entity)
    {
        if (await JSRuntime.InvokeAsync<bool>("confirm", $"Are you sure you want to delete '{entity.Name}'?"))
        {
            try
            {
                var result = await [EntityName]Service.DeleteAsync(entity.Id);
                
                if (result.IsSuccess)
                {
                    successMessage = "Deleted successfully!";
                    await LoadEntitiesAsync();
                }
                else
                {
                    errorMessage = result.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                errorMessage = "Failed to delete. Please try again.";
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

    private string GetStatusColor([EntityName]Status status)
    {
        return status switch
        {
            [EntityName]Status.Active => "success",
            [EntityName]Status.Inactive => "warning",
            [EntityName]Status.Deleted => "danger",
            _ => "secondary"
        };
    }
}
```

### Pages Checklist
- [ ] Proper error handling implemented
- [ ] Loading states shown to users
- [ ] Success/error messages displayed
- [ ] Form validation included
- [ ] Confirmation dialogs for destructive actions
- [ ] Responsive design considerations
- [ ] Consistent UI patterns followed

---

## 6. Dependency Injection Registration

### Purpose
Configure all services and dependencies in the application startup.

### Template Structure
```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Database Configuration
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository Registration
builder.Services.AddScoped<I[EntityName]Repository, [EntityName]Repository>();

// Service Registration  
builder.Services.AddScoped<[EntityName]Service>();

// Blazor Server
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var app = builder.Build();

// Configure the HTTP request pipeline
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

### Registration Checklist
- [ ] DbContext registered with connection string
- [ ] All repositories registered with interfaces
- [ ] All services registered
- [ ] Proper service lifetimes chosen (Scoped for most cases)

---

## Consistency Guidelines

### Naming Conventions
- **Entities**: Singular nouns (Customer, Order, Product)
- **DbSets**: Plural nouns (Customers, Orders, Products)  
- **Repositories**: [Entity]Repository
- **Services**: [Entity]Service
- **Interfaces**: I[Entity]Repository

### File Organization
```
/Data
  /Entities
    Customer.cs
    Order.cs
  /Context
    AppDbContext.cs
/Repositories
  /Interfaces
    ICustomerRepository.cs
  CustomerRepository.cs
/Services
  /Models
    CustomerModels.cs
  CustomerService.cs
/Pages
  /Customers
    Index.razor
    Create.razor
    Edit.razor
```

### Code Quality Standards
- Use async/await consistently
- Implement proper error handling
- Follow C# coding conventions
- Add XML documentation for public APIs
- Use meaningful variable and method names
- Keep methods focused and single-purpose

---

## Testing Strategy

### Unit Testing
- Test services with mocked repositories
- Test repository logic with in-memory database
- Test business validation logic

### Integration Testing
- Test complete workflows end-to-end
- Test database operations
- Test API endpoints if applicable

---

## Common Patterns to Follow

### Error Handling
- Use ServiceResult pattern for business operations
- Provide user-friendly error messages
- Log technical errors for debugging

### Validation
- Use data annotations on entities and request models
- Implement business validation in services
- Provide clear validation feedback to users

### Security Considerations
- Validate all user inputs
- Implement proper authorization
- Sanitize data before database operations
- Use parameterized queries (EF Core handles this)

---

This guide provides a solid foundation for building consistent, maintainable ERP systems using Blazor Server. Follow these patterns and guidelines to ensure code quality and system reliability across your entire application.