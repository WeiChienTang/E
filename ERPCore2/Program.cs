using ERPCore2.Components;
using ERPCore2.Data;
using ERPCore2.Data.Context;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using ERPCore2.Services;
using Microsoft.AspNetCore.Authorization;

// 檢查命令列參數
var commandLineArgs = Environment.GetCommandLineArgs();
bool isMigrationMode = commandLineArgs.Contains("--migrate") || Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Migration";
bool isSeedDataMode = commandLineArgs.Contains("--seed-data");
bool isSetupMode = commandLineArgs.Contains("--setup"); // 完整設定模式

if (isMigrationMode || isSeedDataMode || isSetupMode)
{
    var migrationBuilder = WebApplication.CreateBuilder(args);
    
    // 設定資料庫連接
    var connectionString = migrationBuilder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connectionString))
    {
        Environment.Exit(1);
    }
    
    // 註冊必要的服務進行遷移
    migrationBuilder.Services.AddDbContextFactory<AppDbContext>(options => 
        options.UseSqlServer(connectionString,
            sqlServerOptions => sqlServerOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
        .ConfigureWarnings(warnings => 
            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.SqlServerEventId.SavepointsDisabledBecauseOfMARS)));
    
    // 建立應用程式但不啟動 Web 服務器
    var migrationApp = migrationBuilder.Build();
    
    try
    {
        using var scope = migrationApp.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        await context.Database.MigrateAsync();
        
        // 初始化種子資料
        await SeedData.InitializeAsync(scope.ServiceProvider);
        Environment.Exit(0);
    }
    catch (Exception)
    {
        Environment.Exit(1);
    }
}

var builder = WebApplication.CreateBuilder(args);

// 條件式 HTTPS 設定：只有當憑證檔案存在時才啟用，不存在時自動降級為 HTTP
var httpsConfig = builder.Configuration.GetSection("HttpsConfig");
if (httpsConfig.Exists() && httpsConfig.GetValue<bool>("Enabled"))
{
    var certPath = httpsConfig["CertificatePath"] ?? string.Empty;
    var certPassword = httpsConfig["CertificatePassword"] ?? string.Empty;
    var httpPort = httpsConfig.GetValue<int>("HttpPort", 6011);
    var httpsPort = httpsConfig.GetValue<int>("HttpsPort", 6012);

    // 優先從 .pwd 檔案讀取最新密碼
    // 原因：dotnet publish 會覆蓋 appsettings.Production.json（還原舊密碼），
    //       但 .pwd 檔案與 cert.pfx 同目錄且不在 publish 輸出中，因此不受影響。
    //       如此一來使用者不需要在每次程式更新後重新安裝憑證。
    if (!string.IsNullOrEmpty(certPath))
    {
        var pwdFilePath = Path.ChangeExtension(certPath, ".pwd");
        if (File.Exists(pwdFilePath))
        {
            try { certPassword = File.ReadAllText(pwdFilePath).Trim(); }
            catch { /* 讀取失敗時繼續使用 appsettings 的密碼 */ }
        }
    }

    if (!string.IsNullOrEmpty(certPath) && File.Exists(certPath))
    {
        // 預先驗證憑證密碼，避免密碼不符時直接崩潰
        bool certValid = false;
        try
        {
            using var testCert = System.Security.Cryptography.X509Certificates.X509CertificateLoader.LoadPkcs12FromFile(
                certPath, certPassword);
            certValid = true;
        }
        catch (Exception certEx)
        {
            Console.Error.WriteLine($"[WARNING] 憑證密碼不符或憑證損毀，已自動降級為 HTTP 模式。錯誤：{certEx.Message}");
            Console.Error.WriteLine("[WARNING] 請至系統設定 → 憑證管理 → 重新產生憑證，再重新啟動伺服器。");
        }

        if (certValid)
        {
            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.ListenAnyIP(httpPort);
                serverOptions.ListenAnyIP(httpsPort, listenOptions =>
                {
                    listenOptions.UseHttps(certPath, certPassword);
                });
            });
            builder.Configuration["urls"] = $"https://*:{httpsPort};http://*:{httpPort}";
        }
    }
    // 憑證不存在或密碼不符時靜默降級，維持 HTTP 模式
}

// 註冊應用程式服務
builder.Services.AddApplicationServices(builder.Configuration.GetConnectionString("DefaultConnection")!);

