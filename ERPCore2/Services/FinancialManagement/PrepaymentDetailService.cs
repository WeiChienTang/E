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
    public class PrepaymentDetailService : GenericManagementService<PrepaymentDetail>, IPrepaymentDetailService
    {
        /// <summary>
        /// 完整建構子 - 使用 ILogger
        /// </summary>
        public PrepaymentDetailService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<PrepaymentDetail>> logger) : base(contextFactory, logger)
        {
        }

        /// <summary>
        /// 簡易建構子 - 不使用 ILogger
        /// </summary>
        public PrepaymentDetailService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        #region 查詢方法

        /// <summary>
        /// 取得應收沖款單的預收款明細
        /// </summary>
        public async Task<List<PrepaymentDto>> GetByReceivableSetoffIdAsync(int setoffId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var details = await context.PrepaymentDetails
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
                return new List<PrepaymentDto>();
            }
        }

        /// <summary>
        /// 取得應付沖款單的預付款明細
        /// </summary>
        public async Task<List<PrepaymentDto>> GetByPayableSetoffIdAsync(int setoffId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var details = await context.PrepaymentDetails
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
                return new List<PrepaymentDto>();
            }
        }

        /// <summary>
        /// 取得客戶可用的預收款列表
        /// </summary>
        public async Task<List<PrepaymentDto>> GetAvailablePrepaymentsByCustomerAsync(int customerId, int? excludeSetoffId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 取得客戶的所有預收款
                var prepayments = await context.Prepayments
                    .Include(p => p.Customer)
                    .Where(p => p.CustomerId == customerId && p.PrepaymentType == PrepaymentType.Prepayment)
                    .ToListAsync();

                var result = new List<PrepaymentDto>();

                foreach (var prepayment in prepayments)
                {
                    // 計算已用金額（排除指定的沖款單）
                    var usedAmount = await GetUsedAmountAsync(prepayment.Id, excludeSetoffId);
                    var availableAmount = prepayment.Amount - usedAmount;

                    // 只加入有可用餘額的預收款
                    if (availableAmount > 0)
                    {
                        result.Add(new PrepaymentDto
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
                return new List<PrepaymentDto>();
            }
        }

        /// <summary>
        /// 取得供應商可用的預付款列表
        /// </summary>
        public async Task<List<PrepaymentDto>> GetAvailablePrepaidsBySupplierAsync(int supplierId, int? excludeSetoffId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 取得供應商的所有預付款
                var prepayments = await context.Prepayments
                    .Include(p => p.Supplier)
                    .Where(p => p.SupplierId == supplierId && p.PrepaymentType == PrepaymentType.Prepaid)
                    .ToListAsync();

                var result = new List<PrepaymentDto>();

                foreach (var prepayment in prepayments)
                {
                    // 計算已用金額（排除指定的沖款單）
                    var usedAmount = await GetUsedAmountAsync(prepayment.Id, excludeSetoffId);
                    var availableAmount = prepayment.Amount - usedAmount;

                    // 只加入有可用餘額的預付款
                    if (availableAmount > 0)
                    {
                        result.Add(new PrepaymentDto
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
                return new List<PrepaymentDto>();
            }
        }

        /// <summary>
        /// 取得客戶所有預收款（含已使用和可用的，用於編輯模式）
        /// </summary>
        public async Task<List<PrepaymentDto>> GetAllPrepaymentsForEditAsync(int customerId, int setoffId)
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
                return new List<PrepaymentDto>();
            }
        }

        /// <summary>
        /// 取得供應商所有預付款（含已使用和可用的，用於編輯模式）
        /// </summary>
        public async Task<List<PrepaymentDto>> GetAllPrepaidsForEditAsync(int supplierId, int setoffId)
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
                return new List<PrepaymentDto>();
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
                
                var query = context.PrepaymentDetails.Where(d => d.PrepaymentId == prepaymentId);
                
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
        
        /// <summary>
        /// 取得客戶有剩餘預收款的應收沖款單列表
        /// </summary>
        public async Task<List<PrepaymentDto>> GetReceivableSetoffsWithAvailablePrepaymentAsync(int customerId, int? excludeSetoffId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 查詢客戶的所有應收沖款單
                var setoffs = await context.AccountsReceivableSetoffs
                    .Include(s => s.PrepaymentDetails)
                        .ThenInclude(pd => pd.Prepayment)
                    .Where(s => s.CustomerId == customerId && s.Status == EntityStatus.Active)
                    .ToListAsync();
                
                // 排除當前編輯的沖款單
                if (excludeSetoffId.HasValue)
                {
                    setoffs = setoffs.Where(s => s.Id != excludeSetoffId.Value).ToList();
                }
                
                var result = new List<PrepaymentDto>();
                
                foreach (var setoff in setoffs)
                {
                    // 計算此沖款單的總預收款金額和已使用金額
                    var prepaymentDetails = setoff.PrepaymentDetails.Where(pd => pd.Prepayment != null).ToList();
                    
                    if (!prepaymentDetails.Any()) continue;
                    
                    // 計算總金額（此沖款單所有預收款的本次使用金額）
                    decimal totalAmount = prepaymentDetails.Sum(pd => pd.UseAmount);
                    
                    // 計算已被其他沖款單使用的金額
                    decimal usedByOthers = 0;
                    foreach (var pd in prepaymentDetails)
                    {
                        if (pd.Prepayment != null)
                        {
                            var otherUsage = await context.PrepaymentDetails
                                .Where(d => d.PrepaymentId == pd.PrepaymentId && 
                                           d.AccountsReceivableSetoffId != setoff.Id)
                                .SumAsync(d => d.UseAmount);
                            usedByOthers += otherUsage;
                        }
                    }
                    
                    decimal availableAmount = totalAmount - usedByOthers;
                    
                    // 只加入有可用金額的沖款單
                    if (availableAmount > 0)
                    {
                        result.Add(new PrepaymentDto
                        {
                            SourceSetoffId = setoff.Id,
                            SourceSetoffNumber = setoff.SetoffNumber,
                            Code = setoff.SetoffNumber,
                            PaymentDate = setoff.SetoffDate,
                            Amount = totalAmount,
                            UsedAmount = usedByOthers,
                            PrepaymentType = PrepaymentType.PrepaymentToSetoff,
                            Remarks = $"來源：{setoff.SetoffNumber}"
                        });
                    }
                }
                
                return result.OrderBy(p => p.PaymentDate).ThenBy(p => p.Code).ToList();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetReceivableSetoffsWithAvailablePrepaymentAsync), GetType(), _logger, new
                {
                    CustomerId = customerId,
                    ExcludeSetoffId = excludeSetoffId
                });
                return new List<PrepaymentDto>();
            }
        }
        
        /// <summary>
        /// 取得供應商有剩餘預付款的應付沖款單列表
        /// </summary>
        public async Task<List<PrepaymentDto>> GetPayableSetoffsWithAvailablePrepaidAsync(int supplierId, int? excludeSetoffId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 查詢供應商的所有應付沖款單
                var setoffs = await context.AccountsPayableSetoffs
                    .Include(s => s.PaymentDetails) // 注意：這裡可能需要確認是否有 PrepaymentDetails 導航屬性
                    .Where(s => s.SupplierId == supplierId && s.Status == EntityStatus.Active)
                    .ToListAsync();
                
                // 排除當前編輯的沖款單
                if (excludeSetoffId.HasValue)
                {
                    setoffs = setoffs.Where(s => s.Id != excludeSetoffId.Value).ToList();
                }
                
                var result = new List<PrepaymentDto>();
                
                foreach (var setoff in setoffs)
                {
                    // 查詢此沖款單的預付款明細
                    var prepaymentDetails = await context.PrepaymentDetails
                        .Include(pd => pd.Prepayment)
                        .Where(pd => pd.AccountsPayableSetoffId == setoff.Id && pd.Prepayment != null)
                        .ToListAsync();
                    
                    if (!prepaymentDetails.Any()) continue;
                    
                    // 計算總金額（此沖款單所有預付款的本次使用金額）
                    decimal totalAmount = prepaymentDetails.Sum(pd => pd.UseAmount);
                    
                    // 計算已被其他沖款單使用的金額
                    decimal usedByOthers = 0;
                    foreach (var pd in prepaymentDetails)
                    {
                        if (pd.Prepayment != null)
                        {
                            var otherUsage = await context.PrepaymentDetails
                                .Where(d => d.PrepaymentId == pd.PrepaymentId && 
                                           d.AccountsPayableSetoffId != setoff.Id)
                                .SumAsync(d => d.UseAmount);
                            usedByOthers += otherUsage;
                        }
                    }
                    
                    decimal availableAmount = totalAmount - usedByOthers;
                    
                    // 只加入有可用金額的沖款單
                    if (availableAmount > 0)
                    {
                        result.Add(new PrepaymentDto
                        {
                            SourceSetoffId = setoff.Id,
                            SourceSetoffNumber = setoff.SetoffNumber,
                            Code = setoff.SetoffNumber,
                            PaymentDate = setoff.SetoffDate,
                            Amount = totalAmount,
                            UsedAmount = usedByOthers,
                            PrepaymentType = PrepaymentType.PrepaidToSetoff,
                            Remarks = $"來源：{setoff.SetoffNumber}"
                        });
                    }
                }
                
                return result.OrderBy(p => p.PaymentDate).ThenBy(p => p.Code).ToList();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPayableSetoffsWithAvailablePrepaidAsync), GetType(), _logger, new
                {
                    SupplierId = supplierId,
                    ExcludeSetoffId = excludeSetoffId
                });
                return new List<PrepaymentDto>();
            }
        }

        #endregion

        #region 儲存與刪除方法

        /// <summary>
        /// 儲存應收沖款單的預收款明細
        /// </summary>
        public async Task<ServiceResult> SaveReceivableSetoffPrepaymentsAsync(int setoffId, List<PrepaymentDto> prepayments, List<int> deletedDetailIds)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 刪除標記為刪除的明細
                if (deletedDetailIds.Any())
                {
                    var detailsToDelete = await context.PrepaymentDetails
                        .Where(d => deletedDetailIds.Contains(d.Id))
                        .ToListAsync();
                    
                    context.PrepaymentDetails.RemoveRange(detailsToDelete);
                }
                
                // 處理每個預收款明細
                foreach (var dto in prepayments.Where(p => p.ThisTimeUseAmount > 0))
                {
                    int prepaymentId = dto.PrepaymentId;
                    
                    // 如果有新增金額且 PrepaymentId 為 0，表示需要建立新的預收款
                    if (dto.ThisTimeAddAmount > 0 && dto.PrepaymentId == 0)
                    {
                        // 取得沖款單資訊以獲取客戶ID
                        var setoff = await context.AccountsReceivableSetoffs
                            .AsNoTracking()
                            .FirstOrDefaultAsync(s => s.Id == setoffId);
                        
                        if (setoff == null)
                        {
                            return ServiceResult.Failure("找不到對應的沖款單");
                        }
                        
                        // 建立新的預收款記錄
                        var newPrepayment = new Prepayment
                        {
                            Code = dto.Code,
                            PrepaymentType = PrepaymentType.Prepayment,
                            PaymentDate = dto.PaymentDate,
                            Amount = dto.ThisTimeAddAmount,
                            CustomerId = setoff.CustomerId,
                            Remarks = dto.Remarks,
                            Status = EntityStatus.Active,
                            CreatedAt = DateTime.Now,
                            CreatedBy = "System" // 從沖款單建立
                        };
                        
                        await context.Prepayments.AddAsync(newPrepayment);
                        await context.SaveChangesAsync(); // 儲存以取得 ID
                        
                        prepaymentId = newPrepayment.Id;
                    }
                    
                    if (dto.Id > 0)
                    {
                        // 更新現有明細
                        var existingDetail = await context.PrepaymentDetails.FindAsync(dto.Id);
                        if (existingDetail != null)
                        {
                            existingDetail.UseAmount = dto.ThisTimeUseAmount;
                            existingDetail.UpdatedAt = DateTime.Now;
                            existingDetail.UpdatedBy = "System";
                        }
                    }
                    else
                    {
                        // 新增明細
                        var newDetail = new PrepaymentDetail
                        {
                            AccountsReceivableSetoffId = setoffId,
                            PrepaymentId = prepaymentId,
                            UseAmount = dto.ThisTimeUseAmount,
                            Status = EntityStatus.Active,
                            CreatedAt = DateTime.Now,
                            CreatedBy = "System"
                        };
                        
                        await context.PrepaymentDetails.AddAsync(newDetail);
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
        public async Task<ServiceResult> SavePayableSetoffPrepaymentsAsync(int setoffId, List<PrepaymentDto> prepayments, List<int> deletedDetailIds)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 刪除標記為刪除的明細
                if (deletedDetailIds.Any())
                {
                    var detailsToDelete = await context.PrepaymentDetails
                        .Where(d => deletedDetailIds.Contains(d.Id))
                        .ToListAsync();
                    
                    context.PrepaymentDetails.RemoveRange(detailsToDelete);
                }
                
                // 處理每個預付款明細
                foreach (var dto in prepayments.Where(p => p.ThisTimeUseAmount > 0))
                {
                    int prepaymentId = dto.PrepaymentId;
                    
                    // 如果有新增金額且 PrepaymentId 為 0，表示需要建立新的預付款
                    if (dto.ThisTimeAddAmount > 0 && dto.PrepaymentId == 0)
                    {
                        // 取得沖款單資訊以獲取供應商ID
                        var setoff = await context.AccountsPayableSetoffs
                            .AsNoTracking()
                            .FirstOrDefaultAsync(s => s.Id == setoffId);
                        
                        if (setoff == null)
                        {
                            return ServiceResult.Failure("找不到對應的沖款單");
                        }
                        
                        // 建立新的預付款記錄
                        var newPrepayment = new Prepayment
                        {
                            Code = dto.Code,
                            PrepaymentType = PrepaymentType.Prepaid,
                            PaymentDate = dto.PaymentDate,
                            Amount = dto.ThisTimeAddAmount,
                            SupplierId = setoff.SupplierId,
                            Remarks = dto.Remarks,
                            Status = EntityStatus.Active,
                            CreatedAt = DateTime.Now,
                            CreatedBy = "System" // 從沖款單建立
                        };
                        
                        await context.Prepayments.AddAsync(newPrepayment);
                        await context.SaveChangesAsync(); // 儲存以取得 ID
                        
                        prepaymentId = newPrepayment.Id;
                    }
                    
                    if (dto.Id > 0)
                    {
                        // 更新現有明細
                        var existingDetail = await context.PrepaymentDetails.FindAsync(dto.Id);
                        if (existingDetail != null)
                        {
                            existingDetail.UseAmount = dto.ThisTimeUseAmount;
                            existingDetail.UpdatedAt = DateTime.Now;
                            existingDetail.UpdatedBy = "System";
                        }
                    }
                    else
                    {
                        // 新增明細
                        var newDetail = new PrepaymentDetail
                        {
                            AccountsPayableSetoffId = setoffId,
                            PrepaymentId = prepaymentId,
                            UseAmount = dto.ThisTimeUseAmount,
                            Status = EntityStatus.Active,
                            CreatedAt = DateTime.Now,
                            CreatedBy = "System"
                        };
                        
                        await context.PrepaymentDetails.AddAsync(newDetail);
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
                var details = await context.PrepaymentDetails
                    .Where(d => d.AccountsReceivableSetoffId == setoffId)
                    .ToListAsync();
                
                if (details.Any())
                {
                    context.PrepaymentDetails.RemoveRange(details);
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
                var details = await context.PrepaymentDetails
                    .Where(d => d.AccountsPayableSetoffId == setoffId)
                    .ToListAsync();
                
                if (details.Any())
                {
                    context.PrepaymentDetails.RemoveRange(details);
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
        private PrepaymentDto MapToDto(PrepaymentDetail detail)
        {
            var prepayment = detail.Prepayment!;
            var usedAmount = GetUsedAmountAsync(prepayment.Id, 
                detail.AccountsReceivableSetoffId ?? detail.AccountsPayableSetoffId).Result;

            return new PrepaymentDto
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

