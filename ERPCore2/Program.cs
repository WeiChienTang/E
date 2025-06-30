using ERPCore2.Components;
using ERPCore2.Data;
using ERPCore2.Data.Context;
using Microsoft.EntityFrameworkCore;

// 檢查命令列參數
var commandLineArgs = Environment.GetCommandLineArgs();
bool isMigrationMode = commandLineArgs.Contains("--migrate") || Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Migration";
bool isSeedDataMode = commandLineArgs.Contains("--seed-data");
bool isSetupMode = commandLineArgs.Contains("--setup"); // 完整設定模式

if (isMigrationMode || isSeedDataMode || isSetupMode)
{
    Console.WriteLine("Running Entity Framework database migrations...");
    
    var migrationBuilder = WebApplication.CreateBuilder(args);
    
    // 設定資料庫連接
    var connectionString = migrationBuilder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connectionString))
    {
        Console.WriteLine("Error: No database connection string found.");
        Environment.Exit(1);
    }
    
    // 註冊必要的服務進行遷移
    migrationBuilder.Services.AddDbContext<AppDbContext>(options => 
        options.UseSqlServer(connectionString));
    
    // 建立應用程式但不啟動 Web 服務器
    var migrationApp = migrationBuilder.Build();
    
    try
    {
        using var scope = migrationApp.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        Console.WriteLine("Applying database migrations...");
        await context.Database.MigrateAsync();
        
        Console.WriteLine("Database migrations completed successfully.");
        Console.WriteLine("Initializing seed data...");
        
        // 初始化種子資料
        await SeedData.InitializeAsync(scope.ServiceProvider);
        
        Console.WriteLine("Seed data initialization completed.");
        Environment.Exit(0);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error during migration: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        Environment.Exit(1);
    }
}

var builder = WebApplication.CreateBuilder(args);

// 註冊應用程式服務
builder.Services.AddApplicationServices(builder.Configuration.GetConnectionString("DefaultConnection")!);

// 加入 HttpContextAccessor（.NET 9 互動式元件必需）
builder.Services.AddHttpContextAccessor();

// 加入認證和授權服務
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/auth/login";
        options.LogoutPath = "/auth/logout";
        options.AccessDeniedPath = "/access-denied";
        
        // 修復記住我功能：移除全域過期時間限制，讓個別登入決定
        // options.ExpireTimeSpan = TimeSpan.FromHours(8); // 註解掉避免覆蓋個別設定
        options.SlidingExpiration = false; // 關閉滑動過期，讓記住我功能正常工作
        
        // 確保 Cookie 在不同環境下都能正確共享
        options.Cookie.Name = "ERPCore2.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
        
        // 修復記住我功能：針對 HTTP 環境調整安全政策
        if (builder.Environment.IsDevelopment())
        {
            options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.None; // HTTP 環境允許
            options.Cookie.Domain = null; // 允許在 localhost 的不同端口間共享
        }
        else
        {
            options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
        }
    });

builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 加入控制器支援（僅用於認證）
builder.Services.AddControllers();

// 加入 Blazor Server 的認證狀態提供者
builder.Services.AddCascadingAuthenticationState();

// 註冊自定義認證狀態提供者
builder.Services.AddScoped<ERPCore2.Services.Auth.CustomRevalidatingServerAuthenticationStateProvider>();
builder.Services.AddScoped<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>(provider => 
    provider.GetRequiredService<ERPCore2.Services.Auth.CustomRevalidatingServerAuthenticationStateProvider>());




var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// 只在開發環境或有 HTTPS 設定時才使用 HTTPS 重新導向
if (app.Environment.IsDevelopment() || builder.Configuration["urls"]?.Contains("https") == true)
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

// 加入反偽造令牌中間件
app.UseAntiforgery();

app.MapStaticAssets();

// 對應控制器
app.MapControllers();

// 對應 Blazor 組件
app.MapRazorComponents<ERPCore2.Components.App>()
    .AddInteractiveServerRenderMode();

// Initialize seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // 初始化種子資料（包含認證系統資料）
        await SeedData.InitializeAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
}

app.Run();
