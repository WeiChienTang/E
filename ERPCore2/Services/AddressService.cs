using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Services
{
    /// <summary>
    /// 地址管理服務實作 - 直接使用 EF Core，無需 Repository 和 DTO
    /// </summary>
    public class AddressService : IAddressService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AddressService> _logger;

        public AddressService(AppDbContext context, ILogger<AddressService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region 取得地址相關資料

        public async Task<List<AddressType>> GetAddressTypesAsync()
        {
            try
            {
                return await _context.AddressTypes
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
                return await _context.CustomerAddresses
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
                return await _context.CustomerAddresses
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
                // 1. 驗證地址資料
                var validationResult = await ValidateAddressAsync(address);
                if (!validationResult.IsSuccess)
                    return ServiceResult<CustomerAddress>.Failure(validationResult.ErrorMessage);

                // 2. 檢查客戶是否存在
                var customerExists = await _context.Customers
                    .AnyAsync(c => c.CustomerId == address.CustomerId && c.Status != EntityStatus.Deleted);
                if (!customerExists)
                    return ServiceResult<CustomerAddress>.Failure("客戶不存在");

                // 3. 如果設為主要地址，清除其他主要地址標記
                if (address.IsPrimary)
                {
                    await ClearPrimaryAddressAsync(address.CustomerId);
                }

                // 4. 如果是客戶的第一個地址，自動設為主要地址
                var existingAddressCount = await _context.CustomerAddresses
                    .CountAsync(ca => ca.CustomerId == address.CustomerId && ca.Status != EntityStatus.Deleted);
                if (existingAddressCount == 0)
                {
                    address.IsPrimary = true;
                }

                // 5. 設定稽核欄位
                address.Status = EntityStatus.Active;

                // 6. 儲存地址
                _context.CustomerAddresses.Add(address);
                await _context.SaveChangesAsync();

                // 7. 重新載入包含關聯資料的地址
                var savedAddress = await _context.CustomerAddresses
                    .Include(ca => ca.AddressType)
                    .FirstOrDefaultAsync(ca => ca.AddressId == address.AddressId);

                return ServiceResult<CustomerAddress>.Success(savedAddress!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating address for customer {CustomerId}", address.CustomerId);
                return ServiceResult<CustomerAddress>.Failure($"新增地址時發生錯誤: {ex.Message}");
            }
        }

        public async Task<ServiceResult<CustomerAddress>> UpdateAddressAsync(CustomerAddress address)
        {
            try
            {
                // 1. 檢查地址是否存在
                var existingAddress = await _context.CustomerAddresses
                    .FirstOrDefaultAsync(ca => ca.AddressId == address.AddressId && ca.Status != EntityStatus.Deleted);
                if (existingAddress == null)
                    return ServiceResult<CustomerAddress>.Failure("地址不存在");

                // 2. 驗證地址資料
                var validationResult = await ValidateAddressAsync(address);
                if (!validationResult.IsSuccess)
                    return ServiceResult<CustomerAddress>.Failure(validationResult.ErrorMessage);

                // 3. 如果設為主要地址，清除其他主要地址標記
                if (address.IsPrimary && !existingAddress.IsPrimary)
                {
                    await ClearPrimaryAddressAsync(address.CustomerId);
                }

                // 4. 更新地址資料
                _context.Entry(existingAddress).CurrentValues.SetValues(address);
                await _context.SaveChangesAsync();

                // 5. 重新載入包含關聯資料的地址
                var updatedAddress = await _context.CustomerAddresses
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

        public async Task<ServiceResult> DeleteAddressAsync(int addressId)
        {
            try
            {
                var address = await _context.CustomerAddresses
                    .FirstOrDefaultAsync(ca => ca.AddressId == addressId && ca.Status != EntityStatus.Deleted);
                if (address == null)
                    return ServiceResult.Failure("地址不存在");

                // 檢查是否為主要地址，如果是且還有其他地址，需要重新指定主要地址
                if (address.IsPrimary)
                {
                    var otherAddresses = await _context.CustomerAddresses
                        .Where(ca => ca.CustomerId == address.CustomerId && 
                                    ca.AddressId != addressId && 
                                    ca.Status != EntityStatus.Deleted)
                        .ToListAsync();

                    if (otherAddresses.Any())
                    {
                        otherAddresses.First().IsPrimary = true;
                    }
                }

                // 軟刪除
                address.Status = EntityStatus.Deleted;
                await _context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting address {AddressId}", addressId);
                return ServiceResult.Failure($"刪除地址時發生錯誤: {ex.Message}");
            }
        }

        public async Task<ServiceResult> SetPrimaryAddressAsync(int customerId, int addressId)
        {
            try
            {
                // 檢查地址是否屬於該客戶
                var address = await _context.CustomerAddresses
                    .FirstOrDefaultAsync(ca => ca.AddressId == addressId && 
                                              ca.CustomerId == customerId && 
                                              ca.Status != EntityStatus.Deleted);
                if (address == null)
                    return ServiceResult.Failure("地址不存在或不屬於該客戶");

                // 清除該客戶的所有主要地址標記
                await ClearPrimaryAddressAsync(customerId);

                // 設定新的主要地址
                address.IsPrimary = true;
                await _context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting primary address {AddressId} for customer {CustomerId}", addressId, customerId);                return ServiceResult.Failure($"設定主要地址時發生錯誤: {ex.Message}");
            }
        }

        public async Task<ServiceResult> UpdateCustomerAddressesAsync(int customerId, List<CustomerAddress> addresses)
        {
            try
            {
                // 使用交易確保資料一致性
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    // 1. 取得現有地址
                    var existingAddresses = await _context.CustomerAddresses
                        .Where(ca => ca.CustomerId == customerId && ca.Status != EntityStatus.Deleted)
                        .ToListAsync();

                    // 2. 處理需要刪除的地址 (現有地址中不在新地址列表中的)
                    var addressesToDelete = existingAddresses
                        .Where(existing => !addresses.Any(addr => addr.AddressId == existing.AddressId))
                        .ToList();

                    foreach (var addressToDelete in addressesToDelete)
                    {
                        addressToDelete.Status = EntityStatus.Deleted;
                    }

                    // 3. 處理新增和更新的地址
                    foreach (var address in addresses)
                    {
                        address.CustomerId = customerId; // 確保 CustomerId 正確
                        
                        if (address.AddressId == 0)
                        {
                            // 新增地址
                            address.Status = EntityStatus.Active;
                            _context.CustomerAddresses.Add(address);
                        }
                        else
                        {
                            // 更新現有地址
                            var existingAddress = existingAddresses.FirstOrDefault(ea => ea.AddressId == address.AddressId);
                            if (existingAddress != null)
                            {
                                // 更新屬性
                                existingAddress.AddressTypeId = address.AddressTypeId;
                                existingAddress.PostalCode = address.PostalCode;
                                existingAddress.City = address.City;
                                existingAddress.District = address.District;
                                existingAddress.Address = address.Address;
                                existingAddress.IsPrimary = address.IsPrimary;
                                existingAddress.Status = address.Status;
                            }
                        }
                    }

                    // 4. 確保只有一個主要地址
                    var primaryAddresses = addresses.Where(a => a.IsPrimary).ToList();
                    if (primaryAddresses.Count > 1)
                    {
                        // 如果有多個主要地址，只保留第一個
                        for (int i = 1; i < primaryAddresses.Count; i++)
                        {
                            primaryAddresses[i].IsPrimary = false;
                        }
                    }
                    else if (primaryAddresses.Count == 0 && addresses.Any())
                    {
                        // 如果沒有主要地址，將第一個設為主要
                        addresses.First().IsPrimary = true;
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Successfully updated {AddressCount} addresses for customer {CustomerId}", 
                        addresses.Count, customerId);
                    return ServiceResult.Success();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
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
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(address);

            // 使用 DataAnnotations 驗證
            if (!Validator.TryValidateObject(address, validationContext, validationResults, true))
            {
                var errors = validationResults.Select(vr => vr.ErrorMessage ?? "驗證錯誤").ToList();
                return ServiceResult.ValidationFailure(errors);
            }

            // 業務規則驗證
            if (address.AddressTypeId.HasValue)
            {
                var addressTypeExists = await _context.AddressTypes
                    .AnyAsync(at => at.AddressTypeId == address.AddressTypeId && at.Status != EntityStatus.Deleted);
                if (!addressTypeExists)
                {
                    return ServiceResult.Failure("指定的地址類型不存在");
                }
            }

            return ServiceResult.Success();
        }

        public async Task<ServiceResult> EnsureCustomerHasPrimaryAddressAsync(int customerId)
        {
            try
            {
                var primaryAddress = await GetPrimaryAddressAsync(customerId);
                if (primaryAddress != null)
                    return ServiceResult.Success();

                // 如果沒有主要地址，將第一個有效地址設為主要
                var firstAddress = await _context.CustomerAddresses
                    .Where(ca => ca.CustomerId == customerId && ca.Status != EntityStatus.Deleted)
                    .FirstOrDefaultAsync();

                if (firstAddress != null)
                {
                    firstAddress.IsPrimary = true;
                    await _context.SaveChangesAsync();
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring customer {CustomerId} has primary address", customerId);
                return ServiceResult.Failure($"確保主要地址時發生錯誤: {ex.Message}");
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
                _logger.LogError(ex, "Error copying address for customer {CustomerId}", targetCustomerId);
                return ServiceResult<CustomerAddress>.Failure($"複製地址時發生錯誤: {ex.Message}");
            }
        }

        public async Task<int?> GetDefaultAddressTypeIdAsync(string addressTypeName)
        {
            try
            {
                var addressType = await _context.AddressTypes
                    .FirstOrDefaultAsync(at => at.TypeName.Contains(addressTypeName) && at.Status != EntityStatus.Deleted);
                return addressType?.AddressTypeId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting default address type for {AddressTypeName}", addressTypeName);
                return null;
            }
        }

        #endregion

        #region 私有方法

        private async Task ClearPrimaryAddressAsync(int customerId)
        {
            var primaryAddresses = await _context.CustomerAddresses
                .Where(ca => ca.CustomerId == customerId && ca.IsPrimary && ca.Status != EntityStatus.Deleted)
                .ToListAsync();

            foreach (var addr in primaryAddresses)
            {
                addr.IsPrimary = false;
            }
        }

        #endregion
    }
}
