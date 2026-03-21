using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 批次轉傳票服務實作
    /// 分錄規則（含稅；稅額為零時省略稅額行；COGS 必須存在，為零時拒絕轉傳票）：
    ///   進貨入庫：借 品項存貨(1231) + 進項稅額(1268) / 貸 應付帳款(2171)
    ///   進貨退回：借 應付帳款(2171) / 貸 品項存貨(1231) + 進項稅額(1268)
    ///   銷貨出貨：借 應收帳款(1191) / 貸 銷貨收入(4111) + 銷項稅額(2204)
    ///             借 銷貨成本(5111) / 貸 品項存貨(1231)             ← COGS 必須（移動加權平均，需先完成 ReduceStock）
    ///   銷貨退回：借 銷貨收入(4111) + 銷項稅額(2204) / 貸 應收帳款(1191)
    ///             借 品項存貨(1231) / 貸 銷貨成本(5111)             ← COGS 沖回（需先完成 AddStock）
    /// </summary>
    public class JournalEntryAutoGenerationService : IJournalEntryAutoGenerationService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IAccountItemService _accountItemService;
        private readonly IJournalEntryService _journalEntryService;
        private readonly ICompanyService _companyService;
        private readonly ISubAccountService _subAccountService;
        private readonly ILogger<JournalEntryAutoGenerationService> _logger;

        // 標準科目代碼常數（來自商業會計項目表 112 年度種子資料）
        private const string AccountReceivableCode = "1191"; // 應收帳款
        private const string AccountPayableCode    = "2171"; // 應付帳款
        private const string InventoryCode         = "1231"; // 品項存貨
        private const string SalesRevenueCode      = "4111"; // 銷貨收入
        private const string InputVatCode          = "1268"; // 進項稅額
        private const string OutputVatCode         = "2204"; // 銷項稅額
        private const string CostOfGoodsSoldCode      = "5111"; // 銷貨成本
        private const string BankDepositCode          = "1113"; // 銀行存款
        private const string SalesAllowanceCode       = "4114"; // 銷貨折讓
        private const string PurchaseAllowanceCode    = "5124"; // 進貨折讓
        private const string AdvanceFromCustomerCode  = "2221"; // 預收貨款
        private const string AdvanceToSupplierCode    = "1266"; // 預付貨款

        public JournalEntryAutoGenerationService(
            IDbContextFactory<AppDbContext> contextFactory,
            IAccountItemService accountItemService,
            IJournalEntryService journalEntryService,
            ICompanyService companyService,
            ISubAccountService subAccountService,
            ILogger<JournalEntryAutoGenerationService> logger)
        {
            _contextFactory = contextFactory;
            _accountItemService = accountItemService;
            _journalEntryService = journalEntryService;
            _companyService = companyService;
            _subAccountService = subAccountService;
            _logger = logger;
        }

        // ===== 查詢待轉傳票的單據 =====

        public async Task<List<PurchaseReceiving>> GetPendingPurchaseReceivingsAsync(DateTime? from = null, DateTime? to = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.PurchaseReceivings
                    .Include(pr => pr.Supplier)
                    .Where(pr => !pr.IsJournalized && pr.IsApproved);

                if (from.HasValue)
                    query = query.Where(pr => pr.ReceiptDate >= from.Value.Date);
                if (to.HasValue)
                    query = query.Where(pr => pr.ReceiptDate <= to.Value.Date.AddDays(1).AddSeconds(-1));

                return await query.OrderByDescending(pr => pr.ReceiptDate).ThenByDescending(pr => pr.Code).ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPendingPurchaseReceivingsAsync), GetType(), _logger);
                return new List<PurchaseReceiving>();
            }
        }

        public async Task<List<PurchaseReturn>> GetPendingPurchaseReturnsAsync(DateTime? from = null, DateTime? to = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.PurchaseReturns
                    .Include(pr => pr.Supplier)
                    .Where(pr => !pr.IsJournalized && pr.IsApproved);

                if (from.HasValue)
                    query = query.Where(pr => pr.ReturnDate >= from.Value.Date);
                if (to.HasValue)
                    query = query.Where(pr => pr.ReturnDate <= to.Value.Date.AddDays(1).AddSeconds(-1));

                return await query.OrderByDescending(pr => pr.ReturnDate).ThenByDescending(pr => pr.Code).ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPendingPurchaseReturnsAsync), GetType(), _logger);
                return new List<PurchaseReturn>();
            }
        }

        public async Task<List<SalesDelivery>> GetPendingSalesDeliveriesAsync(DateTime? from = null, DateTime? to = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.SalesDeliveries
                    .Include(sd => sd.Customer)
                    .Where(sd => !sd.IsJournalized && sd.IsApproved);

                if (from.HasValue)
                    query = query.Where(sd => sd.DeliveryDate >= from.Value.Date);
                if (to.HasValue)
                    query = query.Where(sd => sd.DeliveryDate <= to.Value.Date.AddDays(1).AddSeconds(-1));

                return await query.OrderByDescending(sd => sd.DeliveryDate).ThenByDescending(sd => sd.Code).ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPendingSalesDeliveriesAsync), GetType(), _logger);
                return new List<SalesDelivery>();
            }
        }

        public async Task<List<SalesReturn>> GetPendingSalesReturnsAsync(DateTime? from = null, DateTime? to = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.SalesReturns
                    .Include(sr => sr.Customer)
                    .Where(sr => !sr.IsJournalized && sr.IsApproved);

                if (from.HasValue)
                    query = query.Where(sr => sr.ReturnDate >= from.Value.Date);
                if (to.HasValue)
                    query = query.Where(sr => sr.ReturnDate <= to.Value.Date.AddDays(1).AddSeconds(-1));

                return await query.OrderByDescending(sr => sr.ReturnDate).ThenByDescending(sr => sr.Code).ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPendingSalesReturnsAsync), GetType(), _logger);
                return new List<SalesReturn>();
            }
        }

        // ===== 轉傳票 =====

        public async Task<(bool Success, string ErrorMessage)> JournalizePurchaseReceivingAsync(int id, string createdBy)
        {
            try
            {
                // 防重複
                var existing = await _journalEntryService.GetBySourceDocumentAsync("PurchaseReceiving", id);
                if (existing != null)
                    return (false, $"進貨入庫單已有對應傳票（{existing.Code}），請先沖銷後再重新轉傳票");

                using var context = await _contextFactory.CreateDbContextAsync();
                var doc = await context.PurchaseReceivings
                    .Include(pr => pr.Supplier)
                    .FirstOrDefaultAsync(pr => pr.Id == id);

                if (doc == null)
                    return (false, "找不到指定的進貨入庫單");

                if (!doc.SupplierId.HasValue)
                    return (false, "進貨入庫單缺少廠商資料，無法自動生成傳票");

                var inventory = await _accountItemService.GetByCodeAsync(InventoryCode);
                var payable   = await GetAPAccountForSupplierAsync(doc.SupplierId.Value);
                var inputVat  = await _accountItemService.GetByCodeAsync(InputVatCode);

                if (inventory == null || payable == null)
                    return (false, "找不到必要的會計科目（品項存貨或應付帳款），請確認種子資料已正確載入");

                var lines = new List<JournalEntryLine>
                {
                    new JournalEntryLine
                    {
                        LineNumber = 1,
                        AccountItemId = inventory.Id,
                        Direction = AccountDirection.Debit,
                        Amount = doc.TotalAmount,
                        LineDescription = "品項存貨"
                    }
                };

                // 稅額行（僅在稅額 > 0 時建立）
                if (doc.PurchaseReceivingTaxAmount > 0 && inputVat != null)
                {
                    lines.Add(new JournalEntryLine
                    {
                        LineNumber = 2,
                        AccountItemId = inputVat.Id,
                        Direction = AccountDirection.Debit,
                        Amount = doc.PurchaseReceivingTaxAmount,
                        LineDescription = "進項稅額"
                    });
                }

                lines.Add(new JournalEntryLine
                {
                    LineNumber = lines.Count + 1,
                    AccountItemId = payable.Id,
                    Direction = AccountDirection.Credit,
                    Amount = doc.PurchaseReceivingTotalAmountIncludingTax,
                    LineDescription = $"應付帳款－{doc.Supplier?.CompanyName ?? doc.SupplierId.ToString()}"
                });

                var result = await CreateAndPostEntryAsync(
                    doc.ReceiptDate,
                    $"進貨入庫：{doc.Code}",
                    "PurchaseReceiving",
                    doc.Id,
                    doc.Code ?? string.Empty,
                    lines,
                    createdBy);

                if (result.Success)
                {
                    doc.IsJournalized = true;
                    doc.JournalizedAt = DateTime.UtcNow;
                    await context.SaveChangesAsync();
                }

                return result;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(JournalizePurchaseReceivingAsync), GetType(), _logger, new { Id = id });
                return (false, "轉傳票過程發生錯誤，請稍後再試");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> JournalizePurchaseReturnAsync(int id, string createdBy)
        {
            try
            {
                var existing = await _journalEntryService.GetBySourceDocumentAsync("PurchaseReturn", id);
                if (existing != null)
                    return (false, $"進貨退回單已有對應傳票（{existing.Code}），請先沖銷後再重新轉傳票");

                using var context = await _contextFactory.CreateDbContextAsync();
                var doc = await context.PurchaseReturns
                    .Include(pr => pr.Supplier)
                    .FirstOrDefaultAsync(pr => pr.Id == id);

                if (doc == null)
                    return (false, "找不到指定的進貨退回單");

                if (!doc.SupplierId.HasValue)
                    return (false, "進貨退回單缺少廠商資料，無法自動生成傳票");

                var inventory = await _accountItemService.GetByCodeAsync(InventoryCode);
                var payable   = await GetAPAccountForSupplierAsync(doc.SupplierId.Value);
                var inputVat  = await _accountItemService.GetByCodeAsync(InputVatCode);

                if (inventory == null || payable == null)
                    return (false, "找不到必要的會計科目（品項存貨或應付帳款），請確認種子資料已正確載入");

                // 借：應付帳款 / 貸：品項存貨 + 進項稅額（沖回）
                var lines = new List<JournalEntryLine>
                {
                    new JournalEntryLine
                    {
                        LineNumber = 1,
                        AccountItemId = payable.Id,
                        Direction = AccountDirection.Debit,
                        Amount = doc.TotalReturnAmountWithTax,
                        LineDescription = $"應付帳款沖回－{doc.Supplier?.CompanyName ?? doc.SupplierId.ToString()}"
                    },
                    new JournalEntryLine
                    {
                        LineNumber = 2,
                        AccountItemId = inventory.Id,
                        Direction = AccountDirection.Credit,
                        Amount = doc.TotalReturnAmount,
                        LineDescription = "品項存貨退回"
                    }
                };

                if (doc.ReturnTaxAmount > 0 && inputVat != null)
                {
                    lines.Add(new JournalEntryLine
                    {
                        LineNumber = 3,
                        AccountItemId = inputVat.Id,
                        Direction = AccountDirection.Credit,
                        Amount = doc.ReturnTaxAmount,
                        LineDescription = "進項稅額沖回"
                    });
                }

                var result = await CreateAndPostEntryAsync(
                    doc.ReturnDate,
                    $"進貨退回：{doc.Code}",
                    "PurchaseReturn",
                    doc.Id,
                    doc.Code ?? string.Empty,
                    lines,
                    createdBy);

                if (result.Success)
                {
                    doc.IsJournalized = true;
                    doc.JournalizedAt = DateTime.UtcNow;
                    await context.SaveChangesAsync();
                }

                return result;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(JournalizePurchaseReturnAsync), GetType(), _logger, new { Id = id });
                return (false, "轉傳票過程發生錯誤，請稍後再試");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> JournalizeSalesDeliveryAsync(int id, string createdBy)
        {
            try
            {
                var existing = await _journalEntryService.GetBySourceDocumentAsync("SalesDelivery", id);
                if (existing != null)
                    return (false, $"銷貨出貨單已有對應傳票（{existing.Code}），請先沖銷後再重新轉傳票");

                using var context = await _contextFactory.CreateDbContextAsync();
                var doc = await context.SalesDeliveries
                    .Include(sd => sd.Customer)
                    .FirstOrDefaultAsync(sd => sd.Id == id);

                if (doc == null)
                    return (false, "找不到指定的銷貨出貨單");

                if (!doc.CustomerId.HasValue)
                    return (false, "銷貨出貨單缺少客戶資料，無法自動生成傳票");

                var receivable  = await GetARAccountForCustomerAsync(doc.CustomerId.Value);
                var revenue     = await _accountItemService.GetByCodeAsync(SalesRevenueCode);
                var outputVat   = await _accountItemService.GetByCodeAsync(OutputVatCode);

                if (receivable == null || revenue == null)
                    return (false, "找不到必要的會計科目（應收帳款或銷貨收入），請確認種子資料已正確載入");

                // 借：應收帳款（含稅）/ 貸：銷貨收入（未稅）+ 銷項稅額
                var lines = new List<JournalEntryLine>
                {
                    new JournalEntryLine
                    {
                        LineNumber = 1,
                        AccountItemId = receivable.Id,
                        Direction = AccountDirection.Debit,
                        Amount = doc.TotalAmountWithTax,
                        LineDescription = $"應收帳款－{doc.Customer?.CompanyName ?? doc.CustomerId?.ToString() ?? ""}"
                    },
                    new JournalEntryLine
                    {
                        LineNumber = 2,
                        AccountItemId = revenue.Id,
                        Direction = AccountDirection.Credit,
                        Amount = doc.TotalAmount,
                        LineDescription = "銷貨收入"
                    }
                };

                if (doc.TaxAmount > 0 && outputVat != null)
                {
                    lines.Add(new JournalEntryLine
                    {
                        LineNumber = 3,
                        AccountItemId = outputVat.Id,
                        Direction = AccountDirection.Credit,
                        Amount = doc.TaxAmount,
                        LineDescription = "銷項稅額"
                    });
                }

                // 銷貨成本分錄（COGS）：借 銷貨成本(5111) / 貸 品項存貨(1231)
                // 金額來源：InventoryTransaction.TotalAmount（ReduceStockAsync 寫入的出庫成本 = 出庫量 × 移動加權均價）
                // ⚠ COGS 為必要分錄（系統設定需追蹤成本）：若找不到庫存異動，拒絕轉傳票並要求先完成出庫作業
                var cogsAmount = await context.InventoryTransactions
                    .Where(t => t.SourceDocumentType == "SalesDelivery" && t.SourceDocumentId == id)
                    .SumAsync(t => t.TotalAmount);

                if (cogsAmount == 0)
                    return (false, $"銷貨出貨單 {doc.Code} 找不到對應的庫存異動記錄，無法計算銷貨成本（COGS）。請確認已執行出庫作業（ReduceStock），再重新轉傳票。");

                var cogs      = await _accountItemService.GetByCodeAsync(CostOfGoodsSoldCode);
                var inventory = await _accountItemService.GetByCodeAsync(InventoryCode);

                if (cogs == null || inventory == null)
                    return (false, $"找不到銷貨成本科目（{CostOfGoodsSoldCode}）或品項存貨科目（{InventoryCode}），請確認種子資料已正確載入");

                lines.Add(new JournalEntryLine
                {
                    LineNumber = lines.Count + 1,
                    AccountItemId = cogs.Id,
                    Direction = AccountDirection.Debit,
                    Amount = cogsAmount,
                    LineDescription = "銷貨成本"
                });
                lines.Add(new JournalEntryLine
                {
                    LineNumber = lines.Count + 1,
                    AccountItemId = inventory.Id,
                    Direction = AccountDirection.Credit,
                    Amount = cogsAmount,
                    LineDescription = "品項存貨－銷貨出倉"
                });

                var result = await CreateAndPostEntryAsync(
                    doc.DeliveryDate,
                    $"銷貨出貨：{doc.Code}",
                    "SalesDelivery",
                    doc.Id,
                    doc.Code ?? string.Empty,
                    lines,
                    createdBy);

                if (result.Success)
                {
                    doc.IsJournalized = true;
                    doc.JournalizedAt = DateTime.UtcNow;
                    await context.SaveChangesAsync();
                }

                return result;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(JournalizeSalesDeliveryAsync), GetType(), _logger, new { Id = id });
                return (false, "轉傳票過程發生錯誤，請稍後再試");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> JournalizeSalesReturnAsync(int id, string createdBy)
        {
            try
            {
                var existing = await _journalEntryService.GetBySourceDocumentAsync("SalesReturn", id);
                if (existing != null)
                    return (false, $"銷貨退回單已有對應傳票（{existing.Code}），請先沖銷後再重新轉傳票");

                using var context = await _contextFactory.CreateDbContextAsync();
                var doc = await context.SalesReturns
                    .Include(sr => sr.Customer)
                    .FirstOrDefaultAsync(sr => sr.Id == id);

                if (doc == null)
                    return (false, "找不到指定的銷貨退回單");

                if (!doc.CustomerId.HasValue)
                    return (false, "銷貨退回單缺少客戶資料，無法自動生成傳票");

                var receivable  = await GetARAccountForCustomerAsync(doc.CustomerId.Value);
                var revenue     = await _accountItemService.GetByCodeAsync(SalesRevenueCode);
                var outputVat   = await _accountItemService.GetByCodeAsync(OutputVatCode);

                if (receivable == null || revenue == null)
                    return (false, "找不到必要的會計科目（應收帳款或銷貨收入），請確認種子資料已正確載入");

                // 借：銷貨收入 + 銷項稅額（沖回）/ 貸：應收帳款（含稅）
                var lines = new List<JournalEntryLine>
                {
                    new JournalEntryLine
                    {
                        LineNumber = 1,
                        AccountItemId = revenue.Id,
                        Direction = AccountDirection.Debit,
                        Amount = doc.TotalReturnAmount,
                        LineDescription = "銷貨收入沖回"
                    }
                };

                if (doc.ReturnTaxAmount > 0 && outputVat != null)
                {
                    lines.Add(new JournalEntryLine
                    {
                        LineNumber = 2,
                        AccountItemId = outputVat.Id,
                        Direction = AccountDirection.Debit,
                        Amount = doc.ReturnTaxAmount,
                        LineDescription = "銷項稅額沖回"
                    });
                }

                lines.Add(new JournalEntryLine
                {
                    LineNumber = lines.Count + 1,
                    AccountItemId = receivable.Id,
                    Direction = AccountDirection.Credit,
                    Amount = doc.TotalReturnAmountWithTax,
                    LineDescription = $"應收帳款沖回－{doc.Customer?.CompanyName ?? doc.CustomerId?.ToString() ?? ""}"
                });

                // 銷貨退回成本沖回：借 品項存貨(1231) / 貸 銷貨成本(5111)
                // 金額來源：InventoryTransaction.TotalAmount（AddStockAsync 寫入的退回入庫成本）
                // ⚠ COGS 沖回為必要分錄：若找不到庫存異動，拒絕轉傳票並要求先完成退回入庫作業
                var returnCogsAmount = await context.InventoryTransactions
                    .Where(t => t.SourceDocumentType == "SalesReturn" && t.SourceDocumentId == id)
                    .SumAsync(t => t.TotalAmount);

                if (returnCogsAmount == 0)
                    return (false, $"銷貨退回單 {doc.Code} 找不到對應的庫存異動記錄，無法計算成本沖回金額。請確認已執行退回入庫作業（AddStock），再重新轉傳票。");

                var returnCogs      = await _accountItemService.GetByCodeAsync(CostOfGoodsSoldCode);
                var returnInventory = await _accountItemService.GetByCodeAsync(InventoryCode);

                if (returnCogs == null || returnInventory == null)
                    return (false, $"找不到銷貨成本科目（{CostOfGoodsSoldCode}）或品項存貨科目（{InventoryCode}），請確認種子資料已正確載入");

                lines.Add(new JournalEntryLine
                {
                    LineNumber = lines.Count + 1,
                    AccountItemId = returnInventory.Id,
                    Direction = AccountDirection.Debit,
                    Amount = returnCogsAmount,
                    LineDescription = "品項存貨－退回入庫"
                });
                lines.Add(new JournalEntryLine
                {
                    LineNumber = lines.Count + 1,
                    AccountItemId = returnCogs.Id,
                    Direction = AccountDirection.Credit,
                    Amount = returnCogsAmount,
                    LineDescription = "銷貨成本沖回"
                });

                var result = await CreateAndPostEntryAsync(
                    doc.ReturnDate,
                    $"銷貨退回：{doc.Code}",
                    "SalesReturn",
                    doc.Id,
                    doc.Code ?? string.Empty,
                    lines,
                    createdBy);

                if (result.Success)
                {
                    doc.IsJournalized = true;
                    doc.JournalizedAt = DateTime.UtcNow;
                    await context.SaveChangesAsync();
                }

                return result;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(JournalizeSalesReturnAsync), GetType(), _logger, new { Id = id });
                return (false, "轉傳票過程發生錯誤，請稍後再試");
            }
        }

        // ===== 沖款單 =====

        public async Task<List<SetoffDocument>> GetPendingSetoffDocumentsAsync(DateTime? from = null, DateTime? to = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.SetoffDocuments
                    .Where(d => !d.IsJournalized);

                if (from.HasValue)
                    query = query.Where(d => d.SetoffDate >= from.Value);
                if (to.HasValue)
                    query = query.Where(d => d.SetoffDate <= to.Value.Date.AddDays(1).AddTicks(-1));

                return await query.OrderByDescending(d => d.SetoffDate).ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPendingSetoffDocumentsAsync), GetType(), _logger);
                return new List<SetoffDocument>();
            }
        }

        public async Task<(bool Success, string ErrorMessage)> JournalizeSetoffDocumentAsync(int id, string createdBy)
        {
            try
            {
                // 防重複
                var existing = await _journalEntryService.GetBySourceDocumentAsync("SetoffDocument", id);
                if (existing != null)
                    return (false, "此沖款單已有對應傳票，請勿重複轉傳票");

                using var context = await _contextFactory.CreateDbContextAsync();
                var doc = await context.SetoffDocuments.FindAsync(id);
                if (doc == null)
                    return (false, $"找不到沖款單 ID: {id}");

                if (doc.CurrentSetoffAmount <= 0)
                    return (false, "沖銷金額為零，無需轉傳票");

                // 共用科目
                var bank = await _accountItemService.GetByCodeAsync(BankDepositCode);
                if (bank == null) return (false, $"找不到科目 {BankDepositCode}（銀行存款）");

                List<JournalEntryLine> lines;
                string description;

                if (doc.SetoffType == SetoffType.AccountsReceivable)
                {
                    // 應收沖款：借 銀行存款+銷貨折讓+預收貨款沖回 / 貸 應收帳款+預收貨款(新建)
                    // 正確公式（來自 UI 驗證）：
                    //   借方：實收現金(TotalCollectionAmount-TotalAllowanceAmount) + 折讓(TotalAllowanceAmount) + 預收沖回(PrepaymentSetoffAmount)
                    //   貸方：應收帳款(CurrentSetoffAmount+TotalAllowanceAmount) + 預收新增(CurrentPrepaymentAmount)
                    // 優先使用客戶子科目（如 1191.C001），找不到才 fallback 到統制科目 1191
                    var receivable = await GetARAccountForCustomerAsync(doc.RelatedPartyId);
                    if (receivable == null) return (false, $"找不到科目 {AccountReceivableCode}（應收帳款）");

                    lines = new List<JournalEntryLine>();
                    int lineNum = 1;

                    // 銀行存款借方 = 實際現金（不含折讓，折讓另設借方科目）
                    var bankCashAmount = doc.TotalCollectionAmount - doc.TotalAllowanceAmount;
                    if (bankCashAmount > 0)
                        lines.Add(new JournalEntryLine
                        {
                            LineNumber = lineNum++,
                            AccountItemId = bank.Id,
                            Direction = AccountDirection.Debit,
                            Amount = bankCashAmount,
                            LineDescription = $"銀行存款收款－{doc.Code}"
                        });

                    if (doc.TotalAllowanceAmount > 0)
                    {
                        var salesAllowance = await _accountItemService.GetByCodeAsync(SalesAllowanceCode);
                        if (salesAllowance == null) return (false, $"找不到科目 {SalesAllowanceCode}（銷貨折讓）");
                        lines.Add(new JournalEntryLine
                        {
                            LineNumber = lineNum++,
                            AccountItemId = salesAllowance.Id,
                            Direction = AccountDirection.Debit,
                            Amount = doc.TotalAllowanceAmount,
                            LineDescription = "銷貨折讓"
                        });
                    }

                    // 預收貨款借方：使用既有預收款抵扣應收帳款
                    AccountItem? advanceFromCustomer = null;
                    if (doc.PrepaymentSetoffAmount > 0 || doc.CurrentPrepaymentAmount > 0)
                    {
                        advanceFromCustomer = await _accountItemService.GetByCodeAsync(AdvanceFromCustomerCode);
                        if (advanceFromCustomer == null) return (false, $"找不到科目 {AdvanceFromCustomerCode}（預收貨款）");
                    }

                    if (doc.PrepaymentSetoffAmount > 0)
                        lines.Add(new JournalEntryLine
                        {
                            LineNumber = lineNum++,
                            AccountItemId = advanceFromCustomer!.Id,
                            Direction = AccountDirection.Debit,
                            Amount = doc.PrepaymentSetoffAmount,
                            LineDescription = "預收貨款沖回"
                        });

                    // 應收帳款貸方 = 本期沖銷 + 折讓（全額應收帳款消除）
                    lines.Add(new JournalEntryLine
                    {
                        LineNumber = lineNum++,
                        AccountItemId = receivable.Id,
                        Direction = AccountDirection.Credit,
                        Amount = doc.CurrentSetoffAmount + doc.TotalAllowanceAmount,
                        LineDescription = $"應收帳款沖銷－{doc.Code}"
                    });

                    // 預收貨款貸方：本期新建立的預收款（客戶多付款）
                    if (doc.CurrentPrepaymentAmount > 0)
                        lines.Add(new JournalEntryLine
                        {
                            LineNumber = lineNum,
                            AccountItemId = advanceFromCustomer!.Id,
                            Direction = AccountDirection.Credit,
                            Amount = doc.CurrentPrepaymentAmount,
                            LineDescription = "預收貨款新增"
                        });

                    description = $"應收沖款：{doc.Code}";
                }
                else
                {
                    // 應付沖款：借 應付帳款+預付貨款(新建) / 貸 銀行存款+進貨折讓+預付貨款沖回
                    // 正確公式（來自 UI 驗證）：
                    //   借方：應付帳款(CurrentSetoffAmount+TotalAllowanceAmount) + 預付新增(CurrentPrepaymentAmount)
                    //   貸方：實付現金(TotalCollectionAmount-TotalAllowanceAmount) + 折讓(TotalAllowanceAmount) + 預付沖回(PrepaymentSetoffAmount)
                    // 優先使用廠商子科目（如 2171.S001），找不到才 fallback 到統制科目 2171
                    var payable = await GetAPAccountForSupplierAsync(doc.RelatedPartyId);
                    if (payable == null) return (false, $"找不到科目 {AccountPayableCode}（應付帳款）");

                    lines = new List<JournalEntryLine>();
                    int lineNum = 1;

                    // 應付帳款借方 = 本期沖銷 + 折讓（全額應付帳款消除）
                    lines.Add(new JournalEntryLine
                    {
                        LineNumber = lineNum++,
                        AccountItemId = payable.Id,
                        Direction = AccountDirection.Debit,
                        Amount = doc.CurrentSetoffAmount + doc.TotalAllowanceAmount,
                        LineDescription = $"應付帳款沖銷－{doc.Code}"
                    });

                    // 預付貨款借方：本期新建立的預付款（廠商多付款）
                    AccountItem? advanceToSupplier = null;
                    if (doc.PrepaymentSetoffAmount > 0 || doc.CurrentPrepaymentAmount > 0)
                    {
                        advanceToSupplier = await _accountItemService.GetByCodeAsync(AdvanceToSupplierCode);
                        if (advanceToSupplier == null) return (false, $"找不到科目 {AdvanceToSupplierCode}（預付貨款）");
                    }

                    if (doc.CurrentPrepaymentAmount > 0)
                        lines.Add(new JournalEntryLine
                        {
                            LineNumber = lineNum++,
                            AccountItemId = advanceToSupplier!.Id,
                            Direction = AccountDirection.Debit,
                            Amount = doc.CurrentPrepaymentAmount,
                            LineDescription = "預付貨款新增"
                        });

                    // 銀行存款貸方 = 實際現金（不含折讓，折讓另設貸方科目）
                    var bankCashAmount = doc.TotalCollectionAmount - doc.TotalAllowanceAmount;
                    if (bankCashAmount > 0)
                        lines.Add(new JournalEntryLine
                        {
                            LineNumber = lineNum++,
                            AccountItemId = bank.Id,
                            Direction = AccountDirection.Credit,
                            Amount = bankCashAmount,
                            LineDescription = $"銀行存款付款－{doc.Code}"
                        });

                    if (doc.TotalAllowanceAmount > 0)
                    {
                        var purchaseAllowance = await _accountItemService.GetByCodeAsync(PurchaseAllowanceCode);
                        if (purchaseAllowance == null) return (false, $"找不到科目 {PurchaseAllowanceCode}（進貨折讓）");
                        lines.Add(new JournalEntryLine
                        {
                            LineNumber = lineNum++,
                            AccountItemId = purchaseAllowance.Id,
                            Direction = AccountDirection.Credit,
                            Amount = doc.TotalAllowanceAmount,
                            LineDescription = "進貨折讓"
                        });
                    }

                    // 預付貨款貸方：使用既有預付款抵扣應付帳款
                    if (doc.PrepaymentSetoffAmount > 0)
                        lines.Add(new JournalEntryLine
                        {
                            LineNumber = lineNum,
                            AccountItemId = advanceToSupplier!.Id,
                            Direction = AccountDirection.Credit,
                            Amount = doc.PrepaymentSetoffAmount,
                            LineDescription = "預付貨款沖回"
                        });

                    description = $"應付沖款：{doc.Code}";
                }

                var result = await CreateAndPostEntryAsync(
                    doc.SetoffDate,
                    description,
                    "SetoffDocument",
                    doc.Id,
                    doc.Code ?? string.Empty,
                    lines,
                    createdBy);

                if (!result.Success) return result;

                doc.IsJournalized = true;
                doc.JournalizedAt = DateTime.UtcNow;
                await context.SaveChangesAsync();

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(JournalizeSetoffDocumentAsync), GetType(), _logger, new { Id = id });
                return (false, "轉傳票過程發生錯誤，請稍後再試");
            }
        }

        // ===== P2-B：材料領用傳票 =====

        // 材料領用科目代碼
        private const string WorkInProgressCode  = "1321"; // 在製品
        private const string SuppliesExpenseCode = "6311"; // 物料費用
        private const string FinishedGoodsCode   = "1241"; // 製成品

        public async Task<List<MaterialIssue>> GetPendingMaterialIssuesAsync(DateTime? from = null, DateTime? to = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.MaterialIssues
                    .Include(m => m.Employee)
                    .Include(m => m.MaterialIssueDetails)
                    .Where(m => m.IsConfirmed && !m.IsJournalized);

                if (from.HasValue)
                    query = query.Where(m => m.IssueDate >= from.Value.Date);
                if (to.HasValue)
                    query = query.Where(m => m.IssueDate <= to.Value.Date.AddDays(1).AddSeconds(-1));

                return await query.OrderByDescending(m => m.IssueDate).ThenByDescending(m => m.Code).ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPendingMaterialIssuesAsync), GetType(), _logger);
                return new List<MaterialIssue>();
            }
        }

        public async Task<(bool Success, string ErrorMessage)> JournalizeMaterialIssueAsync(int id, string createdBy)
        {
            try
            {
                var existing = await _journalEntryService.GetBySourceDocumentAsync("MaterialIssue", id);
                if (existing != null)
                    return (false, $"領料單已有對應傳票（{existing.Code}），請先沖銷後再重新轉傳票");

                using var context = await _contextFactory.CreateDbContextAsync();
                var doc = await context.MaterialIssues
                    .Include(m => m.MaterialIssueDetails)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (doc == null)
                    return (false, "找不到指定的領料單");
                if (!doc.IsConfirmed)
                    return (false, "領料單尚未確認，無法轉傳票");

                // 確認所有明細均已設定單位成本（避免部分 null 導致傳票金額不完整）
                var missingCostDetails = doc.MaterialIssueDetails
                    .Where(d => !d.UnitCost.HasValue || d.UnitCost.Value <= 0)
                    .ToList();

                if (missingCostDetails.Any())
                    return (false, $"領料單有 {missingCostDetails.Count} 筆明細尚未設定單位成本（或成本為零），無法建立傳票。請先確認所有明細的單位成本後再轉傳票");

                // 計算總成本（UnitCost × IssueQuantity）
                var totalCost = doc.MaterialIssueDetails
                    .Sum(d => d.UnitCost!.Value * d.IssueQuantity);

                if (totalCost <= 0)
                    return (false, "領料單總成本為零，無法建立傳票");

                // 依用途決定借方科目
                var inventory = await _accountItemService.GetByCodeAsync(InventoryCode);
                if (inventory == null)
                    return (false, $"找不到品項存貨科目（代碼 {InventoryCode}）");

                string debitCode = doc.IssueType == Models.Enums.MaterialIssueType.Production
                    ? WorkInProgressCode
                    : SuppliesExpenseCode;

                var debitAccount = await _accountItemService.GetByCodeAsync(debitCode);
                if (debitAccount == null)
                    return (false, $"找不到借方科目（代碼 {debitCode}）");

                string issueTypeLabel = doc.IssueType switch
                {
                    Models.Enums.MaterialIssueType.Production         => "在製品",
                    Models.Enums.MaterialIssueType.GeneralConsumption => "物料費用",
                    Models.Enums.MaterialIssueType.Sample             => "物料費用（樣品）",
                    _                                                  => "物料費用"
                };

                var lines = new List<JournalEntryLine>
                {
                    new JournalEntryLine
                    {
                        LineNumber    = 1,
                        AccountItemId = debitAccount.Id,
                        Direction     = AccountDirection.Debit,
                        Amount        = totalCost,
                        LineDescription = $"{issueTypeLabel}－{doc.Code}"
                    },
                    new JournalEntryLine
                    {
                        LineNumber    = 2,
                        AccountItemId = inventory.Id,
                        Direction     = AccountDirection.Credit,
                        Amount        = totalCost,
                        LineDescription = $"品項存貨領用－{doc.Code}"
                    }
                };

                var result = await CreateAndPostEntryAsync(
                    doc.IssueDate, $"領料：{doc.Code}",
                    "MaterialIssue", doc.Id, doc.Code ?? string.Empty,
                    lines, createdBy);

                if (result.Success)
                {
                    doc.IsJournalized = true;
                    doc.JournalizedAt = DateTime.UtcNow;
                    await context.SaveChangesAsync();
                }

                return result;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(JournalizeMaterialIssueAsync), GetType(), _logger, new { Id = id });
                return (false, "轉傳票過程發生錯誤，請稍後再試");
            }
        }

        // ===== P2-C：生產完工傳票 =====

        public async Task<List<ProductionScheduleCompletion>> GetPendingProductionCompletionsAsync(DateTime? from = null, DateTime? to = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.ProductionScheduleCompletions
                    .Include(c => c.ProductionScheduleItem)
                        .ThenInclude(i => i.Item)
                    .Include(c => c.InventoryTransaction)
                    .Where(c => !c.IsJournalized && c.InventoryTransactionId != null);

                if (from.HasValue)
                    query = query.Where(c => c.CompletionDate >= from.Value.Date);
                if (to.HasValue)
                    query = query.Where(c => c.CompletionDate <= to.Value.Date.AddDays(1).AddSeconds(-1));

                return await query.OrderByDescending(c => c.CompletionDate).ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPendingProductionCompletionsAsync), GetType(), _logger);
                return new List<ProductionScheduleCompletion>();
            }
        }

        public async Task<(bool Success, string ErrorMessage)> JournalizeProductionCompletionAsync(int id, string createdBy)
        {
            try
            {
                var existing = await _journalEntryService.GetBySourceDocumentAsync("ProductionScheduleCompletion", id);
                if (existing != null)
                    return (false, $"完工記錄已有對應傳票（{existing.Code}），請先沖銷後再重新轉傳票");

                using var context = await _contextFactory.CreateDbContextAsync();
                var completion = await context.ProductionScheduleCompletions
                    .Include(c => c.InventoryTransaction)
                    .Include(c => c.ProductionScheduleItem)
                        .ThenInclude(i => i.Item)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (completion == null)
                    return (false, "找不到指定的完工記錄");

                if (completion.InventoryTransactionId == null || completion.InventoryTransaction == null)
                    return (false, "完工記錄尚未建立庫存異動記錄，無法取得成本金額。請先完成入庫作業");

                var totalAmount = completion.InventoryTransaction.TotalAmount;
                if (totalAmount <= 0)
                    return (false, $"庫存異動金額為零或負數（{totalAmount}），無法建立有效的傳票分錄");

                var finishedGoods  = await _accountItemService.GetByCodeAsync(FinishedGoodsCode);
                var workInProgress = await _accountItemService.GetByCodeAsync(WorkInProgressCode);

                if (finishedGoods == null)
                    return (false, $"找不到製成品科目（代碼 {FinishedGoodsCode}）");
                if (workInProgress == null)
                    return (false, $"找不到在製品科目（代碼 {WorkInProgressCode}）");

                var productName = completion.ProductionScheduleItem?.Item?.Name ?? $"完工記錄 #{id}";

                var lines = new List<JournalEntryLine>
                {
                    new JournalEntryLine
                    {
                        LineNumber    = 1,
                        AccountItemId = finishedGoods.Id,
                        Direction     = AccountDirection.Debit,
                        Amount        = totalAmount,
                        LineDescription = $"製成品入庫－{productName}"
                    },
                    new JournalEntryLine
                    {
                        LineNumber    = 2,
                        AccountItemId = workInProgress.Id,
                        Direction     = AccountDirection.Credit,
                        Amount        = totalAmount,
                        LineDescription = $"在製品轉出－{productName}"
                    }
                };

                var docCode = completion.Code ?? $"Comp#{id}";
                var result = await CreateAndPostEntryAsync(
                    completion.CompletionDate, $"生產完工入庫：{productName}",
                    "ProductionScheduleCompletion", completion.Id, docCode,
                    lines, createdBy);

                if (result.Success)
                {
                    completion.IsJournalized = true;
                    completion.JournalizedAt = DateTime.UtcNow;
                    await context.SaveChangesAsync();
                }

                return result;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(JournalizeProductionCompletionAsync), GetType(), _logger, new { Id = id });
                return (false, "轉傳票過程發生錯誤，請稍後再試");
            }
        }

        // ===== 私有 Helper =====

        /// <summary>
        /// 取得客戶的應收帳款科目（優先使用子科目，fallback 統制科目 1191）
        /// </summary>
        private async Task<AccountItem?> GetARAccountForCustomerAsync(int customerId)
        {
            var sub = await _subAccountService.GetSubAccountForCustomerAsync(customerId);
            return sub ?? await _accountItemService.GetByCodeAsync(AccountReceivableCode);
        }

        /// <summary>
        /// 取得廠商的應付帳款科目（優先使用子科目，fallback 統制科目 2171）
        /// </summary>
        private async Task<AccountItem?> GetAPAccountForSupplierAsync(int supplierId)
        {
            var sub = await _subAccountService.GetSubAccountForSupplierAsync(supplierId);
            return sub ?? await _accountItemService.GetByCodeAsync(AccountPayableCode);
        }

        /// <summary>
        /// 建立傳票、呼叫 SaveWithLinesAsync（取得 Id）後立即 PostEntryAsync
        /// </summary>
        private async Task<(bool Success, string ErrorMessage)> CreateAndPostEntryAsync(
            DateTime entryDate,
            string description,
            string sourceDocumentType,
            int sourceDocumentId,
            string sourceDocumentCode,
            List<JournalEntryLine> lines,
            string createdBy)
        {
            var company = await _companyService.GetPrimaryCompanyAsync();
            if (company == null)
                return (false, "找不到預設公司，請先在系統設定中建立公司資料");

            var entry = new JournalEntry
            {
                EntryDate            = entryDate,
                EntryType            = JournalEntryType.AutoGenerated,
                JournalEntryStatus   = JournalEntryStatus.Draft,
                Description          = description,
                CompanyId            = company.Id,
                FiscalYear           = entryDate.Year,
                FiscalPeriod         = entryDate.Month,
                SourceDocumentType   = sourceDocumentType,
                SourceDocumentId     = sourceDocumentId,
                SourceDocumentCode   = sourceDocumentCode,
                Lines                = lines
            };

            var (saved, saveError) = await _journalEntryService.SaveWithLinesAsync(entry, createdBy);
            if (!saved)
                return (false, $"建立傳票失敗：{saveError}");

            // EF Core 在 SaveChangesAsync 後已將 entry.Id 回填
            var (posted, postError) = await _journalEntryService.PostEntryAsync(entry.Id, createdBy);
            if (!posted)
            {
                // 過帳失敗時自動作廢草稿，避免殘留草稿佔用 SourceDocumentType/Id
                // 導致下次轉傳票時誤報「已有對應傳票」
                await _journalEntryService.CancelDraftEntryAsync(entry.Id, createdBy);
                return (false, $"過帳失敗：{postError}");
            }

            return (true, string.Empty);
        }
    }
}
