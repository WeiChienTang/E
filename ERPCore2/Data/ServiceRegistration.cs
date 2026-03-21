using ERPCore2.Data.Context;
using ERPCore2.Services;
using ERPCore2.Services.Customers;
using ERPCore2.Services.Import;
using ERPCore2.Services.Export;
using ERPCore2.Services.Reports;
using ERPCore2.Services.Reports.Configuration;
using ERPCore2.Services.Reports.Interfaces;
using ERPCore2.Services.Suppliers;
using ERPCore2.Services.Systems;
using ERPCore2.Helpers;
using ERPCore2.Helpers.Common;
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
                    sqlServerOptions => sqlServerOptions
                        .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                        .CommandTimeout(10))  // 10 秒命令逾時，避免 DB 不穩定時卡住 30 秒（預設值）
                        // EnableRetryOnFailure 已移除：即使 maxRetryCount=0/1，SqlServerRetryingExecutionStrategy
                        // 仍會禁止全系統 50+ 個 BeginTransactionAsync() 呼叫，導致所有交易失敗。
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

            // 資料庫匯入工具（SuperAdmin）
            services.AddScoped<IDatabaseImportService, DatabaseImportService>();

            // 資料庫匯出工具（SuperAdmin）
            services.AddScoped<IDatabaseExportService, DatabaseExportService>();

            // Helper 服務
            services.AddScoped<ActionButtonHelper>();
            services.AddScoped<ContactLinkHelper>();
            services.AddScoped<RelatedDocumentsHelper>();

            // 客戶相關服務
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<ICustomerVisitService, CustomerVisitService>();
            services.AddScoped<ICustomerComplaintService, CustomerComplaintService>();
            services.AddScoped<ICustomerBankAccountService, CustomerBankAccountService>();

            // CRM 潛在客戶服務
            services.AddScoped<ERPCore2.Services.Crm.ICrmLeadService, ERPCore2.Services.Crm.CrmLeadService>();
            services.AddScoped<ERPCore2.Services.Crm.ICrmLeadFollowUpService, ERPCore2.Services.Crm.CrmLeadFollowUpService>();

            // 員工工具配給服務
            services.AddScoped<IEmployeeToolService, EmployeeToolService>();

            // 員工教育訓練與證照服務
            services.AddScoped<IEmployeeLicenseService, EmployeeLicenseService>();
            services.AddScoped<IEmployeeTrainingRecordService, EmployeeTrainingRecordService>();
            services.AddScoped<ERPCore2.Services.Customers.ICustomerChartService, ERPCore2.Services.Customers.CustomerChartService>();
            services.AddScoped<ERPCore2.Services.Suppliers.ISupplierChartService, ERPCore2.Services.Suppliers.SupplierChartService>();
            services.AddScoped<ERPCore2.Services.Employees.IEmployeeChartService, ERPCore2.Services.Employees.EmployeeChartService>();
            services.AddScoped<ERPCore2.Services.Purchase.IPurchaseChartService, ERPCore2.Services.Purchase.PurchaseChartService>();
            services.AddScoped<ERPCore2.Services.Sales.ISalesChartService, ERPCore2.Services.Sales.SalesChartService>();
            services.AddScoped<ERPCore2.Services.Inventory.IInventoryChartService, ERPCore2.Services.Inventory.InventoryChartService>();
            services.AddScoped<ERPCore2.Services.Items.IItemChartService, ERPCore2.Services.Items.ItemChartService>();
            services.AddScoped<ERPCore2.Services.ProductionManagement.IProductionChartService, ERPCore2.Services.ProductionManagement.ProductionChartService>();
            services.AddScoped<ERPCore2.Services.FinancialManagement.IFinancialChartService, ERPCore2.Services.FinancialManagement.FinancialChartService>();
            services.AddScoped<ERPCore2.Services.Payroll.IPayrollChartService, ERPCore2.Services.Payroll.PayrollChartService>();
            services.AddScoped<ERPCore2.Services.ScaleManagement.IScaleChartService, ERPCore2.Services.ScaleManagement.ScaleChartService>();
            services.AddScoped<ERPCore2.Services.Vehicles.IVehicleChartService, ERPCore2.Services.Vehicles.VehicleChartService>();

            services.AddScoped<IPaymentMethodService, PaymentMethodService>();

            // 財務管理服務
            services.AddScoped<IFiscalPeriodService, FiscalPeriodService>();
            services.AddScoped<ICurrencyService, CurrencyService>();
            services.AddScoped<IBankService, BankService>();
            services.AddScoped<IBankStatementService, BankStatementService>();
            services.AddScoped<IAccountItemService, AccountItemService>();
            services.AddScoped<IJournalEntryService, JournalEntryService>();
            services.AddScoped<ISubAccountService, SubAccountService>();
            services.AddScoped<IJournalEntryAutoGenerationService, JournalEntryAutoGenerationService>();
            services.AddScoped<IFiscalYearClosingService, FiscalYearClosingService>();
            services.AddScoped<IJournalEntryAttachmentService, JournalEntryAttachmentService>();
            services.AddScoped<IBankStatementImportService, BankStatementImportService>();
            services.AddScoped<ISetoffDocumentService, SetoffDocumentService>();
            services.AddScoped<ISetoffItemDetailService, SetoffItemDetailService>();
            services.AddScoped<ISetoffPaymentService, SetoffPaymentService>();
            services.AddScoped<ISetoffPrepaymentService, SetoffPrepaymentService>();
            services.AddScoped<ISetoffPrepaymentUsageService, SetoffPrepaymentUsageService>();
            services.AddScoped<IPrepaymentTypeService, PrepaymentTypeService>();

            // 名片服務
            services.AddScoped<IBusinessCardService, BusinessCardService>();

            // 廠商相關服務
            services.AddScoped<ISupplierService, SupplierService>();
            services.AddScoped<ISupplierVisitService, SupplierVisitService>();
            services.AddScoped<ISupplierBankAccountService, SupplierBankAccountService>();

            // 品項相關服務
            services.AddScoped<IItemService, ItemService>();
            services.AddScoped<IItemCategoryService, ItemCategoryService>();
            services.AddScoped<IItemSupplierService, ItemSupplierService>();
            services.AddScoped<IItemCustomerService, ItemCustomerService>();
            services.AddScoped<IItemPhotoService, ItemPhotoService>();
            services.AddScoped<ISizeService, SizeService>();
            
            // 品項價格服務
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
            services.AddScoped<IPurchaseReturnReasonService, PurchaseReturnReasonService>();

            // 銷貨相關服務
            services.AddScoped<IQuotationService, QuotationService>();
            services.AddScoped<IQuotationDetailService, QuotationDetailService>();
            services.AddScoped<IQuotationCompositionDetailService, QuotationCompositionDetailService>();
            services.AddScoped<IQuotationPhotoService, QuotationPhotoService>();

            services.AddScoped<ISalesOrderService, SalesOrderService>();
            services.AddScoped<ISalesOrderDetailService, SalesOrderDetailService>();
            services.AddScoped<ISalesOrderCompositionDetailService, SalesOrderCompositionDetailService>();
            services.AddScoped<ISalesOrderPhotoService, SalesOrderPhotoService>();

            services.AddScoped<ISalesDeliveryService, SalesDeliveryService>();
            services.AddScoped<ISalesDeliveryDetailService, SalesDeliveryDetailService>();

            services.AddScoped<ISalesReturnService, SalesReturnService>();
            services.AddScoped<ISalesReturnDetailService, SalesReturnDetailService>();
            services.AddScoped<ISalesReturnReasonService, SalesReturnReasonService>();
            services.AddScoped<ISalesTargetService, SalesTargetService>();

            // BOM基礎資料表服務
            services.AddScoped<IWeatherService, WeatherService>();
            services.AddScoped<IColorService, ColorService>();
            services.AddScoped<IMaterialService, MaterialService>();
            services.AddScoped<ICompositionCategoryService, CompositionCategoryService>();
            
            // 品項合成（BOM）服務
            services.AddScoped<IItemCompositionService, ItemCompositionService>();
            services.AddScoped<IItemCompositionDetailService, ItemCompositionDetailService>();
            
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

            // 設備管理服務
            services.AddScoped<IEquipmentCategoryService, EquipmentCategoryService>();
            services.AddScoped<IEquipmentService, EquipmentService>();
            services.AddScoped<IEquipmentMaintenanceService, EquipmentMaintenanceService>();

            // 磅秤管理服務
            services.AddScoped<IScaleRecordService, ScaleRecordService>();

            // RS-232C 串列埠服務（Singleton：實體串列埠連線需跨 Circuit 持久化）
            services.AddSingleton<ISerialPortService, SerialPortService>();

            // 認證和授權服務
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IEmployeePreferenceService, EmployeePreferenceService>();
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
            services.AddScoped<ICompanyBankAccountService, CompanyBankAccountService>();

            // 公司模組管理服務（控制各功能模組的啟用狀態）
            services.AddScoped<ICompanyModuleService, CompanyModuleService>();

            // 憑證管理服務（自動產生自簽 SSL 憑證）
            services.AddScoped<ICertificateService, CertificateService>();

            // 系統參數服務
            services.AddScoped<ISystemParameterService, SystemParameterService>();
            
            // 紙張設定服務
            services.AddScoped<IPaperSettingService, PaperSettingService>();
            
            // 報表列印配置服務
            services.AddScoped<ERPCore2.Services.Reports.Configuration.IReportPrintConfigurationService, 
                              ERPCore2.Services.Reports.Configuration.ReportPrintConfigurationService>();
            
            // 文字訊息範本服務
            services.AddScoped<ITextMessageTemplateService, TextMessageTemplateService>();
            
            // 報表服務 - 介面位於 ERPCore2.Services.Reports.Interfaces
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IReportService, ReportService>();
            // 格式化列印服務（支援表格線、圖片等格式化列印）- 僅 Windows 平台
            if (OperatingSystem.IsWindowsVersionAtLeast(6, 1))
            {
                services.AddScoped<ERPCore2.Services.Reports.Interfaces.IFormattedPrintService, FormattedPrintService>();
                // 品項條碼報表服務（使用 System.Drawing，僅 Windows 平台）
                services.AddScoped<ERPCore2.Services.Reports.Interfaces.IItemBarcodeReportService, ItemBarcodeReportService>();
            }
            // Excel 匯出服務（跨平台）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IExcelExportService, ExcelExportService>();
            // PDF 匯出服務（跨平台，使用 PuppeteerSharp）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IPdfExportService, PdfExportService>();
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
            // 品項清單表報表服務（PD001）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IItemListReportService, ItemListReportService>();
            // 品項詳細資料報表服務（PD005）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IItemDetailReportService, ItemDetailReportService>();
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
            // 製令單報表服務（MO001）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IManufacturingOrderReportService, ManufacturingOrderReportService>();
            // 生產排程表報表服務（PD004）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IProductionScheduleReportService, ProductionScheduleReportService>();
            // 用料損耗退料記錄報表服務（PD006）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IMaterialScrapReportService, MaterialScrapReportService>();
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IMaterialRequirementsReportService, MaterialRequirementsReportService>();
            // 庫存現況表報表服務（IV003）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IInventoryStatusReportService, InventoryStatusReportService>();
            // 庫存盤點差異表報表服務（IV002）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IStockTakingDifferenceReportService, StockTakingDifferenceReportService>();
            // 車輛管理表報表服務（VH001）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IVehicleListReportService, VehicleListReportService>();
            // 車輛保養表報表服務（VH002）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IVehicleMaintenanceReportService, VehicleMaintenanceReportService>();
            // 設備清單報表服務（EQ001）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IEquipmentListReportService, EquipmentListReportService>();
            // 設備保養維修記錄報表服務（EQ002）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IEquipmentMaintenanceReportService, EquipmentMaintenanceReportService>();
            // 員工名冊表報表服務（HR001）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IEmployeeRosterReportService, EmployeeRosterReportService>();
            // 員工詳細資料報表服務（HR002）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IEmployeeDetailReportService, EmployeeDetailReportService>();
            // 客戶名冊表報表服務（AR005）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.ICustomerRosterReportService, CustomerRosterReportService>();
            // 客戶詳細資料報表服務（AR006）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.ICustomerDetailReportService, CustomerDetailReportService>();
            // 客訴報告服務（AR008）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.ICustomerComplaintReportService, CustomerComplaintReportService>();
            // 廠商名冊表報表服務（AP004）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.ISupplierRosterReportService, SupplierRosterReportService>();
            // 廠商詳細資料報表服務（AP005）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.ISupplierDetailReportService, SupplierDetailReportService>();
            // 磅秤紀錄報表服務（WL001）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IScaleRecordReportService, ScaleRecordReportService>();
            // 會計科目表報表服務（FN005）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IAccountItemListReportService, AccountItemListReportService>();
            // 會計報表服務（FN006~FN011）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.ITrialBalanceReportService, TrialBalanceReportService>();
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IIncomeStatementReportService, IncomeStatementReportService>();
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IBalanceSheetReportService, BalanceSheetReportService>();
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IGeneralLedgerReportService, GeneralLedgerReportService>();
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.ISubsidiaryLedgerReportService, SubsidiaryLedgerReportService>();
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IDetailAccountBalanceReportService, DetailAccountBalanceReportService>();
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IARAgingReportService, ARAgingReportService>();
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IAPAgingReportService, APAgingReportService>();
            // 現金流量表報表服務（FN014）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.ICashFlowReportService, CashFlowReportService>();
            // 銀行存款餘額調節表報表服務（FN015）
            services.AddScoped<ERPCore2.Services.Reports.Interfaces.IBankReconciliationReportService, BankReconciliationReportService>();
            // 條碼生成服務
            services.AddSingleton<ERPCore2.Services.Reports.Interfaces.IBarcodeGeneratorService, BarcodeGeneratorService>();

            // 檔案存留服務
            services.AddScoped<IDocumentCategoryService, DocumentCategoryService>();
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<IDocumentFileService, DocumentFileService>();

            // 個人工具服務
            services.AddScoped<ERPCore2.Services.PersonalTools.IStickyNoteService, ERPCore2.Services.PersonalTools.StickyNoteService>();
            services.AddScoped<ERPCore2.Services.PersonalTools.ICalendarEventService, ERPCore2.Services.PersonalTools.CalendarEventService>();
            services.AddScoped<ERPCore2.Services.PersonalTools.IPersonalNotificationService, ERPCore2.Services.PersonalTools.PersonalNotificationService>();

            // 薪資模組服務
            services.AddScoped<ERPCore2.Services.Payroll.IPayrollItemService, ERPCore2.Services.Payroll.PayrollItemService>();
            services.AddScoped<ERPCore2.Services.Payroll.IEmployeeSalaryService, ERPCore2.Services.Payroll.EmployeeSalaryService>();
            services.AddScoped<ERPCore2.Services.Payroll.IEmployeeBankAccountService, ERPCore2.Services.Payroll.EmployeeBankAccountService>();
            services.AddScoped<ERPCore2.Services.Payroll.IPayrollPeriodService, ERPCore2.Services.Payroll.PayrollPeriodService>();
            services.AddScoped<ERPCore2.Services.Payroll.IPayrollCalculationService, ERPCore2.Services.Payroll.PayrollCalculationService>();
            // 薪資費率表服務（Phase 2）
            services.AddScoped<ERPCore2.Services.Payroll.IMinimumWageService, ERPCore2.Services.Payroll.MinimumWageService>();
            services.AddScoped<ERPCore2.Services.Payroll.IInsuranceRateService, ERPCore2.Services.Payroll.InsuranceRateService>();
            services.AddScoped<ERPCore2.Services.Payroll.ILaborInsuranceGradeService, ERPCore2.Services.Payroll.LaborInsuranceGradeService>();
            services.AddScoped<ERPCore2.Services.Payroll.IHealthInsuranceGradeService, ERPCore2.Services.Payroll.HealthInsuranceGradeService>();
            services.AddScoped<ERPCore2.Services.Payroll.IWithholdingTaxTableService, ERPCore2.Services.Payroll.WithholdingTaxTableService>();
            services.AddScoped<ERPCore2.Services.Payroll.IMonthlyAttendanceSummaryService, ERPCore2.Services.Payroll.MonthlyAttendanceSummaryService>();
            services.AddScoped<ERPCore2.Services.Payroll.IAttendanceDailyRecordService, ERPCore2.Services.Payroll.AttendanceDailyRecordService>();
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
