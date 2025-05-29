using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 客戶地址管理服務實作 - 統一處理所有客戶地址相關操作
    /// 合併原本的 AddressService 和 CustomerAddressService 功能
    /// </summary>
    public class CustomerAddressService : ICustomerAddressService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ILogger<CustomerAddressService> _logger;

        public CustomerAddressService(IDbContextFactory<AppDbContext> contextFactory, ILogger<CustomerAddressService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        #region 資料查詢

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

        public async Task<CustomerAddress?> GetByIdAsync(int addressId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.CustomerAddresses
                    .Where(ca => ca.AddressId == addressId && ca.Status != EntityStatus.Deleted)
                    .Include(ca => ca.AddressType)
                    .Include(ca => ca.Customer)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting address by ID {AddressId}", addressId);
                throw;
            }
        }

        #endregion

        #region CRUD 操作

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
                    await ClearPrimaryAddressesAsync(context, address.CustomerId);
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
                address.CreatedDate = DateTime.Now;
                address.CreatedBy = "System"; // TODO: 從認證取得使用者

                context.CustomerAddresses.Add(address);
                await context.SaveChangesAsync();

                // 6. 重新查詢包含關聯資料的地址
                var savedAddress = await context.CustomerAddresses
                    .Include(ca => ca.AddressType)
                    .FirstOrDefaultAsync(ca => ca.AddressId == address.AddressId);

                _logger.LogInformation("Created address for customer {CustomerId}, AddressId: {AddressId}", 
                    address.CustomerId, address.AddressId);

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
                    await ClearPrimaryAddressesAsync(context, address.CustomerId);
                }

                // 4. 更新欄位
                existingAddress.AddressTypeId = address.AddressTypeId;
                existingAddress.PostalCode = address.PostalCode;
                existingAddress.City = address.City;
                existingAddress.District = address.District;
                existingAddress.Address = address.Address;
                existingAddress.IsPrimary = address.IsPrimary;
                existingAddress.ModifiedDate = DateTime.Now;
                existingAddress.ModifiedBy = "System"; // TODO: 從認證取得使用者

                await context.SaveChangesAsync();

                // 5. 重新查詢包含關聯資料的地址
                var updatedAddress = await context.CustomerAddresses
                    .Include(ca => ca.AddressType)
                    .FirstOrDefaultAsync(ca => ca.AddressId == address.AddressId);

                _logger.LogInformation("Updated address {AddressId} for customer {CustomerId}", 
                    address.AddressId, address.CustomerId);

                return ServiceResult<CustomerAddress>.Success(updatedAddress!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address {AddressId}", address.AddressId);
                return ServiceResult<CustomerAddress>.Failure($"更新地址時發生錯誤: {ex.Message}");
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

                // 檢查是否為客戶的唯一地址
                var customerAddressCount = await context.CustomerAddresses
                    .CountAsync(ca => ca.CustomerId == address.CustomerId && ca.Status != EntityStatus.Deleted);

                if (customerAddressCount <= 1)
                    return ServiceResult<bool>.Failure("客戶至少需要保留一個地址");

                // 軟刪除
                address.Status = EntityStatus.Deleted;
                address.ModifiedDate = DateTime.Now;
                address.ModifiedBy = "System"; // TODO: 從認證取得使用者

                // 如果刪除的是主要地址，需要設定新的主要地址
                if (address.IsPrimary)
                {
                    await SetNewPrimaryAddressAsync(context, address.CustomerId);
                }

                await context.SaveChangesAsync();

                _logger.LogInformation("Deleted address {AddressId} for customer {CustomerId}", 
                    addressId, address.CustomerId);

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
                    // 驗證所有地址
                    foreach (var address in addresses)
                    {
                        var validationResult = await ValidateAddressAsync(address);
                        if (!validationResult.IsSuccess)
                            return ServiceResult<bool>.Failure($"地址驗證失敗: {validationResult.ErrorMessage}");
                    }

                    // 確保只有一個主要地址
                    var primaryCount = addresses.Count(a => a.IsPrimary);
                    if (primaryCount == 0 && addresses.Any())
                        return ServiceResult<bool>.Failure("至少需要設定一個主要地址");
                    if (primaryCount > 1)
                        return ServiceResult<bool>.Failure("只能設定一個主要地址");

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
                            existing.ModifiedDate = DateTime.Now;
                            existing.ModifiedBy = "System";
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
                            address.CreatedDate = DateTime.Now;
                            address.CreatedBy = "System";
                            context.CustomerAddresses.Add(address);
                        }
                        else
                        {
                            // 更新
                            var existing = existingAddresses.FirstOrDefault(ea => ea.AddressId == address.AddressId);
                            if (existing != null)
                            {
                                existing.AddressTypeId = address.AddressTypeId;
                                existing.PostalCode = address.PostalCode;
                                existing.City = address.City;
                                existing.District = address.District;
                                existing.Address = address.Address;
                                existing.IsPrimary = address.IsPrimary;
                                existing.ModifiedDate = DateTime.Now;
                                existing.ModifiedBy = "System";
                            }
                        }
                    }

                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Updated addresses for customer {CustomerId}", customerId);
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
                _logger.LogError(ex, "Error updating addresses for customer {CustomerId}", customerId);
                return ServiceResult<bool>.Failure($"更新客戶地址時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 業務邏輯操作

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
                await ClearPrimaryAddressesAsync(context, address.CustomerId);

                // 設為主要地址
                address.IsPrimary = true;
                address.ModifiedDate = DateTime.Now;
                address.ModifiedBy = "System";

                await context.SaveChangesAsync();

                _logger.LogInformation("Set address {AddressId} as primary for customer {CustomerId}", 
                    addressId, address.CustomerId);

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting primary address {AddressId}", addressId);
                return ServiceResult<bool>.Failure($"設定主要地址時發生錯誤: {ex.Message}");
            }
        }

        public async Task<ServiceResult<CustomerAddress>> CopyAddressToCustomerAsync(CustomerAddress sourceAddress, int targetCustomerId, int? targetAddressTypeId = null)
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
                        firstAddress.ModifiedDate = DateTime.Now;
                        firstAddress.ModifiedBy = "System";
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

        #region 記憶體操作方法（用於UI編輯）

        public CustomerAddress CreateNewAddress(int customerId, int addressCount)
        {
            try
            {
                return new CustomerAddress
                {
                    CustomerId = customerId,
                    Status = EntityStatus.Active,
                    IsPrimary = addressCount == 0, // 第一個地址設為主要地址
                    PostalCode = string.Empty,
                    City = string.Empty,
                    District = string.Empty,
                    Address = string.Empty
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new address for customer {CustomerId}", customerId);
                throw;
            }
        }

        public void InitializeDefaultAddresses(List<CustomerAddress> addressList, List<AddressType> addressTypes)
        {
            try
            {
                addressList.Clear();
                
                // 尋找公司地址類型
                var companyAddressType = addressTypes.FirstOrDefault(at => at.TypeName.Contains("公司"));
                
                // 如果找不到公司地址類型，使用第一個可用的類型
                if (companyAddressType == null && addressTypes.Any())
                    companyAddressType = addressTypes.First();
                
                // 新增預設公司地址
                if (companyAddressType != null)
                {
                    addressList.Add(new CustomerAddress
                    {
                        AddressTypeId = companyAddressType.AddressTypeId,
                        IsPrimary = true,
                        Status = EntityStatus.Active,
                        PostalCode = string.Empty,
                        City = string.Empty,
                        District = string.Empty,
                        Address = string.Empty
                    });
                }
                
                _logger.LogInformation("Initialized {AddressCount} default addresses", addressList.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing default addresses");
                throw;
            }
        }

        public ServiceResult AddAddress(List<CustomerAddress> addressList, CustomerAddress newAddress)
        {
            try
            {
                if (addressList == null)
                    return ServiceResult.Failure("地址清單不可為空");

                if (newAddress == null)
                    return ServiceResult.Failure("新地址不可為空");

                // 設定預設值
                newAddress.Status = EntityStatus.Active;
                
                // 如果是第一個地址，設為主要地址
                if (!addressList.Any())
                {
                    newAddress.IsPrimary = true;
                }

                addressList.Add(newAddress);
                
                _logger.LogInformation("Added new address, total count: {AddressCount}", addressList.Count);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding address");
                return ServiceResult.Failure($"新增地址時發生錯誤: {ex.Message}");
            }
        }

        public ServiceResult RemoveAddress(List<CustomerAddress> addressList, int index)
        {
            try
            {
                if (addressList == null)
                    return ServiceResult.Failure("地址清單不可為空");

                if (index < 0 || index >= addressList.Count)
                    return ServiceResult.Failure("無效的地址索引");

                if (addressList.Count <= 1)
                    return ServiceResult.Failure("至少需要保留一個地址");

                var removedAddress = addressList[index];
                addressList.RemoveAt(index);
                
                // 如果刪除的是主要地址，將第一個地址設為主要
                if (removedAddress.IsPrimary && addressList.Any())
                {
                    addressList[0].IsPrimary = true;
                }
                
                _logger.LogInformation("Removed address at index {Index}, remaining count: {AddressCount}", index, addressList.Count);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing address at index {Index}", index);
                return ServiceResult.Failure($"移除地址時發生錯誤: {ex.Message}");
            }
        }

        public ServiceResult SetPrimaryAddress(List<CustomerAddress> addressList, int index)
        {
            try
            {
                if (addressList == null)
                    return ServiceResult.Failure("地址清單不可為空");

                if (index < 0 || index >= addressList.Count)
                    return ServiceResult.Failure("無效的地址索引");

                // 清除所有主要地址標記
                foreach (var address in addressList)
                {
                    address.IsPrimary = false;
                }
                
                // 設定新的主要地址
                addressList[index].IsPrimary = true;
                
                _logger.LogInformation("Set primary address to index {Index}", index);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting primary address at index {Index}", index);
                return ServiceResult.Failure($"設定主要地址時發生錯誤: {ex.Message}");
            }
        }

        public ServiceResult CopyAddressFromFirst(List<CustomerAddress> addressList, int targetIndex)
        {
            try
            {
                if (addressList == null)
                    return ServiceResult.Failure("地址清單不可為空");

                if (targetIndex < 1 || targetIndex >= addressList.Count)
                    return ServiceResult.Failure("無效的目標地址索引");

                if (addressList.Count < 2)
                    return ServiceResult.Failure("至少需要兩個地址才能進行複製");

                var sourceAddress = addressList[0];
                var targetAddress = addressList[targetIndex];
                
                // 複製地址資料（不包含主要地址標記和地址類型）
                targetAddress.PostalCode = sourceAddress.PostalCode;
                targetAddress.City = sourceAddress.City;
                targetAddress.District = sourceAddress.District;
                targetAddress.Address = sourceAddress.Address;
                
                _logger.LogInformation("Copied address from index 0 to index {TargetIndex}", targetIndex);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error copying address from first to index {TargetIndex}", targetIndex);
                return ServiceResult.Failure($"複製地址時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 記憶體欄位更新方法

        public ServiceResult UpdateAddressType(List<CustomerAddress> addressList, int index, int? addressTypeId)
        {
            try
            {
                if (addressList == null)
                    return ServiceResult.Failure("地址清單不可為空");

                if (index < 0 || index >= addressList.Count)
                    return ServiceResult.Failure("無效的地址索引");

                addressList[index].AddressTypeId = addressTypeId;
                
                _logger.LogDebug("Updated address type for index {Index} to {AddressTypeId}", index, addressTypeId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address type for index {Index}", index);
                return ServiceResult.Failure($"更新地址類型時發生錯誤: {ex.Message}");
            }
        }

        public ServiceResult UpdatePostalCode(List<CustomerAddress> addressList, int index, string? postalCode)
        {
            try
            {
                if (addressList == null)
                    return ServiceResult.Failure("地址清單不可為空");

                if (index < 0 || index >= addressList.Count)
                    return ServiceResult.Failure("無效的地址索引");

                addressList[index].PostalCode = postalCode;
                
                _logger.LogDebug("Updated postal code for index {Index} to {PostalCode}", index, postalCode);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating postal code for index {Index}", index);
                return ServiceResult.Failure($"更新郵遞區號時發生錯誤: {ex.Message}");
            }
        }

        public ServiceResult UpdateCity(List<CustomerAddress> addressList, int index, string? city)
        {
            try
            {
                if (addressList == null)
                    return ServiceResult.Failure("地址清單不可為空");

                if (index < 0 || index >= addressList.Count)
                    return ServiceResult.Failure("無效的地址索引");

                addressList[index].City = city;
                
                _logger.LogDebug("Updated city for index {Index} to {City}", index, city);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating city for index {Index}", index);
                return ServiceResult.Failure($"更新城市時發生錯誤: {ex.Message}");
            }
        }

        public ServiceResult UpdateDistrict(List<CustomerAddress> addressList, int index, string? district)
        {
            try
            {
                if (addressList == null)
                    return ServiceResult.Failure("地址清單不可為空");

                if (index < 0 || index >= addressList.Count)
                    return ServiceResult.Failure("無效的地址索引");

                addressList[index].District = district;
                
                _logger.LogDebug("Updated district for index {Index} to {District}", index, district);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating district for index {Index}", index);
                return ServiceResult.Failure($"更新行政區時發生錯誤: {ex.Message}");
            }
        }

        public ServiceResult UpdateAddress(List<CustomerAddress> addressList, int index, string? address)
        {
            try
            {
                if (addressList == null)
                    return ServiceResult.Failure("地址清單不可為空");

                if (index < 0 || index >= addressList.Count)
                    return ServiceResult.Failure("無效的地址索引");

                addressList[index].Address = address;
                
                _logger.LogDebug("Updated address for index {Index} to {Address}", index, address);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address for index {Index}", index);
                return ServiceResult.Failure($"更新地址時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 驗證和輔助方法

        public ServiceResult ValidateAddresses(List<CustomerAddress> addresses)
        {
            try
            {
                var errors = new List<string>();

                if (addresses == null || !addresses.Any())
                {
                    return ServiceResult.Success(); // 地址為選填
                }

                // 檢查主要地址數量
                var primaryCount = addresses.Count(a => a.IsPrimary);
                if (primaryCount == 0)
                {
                    errors.Add("至少需要設定一個主要地址");
                }
                else if (primaryCount > 1)
                {
                    errors.Add("只能設定一個主要地址");
                }

                // 檢查每個地址的內容
                for (int i = 0; i < addresses.Count; i++)
                {
                    var address = addresses[i];
                    var prefix = $"地址 {i + 1}";

                    // 檢查地址長度限制
                    if (!string.IsNullOrEmpty(address.PostalCode) && address.PostalCode.Length > 10)
                        errors.Add($"{prefix}: 郵遞區號不可超過10個字元");

                    if (!string.IsNullOrEmpty(address.City) && address.City.Length > 50)
                        errors.Add($"{prefix}: 城市不可超過50個字元");

                    if (!string.IsNullOrEmpty(address.District) && address.District.Length > 50)
                        errors.Add($"{prefix}: 行政區不可超過50個字元");

                    if (!string.IsNullOrEmpty(address.Address) && address.Address.Length > 200)
                        errors.Add($"{prefix}: 地址不可超過200個字元");

                    // 檢查是否有實際的地址內容
                    var hasContent = !string.IsNullOrWhiteSpace(address.PostalCode) ||
                                   !string.IsNullOrWhiteSpace(address.City) ||
                                   !string.IsNullOrWhiteSpace(address.District) ||
                                   !string.IsNullOrWhiteSpace(address.Address);

                    if (!hasContent && address.AddressTypeId.HasValue)
                        errors.Add($"{prefix}: 已選擇地址類型但未填寫地址資訊");
                }

                if (errors.Any())
                {
                    var errorMessage = string.Join("; ", errors);
                    _logger.LogWarning("Address validation failed: {Errors}", errorMessage);
                    return ServiceResult.Failure(errorMessage);
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating addresses");
                return ServiceResult.Failure($"驗證地址時發生錯誤: {ex.Message}");
            }
        }

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

        public ServiceResult EnsurePrimaryAddressExists(List<CustomerAddress> addresses)
        {
            try
            {
                if (addresses == null || !addresses.Any())
                    return ServiceResult.Success();

                var primaryCount = addresses.Count(a => a.IsPrimary);
                
                if (primaryCount == 0)
                {
                    // 沒有主要地址，將第一個設為主要
                    addresses[0].IsPrimary = true;
                    _logger.LogInformation("Set first address as primary address");
                }
                else if (primaryCount > 1)
                {
                    // 有多個主要地址，只保留第一個
                    bool foundFirst = false;
                    foreach (var address in addresses)
                    {
                        if (address.IsPrimary)
                        {
                            if (!foundFirst)
                            {
                                foundFirst = true;
                            }
                            else
                            {
                                address.IsPrimary = false;
                            }
                        }
                    }
                    _logger.LogInformation("Fixed multiple primary addresses, kept only the first one");
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring primary address exists");
                return ServiceResult.Failure($"確保主要地址存在時發生錯誤: {ex.Message}");
            }
        }

        public int GetAddressCompletedFieldsCount(List<CustomerAddress> addresses)
        {
            try
            {
                if (addresses == null || !addresses.Any())
                    return 0;
                    
                // 計算至少有一個完整地址的數量
                int completedAddresses = addresses.Count(addr => 
                    !string.IsNullOrWhiteSpace(addr.PostalCode) &&
                    !string.IsNullOrWhiteSpace(addr.City) &&
                    !string.IsNullOrWhiteSpace(addr.District) &&
                    !string.IsNullOrWhiteSpace(addr.Address)
                );
                
                return completedAddresses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating address completed fields count");
                return 0;
            }
        }

        public int? GetDefaultAddressTypeId(int addressIndex, List<AddressType> addressTypes)
        {
            try
            {
                if (!addressTypes.Any()) return null;
                
                return addressIndex switch
                {
                    0 => addressTypes.FirstOrDefault(at => at.TypeName.Contains("住宅") || at.TypeName.Contains("公司"))?.AddressTypeId ??
                         addressTypes.FirstOrDefault()?.AddressTypeId,
                    1 => addressTypes.FirstOrDefault(at => at.TypeName.Contains("通訊") || at.TypeName.Contains("寄信"))?.AddressTypeId ??
                         addressTypes.Skip(1).FirstOrDefault()?.AddressTypeId,
                    _ => addressTypes.FirstOrDefault()?.AddressTypeId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting default address type ID for index {AddressIndex}", addressIndex);
                throw;
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
                address.ModifiedDate = DateTime.Now;
                address.ModifiedBy = "System";
            }

            if (primaryAddresses.Any())
            {
                await context.SaveChangesAsync();
            }
        }

        private async Task SetNewPrimaryAddressAsync(AppDbContext context, int customerId)
        {
            // 找到第一個地址設為主要地址
            var firstAddress = await context.CustomerAddresses
                .Where(ca => ca.CustomerId == customerId && ca.Status != EntityStatus.Deleted)
                .OrderBy(ca => ca.AddressId)
                .FirstOrDefaultAsync();

            if (firstAddress != null)
            {
                firstAddress.IsPrimary = true;
                firstAddress.ModifiedDate = DateTime.Now;
                firstAddress.ModifiedBy = "System";
                await context.SaveChangesAsync();
            }
        }

        #endregion
    }
}