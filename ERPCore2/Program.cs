using ERPCore2.Components;
using ERPCore2.Data;
using ERPCore2.Test;
using Microsoft.EntityFrameworkCore;

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
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        
        // 確保 Cookie 在不同環境下都能正確共享
        options.Cookie.Name = "ERPCore2.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
        options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
        
        // 在開發環境中允許跨子域共享 Cookie
        if (builder.Environment.IsDevelopment())
        {
            options.Cookie.Domain = null; // 允許在 localhost 的不同端口間共享
        }
    });

builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 加入 Blazor Server 的認證狀態提供者
builder.Services.AddCascadingAuthenticationState();

// 加入控制器服務
builder.Services.AddControllers();

// 註冊 HttpClient 用於 API 調用
builder.Services.AddHttpClient();

// 為 Blazor 組件配置具有 BaseAddress 的 HttpClient
builder.Services.AddScoped(sp =>
{
    var httpContext = sp.GetService<IHttpContextAccessor>()?.HttpContext;
    var request = httpContext?.Request;
    
    var httpClientHandler = new HttpClientHandler()
    {
        UseCookies = true
    };
    
    var httpClient = new HttpClient(httpClientHandler);
    
    if (request != null)
    {
        var baseUri = $"{request.Scheme}://{request.Host}";
        httpClient.BaseAddress = new Uri(baseUri);
        
        // 複製當前請求的 Cookie 到 HttpClient
        if (request.Headers.ContainsKey("Cookie"))
        {
            var cookies = request.Headers["Cookie"].ToString();
            httpClient.DefaultRequestHeaders.Add("Cookie", cookies);
        }
    }
    else
    {
        // 🔧 生產環境的預設值 - 使用動態端口檢測
        var urls = builder.Configuration["urls"] ?? "http://localhost:5000";
        var firstUrl = urls.Split(';')[0];
        httpClient.BaseAddress = new Uri(firstUrl);
    }
    
    return httpClient;
});

// 配置反偽令牌為寬鬆模式
builder.Services.AddAntiforgery(options =>
{
    // 允許沒有令牌的請求通過（適用於互動式表單）
    options.Cookie.Name = "__RequestVerificationToken";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// 加入認證和授權中介軟體
app.UseAuthentication();
app.UseAuthorization();

// 添加反偽令牌中介軟體但配置為寬鬆模式
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// 對應控制器
app.MapControllers();

// Initialize seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // 測試資料庫連接和查詢
        if (app.Environment.IsDevelopment())
        {
            await DatabaseTestHelper.TestDatabaseConnection();
        }
        
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
