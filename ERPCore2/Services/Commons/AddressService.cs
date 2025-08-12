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
                return await _context.Addresses
                    .Where(a => a.OwnerType == ownerType && a.OwnerId == ownerId && !a.IsDeleted)
                    .Include(a => a.AddressType)
                    .OrderByDescending(a => a.IsPrimary)
                    .ThenBy(a => a.CreatedAt)
                    .ToListAsync();
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
                    .FirstOrDefaultAsync(a => a.Id == addressId && !a.IsDeleted);
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
                return await _context.Addresses
                    .Include(a => a.AddressType)
                    .FirstOrDefaultAsync(a => 
                        a.OwnerType == ownerType && 
                        a.OwnerId == ownerId && 
                        a.IsPrimary && 
                        !a.IsDeleted);
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

                // 如果這是第一個地址，自動設為主要地址
                var existingAddresses = await _context.Addresses
                    .Where(a => a.OwnerType == ownerType && a.OwnerId == ownerId && !a.IsDeleted)
                    .CountAsync();

                if (existingAddresses == 0)
                {
                    address.IsPrimary = true;
                }
                else if (address.IsPrimary)
                {
                    // 如果設為主要地址，先將其他地址設為非主要
                    await SetAllAddressesAsNonPrimaryAsync(ownerType, ownerId);
                }

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
                if (existingAddress == null || existingAddress.IsDeleted)
                {
                    throw new ArgumentException($"地址不存在: ID {address.Id}");
                }

                // 更新欄位
                existingAddress.AddressTypeId = address.AddressTypeId;
                existingAddress.PostalCode = address.PostalCode;
                existingAddress.City = address.City;
                existingAddress.District = address.District;
                existingAddress.AddressLine = address.AddressLine;
                existingAddress.Remarks = address.Remarks;
                existingAddress.UpdatedAt = DateTime.Now;

                // 處理主要地址設定
                if (address.IsPrimary && !existingAddress.IsPrimary)
                {
                    await SetAllAddressesAsNonPrimaryAsync(existingAddress.OwnerType, existingAddress.OwnerId);
                    existingAddress.IsPrimary = true;
                }
                else if (!address.IsPrimary && existingAddress.IsPrimary)
                {
                    existingAddress.IsPrimary = false;
                }

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
                if (address == null || address.IsDeleted)
                {
                    throw new ArgumentException($"地址不存在: ID {addressId}");
                }

                // 軟刪除
                address.IsDeleted = true;
                address.UpdatedAt = DateTime.Now;

                // 如果刪除的是主要地址，設定其他地址為主要地址
                if (address.IsPrimary)
                {
                    var otherAddress = await _context.Addresses
                        .Where(a => a.OwnerType == address.OwnerType && 
                                  a.OwnerId == address.OwnerId && 
                                  a.Id != addressId && 
                                  !a.IsDeleted)
                        .FirstOrDefaultAsync();

                    if (otherAddress != null)
                    {
                        otherAddress.IsPrimary = true;
                    }
                }

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
                // 先將所有地址設為非主要
                await SetAllAddressesAsNonPrimaryAsync(ownerType, ownerId);

                // 設定指定地址為主要
                var targetAddress = await _context.Addresses
                    .FirstOrDefaultAsync(a => a.Id == addressId && 
                                           a.OwnerType == ownerType && 
                                           a.OwnerId == ownerId && 
                                           !a.IsDeleted);

                if (targetAddress != null)
                {
                    targetAddress.IsPrimary = true;
                    targetAddress.UpdatedAt = DateTime.Now;
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("成功設定 {OwnerType} ID {OwnerId} 的主要地址為 ID {AddressId}", 
                        ownerType, ownerId, addressId);
                }
                else
                {
                    throw new ArgumentException($"地址不存在或已刪除: ID {addressId}");
                }
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
                AddressOwnerTypes.Customer => await _context.Customers.AnyAsync(c => c.Id == ownerId && !c.IsDeleted),
                AddressOwnerTypes.Supplier => await _context.Suppliers.AnyAsync(s => s.Id == ownerId && !s.IsDeleted),
                AddressOwnerTypes.Employee => await _context.Employees.AnyAsync(e => e.Id == ownerId && !e.IsDeleted),
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

        private async Task SetAllAddressesAsNonPrimaryAsync(string ownerType, int ownerId)
        {
            var addresses = await _context.Addresses
                .Where(a => a.OwnerType == ownerType && a.OwnerId == ownerId && !a.IsDeleted)
                .ToListAsync();

            foreach (var addr in addresses)
            {
                addr.IsPrimary = false;
            }
        }

        #endregion
    }
}
