using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 地址管理服務實作 - 使用 DbContextFactory 避免並發問題
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

        #region 地址資料查詢

        public async Task<List<AddressType>> GetAddressTypesAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.AddressTypes
                    .Where(at => at.Status == EntityStatus.Active)
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
                    .Include(ca => ca.AddressType)
                    .Where(ca => ca.CustomerId == customerId && ca.Status != EntityStatus.Deleted)
                    .OrderBy(ca => ca.IsPrimary ? 0 : 1) // 主要地址排在前面
                    .ThenBy(ca => ca.AddressType!.TypeName)
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
                    .Include(ca => ca.AddressType)
                    .FirstOrDefaultAsync(ca => ca.CustomerId == customerId && 
                                             ca.IsPrimary && 
                                             ca.Status != EntityStatus.Deleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting primary address for customer {CustomerId}", customerId);
                throw;
            }
        }

        #endregion

        #region 地址 CRUD 操作

        public async Task<ServiceResult<CustomerAddress>> CreateAddressAsync(CustomerAddress address)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                // 驗證地址
                var validationResult = await ValidateAddressAsync(address);
                if (!validationResult.IsSuccess)
                    return ServiceResult<CustomerAddress>.Failure(validationResult.ErrorMessage);

                // 確保客戶存在
                var customerExists = await context.Customers
                    .AnyAsync(c => c.CustomerId == address.CustomerId && c.Status != EntityStatus.Deleted);
                if (!customerExists)
                    return ServiceResult<CustomerAddress>.Failure("客戶不存在");

                // 如果設為主要地址，需要先清除其他主要地址
                if (address.IsPrimary)
                {
                    await ClearPrimaryAddressesAsync(context, address.CustomerId);
                }

                // 設定預設值
                address.Status = EntityStatus.Active;

                context.CustomerAddresses.Add(address);
                await context.SaveChangesAsync();

                _logger.LogInformation("Created address for customer {CustomerId}", address.CustomerId);
                return ServiceResult<CustomerAddress>.Success(address);
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

                var existingAddress = await context.CustomerAddresses
                    .FirstOrDefaultAsync(ca => ca.AddressId == address.AddressId);

                if (existingAddress == null)
                    return ServiceResult<CustomerAddress>.Failure("地址不存在");

                // 驗證地址
                var validationResult = await ValidateAddressAsync(address);
                if (!validationResult.IsSuccess)
                    return ServiceResult<CustomerAddress>.Failure(validationResult.ErrorMessage);

                // 如果設為主要地址，需要先清除其他主要地址
                if (address.IsPrimary && !existingAddress.IsPrimary)
                {
                    await ClearPrimaryAddressesAsync(context, address.CustomerId);
                }

                // 更新屬性
                existingAddress.AddressTypeId = address.AddressTypeId;
                existingAddress.PostalCode = address.PostalCode;
                existingAddress.City = address.City;
                existingAddress.District = address.District;
                existingAddress.Address = address.Address;
                existingAddress.IsPrimary = address.IsPrimary;

                await context.SaveChangesAsync();

                _logger.LogInformation("Updated address {AddressId} for customer {CustomerId}", 
                    address.AddressId, address.CustomerId);
                return ServiceResult<CustomerAddress>.Success(existingAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address {AddressId}", address.AddressId);
                return ServiceResult<CustomerAddress>.Failure($"更新地址時發生錯誤: {ex.Message}");
            }
        }

        public async Task<ServiceResult> DeleteAddressAsync(int addressId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                var address = await context.CustomerAddresses
                    .FirstOrDefaultAsync(ca => ca.AddressId == addressId);

                if (address == null)
                    return ServiceResult.Failure("地址不存在");

                // 檢查是否為客戶的唯一地址
                var customerAddressCount = await context.CustomerAddresses
                    .CountAsync(ca => ca.CustomerId == address.CustomerId && 
                                    ca.Status != EntityStatus.Deleted);

                if (customerAddressCount <= 1)
                    return ServiceResult.Failure("客戶至少需要保留一個地址");

                // 軟刪除
                address.Status = EntityStatus.Deleted;

                // 如果刪除的是主要地址，需要設定另一個地址為主要地址
                if (address.IsPrimary)
                {
                    var newPrimaryAddress = await context.CustomerAddresses
                        .FirstOrDefaultAsync(ca => ca.CustomerId == address.CustomerId && 
                                           ca.AddressId != addressId && 
                                           ca.Status != EntityStatus.Deleted);
                    
                    if (newPrimaryAddress != null)
                        newPrimaryAddress.IsPrimary = true;
                }

                await context.SaveChangesAsync();

                _logger.LogInformation("Deleted address {AddressId} for customer {CustomerId}", 
                    addressId, address.CustomerId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting address {AddressId}", addressId);
                return ServiceResult.Failure($"刪除地址時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 地址業務邏輯

        public async Task<ServiceResult> SetPrimaryAddressAsync(int customerId, int addressId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                // 檢查地址是否存在且屬於該客戶
                var address = await context.CustomerAddresses
                    .FirstOrDefaultAsync(ca => ca.AddressId == addressId && 
                                             ca.CustomerId == customerId && 
                                             ca.Status != EntityStatus.Deleted);

                if (address == null)
                    return ServiceResult.Failure("地址不存在或不屬於該客戶");

                // 清除其他主要地址
                await ClearPrimaryAddressesAsync(context, customerId);

                // 設定新的主要地址
                address.IsPrimary = true;
                await context.SaveChangesAsync();

                _logger.LogInformation("Set address {AddressId} as primary for customer {CustomerId}", 
                    addressId, customerId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting primary address {AddressId} for customer {CustomerId}", 
                    addressId, customerId);
                return ServiceResult.Failure($"設定主要地址時發生錯誤: {ex.Message}");
            }
        }

        public async Task<ServiceResult> UpdateCustomerAddressesAsync(int customerId, List<CustomerAddress> addresses)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                // 驗證所有地址
                foreach (var address in addresses)
                {
                    var validationResult = await ValidateAddressAsync(address);
                    if (!validationResult.IsSuccess)
                        return ServiceResult.Failure($"地址驗證失敗: {validationResult.ErrorMessage}");
                }

                // 確保只有一個主要地址
                var primaryCount = addresses.Count(a => a.IsPrimary);
                if (primaryCount == 0)
                    return ServiceResult.Failure("至少需要設定一個主要地址");
                if (primaryCount > 1)
                    return ServiceResult.Failure("只能設定一個主要地址");

                // 取得現有地址
                var existingAddresses = await context.CustomerAddresses
                    .Where(ca => ca.CustomerId == customerId)
                    .ToListAsync();

                // 更新或新增地址
                foreach (var address in addresses)
                {
                    address.CustomerId = customerId;
                    
                    if (address.AddressId == 0)
                    {
                        // 新地址
                        address.Status = EntityStatus.Active;
                        context.CustomerAddresses.Add(address);
                    }
                    else
                    {
                        // 更新現有地址
                        var existingAddress = existingAddresses
                            .FirstOrDefault(ea => ea.AddressId == address.AddressId);
                        
                        if (existingAddress != null)
                        {
                            existingAddress.AddressTypeId = address.AddressTypeId;
                            existingAddress.PostalCode = address.PostalCode;
                            existingAddress.City = address.City;
                            existingAddress.District = address.District;
                            existingAddress.Address = address.Address;
                            existingAddress.IsPrimary = address.IsPrimary;
                        }
                    }
                }

                // 標記刪除未在新清單中的地址
                var newAddressIds = addresses.Where(a => a.AddressId > 0).Select(a => a.AddressId);
                var addressesToDelete = existingAddresses
                    .Where(ea => !newAddressIds.Contains(ea.AddressId) && ea.Status != EntityStatus.Deleted);

                foreach (var addressToDelete in addressesToDelete)
                {
                    addressToDelete.Status = EntityStatus.Deleted;
                }

                await context.SaveChangesAsync();

                _logger.LogInformation("Updated addresses for customer {CustomerId}", customerId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating addresses for customer {CustomerId}", customerId);
                return ServiceResult.Failure($"更新客戶地址時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 地址驗證和業務規則

        public async Task<ServiceResult> ValidateAddressAsync(CustomerAddress address)
        {
            try
            {
                var errors = new List<string>();

                // 檢查必要欄位
                if (address.CustomerId <= 0)
                    errors.Add("客戶ID無效");

                // 檢查長度限制
                if (!string.IsNullOrEmpty(address.PostalCode) && address.PostalCode.Length > 10)
                    errors.Add("郵遞區號不可超過10個字元");

                if (!string.IsNullOrEmpty(address.City) && address.City.Length > 50)
                    errors.Add("城市不可超過50個字元");

                if (!string.IsNullOrEmpty(address.District) && address.District.Length > 50)
                    errors.Add("行政區不可超過50個字元");

                if (!string.IsNullOrEmpty(address.Address) && address.Address.Length > 200)
                    errors.Add("地址不可超過200個字元");

                // 檢查地址類型是否存在
                if (address.AddressTypeId.HasValue)
                {
                    using var context = _contextFactory.CreateDbContext();
                    var addressTypeExists = await context.AddressTypes
                        .AnyAsync(at => at.AddressTypeId == address.AddressTypeId.Value && 
                                      at.Status == EntityStatus.Active);
                    
                    if (!addressTypeExists)
                        errors.Add("指定的地址類型不存在或已停用");
                }

                if (errors.Any())
                    return ServiceResult.ValidationFailure(errors);

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating address");
                return ServiceResult.Failure("驗證地址時發生錯誤");
            }
        }

        public async Task<ServiceResult> EnsureCustomerHasPrimaryAddressAsync(int customerId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                var primaryAddress = await context.CustomerAddresses
                    .FirstOrDefaultAsync(ca => ca.CustomerId == customerId && 
                                             ca.IsPrimary && 
                                             ca.Status != EntityStatus.Deleted);

                if (primaryAddress == null)
                {
                    // 尋找第一個可用的地址設為主要地址
                    var firstAddress = await context.CustomerAddresses
                        .FirstOrDefaultAsync(ca => ca.CustomerId == customerId && 
                                                 ca.Status != EntityStatus.Deleted);

                    if (firstAddress != null)
                    {
                        firstAddress.IsPrimary = true;
                        await context.SaveChangesAsync();
                        
                        _logger.LogInformation("Set first address as primary for customer {CustomerId}", customerId);
                    }
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring customer has primary address for customer {CustomerId}", customerId);
                return ServiceResult.Failure($"確保客戶有主要地址時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 地址操作輔助方法

        public async Task<ServiceResult<CustomerAddress>> CopyFromAddressAsync(CustomerAddress sourceAddress, int targetCustomerId, int? targetAddressTypeId = null)
        {
            try
            {
                var newAddress = new CustomerAddress
                {
                    CustomerId = targetCustomerId,
                    AddressTypeId = targetAddressTypeId ?? sourceAddress.AddressTypeId,
                    PostalCode = sourceAddress.PostalCode,
                    City = sourceAddress.City,
                    District = sourceAddress.District,
                    Address = sourceAddress.Address,
                    IsPrimary = false, // 複製的地址不設為主要地址
                    Status = EntityStatus.Active
                };

                return await CreateAddressAsync(newAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error copying address from customer {SourceCustomerId} to {TargetCustomerId}", 
                    sourceAddress.CustomerId, targetCustomerId);
                return ServiceResult<CustomerAddress>.Failure($"複製地址時發生錯誤: {ex.Message}");
            }
        }

        public async Task<int?> GetDefaultAddressTypeIdAsync(string addressTypeName)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var addressType = await context.AddressTypes
                    .FirstOrDefaultAsync(at => at.TypeName.Contains(addressTypeName) && 
                                             at.Status == EntityStatus.Active);

                return addressType?.AddressTypeId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting default address type ID for name {AddressTypeName}", addressTypeName);
                throw;
            }
        }

        public async Task<List<CustomerAddress>> GetAddressesWithDefaultAsync(int customerId, List<AddressType> addressTypes)
        {
            try
            {
                var addresses = await GetAddressesByCustomerIdAsync(customerId);

                // 如果客戶沒有地址，建立預設地址
                if (!addresses.Any() && addressTypes.Any())
                {
                    var defaultAddressType = addressTypes.FirstOrDefault(at => at.TypeName.Contains("公司")) ??
                                           addressTypes.FirstOrDefault();

                    if (defaultAddressType != null)
                    {
                        var defaultAddress = new CustomerAddress
                        {
                            CustomerId = customerId,
                            AddressTypeId = defaultAddressType.AddressTypeId,
                            IsPrimary = true,
                            Status = EntityStatus.Active,
                            PostalCode = string.Empty,
                            City = string.Empty,
                            District = string.Empty,
                            Address = string.Empty
                        };

                        var createResult = await CreateAddressAsync(defaultAddress);
                        if (createResult.IsSuccess && createResult.Data != null)
                        {
                            // 重新載入以包含 AddressType 導航屬性
                            addresses = await GetAddressesByCustomerIdAsync(customerId);
                        }
                    }
                }

                return addresses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting addresses with default for customer {CustomerId}", customerId);
                throw;
            }
        }

        #endregion

        #region 私有輔助方法

        private async Task ClearPrimaryAddressesAsync(AppDbContext context, int customerId)
        {
            var primaryAddresses = await context.CustomerAddresses
                .Where(ca => ca.CustomerId == customerId && 
                           ca.IsPrimary && 
                           ca.Status != EntityStatus.Deleted)
                .ToListAsync();

            foreach (var address in primaryAddresses)
            {
                address.IsPrimary = false;
            }
        }

        #endregion
    }
}