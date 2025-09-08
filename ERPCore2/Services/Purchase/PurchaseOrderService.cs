using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 採購訂單服務實作
    /// </summary>
    public class PurchaseOrderService : GenericManagementService<PurchaseOrder>, IPurchaseOrderService
    {
        private readonly IInventoryStockService? _inventoryStockService;

        public PurchaseOrderService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        public PurchaseOrderService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<PurchaseOrder>> logger) : base(contextFactory, logger)
        {
        }

        public PurchaseOrderService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<PurchaseOrder>> logger,
            IInventoryStockService inventoryStockService) : base(contextFactory, logger)
        {
            _inventoryStockService = inventoryStockService;
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
                    .Where(po => !po.IsDeleted)
                    .OrderByDescending(po => po.OrderDate)
                    .ThenBy(po => po.PurchaseOrderNumber)
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
                    .Include(po => po.PurchaseOrderDetails)
                        .ThenInclude(pod => pod.Product)
                            .ThenInclude(p => p.Unit)
                    .Include(po => po.PurchaseReceivings)
                        .ThenInclude(pr => pr.PurchaseReceivingDetails)
                    .FirstOrDefaultAsync(po => po.Id == id && !po.IsDeleted);
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
                    .Where(po => !po.IsDeleted && 
                               (po.PurchaseOrderNumber.Contains(searchTerm) ||
                                po.Supplier.CompanyName.Contains(searchTerm) ||
                                po.Company != null && po.Company.CompanyName.Contains(searchTerm) ||
                                po.PurchasePersonnel != null && po.PurchasePersonnel.Contains(searchTerm) ||
                                po.Remarks != null && po.Remarks.Contains(searchTerm)))
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
                
                if (string.IsNullOrWhiteSpace(entity.PurchaseOrderNumber))
                    errors.Add("採購單號不能為空");
                
                if (entity.SupplierId <= 0)
                    errors.Add("必須選擇供應商");
                
                if (entity.CompanyId <= 0)
                    errors.Add("必須選擇採購公司");
                
                if (entity.OrderDate > DateTime.Today.AddDays(1))
                    errors.Add("訂單日期不能超過明天");
                
                if (entity.ExpectedDeliveryDate.HasValue && entity.ExpectedDeliveryDate.Value < entity.OrderDate)
                    errors.Add("預計到貨日期不能早於訂單日期");
                
                // 檢查採購單號是否重複
                if (!string.IsNullOrWhiteSpace(entity.PurchaseOrderNumber))
                {
                    using var context = await _contextFactory.CreateDbContextAsync();
                    var exists = await context.PurchaseOrders
                        .AnyAsync(po => po.PurchaseOrderNumber == entity.PurchaseOrderNumber && 
                                       po.Id != entity.Id && !po.IsDeleted);
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
                    OrderNumber = entity.PurchaseOrderNumber 
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
                    .Include(po => po.PurchaseOrderDetails)
                        .ThenInclude(pod => pod.Product)
                    .Where(po => po.SupplierId == supplierId && !po.IsDeleted)
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
                               po.OrderDate <= endDate && !po.IsDeleted)
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
                    .Include(po => po.PurchaseOrderDetails)
                        .ThenInclude(pod => pod.Product)
                    .FirstOrDefaultAsync(po => po.PurchaseOrderNumber == purchaseOrderNumber && !po.IsDeleted);
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

        #endregion

        #region 訂單操作

        public async Task<ServiceResult> SubmitOrderAsync(int orderId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var order = await context.PurchaseOrders
                    .Include(po => po.PurchaseOrderDetails)
                    .FirstOrDefaultAsync(po => po.Id == orderId && !po.IsDeleted);
                
                if (order == null)
                    return ServiceResult.Failure("找不到採購訂單");
                
                if (!order.PurchaseOrderDetails.Any())
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
                        .Include(po => po.PurchaseOrderDetails)
                        .FirstOrDefaultAsync(po => po.Id == orderId && !po.IsDeleted);
                    
                    if (order == null)
                        return ServiceResult.Failure("找不到採購訂單");
                    
                    if (order.IsApproved)
                        return ServiceResult.Failure("採購訂單已經核准，無需重複核准");

                    // 檢查是否有明細
                    if (!order.PurchaseOrderDetails.Any() || 
                        !order.PurchaseOrderDetails.Any(pod => !pod.IsDeleted))
                        return ServiceResult.Failure("採購訂單沒有有效的商品明細，無法核准");

                    // 更新採購訂單狀態
                    order.ApprovedBy = approvedBy;
                    order.ApprovedAt = DateTime.Now;
                    order.IsApproved = true;
                    order.UpdatedAt = DateTime.Now;
                    // TODO: 根據當前登入使用者設置 UpdatedBy
                    // order.UpdatedBy = currentUser.Name;
                    
                    await context.SaveChangesAsync();

                    // 採購確認時，增加在途庫存
                    var inventoryUpdateResult = await UpdateInTransitStockAsync(orderId, true);
                    if (!inventoryUpdateResult.IsSuccess)
                    {
                        await transaction.RollbackAsync();
                        return ServiceResult.Failure($"核准訂單成功，但更新在途庫存失敗：{inventoryUpdateResult.ErrorMessage}");
                    }

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
                        .Include(po => po.PurchaseOrderDetails)
                        .FirstOrDefaultAsync(po => po.Id == orderId && !po.IsDeleted);
                    
                    if (order == null)
                        return ServiceResult.Failure("找不到採購訂單");
                    
                    if (order.ReceivedAmount > 0)
                        return ServiceResult.Failure("已有進貨記錄的訂單無法駁回");

                    // 記錄是否之前已核准（需要減少在途庫存）
                    bool wasApproved = order.IsApproved;
                    
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

                    // 如果之前已核准，需要減少在途庫存
                    if (wasApproved)
                    {
                        var inventoryUpdateResult = await UpdateInTransitStockAsync(orderId, false);
                        if (!inventoryUpdateResult.IsSuccess)
                        {
                            await transaction.RollbackAsync();
                            return ServiceResult.Failure($"駁回訂單成功，但更新在途庫存失敗：{inventoryUpdateResult.ErrorMessage}");
                        }
                    }

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

        #region 採購在途庫存管理

        /// <summary>
        /// 更新採購訂單的在途庫存
        /// 採購確認時增加在途庫存，不檢查倉庫和庫位
        /// </summary>
        /// <param name="purchaseOrderId">採購訂單ID</param>
        /// <param name="isIncrease">是否增加在途庫存</param>
        /// <returns></returns>
        private async Task<ServiceResult> UpdateInTransitStockAsync(int purchaseOrderId, bool isIncrease)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 取得採購訂單明細
                var orderDetails = await context.PurchaseOrderDetails
                    .Include(pod => pod.PurchaseOrder)
                    .Where(pod => pod.PurchaseOrderId == purchaseOrderId && !pod.IsDeleted)
                    .ToListAsync();

                if (!orderDetails.Any())
                {
                    return ServiceResult.Success(); // 沒有明細，直接成功
                }

                // 逐筆更新商品的在途庫存
                foreach (var detail in orderDetails)
                {
                    await UpdateProductInTransitStockAsync(
                        detail.ProductId, 
                        detail.OrderQuantity, 
                        isIncrease,
                        detail.PurchaseOrder.PurchaseOrderNumber);
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateInTransitStockAsync), GetType(), _logger, new { 
                    Method = nameof(UpdateInTransitStockAsync),
                    ServiceType = GetType().Name,
                    PurchaseOrderId = purchaseOrderId,
                    IsIncrease = isIncrease 
                });
                return ServiceResult.Failure("更新在途庫存時發生錯誤");
            }
        }

        /// <summary>
        /// 更新特定商品的在途庫存
        /// 不檢查倉庫和庫位，只針對商品本身更新在途庫存
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <param name="quantity">數量</param>
        /// <param name="isIncrease">是否增加</param>
        /// <param name="orderNumber">採購訂單號</param>
        /// <returns></returns>
        private async Task UpdateProductInTransitStockAsync(int productId, int quantity, bool isIncrease, string orderNumber)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 查找該商品的庫存記錄（不考慮特定倉庫或庫位）
                var stock = await context.InventoryStocks
                    .FirstOrDefaultAsync(i => i.ProductId == productId && 
                                            !i.IsDeleted);

                if (stock == null)
                {
                    // 如果沒有庫存記錄，創建一個基本記錄
                    stock = new InventoryStock
                    {
                        ProductId = productId,
                        WarehouseId = null,  // 採購階段不指定倉庫
                        WarehouseLocationId = null,  // 採購階段不指定庫位
                        CurrentStock = 0,
                        ReservedStock = 0,
                        InTransitStock = isIncrease ? quantity : 0,
                        Status = EntityStatus.Active,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    await context.InventoryStocks.AddAsync(stock);
                }
                else
                {
                    // 更新在途庫存
                    if (isIncrease)
                    {
                        stock.InTransitStock += quantity;
                    }
                    else
                    {
                        stock.InTransitStock = Math.Max(0, stock.InTransitStock - quantity);
                    }
                    stock.UpdatedAt = DateTime.Now;
                }

                await context.SaveChangesAsync();

                // 記錄庫存異動日誌
                await LogInventoryTransactionAsync(context, productId, quantity, isIncrease, orderNumber, stock.Id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateProductInTransitStockAsync), GetType(), _logger, new { 
                    Method = nameof(UpdateProductInTransitStockAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId,
                    Quantity = quantity,
                    IsIncrease = isIncrease,
                    OrderNumber = orderNumber 
                });
                throw;
            }
        }

        /// <summary>
        /// 記錄庫存異動日誌
        /// </summary>
        private async Task LogInventoryTransactionAsync(AppDbContext context, int productId, 
            int quantity, bool isIncrease, string orderNumber, int inventoryStockId)
        {
            try
            {
                // 由於 InventoryTransaction 需要 WarehouseId，我們需要找一個預設倉庫
                // 或者採購階段不記錄 InventoryTransaction，只在實際入庫時記錄
                
                // 暫時跳過記錄 InventoryTransaction，因為採購階段沒有指定倉庫
                // 實際的庫存異動記錄應該在入庫時由庫存服務負責
                
                // var transaction = new InventoryTransaction
                // {
                //     ProductId = productId,
                //     WarehouseId = ?, // 採購階段沒有指定倉庫
                //     WarehouseLocationId = null,
                //     TransactionType = InventoryTransactionTypeEnum.Purchase,
                //     TransactionNumber = orderNumber,
                //     TransactionDate = DateTime.Now,
                //     Quantity = isIncrease ? quantity : -quantity,
                //     TransactionRemarks = isIncrease ? "採購訂單核准-增加在途庫存" : "採購訂單駁回-減少在途庫存",
                //     ReferenceNumber = orderNumber,
                //     InventoryStockId = inventoryStockId,
                //     Status = EntityStatus.Active,
                //     CreatedAt = DateTime.Now,
                //     UpdatedAt = DateTime.Now
                // };

                // await context.InventoryTransactions.AddAsync(transaction);
                // await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(LogInventoryTransactionAsync), GetType(), _logger, new { 
                    Method = nameof(LogInventoryTransactionAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId,
                    Quantity = quantity,
                    IsIncrease = isIncrease,
                    OrderNumber = orderNumber 
                });
                // 日誌記錄失敗不應該影響主要流程，所以不重新拋出異常
            }
        }

        public async Task<ServiceResult> CancelOrderAsync(int orderId, string reason)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var order = await context.PurchaseOrders
                    .FirstOrDefaultAsync(po => po.Id == orderId && !po.IsDeleted);
                
                if (order == null)
                    return ServiceResult.Failure("找不到採購訂單");
                
                if (order.ReceivedAmount > 0)
                    return ServiceResult.Failure("已有進貨記錄的訂單無法取消");
                
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
                    OrderId = orderId,
                    Reason = reason 
                });
                return ServiceResult.Failure("取消訂單時發生錯誤");
            }
        }

        public async Task<ServiceResult> CloseOrderAsync(int orderId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var order = await context.PurchaseOrders
                    .FirstOrDefaultAsync(po => po.Id == orderId && !po.IsDeleted);
                
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

        public async Task<ServiceResult> UpdateReceivedAmountAsync(int orderId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();
                
                try
                {
                    var order = await context.PurchaseOrders
                        .Include(po => po.PurchaseOrderDetails)
                        .FirstOrDefaultAsync(po => po.Id == orderId && !po.IsDeleted);
                    
                    if (order == null)
                        return ServiceResult.Failure("找不到採購訂單");
                    
                    // 計算已進貨金額
                    var receivedAmount = order.PurchaseOrderDetails.Sum(pod => pod.ReceivedAmount);
                    order.ReceivedAmount = receivedAmount;
                    
                    // 移除狀態更新邏輯，只更新金額
                    order.UpdatedAt = DateTime.Now;
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateReceivedAmountAsync), GetType(), _logger, new { 
                    Method = nameof(UpdateReceivedAmountAsync),
                    ServiceType = GetType().Name,
                    OrderId = orderId 
                });
                return ServiceResult.Failure("更新進貨金額時發生錯誤");
            }
        }

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
        /// </summary>
        /// <param name="entity">採購單實體</param>
        /// <returns>刪除檢查結果</returns>
        protected override async Task<ServiceResult> CanDeleteAsync(PurchaseOrder entity)
        {
            try
            {
                var checkResult = await DependencyCheckHelper.CheckPurchaseOrderDependenciesAsync(_contextFactory, entity.Id);
                if (!checkResult.CanDelete)
                {
                    return ServiceResult.Failure(checkResult.GetFormattedErrorMessage("採購單"));
                }
                
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
                    .Where(po => !po.IsDeleted && 
                               po.IsApproved && // 只包含已核准的訂單
                               po.ReceivedAmount < po.TotalAmount) // 改為用進貨金額判斷
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
                    .Where(po => !po.IsDeleted && 
                               po.IsApproved && // 只包含已核准的訂單
                               po.ReceivedAmount < po.TotalAmount && // 改為用進貨金額判斷
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
                    .Where(po => po.SupplierId == supplierId && !po.IsDeleted);
                
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

        #region 明細相關

        public async Task<List<PurchaseOrderDetail>> GetOrderDetailsAsync(int orderId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseOrderDetails
                    .Include(pod => pod.Product)
                        .ThenInclude(p => p.Unit)
                    .Where(pod => pod.PurchaseOrderId == orderId && !pod.IsDeleted)
                    .OrderBy(pod => pod.Product.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetOrderDetailsAsync), GetType(), _logger, new { 
                    Method = nameof(GetOrderDetailsAsync),
                    ServiceType = GetType().Name,
                    OrderId = orderId 
                });
                return new List<PurchaseOrderDetail>();
            }
        }

        public async Task<ServiceResult> AddOrderDetailAsync(PurchaseOrderDetail detail)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();
                
                try
                {
                    // 移除重複商品檢查 - 允許同一商品在同一訂單中出現多次
                    // 這樣可以支援不同價格、不同備註或不同交期的相同商品採購
                    
                    detail.Status = EntityStatus.Active;
                    detail.CreatedAt = DateTime.Now;
                    
                    await context.PurchaseOrderDetails.AddAsync(detail);
                    await context.SaveChangesAsync();
                    
                    // 更新訂單總金額
                    var order = await context.PurchaseOrders
                        .Include(po => po.PurchaseOrderDetails)
                        .FirstOrDefaultAsync(po => po.Id == detail.PurchaseOrderId);
                    
                    if (order != null)
                    {
                        order.TotalAmount = order.PurchaseOrderDetails
                            .Where(pod => !pod.IsDeleted)
                            .Sum(pod => pod.SubtotalAmount);
                        order.UpdatedAt = DateTime.Now;
                        await context.SaveChangesAsync();
                    }
                    
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(AddOrderDetailAsync), GetType(), _logger, new { 
                    Method = nameof(AddOrderDetailAsync),
                    ServiceType = GetType().Name,
                    OrderId = detail.PurchaseOrderId,
                    ProductId = detail.ProductId 
                });
                return ServiceResult.Failure("新增訂單明細時發生錯誤");
            }
        }

        public async Task<ServiceResult> UpdateOrderDetailAsync(PurchaseOrderDetail detail)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();
                
                try
                {
                    var existing = await context.PurchaseOrderDetails
                        .FirstOrDefaultAsync(pod => pod.Id == detail.Id && !pod.IsDeleted);
                    
                    if (existing == null)
                        return ServiceResult.Failure("找不到訂單明細");
                    
                    existing.OrderQuantity = detail.OrderQuantity;
                    existing.UnitPrice = detail.UnitPrice;
                    existing.Remarks = detail.Remarks;
                    existing.ExpectedDeliveryDate = detail.ExpectedDeliveryDate;
                    existing.UpdatedAt = DateTime.Now;
                    
                    await context.SaveChangesAsync();
                    
                    // 更新訂單總金額
                    var order = await context.PurchaseOrders
                        .Include(po => po.PurchaseOrderDetails)
                        .FirstOrDefaultAsync(po => po.Id == existing.PurchaseOrderId);
                    
                    if (order != null)
                    {
                        order.TotalAmount = order.PurchaseOrderDetails
                            .Where(pod => !pod.IsDeleted)
                            .Sum(pod => pod.SubtotalAmount);
                        order.UpdatedAt = DateTime.Now;
                        await context.SaveChangesAsync();
                    }
                    
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateOrderDetailAsync), GetType(), _logger, new { 
                    Method = nameof(UpdateOrderDetailAsync),
                    ServiceType = GetType().Name,
                    DetailId = detail.Id 
                });
                return ServiceResult.Failure("更新訂單明細時發生錯誤");
            }
        }

        public async Task<ServiceResult> DeleteOrderDetailAsync(int detailId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();
                
                try
                {
                    var detail = await context.PurchaseOrderDetails
                        .FirstOrDefaultAsync(pod => pod.Id == detailId && !pod.IsDeleted);
                    
                    if (detail == null)
                        return ServiceResult.Failure("找不到訂單明細");
                    
                    // 檢查是否已有進貨記錄
                    var hasReceipts = await context.PurchaseReceivingDetails
                        .AnyAsync(prd => prd.PurchaseOrderDetailId == detailId && !prd.IsDeleted);
                    
                    if (hasReceipts)
                        return ServiceResult.Failure("此明細已有進貨記錄，無法刪除");
                    
                    detail.IsDeleted = true;
                    detail.UpdatedAt = DateTime.Now;
                    
                    await context.SaveChangesAsync();
                    
                    // 更新訂單總金額
                    var order = await context.PurchaseOrders
                        .Include(po => po.PurchaseOrderDetails)
                        .FirstOrDefaultAsync(po => po.Id == detail.PurchaseOrderId);
                    
                    if (order != null)
                    {
                        order.TotalAmount = order.PurchaseOrderDetails
                            .Where(pod => !pod.IsDeleted)
                            .Sum(pod => pod.SubtotalAmount);
                        order.UpdatedAt = DateTime.Now;
                        await context.SaveChangesAsync();
                    }
                    
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeleteOrderDetailAsync), GetType(), _logger, new { 
                    Method = nameof(DeleteOrderDetailAsync),
                    ServiceType = GetType().Name,
                    DetailId = detailId 
                });
                return ServiceResult.Failure("刪除訂單明細時發生錯誤");
            }
        }

        /// <summary>
        /// 取得指定供應商尚未完成進貨的採購明細
        /// </summary>
        public async Task<List<PurchaseOrderDetail>> GetPendingReceivingDetailsBySupplierAsync(int supplierId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                return await context.PurchaseOrderDetails
                    .Include(pod => pod.Product)
                        .ThenInclude(p => p.Unit)
                    .Include(pod => pod.PurchaseOrder)
                        .ThenInclude(po => po.Supplier)
                    .Where(pod => 
                        pod.PurchaseOrder.SupplierId == supplierId &&
                        !pod.IsDeleted &&
                        !pod.PurchaseOrder.IsDeleted &&
                        pod.PurchaseOrder.IsApproved &&
                        // 檢查尚未完全進貨的明細 - 考慮手動完成標記
                        !context.PurchaseReceivingDetails
                            .Where(prd => prd.PurchaseOrderDetailId == pod.Id && !prd.IsDeleted)
                            .Any(prd => prd.IsReceivingCompleted) &&
                        pod.OrderQuantity > context.PurchaseReceivingDetails
                            .Where(prd => prd.PurchaseOrderDetailId == pod.Id && !prd.IsDeleted)
                            .Sum(prd => prd.ReceivedQuantity)
                    )
                    .OrderBy(pod => pod.PurchaseOrder.OrderDate)
                    .ThenBy(pod => pod.PurchaseOrder.PurchaseOrderNumber)
                    .ThenBy(pod => pod.Product.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPendingReceivingDetailsBySupplierAsync), GetType(), _logger, new { 
                    Method = nameof(GetPendingReceivingDetailsBySupplierAsync),
                    ServiceType = GetType().Name,
                    SupplierId = supplierId 
                });
                return new List<PurchaseOrderDetail>();
            }
        }

        /// <summary>
        /// 取得指定供應商尚未完成進貨的採購明細（包含剩餘數量資訊）
        /// </summary>
        public async Task<List<PurchaseOrderDetail>> GetPendingReceivingDetailsBySupplierWithQuantityAsync(int supplierId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var details = await context.PurchaseOrderDetails
                    .Include(pod => pod.Product)
                        .ThenInclude(p => p.Unit)
                    .Include(pod => pod.PurchaseOrder)
                        .ThenInclude(po => po.Supplier)
                    .Where(pod => 
                        pod.PurchaseOrder.SupplierId == supplierId &&
                        !pod.IsDeleted &&
                        !pod.PurchaseOrder.IsDeleted &&
                        pod.PurchaseOrder.IsApproved
                    )
                    .Select(pod => new 
                    {
                        Detail = pod,
                        ReceivedQuantity = context.PurchaseReceivingDetails
                            .Where(prd => prd.PurchaseOrderDetailId == pod.Id && !prd.IsDeleted)
                            .Sum(prd => prd.ReceivedQuantity)
                    })
                    .Where(x => x.Detail.OrderQuantity > x.ReceivedQuantity)
                    .OrderBy(x => x.Detail.PurchaseOrder.OrderDate)
                    .ThenBy(x => x.Detail.PurchaseOrder.PurchaseOrderNumber)
                    .ThenBy(x => x.Detail.Product.Code)
                    .ToListAsync();

                // 設定剩餘數量資訊到明細物件的備註或額外屬性
                var result = details.Select(x => 
                {
                    var detail = x.Detail;
                    // 可以在這裡計算並設定剩餘數量等額外資訊
                    return detail;
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPendingReceivingDetailsBySupplierWithQuantityAsync), GetType(), _logger, new { 
                    Method = nameof(GetPendingReceivingDetailsBySupplierWithQuantityAsync),
                    ServiceType = GetType().Name,
                    SupplierId = supplierId 
                });
                return new List<PurchaseOrderDetail>();
            }
        }

        /// <summary>
        /// 取得指定供應商的採購明細（可根據編輯模式決定是否隱藏已完成項目）
        /// </summary>
        /// <param name="supplierId">供應商ID</param>
        /// <param name="isEditMode">是否為編輯模式，true=顯示所有項目，false=隱藏已完成項目</param>
        public async Task<List<PurchaseOrderDetail>> GetReceivingDetailsBySupplierAsync(int supplierId, bool isEditMode = false)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var query = context.PurchaseOrderDetails
                    .Include(pod => pod.Product)
                        .ThenInclude(p => p.Unit)
                    .Include(pod => pod.PurchaseOrder)
                        .ThenInclude(po => po.Supplier)
                    .Where(pod => 
                        pod.PurchaseOrder.SupplierId == supplierId &&
                        !pod.IsDeleted &&
                        !pod.PurchaseOrder.IsDeleted &&
                        pod.PurchaseOrder.IsApproved);

                // 如果不是編輯模式，則過濾掉已完成的項目
                if (!isEditMode)
                {
                    query = query.Where(pod =>
                        // 檢查尚未完全進貨的明細 - 考慮手動完成標記
                        !context.PurchaseReceivingDetails
                            .Where(prd => prd.PurchaseOrderDetailId == pod.Id && !prd.IsDeleted)
                            .Any(prd => prd.IsReceivingCompleted) &&
                        pod.OrderQuantity > context.PurchaseReceivingDetails
                            .Where(prd => prd.PurchaseOrderDetailId == pod.Id && !prd.IsDeleted)
                            .Sum(prd => prd.ReceivedQuantity)
                    );
                }
                
                return await query
                    .OrderBy(pod => pod.PurchaseOrder.OrderDate)
                    .ThenBy(pod => pod.PurchaseOrder.PurchaseOrderNumber)
                    .ThenBy(pod => pod.Product.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetReceivingDetailsBySupplierAsync), GetType(), _logger, new { 
                    Method = nameof(GetReceivingDetailsBySupplierAsync),
                    ServiceType = GetType().Name,
                    SupplierId = supplierId,
                    IsEditMode = isEditMode 
                });
                return new List<PurchaseOrderDetail>();
            }
        }

        /// <summary>
        /// 獲取供應商的未完成採購單（包含完整關聯資料用於判斷完成狀態）
        /// </summary>
        /// <param name="supplierId">供應商ID</param>
        /// <returns>未完成的採購單清單</returns>
        public async Task<List<PurchaseOrder>> GetIncompleteOrdersBySupplierAsync(int supplierId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PurchaseOrders
                    .Include(po => po.Supplier)
                    .Include(po => po.Warehouse)
                    .Include(po => po.PurchaseOrderDetails)
                        .ThenInclude(pod => pod.Product)
                    .Include(po => po.PurchaseOrderDetails)
                        .ThenInclude(pod => pod.PurchaseReceivingDetails)
                    .Where(po => po.SupplierId == supplierId && 
                               !po.IsDeleted && 
                               po.IsApproved) // 只包含已核准的訂單
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetIncompleteOrdersBySupplierAsync), GetType(), _logger, new { 
                    Method = nameof(GetIncompleteOrdersBySupplierAsync),
                    ServiceType = GetType().Name,
                    SupplierId = supplierId 
                });
                return new List<PurchaseOrder>();
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
                    .Where(po => po.PurchaseOrderNumber.StartsWith(prefix))
                    .OrderByDescending(po => po.PurchaseOrderNumber)
                    .FirstOrDefaultAsync();
                
                if (lastOrder == null)
                {
                    return $"{prefix}001";
                }
                
                var lastNumber = lastOrder.PurchaseOrderNumber.Substring(prefix.Length);
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

        #endregion

        #endregion
    }
}
