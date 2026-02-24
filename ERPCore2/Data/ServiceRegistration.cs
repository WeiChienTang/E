using ERPCore2.Data.Context;
using ERPCore2.Services;
using ERPCore2.Services.Reports;
using ERPCore2.Services.Reports.Configuration;
using ERPCore2.Services.Reports.Interfaces;
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
                    sqlServerOptions => sqlServerOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                .ConfigureWarnings(warnings => 
                    warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.SqlServerEventId.SavepointsDisabledBecauseOfMARS)));
            
            //  加入記憶體快取服務 (權限快取、參考資料快取使用)
            services.AddMemoryCache();
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
            services.AddScoped<IAccountItemService, AccountItemService>();
            services.AddScoped<IJournalEntryService, JournalEntryService>();
            services.AddScoped<ISubAccountService, SubAccountService>();
            services.AddScoped<IJournalEntryAutoGenerationService, JournalEntryAutoGenerationService>();
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
            
            // 推薦服務
            services.AddScoped<ISupplierRecommendationService, SupplierRecommendationService>();
            services.AddScoped<IPriceHistoryService, PriceHistoryService>();

            // 庫存相關服務
            services.AddScoped<IWarehouseService, WarehouseService>();
            services.AddScoped<IWarehouseLocationService, WarehouseLocationService>();
            services.AddScoped<IUnitService, UnitService>();
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
            services.AddScoped<IQuotationCompositionDetailService, QuotationCompositionDetailService>();
            
            services.AddScoped<ISalesOrderService, SalesOrderService>();
            services.AddScoped<ISalesOrderDetailService, SalesOrderDetailService>();
            services.AddScoped<ISalesOrderCompositionDetailService, SalesOrderCompositionDetailService>();

            services.AddScoped<ISalesDeliveryService, SalesDeliveryService>();
            services.AddScoped<ISalesDeliveryDetailService, SalesDeliveryDetailService>();

            services.AddScoped<ISalesReturnService, SalesReturnService>();
            services.AddScoped<ISalesReturnDetailService, SalesReturnDetailService>();
            services.AddScoped<ISalesReturnReasonService, SalesReturnReasonService>();

            // BOM基礎資料表服務
            services.AddScoped<IWeatherService, WeatherService>();
            services.AddScoped<IColorService, ColorService>();
            services.AddScoped<IMaterialService, MaterialService>();
            services.AddScoped<ICompositionCategoryService, CompositionCategoryService>();
            
            // 商品合成（BOM）服務
            services.AddScoped<IProductCompositionService, ProductCompositionService>();
            services.AddScoped<IProductCompositionDetailService, ProductCompositionDetailService>();
            
            // 生產排程服務
            services.AddScoped<IProductionScheduleService, ProductionScheduleService>();
            services.AddScoped<IProductionScheduleItemService, ProductionScheduleItemService>();
            services.AddScoped<IProductionScheduleDetailService, ProductionScheduleDetailService>();
            services.AddScoped<IProductionScheduleCompletionService, ProductionScheduleCompletionService>();
            services.AddScoped<IProductionScheduleAllocationService, ProductionScheduleAllocationService>();

            // 車輛管理服務
            services.AddScoped<IVehicleTypeService, VehicleTypeService>();
            services.AddScoped<IVehicleService, VehicleService>();
            services.AddScoped<IVehicleMaintenanceService, VehicleMaintenanceService>();

            // 廢料管理服務
            services.AddScoped<IWasteTypeService, WasteTypeService>();
            services.AddScoped<IWasteRecordService, WasteRecordService>();

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

            // 儀表板服務
            services.AddScoped<IDashboardService, DashboardService>();

            // 導航權限收集服務
            services.AddSingleton<INavigationPermissionCollector, NavigationPermissionCollector>();
            
            // 導航搜尋服務
            services.AddScoped<INavigationSearchService, NavigationSearchService>();
            
            // 報表搜尋服務
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IReportSearchService, ERPCore2.Services.Reports.ReportSearchService>();
            
            // 錯誤記錄服務
            services.AddScoped<IErrorLogService, ErrorLogService>();
            
            // 刪除記錄服務 (已棄用 - 不再使用軟刪除)
            // services.AddScoped<IDeletedRecordService, DeletedRecordService>();
            
            // 公司設定服務
            services.AddScoped<ICompanyService, CompanyService>();

            // 公司模組管理服務（控制各功能模組的啟用狀態）
            services.AddScoped<ICompanyModuleService, CompanyModuleService>();

            // 系統參數服務
            services.AddScoped<ISystemParameterService, SystemParameterService>();
            
            // 紙張設定服務
            services.AddScoped<IPaperSettingService, PaperSettingService>();
            
            // 印表機設定服務
            services.AddScoped<IPrinterConfigurationService, PrinterConfigurationService>();
            services.AddScoped<IPrinterTestService, PrinterTestService>();
            
            // 報表列印配置服務
            services.AddScoped<ERPCore2.Services.Reports.Configuration.IReportPrintConfigurationService, 
                              ERPCore2.Services.Reports.Configuration.ReportPrintConfigurationService>();
            
            // 文字訊息範本服務
            services.AddScoped<ITextMessageTemplateService, TextMessageTemplateService>();
            
            // 報表服務 - 介面位於 ERPCore2.Services.Reports.Interfaces
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IReportService, ReportService>();
            // 報表列印服務（伺服器端直接列印）- 僅 Windows 平台
            if (OperatingSystem.IsWindowsVersionAtLeast(6, 1))
            {
                services.AddScoped<ERPCore2.Services.Reports.Interfaces.IReportPrintService, ERPCore2.Services.Reports.ReportPrintService>();
                // 純文字列印服務（共用的純文字報表列印功能）
                services.AddScoped<IPlainTextPrintService, PlainTextPrintService>();
                // 格式化列印服務（支援表格線、圖片等格式化列印）
                services.AddScoped<ERPCore2.Services.Reports.Interfaces.IFormattedPrintService, FormattedPrintService>();
                // 商品條碼報表服務（使用 System.Drawing，僅 Windows 平台）
                services.AddScoped<ERPCore2.Services.Reports.Interfaces.IProductBarcodeReportService, ProductBarcodeReportService>();
            }
            // Excel 匯出服務（跨平台）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IExcelExportService, ExcelExportService>();
            // 使用採購單報表服務
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IPurchaseOrderReportService, PurchaseOrderReportService>();
            // 進貨單（入庫單）報表服務
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IPurchaseReceivingReportService, PurchaseReceivingReportService>();
            // 進貨退出單報表服務
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IPurchaseReturnReportService, PurchaseReturnReportService>();
            // 銷貨單報表服務
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.ISalesOrderReportService, SalesOrderReportService>();
            // 出貨單報表服務
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.ISalesDeliveryReportService, SalesDeliveryReportService>();
            // 銷貨退回單報表服務
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.ISalesReturnReportService, SalesReturnReportService>();
            // 報價單報表服務
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IQuotationReportService, QuotationReportService>();
            // 沖款單報表服務（應收沖款單 FN003 / 應付沖款單 FN004）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.ISetoffDocumentReportService, SetoffDocumentReportService>();
            // 商品清單表報表服務（PD001）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IProductListReportService, ProductListReportService>();
            // 商品詳細資料報表服務（PD005）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IProductDetailReportService, ProductDetailReportService>();
            // 物料清單報表服務（PD002）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IBOMReportService, BOMReportService>();
            // 客戶銷售分析報表服務（AR003）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.ICustomerSalesAnalysisReportService, CustomerSalesAnalysisReportService>();
            // 客戶交易明細報表服務（AR004）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.ICustomerTransactionReportService, CustomerTransactionReportService>();
            // 客戶對帳單報表服務（AR002）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.ICustomerStatementReportService, CustomerStatementReportService>();
            // 廠商對帳單報表服務（AP002）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.ISupplierStatementReportService, SupplierStatementReportService>();
            // 生產排程表報表服務（PD004）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IProductionScheduleReportService, ProductionScheduleReportService>();
            // 庫存現況表報表服務（IV003）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IInventoryStatusReportService, InventoryStatusReportService>();
            // 庫存盤點差異表報表服務（IV002）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IStockTakingDifferenceReportService, StockTakingDifferenceReportService>();
            // 車輛管理表報表服務（VH001）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IVehicleListReportService, VehicleListReportService>();
            // 車輛保養表報表服務（VH002）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IVehicleMaintenanceReportService, VehicleMaintenanceReportService>();
            // 員工名冊表報表服務（HR001）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IEmployeeRosterReportService, EmployeeRosterReportService>();
            // 員工詳細資料報表服務（HR002）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IEmployeeDetailReportService, EmployeeDetailReportService>();
            // 客戶名冊表報表服務（AR005）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.ICustomerRosterReportService, CustomerRosterReportService>();
            // 客戶詳細資料報表服務（AR006）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.ICustomerDetailReportService, CustomerDetailReportService>();
            // 廠商名冊表報表服務（AP004）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.ISupplierRosterReportService, SupplierRosterReportService>();
            // 廠商詳細資料報表服務（AP005）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.ISupplierDetailReportService, SupplierDetailReportService>();
            // 廢料記錄報表服務（WL001）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IWasteRecordReportService, WasteRecordReportService>();
            // 會計科目表報表服務（FN005）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IAccountItemListReportService, AccountItemListReportService>();
            // 財務報表服務（FN006~FN008）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.ITrialBalanceReportService, TrialBalanceReportService>();
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IIncomeStatementReportService, IncomeStatementReportService>();
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IBalanceSheetReportService, BalanceSheetReportService>();
            // 條碼生成服務
            services.AddSingleton<ERPCore2.Services.Reports.Interfaces.IBarcodeGeneratorService, BarcodeGeneratorService>();
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
