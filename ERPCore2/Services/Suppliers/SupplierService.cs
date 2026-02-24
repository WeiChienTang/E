using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 廠商服務實作 - 繼承 GenericManagementService
    /// </summary>
    public class SupplierService : GenericManagementService<Supplier>, ISupplierService
    {
        private readonly ISubAccountService _subAccountService;

        /// <summary>
        /// 完整建構子 - 包含 ILogger
        /// </summary>
        public SupplierService(
            IDbContextFactory<AppDbContext> contextFactory,
            ISubAccountService subAccountService,
            ILogger<GenericManagementService<Supplier>> logger) : base(contextFactory, logger)
        {
            _subAccountService = subAccountService;
        }

        /// <summary>
        /// 簡易建構子 - 不包含 ILogger
        /// </summary>
        public SupplierService(
            IDbContextFactory<AppDbContext> contextFactory,
            ISubAccountService subAccountService) : base(contextFactory)
        {
            _subAccountService = subAccountService;
        }

        #region 覆寫基底方法

        /// <summary>
        /// 覆寫建立方法 - 建立後自動產生子科目（若系統參數啟用）
        /// </summary>
        public override async Task<ServiceResult<Supplier>> CreateAsync(Supplier entity)
        {
            var result = await base.CreateAsync(entity);
            if (result.IsSuccess)
                await _subAccountService.GetOrCreateSupplierSubAccountAsync(entity.Id, entity.CreatedBy ?? "system");
            return result;
        }

        public override async Task<List<Supplier>> GetAllAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                return await context.Suppliers
                    .AsQueryable()
                    .OrderBy(s => s.CompanyName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger);
                throw;
            }
        }

        public override async Task<Supplier?> GetByIdAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                return await context.Suppliers
                    .FirstOrDefaultAsync(s => s.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new { Id = id });
                throw;
            }
        }

        public override async Task<List<Supplier>> SearchAsync(string searchTerm)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                return await context.Suppliers
                    .Where(s => (s.CompanyName != null && s.CompanyName.Contains(searchTerm)) ||
                                (s.Code != null && s.Code.Contains(searchTerm)) ||
                                (s.ContactPerson != null && s.ContactPerson.Contains(searchTerm)) ||
                                (s.TaxNumber != null && s.TaxNumber.Contains(searchTerm)))
                    .OrderBy(s => s.CompanyName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { SearchTerm = searchTerm });
                throw;
            }
        }

        public override async Task<ServiceResult> ValidateAsync(Supplier entity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var errors = new List<string>();

                // 檢查必要欄位
                if (string.IsNullOrWhiteSpace(entity.Code))
                    errors.Add("廠商編號為必填");
                
                // 檢查公司名稱、負責人或聯絡人至少要有一個
                if (string.IsNullOrWhiteSpace(entity.CompanyName) && string.IsNullOrWhiteSpace(entity.ContactPerson) && string.IsNullOrWhiteSpace(entity.ResponsiblePerson))
                    errors.Add("公司名稱、負責人或聯絡人至少需填寫一項");

                // 檢查長度限制
                if (entity.Code?.Length > 20)
                    errors.Add("廠商編號不可超過20個字元");
                
                if (entity.CompanyName?.Length > 100)
                    errors.Add("公司名稱不可超過100個字元");

                if (!string.IsNullOrEmpty(entity.ContactPerson) && entity.ContactPerson.Length > 50)
                    errors.Add("聯絡人不可超過50個字元");

                if (!string.IsNullOrEmpty(entity.TaxNumber) && entity.TaxNumber.Length > 8)
                    errors.Add("統一編號不可超過8個字元");

                // 檢查廠商編號是否重複
                if (!string.IsNullOrWhiteSpace(entity.Code))
                {
                    var isDuplicate = await context.Suppliers
                        .Where(s => s.Code == entity.Code)
                        .Where(s => s.Id != entity.Id) // 排除自己
                        .AnyAsync();

                    if (isDuplicate)
                        errors.Add("廠商編號已存在");
                }

                // 檢查公司名稱是否重複
                if (!string.IsNullOrWhiteSpace(entity.CompanyName))
                {
                    var isCompanyNameDuplicate = await context.Suppliers
                        .Where(s => s.CompanyName == entity.CompanyName)
                        .Where(s => s.Id != entity.Id) // 排除自己
                        .AnyAsync();

                    if (isCompanyNameDuplicate)
                        errors.Add("公司名稱已存在");
                }

                if (errors.Any())
                    return ServiceResult.Failure(string.Join("; ", errors));

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, 
                    new { EntityId = entity.Id, SupplierCode = entity.Code });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        #endregion

        #region 業務特定查詢方法

        public async Task<Supplier?> GetBySupplierCodeAsync(string supplierCode)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                return await context.Suppliers
                    .FirstOrDefaultAsync(s => s.Code == supplierCode);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySupplierCodeAsync), GetType(), _logger, 
                    new { SupplierCode = supplierCode });
                throw;
            }
        }

        public async Task<bool> IsSupplierCodeExistsAsync(string supplierCode, int? excludeId = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                var query = context.Suppliers.Where(s => s.Code == supplierCode);
                
                if (excludeId.HasValue)
                    query = query.Where(s => s.Id != excludeId.Value);
                    
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsSupplierCodeExistsAsync), GetType(), _logger, 
                    new { SupplierCode = supplierCode, ExcludeId = excludeId });
                throw;
            }
        }

        public async Task<bool> IsCompanyNameExistsAsync(string companyName, int? excludeId = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                if (string.IsNullOrWhiteSpace(companyName))
                    return false;

                var query = context.Suppliers.Where(s => s.CompanyName == companyName);
                
                if (excludeId.HasValue)
                    query = query.Where(s => s.Id != excludeId.Value);
                    
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsCompanyNameExistsAsync), GetType(), _logger, 
                    new { CompanyName = companyName, ExcludeId = excludeId });
                throw;
            }
        }

        #endregion

        // 聯絡資料管理已移至 ContactService
        // 地址資料管理已移至 AddressService
        // #region 地址資料管理
        // ... 相關方法已移除，請使用 IAddressService
        // #endregion

        #region 狀態管理

        public async Task<ServiceResult> UpdateSupplierStatusAsync(int supplierId, EntityStatus status)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                var supplier = await GetByIdAsync(supplierId);
                if (supplier == null)
                {
                    return ServiceResult.Failure("找不到指定的廠商");
                }

                supplier.Status = status;
                supplier.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateSupplierStatusAsync), GetType(), _logger, 
                    new { SupplierId = supplierId, Status = status });
                return ServiceResult.Failure($"更新廠商狀態時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 跨實體業務操作

        /// <inheritdoc />
        public async Task<ServiceResult<Customer>> CopyToCustomerAsync(int supplierId)
        {
            try
            {
                var supplier = await GetByIdAsync(supplierId);
                if (supplier == null)
                    return ServiceResult<Customer>.Failure($"找不到 ID 為 {supplierId} 的廠商");

                if (string.IsNullOrWhiteSpace(supplier.CompanyName))
                    return ServiceResult<Customer>.Failure("廠商公司名稱不可為空白，無法複製至客戶");

                using var context = await _contextFactory.CreateDbContextAsync();

                // 驗證客戶公司名稱不重複
                var nameExists = await context.Customers
                    .AnyAsync(c => c.CompanyName == supplier.CompanyName);
                if (nameExists)
                    return ServiceResult<Customer>.Failure($"客戶清單中已存在「{supplier.CompanyName}」，無法重複建立");

                // 產生唯一客戶編號（CUST 前置，與 CustomerService 相同格式）
                var existingCodes = await context.Customers
                    .Where(c => c.Code != null && c.Code.StartsWith("CUST"))
                    .Select(c => c.Code!)
                    .ToListAsync();

                int maxNum = 0;
                foreach (var code in existingCodes)
                {
                    if (int.TryParse(code.Substring(4), out int num) && num > maxNum)
                        maxNum = num;
                }

                string customerCode;
                do
                {
                    maxNum++;
                    customerCode = $"CUST{maxNum:D3}";
                } while (await context.Customers.AnyAsync(c => c.Code == customerCode));

                var newCustomer = new Customer
                {
                    Code                 = customerCode,
                    CompanyName          = supplier.CompanyName,
                    ContactPerson        = supplier.ContactPerson,
                    TaxNumber            = supplier.TaxNumber,
                    ResponsiblePerson    = supplier.ResponsiblePerson,
                    CompanyContactPhone  = supplier.SupplierContactPhone,
                    ContactPhone         = supplier.ContactPhone,
                    MobilePhone          = supplier.MobilePhone,
                    Email                = supplier.Email,
                    Fax                  = supplier.Fax,
                    ContactAddress       = supplier.ContactAddress,
                    ShippingAddress      = supplier.SupplierAddress,
                    Website              = supplier.Website,
                    JobTitle             = supplier.JobTitle,
                    PaymentMethodId      = supplier.PaymentMethodId,
                    PaymentTerms         = supplier.PaymentTerms,
                    Remarks              = supplier.Remarks,
                    Status               = EntityStatus.Active
                };

                context.Customers.Add(newCustomer);
                await context.SaveChangesAsync();

                return ServiceResult<Customer>.Success(newCustomer);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CopyToCustomerAsync), GetType(), _logger,
                    additionalData: $"複製廠商至客戶失敗 - SupplierId: {supplierId}");
                return ServiceResult<Customer>.Failure($"複製至客戶時發生錯誤：{ex.Message}");
            }
        }

        #endregion

        #region 輔助方法

        public void InitializeNewSupplier(Supplier supplier)
        {
            try
            {
                supplier.Code = string.Empty;
                supplier.CompanyName = string.Empty;
                supplier.ContactPerson = string.Empty;
                supplier.TaxNumber = string.Empty;
                supplier.Status = EntityStatus.Active;
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(InitializeNewSupplier), GetType(), _logger);
                throw;
            }
        }

        public int GetBasicRequiredFieldsCount()
        {
            try
            {
                return 2; // SupplierCode, CompanyName
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(GetBasicRequiredFieldsCount), GetType(), _logger);
                throw;
            }
        }

        public int GetBasicCompletedFieldsCount(Supplier supplier)
        {
            try
            {
                int count = 0;

                if (!string.IsNullOrWhiteSpace(supplier.Code))
                    count++;

                // 公司名稱或聯絡人或負責人至少要有一個
                if (!string.IsNullOrWhiteSpace(supplier.CompanyName) || !string.IsNullOrWhiteSpace(supplier.ContactPerson) || !string.IsNullOrWhiteSpace(supplier.ResponsiblePerson))
                    count++;

                return count;
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(GetBasicCompletedFieldsCount), GetType(), _logger, 
                    new { SupplierId = supplier.Id });
                throw;
            }
        }
        
        /// <summary>
        /// 覆寫基底類別的 CanDeleteAsync 方法，實作供應商特定的刪除檢查
        /// </summary>
        protected override async Task<ServiceResult> CanDeleteAsync(Supplier entity)
        {
            try
            {
                var dependencyCheck = await DependencyCheckHelper.CheckSupplierDependenciesAsync(_contextFactory, entity.Id);
                if (!dependencyCheck.CanDelete)
                {
                    return ServiceResult.Failure(dependencyCheck.GetFormattedErrorMessage("供應商"));
                }
                
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteAsync), GetType(), _logger, new { 
                    Method = nameof(CanDeleteAsync),
                    ServiceType = GetType().Name,
                    SupplierId = entity.Id 
                });
                return ServiceResult.Failure("檢查供應商刪除條件時發生錯誤");
            }
        }

        #endregion
    }
}




