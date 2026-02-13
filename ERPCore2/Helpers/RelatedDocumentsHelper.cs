using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Models;
using ERPCore2.Services;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Helpers
{
    /// <summary>
    /// 相關單據查詢 Helper - 查詢與明細項目相關的退貨單和沖款單
    /// </summary>
    public class RelatedDocumentsHelper
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IProductCompositionService _productCompositionService;

        public RelatedDocumentsHelper(
            IDbContextFactory<AppDbContext> contextFactory,
            IProductCompositionService productCompositionService)
        {
            _contextFactory = contextFactory;
            _productCompositionService = productCompositionService;
        }

        /// <summary>
        /// 取得與進貨明細相關的單據（退貨單 + 沖款單）
        /// </summary>
        public async Task<List<RelatedDocument>> GetRelatedDocumentsForPurchaseReceivingDetailAsync(int purchaseReceivingDetailId)
        {
            var documents = new List<RelatedDocument>();

            using var context = await _contextFactory.CreateDbContextAsync();

            // 1. 查詢退貨單
            var returnDetails = await context.PurchaseReturnDetails
                .Include(d => d.PurchaseReturn)
                .Where(d => d.PurchaseReceivingDetailId == purchaseReceivingDetailId)
                .ToListAsync();

            foreach (var detail in returnDetails)
            {
                documents.Add(new RelatedDocument
                {
                    DocumentId = detail.PurchaseReturnId,
                    DocumentType = RelatedDocumentType.ReturnDocument,
                    DocumentNumber = detail.PurchaseReturn.Code ?? string.Empty,
                    DocumentDate = detail.PurchaseReturn.ReturnDate,
                    Quantity = detail.ReturnQuantity,
                    Remarks = detail.PurchaseReturn.Remarks
                });
            }

            // 2. 查詢沖款單
            var setoffDetails = await context.SetoffProductDetails
                .Include(d => d.SetoffDocument)
                .Where(d => d.SourceDetailType == SetoffDetailType.PurchaseReceivingDetail 
                         && d.SourceDetailId == purchaseReceivingDetailId)
                .ToListAsync();

            foreach (var detail in setoffDetails)
            {
                if (detail.SetoffDocument == null) continue;
                
                documents.Add(new RelatedDocument
                {
                    DocumentId = detail.SetoffDocumentId,
                    DocumentType = RelatedDocumentType.SetoffDocument,
                    DocumentNumber = detail.SetoffDocument.Code ?? string.Empty,
                    DocumentDate = detail.SetoffDocument.SetoffDate,
                    Amount = detail.CurrentSetoffAmount,
                    CurrentAmount = detail.CurrentSetoffAmount,
                    TotalAmount = detail.TotalSetoffAmount,
                    Remarks = detail.SetoffDocument.Remarks
                });
            }

            return documents.OrderByDescending(d => d.DocumentDate).ToList();
        }

        /// <summary>
        /// 取得與銷貨訂單明細相關的單據（退貨單 + 沖款單）
        /// </summary>
        public async Task<List<RelatedDocument>> GetRelatedDocumentsForSalesOrderDetailAsync(int salesOrderDetailId)
        {
            var documents = new List<RelatedDocument>();

            using var context = await _contextFactory.CreateDbContextAsync();

            // 1. 查詢銷貨單/出貨單
            var deliveryDetails = await context.SalesDeliveryDetails
                .Include(d => d.SalesDelivery)
                .Where(d => d.SalesOrderDetailId == salesOrderDetailId)
                .ToListAsync();

            foreach (var detail in deliveryDetails)
            {
                documents.Add(new RelatedDocument
                {
                    DocumentId = detail.SalesDeliveryId,
                    DocumentType = RelatedDocumentType.DeliveryDocument,
                    DocumentNumber = detail.SalesDelivery.Code ?? string.Empty,
                    DocumentDate = detail.SalesDelivery.DeliveryDate,
                    Quantity = detail.DeliveryQuantity,
                    UnitPrice = detail.UnitPrice,
                    Remarks = detail.SalesDelivery.Remarks
                });
            }

            // 2. 查詢生產排程
            var scheduleItems = await context.ProductionScheduleItems
                .Include(i => i.ProductionSchedule)
                .Include(i => i.Product)
                .Where(i => i.SalesOrderDetailId == salesOrderDetailId)
                .ToListAsync();

            foreach (var item in scheduleItems)
            {
                documents.Add(new RelatedDocument
                {
                    DocumentId = item.ProductionScheduleId,
                    DocumentType = RelatedDocumentType.ProductionSchedule,
                    DocumentNumber = item.ProductionSchedule.Code ?? string.Empty,
                    DocumentDate = item.ProductionSchedule.ScheduleDate,
                    Quantity = item.ScheduledQuantity,
                    Remarks = $"{item.Product.Name} - {item.ProductionItemStatus.ToString()}"
                });
            }

            // 3. 查詢退貨單 - 注意：退貨現在是從出貨單產生，不直接從訂單
            // 透過出貨明細查詢退貨明細
            var deliveryDetailIds = deliveryDetails.Select(d => d.Id).ToList();
            if (deliveryDetailIds.Any())
            {
                var returnDetails = await context.SalesReturnDetails
                    .Include(d => d.SalesReturn)
                    .Where(d => deliveryDetailIds.Contains(d.SalesDeliveryDetailId ?? 0))
                    .ToListAsync();

                foreach (var detail in returnDetails)
                {
                    documents.Add(new RelatedDocument
                    {
                        DocumentId = detail.SalesReturnId,
                        DocumentType = RelatedDocumentType.ReturnDocument,
                        DocumentNumber = detail.SalesReturn.Code ?? string.Empty,
                        DocumentDate = detail.SalesReturn.ReturnDate,
                        Quantity = detail.ReturnQuantity,
                        Remarks = detail.SalesReturn.Remarks
                    });
                }
            }

            // 4. 查詢沖款單
            var setoffDetails = await context.SetoffProductDetails
                .Include(d => d.SetoffDocument)
                .Where(d => d.SourceDetailType == SetoffDetailType.SalesOrderDetail 
                         && d.SourceDetailId == salesOrderDetailId)
                .ToListAsync();

            foreach (var detail in setoffDetails)
            {
                if (detail.SetoffDocument == null) continue;
                
                documents.Add(new RelatedDocument
                {
                    DocumentId = detail.SetoffDocumentId,
                    DocumentType = RelatedDocumentType.SetoffDocument,
                    DocumentNumber = detail.SetoffDocument.Code ?? string.Empty,
                    DocumentDate = detail.SetoffDocument.SetoffDate,
                    Amount = detail.CurrentSetoffAmount,
                    CurrentAmount = detail.CurrentSetoffAmount,
                    TotalAmount = detail.TotalSetoffAmount,
                    Remarks = detail.SetoffDocument.Remarks
                });
            }

            return documents.OrderByDescending(d => d.DocumentDate).ToList();
        }

        /// <summary>
        /// 取得與採購退貨明細相關的單據（沖款單）
        /// </summary>
        public async Task<List<RelatedDocument>> GetRelatedDocumentsForPurchaseReturnDetailAsync(int purchaseReturnDetailId)
        {
            var documents = new List<RelatedDocument>();

            using var context = await _contextFactory.CreateDbContextAsync();

            // 查詢沖款單
            var setoffDetails = await context.SetoffProductDetails
                .Include(d => d.SetoffDocument)
                .Where(d => d.SourceDetailType == SetoffDetailType.PurchaseReturnDetail 
                         && d.SourceDetailId == purchaseReturnDetailId)
                .ToListAsync();

            foreach (var detail in setoffDetails)
            {
                if (detail.SetoffDocument == null) continue;
                
                documents.Add(new RelatedDocument
                {
                    DocumentId = detail.SetoffDocumentId,
                    DocumentType = RelatedDocumentType.SetoffDocument,
                    DocumentNumber = detail.SetoffDocument.Code ?? string.Empty,
                    DocumentDate = detail.SetoffDocument.SetoffDate,
                    Amount = detail.CurrentSetoffAmount,
                    CurrentAmount = detail.CurrentSetoffAmount,
                    TotalAmount = detail.TotalSetoffAmount,
                    Remarks = detail.SetoffDocument.Remarks
                });
            }

            return documents.OrderByDescending(d => d.DocumentDate).ToList();
        }

        /// <summary>
        /// 取得與銷貨退回明細相關的單據（沖款單）
        /// </summary>
        public async Task<List<RelatedDocument>> GetRelatedDocumentsForSalesReturnDetailAsync(int salesReturnDetailId)
        {
            var documents = new List<RelatedDocument>();

            using var context = await _contextFactory.CreateDbContextAsync();

            // 查詢沖款單
            var setoffDetails = await context.SetoffProductDetails
                .Include(d => d.SetoffDocument)
                .Where(d => d.SourceDetailType == SetoffDetailType.SalesReturnDetail 
                         && d.SourceDetailId == salesReturnDetailId)
                .ToListAsync();

            foreach (var detail in setoffDetails)
            {
                if (detail.SetoffDocument == null) continue;
                
                documents.Add(new RelatedDocument
                {
                    DocumentId = detail.SetoffDocumentId,
                    DocumentType = RelatedDocumentType.SetoffDocument,
                    DocumentNumber = detail.SetoffDocument.Code ?? string.Empty,
                    DocumentDate = detail.SetoffDocument.SetoffDate,
                    Amount = detail.CurrentSetoffAmount,
                    CurrentAmount = detail.CurrentSetoffAmount,
                    TotalAmount = detail.TotalSetoffAmount,
                    Remarks = detail.SetoffDocument.Remarks
                });
            }

            return documents.OrderByDescending(d => d.DocumentDate).ToList();
        }

        /// <summary>
        /// 取得與銷貨出貨明細相關的單據（退貨單、收款單）
        /// </summary>
        public async Task<List<RelatedDocument>> GetRelatedDocumentsForSalesDeliveryDetailAsync(int salesDeliveryDetailId)
        {
            var documents = new List<RelatedDocument>();

            using var context = await _contextFactory.CreateDbContextAsync();

            // 先取得出貨明細及其關聯的訂單明細
            var deliveryDetail = await context.SalesDeliveryDetails
                .Include(d => d.SalesOrderDetail)
                .FirstOrDefaultAsync(d => d.Id == salesDeliveryDetailId);

            if (deliveryDetail == null)
            {
                return documents;
            }

            // 1. 查詢退貨單
            var returnDetails = await context.SalesReturnDetails
                .Include(d => d.SalesReturn)
                .Where(d => d.SalesDeliveryDetailId == salesDeliveryDetailId)
                .ToListAsync();

            foreach (var detail in returnDetails)
            {
                documents.Add(new RelatedDocument
                {
                    DocumentId = detail.SalesReturnId,
                    DocumentType = RelatedDocumentType.ReturnDocument,
                    DocumentNumber = detail.SalesReturn.Code ?? string.Empty,
                    DocumentDate = detail.SalesReturn.ReturnDate,
                    Quantity = detail.ReturnQuantity,
                    Remarks = detail.SalesReturn.Remarks
                });
            }

            // 2. 查詢沖款單（透過關聯的訂單明細查詢）
            if (deliveryDetail.SalesOrderDetailId.HasValue)
            {
                var setoffDetails = await context.SetoffProductDetails
                    .Include(d => d.SetoffDocument)
                    .Where(d => d.SourceDetailType == SetoffDetailType.SalesOrderDetail 
                             && d.SourceDetailId == deliveryDetail.SalesOrderDetailId.Value)
                    .ToListAsync();

                foreach (var detail in setoffDetails)
                {
                    if (detail.SetoffDocument == null) continue;
                    
                    documents.Add(new RelatedDocument
                    {
                        DocumentId = detail.SetoffDocumentId,
                        DocumentType = RelatedDocumentType.SetoffDocument,
                        DocumentNumber = detail.SetoffDocument.Code ?? string.Empty,
                        DocumentDate = detail.SetoffDocument.SetoffDate,
                        Amount = detail.CurrentSetoffAmount,
                        CurrentAmount = detail.CurrentSetoffAmount,
                        TotalAmount = detail.TotalSetoffAmount,
                        Remarks = detail.SetoffDocument.Remarks
                    });
                }
            }

            return documents.OrderByDescending(d => d.DocumentDate).ToList();
        }

        /// <summary>
        /// 取得與採購訂單明細相關的單據（入庫單）
        /// </summary>
        public async Task<List<RelatedDocument>> GetRelatedDocumentsForPurchaseOrderDetailAsync(int purchaseOrderDetailId)
        {
            var documents = new List<RelatedDocument>();

            using var context = await _contextFactory.CreateDbContextAsync();

            // 查詢入庫單
            var receivingDetails = await context.PurchaseReceivingDetails
                .Include(d => d.PurchaseReceiving)
                .Where(d => d.PurchaseOrderDetailId == purchaseOrderDetailId)
                .ToListAsync();

            foreach (var detail in receivingDetails)
            {
                documents.Add(new RelatedDocument
                {
                    DocumentId = detail.PurchaseReceivingId,
                    DocumentType = RelatedDocumentType.ReceivingDocument,
                    DocumentNumber = detail.PurchaseReceiving.Code ?? string.Empty,
                    DocumentDate = detail.PurchaseReceiving.ReceiptDate,
                    Quantity = detail.ReceivedQuantity,
                    UnitPrice = detail.UnitPrice,
                    Remarks = detail.PurchaseReceiving.Remarks
                });
            }

            return documents.OrderByDescending(d => d.DocumentDate).ToList();
        }

        /// <summary>
        /// 取得與報價單明細相關的單據（銷貨訂單）
        /// </summary>
        public async Task<List<RelatedDocument>> GetRelatedDocumentsByQuotationDetailAsync(int quotationDetailId)
        {
            var documents = new List<RelatedDocument>();

            using var context = await _contextFactory.CreateDbContextAsync();

            // 查詢銷貨訂單
            var salesOrderDetails = await context.SalesOrderDetails
                .Include(d => d.SalesOrder)
                .Where(d => d.QuotationDetailId == quotationDetailId)
                .ToListAsync();

            foreach (var detail in salesOrderDetails)
            {
                documents.Add(new RelatedDocument
                {
                    DocumentId = detail.SalesOrderId,
                    DocumentType = RelatedDocumentType.SalesOrder,
                    DocumentNumber = detail.SalesOrder.Code ?? string.Empty,
                    DocumentDate = detail.SalesOrder.OrderDate,
                    Quantity = detail.OrderQuantity,
                    UnitPrice = detail.UnitPrice,
                    Remarks = detail.SalesOrder.Remarks
                });
            }

            return documents.OrderByDescending(d => d.DocumentDate).ToList();
        }

        /// <summary>
        /// 取得與預收付款項相關的單據（使用此預收付款項的所有沖款單）
        /// </summary>
        public async Task<List<RelatedDocument>> GetRelatedDocumentsForPrepaymentAsync(int prepaymentId)
        {
            var documents = new List<RelatedDocument>();

            using var context = await _contextFactory.CreateDbContextAsync();

            // 查詢使用此預收付款項的所有沖款單
            var usageRecords = await context.SetoffPrepaymentUsages
                .Include(u => u.SetoffDocument)
                .Where(u => u.SetoffPrepaymentId == prepaymentId)
                .OrderByDescending(u => u.UsageDate)
                .ToListAsync();

            foreach (var usage in usageRecords)
            {
                if (usage.SetoffDocument == null) continue;
                
                documents.Add(new RelatedDocument
                {
                    DocumentId = usage.SetoffDocumentId,
                    DocumentType = RelatedDocumentType.SetoffDocument,
                    DocumentNumber = usage.SetoffDocument.Code ?? string.Empty,
                    DocumentDate = usage.SetoffDocument.SetoffDate,
                    Amount = usage.UsedAmount,  // 使用金額
                    Remarks = usage.SetoffDocument.Remarks
                });
            }

            return documents;
        }
        
        /// <summary>
        /// 取得指定商品的所有物料清單
        /// </summary>
        public async Task<List<RelatedDocument>> GetProductCompositionsAsync(int productId)
        {
            var documents = new List<RelatedDocument>();
            
            try
            {
                // 從 ProductCompositionService 取得該商品的所有物料清單
                var compositions = await _productCompositionService.GetByProductIdAsync(productId);
                
                foreach (var composition in compositions)
                {
                    // 組合備註資訊
                    var remarksParts = new List<string>();
                    
                    if (composition.CompositionCategory != null)
                    {
                        remarksParts.Add($"類型：{composition.CompositionCategory.Name}");
                    }
                    
                    if (composition.Customer != null)
                    {
                        remarksParts.Add($"客戶：{composition.Customer.CompanyName}");
                    }
                    
                    if (!string.IsNullOrEmpty(composition.Specification))
                    {
                        remarksParts.Add($"規格：{composition.Specification}");
                    }
                    
                    if (!string.IsNullOrEmpty(composition.Remarks))
                    {
                        remarksParts.Add(composition.Remarks);
                    }
                    
                    documents.Add(new RelatedDocument
                    {
                        DocumentId = composition.Id,
                        DocumentType = RelatedDocumentType.ProductComposition,
                        DocumentNumber = composition.Code ?? string.Empty,
                        DocumentDate = composition.CreatedAt,
                        Remarks = remarksParts.Any() ? string.Join(" | ", remarksParts) : null
                    });
                }
            }
            catch
            {
                // 發生錯誤時返回空列表
                return new List<RelatedDocument>();
            }
            
            return documents.OrderByDescending(d => d.DocumentDate).ToList();
        }

        #region 批次查詢方法 - 解決 N+1 問題

        /// <summary>
        /// 批次檢查多個採購訂單明細是否有使用紀錄（入庫單）
        /// 優化 N+1 查詢問題：將 N 次查詢合併為 1 次
        /// </summary>
        /// <param name="detailIds">採購訂單明細 ID 列表</param>
        /// <returns>Dictionary，Key 為明細 ID，Value 為是否有使用紀錄</returns>
        public async Task<Dictionary<int, bool>> HasUsageRecordBatchForPurchaseOrderDetailsAsync(List<int> detailIds)
        {
            var result = new Dictionary<int, bool>();
            if (detailIds == null || !detailIds.Any()) return result;

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                // 單次 DB 查詢：找出所有有入庫紀錄的 PurchaseOrderDetailId
                var usedIds = await context.PurchaseReceivingDetails
                    .Where(d => d.PurchaseOrderDetailId.HasValue && detailIds.Contains(d.PurchaseOrderDetailId.Value))
                    .Select(d => d.PurchaseOrderDetailId!.Value)
                    .Distinct()
                    .ToListAsync();

                foreach (var id in detailIds)
                {
                    result[id] = usedIds.Contains(id);
                }
            }
            catch
            {
                // 發生錯誤時，全部回傳 false
                foreach (var id in detailIds)
                {
                    result[id] = false;
                }
            }

            return result;
        }

        /// <summary>
        /// 批次檢查多個進貨明細是否有使用紀錄（退貨單 + 沖款單）
        /// </summary>
        public async Task<Dictionary<int, bool>> HasUsageRecordBatchForPurchaseReceivingDetailsAsync(List<int> detailIds)
        {
            var result = new Dictionary<int, bool>();
            if (detailIds == null || !detailIds.Any()) return result;

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                // 查詢退貨單使用紀錄 (PurchaseReceivingDetailId 是 int? 類型)
                var returnUsedIds = await context.PurchaseReturnDetails
                    .Where(d => d.PurchaseReceivingDetailId.HasValue && detailIds.Contains(d.PurchaseReceivingDetailId.Value))
                    .Select(d => d.PurchaseReceivingDetailId!.Value)
                    .Distinct()
                    .ToListAsync();

                // 查詢沖款單使用紀錄
                var setoffUsedIds = await context.SetoffProductDetails
                    .Where(d => d.SourceDetailType == SetoffDetailType.PurchaseReceivingDetail 
                             && detailIds.Contains(d.SourceDetailId))
                    .Select(d => d.SourceDetailId)
                    .Distinct()
                    .ToListAsync();

                var allUsedIds = returnUsedIds.Concat(setoffUsedIds).Distinct().ToHashSet();

                foreach (var id in detailIds)
                {
                    result[id] = allUsedIds.Contains(id);
                }
            }
            catch
            {
                foreach (var id in detailIds)
                {
                    result[id] = false;
                }
            }

            return result;
        }

        /// <summary>
        /// 批次檢查多個銷貨訂單明細是否有使用紀錄（出貨單 + 排程）
        /// </summary>
        public async Task<Dictionary<int, bool>> HasUsageRecordBatchForSalesOrderDetailsAsync(List<int> detailIds)
        {
            var result = new Dictionary<int, bool>();
            if (detailIds == null || !detailIds.Any()) return result;

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                // 查詢出貨單使用紀錄
                var deliveryUsedIds = await context.SalesDeliveryDetails
                    .Where(d => d.SalesOrderDetailId.HasValue && detailIds.Contains(d.SalesOrderDetailId.Value))
                    .Select(d => d.SalesOrderDetailId!.Value)
                    .Distinct()
                    .ToListAsync();

                // 查詢生產排程使用紀錄
                var scheduleUsedIds = await context.ProductionScheduleItems
                    .Where(i => i.SalesOrderDetailId.HasValue && detailIds.Contains(i.SalesOrderDetailId.Value))
                    .Select(i => i.SalesOrderDetailId!.Value)
                    .Distinct()
                    .ToListAsync();

                var allUsedIds = deliveryUsedIds.Concat(scheduleUsedIds).Distinct().ToHashSet();

                foreach (var id in detailIds)
                {
                    result[id] = allUsedIds.Contains(id);
                }
            }
            catch
            {
                foreach (var id in detailIds)
                {
                    result[id] = false;
                }
            }

            return result;
        }

        /// <summary>
        /// 批次檢查多個報價單明細是否有使用紀錄（銷貨訂單）
        /// </summary>
        public async Task<Dictionary<int, bool>> HasUsageRecordBatchForQuotationDetailsAsync(List<int> detailIds)
        {
            var result = new Dictionary<int, bool>();
            if (detailIds == null || !detailIds.Any()) return result;

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var usedIds = await context.SalesOrderDetails
                    .Where(d => d.QuotationDetailId.HasValue && detailIds.Contains(d.QuotationDetailId.Value))
                    .Select(d => d.QuotationDetailId!.Value)
                    .Distinct()
                    .ToListAsync();

                foreach (var id in detailIds)
                {
                    result[id] = usedIds.Contains(id);
                }
            }
            catch
            {
                foreach (var id in detailIds)
                {
                    result[id] = false;
                }
            }

            return result;
        }

        /// <summary>
        /// 批次檢查多個銷貨出貨明細是否有使用紀錄（退貨單 + 沖款單）
        /// </summary>
        public async Task<Dictionary<int, bool>> HasUsageRecordBatchForSalesDeliveryDetailsAsync(List<int> detailIds)
        {
            var result = new Dictionary<int, bool>();
            if (detailIds == null || !detailIds.Any()) return result;

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                // 查詢退貨單使用紀錄
                var returnUsedIds = await context.SalesReturnDetails
                    .Where(d => d.SalesDeliveryDetailId.HasValue && detailIds.Contains(d.SalesDeliveryDetailId.Value))
                    .Select(d => d.SalesDeliveryDetailId!.Value)
                    .Distinct()
                    .ToListAsync();

                // 查詢沖款單使用紀錄
                var setoffUsedIds = await context.SetoffProductDetails
                    .Where(d => d.SourceDetailType == SetoffDetailType.SalesDeliveryDetail 
                             && detailIds.Contains(d.SourceDetailId))
                    .Select(d => d.SourceDetailId)
                    .Distinct()
                    .ToListAsync();

                var allUsedIds = returnUsedIds.Concat(setoffUsedIds).Distinct().ToHashSet();

                foreach (var id in detailIds)
                {
                    result[id] = allUsedIds.Contains(id);
                }
            }
            catch
            {
                foreach (var id in detailIds)
                {
                    result[id] = false;
                }
            }

            return result;
        }

        #endregion
    }
}
