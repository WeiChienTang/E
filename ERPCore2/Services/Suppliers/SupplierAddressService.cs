using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services.GenericManagementService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 廠商地址管理服務實作 - 繼承 GenericManagementService
    /// </summary>
    public class SupplierAddressService : GenericManagementService<SupplierAddress>, ISupplierAddressService
    {
        private readonly ILogger<SupplierAddressService> _logger;

        public SupplierAddressService(AppDbContext context, ILogger<SupplierAddressService> logger) : base(context)
        {
            _logger = logger;
        }

        #region 覆寫基底方法

        public override async Task<List<SupplierAddress>> GetAllAsync()
        {
            return await _dbSet
                .Include(sa => sa.Supplier)
                .Include(sa => sa.AddressType)
                .Where(sa => !sa.IsDeleted)
                .OrderBy(sa => sa.Supplier.CompanyName)
                .ThenBy(sa => sa.AddressType!.TypeName)
                .ToListAsync();
        }

        public override async Task<List<SupplierAddress>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            return await _dbSet
                .Include(sa => sa.Supplier)
                .Include(sa => sa.AddressType)
                .Where(sa => !sa.IsDeleted &&
                           ((sa.Address != null && sa.Address.Contains(searchTerm)) ||
                            (sa.City != null && sa.City.Contains(searchTerm)) ||
                            (sa.District != null && sa.District.Contains(searchTerm)) ||
                            (sa.PostalCode != null && sa.PostalCode.Contains(searchTerm)) ||
                            sa.Supplier.CompanyName.Contains(searchTerm)))
                .OrderBy(sa => sa.Supplier.CompanyName)
                .ThenBy(sa => sa.AddressType!.TypeName)
                .ToListAsync();
        }

        public override Task<ServiceResult> ValidateAsync(SupplierAddress entity)
        {
            var errors = new List<string>();

            // 驗證必填欄位
            if (entity.SupplierId <= 0)
            {
                errors.Add("廠商為必填欄位");
            }

            // 驗證地址資料長度
            if (!string.IsNullOrWhiteSpace(entity.PostalCode) && entity.PostalCode.Length > 10)
            {
                errors.Add("郵遞區號不可超過10個字元");
            }

            if (!string.IsNullOrWhiteSpace(entity.City) && entity.City.Length > 50)
            {
                errors.Add("城市不可超過50個字元");
            }

            if (!string.IsNullOrWhiteSpace(entity.District) && entity.District.Length > 50)
            {
                errors.Add("行政區不可超過50個字元");
            }

            if (!string.IsNullOrWhiteSpace(entity.Address) && entity.Address.Length > 200)
            {
                errors.Add("地址不可超過200個字元");
            }            return Task.FromResult(errors.Any() 
                ? ServiceResult.Failure(string.Join("; ", errors))
                : ServiceResult.Success());
        }

        #endregion

        #region 業務特定查詢方法

        public async Task<List<SupplierAddress>> GetBySupplierIdAsync(int supplierId)
        {
            try
            {
                return await _dbSet
                    .Include(sa => sa.AddressType)
                    .Where(sa => sa.SupplierId == supplierId && !sa.IsDeleted)
                    .OrderBy(sa => sa.AddressType!.TypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting supplier addresses for supplier {SupplierId}", supplierId);
                throw;
            }
        }

        public async Task<List<SupplierAddress>> GetByAddressTypeAsync(int addressTypeId)
        {
            try
            {
                return await _dbSet
                    .Include(sa => sa.Supplier)
                    .Include(sa => sa.AddressType)
                    .Where(sa => sa.AddressTypeId == addressTypeId && !sa.IsDeleted)
                    .OrderBy(sa => sa.Supplier.CompanyName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting supplier addresses by type {AddressTypeId}", addressTypeId);
                throw;
            }
        }

        public async Task<SupplierAddress?> GetPrimaryAddressAsync(int supplierId)
        {
            try
            {
                return await _dbSet
                    .Include(sa => sa.AddressType)
                    .FirstOrDefaultAsync(sa => sa.SupplierId == supplierId && sa.IsPrimary && !sa.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting primary address for supplier {SupplierId}", supplierId);
                throw;
            }
        }

        public async Task<SupplierAddress?> GetAddressByTypeAsync(int supplierId, int addressTypeId)
        {
            try
            {
                return await _dbSet
                    .Include(sa => sa.AddressType)
                    .FirstOrDefaultAsync(sa => sa.SupplierId == supplierId && 
                                              sa.AddressTypeId == addressTypeId && 
                                              !sa.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting address by type for supplier {SupplierId}, type {AddressTypeId}", 
                    supplierId, addressTypeId);
                throw;
            }
        }

        #endregion

        #region 業務邏輯操作

        public async Task<ServiceResult> SetPrimaryAddressAsync(int addressId)
        {
            try
            {
                var address = await GetByIdAsync(addressId);
                if (address == null)
                {
                    return ServiceResult.Failure("找不到指定的地址");
                }

                // 將同一廠商的其他地址設為非主要
                var otherAddresses = await _dbSet
                    .Where(sa => sa.SupplierId == address.SupplierId && 
                                sa.Id != addressId && 
                                sa.IsPrimary && 
                                !sa.IsDeleted)
                    .ToListAsync();

                foreach (var otherAddress in otherAddresses)
                {
                    otherAddress.IsPrimary = false;
                    otherAddress.UpdatedAt = DateTime.UtcNow;
                }

                // 設定指定地址為主要
                address.IsPrimary = true;
                address.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting primary address {AddressId}", addressId);
                return ServiceResult.Failure($"設定主要地址時發生錯誤: {ex.Message}");
            }
        }

        public async Task<ServiceResult<SupplierAddress>> CopyAddressToSupplierAsync(SupplierAddress sourceAddress, int targetSupplierId, int? targetAddressTypeId = null)
        {
            try
            {
                var newAddress = new SupplierAddress
                {
                    SupplierId = targetSupplierId,
                    AddressTypeId = targetAddressTypeId ?? sourceAddress.AddressTypeId,
                    PostalCode = sourceAddress.PostalCode,
                    City = sourceAddress.City,
                    District = sourceAddress.District,
                    Address = sourceAddress.Address,
                    IsPrimary = false, // 複製的地址預設不是主要地址
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var result = await CreateAsync(newAddress);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error copying address to supplier {TargetSupplierId}", targetSupplierId);
                return ServiceResult<SupplierAddress>.Failure($"複製地址時發生錯誤: {ex.Message}");
            }
        }

        public async Task<ServiceResult> EnsureSupplierHasPrimaryAddressAsync(int supplierId)
        {
            try
            {
                var hasPrimaryAddress = await _dbSet
                    .AnyAsync(sa => sa.SupplierId == supplierId && sa.IsPrimary && !sa.IsDeleted);

                if (!hasPrimaryAddress)
                {
                    // 找第一個可用的地址設為主要
                    var firstAddress = await _dbSet
                        .Where(sa => sa.SupplierId == supplierId && !sa.IsDeleted)
                        .OrderBy(sa => sa.Id)
                        .FirstOrDefaultAsync();

                    if (firstAddress != null)
                    {
                        firstAddress.IsPrimary = true;
                        firstAddress.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring supplier has primary address {SupplierId}", supplierId);
                return ServiceResult.Failure($"確保主要地址時發生錯誤: {ex.Message}");
            }
        }

        public async Task<List<SupplierAddress>> GetAddressesWithDefaultAsync(int supplierId, List<AddressType> addressTypes)
        {
            try
            {
                var addresses = await GetBySupplierIdAsync(supplierId);
                
                // 如果沒有地址，建立預設的地址
                if (!addresses.Any())
                {
                    InitializeDefaultAddresses(addresses, addressTypes);
                }

                return addresses.OrderBy(a => a.AddressType?.TypeName).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting addresses with default for supplier {SupplierId}", supplierId);
                throw;
            }
        }

        public async Task<ServiceResult> UpdateSupplierAddressesAsync(int supplierId, List<SupplierAddress> addresses)
        {
            try
            {
                // 取得現有地址
                var existingAddresses = await _dbSet
                    .Where(sa => sa.SupplierId == supplierId && !sa.IsDeleted)
                    .ToListAsync();

                // 刪除不在新列表中的地址
                var addressesToDelete = existingAddresses
                    .Where(ea => !addresses.Any(a => a.Id == ea.Id))
                    .ToList();

                foreach (var address in addressesToDelete)
                {
                    address.IsDeleted = true;
                    address.UpdatedAt = DateTime.UtcNow;
                }

                // 更新或新增地址
                foreach (var address in addresses)
                {
                    address.SupplierId = supplierId;
                    
                    if (address.Id == 0)
                    {
                        // 新增
                        address.CreatedAt = DateTime.UtcNow;
                        address.UpdatedAt = DateTime.UtcNow;
                        _dbSet.Add(address);
                    }
                    else
                    {
                        // 更新
                        var existingAddress = existingAddresses.FirstOrDefault(ea => ea.Id == address.Id);
                        if (existingAddress != null)
                        {
                            existingAddress.AddressTypeId = address.AddressTypeId;
                            existingAddress.PostalCode = address.PostalCode;
                            existingAddress.City = address.City;
                            existingAddress.District = address.District;
                            existingAddress.Address = address.Address;
                            existingAddress.IsPrimary = address.IsPrimary;
                            existingAddress.UpdatedAt = DateTime.UtcNow;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating supplier addresses for supplier {SupplierId}", supplierId);
                return ServiceResult.Failure($"更新廠商地址時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 記憶體操作方法（用於UI編輯）

        public SupplierAddress CreateNewAddress(int supplierId, int addressCount)
        {
            return new SupplierAddress
            {
                SupplierId = supplierId,
                AddressTypeId = null,
                PostalCode = string.Empty,
                City = string.Empty,
                District = string.Empty,
                Address = string.Empty,
                IsPrimary = addressCount == 0, // 第一個地址預設為主要
                Status = EntityStatus.Active
            };
        }

        public void InitializeDefaultAddresses(List<SupplierAddress> addressList, List<AddressType> addressTypes)
        {
            // 確保有基本的地址類型
            var defaultTypes = new[] { "營業地址", "聯絡地址", "發票地址" };

            foreach (var typeName in defaultTypes)
            {
                var addressType = addressTypes.FirstOrDefault(at => at.TypeName == typeName);
                if (addressType != null && !addressList.Any(a => a.AddressTypeId == addressType.Id))
                {
                    var newAddress = new SupplierAddress
                    {
                        AddressTypeId = addressType.Id,
                        PostalCode = string.Empty,
                        City = string.Empty,
                        District = string.Empty,
                        Address = string.Empty,
                        IsPrimary = addressList.Count == 0,
                        Status = EntityStatus.Active
                    };
                    addressList.Add(newAddress);
                }
            }
        }

        public int? GetDefaultAddressTypeId(int addressCount, List<AddressType> addressTypes)
        {
            // 根據地址數量決定預設的地址類型
            var defaultTypes = new[] { "營業地址", "聯絡地址", "發票地址" };
            
            if (addressCount < defaultTypes.Length)
            {
                var typeName = defaultTypes[addressCount];
                var addressType = addressTypes.FirstOrDefault(at => at.TypeName == typeName);
                return addressType?.Id;
            }
            
            // 如果超出預設類型，返回第一個可用的類型
            return addressTypes.FirstOrDefault()?.Id;
        }

        public ServiceResult EnsurePrimaryAddressExists(List<SupplierAddress> addresses)
        {
            if (!addresses.Any(a => a.IsPrimary))
            {
                var firstAddress = addresses.FirstOrDefault();
                if (firstAddress != null)
                {
                    firstAddress.IsPrimary = true;
                }
            }

            return ServiceResult.Success();
        }

        public int GetCompletedAddressCount(List<SupplierAddress> addresses)
        {
            if (addresses == null)
                return 0;

            return addresses.Count(a =>
                !string.IsNullOrWhiteSpace(a.Address) &&
                !string.IsNullOrWhiteSpace(a.City));
        }

        public int GetAddressCompletedFieldsCount(List<SupplierAddress> addresses)
        {
            if (addresses == null)
                return 0;

            int totalFields = 0;
            int completedFields = 0;

            foreach (var address in addresses)
            {
                totalFields += 4; // AddressTypeId, City, District, Address

                if (address.AddressTypeId.HasValue)
                    completedFields++;

                if (!string.IsNullOrWhiteSpace(address.City))
                    completedFields++;

                if (!string.IsNullOrWhiteSpace(address.District))
                    completedFields++;

                if (!string.IsNullOrWhiteSpace(address.Address))
                    completedFields++;
            }

            return completedFields;
        }

        #endregion
    }
}
