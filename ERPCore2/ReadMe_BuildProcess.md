# Blazor Server ERP Design Guide

## Architecture Overview

This guide provides a systematic approach to building ERP systems using Blazor Server with a simplified, efficient architecture. The architecture follows the dependency flow:

```
Database ↔ DbContext (EF Core) ↔ Service ↔ Blazor Pages/Controllers
```

### Core Principles
- **Separation of Concerns**: Each layer has a single responsibility
- **Dependency Inversion**: Depend on abstractions, not concrete implementations
- **SOLID Principles**: Maintain clean, maintainable code
- **Simplicity**: Avoid unnecessary abstractions when Entity Framework Core already provides them
- **Consistency**: Follow the same patterns across all modules

### Why This Architecture?

#### **Entity Framework Core IS the Repository**
Entity Framework Core already implements the Repository and Unit of Work patterns:
- `DbContext` = Unit of Work
- `DbSet<T>` = Repository for entity T
- Adding another repository layer creates unnecessary complexity

#### **Service Layer Benefits**
- **Single Responsibility**: Handles both business logic and data access
- **Better Performance**: Direct access to EF Core's full feature set
- **Easier Maintenance**: Fewer layers to debug and maintain
- **Modern Approach**: Aligns with current .NET best practices

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

## 3. Service Implementation

### Purpose
Services contain business logic, data access operations, validation, and orchestrate complex workflows using Entity Framework Core directly.

### Design Guidelines

#### ✅ **Best Practices**
- Implement comprehensive business validation
- Return structured results (ServiceResult pattern)
- Handle complex business workflows
- Use EF Core directly for data operations
- Include proper error messages
- Implement transaction management when needed

#### ❌ **Common Pitfalls**
- Don't bypass business rules
- Avoid tight coupling to UI concerns
- Don't ignore validation
- Don't create unnecessary abstractions over EF Core

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

// Service Interface
public interface I[EntityName]Service
{
    Task<List<[EntityName]>> GetAllAsync();
    Task<[EntityName]?> GetByIdAsync(int id);
    Task<ServiceResult<[EntityName]>> CreateAsync(Create[EntityName]Request request);
    Task<ServiceResult<[EntityName]>> UpdateAsync(Update[EntityName]Request request);
    Task<ServiceResult> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<[EntityName]?> GetByNameAsync(string name);
    Task<(List<[EntityName]> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);
}

// Service Implementation - Using EF Core Directly
public class [EntityName]Service : I[EntityName]Service
{
    private readonly AppDbContext _context;
    private readonly ILogger<[EntityName]Service> _logger;
    
