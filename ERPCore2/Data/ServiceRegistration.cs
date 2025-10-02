using ERPCore2.Data.Context;
using ERPCore2.Services;
using ERPCore2.Services.Sales;
using ERPCore2.Services.Reports;
using ERPCore2.Helpers;
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
            // Database Configuration - 使用 DbContextFactory 註冊
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

            // Helper 服務
            services.AddScoped<ActionButtonHelper>();

            // 統一地址服務 (推薦使用)
            services.AddScoped<IAddressService, AddressService>();
            
            // 統一聯絡方式服務 (推薦使用)
            services.AddScoped<IContactService, ContactService>();

            // 客戶相關服務
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<ICustomerTypeService, CustomerTypeService>();

            // 共用資料服務
            services.AddScoped<IContactTypeService, ContactTypeService>();
            services.AddScoped<IAddressTypeService, AddressTypeService>();
            services.AddScoped<IPaymentMethodService, PaymentMethodService>();

            // 財務管理服務
            services.AddScoped<IFinancialTransactionService, FinancialTransactionService>();
            services.AddScoped<IAccountsReceivableSetoffService, AccountsReceivableSetoffService>();
            services.AddScoped<IAccountsReceivableSetoffDetailService, AccountsReceivableSetoffDetailService>();
            services.AddScoped<IAccountsPayableSetoffService, AccountsPayableSetoffService>();
            services.AddScoped<IAccountsPayableSetoffDetailService, AccountsPayableSetoffDetailService>();
            services.AddScoped<ISetoffPaymentDetailService, SetoffPaymentDetailService>();
            services.AddScoped<IAccountsPayableSetoffPaymentDetailService, AccountsPayableSetoffPaymentDetailService>();
            services.AddScoped<ICurrencyService, CurrencyService>();
            services.AddScoped<IBankService, BankService>();
            services.AddScoped<IPrepaymentService, PrepaymentService>();

            // 廠商相關服務
            services.AddScoped<ISupplierService, SupplierService>();
            services.AddScoped<ISupplierTypeService, SupplierTypeService>();

            // 商品相關服務
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IProductCategoryService, ProductCategoryService>();
            services.AddScoped<IProductSupplierService, ProductSupplierService>();
            services.AddScoped<ISizeService, SizeService>();
            
            // 商品價格服務
            services.AddScoped<IProductPricingService, ProductPricingService>();
            services.AddScoped<ISupplierPricingService, SupplierPricingService>();
            services.AddScoped<IPriceHistoryService, PriceHistoryService>();

            // 庫存相關服務
            services.AddScoped<IWarehouseService, WarehouseService>();
            services.AddScoped<IWarehouseLocationService, WarehouseLocationService>();
            services.AddScoped<IUnitService, UnitService>();
            services.AddScoped<IUnitConversionService, UnitConversionService>();
            services.AddScoped<IInventoryTransactionTypeService, InventoryTransactionTypeService>();
            
            // 庫存管理服務
            services.AddScoped<IInventoryStockService, InventoryStockService>();
            services.AddScoped<IInventoryTransactionService, InventoryTransactionService>();
            services.AddScoped<IInventoryReservationService, InventoryReservationService>();
            services.AddScoped<IStockTakingService, StockTakingService>();

            // 採購相關服務
            services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
            services.AddScoped<IPurchaseOrderDetailService, PurchaseOrderDetailService>();
            services.AddScoped<IPurchaseReceivingService, PurchaseReceivingService>();
            services.AddScoped<IPurchaseReceivingDetailService, PurchaseReceivingDetailService>();
            services.AddScoped<IPurchaseReturnService, PurchaseReturnService>();
            services.AddScoped<IPurchaseReturnDetailService, PurchaseReturnDetailService>();

            // 銷貨相關服務
            services.AddScoped<ISalesOrderService, SalesOrderService>();
            services.AddScoped<ISalesOrderDetailService, SalesOrderDetailService>();

            services.AddScoped<ISalesReturnService, SalesReturnService>();
            services.AddScoped<ISalesReturnDetailService, SalesReturnDetailService>();
            services.AddScoped<ISalesReturnReasonService, SalesReturnReasonService>();

            // BOM基礎資料表服務
            services.AddScoped<IWeatherService, WeatherService>();
            services.AddScoped<IColorService, ColorService>();
            services.AddScoped<IMaterialService, MaterialService>();

            // 認證和授權服務
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IEmployeePositionService, EmployeePositionService>();
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IPermissionManagementService, PermissionManagementService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();

            // 記憶體快取服務（用於權限快取）
            services.AddMemoryCache();

            // 導航權限收集服務
            services.AddSingleton<INavigationPermissionCollector, NavigationPermissionCollector>();
            
            // 導航搜尋服務
            services.AddScoped<INavigationSearchService, NavigationSearchService>();
            
            // 錯誤記錄服務
            services.AddScoped<IErrorLogService, ErrorLogService>();
            
            // 刪除記錄服務 (已棄用 - 不再使用軟刪除)
            // services.AddScoped<IDeletedRecordService, DeletedRecordService>();
            
            // 公司設定服務
            services.AddScoped<ICompanyService, CompanyService>();
            
            // 系統參數服務
            services.AddScoped<ISystemParameterService, SystemParameterService>();
            
            // 紙張設定服務
            services.AddScoped<IPaperSettingService, PaperSettingService>();
            
            // 印表機設定服務
            services.AddScoped<IPrinterConfigurationService, PrinterConfigurationService>();
            services.AddScoped<IPrinterTestService, PrinterTestService>();
            
            // 報表列印配置服務
            services.AddScoped<IReportPrintConfigurationService, ReportPrintConfigurationService>();
            
            // 報表服務
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IPurchaseOrderReportService, PurchaseOrderReportService>();
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
