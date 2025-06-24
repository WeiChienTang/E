using ERPCore2.Data.Context;
using ERPCore2.Services;
using ERPCore2.Services.Notifications;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data
{
    /// <summary>
    /// 服務註冊配置類別
    /// 統一管理依賴注入服務註冊，保持 Program.cs 整潔
    /// </summary>
    public static class ServiceRegistration
    {
        /// <summary>
        /// 註冊資料庫相關服務
        /// </summary>
        /// <param name="services">服務集合</param>
        /// <param name="connectionString">資料庫連接字串</param>
        public static void AddDatabaseServices(this IServiceCollection services, string connectionString)
        {
            // Database Configuration - 使用 DbContextFactory 解決並發問題
            services.AddDbContextFactory<AppDbContext>(options =>
                options.UseSqlServer(connectionString));
        }

        /// <summary>
        /// 註冊業務邏輯服務
        /// </summary>
        /// <param name="services">服務集合</param>
        public static void AddServices(this IServiceCollection services)
        {
            // 通知服務
            services.AddScoped<INotificationService, NotificationService>();

            // 客戶相關服務
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<ICustomerContactService, CustomerContactService>();
            services.AddScoped<ICustomerAddressService, CustomerAddressService>();
            services.AddScoped<ICustomerTypeService, CustomerTypeService>();

            // 共用資料服務
            services.AddScoped<IContactTypeService, ContactTypeService>();
            services.AddScoped<IAddressTypeService, AddressTypeService>();

            // 行業類型服務
            services.AddScoped<IIndustryTypeService, IndustryTypeService>();

            // 廠商相關服務
            services.AddScoped<ISupplierService, SupplierService>();
            services.AddScoped<ISupplierContactService, SupplierContactService>();
            services.AddScoped<ISupplierAddressService, SupplierAddressService>();
            services.AddScoped<ISupplierTypeService, SupplierTypeService>();

            // 商品相關服務
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IProductCategoryService, ProductCategoryService>();
            services.AddScoped<IProductSupplierService, ProductSupplierService>();

            // 庫存相關服務
            services.AddScoped<IWarehouseService, WarehouseService>();
            services.AddScoped<IWarehouseLocationService, WarehouseLocationService>();
            services.AddScoped<IUnitService, UnitService>();
            services.AddScoped<IUnitConversionService, UnitConversionService>();
            services.AddScoped<IInventoryTransactionTypeService, InventoryTransactionTypeService>();

            // BOM基礎資料表服務
            services.AddScoped<IWeatherService, WeatherService>();
            services.AddScoped<IColorService, ColorService>();
            services.AddScoped<IMaterialService, MaterialService>();

            // 認證和授權服務
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IEmployeeContactService, EmployeeContactService>();
            services.AddScoped<IEmployeeAddressService, EmployeeAddressService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IPermissionManagementService, PermissionManagementService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();

            // 記憶體快取服務（用於權限快取）
            services.AddMemoryCache();

            // 導航搜尋服務
            services.AddScoped<INavigationSearchService, NavigationSearchService>();
        }

        /// <summary>
        /// 註冊所有應用程式服務
        /// </summary>
        /// <param name="services">服務集合</param>
        /// <param name="connectionString">資料庫連接字串</param>
        public static void AddApplicationServices(this IServiceCollection services, string connectionString)
        {
            // 註冊資料庫服務
            services.AddDatabaseServices(connectionString);

            // 註冊業務邏輯服務
            services.AddServices();
        }
    }
}
