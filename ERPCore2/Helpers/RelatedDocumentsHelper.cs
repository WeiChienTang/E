using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Models;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Helpers
{
    /// <summary>
    /// 相關單據查詢 Helper - 查詢與明細項目相關的退貨單和沖款單
    /// </summary>
    public class RelatedDocumentsHelper
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public RelatedDocumentsHelper(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
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
                    DocumentNumber = detail.PurchaseReturn.PurchaseReturnNumber,
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
                documents.Add(new RelatedDocument
                {
                    DocumentId = detail.SetoffDocumentId,
                    DocumentType = RelatedDocumentType.SetoffDocument,
                    DocumentNumber = detail.SetoffDocument.SetoffNumber,
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

            // 1. 查詢退貨單
            var returnDetails = await context.SalesReturnDetails
                .Include(d => d.SalesReturn)
                .Where(d => d.SalesOrderDetailId == salesOrderDetailId)
                .ToListAsync();

            foreach (var detail in returnDetails)
            {
                documents.Add(new RelatedDocument
                {
                    DocumentId = detail.SalesReturnId,
                    DocumentType = RelatedDocumentType.ReturnDocument,
                    DocumentNumber = detail.SalesReturn.SalesReturnNumber,
                    DocumentDate = detail.SalesReturn.ReturnDate,
                    Quantity = detail.ReturnQuantity,
                    Remarks = detail.SalesReturn.Remarks
                });
            }

            // 2. 查詢沖款單
            var setoffDetails = await context.SetoffProductDetails
                .Include(d => d.SetoffDocument)
                .Where(d => d.SourceDetailType == SetoffDetailType.SalesOrderDetail 
                         && d.SourceDetailId == salesOrderDetailId)
                .ToListAsync();

            foreach (var detail in setoffDetails)
            {
                documents.Add(new RelatedDocument
                {
                    DocumentId = detail.SetoffDocumentId,
                    DocumentType = RelatedDocumentType.SetoffDocument,
                    DocumentNumber = detail.SetoffDocument.SetoffNumber,
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
                documents.Add(new RelatedDocument
                {
                    DocumentId = detail.SetoffDocumentId,
                    DocumentType = RelatedDocumentType.SetoffDocument,
                    DocumentNumber = detail.SetoffDocument.SetoffNumber,
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
                documents.Add(new RelatedDocument
                {
                    DocumentId = detail.SetoffDocumentId,
                    DocumentType = RelatedDocumentType.SetoffDocument,
                    DocumentNumber = detail.SetoffDocument.SetoffNumber,
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
                    DocumentNumber = detail.PurchaseReceiving.ReceiptNumber,
                    DocumentDate = detail.PurchaseReceiving.ReceiptDate,
                    Quantity = detail.ReceivedQuantity,
                    UnitPrice = detail.UnitPrice,
                    Remarks = detail.PurchaseReceiving.Remarks
                });
            }

            return documents.OrderByDescending(d => d.DocumentDate).ToList();
        }
    }
}
