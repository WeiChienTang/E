using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services.GenericManagementService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 庫存異動類型服務實作
    /// </summary>
    public class InventoryTransactionTypeService : GenericManagementService<InventoryTransactionType>, IInventoryTransactionTypeService
    {
        public InventoryTransactionTypeService(
            AppDbContext context, 
            ILogger<GenericManagementService<InventoryTransactionType>> logger, 
            IErrorLogService errorLogService) : base(context, logger, errorLogService)
        {
        }

        /// <summary>
        /// 覆寫搜尋功能
        /// </summary>
        public override async Task<List<InventoryTransactionType>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                return await _dbSet
                    .Where(t => !t.IsDeleted &&
                               (t.TypeName.Contains(searchTerm) ||
                                t.TypeCode.Contains(searchTerm)))
                    .OrderBy(t => t.TypeCode)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(SearchAsync),
                    SearchTerm = searchTerm,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error searching inventory transaction types with term {SearchTerm}", searchTerm);
                throw;
            }
        }

        /// <summary>
        /// 檢查類型代碼是否存在
        /// </summary>
        public async Task<bool> IsTypeCodeExistsAsync(string typeCode, int? excludeId = null)
        {
            try
            {
                var query = _dbSet.Where(t => t.TypeCode == typeCode && !t.IsDeleted);

                if (excludeId.HasValue)
                    query = query.Where(t => t.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(IsTypeCodeExistsAsync),
                    TypeCode = typeCode,
                    ExcludeId = excludeId,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error checking if type code exists {TypeCode}", typeCode);
                return false; // 安全預設值
            }
        }

        /// <summary>
        /// 根據異動類型取得類型設定
        /// </summary>
        public async Task<List<InventoryTransactionType>> GetByTransactionTypeAsync(InventoryTransactionTypeEnum transactionType)
        {
            try
            {
                return await _dbSet
                    .Where(t => !t.IsDeleted && t.TransactionType == transactionType)
                    .OrderBy(t => t.TypeCode)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetByTransactionTypeAsync),
                    TransactionType = transactionType,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error getting inventory transaction types by type {TransactionType}", transactionType);
                throw;
            }
        }

        /// <summary>
        /// 取得需要審核的異動類型
        /// </summary>
        public async Task<List<InventoryTransactionType>> GetRequiresApprovalTypesAsync()
        {
            try
            {
                return await _dbSet
                    .Where(t => !t.IsDeleted && t.RequiresApproval)
                    .OrderBy(t => t.TypeCode)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetRequiresApprovalTypesAsync),
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error getting requires approval types");
                throw;
            }
        }

        /// <summary>
        /// 取得影響成本的異動類型
        /// </summary>
        public async Task<List<InventoryTransactionType>> GetAffectsCostTypesAsync()
        {
            try
            {
                return await _dbSet
                    .Where(t => !t.IsDeleted && t.AffectsCost)
                    .OrderBy(t => t.TypeCode)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetAffectsCostTypesAsync),
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error getting affects cost types");
                throw;
            }
        }

        /// <summary>
        /// 產生下一個異動單號
        /// </summary>
        public async Task<string> GenerateNextNumberAsync(int typeId)
        {
            try
            {
                var transactionType = await GetByIdAsync(typeId);
                if (transactionType == null || !transactionType.AutoGenerateNumber)
                    return string.Empty;

                var prefix = transactionType.NumberPrefix ?? transactionType.TypeCode;
                var today = DateTime.Now.ToString("yyyyMMdd");
                
                // 這裡可以實作更複雜的單號產生邏輯
                // 例如：查詢當日最大單號，然後遞增
                var randomSuffix = new Random().Next(1, 9999).ToString("D4");
                
                return $"{prefix}{today}{randomSuffix}";
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GenerateNextNumberAsync),
                    TypeId = typeId,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error generating next number for type {TypeId}", typeId);
                return string.Empty; // 安全預設值
            }
        }

        /// <summary>
        /// 覆寫驗證方法
        /// </summary>
        public override async Task<ServiceResult> ValidateAsync(InventoryTransactionType entity)
        {
            try
            {
                // 基本驗證
                if (string.IsNullOrWhiteSpace(entity.TypeCode))
                    return ServiceResult.Failure("類型代碼為必填");
                
                if (string.IsNullOrWhiteSpace(entity.TypeName))
                    return ServiceResult.Failure("類型名稱為必填");

                // 檢查類型代碼是否重複
                if (await IsTypeCodeExistsAsync(entity.TypeCode, entity.Id))
                    return ServiceResult.Failure("類型代碼已存在");

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(ValidateAsync),
                    EntityId = entity.Id,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error validating inventory transaction type entity {EntityId}", entity.Id);
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        /// <summary>
        /// 覆寫名稱存在檢查（使用類型名稱）
        /// </summary>
        public override async Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
        {
            try
            {
                var query = _dbSet.Where(t => t.TypeName == name && !t.IsDeleted);

                if (excludeId.HasValue)
                    query = query.Where(t => t.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(IsNameExistsAsync),
                    Name = name,
                    ExcludeId = excludeId,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error checking if name exists {Name}", name);
                return false; // 安全預設值
            }
        }
    }
}