using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services.Crm
{
    /// <summary>
    /// 潛在客戶服務實作
    /// </summary>
    public class CrmLeadService : GenericManagementService<CrmLead>, ICrmLeadService
    {
        private readonly ICustomerService _customerService;

        public CrmLeadService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<CrmLead>> logger,
            ICustomerService customerService) : base(contextFactory, logger)
        {
            _customerService = customerService;
        }

        protected override IQueryable<CrmLead> BuildGetAllQuery(AppDbContext context)
        {
            return context.CrmLeads
                .Include(l => l.AssignedEmployee)
                .Include(l => l.ConvertedCustomer)
                .OrderByDescending(l => l.CreatedAt);
        }

        public override async Task<CrmLead?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.CrmLeads
                    .Include(l => l.AssignedEmployee)
                    .Include(l => l.ConvertedCustomer)
                    .Include(l => l.FollowUps).ThenInclude(f => f.Employee)
                    .FirstOrDefaultAsync(l => l.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new { Id = id });
                throw;
            }
        }

        public override async Task<List<CrmLead>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                var lower = searchTerm.ToLower();
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.CrmLeads
                    .Include(l => l.AssignedEmployee)
                    .Include(l => l.ConvertedCustomer)
                    .Where(l => l.CompanyName.ToLower().Contains(lower) ||
                                (l.ContactPerson != null && l.ContactPerson.ToLower().Contains(lower)) ||
                                (l.Industry != null && l.Industry.ToLower().Contains(lower)))
                    .OrderByDescending(l => l.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { SearchTerm = searchTerm });
                throw;
            }
        }

        public override async Task<ServiceResult> ValidateAsync(CrmLead entity)
        {
            try
            {
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(entity.CompanyName))
                    errors.Add("請輸入公司名稱");

                // 公司名稱重複檢查（排除自身）
                var nameExists = await IsCompanyNameExistsAsync(entity.CompanyName, entity.Id > 0 ? entity.Id : null);
                if (nameExists)
                    errors.Add($"公司名稱「{entity.CompanyName}」已存在");

                return errors.Any()
                    ? ServiceResult.Failure(string.Join("; ", errors))
                    : ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new { EntityId = entity.Id });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        public async Task<(List<CrmLead> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<CrmLead>, IQueryable<CrmLead>>? filterFunc,
            int pageNumber,
            int pageSize)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                IQueryable<CrmLead> query = context.CrmLeads
                    .Include(l => l.AssignedEmployee)
                    .Include(l => l.ConvertedCustomer);

                if (filterFunc != null) query = filterFunc(query);
                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderByDescending(l => l.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                return (items, totalCount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPagedWithFiltersAsync), GetType(), _logger);
                return (new List<CrmLead>(), 0);
            }
        }

        public async Task<List<CrmLead>> GetByEmployeeAsync(int employeeId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.CrmLeads
                    .Include(l => l.AssignedEmployee)
                    .Include(l => l.ConvertedCustomer)
                    .Where(l => l.AssignedEmployeeId == employeeId)
                    .OrderByDescending(l => l.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByEmployeeAsync), GetType(), _logger, new { EmployeeId = employeeId });
                throw;
            }
        }

        public async Task<ServiceResult<int>> ConvertToCustomerAsync(int leadId, string? createdBy = null)
        {
            try
            {
                var lead = await GetByIdAsync(leadId);
                if (lead == null)
                    return ServiceResult<int>.Failure($"找不到 ID 為 {leadId} 的潛在客戶");

                if (lead.ConvertedCustomerId.HasValue)
                    return ServiceResult<int>.Failure("此潛在客戶已轉換為正式客戶");

                if (string.IsNullOrWhiteSpace(lead.CompanyName))
                    return ServiceResult<int>.Failure("公司名稱不可為空白");

                // 建立正式客戶
                var newCustomer = new Customer
                {
                    CompanyName   = lead.CompanyName,
                    ContactPerson = lead.ContactPerson,
                    ContactPhone  = lead.ContactPhone,
                    Email         = lead.Email,
                    CustomerSource = lead.LeadSource switch
                    {
                        LeadSource.BusinessDevelopment => CustomerSource.BusinessDevelopment,
                        LeadSource.Referral            => CustomerSource.Referral,
                        LeadSource.Exhibition          => CustomerSource.Exhibition,
                        LeadSource.Internet            => CustomerSource.Internet,
                        _                              => CustomerSource.Other
                    },
                    EmployeeId = lead.AssignedEmployeeId,
                    Status     = EntityStatus.Active,
                    CreatedBy  = createdBy
                };

                var createResult = await _customerService.CreateAsync(newCustomer);
                if (!createResult.IsSuccess)
                    return ServiceResult<int>.Failure($"建立客戶失敗：{createResult.ErrorMessage}");

                // 更新潛在客戶的轉換資訊
                lead.ConvertedCustomerId = createResult.Data?.Id ?? newCustomer.Id;
                lead.ConvertedAt         = DateTime.UtcNow;
                lead.LeadStage           = LeadStage.Won;

                var updateResult = await UpdateAsync(lead);
                if (!updateResult.IsSuccess)
                    return ServiceResult<int>.Failure($"更新潛在客戶狀態失敗：{updateResult.ErrorMessage}");

                return ServiceResult<int>.Success(lead.ConvertedCustomerId.Value);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ConvertToCustomerAsync), GetType(), _logger,
                    additionalData: $"轉換失敗 - LeadId: {leadId}");
                return ServiceResult<int>.Failure($"轉換時發生錯誤：{ex.Message}");
            }
        }

        public async Task<bool> IsCompanyNameExistsAsync(string companyName, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(companyName)) return false;
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.CrmLeads.Where(l => l.CompanyName == companyName);
                if (excludeId.HasValue) query = query.Where(l => l.Id != excludeId.Value);
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsCompanyNameExistsAsync), GetType(), _logger);
                return false;
            }
        }
    }
}
