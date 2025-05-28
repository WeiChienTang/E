using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 客戶地址管理服務實作 - 專門處理客戶地址相關的業務邏輯
    /// </summary>
    public class CustomerAddressService : ICustomerAddressService
    {
        private readonly ILogger<CustomerAddressService> _logger;

        public CustomerAddressService(ILogger<CustomerAddressService> logger)
        {
            _logger = logger;
        }

        #region 地址初始化和配置

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

        #endregion

        #region 地址操作業務邏輯

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

        #region 地址資料更新

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

        #region 地址驗證和計算

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

        #endregion
    }
}
