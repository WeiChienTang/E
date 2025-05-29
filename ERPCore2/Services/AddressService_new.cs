using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Services
{
    /// <summary>
    /// 地址管理服務實作 - 使用 DbContextFactory 解決並發問題
    /// </summary>
    public class AddressService : IAddressService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ILogger<AddressService> _logger;

        public AddressService(IDbContextFactory<AppDbContext> contextFactory, ILogger<AddressService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        #region 取得地址相關資料

        public async Task<List<AddressType>> GetAddressTypesAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.AddressTypes
                    .Where(at => at.Status != EntityStatus.Deleted)
                    .OrderBy(at => at.TypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting address types");
                throw;
            }
        }

        public async Task<List<CustomerAddress>> GetAddressesByCustomerIdAsync(int customerId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.CustomerAddresses
                    .Where(ca => ca.CustomerId == customerId && ca.Status != EntityStatus.Deleted)
                    .Include(ca => ca.AddressType)
                    .OrderByDescending(ca => ca.IsPrimary)
                    .ThenBy(ca => ca.AddressId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting addresses for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<CustomerAddress?> GetPrimaryAddressAsync(int customerId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.CustomerAddresses
                    .Where(ca => ca.CustomerId == customerId && ca.IsPrimary && ca.Status != EntityStatus.Deleted)
                    .Include(ca => ca.AddressType)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting primary address for customer {CustomerId}", customerId);
                throw;
            }
        }

        #endregion

        #region 地址業務邏輯操作

        public async Task<ServiceResult<CustomerAddress>> CreateAddressAsync(CustomerAddress address)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                // 1. 驗證地址資料
                var validationResult = await ValidateAddressAsync(address);
                if (!validationResult.IsSuccess)
                    return ServiceResult<CustomerAddress>.Failure(validationResult.ErrorMessage);

                // 2. 檢查客戶是否存在
                var customerExists = await context.Customers
                    .AnyAsync(c => c.CustomerId == address.CustomerId && c.Status != EntityStatus.Deleted);
                if (!customerExists)
                    return ServiceResult<CustomerAddress>.Failure("客戶不存在");

                // 3. 如果設為主要地址，清除其他主要地址標記
                if (address.IsPrimary)
                {
                    await ClearPrimaryAddressAsync(address.CustomerId);
                }

                // 4. 如果是客戶的第一個地址，自動設為主要地址
                var existingAddressCount = await context.CustomerAddresses
                    .CountAsync(ca => ca.CustomerId == address.CustomerId && ca.Status != EntityStatus.Deleted);

                if (existingAddressCount == 0)
                {
                    address.IsPrimary = true;
                }

                // 5. 設定基本欄位
                address.Status = EntityStatus.Active;
                address.CreatedAt = DateTime.Now;
                address.UpdatedAt = DateTime.Now;

                context.CustomerAddresses.Add(address);
                await context.SaveChangesAsync();

                // 6. 重新查詢包含關聯資料的地址
                var savedAddress = await context.CustomerAddresses
                    .Include(ca => ca.AddressType)
                    .FirstOrDefaultAsync(ca => ca.AddressId == address.AddressId);

                return ServiceResult<CustomerAddress>.Success(savedAddress!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating address for customer {CustomerId}", address.CustomerId);
                return ServiceResult<CustomerAddress>.Failure($"建立地址時發生錯誤: {ex.Message}");
            }
        }

        public async Task<ServiceResult<CustomerAddress>> UpdateAddressAsync(CustomerAddress address)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                // 1. 驗證地址資料
                var validationResult = await ValidateAddressAsync(address);
                if (!validationResult.IsSuccess)
                    return ServiceResult<CustomerAddress>.Failure(validationResult.ErrorMessage);

                // 2. 檢查地址是否存在
                var existingAddress = await context.CustomerAddresses
                    .FirstOrDefaultAsync(ca => ca.AddressId == address.AddressId && ca.Status != EntityStatus.Deleted);

                if (existingAddress == null)
                    return ServiceResult<CustomerAddress>.Failure("地址不存在");

                // 3. 如果設為主要地址，清除其他主要地址標記
                if (address.IsPrimary && !existingAddress.IsPrimary)
                {
                    await ClearPrimaryAddressAsync(address.CustomerId);
                }

                // 4. 更新欄位
                context.Entry(existingAddress).CurrentValues.SetValues(address);
                existingAddress.UpdatedAt = DateTime.Now;

                await context.SaveChangesAsync();

                // 5. 重新查詢包含關聯資料的地址
                var updatedAddress = await context.CustomerAddresses
                    .Include(ca => ca.AddressType)
                    .FirstOrDefaultAsync(ca => ca.AddressId == address.AddressId);

                return ServiceResult<CustomerAddress>.Success(updatedAddress!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address {AddressId}", address.AddressId);
                return ServiceResult<CustomerAddress>.Failure($"更新地址時發生錯誤: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> SetPrimaryAddressAsync(int addressId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                var address = await context.CustomerAddresses
                    .FirstOrDefaultAsync(ca => ca.AddressId == addressId && ca.Status != EntityStatus.Deleted);

                if (address == null)
                    return ServiceResult<bool>.Failure("地址不存在");

                if (address.IsPrimary)
                    return ServiceResult<bool>.Success(true); // 已經是主要地址

                // 清除該客戶其他主要地址標記
                await ClearPrimaryAddressAsync(address.CustomerId);

                // 設為主要地址
                address.IsPrimary = true;
                address.UpdatedAt = DateTime.Now;

                // 檢查是否有其他主要地址，如果有則清除
                var otherAddresses = await context.CustomerAddresses
                    .Where(ca => ca.CustomerId == address.CustomerId && 
                                ca.AddressId != addressId && 
                                ca.IsPrimary && 
                                ca.Status != EntityStatus.Deleted)
                    .ToListAsync();

                foreach (var otherAddress in otherAddresses)
                {
                    otherAddress.IsPrimary = false;
                    otherAddress.UpdatedAt = DateTime.Now;
                }

                await context.SaveChangesAsync();
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting primary address {AddressId}", addressId);
                return ServiceResult<bool>.Failure($"設定主要地址時發生錯誤: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> DeleteAddressAsync(int addressId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                var address = await context.CustomerAddresses
                    .FirstOrDefaultAsync(ca => ca.AddressId == addressId && ca.Status != EntityStatus.Deleted);

                if (address == null)
                    return ServiceResult<bool>.Failure("地址不存在");

                // 軟刪除
                address.Status = EntityStatus.Deleted;
                address.UpdatedAt = DateTime.Now;

                // 如果刪除的是主要地址，需要設定新的主要地址
                if (address.IsPrimary)
                {
                    await SetNewPrimaryAddressAsync(address.CustomerId);
                }

                await context.SaveChangesAsync();
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting address {AddressId}", addressId);
                return ServiceResult<bool>.Failure($"刪除地址時發生錯誤: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> UpdateCustomerAddressesAsync(int customerId, List<CustomerAddress> addresses)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                using var transaction = await context.Database.BeginTransactionAsync();
                
                try
                {
                    // 取得現有地址
                    var existingAddresses = await context.CustomerAddresses
                        .Where(ca => ca.CustomerId == customerId && ca.Status != EntityStatus.Deleted)
                        .ToListAsync();

                    // 刪除不在新清單中的地址
                    foreach (var existing in existingAddresses)
                    {
                        if (!addresses.Any(a => a.AddressId == existing.AddressId))
                        {
                            existing.Status = EntityStatus.Deleted;
                            existing.UpdatedAt = DateTime.Now;
                        }
                    }

                    // 新增或更新地址
                    foreach (var address in addresses)
                    {
                        if (address.AddressId == 0)
                        {
                            // 新增
                            address.CustomerId = customerId;
                            address.Status = EntityStatus.Active;
                            address.CreatedAt = DateTime.Now;
                            address.UpdatedAt = DateTime.Now;
                            context.CustomerAddresses.Add(address);
                        }
                        else
                        {
                            // 更新
                            var existing = existingAddresses.FirstOrDefault(ea => ea.AddressId == address.AddressId);
                            if (existing != null)
                            {
                                context.Entry(existing).CurrentValues.SetValues(address);
                                existing.UpdatedAt = DateTime.Now;
                            }
                        }
                    }

                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return ServiceResult<bool>.Success(true);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer addresses for {CustomerId}", customerId);
                return ServiceResult<bool>.Failure($"更新地址清單時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 私有輔助方法

        private async Task<ServiceResult<bool>> ValidateAddressAsync(CustomerAddress address)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                // 檢查地址類型是否存在且有效
                if (address.AddressTypeId.HasValue)
                {
                    var addressTypeExists = await context.AddressTypes
                        .AnyAsync(at => at.AddressTypeId == address.AddressTypeId.Value && at.Status != EntityStatus.Deleted);
                    if (!addressTypeExists)
                        return ServiceResult<bool>.Failure("指定的地址類型不存在");
                }

                // 其他驗證邏輯...
                if (string.IsNullOrWhiteSpace(address.AddressLine))
                    return ServiceResult<bool>.Failure("地址行為必填");

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating address");
                return ServiceResult<bool>.Failure("驗證地址時發生錯誤");
            }
        }

        private async Task SetNewPrimaryAddressAsync(int customerId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                // 找到第一個地址設為主要地址
                var firstAddress = await context.CustomerAddresses
                    .Where(ca => ca.CustomerId == customerId && ca.Status != EntityStatus.Deleted)
                    .OrderBy(ca => ca.AddressId)
                    .FirstOrDefaultAsync();

                if (firstAddress != null)
                {
                    firstAddress.IsPrimary = true;
                    firstAddress.UpdatedAt = DateTime.Now;
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting new primary address for customer {CustomerId}", customerId);
            }
        }

        private async Task<string> GetAddressTypeNameAsync(int? addressTypeId)
        {
            if (!addressTypeId.HasValue)
                return "未指定";

            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                var addressType = await context.AddressTypes
                    .FirstOrDefaultAsync(at => at.AddressTypeId == addressTypeId.Value);
                return addressType?.TypeName ?? "未知類型";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting address type name for {AddressTypeId}", addressTypeId);
                return "未知類型";
            }
        }

        private async Task ClearPrimaryAddressAsync(int customerId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                var primaryAddresses = await context.CustomerAddresses
                    .Where(ca => ca.CustomerId == customerId && ca.IsPrimary && ca.Status != EntityStatus.Deleted)
                    .ToListAsync();

                foreach (var address in primaryAddresses)
                {
                    address.IsPrimary = false;
                    address.UpdatedAt = DateTime.Now;
                }

                if (primaryAddresses.Any())
                {
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing primary addresses for customer {CustomerId}", customerId);
            }
        }

        #endregion
    }
}
