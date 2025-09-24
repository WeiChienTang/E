using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 統一地址服務實作 - 管理所有實體的地址資訊
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

        #region 查詢方法

        public async Task<List<Address>> GetAddressesByOwnerAsync(string ownerType, int ownerId)
        {
            try
            {
                // 偵錯：先查詢所有相關記錄
                var result = await _context.Addresses
                    .Where(a => a.OwnerType == ownerType && a.OwnerId == ownerId)
                    .Include(a => a.AddressType)
                    .OrderBy(a => a.CreatedAt)
                    .ToListAsync();
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得 {OwnerType} ID {OwnerId} 的地址時發生錯誤", ownerType, ownerId);
                throw;
            }
        }

        public async Task<Address?> GetAddressByIdAsync(int addressId)
        {
            try
            {
                return await _context.Addresses
                    .Include(a => a.AddressType)
                    .FirstOrDefaultAsync(a => a.Id == addressId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得地址 ID {AddressId} 時發生錯誤", addressId);
                throw;
            }
        }

        public async Task<Address?> GetPrimaryAddressAsync(string ownerType, int ownerId)
        {
            try
            {
                // 由於移除了 IsPrimary 屬性，回傳第一個找到的地址
                return await _context.Addresses
                    .Include(a => a.AddressType)
                    .FirstOrDefaultAsync(a => 
                        a.OwnerType == ownerType && 
                        a.OwnerId == ownerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得 {OwnerType} ID {OwnerId} 的主要地址時發生錯誤", ownerType, ownerId);
                throw;
            }
        }

        #endregion

        #region 特定實體方法

        public async Task<List<Address>> GetCustomerAddressesAsync(int customerId)
        {
            return await GetAddressesByOwnerAsync(AddressOwnerTypes.Customer, customerId);
        }

        public async Task<Address?> GetCustomerPrimaryAddressAsync(int customerId)
        {
            return await GetPrimaryAddressAsync(AddressOwnerTypes.Customer, customerId);
        }

        public async Task<List<Address>> GetSupplierAddressesAsync(int supplierId)
        {
            return await GetAddressesByOwnerAsync(AddressOwnerTypes.Supplier, supplierId);
        }

        public async Task<Address?> GetSupplierPrimaryAddressAsync(int supplierId)
        {
            return await GetPrimaryAddressAsync(AddressOwnerTypes.Supplier, supplierId);
        }

        public async Task<List<Address>> GetEmployeeAddressesAsync(int employeeId)
        {
            return await GetAddressesByOwnerAsync(AddressOwnerTypes.Employee, employeeId);
        }

        public async Task<Address?> GetEmployeePrimaryAddressAsync(int employeeId)
        {
            return await GetPrimaryAddressAsync(AddressOwnerTypes.Employee, employeeId);
        }

        #endregion

        #region 修改方法

        public async Task<Address> CreateAddressAsync(string ownerType, int ownerId, Address address)
        {
            try
            {
                if (!IsValidOwnerType(ownerType))
                {
                    throw new ArgumentException($"無效的擁有者類型: {ownerType}");
                }

                if (!await ValidateOwnerExistsAsync(ownerType, ownerId))
                {
                    throw new ArgumentException($"擁有者不存在: {ownerType} ID {ownerId}");
                }

                address.OwnerType = ownerType;
                address.OwnerId = ownerId;
                address.CreatedAt = DateTime.Now;
                address.Status = EntityStatus.Active;

                _context.Addresses.Add(address);
                await _context.SaveChangesAsync();

                _logger.LogInformation("成功建立 {OwnerType} ID {OwnerId} 的地址 ID {AddressId}", 
                    ownerType, ownerId, address.Id);

                return address;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立 {OwnerType} ID {OwnerId} 的地址時發生錯誤", ownerType, ownerId);
                throw;
            }
        }

        public async Task<Address> UpdateAddressAsync(Address address)
        {
            try
            {
                var existingAddress = await _context.Addresses.FindAsync(address.Id);
                if (existingAddress == null)
                {
                    throw new ArgumentException($"地址不存在: ID {address.Id}");
                }

                // 更新欄位
                existingAddress.AddressTypeId = address.AddressTypeId;
                existingAddress.AddressLine = address.AddressLine;
                existingAddress.Remarks = address.Remarks;
                existingAddress.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation("成功更新地址 ID {AddressId}", address.Id);

                return existingAddress;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新地址 ID {AddressId} 時發生錯誤", address.Id);
                throw;
            }
        }

        public async Task DeleteAddressAsync(int addressId)
        {
            try
            {
                var address = await _context.Addresses.FindAsync(addressId);
                if (address == null)
                {
                    throw new ArgumentException($"地址不存在: ID {addressId}");
                }

                // 檢查是否有其他資料依賴此地址
                // 由於地址是被其他實體擁有的，而不是被引用的，所以通常可以直接刪除
                // 但為了安全起見，可以添加額外的檢查

                // 硬刪除
                _context.Addresses.Remove(address);

                await _context.SaveChangesAsync();

                _logger.LogInformation("成功刪除地址 ID {AddressId}", addressId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除地址 ID {AddressId} 時發生錯誤", addressId);
                throw;
            }
        }

        public async Task SetPrimaryAddressAsync(string ownerType, int ownerId, int addressId)
        {
            try
            {
                // 由於移除了 IsPrimary 屬性，此方法保持為空實作以維持相容性
                // 或者可以添加日誌記錄這個已不再使用的方法被呼叫
                _logger.LogWarning("SetPrimaryAddressAsync 已不再使用，因為 IsPrimary 屬性已被移除");
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "設定主要地址時發生錯誤: {OwnerType} ID {OwnerId}, 地址 ID {AddressId}", 
                    ownerType, ownerId, addressId);
                throw;
            }
        }

        #endregion

        #region 驗證方法

        public async Task<bool> ValidateOwnerExistsAsync(string ownerType, int ownerId)
        {
            return ownerType switch
            {
                AddressOwnerTypes.Customer => await _context.Customers.AnyAsync(c => c.Id == ownerId),
                AddressOwnerTypes.Supplier => await _context.Suppliers.AnyAsync(s => s.Id == ownerId),
                AddressOwnerTypes.Employee => await _context.Employees.AnyAsync(e => e.Id == ownerId),
                _ => false
            };
        }

        public bool IsValidOwnerType(string ownerType)
        {
            return ownerType == AddressOwnerTypes.Customer ||
                   ownerType == AddressOwnerTypes.Supplier ||
                   ownerType == AddressOwnerTypes.Employee;
        }

        #endregion

        #region 私有方法

        // 保留方法以維持相容性，但已不再使用
        private async Task SetAllAddressesAsNonPrimaryAsync(string ownerType, int ownerId)
        {
            // 由於 IsPrimary 屬性已移除，此方法不再執行任何操作
            await Task.CompletedTask;
        }

        #endregion
    }
}

