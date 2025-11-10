using ERPCore2.Data.Context;
using ERPCore2.Services;
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
                options.UseSqlServer(connectionString,
                    sqlServerOptions => sqlServerOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));
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
            services.AddScoped<RelatedDocumentsHelper>();

            // 客戶相關服務
            services.AddScoped<ICustomerService, CustomerService>();

            services.AddScoped<IPaymentMethodService, PaymentMethodService>();

            // 財務管理服務
            services.AddScoped<ICurrencyService, CurrencyService>();
            services.AddScoped<IBankService, BankService>();
            services.AddScoped<ISetoffDocumentService, SetoffDocumentService>();
            services.AddScoped<ISetoffProductDetailService, SetoffProductDetailService>();
            services.AddScoped<ISetoffPaymentService, SetoffPaymentService>();
            services.AddScoped<ISetoffPrepaymentService, SetoffPrepaymentService>();
            services.AddScoped<ISetoffPrepaymentUsageService, SetoffPrepaymentUsageService>();
            services.AddScoped<IPrepaymentTypeService, PrepaymentTypeService>();

            // 廠商相關服務
            services.AddScoped<ISupplierService, SupplierService>();

            // 商品相關服務
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IProductCategoryService, ProductCategoryService>();
            services.AddScoped<IProductSupplierService, ProductSupplierService>();
            services.AddScoped<ISizeService, SizeService>();
            
            // 商品價格服務
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
            services.AddScoped<IInventoryStockDetailService, InventoryStockDetailService>();
            services.AddScoped<IInventoryTransactionService, InventoryTransactionService>();
            services.AddScoped<IInventoryReservationService, InventoryReservationService>();
            services.AddScoped<IStockTakingService, StockTakingService>();
            services.AddScoped<IMaterialIssueService, MaterialIssueService>();
            services.AddScoped<IMaterialIssueDetailService, MaterialIssueDetailService>();

            // 採購相關服務
            services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
            services.AddScoped<IPurchaseOrderDetailService, PurchaseOrderDetailService>();
            services.AddScoped<IPurchaseReceivingService, PurchaseReceivingService>();
            services.AddScoped<IPurchaseReceivingDetailService, PurchaseReceivingDetailService>();
            services.AddScoped<IPurchaseReturnService, PurchaseReturnService>();
            services.AddScoped<IPurchaseReturnDetailService, PurchaseReturnDetailService>();

            // 銷貨相關服務
            services.AddScoped<IQuotationService, QuotationService>();
            services.AddScoped<IQuotationDetailService, QuotationDetailService>();
            
            services.AddScoped<ISalesOrderService, SalesOrderService>();
            services.AddScoped<ISalesOrderDetailService, SalesOrderDetailService>();

            services.AddScoped<ISalesDeliveryService, SalesDeliveryService>();
            services.AddScoped<ISalesDeliveryDetailService, SalesDeliveryDetailService>();

            services.AddScoped<ISalesReturnService, SalesReturnService>();
            services.AddScoped<ISalesReturnDetailService, SalesReturnDetailService>();
            services.AddScoped<ISalesReturnReasonService, SalesReturnReasonService>();

            // BOM基礎資料表服務
            services.AddScoped<IWeatherService, WeatherService>();
            services.AddScoped<IColorService, ColorService>();
            services.AddScoped<IMaterialService, MaterialService>();
            
            // 產品合成（BOM）服務
            services.AddScoped<IProductCompositionService, ProductCompositionService>();
            services.AddScoped<IProductCompositionDetailService, ProductCompositionDetailService>();
            
            // 生產排程服務
            services.AddScoped<IProductionScheduleService, ProductionScheduleService>();
            services.AddScoped<IProductionScheduleDetailService, ProductionScheduleDetailService>();

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
            // 使用採購單報表服務
            services.AddScoped<IPurchaseOrderReportService, PurchaseOrderReportService>();
            // 進貨單（入庫單）報表服務
            services.AddScoped<IPurchaseReceivingReportService, PurchaseReceivingReportService>();
            // 進貨退出單報表服務
            services.AddScoped<IPurchaseReturnReportService, PurchaseReturnReportService>();
            // 銷貨單報表服務
            services.AddScoped<ISalesOrderReportService, SalesOrderReportService>();
            // 銷貨退回單報表服務
            services.AddScoped<ISalesReturnReportService, SalesReturnReportService>();
            // 報價單報表服務
            services.AddScoped<IQuotationReportService, QuotationReportService>();
            // 產品條碼報表服務
            services.AddScoped<IProductBarcodeReportService, ProductBarcodeReportService>();
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
