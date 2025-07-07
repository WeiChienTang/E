using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services.GenericManagementService;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 庫存異動類型服務實作
    /// </summary>
    public class InventoryTransactionTypeService : GenericManagementService<InventoryTransactionType>, IInventoryTransactionTypeService
    {
        /// <summary>
        /// 完整建構子
        /// </summary>
        public InventoryTransactionTypeService(
            AppDbContext context, 
            ILogger<GenericManagementService<InventoryTransactionType>> logger) : base(context, logger)
        {
        }

        /// <summary>
        /// 簡易建構子
        /// </summary>
        public InventoryTransactionTypeService(AppDbContext context) : base(context)
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { 
                    SearchTerm = searchTerm 
                });
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsTypeCodeExistsAsync), GetType(), _logger, new { 
                    TypeCode = typeCode,
                    ExcludeId = excludeId 
                });
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByTransactionTypeAsync), GetType(), _logger, new { 
                    TransactionType = transactionType 
                });
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetRequiresApprovalTypesAsync), GetType(), _logger);
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAffectsCostTypesAsync), GetType(), _logger);
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GenerateNextNumberAsync), GetType(), _logger, new { 
                    TypeId = typeId 
                });
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new { 
                    EntityId = entity.Id 
                });
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsNameExistsAsync), GetType(), _logger, new { 
                    Name = name,
                    ExcludeId = excludeId 
                });
                return false; // 安全預設值
            }
        }
    }
}