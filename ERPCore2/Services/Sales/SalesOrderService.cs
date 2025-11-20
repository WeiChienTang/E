using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Models;
using ERPCore2.Services;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 銷貨訂單服務實作 - 繼承 GenericManagementService
    /// </summary>
    public class SalesOrderService : GenericManagementService<SalesOrder>, ISalesOrderService
    {
        private readonly IInventoryStockService _inventoryStockService;
        private readonly ISalesOrderDetailService? _detailService;
        private readonly ISalesReturnDetailService? _salesReturnDetailService;

        /// <summary>
        /// 完整建構子 - 使用 ILogger、InventoryStockService 和 SalesOrderDetailService
        /// </summary>
        public SalesOrderService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<SalesOrder>> logger,
            IInventoryStockService inventoryStockService,
            ISalesOrderDetailService? detailService = null,
            ISalesReturnDetailService? salesReturnDetailService = null) : base(contextFactory, logger)
        {
            _inventoryStockService = inventoryStockService;
            _detailService = detailService;
            _salesReturnDetailService = salesReturnDetailService;
        }

        /// <summary>
        /// 簡易建構子 - 不使用 ILogger 但需要 InventoryStockService
        /// </summary>
        public SalesOrderService(
            IDbContextFactory<AppDbContext> contextFactory,
            IInventoryStockService inventoryStockService) : base(contextFactory)
        {
            _inventoryStockService = inventoryStockService;
        }

        #region 覆寫基底方法

        public override async Task<List<SalesOrder>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesOrders
                    .Include(so => so.Customer)
                    .Include(so => so.Employee)
                    .AsQueryable()
                    .OrderByDescending(so => so.OrderDate)
                    .ThenBy(so => so.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new {
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name
                });
                return new List<SalesOrder>();
            }
        }

        public override async Task<SalesOrder?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesOrders
                    .Include(so => so.Customer)
                    .Include(so => so.Employee)
                    .Include(so => so.SalesOrderDetails)
                    .ThenInclude(sod => sod.Product)
                    .FirstOrDefaultAsync(so => so.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new {
                    Method = nameof(GetByIdAsync),
                    ServiceType = GetType().Name,
                    Id = id
                });
                return null;
            }
        }

        public override async Task<List<SalesOrder>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                var lowerSearchTerm = searchTerm.ToLower();

                return await context.SalesOrders
                    .Include(so => so.Customer)
                    .Include(so => so.Employee)
                    .Where(so => (
                        (so.Code != null && so.Code.ToLower().Contains(lowerSearchTerm)) ||
                        (so.Customer != null && so.Customer.CompanyName != null && so.Customer.CompanyName.ToLower().Contains(lowerSearchTerm))
                    ))
                    .OrderByDescending(so => so.OrderDate)
                    .ThenBy(so => so.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new {
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm
                });
                return new List<SalesOrder>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(SalesOrder entity)
        {
            try
            {
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(entity.Code))
                    errors.Add("銷貨單號不能為空");

                if (entity.CustomerId <= 0)
                    errors.Add("客戶為必選項目");

                if (entity.OrderDate == default)
                    errors.Add("訂單日期不能為空");

                if (!string.IsNullOrWhiteSpace(entity.Code) &&
                    await IsSalesOrderCodeExistsAsync(entity.Code, entity.Id == 0 ? null : entity.Id))
                    errors.Add("銷貨單號已存在");

                if (errors.Any())
                    return ServiceResult.Failure(string.Join("; ", errors));

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new {
                    Method = nameof(ValidateAsync),
                    ServiceType = GetType().Name,
                    EntityId = entity.Id,
                    EntityName = entity.Code
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        #endregion

        #region 自定義方法

        /// <summary>
        /// 檢查銷貨訂單代碼是否已存在（符合 EntityCodeGenerationHelper 約定）
        /// </summary>
        public async Task<bool> IsSalesOrderCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.SalesOrders.Where(so => so.Code == code);
                if (excludeId.HasValue)
                    query = query.Where(so => so.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsSalesOrderCodeExistsAsync), GetType(), _logger, new {
                    Method = nameof(IsSalesOrderCodeExistsAsync),
                    ServiceType = GetType().Name,
                    Code = code,
                    ExcludeId = excludeId
                });
                return false;
            }
        }

        public async Task<List<SalesOrder>> GetByCustomerIdAsync(int customerId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesOrders
                    .Include(so => so.Customer)
                    .Include(so => so.Employee)
                    .Where(so => so.CustomerId == customerId)
                    .OrderByDescending(so => so.OrderDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByCustomerIdAsync), GetType(), _logger, new {
                    Method = nameof(GetByCustomerIdAsync),
                    ServiceType = GetType().Name,
                    CustomerId = customerId
                });
                return new List<SalesOrder>();
            }
        }

        public async Task<List<SalesOrder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesOrders
                    .Include(so => so.Customer)
                    .Include(so => so.Employee)
                    .Where(so => so.OrderDate >= startDate && so.OrderDate <= endDate)
                    .OrderByDescending(so => so.OrderDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByDateRangeAsync), GetType(), _logger, new {
                    Method = nameof(GetByDateRangeAsync),
                    ServiceType = GetType().Name,
                    StartDate = startDate,
                    EndDate = endDate
                });
                return new List<SalesOrder>();
            }
        }

        public async Task<ServiceResult> CalculateOrderTotalAsync(int orderId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var order = await context.SalesOrders
                    .Include(so => so.SalesOrderDetails)
                    .FirstOrDefaultAsync(so => so.Id == orderId);

                if (order == null)
                    return ServiceResult.Failure("找不到指定的銷貨訂單");

                var totalAmount = order.SalesOrderDetails.Sum(d => d.SubtotalAmount);
                var taxAmount = totalAmount * 0.05m; // 假設稅率 5%
                var totalAmountWithTax = totalAmount + taxAmount;

                order.TotalAmount = totalAmount;
                order.SalesTaxAmount = taxAmount;
                order.UpdatedAt = DateTime.Now;

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CalculateOrderTotalAsync), GetType(), _logger, new {
                    Method = nameof(CalculateOrderTotalAsync),
                    ServiceType = GetType().Name,
                    OrderId = orderId
                });
                return ServiceResult.Failure("計算訂單總金額時發生錯誤");
            }
        }

        public async Task<SalesOrder?> GetWithDetailsAsync(int orderId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesOrders
                    .Include(so => so.Customer)
                    .Include(so => so.Employee)
                    .Include(so => so.SalesOrderDetails)
                        .ThenInclude(sod => sod.Product)
                    .Include(so => so.SalesOrderDetails)
                        .ThenInclude(sod => sod.Unit)
                    .FirstOrDefaultAsync(so => so.Id == orderId);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetWithDetailsAsync), GetType(), _logger, new {
                    Method = nameof(GetWithDetailsAsync),
                    ServiceType = GetType().Name,
                    OrderId = orderId
                });
                return null;
            }
        }

        /// <summary>
        /// 驗證銷貨訂單明細的倉庫選擇和庫存是否足夠
        /// </summary>
        /// <param name="salesOrderDetails">銷貨訂單明細清單</param>
        /// <returns>驗證結果，包含倉庫和庫存不足的詳細訊息</returns>
        public async Task<ServiceResult> ValidateWarehouseInventoryStockAsync(List<SalesOrderDetail> salesOrderDetails)
        {
            try
            {
                if (salesOrderDetails == null || !salesOrderDetails.Any())
                {
                    return ServiceResult.Success();
                }

                var errors = new List<string>();
                
                foreach (var detail in salesOrderDetails.Where(d => d.ProductId > 0 && d.OrderQuantity > 0))
                {
                    using var context = await _contextFactory.CreateDbContextAsync();
                    var product = await context.Products.FindAsync(detail.ProductId);
                    var productName = $"{product?.Code ?? "N/A"} - {product?.Name ?? "未知商品"}";
                    
                    // 1. 檢查是否選擇倉庫
                    if (!detail.WarehouseId.HasValue)
                    {
                        errors.Add($"{productName} 必須選擇倉庫");
                        continue;
                    }

                    // 2. 檢查指定倉庫的庫存（合併計算該倉庫內所有位置的庫存）
                    var availableStock = await _inventoryStockService.GetTotalAvailableStockByWarehouseAsync(
                        detail.ProductId, detail.WarehouseId.Value);
                        
                    if (availableStock < detail.OrderQuantity)
                    {
                        var warehouse = await context.Warehouses.FindAsync(detail.WarehouseId.Value);
                        var warehouseName = warehouse?.Name ?? "未知倉庫";
                        
                        errors.Add($"{productName} 在倉庫 {warehouseName} 庫存不足");
                        errors.Add($"可用庫存: {availableStock}，需要: {detail.OrderQuantity}");
                        errors.Add(""); // 空行分隔
                    }
                }
                
                return errors.Any() 
                    ? ServiceResult.Failure(string.Join("\n", errors).TrimEnd())
                    : ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateWarehouseInventoryStockAsync), GetType(), _logger, new {
                    Method = nameof(ValidateWarehouseInventoryStockAsync),
                    ServiceType = GetType().Name,
                    DetailCount = salesOrderDetails?.Count ?? 0
                });
                return ServiceResult.Failure("驗證倉庫庫存時發生錯誤");
            }
        }

        #endregion

        #region 刪除限制檢查

        /// <summary>
        /// 檢查銷貨訂單是否可以被刪除
        /// 如果訂單的任何明細被鎖定（有退貨記錄或收款記錄），則整個訂單都不能刪除
        /// </summary>
        protected override async Task<ServiceResult> CanDeleteAsync(SalesOrder entity)
        {
            try
            {
                // 1. 基礎檢查（外鍵關聯等）
                var baseResult = await base.CanDeleteAsync(entity);
                if (!baseResult.IsSuccess)
                    return baseResult;

                // 2. 載入訂單明細（含商品資訊）
                using var context = await _contextFactory.CreateDbContextAsync();
                var orderWithDetails = await context.SalesOrders
                    .Include(so => so.SalesOrderDetails)
                        .ThenInclude(sod => sod.Product)
                    .FirstOrDefaultAsync(so => so.Id == entity.Id);

                if (orderWithDetails == null || orderWithDetails.SalesOrderDetails == null || !orderWithDetails.SalesOrderDetails.Any())
                {
                    return ServiceResult.Success(); // 沒有明細，可以刪除
                }

                // 3. 檢查每個明細項目
                foreach (var detail in orderWithDetails.SalesOrderDetails)
                {
                    // 檢查 1：退貨記錄檢查
                    if (_salesReturnDetailService != null)
                    {
                        var returnDetails = await _salesReturnDetailService.GetBySalesOrderDetailIdAsync(detail.Id);
                        if (returnDetails != null && returnDetails.Any())
                        {
                            var totalReturnQuantity = returnDetails.Sum(rd => rd.ReturnQuantity);
                            var productName = detail.Product?.Name ?? "未知商品";
                            return ServiceResult.Failure(
                                $"無法刪除此銷貨訂單，因為商品「{productName}」已有退貨記錄（已退貨 {totalReturnQuantity} 個）");
                        }
                    }

                    // 檢查 2：收款記錄檢查
                    if (detail.TotalReceivedAmount > 0)
                    {
                        var productName = detail.Product?.Name ?? "未知商品";
                        return ServiceResult.Failure(
                            $"無法刪除此銷貨訂單，因為商品「{productName}」已有收款記錄（已收款 {detail.TotalReceivedAmount:N2} 元）");
                    }
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteAsync), GetType(), _logger, new
                {
                    Method = nameof(CanDeleteAsync),
                    ServiceType = GetType().Name,
                    EntityId = entity.Id,
                    SalesOrderNumber = entity.Code
                });
                return ServiceResult.Failure("檢查刪除權限時發生錯誤");
            }
        }

        #endregion

        #region 覆寫刪除方法 - 含庫存回滾機制

        /// <summary>
        /// 覆寫刪除方法 - 刪除主檔時同步回退庫存
        /// 功能：刪除銷貨訂單時，自動回退已扣減的庫存數量
        /// 處理流程：
        /// 1. 驗證訂單存在性
        /// 2. 查找相關庫存交易記錄
        /// 3. 進行庫存回退操作
        /// 4. 執行原本的軟刪除（主檔）
        /// 5. 使用資料庫交易確保資料一致性
        /// 6. 任何步驟失敗時回滾所有變更
        /// </summary>
        /// <param name="id">要刪除的銷貨訂單ID</param>
        /// <returns>刪除結果，包含成功狀態及錯誤訊息</returns>
        public override async Task<ServiceResult> PermanentDeleteAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();
                
                try
                {
                    // 1. 先取得主記錄（含詳細資料，包含明細關聯）
                    var entity = await context.SalesOrders
                        .Include(so => so.SalesOrderDetails)
                            .ThenInclude(sod => sod.Product)
                        .FirstOrDefaultAsync(x => x.Id == id);
                        
                    if (entity == null)
                    {
                        return ServiceResult.Failure("找不到要刪除的資料");
                    }
                    
                    // 2. 檢查是否可以刪除
                    var canDeleteResult = await CanDeleteAsync(entity);
                    if (!canDeleteResult.IsSuccess)
                    {
                        return canDeleteResult;
                    }
                    
                    // 3. 💡 銷貨訂單不影響庫存，無需回滾庫存
                    // 庫存扣減應發生在「銷貨出貨單」階段，而非「銷貨訂單」階段

                    // 4. 更新報價單明細的已轉銷貨數量，並清空報價單主檔的轉單關聯
                    var detailsToDelete = await context.SalesOrderDetails
                        .Where(sod => sod.SalesOrderId == id)
                        .ToListAsync();
                        
                    if (detailsToDelete.Any())
                    {
                        // 收集所有相關的報價單ID（用於後續清空轉單關聯）
                        var relatedQuotationIds = new HashSet<int>();
                        
                        // 扣減報價單明細的已轉銷貨數量
                        foreach (var detail in detailsToDelete)
                        {
                            if (detail.QuotationDetailId.HasValue)
                            {
                                var quotationDetail = await context.QuotationDetails.FindAsync(detail.QuotationDetailId.Value);
                                if (quotationDetail != null)
                                {
                                    quotationDetail.ConvertedQuantity -= detail.OrderQuantity;
                                    
                                    // 確保不會變成負數
                                    if (quotationDetail.ConvertedQuantity < 0)
                                        quotationDetail.ConvertedQuantity = 0;
                                    
                                    quotationDetail.UpdatedAt = DateTime.Now;
                                    
                                    // 記錄相關的報價單ID
                                    relatedQuotationIds.Add(quotationDetail.QuotationId);
                                }
                            }
                        }
                        
                        // 不需要清空報價單的轉單關聯（已移除 ConvertedToSalesOrderId 欄位）
                        // 銷貨訂單透過 QuotationId 關聯到報價單，刪除訂單時會自動解除關聯
                        
                        context.SalesOrderDetails.RemoveRange(detailsToDelete);
                    }

                    // 5. 永久刪除主檔
                    context.SalesOrders.Remove(entity);

                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    
                    return ServiceResult.Success();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(PermanentDeleteAsync), GetType(), _logger, new { 
                    Method = nameof(PermanentDeleteAsync),
                    ServiceType = GetType().Name,
                    Id = id 
                });
                return ServiceResult.Failure("永久刪除銷貨訂單過程發生錯誤");
            }
        }

        #endregion

        #region 出貨明細查詢

        /// <summary>
        /// 取得客戶的出貨明細（可篩選是否包含已完成和是否檢查審核）
        /// 此方法用於載入該客戶所有未完成出貨的銷貨訂單明細
        /// </summary>
        /// <param name="customerId">客戶ID</param>
        /// <param name="includeCompleted">是否包含已完成的明細</param>
        /// <param name="checkApproval">是否檢查審核狀態（銷貨訂單無審核機制，此參數保留以保持介面一致性）</param>
        public async Task<List<SalesOrderDetail>> GetDeliveryDetailsByCustomerAsync(int customerId, bool includeCompleted, bool checkApproval = true)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var query = context.SalesOrderDetails
                    .Include(sod => sod.SalesOrder)
                        .ThenInclude(so => so.Customer)
                    .Include(sod => sod.Product)
                    .Include(sod => sod.Warehouse)
                    .Where(sod => sod.SalesOrder.CustomerId == customerId);

                // 注意：銷貨訂單沒有審核機制，checkApproval 參數保留但不使用
                // 此參數僅為保持與採購訂單 API 一致性

                if (!includeCompleted)
                {
                    // 只包含未完成的明細：既未結清，且數量未滿
                    query = query.Where(sod => !sod.IsSettled && sod.DeliveredQuantity < sod.OrderQuantity);
                }

                return await query.OrderBy(sod => sod.SalesOrder.Code)
                                .ThenBy(sod => sod.Product.Name)
                                .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetDeliveryDetailsByCustomerAsync), GetType(), _logger, new { 
                    CustomerId = customerId,
                    IncludeCompleted = includeCompleted,
                    CheckApproval = checkApproval
                });
                return new List<SalesOrderDetail>();
            }
        }

        #endregion

        #region 批次列印查詢

        /// <summary>
        /// 根據批次列印條件查詢銷貨訂單（批次列印專用）
        /// 設計理念：靈活組合日期、客戶、狀態等多種篩選條件，適用於批次列印場景
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <returns>符合條件的銷貨訂單列表（包含完整關聯資料）</returns>
        public async Task<List<SalesOrder>> GetByBatchCriteriaAsync(BatchPrintCriteria criteria)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 建立基礎查詢（包含必要關聯資料）
                IQueryable<SalesOrder> query = context.SalesOrders
                    .Include(so => so.Customer)
                    .Include(so => so.Employee)
                    .Include(so => so.SalesOrderDetails)
                        .ThenInclude(sod => sod.Product)
                    .AsQueryable();

                // 日期範圍篩選
                if (criteria.StartDate.HasValue)
                {
                    query = query.Where(so => so.OrderDate >= criteria.StartDate.Value.Date);
                }
                if (criteria.EndDate.HasValue)
                {
                    var endDate = criteria.EndDate.Value.Date.AddDays(1);
                    query = query.Where(so => so.OrderDate < endDate);
                }

                // 關聯實體篩選（客戶）
                if (criteria.RelatedEntityIds != null && criteria.RelatedEntityIds.Any())
                {
                    query = query.Where(so => criteria.RelatedEntityIds.Contains(so.CustomerId));
                }

                // 公司篩選（如果需要）
                if (criteria.CompanyId.HasValue)
                {
                    // 銷貨訂單可能沒有 CompanyId，視實際需求調整
                    // query = query.Where(so => so.CompanyId == criteria.CompanyId.Value);
                }

                // 單據編號關鍵字搜尋
                if (!string.IsNullOrWhiteSpace(criteria.DocumentNumberKeyword))
                {
                    query = query.Where(so => so.Code != null && so.Code.Contains(criteria.DocumentNumberKeyword));
                }

                // 是否包含已取消的單據
                if (!criteria.IncludeCancelled)
                {
                    // 銷貨訂單可能沒有取消狀態欄位，視實際需求調整
                    // query = query.Where(so => !so.IsCancelled);
                }

                // 排序：先按客戶分組，同客戶內再按日期和單據編號排序
                query = criteria.SortDirection == Models.SortDirection.Ascending
                    ? query.OrderBy(so => so.Customer.CompanyName)
                           .ThenBy(so => so.OrderDate)
                           .ThenBy(so => so.Code)
                    : query.OrderBy(so => so.Customer.CompanyName)
                           .ThenByDescending(so => so.OrderDate)
                           .ThenBy(so => so.Code);

                // 限制最大筆數
                if (criteria.MaxResults.HasValue && criteria.MaxResults.Value > 0)
                {
                    query = query.Take(criteria.MaxResults.Value);
                }

                // 執行查詢
                var results = await query.ToListAsync();

                return results;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByBatchCriteriaAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByBatchCriteriaAsync),
                    ServiceType = GetType().Name,
                    Criteria = new
                    {
                        criteria.StartDate,
                        criteria.EndDate,
                        RelatedEntityCount = criteria.RelatedEntityIds?.Count ?? 0,
                        criteria.MaxResults
                    }
                });
                return new List<SalesOrder>();
            }
        }

        #endregion
    }
}