// 加入 HttpContextAccessor（.NET 9 互動式元件必需）
builder.Services.AddHttpContextAccessor();

// 加入認證和授權服務
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/auth/login";
        options.LogoutPath = "/auth/logout";
        options.AccessDeniedPath = "/access-denied";
        
        // 修復記住我功能：讓個別登入決定過期時間
        options.ExpireTimeSpan = TimeSpan.FromHours(8); // 預設 8 小時，但會被個別設定覆蓋
        options.SlidingExpiration = true; // 啟用滑動過期，延長活躍用戶的 session
        
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
            // SameAsRequest：HTTP 連線時不加 Secure，HTTPS 連線時自動加 Secure
            // 避免在尚未設定憑證的 HTTP 環境下，Cookie 因 Secure 旗標無法傳送導致登入失敗
            options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
        }
        
        // 加入事件處理來調試 Cookie 問題
        options.Events = new CookieAuthenticationEvents
        {
            OnValidatePrincipal = context =>
            {
                return Task.CompletedTask;
            },
            OnRedirectToLogin = context =>
            {
                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            }
        };
    });

// 加入授權服務
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Permission", policy =>
    {
        policy.Requirements.Add(new PermissionRequirement(""));
    });

// 註冊授權處理器
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

// 註冊導航權限服務
builder.Services.AddScoped<INavigationPermissionService, NavigationPermissionService>();

// Add services to the container.
builder.Services.AddRazorComponents(options => 
    {
        // 在開發模式啟用詳細錯誤
        options.DetailedErrors = builder.Environment.IsDevelopment();
    })
    .AddInteractiveServerComponents(options =>
    {
        // 配置 Circuit 選項來改善 JavaScript interop 錯誤處理
        options.DetailedErrors = builder.Environment.IsDevelopment();
        options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(1);
        options.MaxBufferedUnacknowledgedRenderBatches = 10;
    });

// 加入控制器支援（僅用於認證）
builder.Services.AddControllers();

// 加入 Blazor Server 的認證狀態提供者
builder.Services.AddCascadingAuthenticationState();

// 註冊自定義認證狀態提供者
builder.Services.AddScoped<CustomRevalidatingServerAuthenticationStateProvider>();
builder.Services.AddScoped<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>(provider => 
    provider.GetRequiredService<CustomRevalidatingServerAuthenticationStateProvider>());

var app = builder.Build();

// 初始化 ErrorHandlingHelper
ErrorHandlingHelper.Initialize(app.Services);

// 註冊全域例外處理中間件 (必須在其他中間件之前)
app.UseMiddleware<GlobalExceptionHelper>();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// 只在開發環境或有 HTTPS 設定時才使用 HTTPS 重新導向
// 排除 /api/certificate 路徑：用戶在安裝憑證前無法建立 HTTPS 信任關係，
// 若對此路徑做重新導向，會造成「需要憑證才能下載憑證」的死結。
if (app.Environment.IsDevelopment() || builder.Configuration["urls"]?.Contains("https") == true)
{
    app.UseWhen(
        context => !context.Request.Path.StartsWithSegments("/api/certificate"),
        appBuilder => appBuilder.UseHttpsRedirection()
    );
}

app.UseAuthentication();
app.UseAuthorization();

// 加入權限檢查中間件
app.UseMiddleware<PermissionCheckMiddleware>();

// 設定靜態檔案存取 - 提供 Resources 資料夾的圖片
var resourcesPath = Path.Combine(builder.Environment.ContentRootPath, "Resources");
if (!Directory.Exists(resourcesPath))
{
    Directory.CreateDirectory(resourcesPath);
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(resourcesPath),
    RequestPath = "/Resources"
});

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
    catch
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
    }
}

// 自動執行資料庫遷移（生產環境）
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        var context = services.GetRequiredService<AppDbContext>();
        
        // 檢查是否有待處理的遷移
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            logger.LogInformation("偵測到待處理的資料庫遷移，開始執行自動遷移...");
            await context.Database.MigrateAsync();
            logger.LogInformation("資料庫遷移完成");
        }
        else
        {
            logger.LogInformation("資料庫架構已是最新版本");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "自動資料庫遷移失敗。請檢查資料庫連接或手動執行遷移。");
        // 不中斷應用程式啟動，讓使用者看到錯誤訊息
    }
}

app.Run();
