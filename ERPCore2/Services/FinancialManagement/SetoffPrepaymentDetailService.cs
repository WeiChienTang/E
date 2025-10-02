using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using ERPCore2.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 沖款預收/預付款明細服務實作
    /// </summary>
    public class SetoffPrepaymentDetailService : GenericManagementService<PrepaymentDetail>, ISetoffPrepaymentDetailService
    {
        /// <summary>
        /// 完整建構子 - 使用 ILogger
        /// </summary>
        public SetoffPrepaymentDetailService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<PrepaymentDetail>> logger) : base(contextFactory, logger)
        {
        }

        /// <summary>
        /// 簡易建構子 - 不使用 ILogger
        /// </summary>
        public SetoffPrepaymentDetailService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        #region 查詢方法

        /// <summary>
        /// 取得應收沖款單的預收款明細
        /// </summary>
        public async Task<List<SetoffPrepaymentDto>> GetByReceivableSetoffIdAsync(int setoffId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var details = await context.SetoffPrepaymentDetails
                    .Include(d => d.Prepayment)
                        .ThenInclude(p => p!.Customer)
                    .Where(d => d.AccountsReceivableSetoffId == setoffId)
                    .ToListAsync();

                return details.Select(d => MapToDto(d)).ToList();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByReceivableSetoffIdAsync), GetType(), _logger, new
                {
                    SetoffId = setoffId
                });
                return new List<SetoffPrepaymentDto>();
            }
        }

        /// <summary>
        /// 取得應付沖款單的預付款明細
        /// </summary>
        public async Task<List<SetoffPrepaymentDto>> GetByPayableSetoffIdAsync(int setoffId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var details = await context.SetoffPrepaymentDetails
                    .Include(d => d.Prepayment)
                        .ThenInclude(p => p!.Supplier)
                    .Where(d => d.AccountsPayableSetoffId == setoffId)
                    .ToListAsync();

                return details.Select(d => MapToDto(d)).ToList();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByPayableSetoffIdAsync), GetType(), _logger, new
                {
                    SetoffId = setoffId
                });
                return new List<SetoffPrepaymentDto>();
            }
        }

        /// <summary>
        /// 取得客戶可用的預收款列表
        /// </summary>
        public async Task<List<SetoffPrepaymentDto>> GetAvailablePrepaymentsByCustomerAsync(int customerId, int? excludeSetoffId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 取得客戶的所有預收款
                var prepayments = await context.Prepayments
                    .Include(p => p.Customer)
                    .Where(p => p.CustomerId == customerId && p.PrepaymentType == PrepaymentType.Prepayment)
                    .ToListAsync();

                var result = new List<SetoffPrepaymentDto>();

                foreach (var prepayment in prepayments)
                {
                    // 計算已用金額（排除指定的沖款單）
                    var usedAmount = await GetUsedAmountAsync(prepayment.Id, excludeSetoffId);
                    var availableAmount = prepayment.Amount - usedAmount;

                    // 只加入有可用餘額的預收款
                    if (availableAmount > 0)
                    {
                        result.Add(new SetoffPrepaymentDto
                        {
                            PrepaymentId = prepayment.Id,
                            Code = prepayment.Code ?? string.Empty,
                            PaymentDate = prepayment.PaymentDate,
                            Amount = prepayment.Amount,
                            UsedAmount = usedAmount,
                            PrepaymentType = prepayment.PrepaymentType,
                            PartnerName = prepayment.Customer?.CompanyName,
                            Remarks = prepayment.Remarks
                        });
                    }
                }

                return result.OrderBy(p => p.PaymentDate).ThenBy(p => p.Code).ToList();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAvailablePrepaymentsByCustomerAsync), GetType(), _logger, new
                {
                    CustomerId = customerId,
                    ExcludeSetoffId = excludeSetoffId
                });
                return new List<SetoffPrepaymentDto>();
            }
        }

        /// <summary>
        /// 取得供應商可用的預付款列表
        /// </summary>
        public async Task<List<SetoffPrepaymentDto>> GetAvailablePrepaidsBySupplierAsync(int supplierId, int? excludeSetoffId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 取得供應商的所有預付款
                var prepayments = await context.Prepayments
                    .Include(p => p.Supplier)
                    .Where(p => p.SupplierId == supplierId && p.PrepaymentType == PrepaymentType.Prepaid)
                    .ToListAsync();

                var result = new List<SetoffPrepaymentDto>();

                foreach (var prepayment in prepayments)
                {
                    // 計算已用金額（排除指定的沖款單）
                    var usedAmount = await GetUsedAmountAsync(prepayment.Id, excludeSetoffId);
                    var availableAmount = prepayment.Amount - usedAmount;

                    // 只加入有可用餘額的預付款
                    if (availableAmount > 0)
                    {
                        result.Add(new SetoffPrepaymentDto
                        {
                            PrepaymentId = prepayment.Id,
                            Code = prepayment.Code ?? string.Empty,
                            PaymentDate = prepayment.PaymentDate,
                            Amount = prepayment.Amount,
                            UsedAmount = usedAmount,
                            PrepaymentType = prepayment.PrepaymentType,
                            PartnerName = prepayment.Supplier?.CompanyName,
                            Remarks = prepayment.Remarks
                        });
                    }
                }

                return result.OrderBy(p => p.PaymentDate).ThenBy(p => p.Code).ToList();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAvailablePrepaidsBySupplierAsync), GetType(), _logger, new
                {
                    SupplierId = supplierId,
                    ExcludeSetoffId = excludeSetoffId
                });
                return new List<SetoffPrepaymentDto>();
            }
        }

        /// <summary>
        /// 取得客戶所有預收款（含已使用和可用的，用於編輯模式）
        /// </summary>
        public async Task<List<SetoffPrepaymentDto>> GetAllPrepaymentsForEditAsync(int customerId, int setoffId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 取得本沖款單已使用的預收款
                var usedPrepayments = await GetByReceivableSetoffIdAsync(setoffId);
                
                // 取得可用的預收款（排除本沖款單）
                var availablePrepayments = await GetAvailablePrepaymentsByCustomerAsync(customerId, setoffId);
                
                // 合併兩個列表（確保不重複）
                var allPrepayments = usedPrepayments
                    .Concat(availablePrepayments.Where(a => !usedPrepayments.Any(u => u.PrepaymentId == a.PrepaymentId)))
                    .OrderBy(p => p.PaymentDate)
                    .ThenBy(p => p.Code)
                    .ToList();

                return allPrepayments;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllPrepaymentsForEditAsync), GetType(), _logger, new
                {
                    CustomerId = customerId,
                    SetoffId = setoffId
                });
                return new List<SetoffPrepaymentDto>();
            }
        }

        /// <summary>
        /// 取得供應商所有預付款（含已使用和可用的，用於編輯模式）
        /// </summary>
        public async Task<List<SetoffPrepaymentDto>> GetAllPrepaidsForEditAsync(int supplierId, int setoffId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 取得本沖款單已使用的預付款
                var usedPrepaids = await GetByPayableSetoffIdAsync(setoffId);
                
                // 取得可用的預付款（排除本沖款單）
                var availablePrepaids = await GetAvailablePrepaidsBySupplierAsync(supplierId, setoffId);
                
                // 合併兩個列表（確保不重複）
                var allPrepaids = usedPrepaids
                    .Concat(availablePrepaids.Where(a => !usedPrepaids.Any(u => u.PrepaymentId == a.PrepaymentId)))
                    .OrderBy(p => p.PaymentDate)
                    .ThenBy(p => p.Code)
                    .ToList();

                return allPrepaids;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllPrepaidsForEditAsync), GetType(), _logger, new
                {
                    SupplierId = supplierId,
                    SetoffId = setoffId
                });
                return new List<SetoffPrepaymentDto>();
            }
        }

        /// <summary>
        /// 計算預收/預付款的已用金額
        /// </summary>
        public async Task<decimal> GetUsedAmountAsync(int prepaymentId, int? excludeSetoffId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var query = context.SetoffPrepaymentDetails.Where(d => d.PrepaymentId == prepaymentId);
                
                // 排除指定的沖款單（編輯模式用）
                if (excludeSetoffId.HasValue)
                {
                    query = query.Where(d => 
                        d.AccountsReceivableSetoffId != excludeSetoffId.Value && 
                        d.AccountsPayableSetoffId != excludeSetoffId.Value);
                }
                
                return await query.SumAsync(d => d.UseAmount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetUsedAmountAsync), GetType(), _logger, new
                {
                    PrepaymentId = prepaymentId,
                    ExcludeSetoffId = excludeSetoffId
                });
                return 0;
            }
        }

        #endregion

        #region 儲存與刪除方法

        /// <summary>
        /// 儲存應收沖款單的預收款明細
        /// </summary>
        public async Task<ServiceResult> SaveReceivableSetoffPrepaymentsAsync(int setoffId, List<SetoffPrepaymentDto> prepayments, List<int> deletedDetailIds)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 刪除標記為刪除的明細
                if (deletedDetailIds.Any())
                {
                    var detailsToDelete = await context.SetoffPrepaymentDetails
                        .Where(d => deletedDetailIds.Contains(d.Id))
                        .ToListAsync();
                    
                    context.SetoffPrepaymentDetails.RemoveRange(detailsToDelete);
                }
                
                // 處理每個預收款明細
                foreach (var dto in prepayments.Where(p => p.ThisTimeUseAmount > 0))
                {
                    if (dto.Id > 0)
                    {
                        // 更新現有明細
                        var existingDetail = await context.SetoffPrepaymentDetails.FindAsync(dto.Id);
                        if (existingDetail != null)
                        {
                            existingDetail.UseAmount = dto.ThisTimeUseAmount;
                            existingDetail.UpdatedAt = DateTime.Now;
                        }
                    }
                    else
                    {
                        // 新增明細
                        var newDetail = new PrepaymentDetail
                        {
                            AccountsReceivableSetoffId = setoffId,
                            PrepaymentId = dto.PrepaymentId,
                            UseAmount = dto.ThisTimeUseAmount,
                            CreatedAt = DateTime.Now
                        };
                        
                        await context.SetoffPrepaymentDetails.AddAsync(newDetail);
                    }
                }
                
                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SaveReceivableSetoffPrepaymentsAsync), GetType(), _logger, new
                {
                    SetoffId = setoffId,
                    PrepaymentsCount = prepayments.Count,
                    DeletedCount = deletedDetailIds.Count
                });
                return ServiceResult.Failure("預收款明細儲存失敗");
            }
        }

        /// <summary>
        /// 儲存應付沖款單的預付款明細
        /// </summary>
        public async Task<ServiceResult> SavePayableSetoffPrepaymentsAsync(int setoffId, List<SetoffPrepaymentDto> prepayments, List<int> deletedDetailIds)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 刪除標記為刪除的明細
                if (deletedDetailIds.Any())
                {
                    var detailsToDelete = await context.SetoffPrepaymentDetails
                        .Where(d => deletedDetailIds.Contains(d.Id))
                        .ToListAsync();
                    
                    context.SetoffPrepaymentDetails.RemoveRange(detailsToDelete);
                }
                
                // 處理每個預付款明細
                foreach (var dto in prepayments.Where(p => p.ThisTimeUseAmount > 0))
                {
                    if (dto.Id > 0)
                    {
                        // 更新現有明細
                        var existingDetail = await context.SetoffPrepaymentDetails.FindAsync(dto.Id);
                        if (existingDetail != null)
                        {
                            existingDetail.UseAmount = dto.ThisTimeUseAmount;
                            existingDetail.UpdatedAt = DateTime.Now;
                        }
                    }
                    else
                    {
                        // 新增明細
                        var newDetail = new PrepaymentDetail
                        {
                            AccountsPayableSetoffId = setoffId,
                            PrepaymentId = dto.PrepaymentId,
                            UseAmount = dto.ThisTimeUseAmount,
                            CreatedAt = DateTime.Now
                        };
                        
                        await context.SetoffPrepaymentDetails.AddAsync(newDetail);
                    }
                }
                
                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SavePayableSetoffPrepaymentsAsync), GetType(), _logger, new
                {
                    SetoffId = setoffId,
                    PrepaymentsCount = prepayments.Count,
                    DeletedCount = deletedDetailIds.Count
                });
                return ServiceResult.Failure("預付款明細儲存失敗");
            }
        }

        /// <summary>
        /// 刪除應收沖款單的所有預收款明細
        /// </summary>
        public async Task<ServiceResult> DeleteByReceivableSetoffIdAsync(int setoffId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var details = await context.SetoffPrepaymentDetails
                    .Where(d => d.AccountsReceivableSetoffId == setoffId)
                    .ToListAsync();
                
                if (details.Any())
                {
                    context.SetoffPrepaymentDetails.RemoveRange(details);
                    await context.SaveChangesAsync();
                }
                
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeleteByReceivableSetoffIdAsync), GetType(), _logger, new
                {
                    SetoffId = setoffId
                });
                return ServiceResult.Failure("刪除預收款明細失敗");
            }
        }

        /// <summary>
        /// 刪除應付沖款單的所有預付款明細
        /// </summary>
        public async Task<ServiceResult> DeleteByPayableSetoffIdAsync(int setoffId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var details = await context.SetoffPrepaymentDetails
                    .Where(d => d.AccountsPayableSetoffId == setoffId)
                    .ToListAsync();
                
                if (details.Any())
                {
                    context.SetoffPrepaymentDetails.RemoveRange(details);
                    await context.SaveChangesAsync();
                }
                
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeleteByPayableSetoffIdAsync), GetType(), _logger, new
                {
                    SetoffId = setoffId
                });
                return ServiceResult.Failure("刪除預付款明細失敗");
            }
        }

        #endregion

        #region 輔助方法

        /// <summary>
        /// 將實體映射為 DTO
        /// </summary>
        private SetoffPrepaymentDto MapToDto(PrepaymentDetail detail)
        {
            var prepayment = detail.Prepayment!;
            var usedAmount = GetUsedAmountAsync(prepayment.Id, 
                detail.AccountsReceivableSetoffId ?? detail.AccountsPayableSetoffId).Result;

            return new SetoffPrepaymentDto
            {
                Id = detail.Id,
                PrepaymentId = detail.PrepaymentId,
                SetoffId = detail.AccountsReceivableSetoffId ?? detail.AccountsPayableSetoffId ?? 0,
                Code = prepayment.Code ?? string.Empty,
                PaymentDate = prepayment.PaymentDate,
                Amount = prepayment.Amount,
                UsedAmount = usedAmount,
                ThisTimeUseAmount = detail.UseAmount,
                OriginalThisTimeUseAmount = detail.UseAmount,
                PrepaymentType = prepayment.PrepaymentType,
                Remarks = prepayment.Remarks,
                PartnerName = prepayment.Customer?.CompanyName ?? prepayment.Supplier?.CompanyName
            };
        }

        #endregion

        #region 覆寫基底類別方法

        /// <summary>
        /// 覆寫驗證邏輯
        /// </summary>
        public override async Task<ServiceResult> ValidateAsync(PrepaymentDetail entity)
        {
            try
            {
                var errors = new List<string>();

                // 必填欄位驗證
                if (entity.PrepaymentId <= 0)
                    errors.Add("預收/預付款ID不能為空");

                if (entity.UseAmount <= 0)
                    errors.Add("使用金額必須大於0");

                // 必須指定應收或應付沖款單（至少一個）
                if (!entity.AccountsReceivableSetoffId.HasValue && !entity.AccountsPayableSetoffId.HasValue)
                    errors.Add("必須指定應收或應付沖款單");

                // 不能同時指定應收和應付沖款單
                if (entity.AccountsReceivableSetoffId.HasValue && entity.AccountsPayableSetoffId.HasValue)
                    errors.Add("不能同時指定應收和應付沖款單");

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

        /// <summary>
        /// 覆寫搜尋功能（此服務不需要搜尋功能）
        /// </summary>
        public override Task<List<PrepaymentDetail>> SearchAsync(string searchTerm)
        {
            // 此服務透過特定方法查詢，不需要一般搜尋功能
            return Task.FromResult(new List<PrepaymentDetail>());
        }

        #endregion
    }
}
