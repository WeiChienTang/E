using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;
using ERPCore2.Services.GenericManagementService;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace ERPCore2.Services
{
    /// <summary>
    /// 客戶地址管理服務實作 - 繼承 GenericManagementService
    /// </summary>
    public class CustomerAddressService : GenericManagementService<CustomerAddress>, ICustomerAddressService
    {
        public CustomerAddressService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<CustomerAddress>> logger) : base(contextFactory, logger)
        {
        }

        public CustomerAddressService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        #region 覆寫基底方法

        public override async Task<List<CustomerAddress>> GetAllAsync()
        {
            try
            {
                return await _dbSet
                    .Include(ca => ca.Customer)
                    .Include(ca => ca.AddressType)
                    .Where(ca => !ca.IsDeleted)
                    .OrderBy(ca => ca.Customer.CompanyName)
                    .ThenBy(ca => ca.AddressType!.TypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger);
                return new List<CustomerAddress>();
            }
        }

        public override async Task<List<CustomerAddress>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                return await _dbSet
                    .Include(ca => ca.Customer)
                    .Include(ca => ca.AddressType)
                    .Where(ca => !ca.IsDeleted &&
                               ((ca.Address != null && ca.Address.Contains(searchTerm)) ||
                                (ca.City != null && ca.City.Contains(searchTerm)) ||
                                (ca.District != null && ca.District.Contains(searchTerm)) ||
                                (ca.PostalCode != null && ca.PostalCode.Contains(searchTerm)) ||
                                ca.Customer.CompanyName.Contains(searchTerm)))
                    .OrderBy(ca => ca.Customer.CompanyName)
                    .ThenBy(ca => ca.AddressType!.TypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger);
                return new List<CustomerAddress>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(CustomerAddress entity)
        {
            try
            {
                var errors = new List<string>();

                // 檢查必要欄位
                if (entity.CustomerId <= 0)
                    errors.Add("客戶ID必須大於0");

                // 檢查地址長度限制
                if (!string.IsNullOrEmpty(entity.PostalCode) && entity.PostalCode.Length > 10)
                    errors.Add("郵遞區號不可超過10個字元");

                if (!string.IsNullOrEmpty(entity.City) && entity.City.Length > 50)
                    errors.Add("城市不可超過50個字元");

                if (!string.IsNullOrEmpty(entity.District) && entity.District.Length > 50)
                    errors.Add("行政區不可超過50個字元");

                if (!string.IsNullOrEmpty(entity.Address) && entity.Address.Length > 200)
                    errors.Add("地址不可超過200個字元");

                // 檢查地址類型是否存在
                if (entity.AddressTypeId.HasValue)
                {
                    var addressTypeExists = await _context.AddressTypes
                        .AnyAsync(at => at.Id == entity.AddressTypeId.Value && at.Status == EntityStatus.Active);

                    if (!addressTypeExists)
                        errors.Add("指定的地址類型不存在或已停用");
                }

                // 檢查客戶是否存在
                var customerExists = await _context.Customers
                    .AnyAsync(c => c.Id == entity.CustomerId && !c.IsDeleted);

                if (!customerExists)
                    errors.Add("指定的客戶不存在");

                if (errors.Any())
                    return ServiceResult.Failure(string.Join("; ", errors));

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger);
                return ServiceResult.Failure("驗證客戶地址時發生錯誤");
            }
        }

        #endregion

        #region 業務特定查詢方法

        public async Task<List<CustomerAddress>> GetByCustomerIdAsync(int customerId)
        {
            try
            {
                return await _dbSet
                    .Include(ca => ca.AddressType)
                    .Where(ca => ca.CustomerId == customerId && !ca.IsDeleted)
                    .OrderBy(ca => ca.AddressType!.TypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByCustomerIdAsync), GetType(), _logger);
                return new List<CustomerAddress>();
            }
        }

        public async Task<CustomerAddress?> GetPrimaryAddressAsync(int customerId)
        {
            try
            {
                return await _dbSet
                    .Include(ca => ca.AddressType)
                    .FirstOrDefaultAsync(ca => ca.CustomerId == customerId && ca.IsPrimary && !ca.IsDeleted);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPrimaryAddressAsync), GetType(), _logger);
                return null;
            }
        }

        public async Task<List<CustomerAddress>> GetByAddressTypeAsync(int addressTypeId)
        {
            try
            {
                return await _dbSet
                    .Include(ca => ca.Customer)
                    .Include(ca => ca.AddressType)
                    .Where(ca => ca.AddressTypeId == addressTypeId && !ca.IsDeleted)
                    .OrderBy(ca => ca.Customer.CompanyName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByAddressTypeAsync), GetType(), _logger);
                return new List<CustomerAddress>();
            }
        }

        #endregion

        #region 業務邏輯操作

        public async Task<ServiceResult> SetPrimaryAddressAsync(int addressId)
        {
            try
            {
                var address = await _dbSet
                    .FirstOrDefaultAsync(ca => ca.Id == addressId && !ca.IsDeleted);

                if (address == null)
                    return ServiceResult.Failure("地址不存在");

                // 將該客戶的其他地址設為非主要
                var otherAddresses = await _dbSet
                    .Where(ca => ca.CustomerId == address.CustomerId && ca.Id != addressId && !ca.IsDeleted)
                    .ToListAsync();

                foreach (var otherAddress in otherAddresses)
                {
                    otherAddress.IsPrimary = false;
                    otherAddress.UpdatedAt = DateTime.UtcNow;
                    otherAddress.UpdatedBy = "System"; // TODO: 從認證取得使用者
                }

                // 設定為主要地址
                address.IsPrimary = true;
                address.UpdatedAt = DateTime.UtcNow;
                address.UpdatedBy = "System"; // TODO: 從認證取得使用者

                await _context.SaveChangesAsync();

                _logger?.LogInformation("Successfully set primary address {AddressId} for customer {CustomerId}",
                    addressId, address.CustomerId);

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SetPrimaryAddressAsync), GetType(), _logger);
                return ServiceResult.Failure("設定主要地址時發生錯誤");
            }
        }

        public async Task<ServiceResult<CustomerAddress>> CopyAddressToCustomerAsync(CustomerAddress sourceAddress, int targetCustomerId, int? targetAddressTypeId = null)
        {
            try
            {
                // 檢查目標客戶是否存在
                var targetCustomerExists = await _context.Customers
                    .AnyAsync(c => c.Id == targetCustomerId && !c.IsDeleted);

                if (!targetCustomerExists)
                    return ServiceResult<CustomerAddress>.Failure("目標客戶不存在");

                // 創建新地址
                var newAddress = new CustomerAddress
                {
                    CustomerId = targetCustomerId,
                    AddressTypeId = targetAddressTypeId ?? sourceAddress.AddressTypeId,
                    PostalCode = sourceAddress.PostalCode,
                    City = sourceAddress.City,
                    District = sourceAddress.District,
                    Address = sourceAddress.Address,
                    IsPrimary = false, // 預設不是主要地址
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System" // TODO: 從認證取得使用者
                };

                // 驗證新地址
                var validationResult = await ValidateAsync(newAddress);
                if (!validationResult.IsSuccess)
                    return ServiceResult<CustomerAddress>.Failure(validationResult.ErrorMessage!);

                _dbSet.Add(newAddress);
                await _context.SaveChangesAsync();

                _logger?.LogInformation("Successfully copied address from customer {SourceCustomerId} to customer {TargetCustomerId}",
                    sourceAddress.CustomerId, targetCustomerId);

                return ServiceResult<CustomerAddress>.Success(newAddress);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CopyAddressToCustomerAsync), GetType(), _logger);
                return ServiceResult<CustomerAddress>.Failure("複製地址時發生錯誤");
            }
        }

        public async Task<ServiceResult> EnsureCustomerHasPrimaryAddressAsync(int customerId)
        {
            try
            {
                // 檢查是否已有主要地址
                var hasPrimaryAddress = await _dbSet
                    .AnyAsync(ca => ca.CustomerId == customerId && ca.IsPrimary && !ca.IsDeleted);

                if (hasPrimaryAddress)
                    return ServiceResult.Success();

                // 如果沒有主要地址，將第一個地址設為主要
                var firstAddress = await _dbSet
                    .Where(ca => ca.CustomerId == customerId && !ca.IsDeleted)
                    .OrderBy(ca => ca.CreatedAt)
                    .FirstOrDefaultAsync();

                if (firstAddress != null)
                {
                    firstAddress.IsPrimary = true;
                    firstAddress.UpdatedAt = DateTime.UtcNow;
                    firstAddress.UpdatedBy = "System"; // TODO: 從認證取得使用者

                    await _context.SaveChangesAsync();

                    _logger?.LogInformation("Set first address {AddressId} as primary for customer {CustomerId}",
                        firstAddress.Id, customerId);
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(EnsureCustomerHasPrimaryAddressAsync), GetType(), _logger);
                return ServiceResult.Failure("確保客戶有主要地址時發生錯誤");
            }
        }

        public async Task<List<CustomerAddress>> GetAddressesWithDefaultAsync(int customerId, List<AddressType> addressTypes)
        {
            try
            {
                // 取得現有的客戶地址
                var existingAddresses = await GetByCustomerIdAsync(customerId);
                
                // 如果沒有地址，初始化預設地址
                if (!existingAddresses.Any())
                {
                    var defaultAddresses = new List<CustomerAddress>();
                    InitializeDefaultAddresses(defaultAddresses, addressTypes);
                    
                    // 設定客戶ID
                    foreach (var address in defaultAddresses)
                    {
                        address.CustomerId = customerId;
                    }
                    
                    return defaultAddresses;
                }
                
                return existingAddresses;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAddressesWithDefaultAsync), GetType(), _logger);
                return new List<CustomerAddress>();
            }
        }

        public async Task<ServiceResult> UpdateCustomerAddressesAsync(int customerId, List<CustomerAddress> addresses)
        {
            try
            {
                // 驗證客戶是否存在
                var customerExists = await _context.Customers
                    .AnyAsync(c => c.Id == customerId && !c.IsDeleted);
                
                if (!customerExists)
                    return ServiceResult.Failure("客戶不存在");

                // 取得現有地址
                var existingAddresses = await _dbSet
                    .Where(ca => ca.CustomerId == customerId)
                    .ToListAsync();

                // 刪除現有地址
                _context.CustomerAddresses.RemoveRange(existingAddresses);
                
                // 新增更新的地址
                foreach (var address in addresses.Where(a => !string.IsNullOrWhiteSpace(a.Address) || 
                                                            !string.IsNullOrWhiteSpace(a.City)))
                {
                    // 建立新的地址實體以避免 ID 衝突
                    var newAddress = new CustomerAddress
                    {
                        CustomerId = customerId,
                        AddressTypeId = address.AddressTypeId,
                        PostalCode = address.PostalCode,
                        City = address.City,
                        District = address.District,
                        Address = address.Address,
                        IsPrimary = address.IsPrimary,
                        Status = EntityStatus.Active,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "System", // TODO: 從認證取得使用者
                        Remarks = address.Remarks
                    };
                    _context.CustomerAddresses.Add(newAddress);
                }

                await _context.SaveChangesAsync();

                _logger?.LogInformation("Successfully updated addresses for customer {CustomerId}", customerId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateCustomerAddressesAsync), GetType(), _logger);
                return ServiceResult.Failure("更新客戶地址時發生錯誤");
            }
        }

        #endregion

        #region 記憶體操作方法（用於UI編輯）

        public CustomerAddress CreateNewAddress(int customerId, int addressCount)
        {
            try
            {
                return new CustomerAddress
                {
                    CustomerId = customerId,
                    AddressTypeId = null,
                    PostalCode = string.Empty,
                    City = string.Empty,
                    District = string.Empty,
                    Address = string.Empty,
                    IsPrimary = addressCount == 0, // 第一個地址預設為主要
                    Status = EntityStatus.Active
                };
            }
            catch (Exception ex)
            {
                // 使用統一的錯誤處理
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(CreateNewAddress), GetType(), _logger, 
                    new { CustomerId = customerId });
                
                // 返回預設地址
                return new CustomerAddress
                {
                    CustomerId = customerId,
                    AddressTypeId = null,
                    PostalCode = string.Empty,
                    City = string.Empty,
                    District = string.Empty,
                    Address = string.Empty,
                    IsPrimary = true,
                    Status = EntityStatus.Active
                };
            }
        }

        public void InitializeDefaultAddresses(List<CustomerAddress> addressList, List<AddressType> addressTypes)
        {
            try
            {
                // 確保有基本的地址類型
                var defaultTypes = new[] { "營業地址", "聯絡地址", "發票地址" };

                foreach (var typeName in defaultTypes)
                {
                    var addressType = addressTypes.FirstOrDefault(at => at.TypeName == typeName);
                    if (addressType != null && !addressList.Any(a => a.AddressTypeId == addressType.Id))
                    {
                        var newAddress = new CustomerAddress
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
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(InitializeDefaultAddresses), GetType(), _logger);
            }
        }

        public int? GetDefaultAddressTypeId(int addressCount, List<AddressType> addressTypes)
        {
            try
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
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(GetDefaultAddressTypeId), GetType(), _logger, 
                    new { AddressCount = addressCount });
                return null;
            }
        }

        public ServiceResult AddAddress(List<CustomerAddress> addressList, CustomerAddress newAddress)
        {
            try
            {
                // 如果是第一個地址，自動設為主要
                if (addressList.Count == 0)
                    newAddress.IsPrimary = true;

                addressList.Add(newAddress);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(AddAddress), GetType(), _logger);
                return ServiceResult.Failure("新增地址時發生錯誤");
            }
        }

        public ServiceResult RemoveAddress(List<CustomerAddress> addressList, int index)
        {
            try
            {
                if (index < 0 || index >= addressList.Count)
                    return ServiceResult.Failure("索引超出範圍");

                var addressToRemove = addressList[index];
                addressList.RemoveAt(index);

                // 如果移除的是主要地址且還有其他地址，將第一個設為主要
                if (addressToRemove.IsPrimary && addressList.Count > 0)
                {
                    addressList[0].IsPrimary = true;
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(RemoveAddress), GetType(), _logger, 
                    new { Index = index });
                return ServiceResult.Failure("移除地址時發生錯誤");
            }
        }

        public ServiceResult SetPrimaryAddress(List<CustomerAddress> addressList, int index)
        {
            try
            {
                if (index < 0 || index >= addressList.Count)
                    return ServiceResult.Failure("索引超出範圍");

                // 將所有地址設為非主要
                foreach (var address in addressList)
                {
                    address.IsPrimary = false;
                }

                // 設定指定的地址為主要
                addressList[index].IsPrimary = true;

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(SetPrimaryAddress), GetType(), _logger, 
                    new { Index = index });
                return ServiceResult.Failure("設定主要地址時發生錯誤");
            }
        }

        public ServiceResult CopyAddressFromFirst(List<CustomerAddress> addressList, int targetIndex)
        {
            try
            {
                if (addressList.Count == 0)
                    return ServiceResult.Failure("沒有可複製的地址");

                if (targetIndex < 0 || targetIndex >= addressList.Count)
                    return ServiceResult.Failure("目標索引超出範圍");

                var sourceAddress = addressList[0];
                var targetAddress = addressList[targetIndex];

                // 複製地址資訊（除了主要地址標記）
                targetAddress.PostalCode = sourceAddress.PostalCode;
                targetAddress.City = sourceAddress.City;
                targetAddress.District = sourceAddress.District;
                targetAddress.Address = sourceAddress.Address;

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(CopyAddressFromFirst), GetType(), _logger, 
                    new { TargetIndex = targetIndex });
                return ServiceResult.Failure("複製地址時發生錯誤");
            }
        }

        #endregion

        #region 記憶體欄位更新方法

        public ServiceResult UpdateAddressType(List<CustomerAddress> addressList, int index, int? addressTypeId)
        {
            try
            {
                if (index < 0 || index >= addressList.Count)
                    return ServiceResult.Failure("索引超出範圍");

                addressList[index].AddressTypeId = addressTypeId;
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(UpdateAddressType), GetType(), _logger, 
                    new { Index = index });
                return ServiceResult.Failure("更新地址類型時發生錯誤");
            }
        }

        public ServiceResult UpdatePostalCode(List<CustomerAddress> addressList, int index, string? postalCode)
        {
            try
            {
                if (index < 0 || index >= addressList.Count)
                    return ServiceResult.Failure("索引超出範圍");

                if (!string.IsNullOrEmpty(postalCode) && postalCode.Length > 10)
                    return ServiceResult.Failure("郵遞區號不可超過10個字元");

                addressList[index].PostalCode = postalCode;
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(UpdatePostalCode), GetType(), _logger, 
                    new { Index = index });
                return ServiceResult.Failure("更新郵遞區號時發生錯誤");
            }
        }

        public ServiceResult UpdateCity(List<CustomerAddress> addressList, int index, string? city)
        {
            try
            {
                if (index < 0 || index >= addressList.Count)
                    return ServiceResult.Failure("索引超出範圍");

                if (!string.IsNullOrEmpty(city) && city.Length > 50)
                    return ServiceResult.Failure("城市不可超過50個字元");

                addressList[index].City = city;
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(UpdateCity), GetType(), _logger, 
                    new { Index = index });
                return ServiceResult.Failure("更新城市時發生錯誤");
            }
        }

        public ServiceResult UpdateDistrict(List<CustomerAddress> addressList, int index, string? district)
        {
            try
            {
                if (index < 0 || index >= addressList.Count)
                    return ServiceResult.Failure("索引超出範圍");

                if (!string.IsNullOrEmpty(district) && district.Length > 50)
                    return ServiceResult.Failure("行政區不可超過50個字元");

                addressList[index].District = district;
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(UpdateDistrict), GetType(), _logger, 
                    new { Index = index });
                return ServiceResult.Failure("更新行政區時發生錯誤");
            }
        }

        public ServiceResult UpdateAddress(List<CustomerAddress> addressList, int index, string? address)
        {
            try
            {
                if (index < 0 || index >= addressList.Count)
                    return ServiceResult.Failure("索引超出範圍");

                if (!string.IsNullOrEmpty(address) && address.Length > 200)
                    return ServiceResult.Failure("地址不可超過200個字元");

                addressList[index].Address = address;
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(UpdateAddress), GetType(), _logger, 
                    new { Index = index });
                return ServiceResult.Failure("更新地址時發生錯誤");
            }
        }

        #endregion

        #region 驗證和輔助方法

        public ServiceResult ValidateAddressList(List<CustomerAddress> addresses)
        {
            try
            {
                var errors = new List<string>();

                if (addresses == null || addresses.Count == 0)
                {
                    errors.Add("至少需要一個地址");
                    return ServiceResult.Failure(string.Join("; ", errors));
                }

                // 檢查是否有主要地址
                var primaryCount = addresses.Count(a => a.IsPrimary);
                if (primaryCount == 0)
                    errors.Add("必須指定一個主要地址");
                else if (primaryCount > 1)
                    errors.Add("只能有一個主要地址");

                // 驗證每個地址
                for (int i = 0; i < addresses.Count; i++)
                {
                    var address = addresses[i];

                    if (!string.IsNullOrEmpty(address.PostalCode) && address.PostalCode.Length > 10)
                        errors.Add($"第{i + 1}個地址的郵遞區號不可超過10個字元");

                    if (!string.IsNullOrEmpty(address.City) && address.City.Length > 50)
                        errors.Add($"第{i + 1}個地址的城市不可超過50個字元");

                    if (!string.IsNullOrEmpty(address.District) && address.District.Length > 50)
                        errors.Add($"第{i + 1}個地址的行政區不可超過50個字元");

                    if (!string.IsNullOrEmpty(address.Address) && address.Address.Length > 200)
                        errors.Add($"第{i + 1}個地址的地址不可超過200個字元");
                }

                if (errors.Any())
                    return ServiceResult.Failure(string.Join("; ", errors));

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(ValidateAddressList), GetType(), _logger);
                return ServiceResult.Failure("驗證地址清單時發生錯誤");
            }
        }

        public ServiceResult EnsurePrimaryAddressExists(List<CustomerAddress> addresses)
        {
            try
            {
                if (addresses == null || addresses.Count == 0)
                    return ServiceResult.Failure("沒有地址資料");

                // 檢查是否有主要地址
                var primaryCount = addresses.Count(a => a.IsPrimary);
                if (primaryCount == 0)
                {
                    // 如果沒有主要地址，將第一個設為主要
                    addresses[0].IsPrimary = true;
                    return ServiceResult.Success();
                }
                else if (primaryCount > 1)
                {
                    // 如果有多個主要地址，只保留第一個
                    bool firstPrimaryFound = false;
                    foreach (var address in addresses)
                    {
                        if (address.IsPrimary)
                        {
                            if (firstPrimaryFound)
                            {
                                address.IsPrimary = false;
                            }
                            else
                            {
                                firstPrimaryFound = true;
                            }
                        }
                    }
                    return ServiceResult.Success();
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(EnsurePrimaryAddressExists), GetType(), _logger);
                return ServiceResult.Failure("確保主要地址存在時發生錯誤");
            }
        }

        public int GetCompletedAddressCount(List<CustomerAddress> addresses)
        {
            try
            {
                if (addresses == null)
                    return 0;

                return addresses.Count(a =>
                    !string.IsNullOrWhiteSpace(a.Address) &&
                    !string.IsNullOrWhiteSpace(a.City));
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(GetCompletedAddressCount), GetType(), _logger);
                return 0;
            }
        }

        public int GetAddressCompletedFieldsCount(List<CustomerAddress> addresses)
        {
            try
            {
                if (addresses == null || !addresses.Any())
                    return 0;

                int completedFields = 0;
                
                foreach (var address in addresses)
                {
                    if (address.AddressTypeId > 0) completedFields++;
                    if (!string.IsNullOrWhiteSpace(address.PostalCode)) completedFields++;
                    if (!string.IsNullOrWhiteSpace(address.City)) completedFields++;
                    if (!string.IsNullOrWhiteSpace(address.District)) completedFields++;
                    if (!string.IsNullOrWhiteSpace(address.Address)) completedFields++;
                }

                return completedFields;
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(GetAddressCompletedFieldsCount), GetType(), _logger);
                return 0;
            }
        }

        #endregion
    }
}