    public [EntityName]Service(AppDbContext context, ILogger<[EntityName]Service> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<List<[EntityName]>> GetAllAsync()
    {
        try
        {
            return await _context.[EntityName]s
                .Where(e => e.Status != [EntityName]Status.Deleted)
                .OrderBy(e => e.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all {EntityName}s", typeof([EntityName]).Name);
            throw;
        }
    }
    
    public async Task<[EntityName]?> GetByIdAsync(int id)
    {
        if (id <= 0)
            return null;
        
        try
        {
            return await _context.[EntityName]s
                .FirstOrDefaultAsync(e => e.Id == id && e.Status != [EntityName]Status.Deleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting {EntityName} with ID {Id}", typeof([EntityName]).Name, id);
            throw;
        }
    }
    
    public async Task<ServiceResult<[EntityName]>> CreateAsync(Create[EntityName]Request request)
    {
        try
        {
            // Business Validation
            var validationResult = await ValidateCreateRequestAsync(request);
            if (!validationResult.IsSuccess)
                return ServiceResult<[EntityName]>.Failure(validationResult.ErrorMessage);
            
            // Business Rules - Check for duplicates
            var existingEntity = await _context.[EntityName]s
                .FirstOrDefaultAsync(e => e.Name == request.Name && e.Status != [EntityName]Status.Deleted);
            
            if (existingEntity != null)
                return ServiceResult<[EntityName]>.Failure("Name already exists");
            
            // Create Entity
            var entity = new [EntityName]
            {
                Name = request.Name,
                Description = request.Description,
                Status = [EntityName]Status.Active,
                CreatedDate = DateTime.UtcNow
            };
            
            // Save to Database
            _context.[EntityName]s.Add(entity);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("{EntityName} created with ID {Id}", typeof([EntityName]).Name, entity.Id);
            return ServiceResult<[EntityName]>.Success(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating {EntityName}", typeof([EntityName]).Name);
            return ServiceResult<[EntityName]>.Failure("An error occurred while creating the record");
        }
    }
    
    public async Task<ServiceResult<[EntityName]>> UpdateAsync(Update[EntityName]Request request)
    {
        try
        {
            // Get existing entity
            var entity = await _context.[EntityName]s
                .FirstOrDefaultAsync(e => e.Id == request.Id && e.Status != [EntityName]Status.Deleted);
            
            if (entity == null)
                return ServiceResult<[EntityName]>.Failure("Entity not found");
            
            // Business Rules - Check for duplicates (excluding current entity)
            var existingEntity = await _context.[EntityName]s
                .FirstOrDefaultAsync(e => e.Name == request.Name && 
                                        e.Id != request.Id && 
                                        e.Status != [EntityName]Status.Deleted);
            
            if (existingEntity != null)
                return ServiceResult<[EntityName]>.Failure("Name already exists");
            
            // Update Entity
            entity.Name = request.Name;
            entity.Description = request.Description;
            entity.ModifiedDate = DateTime.UtcNow;
            
            // Save to Database
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("{EntityName} updated with ID {Id}", typeof([EntityName]).Name, entity.Id);
            return ServiceResult<[EntityName]>.Success(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating {EntityName} with ID {Id}", typeof([EntityName]).Name, request.Id);
            return ServiceResult<[EntityName]>.Failure("An error occurred while updating the record");
        }
    }
    
    public async Task<ServiceResult> DeleteAsync(int id)
    {
        try
        {
            var entity = await _context.[EntityName]s
                .FirstOrDefaultAsync(e => e.Id == id && e.Status != [EntityName]Status.Deleted);
            
            if (entity == null)
                return ServiceResult.Failure("Entity not found");
            
            // Business Rules - Check for dependencies
            // Example: Check if entity is being used elsewhere
            var hasReferences = await _context.SomeRelatedEntities
                .AnyAsync(r => r.[EntityName]Id == id);
            
            if (hasReferences)
                return ServiceResult.Failure("Cannot delete: Entity is being used by other records");
            
            // Soft Delete
            entity.Status = [EntityName]Status.Deleted;
            entity.ModifiedDate = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("{EntityName} deleted with ID {Id}", typeof([EntityName]).Name, id);
            return ServiceResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting {EntityName} with ID {Id}", typeof([EntityName]).Name, id);
            return ServiceResult.Failure("An error occurred while deleting the record");
        }
    }
    
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.[EntityName]s
            .AnyAsync(e => e.Id == id && e.Status != [EntityName]Status.Deleted);
    }
    
    public async Task<[EntityName]?> GetByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;
            
        return await _context.[EntityName]s
            .FirstOrDefaultAsync(e => e.Name == name && e.Status != [EntityName]Status.Deleted);
    }
    
    public async Task<(List<[EntityName]> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
    {
        var query = _context.[EntityName]s
            .Where(e => e.Status != [EntityName]Status.Deleted);
        
        var totalCount = await query.CountAsync();
        
        var items = await query
            .OrderBy(e => e.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return (items, totalCount);
    }
    
    // Complex Business Operations Example
    public async Task<ServiceResult> ProcessComplexBusinessOperationAsync(int entityId, SomeBusinessRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Step 1: Get and validate entity
            var entity = await GetByIdAsync(entityId);
            if (entity == null)
                return ServiceResult.Failure("Entity not found");
            
            // Step 2: Business validation
            if (entity.Status != [EntityName]Status.Active)
                return ServiceResult.Failure("Operation not allowed for inactive entities");
            
            // Step 3: Update multiple related entities
            var relatedEntities = await _context.RelatedEntities
                .Where(r => r.[EntityName]Id == entityId)
                .ToListAsync();
            
            foreach (var related in relatedEntities)
            {
                // Apply business logic
                related.SomeProperty = CalculateSomeValue(related, request);
                related.ModifiedDate = DateTime.UtcNow;
            }
            
            // Step 4: Update main entity
            entity.SomeCalculatedField = CalculateMainValue(entity, relatedEntities);
            entity.ModifiedDate = DateTime.UtcNow;
            
            // Step 5: Save all changes
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            return ServiceResult.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error in complex business operation for entity {Id}", entityId);
            return ServiceResult.Failure("Business operation failed");
        }
    }
    
    private async Task<ServiceResult> ValidateCreateRequestAsync(Create[EntityName]Request request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return ServiceResult.Failure("Name is required");
        
        if (request.Name.Length > 100)
            return ServiceResult.Failure("Name cannot exceed 100 characters");
        
        // Add more business validation rules as needed
        
        return ServiceResult.Success();
    }
}
```

### Service Checklist
- [ ] Interface defined with all necessary methods
- [ ] All methods are async and properly handle exceptions
- [ ] Service uses EF Core DbContext directly
- [ ] Business validation implemented
- [ ] Soft delete implemented (using Status field)
- [ ] Duplicate checking implemented
- [ ] Transaction management for complex operations
- [ ] Proper logging implemented
- [ ] ServiceResult pattern used consistently
- [ ] Error messages are user-friendly

---

## 4. Blazor Pages Implementation

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
@inject I[EntityName]Service [EntityName]Service
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

## 5. Dependency Injection Registration

### Purpose
Configure all services and dependencies in the application startup.

### Template Structure
```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Database Configuration
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Service Registration (No Repository needed!)
builder.Services.AddScoped<I[EntityName]Service, [EntityName]Service>();

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
- [ ] All services registered with their interfaces
- [ ] Proper service lifetimes chosen (Scoped for most cases)
- [ ] No repository registrations needed

---

## Testing Strategy

### Unit Testing with EF Core In-Memory Database
```csharp
[TestFixture]
public class [EntityName]ServiceTests
{
    private AppDbContext _context;
    private [EntityName]Service _service;
    private ILogger<[EntityName]Service> _logger;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _logger = new Mock<ILogger<[EntityName]Service>>().Object;
        _service = new [EntityName]Service(_context, _logger);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task CreateAsync_ShouldReturnSuccess_WhenValidData()
    {
        // Arrange
        var request = new Create[EntityName]Request
        {
            Name = "Test Entity",
            Description = "Test Description"
        };

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual("Test Entity", result.Data.Name);
    }

    [Test]
    public async Task CreateAsync_ShouldReturnFailure_WhenDuplicateName()
    {
        // Arrange
        var existingEntity = new [EntityName]
        {
            Name = "Existing Entity",
            Status = [EntityName]Status.Active,
            CreatedDate = DateTime.UtcNow
        };
        _context.[EntityName]s.Add(existingEntity);
        await _context.SaveChangesAsync();

        var request = new Create[EntityName]Request
        {
            Name = "Existing Entity",
            Description = "Test Description"
        };

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Name already exists", result.ErrorMessage);
    }
}
```

### Integration Testing
- Test complete workflows end-to-end
- Test database operations with real database
- Test API endpoints if applicable

---

## 6. Dependency Injection Registration

---

## Consistency Guidelines

### Naming Conventions
- **Entities**: Singular nouns (Customer, Order, Product)
- **DbSets**: Plural nouns (Customers, Orders, Products)  
- **Services**: [Entity]Service
- **Interfaces**: I[Entity]Service

### File Organization
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

### Simplified Architecture Benefits

#### **Performance**
- Direct EF Core access provides optimal performance
- No unnecessary data mapping between layers
- Full access to EF Core's advanced features (Include, Select, etc.)

#### **Maintainability**
- Fewer layers = easier debugging
- Less boilerplate code to maintain
- Single point of truth for business logic

#### **Testability**
- EF Core In-Memory Database for unit testing
- Easy to mock DbContext for isolated testing
- No complex repository mocking required

#### **Modern Approach**
- Aligns with Microsoft's recommendations
- Leverages EF Core's built-in patterns
- Reduces over-engineering

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

### Common Patterns to Follow

### Error Handling
- Use ServiceResult pattern for business operations
- Provide user-friendly error messages
- Log technical errors for debugging
- Handle database exceptions gracefully

### Validation
- Use data annotations on entities and request models
- Implement business validation in services
- Provide clear validation feedback to users

### Transaction Management
- Use EF Core transactions for complex operations
- Ensure data consistency across multiple operations
- Implement proper rollback mechanisms

### Security Considerations
- Validate all user inputs
- Implement proper authorization
- Sanitize data before database operations
- Use parameterized queries (EF Core handles this automatically)

### Performance Optimization
- Use async/await consistently
- Implement proper indexing in database
- Use Select projections for large datasets
- Implement pagination for large result sets
- Use Include() judiciously to avoid N+1 queries

---

This guide provides a modern, efficient foundation for building ERP systems using Blazor Server with Entity Framework Core. By eliminating the unnecessary Repository layer, you get better performance, easier maintenance, and cleaner code while still following SOLID principles and maintaining testability.