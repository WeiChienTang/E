using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 沖銷商品明細服務實作
    /// </summary>
    public class SetoffProductDetailService : GenericManagementService<SetoffProductDetail>, ISetoffProductDetailService
    {
        public SetoffProductDetailService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<SetoffProductDetail>> logger) : base(contextFactory, logger)
        {
        }

        public SetoffProductDetailService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        /// <summary>
        /// 覆寫 CreateAsync 以計算累計金額
        /// </summary>
        public override async Task<ServiceResult<SetoffProductDetail>> CreateAsync(SetoffProductDetail entity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 查詢該來源明細的既有累計金額
                var existingTotal = await context.SetoffProductDetails
                    .Where(d => d.SourceDetailId == entity.SourceDetailId && d.SourceDetailType == entity.SourceDetailType)
                    .SumAsync(d => d.CurrentSetoffAmount);
                
                var existingAllowance = await context.SetoffProductDetails
                    .Where(d => d.SourceDetailId == entity.SourceDetailId && d.SourceDetailType == entity.SourceDetailType)
                    .SumAsync(d => d.CurrentAllowanceAmount);
                
                // 設定本筆記錄的累計金額（既有累計 + 本次金額）
                entity.TotalSetoffAmount = existingTotal + entity.CurrentSetoffAmount;
                entity.TotalAllowanceAmount = existingAllowance + entity.CurrentAllowanceAmount;
                
                // 呼叫基底類別的 CreateAsync
                var result = await base.CreateAsync(entity);
                
                // 如果成功，更新來源明細的累計金額
                if (result.IsSuccess)
                {
                    await UpdateSourceDetailTotalAmountAsync(entity.SourceDetailId, entity.SourceDetailType);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateAsync), GetType(), _logger, new
                {
                    Method = nameof(CreateAsync),
                    ServiceType = GetType().Name,
                    EntityId = entity.Id
                });
                return ServiceResult<SetoffProductDetail>.Failure($"新增沖銷商品明細時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 覆寫 UpdateAsync 以重新計算累計金額
        /// </summary>
        public override async Task<ServiceResult<SetoffProductDetail>> UpdateAsync(SetoffProductDetail entity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 查詢該來源明細的既有累計金額（排除當前正在更新的這筆）
                var existingTotal = await context.SetoffProductDetails
                    .Where(d => d.SourceDetailId == entity.SourceDetailId 
                             && d.SourceDetailType == entity.SourceDetailType
                             && d.Id != entity.Id)
                    .SumAsync(d => d.CurrentSetoffAmount);
                
                var existingAllowance = await context.SetoffProductDetails
                    .Where(d => d.SourceDetailId == entity.SourceDetailId 
                             && d.SourceDetailType == entity.SourceDetailType
                             && d.Id != entity.Id)
                    .SumAsync(d => d.CurrentAllowanceAmount);
                
                // 重新計算本筆記錄的累計金額
                entity.TotalSetoffAmount = existingTotal + entity.CurrentSetoffAmount;
                entity.TotalAllowanceAmount = existingAllowance + entity.CurrentAllowanceAmount;
                
                // 呼叫基底類別的 UpdateAsync
                var result = await base.UpdateAsync(entity);
                
                // 如果成功，更新來源明細的累計金額
                if (result.IsSuccess)
                {
                    await UpdateSourceDetailTotalAmountAsync(entity.SourceDetailId, entity.SourceDetailType);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateAsync), GetType(), _logger, new
                {
                    Method = nameof(UpdateAsync),
                    ServiceType = GetType().Name,
                    EntityId = entity.Id
                });
                return ServiceResult<SetoffProductDetail>.Failure($"更新沖銷商品明細時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 覆寫 GetAllAsync 以包含關聯資料
        /// </summary>
        public override async Task<List<SetoffProductDetail>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SetoffProductDetails
                    .Include(d => d.SetoffDocument)
                    .Include(d => d.Product)
                    .OrderByDescending(d => d.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new
                {
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name
                });
                return new List<SetoffProductDetail>();
            }
        }

        /// <summary>
        /// 覆寫 GetByIdAsync 以包含關聯資料
        /// </summary>
        public override async Task<SetoffProductDetail?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SetoffProductDetails
                    .Include(d => d.SetoffDocument)
                    .Include(d => d.Product)
                    .FirstOrDefaultAsync(d => d.Id == id);
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

        /// <summary>
        /// 根據沖款單ID取得商品明細
        /// </summary>
        public async Task<List<SetoffProductDetail>> GetBySetoffDocumentIdAsync(int setoffDocumentId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SetoffProductDetails
                    .Include(d => d.Product)
                    .Where(d => d.SetoffDocumentId == setoffDocumentId)
                    .OrderBy(d => d.Product.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySetoffDocumentIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetBySetoffDocumentIdAsync),
                    ServiceType = GetType().Name,
                    SetoffDocumentId = setoffDocumentId
                });
                return new List<SetoffProductDetail>();
            }
        }

        /// <summary>
        /// 取得特定客戶的未結清明細（應收帳款）
        /// </summary>
        public async Task<List<UnsettledDetailDto>> GetUnsettledReceivableDetailsAsync(int customerId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var results = new List<UnsettledDetailDto>();

                // 1. 查詢未結清的銷貨訂單明細
                var salesOrderDetails = await context.SalesOrderDetails
                    .Include(d => d.SalesOrder)
                    .Include(d => d.Product)
                    .Where(d => d.SalesOrder.CustomerId == customerId && !d.IsSettled)
                    .Select(d => new UnsettledDetailDto
                    {
                        SourceDetailType = SetoffDetailType.SalesOrderDetail,
                        SourceDetailId = d.Id,
                        SourceDocumentNumber = d.SalesOrder.SalesOrderNumber,
                        ProductId = d.ProductId,
                        ProductName = d.Product.Name,
                        ProductCode = d.Product.Code ?? string.Empty,
                        TotalAmount = d.SubtotalAmount,
                        PaidAmount = d.TotalReceivedAmount,
                        DocumentDate = d.SalesOrder.OrderDate,
                        IsReturn = false
                    })
                    .ToListAsync();

                results.AddRange(salesOrderDetails);

                // 2. 查詢未結清的銷貨退回明細
                var salesReturnDetails = await context.SalesReturnDetails
                    .Include(d => d.SalesReturn)
                    .Include(d => d.Product)
                    .Where(d => d.SalesReturn.CustomerId == customerId && !d.IsSettled)
                    .Select(d => new UnsettledDetailDto
                    {
                        SourceDetailType = SetoffDetailType.SalesReturnDetail,
                        SourceDetailId = d.Id,
                        SourceDocumentNumber = d.SalesReturn.SalesReturnNumber,
                        ProductId = d.ProductId,
                        ProductName = d.Product.Name,
                        ProductCode = d.Product.Code ?? string.Empty,
                        TotalAmount = d.ReturnSubtotalAmount,
                        PaidAmount = d.TotalPaidAmount,
                        DocumentDate = d.SalesReturn.ReturnDate,
                        IsReturn = true
                    })
                    .ToListAsync();

                results.AddRange(salesReturnDetails);

                // 3. 查詢每筆明細的歷史累計沖款和折讓金額（從 SetoffProductDetail 表）
                foreach (var detail in results)
                {
                    var historicalSetoff = await context.SetoffProductDetails
                        .Where(d => d.SourceDetailId == detail.SourceDetailId 
                                 && d.SourceDetailType == detail.SourceDetailType)
                        .SumAsync(d => d.CurrentSetoffAmount);

                    var historicalAllowance = await context.SetoffProductDetails
                        .Where(d => d.SourceDetailId == detail.SourceDetailId 
                                 && d.SourceDetailType == detail.SourceDetailType)
                        .SumAsync(d => d.CurrentAllowanceAmount);

                    detail.TotalHistoricalSetoffAmount = historicalSetoff;
                    detail.TotalHistoricalAllowanceAmount = historicalAllowance;
                }

                return results.OrderBy(r => r.DocumentDate).ThenBy(r => r.ProductName).ToList();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetUnsettledReceivableDetailsAsync), GetType(), _logger, new
                {
                    Method = nameof(GetUnsettledReceivableDetailsAsync),
                    ServiceType = GetType().Name,
                    CustomerId = customerId
                });
                return new List<UnsettledDetailDto>();
            }
        }

        /// <summary>
        /// 取得特定廠商的未結清明細（應付帳款）
        /// </summary>
        public async Task<List<UnsettledDetailDto>> GetUnsettledPayableDetailsAsync(int supplierId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var results = new List<UnsettledDetailDto>();

                // 1. 查詢未結清的採購進貨明細
                var purchaseReceivingDetails = await context.PurchaseReceivingDetails
                    .Include(d => d.PurchaseReceiving)
                    .Include(d => d.Product)
                    .Where(d => d.PurchaseReceiving.SupplierId == supplierId && !d.IsSettled)
                    .Select(d => new UnsettledDetailDto
                    {
                        SourceDetailType = SetoffDetailType.PurchaseReceivingDetail,
                        SourceDetailId = d.Id,
                        SourceDocumentNumber = d.PurchaseReceiving.ReceiptNumber,
                        ProductId = d.ProductId,
                        ProductName = d.Product.Name,
                        ProductCode = d.Product.Code ?? string.Empty,
                        TotalAmount = d.SubtotalAmount,
                        PaidAmount = d.TotalPaidAmount,
                        DocumentDate = d.PurchaseReceiving.ReceiptDate,
                        IsReturn = false
                    })
                    .ToListAsync();

                results.AddRange(purchaseReceivingDetails);

                // 2. 查詢未結清的採購退回明細
                var purchaseReturnDetails = await context.PurchaseReturnDetails
                    .Include(d => d.PurchaseReturn)
                    .Include(d => d.Product)
                    .Where(d => d.PurchaseReturn.SupplierId == supplierId && !d.IsSettled)
                    .Select(d => new UnsettledDetailDto
                    {
                        SourceDetailType = SetoffDetailType.PurchaseReturnDetail,
                        SourceDetailId = d.Id,
                        SourceDocumentNumber = d.PurchaseReturn.PurchaseReturnNumber,
                        ProductId = d.ProductId,
                        ProductName = d.Product.Name,
                        ProductCode = d.Product.Code ?? string.Empty,
                        TotalAmount = d.ReturnSubtotalAmount,
                        PaidAmount = d.TotalReceivedAmount,
                        DocumentDate = d.PurchaseReturn.ReturnDate,
                        IsReturn = true
                    })
                    .ToListAsync();

                results.AddRange(purchaseReturnDetails);

                // 3. 查詢每筆明細的歷史累計沖款和折讓金額（從 SetoffProductDetail 表）
                foreach (var detail in results)
                {
                    var historicalSetoff = await context.SetoffProductDetails
                        .Where(d => d.SourceDetailId == detail.SourceDetailId 
                                 && d.SourceDetailType == detail.SourceDetailType)
                        .SumAsync(d => d.CurrentSetoffAmount);

                    var historicalAllowance = await context.SetoffProductDetails
                        .Where(d => d.SourceDetailId == detail.SourceDetailId 
                                 && d.SourceDetailType == detail.SourceDetailType)
                        .SumAsync(d => d.CurrentAllowanceAmount);

                    detail.TotalHistoricalSetoffAmount = historicalSetoff;
                    detail.TotalHistoricalAllowanceAmount = historicalAllowance;
                }

                return results.OrderBy(r => r.DocumentDate).ThenBy(r => r.ProductName).ToList();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetUnsettledPayableDetailsAsync), GetType(), _logger, new
                {
                    Method = nameof(GetUnsettledPayableDetailsAsync),
                    ServiceType = GetType().Name,
                    SupplierId = supplierId
                });
                return new List<UnsettledDetailDto>();
            }
        }

        /// <summary>
        /// 根據來源明細類型和ID取得明細資訊（不論是否結清）
        /// </summary>
        public async Task<UnsettledDetailDto?> GetSourceDetailInfoAsync(
            SetoffDetailType sourceDetailType, 
            int sourceDetailId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                UnsettledDetailDto? result = null;

                switch (sourceDetailType)
                {
                    case SetoffDetailType.SalesOrderDetail:
                        var salesDetail = await context.SalesOrderDetails
                            .Include(d => d.SalesOrder)
                            .Include(d => d.Product)
                            .FirstOrDefaultAsync(d => d.Id == sourceDetailId);
                        
                        if (salesDetail != null)
                        {
                            result = new UnsettledDetailDto
                            {
                                SourceDetailType = SetoffDetailType.SalesOrderDetail,
                                SourceDetailId = salesDetail.Id,
                                SourceDocumentNumber = salesDetail.SalesOrder.SalesOrderNumber,
                                ProductId = salesDetail.ProductId,
                                ProductName = salesDetail.Product.Name,
                                ProductCode = salesDetail.Product.Code ?? string.Empty,
                                TotalAmount = salesDetail.SubtotalAmount,
                                PaidAmount = salesDetail.TotalReceivedAmount,
                                DocumentDate = salesDetail.SalesOrder.OrderDate,
                                IsReturn = false
                            };
                        }
                        break;

                    case SetoffDetailType.SalesReturnDetail:
                        var salesReturnDetail = await context.SalesReturnDetails
                            .Include(d => d.SalesReturn)
                            .Include(d => d.Product)
                            .FirstOrDefaultAsync(d => d.Id == sourceDetailId);
                        
                        if (salesReturnDetail != null)
                        {
                            result = new UnsettledDetailDto
                            {
                                SourceDetailType = SetoffDetailType.SalesReturnDetail,
                                SourceDetailId = salesReturnDetail.Id,
                                SourceDocumentNumber = salesReturnDetail.SalesReturn.SalesReturnNumber,
                                ProductId = salesReturnDetail.ProductId,
                                ProductName = salesReturnDetail.Product.Name,
                                ProductCode = salesReturnDetail.Product.Code ?? string.Empty,
                                TotalAmount = salesReturnDetail.ReturnSubtotalAmount,
                                PaidAmount = salesReturnDetail.TotalPaidAmount,
                                DocumentDate = salesReturnDetail.SalesReturn.ReturnDate,
                                IsReturn = true
                            };
                        }
                        break;

                    case SetoffDetailType.PurchaseReceivingDetail:
                        var purchaseDetail = await context.PurchaseReceivingDetails
                            .Include(d => d.PurchaseReceiving)
                            .Include(d => d.Product)
                            .FirstOrDefaultAsync(d => d.Id == sourceDetailId);
                        
                        if (purchaseDetail != null)
                        {
                            result = new UnsettledDetailDto
                            {
                                SourceDetailType = SetoffDetailType.PurchaseReceivingDetail,
                                SourceDetailId = purchaseDetail.Id,
                                SourceDocumentNumber = purchaseDetail.PurchaseReceiving.ReceiptNumber,
                                ProductId = purchaseDetail.ProductId,
                                ProductName = purchaseDetail.Product.Name,
                                ProductCode = purchaseDetail.Product.Code ?? string.Empty,
                                TotalAmount = purchaseDetail.SubtotalAmount,
                                PaidAmount = purchaseDetail.TotalPaidAmount,
                                DocumentDate = purchaseDetail.PurchaseReceiving.ReceiptDate,
                                IsReturn = false
                            };
                        }
                        break;

                    case SetoffDetailType.PurchaseReturnDetail:
                        var purchaseReturnDetail = await context.PurchaseReturnDetails
                            .Include(d => d.PurchaseReturn)
                            .Include(d => d.Product)
                            .FirstOrDefaultAsync(d => d.Id == sourceDetailId);
                        
                        if (purchaseReturnDetail != null)
                        {
                            result = new UnsettledDetailDto
                            {
                                SourceDetailType = SetoffDetailType.PurchaseReturnDetail,
                                SourceDetailId = purchaseReturnDetail.Id,
                                SourceDocumentNumber = purchaseReturnDetail.PurchaseReturn.PurchaseReturnNumber,
                                ProductId = purchaseReturnDetail.ProductId,
                                ProductName = purchaseReturnDetail.Product.Name,
                                ProductCode = purchaseReturnDetail.Product.Code ?? string.Empty,
                                TotalAmount = purchaseReturnDetail.ReturnSubtotalAmount,
                                PaidAmount = purchaseReturnDetail.TotalReceivedAmount,
                                DocumentDate = purchaseReturnDetail.PurchaseReturn.ReturnDate,
                                IsReturn = true
                            };
                        }
                        break;
                }

                // 如果找到了來源明細，查詢該明細的歷史累計沖款和折讓金額
                if (result != null)
                {
                    var historicalSetoff = await context.SetoffProductDetails
                        .Where(d => d.SourceDetailId == sourceDetailId 
                                 && d.SourceDetailType == sourceDetailType)
                        .SumAsync(d => d.CurrentSetoffAmount);

                    var historicalAllowance = await context.SetoffProductDetails
                        .Where(d => d.SourceDetailId == sourceDetailId 
                                 && d.SourceDetailType == sourceDetailType)
                        .SumAsync(d => d.CurrentAllowanceAmount);

                    result.TotalHistoricalSetoffAmount = historicalSetoff;
                    result.TotalHistoricalAllowanceAmount = historicalAllowance;
                }

                return result;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetSourceDetailInfoAsync), GetType(), _logger, new
                {
                    Method = nameof(GetSourceDetailInfoAsync),
                    ServiceType = GetType().Name,
                    SourceDetailType = sourceDetailType,
                    SourceDetailId = sourceDetailId
                });
                return null;
            }
        }

        /// <summary>
        /// 更新來源明細的累計沖款金額
        /// </summary>
        public async Task<ServiceResult> UpdateSourceDetailTotalAmountAsync(int sourceDetailId, SetoffDetailType sourceType)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                // 計算該來源明細的總沖款金額
                var totalSetoff = await context.SetoffProductDetails
                    .Where(d => d.SourceDetailId == sourceDetailId && d.SourceDetailType == sourceType)
                    .SumAsync(d => d.CurrentSetoffAmount + d.CurrentAllowanceAmount);

                // 根據來源類型更新對應的實體
                switch (sourceType)
                {
                    case SetoffDetailType.SalesOrderDetail:
                        var salesDetail = await context.SalesOrderDetails.FindAsync(sourceDetailId);
                        if (salesDetail != null)
                        {
                            salesDetail.TotalReceivedAmount = totalSetoff;
                            salesDetail.IsSettled = salesDetail.TotalReceivedAmount >= salesDetail.SubtotalAmount;
                        }
                        break;

                    case SetoffDetailType.SalesReturnDetail:
                        var salesReturnDetail = await context.SalesReturnDetails.FindAsync(sourceDetailId);
                        if (salesReturnDetail != null)
                        {
                            salesReturnDetail.TotalPaidAmount = totalSetoff;
                            salesReturnDetail.IsSettled = salesReturnDetail.TotalPaidAmount >= salesReturnDetail.ReturnSubtotalAmount;
                        }
                        break;

                    case SetoffDetailType.PurchaseReceivingDetail:
                        var purchaseDetail = await context.PurchaseReceivingDetails.FindAsync(sourceDetailId);
                        if (purchaseDetail != null)
                        {
                            purchaseDetail.TotalPaidAmount = totalSetoff;
                            purchaseDetail.IsSettled = purchaseDetail.TotalPaidAmount >= purchaseDetail.SubtotalAmount;
                        }
                        break;

                    case SetoffDetailType.PurchaseReturnDetail:
                        var purchaseReturnDetail = await context.PurchaseReturnDetails.FindAsync(sourceDetailId);
                        if (purchaseReturnDetail != null)
                        {
                            purchaseReturnDetail.TotalReceivedAmount = totalSetoff;
                            purchaseReturnDetail.IsSettled = purchaseReturnDetail.TotalReceivedAmount >= purchaseReturnDetail.ReturnSubtotalAmount;
                        }
                        break;
                }

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateSourceDetailTotalAmountAsync), GetType(), _logger, new
                {
                    Method = nameof(UpdateSourceDetailTotalAmountAsync),
                    ServiceType = GetType().Name,
                    SourceDetailId = sourceDetailId,
                    SourceType = sourceType
                });
                return ServiceResult.Failure("更新累計金額時發生錯誤");
            }
        }

        /// <summary>
        /// 批次建立沖銷商品明細（含驗證）
        /// </summary>
        public async Task<ServiceResult> CreateBatchWithValidationAsync(List<SetoffProductDetail> details)
        {
            try
            {
                if (details == null || !details.Any())
                    return ServiceResult.Success(); // 編輯模式下未異動時，直接回傳成功

                using var context = await _contextFactory.CreateDbContextAsync();

                // 區分新增與更新的明細
                var detailsToAdd = new List<SetoffProductDetail>();
                var detailsToUpdate = new List<SetoffProductDetail>();

                foreach (var detail in details)
                {
                    if (detail.Id > 0)
                    {
                        // 編輯模式：需要更新
                        detailsToUpdate.Add(detail);
                    }
                    else
                    {
                        // 新增模式
                        detailsToAdd.Add(detail);
                    }
                }

                // 驗證每筆明細
                foreach (var detail in details)
                {
                    var validation = await ValidateSetoffAmountAsync(
                        detail.SourceDetailId,
                        detail.SourceDetailType,
                        detail.CurrentSetoffAmount,
                        detail.CurrentAllowanceAmount);

                    if (!validation.IsSuccess)
                        return validation;
                }

                // 處理新增的明細
                foreach (var detail in detailsToAdd)
                {
                    // 查詢該來源明細的既有累計金額
                    var existingTotal = await context.SetoffProductDetails
                        .Where(d => d.SourceDetailId == detail.SourceDetailId 
                                 && d.SourceDetailType == detail.SourceDetailType)
                        .SumAsync(d => d.CurrentSetoffAmount);
                    
                    var existingAllowance = await context.SetoffProductDetails
                        .Where(d => d.SourceDetailId == detail.SourceDetailId 
                                 && d.SourceDetailType == detail.SourceDetailType)
                        .SumAsync(d => d.CurrentAllowanceAmount);
                    
                    // 設定累計金額
                    detail.TotalSetoffAmount = existingTotal + detail.CurrentSetoffAmount;
                    detail.TotalAllowanceAmount = existingAllowance + detail.CurrentAllowanceAmount;
                    
                    // 設定建立資訊
                    detail.CreatedAt = DateTime.UtcNow;
                    detail.UpdatedAt = DateTime.UtcNow;
                    if (detail.Status == default)
                    {
                        detail.Status = EntityStatus.Active;
                    }
                }

                // 處理更新的明細
                foreach (var detail in detailsToUpdate)
                {
                    var existingEntity = await context.SetoffProductDetails.FindAsync(detail.Id);
                    if (existingEntity != null)
                    {
                        // 計算累計金額的差異
                        var setoffDiff = detail.CurrentSetoffAmount - existingEntity.CurrentSetoffAmount;
                        var allowanceDiff = detail.CurrentAllowanceAmount - existingEntity.CurrentAllowanceAmount;
                        
                        // 更新欄位
                        existingEntity.CurrentSetoffAmount = detail.CurrentSetoffAmount;
                        existingEntity.CurrentAllowanceAmount = detail.CurrentAllowanceAmount;
                        existingEntity.TotalSetoffAmount += setoffDiff;
                        existingEntity.TotalAllowanceAmount += allowanceDiff;
                        existingEntity.UpdatedAt = DateTime.UtcNow;
                    }
                }

                // 批次新增
                if (detailsToAdd.Any())
                {
                    await context.SetoffProductDetails.AddRangeAsync(detailsToAdd);
                }
                
                await context.SaveChangesAsync();

                // 更新來源明細的累計金額
                var sourceDetails = details
                    .Select(d => new { d.SourceDetailId, d.SourceDetailType })
                    .Distinct();
                    
                foreach (var source in sourceDetails)
                {
                    await UpdateSourceDetailTotalAmountAsync(source.SourceDetailId, source.SourceDetailType);
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateBatchWithValidationAsync), GetType(), _logger, new
                {
                    Method = nameof(CreateBatchWithValidationAsync),
                    ServiceType = GetType().Name,
                    DetailCount = details?.Count ?? 0
                });
                return ServiceResult.Failure($"批次建立沖銷明細時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 檢查沖款金額是否超過未結清餘額
        /// </summary>
        public async Task<ServiceResult> ValidateSetoffAmountAsync(
            int sourceDetailId,
            SetoffDetailType sourceType,
            decimal currentSetoffAmount,
            decimal currentAllowanceAmount)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                decimal totalAmount = 0;
                decimal paidAmount = 0;

                // 根據來源類型取得金額資訊
                switch (sourceType)
                {
                    case SetoffDetailType.SalesOrderDetail:
                        var salesDetail = await context.SalesOrderDetails.FindAsync(sourceDetailId);
                        if (salesDetail == null)
                            return ServiceResult.Failure("找不到對應的銷貨明細");
                        totalAmount = salesDetail.SubtotalAmount;
                        paidAmount = salesDetail.TotalReceivedAmount;
                        break;

                    case SetoffDetailType.SalesReturnDetail:
                        var salesReturnDetail = await context.SalesReturnDetails.FindAsync(sourceDetailId);
                        if (salesReturnDetail == null)
                            return ServiceResult.Failure("找不到對應的銷貨退回明細");
                        totalAmount = salesReturnDetail.ReturnSubtotalAmount;
                        paidAmount = salesReturnDetail.TotalPaidAmount;
                        break;

                    case SetoffDetailType.PurchaseReceivingDetail:
                        var purchaseDetail = await context.PurchaseReceivingDetails.FindAsync(sourceDetailId);
                        if (purchaseDetail == null)
                            return ServiceResult.Failure("找不到對應的採購進貨明細");
                        totalAmount = purchaseDetail.SubtotalAmount;
                        paidAmount = purchaseDetail.TotalPaidAmount;
                        break;

                    case SetoffDetailType.PurchaseReturnDetail:
                        var purchaseReturnDetail = await context.PurchaseReturnDetails.FindAsync(sourceDetailId);
                        if (purchaseReturnDetail == null)
                            return ServiceResult.Failure("找不到對應的採購退回明細");
                        totalAmount = purchaseReturnDetail.ReturnSubtotalAmount;
                        paidAmount = purchaseReturnDetail.TotalReceivedAmount;
                        break;
                }

                var remainingAmount = totalAmount - paidAmount;
                var totalCurrentAmount = currentSetoffAmount + currentAllowanceAmount;

                // 驗證金額範圍
                if (totalCurrentAmount > remainingAmount)
                {
                    return ServiceResult.Failure($"本次沖款金額({totalCurrentAmount:N2})超過未結清餘額({remainingAmount:N2})");
                }

                if (currentAllowanceAmount < 0)
                {
                    return ServiceResult.Failure("折讓金額不可為負數");
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateSetoffAmountAsync), GetType(), _logger, new
                {
                    Method = nameof(ValidateSetoffAmountAsync),
                    ServiceType = GetType().Name,
                    SourceDetailId = sourceDetailId,
                    SourceType = sourceType,
                    CurrentSetoffAmount = currentSetoffAmount,
                    CurrentAllowanceAmount = currentAllowanceAmount
                });
                return ServiceResult.Failure("驗證沖款金額時發生錯誤");
            }
        }

        /// <summary>
        /// 實作搜尋功能
        /// </summary>
        public override async Task<List<SetoffProductDetail>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                var searchTermLower = searchTerm.ToLower();

                return await context.SetoffProductDetails
                    .Include(d => d.SetoffDocument)
                    .Include(d => d.Product)
                    .Where(d =>
                        d.SetoffDocument.SetoffNumber.ToLower().Contains(searchTermLower) ||
                        d.Product.Name.ToLower().Contains(searchTermLower) ||
                        (d.Product.Code != null && d.Product.Code.ToLower().Contains(searchTermLower)))
                    .OrderByDescending(d => d.CreatedAt)
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
                return new List<SetoffProductDetail>();
            }
        }

        /// <summary>
        /// 實作驗證功能
        /// </summary>
        public override async Task<ServiceResult> ValidateAsync(SetoffProductDetail entity)
        {
            try
            {
                var errors = new List<string>();

                if (entity.SetoffDocumentId <= 0)
                    errors.Add("沖款單據為必填");

                if (entity.ProductId <= 0)
                    errors.Add("商品為必填");

                if (entity.SourceDetailId <= 0)
                    errors.Add("來源明細ID為必填");

                if (entity.CurrentSetoffAmount < 0)
                    errors.Add("本次沖款金額不可為負數");

                if (entity.CurrentAllowanceAmount < 0)
                    errors.Add("本次折讓金額不可為負數");

                // 驗證沖款金額是否超過未結清餘額
                if (entity.CurrentSetoffAmount > 0 || entity.CurrentAllowanceAmount > 0)
                {
                    var amountValidation = await ValidateSetoffAmountAsync(
                        entity.SourceDetailId,
                        entity.SourceDetailType,
                        entity.CurrentSetoffAmount,
                        entity.CurrentAllowanceAmount);

                    if (!amountValidation.IsSuccess)
                        errors.Add(amountValidation.ErrorMessage);
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
                    EntityId = entity.Id
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }
    }
}
