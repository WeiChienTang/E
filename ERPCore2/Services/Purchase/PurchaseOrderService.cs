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
        public PurchaseOrderService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        public PurchaseOrderService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<PurchaseOrder>> logger) : base(contextFactory, logger)
        {
        }

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
                var order = await context.PurchaseOrders
                    .FirstOrDefaultAsync(po => po.Id == orderId && !po.IsDeleted);
                
                if (order == null)
                    return ServiceResult.Failure("找不到採購訂單");
                
                if (order.IsApproved)
                    return ServiceResult.Failure("採購訂單已經核准，無需重複核准");
                
                order.ApprovedBy = approvedBy;
                order.ApprovedAt = DateTime.Now;
                order.IsApproved = true;
                order.UpdatedAt = DateTime.Now;
                // TODO: 根據當前登入使用者設置 UpdatedBy
                // order.UpdatedBy = currentUser.Name;
                
                var saveResult = await context.SaveChangesAsync();
                
                if (saveResult > 0)
                {
                    return ServiceResult.Success();
                }
                else
                {
                    return ServiceResult.Failure("核准訂單失敗：資料庫儲存失敗");
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
                var order = await context.PurchaseOrders
                    .FirstOrDefaultAsync(po => po.Id == orderId && !po.IsDeleted);
                
                if (order == null)
                    return ServiceResult.Failure("找不到採購訂單");
                
                if (order.IsApproved)
                    return ServiceResult.Failure("已審核通過的採購訂單無法駁回");
                
                if (order.ReceivedAmount > 0)
                    return ServiceResult.Failure("已有進貨記錄的訂單無法駁回");
                
                // 重置審核狀態
                order.IsApproved = false;
                order.ApprovedBy = null;
                order.ApprovedAt = null;
                
                // 設定駁回原因
                order.RejectReason = reason;
                
                order.UpdatedAt = DateTime.Now;
                // TODO: 根據當前登入使用者設置 UpdatedBy
                // order.UpdatedBy = currentUser.Name;
                
                var saveResult = await context.SaveChangesAsync();
                
                if (saveResult > 0)
                {
                    return ServiceResult.Success();
                }
                else
                {
                    return ServiceResult.Failure("駁回訂單失敗：資料庫儲存失敗");
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

        public async Task<bool> CanDeleteAsync(int orderId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var order = await context.PurchaseOrders
                    .Include(po => po.PurchaseReceivings)
                    .FirstOrDefaultAsync(po => po.Id == orderId && !po.IsDeleted);
                
                if (order == null) return false;
                
                // 已有進貨記錄的訂單不能刪除
                if (order.PurchaseReceivings.Any(pr => !pr.IsDeleted))
                    return false;
                
                // 移除狀態檢查，只要沒有進貨記錄就可以刪除
                return true;
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
                    existing.DetailRemarks = detail.DetailRemarks;
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
                        // 檢查尚未完全進貨的明細
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
    }
}
