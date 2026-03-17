using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 沖銷品項明細服務實作
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
                return ServiceResult<SetoffProductDetail>.Failure($"新增沖銷品項明細時發生錯誤: {ex.Message}");
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
                return ServiceResult<SetoffProductDetail>.Failure($"更新沖銷品項明細時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 覆寫 BuildGetAllQuery 以包含關聯資料
        /// </summary>
        protected override IQueryable<SetoffProductDetail> BuildGetAllQuery(AppDbContext context)
        {
            return context.SetoffProductDetails
                .Include(d => d.SetoffDocument)
                .Include(d => d.Product)
                .OrderByDescending(d => d.CreatedAt);
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
        /// 根據沖款單ID取得品項明細
        /// </summary>
        public async Task<List<SetoffProductDetail>> GetBySetoffDocumentIdAsync(int setoffDocumentId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SetoffProductDetails
                    .AsNoTracking()  // 不追蹤實體，避免後續操作時的追蹤衝突
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
        /// 計算含稅金額（根據稅率計算方法）
        /// </summary>
        private static decimal CalculateTaxInclusiveAmount(decimal subtotal, decimal? taxRate, TaxCalculationMethod taxMethod)
        {
            var rate = taxRate ?? 0;
            return taxMethod switch
            {
                // 稅外加：金額 = 小計 × (1 + 稅率/100)
                TaxCalculationMethod.TaxExclusive => Math.Round(subtotal * (1 + rate / 100), 0),
                // 稅內含：金額 = 小計（稅已包含在單價中）
                TaxCalculationMethod.TaxInclusive => Math.Round(subtotal, 0),
                // 免稅：金額 = 小計
                TaxCalculationMethod.NoTax => Math.Round(subtotal, 0),
                _ => Math.Round(subtotal, 0)
            };
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

                // 注意：沖款只針對真實發生交易的單據，銷貨訂單（SalesOrder）尚未出貨
                // 應收帳款未成立，故不列入沖款選項。
                // 有效來源：SalesDelivery（出貨）、SalesReturn（退回）

                // 1. 查詢未結清的銷貨退回明細（包含稅率計算所需欄位）
                var salesReturnRawData = await context.SalesReturnDetails
                    .Include(d => d.SalesReturn)
                    .Include(d => d.Product)
                    .Where(d => d.SalesReturn.CustomerId == customerId && !d.IsSettled)
                    .Select(d => new
                    {
                        d.Id,
                        d.SalesReturn.Code,
                        d.ProductId,
                        ProductName = d.Product.Name,
                        ProductCode = d.Product.Code ?? string.Empty,
                        d.ReturnSubtotalAmount,
                        d.TaxRate,
                        d.SalesReturn.TaxCalculationMethod,
                        d.TotalPaidAmount,
                        d.SalesReturn.ReturnDate
                    })
                    .ToListAsync();

                var salesReturnDetails = salesReturnRawData.Select(d => new UnsettledDetailDto
                {
                    SourceDetailType = SetoffDetailType.SalesReturnDetail,
                    SourceDetailId = d.Id,
                    SourceDocumentNumber = d.Code ?? string.Empty,
                    ProductId = d.ProductId,
                    ProductName = d.ProductName!,
                    ProductCode = d.ProductCode,
                    TotalAmount = CalculateTaxInclusiveAmount(d.ReturnSubtotalAmount, d.TaxRate, d.TaxCalculationMethod),
                    PaidAmount = d.TotalPaidAmount,
                    DocumentDate = d.ReturnDate,
                    IsReturn = true
                }).ToList();

                results.AddRange(salesReturnDetails);

                // 3. 查詢未結清的銷貨出貨明細（包含稅率計算所需欄位）
                var salesDeliveryRawData = await context.SalesDeliveryDetails
                    .Include(d => d.SalesDelivery)
                    .Include(d => d.Product)
                    .Where(d => d.SalesDelivery.CustomerId == customerId && !d.IsSettled)
                    .Select(d => new
                    {
                        d.Id,
                        d.SalesDelivery.Code,
                        d.ProductId,
                        ProductName = d.Product.Name,
                        ProductCode = d.Product.Code ?? string.Empty,
                        d.SubtotalAmount,
                        d.TaxRate,
                        d.SalesDelivery.TaxCalculationMethod,
                        d.TotalReceivedAmount,
                        d.SalesDelivery.DeliveryDate
                    })
                    .ToListAsync();

                var salesDeliveryDetails = salesDeliveryRawData.Select(d => new UnsettledDetailDto
                {
                    SourceDetailType = SetoffDetailType.SalesDeliveryDetail,
                    SourceDetailId = d.Id,
                    SourceDocumentNumber = d.Code ?? string.Empty,
                    ProductId = d.ProductId,
                    ProductName = d.ProductName!,
                    ProductCode = d.ProductCode,
                    TotalAmount = CalculateTaxInclusiveAmount(d.SubtotalAmount, d.TaxRate, d.TaxCalculationMethod),
                    PaidAmount = d.TotalReceivedAmount,
                    DocumentDate = d.DeliveryDate,
                    IsReturn = false
                }).ToList();

                results.AddRange(salesDeliveryDetails);

                // 4. 批次查詢歷史累計沖款和折讓金額（修正 Bug-6：原實作為 N+1，每筆明細兩次 DB 查詢）
                // 改為一次批次查詢全部相關 SetoffProductDetail，在記憶體中 GroupBy 加總
                if (results.Any())
                {
                    var returnIds   = salesReturnDetails.Select(d => d.SourceDetailId).ToList();
                    var deliveryIds = salesDeliveryDetails.Select(d => d.SourceDetailId).ToList();

                    var setoffSums = await context.SetoffProductDetails
                        .Where(d =>
                            (d.SourceDetailType == SetoffDetailType.SalesReturnDetail   && returnIds.Contains(d.SourceDetailId)) ||
                            (d.SourceDetailType == SetoffDetailType.SalesDeliveryDetail && deliveryIds.Contains(d.SourceDetailId)))
                        .GroupBy(d => new { d.SourceDetailId, d.SourceDetailType })
                        .Select(g => new
                        {
                            g.Key.SourceDetailId,
                            g.Key.SourceDetailType,
                            TotalSetoff    = g.Sum(d => d.CurrentSetoffAmount),
                            TotalAllowance = g.Sum(d => d.CurrentAllowanceAmount)
                        })
                        .ToListAsync();

                    var setoffLookup = setoffSums.ToDictionary(
                        x => (x.SourceDetailId, x.SourceDetailType));

                    foreach (var detail in results)
                    {
                        if (setoffLookup.TryGetValue((detail.SourceDetailId, detail.SourceDetailType), out var sums))
                        {
                            detail.TotalHistoricalSetoffAmount   = sums.TotalSetoff;
                            detail.TotalHistoricalAllowanceAmount = sums.TotalAllowance;
                        }
                    }
                }

                // 5. 過濾掉應收金額為 0 或未沖款餘額為 0 的明細
                results = results
                    .Where(r => r.TotalAmount != 0 && r.RemainingAmount != 0)
                    .ToList();

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

                // 1. 查詢未結清的採購進貨明細（包含稅率計算所需欄位）
                var purchaseReceivingRawData = await context.PurchaseReceivingDetails
                    .Include(d => d.PurchaseReceiving)
                    .Include(d => d.Product)
                    .Where(d => d.PurchaseReceiving.SupplierId == supplierId && !d.IsSettled)
                    .Select(d => new
                    {
                        d.Id,
                        d.PurchaseReceiving.Code,
                        d.ProductId,
                        ProductName = d.Product.Name,
                        ProductCode = d.Product.Code ?? string.Empty,
                        d.SubtotalAmount,
                        d.TaxRate,
                        d.PurchaseReceiving.TaxCalculationMethod,
                        d.TotalPaidAmount,
                        d.PurchaseReceiving.ReceiptDate
                    })
                    .ToListAsync();

                var purchaseReceivingDetails = purchaseReceivingRawData.Select(d => new UnsettledDetailDto
                {
                    SourceDetailType = SetoffDetailType.PurchaseReceivingDetail,
                    SourceDetailId = d.Id,
                    SourceDocumentNumber = d.Code ?? string.Empty,
                    ProductId = d.ProductId,
                    ProductName = d.ProductName!,
                    ProductCode = d.ProductCode,
                    TotalAmount = CalculateTaxInclusiveAmount(d.SubtotalAmount, d.TaxRate, d.TaxCalculationMethod),
                    PaidAmount = d.TotalPaidAmount,
                    DocumentDate = d.ReceiptDate,
                    IsReturn = false
                }).ToList();

                results.AddRange(purchaseReceivingDetails);

                // 2. 查詢未結清的採購退回明細（包含稅率計算所需欄位）
                var purchaseReturnRawData = await context.PurchaseReturnDetails
                    .Include(d => d.PurchaseReturn)
                    .Include(d => d.Product)
                    .Where(d => d.PurchaseReturn.SupplierId == supplierId && !d.IsSettled)
                    .Select(d => new
                    {
                        d.Id,
                        d.PurchaseReturn.Code,
                        d.ProductId,
                        ProductName = d.Product.Name,
                        ProductCode = d.Product.Code ?? string.Empty,
                        d.ReturnSubtotalAmount,
                        d.TaxRate,
                        d.PurchaseReturn.TaxCalculationMethod,
                        d.TotalReceivedAmount,
                        d.PurchaseReturn.ReturnDate
                    })
                    .ToListAsync();

                var purchaseReturnDetails = purchaseReturnRawData.Select(d => new UnsettledDetailDto
                {
                    SourceDetailType = SetoffDetailType.PurchaseReturnDetail,
                    SourceDetailId = d.Id,
                    SourceDocumentNumber = d.Code ?? string.Empty,
                    ProductId = d.ProductId,
                    ProductName = d.ProductName!,
                    ProductCode = d.ProductCode,
                    TotalAmount = CalculateTaxInclusiveAmount(d.ReturnSubtotalAmount, d.TaxRate, d.TaxCalculationMethod),
                    PaidAmount = d.TotalReceivedAmount,
                    DocumentDate = d.ReturnDate,
                    IsReturn = true
                }).ToList();

                results.AddRange(purchaseReturnDetails);

                // 3. 批次查詢歷史累計沖款和折讓金額（修正 Bug-6：原實作為 N+1，每筆明細兩次 DB 查詢）
                // 改為一次批次查詢全部相關 SetoffProductDetail，在記憶體中 GroupBy 加總
                if (results.Any())
                {
                    var receivingIds = purchaseReceivingDetails.Select(d => d.SourceDetailId).ToList();
                    var returnIds    = purchaseReturnDetails.Select(d => d.SourceDetailId).ToList();

                    var setoffSums = await context.SetoffProductDetails
                        .Where(d =>
                            (d.SourceDetailType == SetoffDetailType.PurchaseReceivingDetail && receivingIds.Contains(d.SourceDetailId)) ||
                            (d.SourceDetailType == SetoffDetailType.PurchaseReturnDetail    && returnIds.Contains(d.SourceDetailId)))
                        .GroupBy(d => new { d.SourceDetailId, d.SourceDetailType })
                        .Select(g => new
                        {
                            g.Key.SourceDetailId,
                            g.Key.SourceDetailType,
                            TotalSetoff    = g.Sum(d => d.CurrentSetoffAmount),
                            TotalAllowance = g.Sum(d => d.CurrentAllowanceAmount)
                        })
                        .ToListAsync();

                    var setoffLookup = setoffSums.ToDictionary(
                        x => (x.SourceDetailId, x.SourceDetailType));

                    foreach (var detail in results)
                    {
                        if (setoffLookup.TryGetValue((detail.SourceDetailId, detail.SourceDetailType), out var sums))
                        {
                            detail.TotalHistoricalSetoffAmount   = sums.TotalSetoff;
                            detail.TotalHistoricalAllowanceAmount = sums.TotalAllowance;
                        }
                    }
                }

                // 4. 過濾掉應付金額為 0 或未沖款餘額為 0 的明細
                results = results
                    .Where(r => r.TotalAmount != 0 && r.RemainingAmount != 0)
                    .ToList();

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
                                SourceDocumentNumber = salesDetail.SalesOrder.Code ?? string.Empty,
                                ProductId = salesDetail.ProductId,
                                ProductName = salesDetail.Product!.Name!,
                                ProductCode = salesDetail.Product.Code ?? string.Empty,
                                TotalAmount = CalculateTaxInclusiveAmount(
                                    salesDetail.SubtotalAmount,
                                    salesDetail.TaxRate,
                                    salesDetail.SalesOrder.TaxCalculationMethod),
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
                                SourceDocumentNumber = salesReturnDetail.SalesReturn.Code ?? string.Empty,
                                ProductId = salesReturnDetail.ProductId,
                                ProductName = salesReturnDetail.Product!.Name!,
                                ProductCode = salesReturnDetail.Product.Code ?? string.Empty,
                                TotalAmount = CalculateTaxInclusiveAmount(
                                    salesReturnDetail.ReturnSubtotalAmount,
                                    salesReturnDetail.TaxRate,
                                    salesReturnDetail.SalesReturn.TaxCalculationMethod),
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
                                SourceDocumentNumber = purchaseDetail.PurchaseReceiving.Code ?? string.Empty,
                                ProductId = purchaseDetail.ProductId,
                                ProductName = purchaseDetail.Product!.Name!,
                                ProductCode = purchaseDetail.Product.Code ?? string.Empty,
                                TotalAmount = CalculateTaxInclusiveAmount(
                                    purchaseDetail.SubtotalAmount,
                                    purchaseDetail.TaxRate,
                                    purchaseDetail.PurchaseReceiving.TaxCalculationMethod),
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
                                SourceDocumentNumber = purchaseReturnDetail.PurchaseReturn.Code ?? string.Empty,
                                ProductId = purchaseReturnDetail.ProductId,
                                ProductName = purchaseReturnDetail.Product!.Name!,
                                ProductCode = purchaseReturnDetail.Product.Code ?? string.Empty,
                                TotalAmount = CalculateTaxInclusiveAmount(
                                    purchaseReturnDetail.ReturnSubtotalAmount,
                                    purchaseReturnDetail.TaxRate,
                                    purchaseReturnDetail.PurchaseReturn.TaxCalculationMethod),
                                PaidAmount = purchaseReturnDetail.TotalReceivedAmount,
                                DocumentDate = purchaseReturnDetail.PurchaseReturn.ReturnDate,
                                IsReturn = true
                            };
                        }
                        break;

                    case SetoffDetailType.SalesDeliveryDetail:
                        var salesDeliveryDetail = await context.SalesDeliveryDetails
                            .Include(d => d.SalesDelivery)
                            .Include(d => d.Product)
                            .FirstOrDefaultAsync(d => d.Id == sourceDetailId);
                        
                        if (salesDeliveryDetail != null)
                        {
                            result = new UnsettledDetailDto
                            {
                                SourceDetailType = SetoffDetailType.SalesDeliveryDetail,
                                SourceDetailId = salesDeliveryDetail.Id,
                                SourceDocumentNumber = salesDeliveryDetail.SalesDelivery.Code ?? string.Empty,
                                ProductId = salesDeliveryDetail.ProductId,
                                ProductName = salesDeliveryDetail.Product!.Name!,
                                ProductCode = salesDeliveryDetail.Product.Code ?? string.Empty,
                                TotalAmount = CalculateTaxInclusiveAmount(
                                    salesDeliveryDetail.SubtotalAmount,
                                    salesDeliveryDetail.TaxRate,
                                    salesDeliveryDetail.SalesDelivery.TaxCalculationMethod),
                                PaidAmount = salesDeliveryDetail.TotalReceivedAmount,
                                DocumentDate = salesDeliveryDetail.SalesDelivery.DeliveryDate,
                                IsReturn = false
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

                // 根據來源類型更新對應的實體（使用含稅金額判斷是否結清）
                switch (sourceType)
                {
                    case SetoffDetailType.SalesOrderDetail:
                        var salesDetail = await context.SalesOrderDetails
                            .Include(d => d.SalesOrder)
                            .FirstOrDefaultAsync(d => d.Id == sourceDetailId);
                        if (salesDetail != null)
                        {
                            salesDetail.TotalReceivedAmount = totalSetoff;
                            var taxInclusiveAmount = CalculateTaxInclusiveAmount(
                                salesDetail.SubtotalAmount,
                                salesDetail.TaxRate,
                                salesDetail.SalesOrder.TaxCalculationMethod);
                            salesDetail.IsSettled = salesDetail.TotalReceivedAmount >= taxInclusiveAmount;
                        }
                        break;

                    case SetoffDetailType.SalesReturnDetail:
                        var salesReturnDetail = await context.SalesReturnDetails
                            .Include(d => d.SalesReturn)
                            .FirstOrDefaultAsync(d => d.Id == sourceDetailId);
                        if (salesReturnDetail != null)
                        {
                            salesReturnDetail.TotalPaidAmount = totalSetoff;
                            var taxInclusiveAmount = CalculateTaxInclusiveAmount(
                                salesReturnDetail.ReturnSubtotalAmount,
                                salesReturnDetail.TaxRate,
                                salesReturnDetail.SalesReturn.TaxCalculationMethod);
                            salesReturnDetail.IsSettled = salesReturnDetail.TotalPaidAmount >= taxInclusiveAmount;
                        }
                        break;

                    case SetoffDetailType.PurchaseReceivingDetail:
                        var purchaseDetail = await context.PurchaseReceivingDetails
                            .Include(d => d.PurchaseReceiving)
                            .FirstOrDefaultAsync(d => d.Id == sourceDetailId);
                        if (purchaseDetail != null)
                        {
                            purchaseDetail.TotalPaidAmount = totalSetoff;
                            var taxInclusiveAmount = CalculateTaxInclusiveAmount(
                                purchaseDetail.SubtotalAmount,
                                purchaseDetail.TaxRate,
                                purchaseDetail.PurchaseReceiving.TaxCalculationMethod);
                            purchaseDetail.IsSettled = purchaseDetail.TotalPaidAmount >= taxInclusiveAmount;
                        }
                        break;

                    case SetoffDetailType.PurchaseReturnDetail:
                        var purchaseReturnDetail = await context.PurchaseReturnDetails
                            .Include(d => d.PurchaseReturn)
                            .FirstOrDefaultAsync(d => d.Id == sourceDetailId);
                        if (purchaseReturnDetail != null)
                        {
                            purchaseReturnDetail.TotalReceivedAmount = totalSetoff;
                            var taxInclusiveAmount = CalculateTaxInclusiveAmount(
                                purchaseReturnDetail.ReturnSubtotalAmount,
                                purchaseReturnDetail.TaxRate,
                                purchaseReturnDetail.PurchaseReturn.TaxCalculationMethod);
                            purchaseReturnDetail.IsSettled = purchaseReturnDetail.TotalReceivedAmount >= taxInclusiveAmount;
                        }
                        break;

                    case SetoffDetailType.SalesDeliveryDetail:
                        var salesDeliveryDetail = await context.SalesDeliveryDetails
                            .Include(d => d.SalesDelivery)
                            .FirstOrDefaultAsync(d => d.Id == sourceDetailId);
                        if (salesDeliveryDetail != null)
                        {
                            salesDeliveryDetail.TotalReceivedAmount = totalSetoff;
                            var taxInclusiveAmount = CalculateTaxInclusiveAmount(
                                salesDeliveryDetail.SubtotalAmount,
                                salesDeliveryDetail.TaxRate,
                                salesDeliveryDetail.SalesDelivery.TaxCalculationMethod);
                            salesDeliveryDetail.IsSettled = salesDeliveryDetail.TotalReceivedAmount >= taxInclusiveAmount;
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
        /// 在現有 context 內更新來源明細的累計沖款金額（不自行建立 context，不自行呼叫 SaveChangesAsync）
        /// 供 CreateBatchWithValidationAsync 在同一交易內使用
        /// </summary>
        private async Task UpdateSourceDetailCacheInContextAsync(AppDbContext context, int sourceDetailId, SetoffDetailType sourceType)
        {
            var totalSetoff = await context.SetoffProductDetails
                .Where(d => d.SourceDetailId == sourceDetailId && d.SourceDetailType == sourceType)
                .SumAsync(d => d.CurrentSetoffAmount + d.CurrentAllowanceAmount);

            switch (sourceType)
            {
                case SetoffDetailType.SalesOrderDetail:
                    var salesDetail = await context.SalesOrderDetails
                        .Include(d => d.SalesOrder)
                        .FirstOrDefaultAsync(d => d.Id == sourceDetailId);
                    if (salesDetail != null)
                    {
                        salesDetail.TotalReceivedAmount = totalSetoff;
                        salesDetail.IsSettled = salesDetail.TotalReceivedAmount >= CalculateTaxInclusiveAmount(
                            salesDetail.SubtotalAmount, salesDetail.TaxRate, salesDetail.SalesOrder.TaxCalculationMethod);
                    }
                    break;

                case SetoffDetailType.SalesReturnDetail:
                    var salesReturnDetail = await context.SalesReturnDetails
                        .Include(d => d.SalesReturn)
                        .FirstOrDefaultAsync(d => d.Id == sourceDetailId);
                    if (salesReturnDetail != null)
                    {
                        salesReturnDetail.TotalPaidAmount = totalSetoff;
                        salesReturnDetail.IsSettled = salesReturnDetail.TotalPaidAmount >= CalculateTaxInclusiveAmount(
                            salesReturnDetail.ReturnSubtotalAmount, salesReturnDetail.TaxRate, salesReturnDetail.SalesReturn.TaxCalculationMethod);
                    }
                    break;

                case SetoffDetailType.PurchaseReceivingDetail:
                    var purchaseDetail = await context.PurchaseReceivingDetails
                        .Include(d => d.PurchaseReceiving)
                        .FirstOrDefaultAsync(d => d.Id == sourceDetailId);
                    if (purchaseDetail != null)
                    {
                        purchaseDetail.TotalPaidAmount = totalSetoff;
                        purchaseDetail.IsSettled = purchaseDetail.TotalPaidAmount >= CalculateTaxInclusiveAmount(
                            purchaseDetail.SubtotalAmount, purchaseDetail.TaxRate, purchaseDetail.PurchaseReceiving.TaxCalculationMethod);
                    }
                    break;

                case SetoffDetailType.PurchaseReturnDetail:
                    var purchaseReturnDetail = await context.PurchaseReturnDetails
                        .Include(d => d.PurchaseReturn)
                        .FirstOrDefaultAsync(d => d.Id == sourceDetailId);
                    if (purchaseReturnDetail != null)
                    {
                        purchaseReturnDetail.TotalReceivedAmount = totalSetoff;
                        purchaseReturnDetail.IsSettled = purchaseReturnDetail.TotalReceivedAmount >= CalculateTaxInclusiveAmount(
                            purchaseReturnDetail.ReturnSubtotalAmount, purchaseReturnDetail.TaxRate, purchaseReturnDetail.PurchaseReturn.TaxCalculationMethod);
                    }
                    break;

                case SetoffDetailType.SalesDeliveryDetail:
                    var salesDeliveryDetail = await context.SalesDeliveryDetails
                        .Include(d => d.SalesDelivery)
                        .FirstOrDefaultAsync(d => d.Id == sourceDetailId);
                    if (salesDeliveryDetail != null)
                    {
                        salesDeliveryDetail.TotalReceivedAmount = totalSetoff;
                        salesDeliveryDetail.IsSettled = salesDeliveryDetail.TotalReceivedAmount >= CalculateTaxInclusiveAmount(
                            salesDeliveryDetail.SubtotalAmount, salesDeliveryDetail.TaxRate, salesDeliveryDetail.SalesDelivery.TaxCalculationMethod);
                    }
                    break;
            }
        }

        /// <summary>
        /// 批次建立沖銷品項明細（含驗證）
        /// </summary>
        public async Task<ServiceResult> CreateBatchWithValidationAsync(List<SetoffProductDetail> details)
        {
            try
            {
                if (details == null || !details.Any())
                    return ServiceResult.Success(); // 編輯模式下未異動時，直接回傳成功

                using var context = await _contextFactory.CreateDbContextAsync();
                await using var transaction = await context.Database.BeginTransactionAsync();

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

                // 計算批次內各 SourceDetailId 的累計金額（防止同批次多筆合計超額）
                var batchTotals = details
                    .GroupBy(d => (d.SourceDetailId, d.SourceDetailType))
                    .ToDictionary(
                        g => g.Key,
                        g => (
                            Setoff: g.Sum(d => d.CurrentSetoffAmount),
                            Allowance: g.Sum(d => d.CurrentAllowanceAmount)
                        ));

                // 驗證每個 SourceDetail 的批次合計不超過可用餘額
                foreach (var ((sourceDetailId, sourceType), totals) in batchTotals)
                {
                    // 編輯模式：找出同一來源下所有要更新的明細 ID，一併排除原始金額
                    var existingIdsInBatch = details
                        .Where(d => d.SourceDetailId == sourceDetailId
                                 && d.SourceDetailType == sourceType
                                 && d.Id > 0)
                        .Select(d => (int?)d.Id)
                        .ToList();

                    // 使用第一個排除 ID（ValidateSetoffAmountAsync 只支援排除一筆）
                    // 若批次內有多筆既有記錄，需逐筆驗證個別差值
                    if (existingIdsInBatch.Count <= 1)
                    {
                        var validation = await ValidateSetoffAmountAsync(
                            sourceDetailId, sourceType,
                            totals.Setoff, totals.Allowance,
                            existingIdsInBatch.FirstOrDefault());
                        if (!validation.IsSuccess)
                            return validation;
                    }
                    else
                    {
                        // 多筆既有記錄：逐筆驗證個別差值
                        foreach (var detail in details.Where(d => d.SourceDetailId == sourceDetailId
                                                               && d.SourceDetailType == sourceType))
                        {
                            var validation = await ValidateSetoffAmountAsync(
                                detail.SourceDetailId, detail.SourceDetailType,
                                detail.CurrentSetoffAmount, detail.CurrentAllowanceAmount,
                                detail.Id > 0 ? detail.Id : null);
                            if (!validation.IsSuccess)
                                return validation;
                        }
                    }
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
                        existingEntity.Remarks = detail.Remarks;
                        existingEntity.UpdatedAt = DateTime.UtcNow;
                    }
                }

                // 批次新增
                if (detailsToAdd.Any())
                {
                    await context.SetoffProductDetails.AddRangeAsync(detailsToAdd);
                }

                await context.SaveChangesAsync(); // 儲存明細（交易內）

                // 更新來源明細的累計金額（在同一 context/交易內，避免跨 context 不一致）
                var sourceDetails = details
                    .Select(d => new { d.SourceDetailId, d.SourceDetailType })
                    .Distinct();

                foreach (var source in sourceDetails)
                {
                    await UpdateSourceDetailCacheInContextAsync(context, source.SourceDetailId, source.SourceDetailType);
                }

                await context.SaveChangesAsync(); // 儲存來源明細快取更新（交易內）
                await transaction.CommitAsync();

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
        /// <param name="sourceDetailId">來源明細ID</param>
        /// <param name="sourceType">來源明細類型</param>
        /// <param name="currentSetoffAmount">本次沖款金額</param>
        /// <param name="currentAllowanceAmount">本次折讓金額</param>
        /// <param name="excludeSetoffDetailId">要排除的沖銷明細ID（編輯模式用）</param>
        public async Task<ServiceResult> ValidateSetoffAmountAsync(
            int sourceDetailId,
            SetoffDetailType sourceType,
            decimal currentSetoffAmount,
            decimal currentAllowanceAmount,
            int? excludeSetoffDetailId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                decimal totalAmount = 0;
                decimal paidAmount = 0;

                // 根據來源類型取得金額資訊（使用含稅金額）
                switch (sourceType)
                {
                    case SetoffDetailType.SalesOrderDetail:
                        var salesDetail = await context.SalesOrderDetails
                            .Include(d => d.SalesOrder)
                            .FirstOrDefaultAsync(d => d.Id == sourceDetailId);
                        if (salesDetail == null)
                            return ServiceResult.Failure("找不到對應的銷貨明細");
                        totalAmount = CalculateTaxInclusiveAmount(
                            salesDetail.SubtotalAmount,
                            salesDetail.TaxRate,
                            salesDetail.SalesOrder.TaxCalculationMethod);
                        paidAmount = salesDetail.TotalReceivedAmount;
                        break;

                    case SetoffDetailType.SalesReturnDetail:
                        var salesReturnDetail = await context.SalesReturnDetails
                            .Include(d => d.SalesReturn)
                            .FirstOrDefaultAsync(d => d.Id == sourceDetailId);
                        if (salesReturnDetail == null)
                            return ServiceResult.Failure("找不到對應的銷貨退回明細");
                        totalAmount = CalculateTaxInclusiveAmount(
                            salesReturnDetail.ReturnSubtotalAmount,
                            salesReturnDetail.TaxRate,
                            salesReturnDetail.SalesReturn.TaxCalculationMethod);
                        paidAmount = salesReturnDetail.TotalPaidAmount;
                        break;

                    case SetoffDetailType.SalesDeliveryDetail:
                        var salesDeliveryDetail = await context.SalesDeliveryDetails
                            .Include(d => d.SalesDelivery)
                            .FirstOrDefaultAsync(d => d.Id == sourceDetailId);
                        if (salesDeliveryDetail == null)
                            return ServiceResult.Failure("找不到對應的銷貨出貨明細");
                        totalAmount = CalculateTaxInclusiveAmount(
                            salesDeliveryDetail.SubtotalAmount,
                            salesDeliveryDetail.TaxRate,
                            salesDeliveryDetail.SalesDelivery.TaxCalculationMethod);
                        paidAmount = salesDeliveryDetail.TotalReceivedAmount;
                        break;

                    case SetoffDetailType.PurchaseReceivingDetail:
                        var purchaseDetail = await context.PurchaseReceivingDetails
                            .Include(d => d.PurchaseReceiving)
                            .FirstOrDefaultAsync(d => d.Id == sourceDetailId);
                        if (purchaseDetail == null)
                            return ServiceResult.Failure("找不到對應的採購進貨明細");
                        totalAmount = CalculateTaxInclusiveAmount(
                            purchaseDetail.SubtotalAmount,
                            purchaseDetail.TaxRate,
                            purchaseDetail.PurchaseReceiving.TaxCalculationMethod);
                        paidAmount = purchaseDetail.TotalPaidAmount;
                        break;

                    case SetoffDetailType.PurchaseReturnDetail:
                        var purchaseReturnDetail = await context.PurchaseReturnDetails
                            .Include(d => d.PurchaseReturn)
                            .FirstOrDefaultAsync(d => d.Id == sourceDetailId);
                        if (purchaseReturnDetail == null)
                            return ServiceResult.Failure("找不到對應的採購退回明細");
                        totalAmount = CalculateTaxInclusiveAmount(
                            purchaseReturnDetail.ReturnSubtotalAmount,
                            purchaseReturnDetail.TaxRate,
                            purchaseReturnDetail.PurchaseReturn.TaxCalculationMethod);
                        paidAmount = purchaseReturnDetail.TotalReceivedAmount;
                        break;
                }

                // 如果是編輯模式，需要扣除原本這筆沖銷明細已經計入的金額
                var previousSetoffAmount = 0m;
                if (excludeSetoffDetailId.HasValue && excludeSetoffDetailId.Value > 0)
                {
                    var existingDetail = await context.SetoffProductDetails
                        .FirstOrDefaultAsync(d => d.Id == excludeSetoffDetailId.Value 
                                                && d.SourceDetailId == sourceDetailId 
                                                && d.SourceDetailType == sourceType);
                    
                    if (existingDetail != null)
                    {
                        previousSetoffAmount = existingDetail.CurrentSetoffAmount + existingDetail.CurrentAllowanceAmount;
                    }
                }

                // 計算可用餘額 = 總金額 - 已付金額 + 原本這筆明細的沖銷金額（編輯模式）
                var remainingAmount = totalAmount - paidAmount + previousSetoffAmount;
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
                    CurrentAllowanceAmount = currentAllowanceAmount,
                    ExcludeSetoffDetailId = excludeSetoffDetailId
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
                        (d.SetoffDocument.Code != null && d.SetoffDocument.Code.ToLower().Contains(searchTermLower)) ||
                        d.Product!.Name!.ToLower().Contains(searchTermLower) ||
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
                    errors.Add("品項為必填");

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
