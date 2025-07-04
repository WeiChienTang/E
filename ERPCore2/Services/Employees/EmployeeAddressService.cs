using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;
using ERPCore2.Services.GenericManagementService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 員工地址管理服務實作
    /// </summary>
    public class EmployeeAddressService : GenericManagementService<EmployeeAddress>, IEmployeeAddressService
    {
        public EmployeeAddressService(
            AppDbContext context, 
            ILogger<GenericManagementService<EmployeeAddress>> logger, 
            IErrorLogService errorLogService) : base(context, logger, errorLogService)
        {
        }

        #region 覆寫基底抽象方法

        public override async Task<List<EmployeeAddress>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                return await _dbSet
                    .Include(ea => ea.Employee)
                    .Include(ea => ea.AddressType)
                    .Where(ea => !ea.IsDeleted &&
                               (ea.Address != null && ea.Address.Contains(searchTerm)) ||
                               (ea.City != null && ea.City.Contains(searchTerm)) ||
                               (ea.District != null && ea.District.Contains(searchTerm)) ||
                               (ea.PostalCode != null && ea.PostalCode.Contains(searchTerm)) ||
                               (ea.Employee != null && ea.Employee.EmployeeCode.Contains(searchTerm)) ||
                               (ea.Employee != null && (ea.Employee.FirstName + " " + ea.Employee.LastName).Contains(searchTerm)))
                    .OrderBy(ea => ea.Employee != null ? ea.Employee.EmployeeCode : string.Empty)
                    .ThenBy(ea => ea.AddressType != null ? ea.AddressType.TypeName : string.Empty)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex);
                _logger.LogError(ex, "搜尋員工地址時發生錯誤: SearchTerm={SearchTerm}", searchTerm);
                return new List<EmployeeAddress>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(EmployeeAddress entity)
        {
            try
            {
                var errors = new List<string>();

                // 基本驗證 - 員工ID
                if (entity.EmployeeId <= 0)
                {
                    errors.Add("員工ID無效");
                }
                else
                {
                    // 檢查員工是否存在
                    var employeeExists = await _context.Employees.AnyAsync(e => e.Id == entity.EmployeeId && !e.IsDeleted);
                    if (!employeeExists)
                    {
                        errors.Add("指定的員工不存在");
                    }
                }

                // 地址類型驗證
                if (entity.AddressTypeId.HasValue)
                {
                    var addressTypeExists = await _context.AddressTypes.AnyAsync(at => at.Id == entity.AddressTypeId.Value && !at.IsDeleted);
                    if (!addressTypeExists)
                    {
                        errors.Add("指定的地址類型不存在");
                    }
                }

                // 檢查重複地址
                if (await IsDuplicateAddressAsync(entity))
                {
                    errors.Add("此員工已存在相同的地址");
                }

                return errors.Any() ? ServiceResult.Failure(string.Join("; ", errors)) : ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex);
                _logger.LogError(ex, "驗證員工地址時發生錯誤: EmployeeId={EmployeeId}", entity.EmployeeId);
                return ServiceResult.Failure("驗證員工地址時發生錯誤");
            }
        }

        public override async Task<List<EmployeeAddress>> GetAllAsync()
        {
            try
            {
                return await _dbSet
                    .Include(ea => ea.Employee)
                    .Include(ea => ea.AddressType)
                    .Where(ea => !ea.IsDeleted)
                    .OrderBy(ea => ea.Employee != null ? ea.Employee.EmployeeCode : string.Empty)
                    .ThenBy(ea => ea.AddressType != null ? ea.AddressType.TypeName : string.Empty)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex);
                _logger.LogError(ex, "取得所有員工地址時發生錯誤");
                return new List<EmployeeAddress>();
            }
        }

        public override async Task<EmployeeAddress?> GetByIdAsync(int id)
        {
            try
            {
                return await _dbSet
                    .Include(ea => ea.Employee)
                    .Include(ea => ea.AddressType)
                    .FirstOrDefaultAsync(ea => ea.Id == id && !ea.IsDeleted);
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex);
                _logger.LogError(ex, "根據ID取得員工地址時發生錯誤: Id={Id}", id);
                return null;
            }
        }

        #endregion

        #region 業務特定查詢方法

        /// <summary>
        /// 根據員工ID取得地址清單
        /// </summary>
        public async Task<List<EmployeeAddress>> GetByEmployeeIdAsync(int employeeId)
        {
            try
            {
                return await _dbSet
                    .Include(ea => ea.AddressType)
                    .Where(ea => ea.EmployeeId == employeeId && !ea.IsDeleted)
                    .OrderBy(ea => ea.AddressType != null ? ea.AddressType.TypeName : string.Empty)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex);
                _logger.LogError(ex, "根據員工ID取得地址清單時發生錯誤: EmployeeId={EmployeeId}", employeeId);
                return new List<EmployeeAddress>();
            }
        }

        /// <summary>
        /// 取得員工的主要地址
        /// </summary>
        public async Task<EmployeeAddress?> GetPrimaryAddressAsync(int employeeId)
        {
            try
            {
                return await _dbSet
                    .Include(ea => ea.AddressType)
                    .FirstOrDefaultAsync(ea => ea.EmployeeId == employeeId && ea.IsPrimary && !ea.IsDeleted);
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex);
                _logger.LogError(ex, "取得員工主要地址時發生錯誤: EmployeeId={EmployeeId}", employeeId);
                return null;
            }
        }

        /// <summary>
        /// 根據地址類型取得地址清單
        /// </summary>
        public async Task<List<EmployeeAddress>> GetByAddressTypeAsync(int addressTypeId)
        {
            try
            {
                return await _dbSet
                    .Include(ea => ea.Employee)
                    .Where(ea => ea.AddressTypeId == addressTypeId && !ea.IsDeleted)
                    .OrderBy(ea => ea.Employee != null ? ea.Employee.EmployeeCode : string.Empty)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex);
                _logger.LogError(ex, "根據地址類型取得地址清單時發生錯誤: AddressTypeId={AddressTypeId}", addressTypeId);
                return new List<EmployeeAddress>();
            }
        }

        #endregion

        #region 業務邏輯操作

        /// <summary>
        /// 設定主要地址
        /// </summary>
        public async Task<ServiceResult> SetPrimaryAddressAsync(int addressId)
        {
            try
            {
                var address = await GetByIdAsync(addressId);
                if (address == null)
                    return ServiceResult.Failure("找不到指定的員工地址");

                // 將同一員工同一地址類型的其他地址設為非主要
                if (address.AddressTypeId.HasValue)
                {
                    var otherAddresses = await _dbSet
                        .Where(ea => ea.EmployeeId == address.EmployeeId &&
                                   ea.AddressTypeId == address.AddressTypeId &&
                                   ea.Id != address.Id &&
                                   !ea.IsDeleted)
                        .ToListAsync();

                    foreach (var otherAddress in otherAddresses)
                    {
                        otherAddress.IsPrimary = false;
                        otherAddress.UpdatedAt = DateTime.Now;
                    }
                }

                // 設定為主要地址
                address.IsPrimary = true;
                address.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex);
                _logger.LogError(ex, "設定主要地址時發生錯誤: AddressId={AddressId}", addressId);
                return ServiceResult.Failure("設定主要地址時發生錯誤");
            }
        }

        /// <summary>
        /// 複製地址到其他員工
        /// </summary>
        public async Task<ServiceResult<EmployeeAddress>> CopyAddressToEmployeeAsync(EmployeeAddress sourceAddress, int targetEmployeeId, int? targetAddressTypeId = null)
        {
            try
            {
                // 檢查目標員工是否存在
                var targetEmployeeExists = await _context.Employees.AnyAsync(e => e.Id == targetEmployeeId && !e.IsDeleted);
                if (!targetEmployeeExists)
                    return ServiceResult<EmployeeAddress>.Failure("目標員工不存在");

                var newAddress = new EmployeeAddress
                {
                    EmployeeId = targetEmployeeId,
                    AddressTypeId = targetAddressTypeId ?? sourceAddress.AddressTypeId,
                    PostalCode = sourceAddress.PostalCode,
                    City = sourceAddress.City,
                    District = sourceAddress.District,
                    Address = sourceAddress.Address,
                    IsPrimary = false, // 複製的地址預設不是主要地址
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Status = EntityStatus.Active
                };

                var createResult = await CreateAsync(newAddress);                if (createResult.IsSuccess)
                {
                    return ServiceResult<EmployeeAddress>.Success(newAddress);
                }

                return ServiceResult<EmployeeAddress>.Failure(createResult.ErrorMessage);
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex);
                _logger.LogError(ex, "複製地址時發生錯誤: SourceAddressId={SourceAddressId}, TargetEmployeeId={TargetEmployeeId}",
                    sourceAddress.Id, targetEmployeeId);
                return ServiceResult<EmployeeAddress>.Failure("複製地址時發生錯誤");
            }
        }

        /// <summary>
        /// 確保員工至少有一個主要地址
        /// </summary>
        public async Task<ServiceResult> EnsureEmployeeHasPrimaryAddressAsync(int employeeId)
        {
            try
            {
                var primaryAddress = await GetPrimaryAddressAsync(employeeId);
                if (primaryAddress != null)
                    return ServiceResult.Success();

                // 沒有主要地址，找第一個地址設為主要
                var firstAddress = await _dbSet
                    .FirstOrDefaultAsync(ea => ea.EmployeeId == employeeId && !ea.IsDeleted);

                if (firstAddress != null)
                {
                    return await SetPrimaryAddressAsync(firstAddress.Id);
                }

                return ServiceResult.Success(); // 沒有地址也算成功
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex);
                _logger.LogError(ex, "確保員工主要地址時發生錯誤: EmployeeId={EmployeeId}", employeeId);
                return ServiceResult.Failure("確保員工主要地址時發生錯誤");
            }
        }

        /// <summary>
        /// 取得員工地址清單並初始化預設地址（如果需要）
        /// </summary>
        public async Task<List<EmployeeAddress>> GetAddressesWithDefaultAsync(int employeeId, List<AddressType> addressTypes)
        {
            try
            {
                var existingAddresses = await GetByEmployeeIdAsync(employeeId);
                var result = new List<EmployeeAddress>(existingAddresses);

                // 為每種地址類型建立預設地址（如果不存在）
                foreach (var addressType in addressTypes.Where(at => at.Status == EntityStatus.Active))
                {
                    if (!existingAddresses.Any(ea => ea.AddressTypeId == addressType.Id))
                    {
                        result.Add(new EmployeeAddress
                        {
                            EmployeeId = employeeId,
                            AddressTypeId = addressType.Id,
                            AddressType = addressType,
                            IsPrimary = false,
                            Status = EntityStatus.Active,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }
                }

                return result.OrderBy(ea => ea.AddressType?.TypeName ?? string.Empty).ToList();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex);
                _logger.LogError(ex, "取得員工地址清單並初始化預設地址時發生錯誤: EmployeeId={EmployeeId}", employeeId);
                return new List<EmployeeAddress>();
            }
        }

        /// <summary>
        /// 更新或新增員工地址
        /// </summary>
        public async Task<ServiceResult<EmployeeAddress>> UpdateOrCreateAddressAsync(EmployeeAddress address)
        {
            try
            {
                if (address.Id > 0)
                {
                    // 更新現有地址
                    var updateResult = await UpdateAsync(address);
                    if (updateResult.IsSuccess)
                    {
                        var updatedAddress = await GetByIdAsync(address.Id);
                        return ServiceResult<EmployeeAddress>.Success(updatedAddress!);
                    }
                    return ServiceResult<EmployeeAddress>.Failure(updateResult.ErrorMessage);
                }
                else
                {
                    // 建立新地址
                    var createResult = await CreateAsync(address);
                    if (createResult.IsSuccess)
                    {
                        return ServiceResult<EmployeeAddress>.Success(address);
                    }
                    return ServiceResult<EmployeeAddress>.Failure(createResult.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex);
                _logger.LogError(ex, "更新或新增員工地址時發生錯誤: AddressId={AddressId}", address.Id);
                return ServiceResult<EmployeeAddress>.Failure("更新或新增地址時發生錯誤");
            }
        }

        /// <summary>
        /// 驗證地址資料完整性
        /// </summary>
        public ServiceResult ValidateAddress(EmployeeAddress address)
        {
            try
            {
                var errors = new List<string>();

                if (address.EmployeeId <= 0)
                    errors.Add("員工ID無效");

                if (string.IsNullOrWhiteSpace(address.Address) &&
                    string.IsNullOrWhiteSpace(address.City) &&
                    string.IsNullOrWhiteSpace(address.District) &&
                    string.IsNullOrWhiteSpace(address.PostalCode))
                {
                    errors.Add("地址資訊不能全部為空");
                }

                return errors.Any() ? ServiceResult.Failure(string.Join("; ", errors)) : ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _errorLogService.LogErrorAsync(ex).Wait();
                _logger.LogError(ex, "驗證地址資料完整性時發生錯誤: EmployeeId={EmployeeId}", address.EmployeeId);
                return ServiceResult.Failure("驗證地址資料時發生錯誤");
            }
        }

        /// <summary>
        /// 格式化完整地址字串
        /// </summary>
        public string FormatFullAddress(EmployeeAddress address)
        {
            try
            {
                var parts = new List<string>();

                if (!string.IsNullOrWhiteSpace(address.PostalCode))
                    parts.Add(address.PostalCode);

                if (!string.IsNullOrWhiteSpace(address.City))
                    parts.Add(address.City);

                if (!string.IsNullOrWhiteSpace(address.District))
                    parts.Add(address.District);

                if (!string.IsNullOrWhiteSpace(address.Address))
                    parts.Add(address.Address);

                return string.Join(" ", parts);
            }
            catch (Exception ex)
            {
                _errorLogService.LogErrorAsync(ex).Wait();
                _logger.LogError(ex, "格式化完整地址字串時發生錯誤");
                return string.Empty;
            }
        }

        /// <summary>
        /// 檢查地址是否重複
        /// </summary>
        public async Task<bool> IsDuplicateAddressAsync(EmployeeAddress address)
        {
            try
            {
                return await _dbSet.AnyAsync(ea =>
                    ea.EmployeeId == address.EmployeeId &&
                    ea.AddressTypeId == address.AddressTypeId &&
                    ea.PostalCode == address.PostalCode &&
                    ea.City == address.City &&
                    ea.District == address.District &&
                    ea.Address == address.Address &&
                    ea.Id != address.Id &&
                    !ea.IsDeleted);
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex);
                _logger.LogError(ex, "檢查地址是否重複時發生錯誤: EmployeeId={EmployeeId}", address.EmployeeId);
                return false;
            }
        }

        #endregion

        #region 統計與報告

        /// <summary>
        /// 取得員工地址完成度統計
        /// </summary>
        public async Task<ServiceResult<Dictionary<string, int>>> GetAddressCompletionStatsAsync(int employeeId)
        {
            try
            {
                var addresses = await GetByEmployeeIdAsync(employeeId);
                var stats = new Dictionary<string, int>
                {
                    ["總地址數"] = addresses.Count,
                    ["完整地址數"] = addresses.Count(a => !string.IsNullOrWhiteSpace(FormatFullAddress(a))),
                    ["主要地址數"] = addresses.Count(a => a.IsPrimary),
                    ["有郵遞區號"] = addresses.Count(a => !string.IsNullOrWhiteSpace(a.PostalCode)),
                    ["有城市"] = addresses.Count(a => !string.IsNullOrWhiteSpace(a.City)),
                    ["有行政區"] = addresses.Count(a => !string.IsNullOrWhiteSpace(a.District)),
                    ["有詳細地址"] = addresses.Count(a => !string.IsNullOrWhiteSpace(a.Address))
                };

                return ServiceResult<Dictionary<string, int>>.Success(stats);
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex);
                _logger.LogError(ex, "取得員工地址統計時發生錯誤: EmployeeId={EmployeeId}", employeeId);
                return ServiceResult<Dictionary<string, int>>.Failure("取得地址統計時發生錯誤");
            }
        }

        /// <summary>
        /// 取得指定城市的員工地址清單
        /// </summary>
        public async Task<List<EmployeeAddress>> GetAddressesByCityAsync(string city)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(city))
                    return new List<EmployeeAddress>();

                return await _dbSet
                    .Include(ea => ea.Employee)
                    .Include(ea => ea.AddressType)
                    .Where(ea => ea.City == city && !ea.IsDeleted)
                    .OrderBy(ea => ea.Employee != null ? ea.Employee.EmployeeCode : string.Empty)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex);
                _logger.LogError(ex, "取得指定城市的員工地址清單時發生錯誤: City={City}", city);
                return new List<EmployeeAddress>();
            }
        }

        /// <summary>
        /// 取得指定行政區的員工地址清單
        /// </summary>
        public async Task<List<EmployeeAddress>> GetAddressesByDistrictAsync(string district)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(district))
                    return new List<EmployeeAddress>();

                return await _dbSet
                    .Include(ea => ea.Employee)
                    .Include(ea => ea.AddressType)
                    .Where(ea => ea.District == district && !ea.IsDeleted)
                    .OrderBy(ea => ea.Employee != null ? ea.Employee.EmployeeCode : string.Empty)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex);
                _logger.LogError(ex, "取得指定行政區的員工地址清單時發生錯誤: District={District}", district);
                return new List<EmployeeAddress>();
            }
        }

        /// <summary>
        /// 取得指定郵遞區號的員工地址清單
        /// </summary>
        public async Task<List<EmployeeAddress>> GetAddressesByPostalCodeAsync(string postalCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(postalCode))
                    return new List<EmployeeAddress>();

                return await _dbSet
                    .Include(ea => ea.Employee)
                    .Include(ea => ea.AddressType)
                    .Where(ea => ea.PostalCode == postalCode && !ea.IsDeleted)
                    .OrderBy(ea => ea.Employee != null ? ea.Employee.EmployeeCode : string.Empty)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex);
                _logger.LogError(ex, "取得指定郵遞區號的員工地址清單時發生錯誤: PostalCode={PostalCode}", postalCode);
                return new List<EmployeeAddress>();
            }
        }

        #endregion

        #region 地址類型相關操作

        /// <summary>
        /// 根據地址類型名稱取得員工地址值
        /// </summary>
        public string GetAddressValue(int employeeId, string addressTypeName,
            List<AddressType> addressTypes, List<EmployeeAddress> employeeAddresses)
        {
            try
            {
                var addressType = addressTypes.FirstOrDefault(at => at.TypeName == addressTypeName);
                if (addressType == null)
                    return string.Empty;

                var address = employeeAddresses.FirstOrDefault(ea =>
                    ea.EmployeeId == employeeId && ea.AddressTypeId == addressType.Id);

                return address != null ? FormatFullAddress(address) : string.Empty;
            }
            catch (Exception ex)
            {
                _errorLogService.LogErrorAsync(ex).Wait();
                _logger.LogError(ex, "取得員工地址值時發生錯誤: EmployeeId={EmployeeId}, AddressTypeName={AddressTypeName}",
                    employeeId, addressTypeName);
                return string.Empty;
            }
        }

        /// <summary>
        /// 更新員工特定類型的地址
        /// </summary>
        public ServiceResult UpdateAddressValue(int employeeId, string addressTypeName, EmployeeAddress addressData,
            List<AddressType> addressTypes, List<EmployeeAddress> employeeAddresses)
        {
            try
            {
                var addressType = addressTypes.FirstOrDefault(at => at.TypeName == addressTypeName);
                if (addressType == null)
                    return ServiceResult.Failure($"找不到地址類型: {addressTypeName}");

                var existingAddress = employeeAddresses.FirstOrDefault(ea =>
                    ea.EmployeeId == employeeId && ea.AddressTypeId == addressType.Id);

                var isEmpty = string.IsNullOrWhiteSpace(addressData.PostalCode) &&
                             string.IsNullOrWhiteSpace(addressData.City) &&
                             string.IsNullOrWhiteSpace(addressData.District) &&
                             string.IsNullOrWhiteSpace(addressData.Address);

                if (isEmpty)
                {
                    // 如果地址為空，移除現有地址
                    if (existingAddress != null)
                    {
                        employeeAddresses.Remove(existingAddress);
                    }
                }
                else
                {
                    if (existingAddress != null)
                    {
                        // 更新現有地址
                        existingAddress.PostalCode = addressData.PostalCode;
                        existingAddress.City = addressData.City;
                        existingAddress.District = addressData.District;
                        existingAddress.Address = addressData.Address;
                        existingAddress.UpdatedAt = DateTime.Now;
                    }
                    else
                    {
                        // 建立新地址
                        var newAddress = new EmployeeAddress
                        {
                            EmployeeId = employeeId,
                            AddressTypeId = addressType.Id,
                            PostalCode = addressData.PostalCode,
                            City = addressData.City,
                            District = addressData.District,
                            Address = addressData.Address,
                            IsPrimary = false,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                            Status = EntityStatus.Active
                        };
                        employeeAddresses.Add(newAddress);
                    }
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _errorLogService.LogErrorAsync(ex).Wait();
                _logger.LogError(ex, "更新員工地址值時發生錯誤: EmployeeId={EmployeeId}, AddressTypeName={AddressTypeName}",
                    employeeId, addressTypeName);
                return ServiceResult.Failure("更新地址時發生錯誤");
            }
        }

        /// <summary>
        /// 計算已完成的地址欄位數量
        /// </summary>
        public int GetAddressCompletedFieldsCount(List<EmployeeAddress> employeeAddresses)
        {
            try
            {
                return employeeAddresses.Count(ea => !string.IsNullOrWhiteSpace(FormatFullAddress(ea)));
            }
            catch (Exception ex)
            {
                _errorLogService.LogErrorAsync(ex).Wait();
                _logger.LogError(ex, "計算已完成的地址欄位數量時發生錯誤");
                return 0;
            }
        }

        /// <summary>
        /// 驗證員工地址資料
        /// </summary>
        public ServiceResult ValidateEmployeeAddresses(List<EmployeeAddress> employeeAddresses)
        {
            try
            {
                var errors = new List<string>();

                // 檢查重複的地址類型
                var duplicateTypes = employeeAddresses
                    .Where(ea => ea.AddressTypeId.HasValue)
                    .GroupBy(ea => ea.AddressTypeId)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicateTypes.Any())
                {
                    errors.Add("存在重複的地址類型");
                }

                // 檢查每種地址類型的主要地址唯一性
                var primaryAddressTypes = employeeAddresses
                    .Where(ea => ea.IsPrimary && ea.AddressTypeId.HasValue)
                    .GroupBy(ea => ea.AddressTypeId)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (primaryAddressTypes.Any())
                {
                    errors.Add("每種地址類型只能有一個主要地址");
                }

                return errors.Any() ? ServiceResult.Failure(string.Join("; ", errors)) : ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _errorLogService.LogErrorAsync(ex).Wait();
                _logger.LogError(ex, "驗證員工地址資料時發生錯誤");
                return ServiceResult.Failure("驗證員工地址資料時發生錯誤");
            }
        }

        /// <summary>
        /// 確保每種地址類型只有一個主要地址
        /// </summary>
        public ServiceResult EnsureUniquePrimaryAddresses(List<EmployeeAddress> employeeAddresses)
        {
            try
            {                var addressTypeGroups = employeeAddresses
                    .Where(ea => ea.AddressTypeId.HasValue)
                    .GroupBy(ea => ea.AddressTypeId!.Value)
                    .ToList();

                foreach (var group in addressTypeGroups)
                {
                    var primaryAddresses = group.Where(ea => ea.IsPrimary).ToList();
                    if (primaryAddresses.Count > 1)
                    {
                        // 保留第一個，其他設為非主要
                        for (int i = 1; i < primaryAddresses.Count; i++)
                        {
                            primaryAddresses[i].IsPrimary = false;
                        }
                    }
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _errorLogService.LogErrorAsync(ex).Wait();
                _logger.LogError(ex, "確保主要地址唯一性時發生錯誤");
                return ServiceResult.Failure("處理主要地址時發生錯誤");
            }
        }

        #endregion
    }
}
