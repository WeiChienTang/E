using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Helpers;
using ERPCore2.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 採購訂單服務實作
    /// </summary>
    public class PurchaseOrderService : GenericManagementService<PurchaseOrder>, IPurchaseOrderService
    {
        private readonly IPurchaseOrderDetailService _purchaseOrderDetailService;
        private readonly ISystemParameterService _systemParameterService;

        public PurchaseOrderService(
            IDbContextFactory<AppDbContext> contextFactory, 
            IPurchaseOrderDetailService purchaseOrderDetailService,
            ISystemParameterService systemParameterService) : base(contextFactory)
        {
            _purchaseOrderDetailService = purchaseOrderDetailService;
            _systemParameterService = systemParameterService;
        }

        public PurchaseOrderService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<PurchaseOrder>> logger,
            IPurchaseOrderDetailService purchaseOrderDetailService,
            ISystemParameterService systemParameterService) : base(contextFactory, logger)
        {
            _purchaseOrderDetailService = purchaseOrderDetailService;
            _systemParameterService = systemParameterService;
        }

        // 採購服務專注於採購流程管理，不處理庫存邏輯
        // 庫存管理由專門的庫存服務負責

        #region 採購訂單管理方法

        #region 覆寫基本方法

        public override async Task<List<PurchaseOrder>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseOrders
                    .Include(po => po.Company)
                    .Include(po => po.Supplier)
                    .Include(po => po.Warehouse)
                    .Include(po => po.ApprovedByUser)
                    .Include(po => po.PurchaseOrderDetails)
                        .ThenInclude(pod => pod.Product)
                    // 不在此處排序 - 排序由 FieldConfiguration 層統一處理
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new { 
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name 
                });
                return new List<PurchaseOrder>();
            }
        }

        public override async Task<PurchaseOrder?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseOrders
                    .Include(po => po.Company)
                    .Include(po => po.Supplier)
                    .Include(po => po.Warehouse)
                    .Include(po => po.ApprovedByUser)
                    // 移除 PurchaseOrderDetails Include - 如需要明細資料應透過 DetailService 取得
                    .Include(po => po.PurchaseReceivings)
                        .ThenInclude(pr => pr.PurchaseReceivingDetails)
                    .FirstOrDefaultAsync(po => po.Id == id);
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

        public override async Task<List<PurchaseOrder>> SearchAsync(string searchTerm)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseOrders
                    .Include(po => po.Company)
                    .Include(po => po.Supplier)
                    .Include(po => po.Warehouse)
                    .Where(po => ((po.Code != null && po.Code.Contains(searchTerm)) ||
                                (po.Supplier != null && po.Supplier.CompanyName.Contains(searchTerm)) ||
                                (po.Company != null && po.Company.CompanyName.Contains(searchTerm)) ||
                                (po.PurchasePersonnel != null && po.PurchasePersonnel.Contains(searchTerm)) ||
                                (po.Remarks != null && po.Remarks.Contains(searchTerm))))
                    .OrderByDescending(po => po.OrderDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { 
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm 
                });
                return new List<PurchaseOrder>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(PurchaseOrder entity)
        {
            try
            {
                var errors = new List<string>();
                
                if (string.IsNullOrWhiteSpace(entity.Code))
                    errors.Add("採購單號不能為空");
                
                if (entity.SupplierId <= 0)
                    errors.Add("必須選擇廠商");
                
                if (entity.CompanyId <= 0)
                    errors.Add("必須選擇採購公司");
                
                if (entity.OrderDate > DateTime.Today.AddDays(1))
                    errors.Add("訂單日期不能超過明天");
                
                if (entity.ExpectedDeliveryDate.HasValue && entity.ExpectedDeliveryDate.Value < entity.OrderDate)
                    errors.Add("預計到貨日期不能早於訂單日期");
                
                // 檢查採購單號是否重複
                if (!string.IsNullOrWhiteSpace(entity.Code))
                {
                    using var context = await _contextFactory.CreateDbContextAsync();
                    var exists = await context.PurchaseOrders
                        .AnyAsync(po => po.Code == entity.Code && 
                                       po.Id != entity.Id);
                    if (exists)
                        errors.Add("採購單號已存在");
                }
                
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
                    OrderNumber = entity.Code 
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        #endregion

        #region 基本查詢

        public async Task<List<PurchaseOrder>> GetBySupplierIdAsync(int supplierId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseOrders
                    .Include(po => po.Supplier)
                    .Include(po => po.Warehouse)
                    // 移除 PurchaseOrderDetails Include - 如需要明細資料應透過 DetailService 取得
                    .Where(po => po.SupplierId == supplierId)
                    .OrderByDescending(po => po.OrderDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySupplierIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetBySupplierIdAsync),
                    ServiceType = GetType().Name,
                    SupplierId = supplierId 
                });
                return new List<PurchaseOrder>();
            }
        }

        public async Task<List<PurchaseOrder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseOrders
                    .Include(po => po.Supplier)
                    .Include(po => po.Warehouse)
                    .Where(po => po.OrderDate >= startDate && 
                               po.OrderDate <= endDate)
                    .OrderByDescending(po => po.OrderDate)
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
                return new List<PurchaseOrder>();
            }
        }

        public async Task<PurchaseOrder?> GetByNumberAsync(string purchaseOrderNumber)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseOrders
                    .Include(po => po.Supplier)
                    .Include(po => po.Warehouse)
                    // 移除 PurchaseOrderDetails Include - 如需要明細資料應透過 DetailService 取得
                    .FirstOrDefaultAsync(po => po.Code == purchaseOrderNumber);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByNumberAsync), GetType(), _logger, new { 
                    Method = nameof(GetByNumberAsync),
                    ServiceType = GetType().Name,
                    PurchaseOrderNumber = purchaseOrderNumber 
                });
                return null;
            }
        }

        /// <summary>
        /// 根據批次列印條件查詢採購單（支援多條件組合篩選）
        /// 設計理念：靈活組合日期、廠商、狀態等多種篩選條件，適用於批次列印場景
        /// </summary>
        /// <param name="criteria">批次列印篩選條件</param>
        /// <returns>符合條件的採購單列表（包含完整關聯資料）</returns>
        public async Task<List<PurchaseOrder>> GetByBatchCriteriaAsync(BatchPrintCriteria criteria)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 建立基礎查詢（包含必要的關聯資料）
                IQueryable<PurchaseOrder> query = context.PurchaseOrders
                    .Include(po => po.Company)
                    .Include(po => po.Supplier)
                    .Include(po => po.Warehouse)
                    .Include(po => po.ApprovedByUser)
                    .AsQueryable();

                // 日期範圍篩選
                if (criteria.StartDate.HasValue)
                {
                    query = query.Where(po => po.OrderDate >= criteria.StartDate.Value.Date);
                }
                if (criteria.EndDate.HasValue)
                {
                    // 包含整天（到當天 23:59:59）
                    var endDate = criteria.EndDate.Value.Date.AddDays(1);
                    query = query.Where(po => po.OrderDate < endDate);
                }

                // 廠商篩選（RelatedEntityIds 對應廠商ID列表）
                if (criteria.RelatedEntityIds != null && criteria.RelatedEntityIds.Any())
                {
                    query = query.Where(po => criteria.RelatedEntityIds.Contains(po.SupplierId));
                }

                // 公司篩選
                if (criteria.CompanyId.HasValue)
                {
                    query = query.Where(po => po.CompanyId == criteria.CompanyId.Value);
                }

                // 倉庫篩選
                if (criteria.WarehouseId.HasValue)
                {
                    query = query.Where(po => po.WarehouseId == criteria.WarehouseId.Value);
                }

                // 狀態篩選（採購單使用 IsApproved 等布林值來表示狀態）
                if (criteria.Statuses != null && criteria.Statuses.Any())
                {
                    // 支援的狀態：Pending（待審核）、Approved（已審核）、Rejected（已駁回）
                    foreach (var status in criteria.Statuses)
                    {
                        switch (status.ToLower())
                        {
                            case "pending":
                            case "待審核":
                                query = query.Where(po => !po.IsApproved && string.IsNullOrEmpty(po.RejectReason));
                                break;
                            case "approved":
                            case "已審核":
                                query = query.Where(po => po.IsApproved);
                                break;
                            case "rejected":
                            case "已駁回":
                                query = query.Where(po => !string.IsNullOrEmpty(po.RejectReason));
                                break;
                        }
                    }
                }

                // 單據編號關鍵字搜尋
                if (!string.IsNullOrWhiteSpace(criteria.DocumentNumberKeyword))
                {
                    query = query.Where(po => po.Code != null && po.Code.Contains(criteria.DocumentNumberKeyword));
                }

                // 是否包含已駁回的單據（預設不包含）
                if (!criteria.IncludeCancelled)
                {
                    query = query.Where(po => string.IsNullOrEmpty(po.RejectReason));
                }

                // 排序：先按廠商分組，同廠商內再按日期和單據編號排序
                // 這樣列印時同一廠商的採購單會集中在一起
                query = criteria.SortDirection == SortDirection.Ascending
                    ? query.OrderBy(po => po.Supplier.CompanyName)
                           .ThenBy(po => po.OrderDate)
                           .ThenBy(po => po.Code)
                    : query.OrderBy(po => po.Supplier.CompanyName)
                           .ThenByDescending(po => po.OrderDate)
                           .ThenBy(po => po.Code);

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
                        SupplierCount = criteria.RelatedEntityIds?.Count ?? 0,
                        criteria.CompanyId,
                        criteria.WarehouseId,
                        StatusCount = criteria.Statuses?.Count ?? 0,
                        criteria.DocumentNumberKeyword,
                        criteria.MaxResults,
                        criteria.IncludeCancelled
                    }
                });
                return new List<PurchaseOrder>();
            }
        }

        #endregion

        #region 訂單操作

        public async Task<ServiceResult> SubmitOrderAsync(int orderId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var order = await context.PurchaseOrders
                    .FirstOrDefaultAsync(po => po.Id == orderId);
                
                if (order == null)
                    return ServiceResult.Failure("找不到採購訂單");
                
                // 使用 DetailService 檢查是否有明細
                var details = await _purchaseOrderDetailService.GetByPurchaseOrderIdAsync(orderId);
                if (!details.Any())
                    return ServiceResult.Failure("訂單必須包含至少一項商品");
                
                order.UpdatedAt = DateTime.Now;
                
                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SubmitOrderAsync), GetType(), _logger, new { 
                    Method = nameof(SubmitOrderAsync),
                    ServiceType = GetType().Name,
                    OrderId = orderId 
                });
                return ServiceResult.Failure("送出訂單時發生錯誤");
            }
        }

        public async Task<ServiceResult> ApproveOrderAsync(int orderId, int approvedBy)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    var order = await context.PurchaseOrders
                        .FirstOrDefaultAsync(po => po.Id == orderId);
                    
                    if (order == null)
                        return ServiceResult.Failure("找不到採購訂單");
                    
                    if (order.IsApproved)
                        return ServiceResult.Failure("採購訂單已經核准，無需重複核准");

                    // 使用 DetailService 檢查是否有明細
                    var details = await _purchaseOrderDetailService.GetByPurchaseOrderIdAsync(orderId);
                    if (!details.Any())
                        return ServiceResult.Failure("採購訂單沒有有效的商品明細，無法核准");

                    // 更新採購訂單狀態
                    order.ApprovedBy = approvedBy;
                    order.ApprovedAt = DateTime.Now;
                    order.IsApproved = true;
                    order.UpdatedAt = DateTime.Now;
                    // TODO: 根據當前登入使用者設置 UpdatedBy
                    // order.UpdatedBy = currentUser.Name;
                    
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ApproveOrderAsync), GetType(), _logger, new { 
                    Method = nameof(ApproveOrderAsync),
                    ServiceType = GetType().Name,
                    OrderId = orderId,
                    ApprovedBy = approvedBy 
                });
                return ServiceResult.Failure("核准訂單時發生錯誤");
            }
        }

        public async Task<ServiceResult> RejectOrderAsync(int orderId, int rejectedBy, string reason)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    var order = await context.PurchaseOrders
                        .FirstOrDefaultAsync(po => po.Id == orderId);
                    
                    if (order == null)
                        return ServiceResult.Failure("找不到採購訂單");
                    
                    // 注釋：已移除 ReceivedAmount 欄位，如需檢查進貨狀況，請透過 GetStatisticsAsync 方法

                    // 重置審核狀態
                    order.IsApproved = false;
                    order.ApprovedBy = null;
                    order.ApprovedAt = null;
                    
                    // 設定駁回原因
                    order.RejectReason = reason;
                    
                    order.UpdatedAt = DateTime.Now;
                    // TODO: 根據當前登入使用者設置 UpdatedBy
                    // order.UpdatedBy = currentUser.Name;
                    
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(RejectOrderAsync), GetType(), _logger, new { 
                    Method = nameof(RejectOrderAsync),
                    ServiceType = GetType().Name,
                    OrderId = orderId,
                    RejectedBy = rejectedBy,
                    Reason = reason 
                });
                return ServiceResult.Failure("駁回訂單時發生錯誤");
            }
        }

        #endregion

        public async Task<ServiceResult> CancelOrderAsync(int orderId, string reason)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var order = await context.PurchaseOrders
                    .FirstOrDefaultAsync(po => po.Id == orderId);
                
                if (order == null)
                    return ServiceResult.Failure("找不到採購訂單");
                
                // 注釋：已移除 ReceivedAmount 欄位，如需檢查進貨狀況，請透過 GetStatisticsAsync 方法
                
                order.Remarks = string.IsNullOrWhiteSpace(order.Remarks) 
                    ? $"取消原因：{reason}" 
                    : $"{order.Remarks}\n取消原因：{reason}";
                order.UpdatedAt = DateTime.Now;
                
                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CancelOrderAsync), GetType(), _logger, new { 
                    Method = nameof(CancelOrderAsync),
                    ServiceType = GetType().Name,
                    OrderId = orderId 
                });
                return ServiceResult.Failure("關閉訂單時發生錯誤");
            }
        }



        public async Task<ServiceResult> CloseOrderAsync(int orderId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var order = await context.PurchaseOrders
                    .FirstOrDefaultAsync(po => po.Id == orderId);
                
                if (order == null)
                    return ServiceResult.Failure("找不到採購訂單");
                
                // 移除狀態檢查，直接更新
                order.UpdatedAt = DateTime.Now;
                
                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CloseOrderAsync), GetType(), _logger, new { 
                    Method = nameof(CloseOrderAsync),
                    ServiceType = GetType().Name,
                    OrderId = orderId 
                });
                return ServiceResult.Failure("關閉訂單時發生錯誤");
            }
        }

        #endregion

        #region 進貨相關

        // 注釋：UpdateReceivedAmountAsync 方法已被移除，因為 ReceivedAmount 欄位已不存在
        // 如需查詢進貨狀況，請使用 PurchaseOrderDetailService.GetStatisticsAsync 方法

        /// <summary>
        /// 檢查採購單是否可以刪除（介面方法）
        /// </summary>
        /// <param name="orderId">採購單ID</param>
        /// <returns>是否可以刪除</returns>
        public async Task<bool> CanDeleteAsync(int orderId)
        {
            try
            {
                var checkResult = await DependencyCheckHelper.CheckPurchaseOrderDependenciesAsync(_contextFactory, orderId);
                return checkResult.CanDelete;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteAsync), GetType(), _logger, new { 
                    Method = nameof(CanDeleteAsync),
                    ServiceType = GetType().Name,
                    OrderId = orderId 
                });
                return false;
            }
        }

        /// <summary>
        /// 檢查採購單是否可以刪除（基類覆寫方法）
        /// 檢查邏輯：
        /// 1. 先執行基類的刪除檢查（外鍵關聯等）
        /// 2. 檢查所有明細項目是否已有入庫記錄
        ///    - 檢查欄位：ReceivedQuantity (已進貨數量)
        ///    - 限制原因：已入庫的採購單不可刪除，以保持庫存資料一致性
        /// 注意：不再檢查「是否已核准」，改為檢查「是否已入庫」
        /// </summary>
        /// <param name="entity">採購單實體</param>
        /// <returns>刪除檢查結果</returns>
        protected override async Task<ServiceResult> CanDeleteAsync(PurchaseOrder entity)
        {
            try
            {
                // 1. 先檢查基類的刪除條件（外鍵關聯等）
                var baseResult = await base.CanDeleteAsync(entity);
                if (!baseResult.IsSuccess)
                {
                    return baseResult;
                }

                // 2. 載入明細資料（如果尚未載入）
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var loadedEntity = await context.PurchaseOrders
                    .Include(po => po.PurchaseOrderDetails)
                        .ThenInclude(pod => pod.Product)
                    .FirstOrDefaultAsync(po => po.Id == entity.Id);

                if (loadedEntity == null)
                {
                    return ServiceResult.Failure("找不到要檢查的採購單");
                }

                // 如果沒有明細，可以刪除
                if (loadedEntity.PurchaseOrderDetails == null || !loadedEntity.PurchaseOrderDetails.Any())
                {
                    return ServiceResult.Success();
                }

                // 3. 檢查每個明細項目是否已有入庫記錄
                foreach (var detail in loadedEntity.PurchaseOrderDetails)
                {
                    if (detail.ReceivedQuantity > 0)
                    {
                        var productName = detail.Product?.Name ?? "未知商品";
                        return ServiceResult.Failure(
                            $"無法刪除此採購單，因為商品「{productName}」已有入庫記錄（已入庫 {detail.ReceivedQuantity} 個）"
                        );
                    }
                }

                // 4. 檢查其他依賴關係
                var checkResult = await DependencyCheckHelper.CheckPurchaseOrderDependenciesAsync(_contextFactory, entity.Id);
                if (!checkResult.CanDelete)
                {
                    return ServiceResult.Failure(checkResult.GetFormattedErrorMessage("採購單"));
                }
                
                // 5. 所有檢查通過，允許刪除
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteAsync), GetType(), _logger, new { 
                    Method = nameof(CanDeleteAsync),
                    ServiceType = GetType().Name,
                    PurchaseOrderId = entity.Id 
                });
                return ServiceResult.Failure("檢查採購單刪除條件時發生錯誤");
            }
        }

        #endregion

        #region 統計查詢

        public async Task<List<PurchaseOrder>> GetPendingOrdersAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseOrders
                    .Include(po => po.Supplier)
                    .Include(po => po.Warehouse)
                    .Where(po => po.IsApproved) // 只包含已核准的訂單
                    // 注釋：已移除 ReceivedAmount 欄位，如需過濾進貨狀況，請在應用層使用 GetStatisticsAsync
                    .OrderBy(po => po.ExpectedDeliveryDate ?? po.OrderDate.AddDays(7))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPendingOrdersAsync), GetType(), _logger, new { 
                    Method = nameof(GetPendingOrdersAsync),
                    ServiceType = GetType().Name 
                });
                return new List<PurchaseOrder>();
            }
        }

        public async Task<List<PurchaseOrder>> GetOverdueOrdersAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var today = DateTime.Today;
                return await context.PurchaseOrders
                    .Include(po => po.Supplier)
                    .Include(po => po.Warehouse)
                    .Where(po => po.IsApproved && // 只包含已核准的訂單
                               // 注釋：已移除 ReceivedAmount 欄位，如需過濾進貨狀況，請在應用層使用 GetStatisticsAsync
                               po.ExpectedDeliveryDate.HasValue && 
                               po.ExpectedDeliveryDate.Value < today)
                    .OrderBy(po => po.ExpectedDeliveryDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetOverdueOrdersAsync), GetType(), _logger, new { 
                    Method = nameof(GetOverdueOrdersAsync),
                    ServiceType = GetType().Name 
                });
                return new List<PurchaseOrder>();
            }
        }

        public async Task<decimal> GetTotalOrderAmountAsync(int supplierId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.PurchaseOrders
                    .Where(po => po.SupplierId == supplierId);
                
                if (startDate.HasValue)
                    query = query.Where(po => po.OrderDate >= startDate.Value);
                
                if (endDate.HasValue)
                    query = query.Where(po => po.OrderDate <= endDate.Value);
                
                return await query.SumAsync(po => po.TotalAmount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetTotalOrderAmountAsync), GetType(), _logger, new { 
                    Method = nameof(GetTotalOrderAmountAsync),
                    ServiceType = GetType().Name,
                    SupplierId = supplierId,
                    StartDate = startDate,
                    EndDate = endDate 
                });
                return 0;
            }
        }

        #endregion

        #region 自動產生編號

        public async Task<string> GenerateOrderNumberAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var today = DateTime.Today;
                var prefix = $"PO{today:yyyyMMdd}";
                
                var lastOrder = await context.PurchaseOrders
                    .Where(po => po.Code != null && po.Code.StartsWith(prefix))
                    .OrderByDescending(po => po.Code)
                    .FirstOrDefaultAsync();
                
                if (lastOrder == null || lastOrder.Code == null)
                {
                    return $"{prefix}001";
                }
                
                var lastNumber = lastOrder.Code.Substring(prefix.Length);
                if (int.TryParse(lastNumber, out var number))
                {
                    return $"{prefix}{(number + 1):D3}";
                }
                
                return $"{prefix}001";
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GenerateOrderNumberAsync), GetType(), _logger, new { 
                    Method = nameof(GenerateOrderNumberAsync),
                    ServiceType = GetType().Name 
                });
                return $"PO{DateTime.Today:yyyyMMdd}001";
            }
        }

        /// <summary>
        /// 檢查採購單編號是否已存在（符合 EntityCodeGenerationHelper 約定）
        /// </summary>
        /// <param name="code">採購單編號</param>
        /// <param name="excludeId">排除的ID（用於編輯模式）</param>
        /// <returns>是否存在</returns>
        public async Task<bool> IsPurchaseOrderCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.PurchaseOrders.Where(po => po.Code == code);
                
                if (excludeId.HasValue)
                {
                    query = query.Where(po => po.Id != excludeId.Value);
                }
                
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsPurchaseOrderCodeExistsAsync), GetType(), _logger, new { 
                    Method = nameof(IsPurchaseOrderCodeExistsAsync),
                    ServiceType = GetType().Name,
                    Code = code,
                    ExcludeId = excludeId
                });
                return false;
            }
        }

        #endregion

        #region 訂單明細管理

        /// <summary>
        /// 獲取採購單的所有明細
        /// </summary>
        public async Task<List<PurchaseOrderDetail>> GetOrderDetailsAsync(int purchaseOrderId)
        {
            try
            {
                return await _purchaseOrderDetailService.GetByPurchaseOrderIdAsync(purchaseOrderId);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetOrderDetailsAsync), GetType(), _logger, new { 
                    PurchaseOrderId = purchaseOrderId 
                });
                return new List<PurchaseOrderDetail>();
            }
        }

        /// <summary>
        /// 新增採購單明細
        /// </summary>
        public async Task<ServiceResult> AddOrderDetailAsync(PurchaseOrderDetail detail)
        {
            try
            {
                return await _purchaseOrderDetailService.CreateAsync(detail);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(AddOrderDetailAsync), GetType(), _logger, new { 
                    PurchaseOrderId = detail.PurchaseOrderId,
                    ProductId = detail.ProductId 
                });
                return ServiceResult.Failure("新增採購單明細時發生錯誤");
            }
        }

        /// <summary>
        /// 更新採購單明細
        /// </summary>
        public async Task<ServiceResult> UpdateOrderDetailAsync(PurchaseOrderDetail detail)
        {
            try
            {
                return await _purchaseOrderDetailService.UpdateAsync(detail);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateOrderDetailAsync), GetType(), _logger, new { 
                    DetailId = detail.Id,
                    PurchaseOrderId = detail.PurchaseOrderId,
                    ProductId = detail.ProductId 
                });
                return ServiceResult.Failure("更新採購單明細時發生錯誤");
            }
        }

        /// <summary>
        /// 刪除採購單明細
        /// </summary>
        public async Task<ServiceResult> DeleteOrderDetailAsync(int detailId)
        {
            try
            {
                return await _purchaseOrderDetailService.PermanentDeleteAsync(detailId);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeleteOrderDetailAsync), GetType(), _logger, new { 
                    DetailId = detailId 
                });
                return ServiceResult.Failure("刪除採購單明細時發生錯誤");
            }
        }

        #endregion

        #region 稅額計算

        /// <summary>
        /// 計算並更新採購單的稅額
        /// </summary>
        /// <param name="purchaseOrderId">採購單ID</param>
        /// <returns>服務結果</returns>
        public async Task<ServiceResult> CalculateAndUpdateTaxAmountAsync(int purchaseOrderId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var purchaseOrder = await context.PurchaseOrders
                    .FirstOrDefaultAsync(po => po.Id == purchaseOrderId);
                
                if (purchaseOrder == null)
                    return ServiceResult.Failure("找不到採購訂單");
                
                // 取得系統稅率
                var taxRate = await _systemParameterService.GetTaxRateAsync();
                
                // 計算稅額: TaxRate(%) * TotalAmount
                purchaseOrder.PurchaseTaxAmount = Math.Round(purchaseOrder.TotalAmount * (taxRate / 100m), 2);
                purchaseOrder.UpdatedAt = DateTime.Now;
                
                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CalculateAndUpdateTaxAmountAsync), GetType(), _logger, new { 
                    Method = nameof(CalculateAndUpdateTaxAmountAsync),
                    ServiceType = GetType().Name,
                    PurchaseOrderId = purchaseOrderId 
                });
                return ServiceResult.Failure("計算稅額時發生錯誤");
            }
        }

        /// <summary>
        /// 計算稅額(不儲存到資料庫)
        /// </summary>
        /// <param name="totalAmount">總金額</param>
        /// <returns>計算出的稅額</returns>
        public async Task<decimal> CalculateTaxAmountAsync(decimal totalAmount)
        {
            try
            {
                // 取得系統稅率
                var taxRate = await _systemParameterService.GetTaxRateAsync();
                
                // 計算稅額: TaxRate(%) * TotalAmount
                return Math.Round(totalAmount * (taxRate / 100m), 2);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CalculateTaxAmountAsync), GetType(), _logger, new { 
                    Method = nameof(CalculateTaxAmountAsync),
                    ServiceType = GetType().Name,
                    TotalAmount = totalAmount 
                });
                return 0m; // 發生錯誤時返回0
            }
        }

        #endregion

        #region 進貨相關查詢

        /// <summary>
        /// 獲取廠商的進貨明細（含審核過濾）
        /// </summary>
        /// <param name="supplierId">廠商ID</param>
        /// <param name="includeCompleted">是否包含已完成的明細</param>
        /// <param name="checkApproval">是否檢查審核狀態（true=只載入已審核，false=不檢查審核）</param>
        public async Task<List<PurchaseOrderDetail>> GetReceivingDetailsBySupplierAsync(int supplierId, bool includeCompleted, bool checkApproval = true)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var query = context.PurchaseOrderDetails
                    .Include(pod => pod.PurchaseOrder)
                        .ThenInclude(po => po.Supplier)
                    .Include(pod => pod.PurchaseOrder)
                        .ThenInclude(po => po.Warehouse)
                    .Include(pod => pod.Product)
                    .Where(pod => pod.PurchaseOrder.SupplierId == supplierId);

                // 根據參數決定是否檢查審核狀態
                if (checkApproval)
                {
                    query = query.Where(pod => pod.PurchaseOrder.IsApproved);
                }

                if (!includeCompleted)
                {
                    // 只包含未完成的明細：既未手動完成，且數量未滿
                    query = query.Where(pod => !pod.IsReceivingCompleted && pod.ReceivedQuantity < pod.OrderQuantity);
                }

                return await query.OrderBy(pod => pod.PurchaseOrder.Code)
                                .ThenBy(pod => pod.Product.Name)
                                .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetReceivingDetailsBySupplierAsync), GetType(), _logger, new { 
                    SupplierId = supplierId,
                    IncludeCompleted = includeCompleted,
                    CheckApproval = checkApproval
                });
                return new List<PurchaseOrderDetail>();
            }
        }

        /// <summary>
        /// 獲取廠商的未完成採購單
        /// </summary>
        public async Task<List<PurchaseOrder>> GetIncompleteOrdersBySupplierAsync(int supplierId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 檢查是否啟用採購單審核功能
                var isApprovalEnabled = await _systemParameterService.IsPurchaseOrderApprovalEnabledAsync();
                
                var query = context.PurchaseOrders
                    .Include(po => po.Company)
                    .Include(po => po.Supplier)
                    .Include(po => po.Warehouse)
                    .Include(po => po.ApprovedByUser)
                    .Include(po => po.PurchaseOrderDetails)
                        .ThenInclude(pod => pod.Product)
                    .Where(po => po.SupplierId == supplierId
                                && po.PurchaseOrderDetails.Any(pod => pod.ReceivedQuantity < pod.OrderQuantity));
                
                // 如果啟用審核，只顯示已核准的採購單
                if (isApprovalEnabled)
                {
                    query = query.Where(po => po.IsApproved);
                }
                
                return await query
                    .OrderBy(po => po.ExpectedDeliveryDate)
                    .ThenBy(po => po.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetIncompleteOrdersBySupplierAsync), GetType(), _logger, new { 
                    SupplierId = supplierId 
                });
                return new List<PurchaseOrder>();
            }
        }

        #endregion

    }
}

