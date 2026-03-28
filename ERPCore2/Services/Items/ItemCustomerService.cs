using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 品項-客戶關聯服務實作
    /// </summary>
    public class ItemCustomerService : GenericManagementService<ItemCustomer>, IItemCustomerService
    {
        public ItemCustomerService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<ItemCustomer>> logger,
            IFieldDisplaySettingService? fieldDisplaySettingService = null) : base(contextFactory, logger)
        {
            _fieldDisplaySettingService = fieldDisplaySettingService;
        }

        public ItemCustomerService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        #region 覆寫基底方法

        protected override IQueryable<ItemCustomer> BuildGetAllQuery(AppDbContext context)
        {
            return context.ItemCustomers
                .Include(pc => pc.Item)
                .Include(pc => pc.Customer)
                .OrderBy(pc => pc.ItemId)
                .ThenBy(pc => pc.Priority);
        }

        public override async Task<ServiceResult> ValidateAsync(ItemCustomer entity)
        {
            try
            {
                var errors = new List<string>();

                if (entity.ItemId <= 0)
                    errors.Add("品項為必填欄位");

                if (entity.CustomerId <= 0)
                    errors.Add("客戶為必填欄位");

                if (entity.ItemId > 0 && entity.CustomerId > 0)
                {
                    var isDuplicate = await IsBindingExistsAsync(entity.ItemId, entity.CustomerId, entity.Id);
                    if (isDuplicate)
                        errors.Add("此品項與客戶的綁定已存在");
                }

                if (entity.Priority < 1 || entity.Priority > 999)
                    errors.Add("優先順序必須介於 1 到 999 之間");

                if (entity.LeadTimeDays.HasValue && (entity.LeadTimeDays.Value < 0 || entity.LeadTimeDays.Value > 365))
                    errors.Add("交貨天數必須介於 0 到 365 之間");

                return errors.Any()
                    ? ServiceResult.Failure(string.Join("; ", errors))
                    : ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger,
                    new { EntityId = entity.Id, ItemId = entity.ItemId, CustomerId = entity.CustomerId });
                return ServiceResult.Failure($"驗證品項-客戶綁定時發生錯誤: {ex.Message}");
            }
        }

        public override async Task<List<ItemCustomer>> SearchAsync(string searchTerm)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                return await context.ItemCustomers
                    .Include(pc => pc.Item)
                    .Include(pc => pc.Customer)
                    .Where(pc => (pc.Item!.Name != null && pc.Item.Name.Contains(searchTerm)) ||
                                 (pc.Item!.Code != null && pc.Item.Code.Contains(searchTerm)) ||
                                 (pc.Customer!.CompanyName != null && pc.Customer.CompanyName.Contains(searchTerm)) ||
                                 (pc.Customer!.Code != null && pc.Customer.Code.Contains(searchTerm)) ||
                                 (pc.CustomerItemCode != null && pc.CustomerItemCode.Contains(searchTerm)))
                    .OrderByDescending(pc => pc.IsPreferred)
                    .ThenBy(pc => pc.Priority)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { SearchTerm = searchTerm });
                throw;
            }
        }

        #endregion

        #region 查詢方法

        public async Task<List<ItemCustomer>> GetByItemIdAsync(int productId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                return await context.ItemCustomers
                    .Include(pc => pc.Customer)
                    .Where(pc => pc.ItemId == productId)
                    .OrderBy(pc => pc.Priority)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByItemIdAsync), GetType(), _logger);
                return new List<ItemCustomer>();
            }
        }

        public async Task<List<ItemCustomer>> GetByCustomerIdAsync(int customerId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                return await context.ItemCustomers
                    .Include(pc => pc.Item)
                    .Where(pc => pc.CustomerId == customerId)
                    .OrderBy(pc => pc.Priority)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByCustomerIdAsync), GetType(), _logger);
                return new List<ItemCustomer>();
            }
        }

        public async Task<List<ItemCustomer>> GetPreferredCustomersByItemIdAsync(int productId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                return await context.ItemCustomers
                    .Include(pc => pc.Customer)
                    .Where(pc => pc.ItemId == productId && pc.IsPreferred)
                    .OrderBy(pc => pc.Priority)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPreferredCustomersByItemIdAsync), GetType(), _logger);
                return new List<ItemCustomer>();
            }
        }

        public async Task<bool> IsBindingExistsAsync(int productId, int customerId, int? excludeId = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                var query = context.ItemCustomers
                    .Where(pc => pc.ItemId == productId && pc.CustomerId == customerId);
                if (excludeId.HasValue)
                    query = query.Where(pc => pc.Id != excludeId.Value);
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsBindingExistsAsync), GetType(), _logger);
                return false;
            }
        }

        #endregion

        #region 輔助方法

        public async Task UpdateLastSaleInfoAsync(int productId, int customerId, decimal price, DateTime date)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                var binding = await context.ItemCustomers
                    .FirstOrDefaultAsync(pc => pc.ItemId == productId && pc.CustomerId == customerId);
                if (binding != null)
                {
                    binding.LastSalePrice = price;
                    binding.LastSaleDate = date;
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateLastSaleInfoAsync), GetType(), _logger);
            }
        }

        #endregion
    }
}
