using ERPCore2.Components;
using ERPCore2.Data;
using ERPCore2.Data.Context;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Services;
using ERPCore2.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Database Configuration - 使用 DbContextFactory 解決並發問題
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 保留舊的 DbContext 註冊以支援現有的依賴注入
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Service Registration (No Repository needed!)
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IContactTypeService, ContactTypeService>();
builder.Services.AddScoped<ICustomerContactService, CustomerContactService>();
builder.Services.AddScoped<ICustomerAddressService, CustomerAddressService>();
builder.Services.AddScoped<ICustomerTypeService, CustomerTypeService>();
builder.Services.AddScoped<IAddressTypeService, AddressTypeService>();
builder.Services.AddScoped<IIndustryTypeService, IndustryTypeService>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Initialize seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // 暫時註釋掉種子資料初始化，專注於測試並發問題修復
        // await ERPCore2.Data.SeedData.InitializeAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
}

app.Run();
