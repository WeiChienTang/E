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
    /// 分錄規則（含稅；稅額為零時省略稅額行；COGS 為零時省略成本行）：
    ///   進貨入庫：借 商品存貨(1231) + 進項稅額(1268) / 貸 應付帳款(2171)
    ///   進貨退回：借 應付帳款(2171) / 貸 商品存貨(1231) + 進項稅額(1268)
    ///   銷貨出貨：借 應收帳款(1191) / 貸 銷貨收入(4111) + 銷項稅額(2204)
    ///             借 銷貨成本(5111) / 貸 商品存貨(1231)             ← COGS（移動加權平均）
    ///   銷貨退回：借 銷貨收入(4111) + 銷項稅額(2204) / 貸 應收帳款(1191)
    ///             借 商品存貨(1231) / 貸 銷貨成本(5111)             ← COGS 沖回
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
        private const string InventoryCode         = "1231"; // 商品存貨
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
                    .Where(pr => !pr.IsJournalized);

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
                    .Where(pr => !pr.IsJournalized);

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
                    .Where(sd => !sd.IsJournalized);

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
                    .Where(sr => !sr.IsJournalized);

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

                var inventory = await _accountItemService.GetByCodeAsync(InventoryCode);
                var payable   = await GetAPAccountForSupplierAsync(doc.SupplierId);
                var inputVat  = await _accountItemService.GetByCodeAsync(InputVatCode);

                if (inventory == null || payable == null)
                    return (false, "找不到必要的會計科目（商品存貨或應付帳款），請確認種子資料已正確載入");

                var lines = new List<JournalEntryLine>
                {
                    new JournalEntryLine
                    {
                        LineNumber = 1,
                        AccountItemId = inventory.Id,
                        Direction = AccountDirection.Debit,
                        Amount = doc.TotalAmount,
                        LineDescription = "商品存貨"
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
                    doc.JournalizedAt = DateTime.Now;
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

                var inventory = await _accountItemService.GetByCodeAsync(InventoryCode);
                var payable   = await GetAPAccountForSupplierAsync(doc.SupplierId);
                var inputVat  = await _accountItemService.GetByCodeAsync(InputVatCode);

                if (inventory == null || payable == null)
                    return (false, "找不到必要的會計科目（商品存貨或應付帳款），請確認種子資料已正確載入");

                // 借：應付帳款 / 貸：商品存貨 + 進項稅額（沖回）
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
                        LineDescription = "商品存貨退回"
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
                    doc.JournalizedAt = DateTime.Now;
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

                var receivable  = await GetARAccountForCustomerAsync(doc.CustomerId);
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
                        LineDescription = $"應收帳款－{doc.Customer?.CompanyName ?? doc.CustomerId.ToString()}"
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

                // 銷貨成本分錄（COGS）：借 銷貨成本(5111) / 貸 商品存貨(1231)
                // 金額來源：InventoryTransaction.TotalAmount（ReduceStockAsync 寫入的出庫成本 = 出庫量 × 移動加權均價）
                var cogsAmount = await context.InventoryTransactions
                    .Where(t => t.SourceDocumentType == "SalesDelivery" && t.SourceDocumentId == id)
                    .SumAsync(t => t.TotalAmount);

                if (cogsAmount > 0)
                {
                    var cogs      = await _accountItemService.GetByCodeAsync(CostOfGoodsSoldCode);
                    var inventory = await _accountItemService.GetByCodeAsync(InventoryCode);

                    if (cogs != null && inventory != null)
                    {
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
                            LineDescription = "商品存貨－銷貨出倉"
                        });
                    }
                }

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
                    doc.JournalizedAt = DateTime.Now;
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

                var receivable  = await GetARAccountForCustomerAsync(doc.CustomerId);
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
                    LineDescription = $"應收帳款沖回－{doc.Customer?.CompanyName ?? doc.CustomerId.ToString()}"
                });

                // 銷貨退回成本沖回：借 商品存貨(1231) / 貸 銷貨成本(5111)
                // 金額來源：InventoryTransaction.TotalAmount（AddStockAsync 寫入的退回入庫成本）
                var returnCogsAmount = await context.InventoryTransactions
                    .Where(t => t.SourceDocumentType == "SalesReturn" && t.SourceDocumentId == id)
                    .SumAsync(t => t.TotalAmount);

                if (returnCogsAmount > 0)
                {
                    var cogs      = await _accountItemService.GetByCodeAsync(CostOfGoodsSoldCode);
                    var inventory = await _accountItemService.GetByCodeAsync(InventoryCode);

                    if (cogs != null && inventory != null)
                    {
                        lines.Add(new JournalEntryLine
                        {
                            LineNumber = lines.Count + 1,
                            AccountItemId = inventory.Id,
                            Direction = AccountDirection.Debit,
                            Amount = returnCogsAmount,
                            LineDescription = "商品存貨－退回入庫"
                        });
                        lines.Add(new JournalEntryLine
                        {
                            LineNumber = lines.Count + 1,
                            AccountItemId = cogs.Id,
                            Direction = AccountDirection.Credit,
                            Amount = returnCogsAmount,
                            LineDescription = "銷貨成本沖回"
                        });
                    }
                }

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
                    doc.JournalizedAt = DateTime.Now;
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
                    // 應收沖款：借 銀行存款+銷貨折讓+預收貨款 / 貸 應收帳款
                    var receivable = await _accountItemService.GetByCodeAsync(AccountReceivableCode);
                    if (receivable == null) return (false, $"找不到科目 {AccountReceivableCode}（應收帳款）");

                    lines = new List<JournalEntryLine>();
                    int lineNum = 1;

                    if (doc.TotalCollectionAmount > 0)
                        lines.Add(new JournalEntryLine
                        {
                            LineNumber = lineNum++,
                            AccountItemId = bank.Id,
                            Direction = AccountDirection.Debit,
                            Amount = doc.TotalCollectionAmount,
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

                    if (doc.PrepaymentSetoffAmount > 0)
                    {
                        var advanceFromCustomer = await _accountItemService.GetByCodeAsync(AdvanceFromCustomerCode);
                        if (advanceFromCustomer == null) return (false, $"找不到科目 {AdvanceFromCustomerCode}（預收貨款）");
                        lines.Add(new JournalEntryLine
                        {
                            LineNumber = lineNum++,
                            AccountItemId = advanceFromCustomer.Id,
                            Direction = AccountDirection.Debit,
                            Amount = doc.PrepaymentSetoffAmount,
                            LineDescription = "預收貨款沖回"
                        });
                    }

                    lines.Add(new JournalEntryLine
                    {
                        LineNumber = lineNum,
                        AccountItemId = receivable.Id,
                        Direction = AccountDirection.Credit,
                        Amount = doc.CurrentSetoffAmount,
                        LineDescription = $"應收帳款沖銷－{doc.Code}"
                    });

                    description = $"應收沖款：{doc.Code}";
                }
                else
                {
                    // 應付沖款：借 應付帳款 / 貸 銀行存款+進貨折讓+預付貨款
                    var payable = await _accountItemService.GetByCodeAsync(AccountPayableCode);
                    if (payable == null) return (false, $"找不到科目 {AccountPayableCode}（應付帳款）");

                    lines = new List<JournalEntryLine>
                    {
                        new JournalEntryLine
                        {
                            LineNumber = 1,
                            AccountItemId = payable.Id,
                            Direction = AccountDirection.Debit,
                            Amount = doc.CurrentSetoffAmount,
                            LineDescription = $"應付帳款沖銷－{doc.Code}"
                        }
                    };
                    int lineNum = 2;

                    if (doc.TotalCollectionAmount > 0)
                        lines.Add(new JournalEntryLine
                        {
                            LineNumber = lineNum++,
                            AccountItemId = bank.Id,
                            Direction = AccountDirection.Credit,
                            Amount = doc.TotalCollectionAmount,
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

                    if (doc.PrepaymentSetoffAmount > 0)
                    {
                        var advanceToSupplier = await _accountItemService.GetByCodeAsync(AdvanceToSupplierCode);
                        if (advanceToSupplier == null) return (false, $"找不到科目 {AdvanceToSupplierCode}（預付貨款）");
                        lines.Add(new JournalEntryLine
                        {
                            LineNumber = lineNum,
                            AccountItemId = advanceToSupplier.Id,
                            Direction = AccountDirection.Credit,
                            Amount = doc.PrepaymentSetoffAmount,
                            LineDescription = "預付貨款沖回"
                        });
                    }

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
                doc.JournalizedAt = DateTime.Now;
                await context.SaveChangesAsync();

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(JournalizeSetoffDocumentAsync), GetType(), _logger, new { Id = id });
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
                return (false, $"過帳失敗：{postError}");

            return (true, string.Empty);
        }
    }
}
