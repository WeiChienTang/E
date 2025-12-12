using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 領貨服務實作
    /// </summary>
    public class MaterialIssueService : GenericManagementService<MaterialIssue>, IMaterialIssueService
    {
        private readonly IInventoryStockService _inventoryStockService;
        private readonly IInventoryTransactionService _inventoryTransactionService;

        public MaterialIssueService(
            IDbContextFactory<AppDbContext> contextFactory,
            IInventoryStockService inventoryStockService,
            IInventoryTransactionService inventoryTransactionService) : base(contextFactory)
        {
            _inventoryStockService = inventoryStockService;
            _inventoryTransactionService = inventoryTransactionService;
        }

        public MaterialIssueService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<MaterialIssue>> logger,
            IInventoryStockService inventoryStockService,
            IInventoryTransactionService inventoryTransactionService) : base(contextFactory, logger)
        {
            _inventoryStockService = inventoryStockService;
            _inventoryTransactionService = inventoryTransactionService;
        }

        #region 覆寫基底方法 - 包含庫存處理

        /// <summary>
        /// 建立領料單並扣除庫存
        /// </summary>
        public override async Task<ServiceResult<MaterialIssue>> CreateAsync(MaterialIssue entity)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // 1. 先執行基本的建立邏輯（包含驗證）
                var createResult = await base.CreateAsync(entity);
                if (!createResult.IsSuccess || createResult.Data == null)
                {
                    return createResult;
                }

                var materialIssue = createResult.Data;

                // 2. 載入領料明細
                var details = await context.MaterialIssueDetails
                    .Where(d => d.MaterialIssueId == materialIssue.Id && d.Status == EntityStatus.Active)
                    .ToListAsync();

                if (!details.Any())
                {
                    await transaction.RollbackAsync();
                    return ServiceResult<MaterialIssue>.Failure("領料單沒有明細資料，無法扣除庫存");
                }

                // 3. 逐筆扣除庫存
                foreach (var detail in details)
                {
                    // 扣除庫存（ReduceStockAsync 內部已經會建立庫存交易記錄）
                    var reduceResult = await _inventoryStockService.ReduceStockAsync(
                        detail.ProductId,
                        detail.WarehouseId,
                        detail.IssueQuantity,
                        InventoryTransactionTypeEnum.MaterialIssue,
                        materialIssue.Code ?? $"MI-{materialIssue.Id}",
                        detail.WarehouseLocationId,
                        $"領料單號：{materialIssue.Code}");

                    if (!reduceResult.IsSuccess)
                    {
                        await transaction.RollbackAsync();
                        return ServiceResult<MaterialIssue>.Failure($"扣除庫存失敗：{reduceResult.ErrorMessage}");
                    }
                }

                await transaction.CommitAsync();
                return ServiceResult<MaterialIssue>.Success(materialIssue);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateAsync), GetType(), _logger, new
                {
                    Method = nameof(CreateAsync),
                    ServiceType = GetType().Name,
                    MaterialIssueId = entity.Id,
                    Code = entity.Code
                });
                return ServiceResult<MaterialIssue>.Failure($"建立領料單並扣除庫存時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 更新領料單（使用差異化調整庫存）
        /// </summary>
        public override async Task<ServiceResult<MaterialIssue>> UpdateAsync(MaterialIssue entity)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // 1. 執行基本的更新邏輯
                var updateResult = await base.UpdateAsync(entity);
                if (!updateResult.IsSuccess || updateResult.Data == null)
                {
                    return updateResult;
                }

                var materialIssue = updateResult.Data;

                // 2. 取得更新後的明細資料
                var details = await context.MaterialIssueDetails
                    .Where(d => d.MaterialIssueId == entity.Id && d.Status == EntityStatus.Active)
                    .ToListAsync();

                // 3. 逐筆處理庫存差異（只調整變化的部分）
                foreach (var detail in details)
                {
                    var result = await UpdateInventoryByDifferenceAsync(context, materialIssue, detail, detail.IssueQuantity);
                    if (!result.IsSuccess)
                    {
                        await transaction.RollbackAsync();
                        return ServiceResult<MaterialIssue>.Failure(result.ErrorMessage);
                    }
                }

                await transaction.CommitAsync();
                return ServiceResult<MaterialIssue>.Success(updateResult.Data);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateAsync), GetType(), _logger, new
                {
                    Method = nameof(UpdateAsync),
                    ServiceType = GetType().Name,
                    MaterialIssueId = entity.Id,
                    Code = entity.Code
                });
                return ServiceResult<MaterialIssue>.Failure($"更新領料單並處理庫存時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 刪除領料單並還原庫存（使用 _DEL 批次邊界標記）
        /// </summary>
        public override async Task<ServiceResult> DeleteAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // 1. 取得領料單及明細
                var materialIssue = await context.MaterialIssues
                    .Include(mi => mi.MaterialIssueDetails)
                    .FirstOrDefaultAsync(mi => mi.Id == id);

                if (materialIssue == null)
                {
                    return ServiceResult.Failure("找不到指定的領料單");
                }

                // 2. 還原庫存（使用 MaterialReturn 類型和 _DEL 後綴）
                var details = materialIssue.MaterialIssueDetails
                    .Where(d => d.Status == EntityStatus.Active)
                    .ToList();

                foreach (var detail in details)
                {
                    // 使用 AddStockAsync 增加庫存（回沖）
                    var addResult = await _inventoryStockService.AddStockAsync(
                        productId: detail.ProductId,
                        warehouseId: detail.WarehouseId,
                        quantity: detail.IssueQuantity,
                        transactionType: InventoryTransactionTypeEnum.MaterialReturn,
                        transactionNumber: $"{materialIssue.Code}_DEL",  // 使用 _DEL 後綴作為批次邊界
                        unitCost: detail.UnitCost,
                        locationId: detail.WarehouseLocationId,
                        remarks: $"刪除領料單 {materialIssue.Code}，回沖庫存");

                    if (!addResult.IsSuccess)
                    {
                        await transaction.RollbackAsync();
                        return ServiceResult.Failure($"還原庫存失敗：{addResult.ErrorMessage}");
                    }
                }

                // 3. 執行軟刪除
                var deleteResult = await base.DeleteAsync(id);
                if (!deleteResult.IsSuccess)
                {
                    await transaction.RollbackAsync();
                    return deleteResult;
                }

                await transaction.CommitAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeleteAsync), GetType(), _logger, new
                {
                    Method = nameof(DeleteAsync),
                    ServiceType = GetType().Name,
                    MaterialIssueId = id
                });
                return ServiceResult.Failure($"刪除領料單並還原庫存時發生錯誤：{ex.Message}");
            }
        }

        public override async Task<List<MaterialIssue>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MaterialIssues
                    .Include(mi => mi.Employee)
                    .Include(mi => mi.Department)
                    .Include(mi => mi.MaterialIssueDetails)
                    .OrderByDescending(mi => mi.IssueDate)
                    .ThenByDescending(mi => mi.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new
                {
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name
                });
                return new List<MaterialIssue>();
            }
        }

        public override async Task<MaterialIssue?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MaterialIssues
                    .Include(mi => mi.Employee)
                    .Include(mi => mi.Department)
                    .Include(mi => mi.MaterialIssueDetails)
                        .ThenInclude(d => d.Product)
                    .Include(mi => mi.MaterialIssueDetails)
                        .ThenInclude(d => d.Warehouse)
                    .Include(mi => mi.MaterialIssueDetails)
                        .ThenInclude(d => d.WarehouseLocation)
                    .FirstOrDefaultAsync(mi => mi.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByIdAsync),
                    ServiceType = GetType().Name,
                    Id = id
                });
                return null;
            }
        }

        public override async Task<List<MaterialIssue>> SearchAsync(string searchTerm)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                return await context.MaterialIssues
                    .Include(mi => mi.Employee)
                    .Include(mi => mi.Department)
                    .Include(mi => mi.MaterialIssueDetails)
                    .Where(mi => (mi.Code != null && mi.Code.Contains(searchTerm)) ||
                                (mi.Employee != null && mi.Employee.Name != null && mi.Employee.Name.Contains(searchTerm)) ||
                                (mi.Department != null && mi.Department.Name != null && mi.Department.Name.Contains(searchTerm)) ||
                                (mi.Remarks != null && mi.Remarks.Contains(searchTerm)))
                    .OrderByDescending(mi => mi.IssueDate)
                    .ThenByDescending(mi => mi.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new
                {
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm
                });
                return new List<MaterialIssue>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(MaterialIssue entity)
        {
            try
            {
                var errors = new List<string>();

                // 驗證領貨單號 (Code)
                if (string.IsNullOrWhiteSpace(entity.Code))
                {
                    errors.Add("領貨單號為必填欄位");
                }
                else
                {
                    // 檢查單號唯一性
                    var isDuplicate = await IsMaterialIssueCodeExistsAsync(entity.Code, entity.Id == 0 ? null : entity.Id);
                    if (isDuplicate)
                    {
                        errors.Add("領貨單號已存在");
                    }
                }

                // 驗證領貨日期
                if (entity.IssueDate == default)
                {
                    errors.Add("領貨日期為必填欄位");
                }

                // 驗證明細
                if (entity.Id == 0 && (entity.MaterialIssueDetails == null || !entity.MaterialIssueDetails.Any()))
                {
                    errors.Add("至少需要一筆領貨明細");
                }

                if (errors.Any())
                    return ServiceResult.Failure(string.Join("; ", errors));

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new
                {
                    Method = nameof(ValidateAsync),
                    ServiceType = GetType().Name,
                    EntityId = entity.Id,
                    Code = entity.Code
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        #endregion

        #region 庫存差異處理

        /// <summary>
        /// 根據明細差異更新庫存（只調整變化的部分，支援有效批次追蹤）
        /// </summary>
        private async Task<ServiceResult> UpdateInventoryByDifferenceAsync(
            AppDbContext context,
            MaterialIssue currentMaterialIssue,
            MaterialIssueDetail detail,
            decimal targetQuantity)
        {
            try
            {
                // 1. 查詢所有相關交易記錄（包含無後綴、_ADJ、_DEL）
                var allTransactions = await context.InventoryTransactions
                    .Where(t => t.TransactionNumber == currentMaterialIssue.Code ||
                                t.TransactionNumber.StartsWith(currentMaterialIssue.Code + "_"))
                    .OrderBy(t => t.TransactionDate).ThenBy(t => t.Id)
                    .ToListAsync();

                // 2. 找到最後一次刪除的批次邊界
                var lastDeleteTransaction = allTransactions
                    .Where(t => t.TransactionNumber.EndsWith("_DEL"))
                    .OrderByDescending(t => t.TransactionDate).ThenByDescending(t => t.Id)
                    .FirstOrDefault();

                // 3. 只統計最後刪除之後的有效記錄（針對當前產品和倉庫）
                var existingTransactions = lastDeleteTransaction != null
                    ? allTransactions.Where(t => t.Id > lastDeleteTransaction.Id &&
                                                 !t.TransactionNumber.EndsWith("_DEL") &&
                                                 t.ProductId == detail.ProductId &&
                                                 t.WarehouseId == detail.WarehouseId).ToList()
                    : allTransactions.Where(t => !t.TransactionNumber.EndsWith("_DEL") &&
                                                 t.ProductId == detail.ProductId &&
                                                 t.WarehouseId == detail.WarehouseId).ToList();

                // 4. 計算已處理數量（累加所有交易的數量）
                var processedQuantity = existingTransactions.Sum(t => t.Quantity);

                // 5. 計算差異
                var quantityDiff = targetQuantity - processedQuantity;

                // 6. 只調整差異部分
                if (quantityDiff != 0)
                {
                    if (quantityDiff > 0)
                    {
                        // 需要增加領料（減少庫存）
                        var reduceResult = await _inventoryStockService.ReduceStockAsync(
                            productId: detail.ProductId,
                            warehouseId: detail.WarehouseId,
                            quantity: (int)quantityDiff,
                            transactionType: InventoryTransactionTypeEnum.MaterialIssue,
                            transactionNumber: $"{currentMaterialIssue.Code}_ADJ",
                            locationId: detail.WarehouseLocationId,
                            remarks: $"編輯領料單 {currentMaterialIssue.Code}，增加領料 {quantityDiff}");
                        
                        if (!reduceResult.IsSuccess)
                        {
                            return ServiceResult.Failure($"減少庫存失敗：{reduceResult.ErrorMessage}");
                        }
                    }
                    else
                    {
                        // 需要減少領料（增加庫存）
                        var addResult = await _inventoryStockService.AddStockAsync(
                            productId: detail.ProductId,
                            warehouseId: detail.WarehouseId,
                            quantity: (int)Math.Abs(quantityDiff),
                            transactionType: InventoryTransactionTypeEnum.MaterialReturn,
                            transactionNumber: $"{currentMaterialIssue.Code}_ADJ",
                            unitCost: detail.UnitCost,
                            locationId: detail.WarehouseLocationId,
                            remarks: $"編輯領料單 {currentMaterialIssue.Code}，減少領料 {Math.Abs(quantityDiff)}");
                        
                        if (!addResult.IsSuccess)
                        {
                            return ServiceResult.Failure($"增加庫存失敗：{addResult.ErrorMessage}");
                        }
                    }
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateInventoryByDifferenceAsync), GetType(), _logger, new
                {
                    Method = nameof(UpdateInventoryByDifferenceAsync),
                    ServiceType = GetType().Name,
                    MaterialIssueCode = currentMaterialIssue.Code,
                    ProductId = detail.ProductId,
                    TargetQuantity = targetQuantity
                });
                return ServiceResult.Failure($"更新庫存差異時發生錯誤：{ex.Message}");
            }
        }

        #endregion

        #region 業務邏輯方法

        public async Task<bool> IsMaterialIssueCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.MaterialIssues.Where(mi => mi.Code == code);

                if (excludeId.HasValue)
                    query = query.Where(mi => mi.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsMaterialIssueCodeExistsAsync), GetType(), _logger, new
                {
                    Method = nameof(IsMaterialIssueCodeExistsAsync),
                    ServiceType = GetType().Name,
                    Code = code,
                    ExcludeId = excludeId
                });
                return false;
            }
        }

        public async Task<string> GenerateIssueNumberAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var today = DateTime.Today;
                var prefix = $"MI{today:yyyyMMdd}";

                // 查詢今天最後一筆單號
                var lastIssue = await context.MaterialIssues
                    .Where(mi => mi.Code != null && mi.Code.StartsWith(prefix))
                    .OrderByDescending(mi => mi.Code)
                    .FirstOrDefaultAsync();

                int nextSequence = 1;
                if (lastIssue != null && !string.IsNullOrEmpty(lastIssue.Code))
                {
                    // 取得流水號部分並加1
                    var sequencePart = lastIssue.Code.Substring(prefix.Length);
                    if (int.TryParse(sequencePart, out int currentSequence))
                    {
                        nextSequence = currentSequence + 1;
                    }
                }

                // 格式: MI + yyyyMMdd + 4位流水號
                return $"{prefix}{nextSequence:D4}";
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GenerateIssueNumberAsync), GetType(), _logger, new
                {
                    Method = nameof(GenerateIssueNumberAsync),
                    ServiceType = GetType().Name
                });
                // 發生錯誤時返回預設單號
                return $"MI{DateTime.Now:yyyyMMddHHmmss}";
            }
        }

        public async Task<MaterialIssue?> GetWithDetailsAsync(int materialIssueId)
        {
            return await GetByIdAsync(materialIssueId);
        }

        public async Task<List<MaterialIssueDetail>> GetDetailsAsync(int materialIssueId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MaterialIssueDetails
                    .Include(d => d.Product)
                    .Include(d => d.Warehouse)
                    .Include(d => d.WarehouseLocation)
                    .Where(d => d.MaterialIssueId == materialIssueId)
                    .OrderBy(d => d.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetDetailsAsync), GetType(), _logger, new
                {
                    Method = nameof(GetDetailsAsync),
                    ServiceType = GetType().Name,
                    MaterialIssueId = materialIssueId
                });
                return new List<MaterialIssueDetail>();
            }
        }

        public async Task<List<MaterialIssue>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MaterialIssues
                    .Include(mi => mi.Employee)
                    .Include(mi => mi.Department)
                    .Include(mi => mi.MaterialIssueDetails)
                    .Where(mi => mi.IssueDate >= startDate && mi.IssueDate <= endDate)
                    .OrderByDescending(mi => mi.IssueDate)
                    .ThenByDescending(mi => mi.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByDateRangeAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByDateRangeAsync),
                    ServiceType = GetType().Name,
                    StartDate = startDate,
                    EndDate = endDate
                });
                return new List<MaterialIssue>();
            }
        }

        public async Task<List<MaterialIssue>> GetByEmployeeAsync(int employeeId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MaterialIssues
                    .Include(mi => mi.Employee)
                    .Include(mi => mi.Department)
                    .Include(mi => mi.MaterialIssueDetails)
                    .Where(mi => mi.EmployeeId == employeeId)
                    .OrderByDescending(mi => mi.IssueDate)
                    .ThenByDescending(mi => mi.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByEmployeeAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByEmployeeAsync),
                    ServiceType = GetType().Name,
                    EmployeeId = employeeId
                });
                return new List<MaterialIssue>();
            }
        }

        public async Task<List<MaterialIssue>> GetByDepartmentAsync(int departmentId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MaterialIssues
                    .Include(mi => mi.Employee)
                    .Include(mi => mi.Department)
                    .Include(mi => mi.MaterialIssueDetails)
                    .Where(mi => mi.DepartmentId == departmentId)
                    .OrderByDescending(mi => mi.IssueDate)
                    .ThenByDescending(mi => mi.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByDepartmentAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByDepartmentAsync),
                    ServiceType = GetType().Name,
                    DepartmentId = departmentId
                });
                return new List<MaterialIssue>();
            }
        }

        #endregion
    }
}
