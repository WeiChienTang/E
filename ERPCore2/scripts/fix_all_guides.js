const fs = require('fs');
const path = require('path');
const guideDir = path.join(__dirname, '..', 'Models', 'FeatureGuides', 'GuideDefinitions');
const resxDir = path.join(__dirname, '..', 'Resources');

// ==================== Helper: Generate .cs file ====================
function genCs(cls, prefix, sections) {
  const sectionCode = sections.map(s => {
    let items;
    if (s.items) {
      items = s.items.map(i => {
        if (typeof i === 'string') {
          // Single-arg: Description/Steps
          if (s.type === 'Tips' && i.includes(',')) {
            const [key, style] = i.split(',');
            return `                    new("${key.trim()}", GuideItemStyle.${style.trim()})`;
          }
          return `                    new("${i}")`;
        }
        // Two-arg: FieldList/FAQ
        return `                    new("${i[0]}", "${i[1]}")`;
      }).join(',\n');
    } else {
      items = '';
    }

    return `
            new GuideSection
            {
                Id = "guide-${prefix}-${s.id}",
                TitleKey = "${s.titleKey}",
                Icon = "${s.icon}",
                BookmarkLabel = "${s.label}",
                BookmarkColor = "${s.color}",
                Type = GuideSectionType.${s.type},
                Items =
                {
${items}
                }
            },`;
  }).join('\n');

  return `using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

public static class ${cls}
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {${sectionCode}
        }
    };
}
`;
}

// ==================== Define all guides to rewrite ====================

const rewrites = {};

// ===== SalesOrder =====
rewrites['SalesOrderGuide'] = { prefix: 'so', sections: [
  { id: 'overview', titleKey: 'Guide.Overview', icon: 'bi-info-circle', label: '概述', color: '#3B82F6', type: 'Description',
    items: ['Guide.SalesOrder.Description'] },
  { id: 'tabs', titleKey: 'Guide.TabDescriptions', icon: 'bi-folder2-open', label: '分頁', color: '#8B5CF6', type: 'FieldList',
    items: [['Tab.OrderData','Guide.SalesOrder.Tab.OrderData'],['Tab.CustomerInfo','Guide.SalesOrder.Tab.CustomerInfo'],['Tab.SalesOrderPhotos','Guide.SalesOrder.Tab.Photos']] },
  { id: 'fields-basic', titleKey: 'Guide.SalesOrder.BasicFieldsTitle', icon: 'bi-input-cursor-text', label: '欄位', color: '#F59E0B', type: 'FieldList',
    items: [['Field.SalesOrderCode','Guide.SalesOrder.Field.Code'],['Entity.Customer','Guide.SalesOrder.Field.Customer'],['Entity.Company','Guide.SalesOrder.Field.Company'],['Field.SalesPerson','Guide.SalesOrder.Field.Salesperson'],['Field.QuotationCreator','Guide.SalesOrder.Field.FormCreator'],['Field.SalesOrderDate','Guide.SalesOrder.Field.OrderDate'],['Field.ExpectedDeliveryDate','Guide.SalesOrder.Field.ExpectedDeliveryDate'],['Field.PaymentTerms','Guide.SalesOrder.Field.PaymentTerms'],['Field.DeliveryTerms','Guide.SalesOrder.Field.DeliveryTerms'],['Field.Remarks','Guide.SalesOrder.Field.Remarks']] },
  { id: 'fields-amount', titleKey: 'Guide.SalesOrder.AmountFieldsTitle', icon: 'bi-calculator', label: '金額', color: '#059669', type: 'FieldList',
    items: [['Field.TaxType','Guide.SalesOrder.Field.TaxMethod'],['Field.TotalAmountFull','Guide.SalesOrder.Field.TotalAmount'],['Field.DiscountAmount','Guide.SalesOrder.Field.DiscountAmount'],['Field.TaxAmount','Guide.SalesOrder.Field.SalesTaxAmount'],['Field.TotalAmountWithTax','Guide.SalesOrder.Field.TotalAmountWithTax']] },
  { id: 'details', titleKey: 'Guide.SalesOrder.DetailTitle', icon: 'bi-table', label: '明細', color: '#8B5CF6', type: 'Steps',
    items: ['Guide.SalesOrder.Detail1','Guide.SalesOrder.Detail2','Guide.SalesOrder.Detail3','Guide.SalesOrder.Detail4','Guide.SalesOrder.Detail5','Guide.SalesOrder.Detail6'] },
  { id: 'actions', titleKey: 'Guide.SalesOrder.ActionsTitle', icon: 'bi-gear', label: '功能', color: '#D946EF', type: 'FieldList',
    items: [['Guide.SalesOrder.Action.ConvertLabel','Guide.SalesOrder.Action.Convert'],['Guide.SalesOrder.Action.ScheduleLabel','Guide.SalesOrder.Action.Schedule'],['Guide.SalesOrder.Action.InventoryLabel','Guide.SalesOrder.Action.Inventory'],['Guide.SalesOrder.Action.PrintLabel','Guide.SalesOrder.Action.Print'],['Guide.SalesOrder.Action.DraftLabel','Guide.SalesOrder.Action.Draft']] },
  { id: 'approval', titleKey: 'Guide.SalesOrder.ApprovalTitle', icon: 'bi-clipboard-check', label: '審核', color: '#EF4444', type: 'Steps',
    items: ['Guide.SalesOrder.Approval1','Guide.SalesOrder.Approval2','Guide.SalesOrder.Approval3','Guide.SalesOrder.Approval4'] },
  { id: 'tips', titleKey: 'Guide.Tips', icon: 'bi-lightbulb', label: '提示', color: '#06B6D4', type: 'Tips',
    items: ['Guide.SalesOrder.Tip1,Tip','Guide.SalesOrder.Tip2,Tip','Guide.SalesOrder.Tip3,Tip','Guide.SalesOrder.Warning1,Warning','Guide.SalesOrder.Warning2,Warning','Guide.SalesOrder.Warning3,Warning'] },
  { id: 'faq', titleKey: 'Guide.SalesOrder.FaqTitle', icon: 'bi-question-diamond', label: 'FAQ', color: '#6366F1', type: 'FAQ',
    items: [['Guide.SalesOrder.Faq1Q','Guide.SalesOrder.Faq1A'],['Guide.SalesOrder.Faq2Q','Guide.SalesOrder.Faq2A'],['Guide.SalesOrder.Faq3Q','Guide.SalesOrder.Faq3A'],['Guide.SalesOrder.Faq4Q','Guide.SalesOrder.Faq4A'],['Guide.SalesOrder.Faq5Q','Guide.SalesOrder.Faq5A']] },
]};

// ===== Quotation =====
rewrites['QuotationGuide'] = { prefix: 'qt', sections: [
  { id: 'overview', titleKey: 'Guide.Overview', icon: 'bi-info-circle', label: '概述', color: '#3B82F6', type: 'Description',
    items: ['Guide.Quotation.Description'] },
  { id: 'tabs', titleKey: 'Guide.TabDescriptions', icon: 'bi-folder2-open', label: '分頁', color: '#8B5CF6', type: 'FieldList',
    items: [['Tab.QuotationData','Guide.Quotation.Tab.QuotationData'],['Tab.CustomerInfo','Guide.Quotation.Tab.CustomerInfo'],['Tab.QuotationPhotos','Guide.Quotation.Tab.Photos']] },
  { id: 'fields', titleKey: 'Guide.FieldDescriptions', icon: 'bi-input-cursor-text', label: '欄位', color: '#F59E0B', type: 'FieldList',
    items: [['Field.QuotationCode','Guide.Quotation.Field.Code'],['Entity.Customer','Guide.Quotation.Field.Customer'],['Entity.Company','Guide.Quotation.Field.Company'],['Field.SalesPerson','Guide.Quotation.Field.Salesperson'],['Field.QuotationDate','Guide.Quotation.Field.Date'],['Field.ProjectName','Guide.Quotation.Field.ProjectName'],['Field.TaxType','Guide.Quotation.Field.TaxMethod'],['Field.PaymentTerms','Guide.Quotation.Field.PaymentTerms'],['Field.DeliveryTerms','Guide.Quotation.Field.DeliveryTerms'],['Field.Remarks','Guide.Quotation.Field.Remarks']] },
  { id: 'amount', titleKey: 'Guide.SalesOrder.AmountFieldsTitle', icon: 'bi-calculator', label: '金額', color: '#059669', type: 'FieldList',
    items: [['Field.SubtotalBeforeDiscount','Guide.Quotation.Field.Subtotal'],['Field.DiscountAmount','Guide.Quotation.Field.Discount'],['Field.QuotationTaxAmount','Guide.Quotation.Field.TaxAmount'],['Field.TotalAmount','Guide.Quotation.Field.TotalAmount']] },
  { id: 'actions', titleKey: 'Guide.SalesOrder.ActionsTitle', icon: 'bi-gear', label: '功能', color: '#D946EF', type: 'FieldList',
    items: [['Guide.Quotation.Action.ConvertLabel','Guide.Quotation.Action.Convert'],['Guide.Quotation.Action.PrintLabel','Guide.Quotation.Action.Print'],['Guide.Quotation.Action.DraftLabel','Guide.Quotation.Action.Draft']] },
  { id: 'tips', titleKey: 'Guide.Tips', icon: 'bi-lightbulb', label: '提示', color: '#06B6D4', type: 'Tips',
    items: ['Guide.Quotation.Tip1,Tip','Guide.Quotation.Tip2,Tip','Guide.Quotation.Warning1,Warning','Guide.Quotation.Warning2,Warning'] },
  { id: 'faq', titleKey: 'Guide.SalesOrder.FaqTitle', icon: 'bi-question-diamond', label: 'FAQ', color: '#6366F1', type: 'FAQ',
    items: [['Guide.Quotation.Faq1Q','Guide.Quotation.Faq1A'],['Guide.Quotation.Faq2Q','Guide.Quotation.Faq2A'],['Guide.Quotation.Faq3Q','Guide.Quotation.Faq3A']] },
]};

// ===== SalesDelivery =====
rewrites['SalesDeliveryGuide'] = { prefix: 'sd', sections: [
  { id: 'overview', titleKey: 'Guide.Overview', icon: 'bi-info-circle', label: '概述', color: '#3B82F6', type: 'Description', items: ['Guide.SalesDelivery.Description'] },
  { id: 'tabs', titleKey: 'Guide.TabDescriptions', icon: 'bi-folder2-open', label: '分頁', color: '#8B5CF6', type: 'FieldList',
    items: [['Tab.DeliveryData','Guide.SalesDelivery.Tab.DeliveryData'],['Tab.CustomerInfo','Guide.SalesDelivery.Tab.CustomerInfo']] },
  { id: 'fields', titleKey: 'Guide.FieldDescriptions', icon: 'bi-input-cursor-text', label: '欄位', color: '#F59E0B', type: 'FieldList',
    items: [['Field.SalesDeliveryCode','Guide.SalesDelivery.Field.Code'],['Entity.Customer','Guide.SalesDelivery.Field.Customer'],['Field.SalesPerson','Guide.SalesDelivery.Field.Salesperson'],['Field.QuotationCreator','Guide.SalesDelivery.Field.FormCreator'],['Field.SalesDeliveryDate','Guide.SalesDelivery.Field.DeliveryDate'],['Field.DeliveryAddress','Guide.SalesDelivery.Field.Address'],['Field.TaxType','Guide.SalesDelivery.Field.TaxMethod'],['Field.Remarks','Guide.SalesDelivery.Field.Remarks']] },
  { id: 'amount', titleKey: 'Guide.SalesOrder.AmountFieldsTitle', icon: 'bi-calculator', label: '金額', color: '#059669', type: 'FieldList',
    items: [['Field.TotalAmount','Guide.SalesDelivery.Field.TotalAmount'],['Field.DiscountAmount','Guide.SalesDelivery.Field.Discount'],['Field.TaxAmount','Guide.SalesDelivery.Field.TaxAmount'],['Field.TotalAmountWithTax','Guide.SalesDelivery.Field.TotalWithTax']] },
  { id: 'actions', titleKey: 'Guide.SalesOrder.ActionsTitle', icon: 'bi-gear', label: '功能', color: '#D946EF', type: 'FieldList',
    items: [['Guide.SalesDelivery.Action.ReturnLabel','Guide.SalesDelivery.Action.Return'],['Guide.SalesDelivery.Action.SetoffLabel','Guide.SalesDelivery.Action.Setoff'],['Guide.SalesDelivery.Action.PrintLabel','Guide.SalesDelivery.Action.Print']] },
  { id: 'tips', titleKey: 'Guide.Tips', icon: 'bi-lightbulb', label: '提示', color: '#06B6D4', type: 'Tips',
    items: ['Guide.SalesDelivery.Tip1,Tip','Guide.SalesDelivery.Tip2,Tip','Guide.SalesDelivery.Warning1,Warning','Guide.SalesDelivery.Warning2,Warning'] },
  { id: 'faq', titleKey: 'Guide.SalesOrder.FaqTitle', icon: 'bi-question-diamond', label: 'FAQ', color: '#6366F1', type: 'FAQ',
    items: [['Guide.SalesDelivery.Faq1Q','Guide.SalesDelivery.Faq1A'],['Guide.SalesDelivery.Faq2Q','Guide.SalesDelivery.Faq2A'],['Guide.SalesDelivery.Faq3Q','Guide.SalesDelivery.Faq3A']] },
]};

// ===== SalesReturn =====
rewrites['SalesReturnGuide'] = { prefix: 'sr', sections: [
  { id: 'overview', titleKey: 'Guide.Overview', icon: 'bi-info-circle', label: '概述', color: '#3B82F6', type: 'Description', items: ['Guide.SalesReturn.Description'] },
  { id: 'tabs', titleKey: 'Guide.TabDescriptions', icon: 'bi-folder2-open', label: '分頁', color: '#8B5CF6', type: 'FieldList',
    items: [['Tab.ReturnData','Guide.SalesReturn.Tab.ReturnData'],['Tab.CustomerInfo','Guide.SalesReturn.Tab.CustomerInfo']] },
  { id: 'fields', titleKey: 'Guide.FieldDescriptions', icon: 'bi-input-cursor-text', label: '欄位', color: '#F59E0B', type: 'FieldList',
    items: [['Field.SalesReturnCode','Guide.SalesReturn.Field.Code'],['Entity.Customer','Guide.SalesReturn.Field.Customer'],['Field.SalesReturnDate','Guide.SalesReturn.Field.ReturnDate'],['Entity.SalesReturnReason','Guide.SalesReturn.Field.Reason'],['Field.TaxType','Guide.SalesReturn.Field.TaxMethod'],['Field.Remarks','Guide.SalesReturn.Field.Remarks']] },
  { id: 'amount', titleKey: 'Guide.SalesOrder.AmountFieldsTitle', icon: 'bi-calculator', label: '金額', color: '#059669', type: 'FieldList',
    items: [['Field.TotalReturnAmount','Guide.SalesReturn.Field.TotalReturn'],['Field.DiscountAmount','Guide.SalesReturn.Field.Discount'],['Field.ReturnTaxAmount','Guide.SalesReturn.Field.TaxAmount'],['Field.TotalReturnAmountWithTax','Guide.SalesReturn.Field.TotalWithTax']] },
  { id: 'actions', titleKey: 'Guide.SalesOrder.ActionsTitle', icon: 'bi-gear', label: '功能', color: '#D946EF', type: 'FieldList',
    items: [['Guide.SalesReturn.Action.SetoffLabel','Guide.SalesReturn.Action.Setoff']] },
  { id: 'tips', titleKey: 'Guide.Tips', icon: 'bi-lightbulb', label: '提示', color: '#06B6D4', type: 'Tips',
    items: ['Guide.SalesReturn.Tip1,Tip','Guide.SalesReturn.Warning1,Warning','Guide.SalesReturn.Warning2,Warning'] },
  { id: 'faq', titleKey: 'Guide.SalesOrder.FaqTitle', icon: 'bi-question-diamond', label: 'FAQ', color: '#6366F1', type: 'FAQ',
    items: [['Guide.SalesReturn.Faq1Q','Guide.SalesReturn.Faq1A'],['Guide.SalesReturn.Faq2Q','Guide.SalesReturn.Faq2A']] },
]};

// ===== PurchaseOrder =====
rewrites['PurchaseOrderGuide'] = { prefix: 'po', sections: [
  { id: 'overview', titleKey: 'Guide.Overview', icon: 'bi-info-circle', label: '概述', color: '#3B82F6', type: 'Description', items: ['Guide.PurchaseOrder.Description'] },
  { id: 'tabs', titleKey: 'Guide.TabDescriptions', icon: 'bi-folder2-open', label: '分頁', color: '#8B5CF6', type: 'FieldList',
    items: [['Tab.PurchaseOrderData','Guide.PurchaseOrder.Tab.OrderData'],['Tab.SupplierInfo','Guide.PurchaseOrder.Tab.SupplierInfo']] },
  { id: 'fields', titleKey: 'Guide.FieldDescriptions', icon: 'bi-input-cursor-text', label: '欄位', color: '#F59E0B', type: 'FieldList',
    items: [['Field.PurchaseOrderCode','Guide.PurchaseOrder.Field.Code'],['Entity.Supplier','Guide.PurchaseOrder.Field.Supplier'],['Field.PurchasingCompany','Guide.PurchaseOrder.Field.Company'],['Field.Purchaser','Guide.PurchaseOrder.Field.Personnel'],['Field.PurchaseDate','Guide.PurchaseOrder.Field.OrderDate'],['Field.DeliveryDate','Guide.PurchaseOrder.Field.ExpectedDate'],['Field.TaxType','Guide.PurchaseOrder.Field.TaxMethod'],['Field.Remarks','Guide.PurchaseOrder.Field.Remarks']] },
  { id: 'amount', titleKey: 'Guide.SalesOrder.AmountFieldsTitle', icon: 'bi-calculator', label: '金額', color: '#059669', type: 'FieldList',
    items: [['Field.TotalAmount','Guide.PurchaseOrder.Field.TotalAmount'],['Field.PurchaseTaxAmount','Guide.PurchaseOrder.Field.TaxAmount'],['Field.PurchaseTotalAmountIncludingTax','Guide.PurchaseOrder.Field.TotalWithTax']] },
  { id: 'actions', titleKey: 'Guide.SalesOrder.ActionsTitle', icon: 'bi-gear', label: '功能', color: '#D946EF', type: 'FieldList',
    items: [['Guide.PurchaseOrder.Action.ConvertLabel','Guide.PurchaseOrder.Action.Convert'],['Guide.PurchaseOrder.Action.CopyMsgLabel','Guide.PurchaseOrder.Action.CopyMsg'],['Guide.PurchaseOrder.Action.PrintLabel','Guide.PurchaseOrder.Action.Print']] },
  { id: 'tips', titleKey: 'Guide.Tips', icon: 'bi-lightbulb', label: '提示', color: '#06B6D4', type: 'Tips',
    items: ['Guide.PurchaseOrder.Tip1,Tip','Guide.PurchaseOrder.Tip2,Tip','Guide.PurchaseOrder.Warning1,Warning','Guide.PurchaseOrder.Warning2,Warning'] },
  { id: 'faq', titleKey: 'Guide.SalesOrder.FaqTitle', icon: 'bi-question-diamond', label: 'FAQ', color: '#6366F1', type: 'FAQ',
    items: [['Guide.PurchaseOrder.Faq1Q','Guide.PurchaseOrder.Faq1A'],['Guide.PurchaseOrder.Faq2Q','Guide.PurchaseOrder.Faq2A'],['Guide.PurchaseOrder.Faq3Q','Guide.PurchaseOrder.Faq3A']] },
]};

// ===== PurchaseReceiving =====
rewrites['PurchaseReceivingGuide'] = { prefix: 'pr', sections: [
  { id: 'overview', titleKey: 'Guide.Overview', icon: 'bi-info-circle', label: '概述', color: '#3B82F6', type: 'Description', items: ['Guide.PurchaseReceiving.Description'] },
  { id: 'tabs', titleKey: 'Guide.TabDescriptions', icon: 'bi-folder2-open', label: '分頁', color: '#8B5CF6', type: 'FieldList',
    items: [['Tab.ReceivingData','Guide.PurchaseReceiving.Tab.ReceivingData'],['Tab.SupplierInfo','Guide.PurchaseReceiving.Tab.SupplierInfo']] },
  { id: 'fields', titleKey: 'Guide.FieldDescriptions', icon: 'bi-input-cursor-text', label: '欄位', color: '#F59E0B', type: 'FieldList',
    items: [['Field.PurchaseReceivingCode','Guide.PurchaseReceiving.Field.Code'],['Entity.Supplier','Guide.PurchaseReceiving.Field.Supplier'],['Field.ReceiptDate','Guide.PurchaseReceiving.Field.ReceiptDate'],['Field.TaxType','Guide.PurchaseReceiving.Field.TaxMethod'],['Field.BatchNumber','Guide.PurchaseReceiving.Field.BatchNumber'],['Field.Remarks','Guide.PurchaseReceiving.Field.Remarks']] },
  { id: 'amount', titleKey: 'Guide.SalesOrder.AmountFieldsTitle', icon: 'bi-calculator', label: '金額', color: '#059669', type: 'FieldList',
    items: [['Field.TotalAmount','Guide.PurchaseReceiving.Field.TotalAmount'],['Field.PurchaseReceivingTaxAmount','Guide.PurchaseReceiving.Field.TaxAmount'],['Field.PurchaseReceivingTotalAmountIncludingTax','Guide.PurchaseReceiving.Field.TotalWithTax']] },
  { id: 'details', titleKey: 'Guide.SalesOrder.DetailTitle', icon: 'bi-table', label: '明細', color: '#8B5CF6', type: 'Steps',
    items: ['Guide.PurchaseReceiving.Detail1','Guide.PurchaseReceiving.Detail2','Guide.PurchaseReceiving.Detail3','Guide.PurchaseReceiving.Detail4'] },
  { id: 'actions', titleKey: 'Guide.SalesOrder.ActionsTitle', icon: 'bi-gear', label: '功能', color: '#D946EF', type: 'FieldList',
    items: [['Guide.PurchaseReceiving.Action.ReturnLabel','Guide.PurchaseReceiving.Action.Return'],['Guide.PurchaseReceiving.Action.SetoffLabel','Guide.PurchaseReceiving.Action.Setoff']] },
  { id: 'tips', titleKey: 'Guide.Tips', icon: 'bi-lightbulb', label: '提示', color: '#06B6D4', type: 'Tips',
    items: ['Guide.PurchaseReceiving.Tip1,Tip','Guide.PurchaseReceiving.Tip2,Tip','Guide.PurchaseReceiving.Warning1,Warning'] },
  { id: 'faq', titleKey: 'Guide.SalesOrder.FaqTitle', icon: 'bi-question-diamond', label: 'FAQ', color: '#6366F1', type: 'FAQ',
    items: [['Guide.PurchaseReceiving.Faq1Q','Guide.PurchaseReceiving.Faq1A'],['Guide.PurchaseReceiving.Faq2Q','Guide.PurchaseReceiving.Faq2A']] },
]};

// ===== PurchaseReturn =====
rewrites['PurchaseReturnGuide'] = { prefix: 'prt', sections: [
  { id: 'overview', titleKey: 'Guide.Overview', icon: 'bi-info-circle', label: '概述', color: '#3B82F6', type: 'Description', items: ['Guide.PurchaseReturn.Description'] },
  { id: 'tabs', titleKey: 'Guide.TabDescriptions', icon: 'bi-folder2-open', label: '分頁', color: '#8B5CF6', type: 'FieldList',
    items: [['Tab.ReturnData','Guide.PurchaseReturn.Tab.ReturnData'],['Tab.SupplierInfo','Guide.PurchaseReturn.Tab.SupplierInfo']] },
  { id: 'fields', titleKey: 'Guide.FieldDescriptions', icon: 'bi-input-cursor-text', label: '欄位', color: '#F59E0B', type: 'FieldList',
    items: [['Field.PurchaseReturnCode','Guide.PurchaseReturn.Field.Code'],['Entity.Supplier','Guide.PurchaseReturn.Field.Supplier'],['Field.PurchaseReturnDate','Guide.PurchaseReturn.Field.ReturnDate'],['Entity.PurchaseReturnReason','Guide.PurchaseReturn.Field.Reason'],['Field.TaxType','Guide.PurchaseReturn.Field.TaxMethod'],['Field.Remarks','Guide.PurchaseReturn.Field.Remarks']] },
  { id: 'amount', titleKey: 'Guide.SalesOrder.AmountFieldsTitle', icon: 'bi-calculator', label: '金額', color: '#059669', type: 'FieldList',
    items: [['Field.TotalReturnAmount','Guide.PurchaseReturn.Field.TotalReturn'],['Field.ReturnTaxAmount','Guide.PurchaseReturn.Field.TaxAmount'],['Field.TotalReturnAmountWithTax','Guide.PurchaseReturn.Field.TotalWithTax']] },
  { id: 'actions', titleKey: 'Guide.SalesOrder.ActionsTitle', icon: 'bi-gear', label: '功能', color: '#D946EF', type: 'FieldList',
    items: [['Guide.PurchaseReturn.Action.SetoffLabel','Guide.PurchaseReturn.Action.Setoff']] },
  { id: 'tips', titleKey: 'Guide.Tips', icon: 'bi-lightbulb', label: '提示', color: '#06B6D4', type: 'Tips',
    items: ['Guide.PurchaseReturn.Tip1,Tip','Guide.PurchaseReturn.Warning1,Warning','Guide.PurchaseReturn.Warning2,Warning'] },
  { id: 'faq', titleKey: 'Guide.SalesOrder.FaqTitle', icon: 'bi-question-diamond', label: 'FAQ', color: '#6366F1', type: 'FAQ',
    items: [['Guide.PurchaseReturn.Faq1Q','Guide.PurchaseReturn.Faq1A'],['Guide.PurchaseReturn.Faq2Q','Guide.PurchaseReturn.Faq2A']] },
]};

// ===== Customer =====
rewrites['CustomerGuide'] = { prefix: 'cust', sections: [
  { id: 'overview', titleKey: 'Guide.Overview', icon: 'bi-info-circle', label: '概述', color: '#3B82F6', type: 'Description', items: ['Guide.Customer.Description'] },
  { id: 'tabs', titleKey: 'Guide.TabDescriptions', icon: 'bi-folder2-open', label: '分頁', color: '#8B5CF6', type: 'FieldList',
    items: [['Tab.CustomerData','Guide.Customer.Tab.CustomerData'],['Tab.VehicleInfo','Guide.Customer.Tab.VehicleInfo'],['Tab.AccountingInfo','Guide.Customer.Tab.Accounting'],['Tab.BankAccounts','Guide.Customer.Tab.BankAccounts'],['Tab.CustomerItems','Guide.Customer.Tab.Items'],['Tab.VisitRecords','Guide.Customer.Tab.Visits'],['Tab.ComplaintRecords','Guide.Customer.Tab.Complaints'],['Tab.TransactionRecords','Guide.Customer.Tab.Transactions'],['Tab.BusinessCards','Guide.Customer.Tab.BusinessCards']] },
  { id: 'fields', titleKey: 'Guide.FieldDescriptions', icon: 'bi-input-cursor-text', label: '欄位', color: '#F59E0B', type: 'FieldList',
    items: [['Field.CustomerCode','Guide.Customer.Field.Code'],['Field.CompanyName','Guide.Customer.Field.Name'],['Field.CustomerType','Guide.Customer.Field.Type'],['Field.ContactPerson','Guide.Customer.Field.Contact'],['Field.ContactPhone','Guide.Customer.Field.Phone'],['Field.Email','Guide.Customer.Field.Email'],['Field.ContactAddress','Guide.Customer.Field.Address'],['Field.TaxNumber','Guide.Customer.Field.TaxNumber'],['Field.SalesManager','Guide.Customer.Field.SalesManager'],['Field.PaymentTerms','Guide.Customer.Field.PaymentTerms'],['Field.Remarks','Guide.Customer.Field.Remarks']] },
  { id: 'tips', titleKey: 'Guide.Tips', icon: 'bi-lightbulb', label: '提示', color: '#06B6D4', type: 'Tips',
    items: ['Guide.Customer.Tip1,Tip','Guide.Customer.Warning1,Warning'] },
  { id: 'faq', titleKey: 'Guide.SalesOrder.FaqTitle', icon: 'bi-question-diamond', label: 'FAQ', color: '#6366F1', type: 'FAQ',
    items: [['Guide.Customer.Faq1Q','Guide.Customer.Faq1A']] },
]};

// ===== Supplier =====
rewrites['SupplierGuide'] = { prefix: 'supp', sections: [
  { id: 'overview', titleKey: 'Guide.Overview', icon: 'bi-info-circle', label: '概述', color: '#3B82F6', type: 'Description', items: ['Guide.Supplier.Description'] },
  { id: 'tabs', titleKey: 'Guide.TabDescriptions', icon: 'bi-folder2-open', label: '分頁', color: '#8B5CF6', type: 'FieldList',
    items: [['Tab.SupplierData','Guide.Supplier.Tab.SupplierData'],['Tab.VehicleInfo','Guide.Supplier.Tab.VehicleInfo'],['Tab.AccountingInfo','Guide.Supplier.Tab.Accounting'],['Tab.BankAccounts','Guide.Supplier.Tab.BankAccounts'],['Tab.SupplierItems','Guide.Supplier.Tab.Items'],['Tab.VisitRecords','Guide.Supplier.Tab.Visits'],['Tab.TransactionRecords','Guide.Supplier.Tab.Transactions'],['Tab.BusinessCards','Guide.Supplier.Tab.BusinessCards']] },
  { id: 'fields', titleKey: 'Guide.FieldDescriptions', icon: 'bi-input-cursor-text', label: '欄位', color: '#F59E0B', type: 'FieldList',
    items: [['Field.SupplierCode','Guide.Supplier.Field.Code'],['Field.SupplierName','Guide.Supplier.Field.Name'],['Field.ContactPerson','Guide.Supplier.Field.Contact'],['Field.ContactPhone','Guide.Supplier.Field.Phone'],['Field.Email','Guide.Supplier.Field.Email'],['Field.ContactAddress','Guide.Supplier.Field.Address'],['Field.TaxNumber','Guide.Supplier.Field.TaxNumber'],['Field.PaymentTerms','Guide.Supplier.Field.PaymentTerms'],['Field.Remarks','Guide.Supplier.Field.Remarks']] },
  { id: 'tips', titleKey: 'Guide.Tips', icon: 'bi-lightbulb', label: '提示', color: '#06B6D4', type: 'Tips',
    items: ['Guide.Supplier.Tip1,Tip'] },
  { id: 'faq', titleKey: 'Guide.SalesOrder.FaqTitle', icon: 'bi-question-diamond', label: 'FAQ', color: '#6366F1', type: 'FAQ',
    items: [['Guide.Supplier.Faq1Q','Guide.Supplier.Faq1A']] },
]};

// ===== Item =====
rewrites['ItemGuide'] = { prefix: 'item', sections: [
  { id: 'overview', titleKey: 'Guide.Overview', icon: 'bi-info-circle', label: '概述', color: '#3B82F6', type: 'Description', items: ['Guide.Item.Description'] },
  { id: 'tabs', titleKey: 'Guide.TabDescriptions', icon: 'bi-folder2-open', label: '分頁', color: '#8B5CF6', type: 'FieldList',
    items: [['Tab.ItemData','Guide.Item.Tab.ItemData'],['Tab.AccountingInfo','Guide.Item.Tab.Accounting'],['Tab.ItemPhotos','Guide.Item.Tab.Photos']] },
  { id: 'fields', titleKey: 'Guide.FieldDescriptions', icon: 'bi-input-cursor-text', label: '欄位', color: '#F59E0B', type: 'FieldList',
    items: [['Field.ItemCode','Guide.Item.Field.Code'],['Field.Barcode','Guide.Item.Field.Barcode'],['Field.ItemName','Guide.Item.Field.Name'],['Field.ItemCategory','Guide.Item.Field.Category'],['Field.Unit','Guide.Item.Field.Unit'],['Entity.Size','Guide.Item.Field.Size'],['Field.Type','Guide.Item.Field.Type'],['Field.TaxRate','Guide.Item.Field.TaxRate'],['Field.Specification','Guide.Item.Field.Spec'],['Field.Remarks','Guide.Item.Field.Remarks']] },
  { id: 'actions', titleKey: 'Guide.SalesOrder.ActionsTitle', icon: 'bi-gear', label: '功能', color: '#D946EF', type: 'FieldList',
    items: [['Guide.Item.Action.PrintLabel','Guide.Item.Action.Print']] },
  { id: 'tips', titleKey: 'Guide.Tips', icon: 'bi-lightbulb', label: '提示', color: '#06B6D4', type: 'Tips',
    items: ['Guide.Item.Tip1,Tip','Guide.Item.Warning1,Warning'] },
  { id: 'faq', titleKey: 'Guide.SalesOrder.FaqTitle', icon: 'bi-question-diamond', label: 'FAQ', color: '#6366F1', type: 'FAQ',
    items: [['Guide.Item.Faq1Q','Guide.Item.Faq1A'],['Guide.Item.Faq2Q','Guide.Item.Faq2A']] },
]};

// ===== Company =====
rewrites['CompanyGuide'] = { prefix: 'comp', sections: [
  { id: 'overview', titleKey: 'Guide.Overview', icon: 'bi-info-circle', label: '概述', color: '#3B82F6', type: 'Description', items: ['Guide.Company.Description'] },
  { id: 'tabs', titleKey: 'Guide.TabDescriptions', icon: 'bi-folder2-open', label: '分頁', color: '#8B5CF6', type: 'FieldList',
    items: [['Tab.CompanyData','Guide.Company.Tab.CompanyData'],['Tab.CompanyLogo','Guide.Company.Tab.Logo'],['Tab.BankAccounts','Guide.Company.Tab.BankAccounts']] },
  { id: 'fields', titleKey: 'Guide.FieldDescriptions', icon: 'bi-input-cursor-text', label: '欄位', color: '#F59E0B', type: 'FieldList',
    items: [['Field.Code','Guide.Company.Field.Code'],['Field.CompanyName','Guide.Company.Field.Name'],['Field.ShortName','Guide.Company.Field.ShortName'],['Field.TaxNumber','Guide.Company.Field.TaxId'],['Field.ResponsiblePerson','Guide.Company.Field.Representative'],['Field.CompanyAddress','Guide.Company.Field.Address'],['Field.Phone','Guide.Company.Field.Phone'],['Field.Email','Guide.Company.Field.Email'],['Field.Website','Guide.Company.Field.Website'],['Field.IsDefaultCompany','Guide.Company.Field.IsDefault']] },
  { id: 'tips', titleKey: 'Guide.Tips', icon: 'bi-lightbulb', label: '提示', color: '#06B6D4', type: 'Tips',
    items: ['Guide.Company.Tip1,Tip'] },
]};

// ===== CrmLead =====
rewrites['CrmLeadGuide'] = { prefix: 'crm', sections: [
  { id: 'overview', titleKey: 'Guide.Overview', icon: 'bi-info-circle', label: '概述', color: '#3B82F6', type: 'Description', items: ['Guide.CrmLead.Description'] },
  { id: 'tabs', titleKey: 'Guide.TabDescriptions', icon: 'bi-folder2-open', label: '分頁', color: '#8B5CF6', type: 'FieldList',
    items: [['Tab.LeadInfo','Guide.CrmLead.Tab.LeadInfo'],['Tab.FollowUpRecords','Guide.CrmLead.Tab.FollowUp']] },
  { id: 'fields', titleKey: 'Guide.FieldDescriptions', icon: 'bi-input-cursor-text', label: '欄位', color: '#F59E0B', type: 'FieldList',
    items: [['Field.CompanyName','Guide.CrmLead.Field.CompanyName'],['Field.ContactPerson','Guide.CrmLead.Field.Contact'],['Field.ContactPhone','Guide.CrmLead.Field.Phone'],['Field.Email','Guide.CrmLead.Field.Email'],['Field.Industry','Guide.CrmLead.Field.Industry'],['Field.LeadSource','Guide.CrmLead.Field.Source'],['Field.LeadStage','Guide.CrmLead.Field.Stage'],['Field.AssignedEmployee','Guide.CrmLead.Field.Assigned'],['Field.EstimatedValue','Guide.CrmLead.Field.Value'],['Field.Remarks','Guide.CrmLead.Field.Remarks']] },
  { id: 'actions', titleKey: 'Guide.SalesOrder.ActionsTitle', icon: 'bi-gear', label: '功能', color: '#D946EF', type: 'FieldList',
    items: [['Guide.CrmLead.Action.ConvertLabel','Guide.CrmLead.Action.Convert']] },
  { id: 'tips', titleKey: 'Guide.Tips', icon: 'bi-lightbulb', label: '提示', color: '#06B6D4', type: 'Tips',
    items: ['Guide.CrmLead.Tip1,Tip','Guide.CrmLead.Warning1,Warning'] },
]};

// ==================== Write all .cs files ====================
let count = 0;
for (const [cls, def] of Object.entries(rewrites)) {
  const fp = path.join(guideDir, `${cls}.cs`);
  fs.writeFileSync(fp, genCs(cls, def.prefix, def.sections), 'utf8');
  count++;
}
console.log(`Rewrote ${count} Guide .cs files`);

// ==================== Add new resx keys ====================
function escXml(s) { return s.replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;'); }
function insertKeys(file, keys) {
  let content = fs.readFileSync(file, 'utf8');
  const marker = 'name="Modal.DefaultTitle"';
  const idx = content.indexOf(marker);
  if (idx === -1) { console.log("ERROR: " + path.basename(file)); return; }
  const lineStart = content.lastIndexOf('\n', idx) + 1;
  const lines = [];
  for (const [key, val] of Object.entries(keys)) {
    if (content.includes(`name="${key}"`)) continue;
    lines.push(`  <data name="${key}" xml:space="preserve"><value>${escXml(val)}</value></data>`);
  }
  if (lines.length === 0) { console.log("No new keys for " + path.basename(file)); return; }
  content = content.slice(0, lineStart) + lines.join('\n') + '\n' + content.slice(lineStart);
  fs.writeFileSync(file, content, 'utf8');
  console.log(`Added ${lines.length} keys to ${path.basename(file)}`);
}

const zhTW = {
  // Tab description keys - SalesOrder
  "Guide.SalesOrder.Tab.OrderData":"填寫訂單基本資料（客戶、日期、稅別等）與管理訂單明細品項，包含數量、單價、折扣的設定",
  "Guide.SalesOrder.Tab.CustomerInfo":"查看所選客戶的完整聯絡資料、地址與相關資訊，不需離開訂單頁面",
  "Guide.SalesOrder.Tab.Photos":"上傳與管理訂單相關的圖片附件",
  // Tab - Quotation
  "Guide.Quotation.Tab.QuotationData":"填寫報價基本資料與管理報價明細品項，包含數量、單價、折扣的設定",
  "Guide.Quotation.Tab.CustomerInfo":"查看所選客戶的完整聯絡資料，不需離開報價頁面",
  "Guide.Quotation.Tab.Photos":"上傳與管理報價相關的圖片附件",
  // Tab - SalesDelivery
  "Guide.SalesDelivery.Tab.DeliveryData":"填寫出貨基本資料（客戶、日期、地址等）與管理出貨明細品項",
  "Guide.SalesDelivery.Tab.CustomerInfo":"查看所選客戶的聯絡方式與地址，方便確認出貨資訊",
  "Guide.SalesDelivery.Field.FormCreator":"建立此出貨單的經辦人員",
  // Tab - SalesReturn
  "Guide.SalesReturn.Tab.ReturnData":"填寫退回基本資料與管理退回明細品項，可篩選出貨單帶入可退品項",
  "Guide.SalesReturn.Tab.CustomerInfo":"查看退貨客戶的聯絡資料",
  // Tab - PurchaseOrder
  "Guide.PurchaseOrder.Tab.OrderData":"填寫採購基本資料（廠商、日期、稅別等）與管理採購明細品項",
  "Guide.PurchaseOrder.Tab.SupplierInfo":"查看所選廠商的聯絡方式、付款條件等資訊",
  // Tab - PurchaseReceiving
  "Guide.PurchaseReceiving.Tab.ReceivingData":"填寫進貨基本資料與管理進貨明細，可從採購單智慧載入品項並指定入庫倉庫",
  "Guide.PurchaseReceiving.Tab.SupplierInfo":"查看供貨廠商的聯絡方式與付款資訊",
  // Tab - PurchaseReturn
  "Guide.PurchaseReturn.Tab.ReturnData":"填寫退回基本資料與管理退回明細，可篩選進貨單帶入可退品項",
  "Guide.PurchaseReturn.Tab.SupplierInfo":"查看退貨廠商的聯絡資料",
  // Tab - Customer
  "Guide.Customer.Tab.CustomerData":"填寫客戶基本資料、聯絡方式、付款條件等核心資訊",
  "Guide.Customer.Tab.VehicleInfo":"管理與此客戶關聯的車輛，可新增、編輯或解除車輛關聯",
  "Guide.Customer.Tab.Accounting":"設定此客戶的會計科目對應（應收帳款、預收款等）",
  "Guide.Customer.Tab.BankAccounts":"維護客戶的銀行帳戶資訊，沖款收款時可選用",
  "Guide.Customer.Tab.Items":"查看此客戶專屬的品項與報價記錄",
  "Guide.Customer.Tab.Visits":"記錄業務員拜訪此客戶的歷史，包含拜訪目的、內容與後續追蹤",
  "Guide.Customer.Tab.Complaints":"管理此客戶的投訴案件，追蹤處理進度與解決方案",
  "Guide.Customer.Tab.Transactions":"查看與此客戶的歷史交易記錄（訂單、出貨、退貨等）",
  "Guide.Customer.Tab.BusinessCards":"管理此客戶的名片資料",
  "Guide.Customer.Field.TaxNumber":"統一編號，用於開立發票",
  "Guide.Customer.Field.SalesManager":"負責此客戶的業務員",
  "Guide.Customer.Field.PaymentTerms":"約定的付款方式與條件",
  // Tab - Supplier
  "Guide.Supplier.Tab.SupplierData":"填寫廠商基本資料、聯絡方式、付款條件等核心資訊",
  "Guide.Supplier.Tab.VehicleInfo":"管理與此廠商關聯的車輛",
  "Guide.Supplier.Tab.Accounting":"設定此廠商的會計科目對應（應付帳款、預付款等）",
  "Guide.Supplier.Tab.BankAccounts":"維護廠商的銀行帳戶資訊，沖款付款時可選用",
  "Guide.Supplier.Tab.Items":"查看此廠商供應的品項與報價記錄",
  "Guide.Supplier.Tab.Visits":"記錄拜訪此廠商的歷史記錄",
  "Guide.Supplier.Tab.Transactions":"查看與此廠商的歷史交易記錄（採購、進貨、退貨等）",
  "Guide.Supplier.Tab.BusinessCards":"管理此廠商的名片資料",
  "Guide.Supplier.Field.TaxNumber":"統一編號",
  "Guide.Supplier.Field.PaymentTerms":"約定的付款方式與條件",
  // Tab - Item
  "Guide.Item.Tab.ItemData":"填寫品項基本資料、分類、單位、規格與供應商等核心資訊",
  "Guide.Item.Tab.Accounting":"設定此品項的會計科目對應（銷貨收入、銷貨成本等）",
  "Guide.Item.Tab.Photos":"上傳與管理品項的產品照片",
  "Guide.Item.Field.Barcode":"品項條碼，用於掃碼快速查詢",
  "Guide.Item.Field.Type":"品項類型（一般品項 / BOM 組合品項）",
  "Guide.Item.Field.TaxRate":"預設稅率，建立單據時自動帶入",
  // Tab - Company
  "Guide.Company.Tab.CompanyData":"填寫公司基本資料、統一編號、地址、聯絡方式等核心資訊",
  "Guide.Company.Tab.Logo":"上傳與管理公司標誌，Logo 會顯示在列印的報表與單據上",
  "Guide.Company.Tab.BankAccounts":"維護公司的銀行帳戶資訊，用於財務管理與對帳",
  // Tab - CrmLead
  "Guide.CrmLead.Tab.LeadInfo":"填寫潛在客戶的基本資料、聯絡方式、商機預估與開發階段",
  "Guide.CrmLead.Tab.FollowUp":"記錄每次跟進的方式、內容與下次聯繫計畫，追蹤開發進度",
  "Guide.CrmLead.Field.Source":"客戶來源管道（如：網路、轉介、展覽等）",
  "Guide.CrmLead.Field.Stage":"開發階段（如：初步接觸、需求確認、報價中、成交等）",
  "Guide.CrmLead.Field.Assigned":"負責跟進此潛在客戶的業務員",
  "Guide.CrmLead.Field.Value":"預估此商機的金額",
  // Missing Field keys
  "Field.PurchasingCompany":"採購公司","Field.Purchaser":"採購人員","Field.PurchaseDate":"採購日期",
  "Field.SalesDeliveryDate":"出貨日期","Field.CompanyAddress":"公司地址","Field.CompanyDescription":"公司說明",
  "Field.ContactPhone":"聯絡電話","Field.MobilePhone":"手機號碼","Field.ContactAddress":"聯絡地址",
  "Field.ShippingAddress":"送貨地址","Field.CollectionDay":"收款日","Field.CreditRating":"信用評等",
  "Field.DefaultDiscountRate":"預設折扣率","Field.BillingAddress":"帳單地址",
  "Field.CurrentPayable":"目前應付","Field.PaymentDay":"付款日",
  "Field.SupplierCode":"廠商代碼","Field.SupplierName":"廠商名稱","Field.SupplierStatus":"廠商狀態","Field.SupplierType":"廠商類型",
  "Field.ItemCode":"品項代碼","Field.Barcode":"條碼","Field.ItemName":"品項名稱","Field.Type":"類型",
  "Field.PurchaseUnit":"採購單位","Field.ProductionUnit":"生產單位","Field.Specification":"規格",
  "Field.CompanyName":"公司名稱","Field.EstablishedDate":"成立日期","Field.ResponsiblePerson":"負責人",
  "Field.Industry":"產業類別","Field.LeadSource":"來源","Field.LeadStage":"開發階段",
  "Field.AssignedEmployee":"負責業務","Field.EstimatedValue":"預估金額",
  "Field.SalesManager":"負責業務員",
};

const enUS = {
  "Guide.SalesOrder.Tab.OrderData":"Enter order basics (customer, date, tax) and manage order line items with quantities, prices, and discounts",
  "Guide.SalesOrder.Tab.CustomerInfo":"View the selected customer's full contact and address info without leaving the order page",
  "Guide.SalesOrder.Tab.Photos":"Upload and manage order-related image attachments",
  "Guide.Quotation.Tab.QuotationData":"Enter quotation basics and manage line items with quantities, prices, and discounts",
  "Guide.Quotation.Tab.CustomerInfo":"View customer contact details without leaving the quotation page",
  "Guide.Quotation.Tab.Photos":"Upload and manage quotation-related images",
  "Guide.SalesDelivery.Tab.DeliveryData":"Enter delivery basics (customer, date, address) and manage delivery line items",
  "Guide.SalesDelivery.Tab.CustomerInfo":"View customer contact and address info for delivery confirmation",
  "Guide.SalesDelivery.Field.FormCreator":"The person who created this delivery note",
  "Guide.SalesReturn.Tab.ReturnData":"Enter return basics and manage return items; filter from deliveries to import returnable items",
  "Guide.SalesReturn.Tab.CustomerInfo":"View return customer's contact details",
  "Guide.PurchaseOrder.Tab.OrderData":"Enter PO basics (supplier, date, tax) and manage purchase line items",
  "Guide.PurchaseOrder.Tab.SupplierInfo":"View selected supplier's contact and payment info",
  "Guide.PurchaseReceiving.Tab.ReceivingData":"Enter receiving basics and manage details; smart-load items from POs and assign warehouses",
  "Guide.PurchaseReceiving.Tab.SupplierInfo":"View supplier's contact and payment info",
  "Guide.PurchaseReturn.Tab.ReturnData":"Enter return basics and manage return items; filter from receiving to import returnable items",
  "Guide.PurchaseReturn.Tab.SupplierInfo":"View return supplier's contact details",
  "Guide.Customer.Tab.CustomerData":"Enter customer basics, contacts, payment terms, and other core info",
  "Guide.Customer.Tab.VehicleInfo":"Manage vehicles linked to this customer",
  "Guide.Customer.Tab.Accounting":"Set accounting mappings (receivables, prepayments, etc.)",
  "Guide.Customer.Tab.BankAccounts":"Maintain customer bank accounts for payment settlement",
  "Guide.Customer.Tab.Items":"View customer-specific items and quotation records",
  "Guide.Customer.Tab.Visits":"Record salesperson visit history with purposes and follow-ups",
  "Guide.Customer.Tab.Complaints":"Manage complaint cases with resolution tracking",
  "Guide.Customer.Tab.Transactions":"View transaction history (orders, deliveries, returns, etc.)",
  "Guide.Customer.Tab.BusinessCards":"Manage customer business cards",
  "Guide.Customer.Field.TaxNumber":"Tax ID for invoicing",
  "Guide.Customer.Field.SalesManager":"Salesperson responsible for this customer",
  "Guide.Customer.Field.PaymentTerms":"Agreed payment terms and conditions",
  "Guide.Supplier.Tab.SupplierData":"Enter supplier basics, contacts, payment terms, and other core info",
  "Guide.Supplier.Tab.VehicleInfo":"Manage vehicles linked to this supplier",
  "Guide.Supplier.Tab.Accounting":"Set accounting mappings (payables, prepayments, etc.)",
  "Guide.Supplier.Tab.BankAccounts":"Maintain supplier bank accounts for payment processing",
  "Guide.Supplier.Tab.Items":"View items supplied with pricing records",
  "Guide.Supplier.Tab.Visits":"Record visit history with this supplier",
  "Guide.Supplier.Tab.Transactions":"View transaction history (POs, receivings, returns, etc.)",
  "Guide.Supplier.Tab.BusinessCards":"Manage supplier business cards",
  "Guide.Supplier.Field.TaxNumber":"Tax ID number",
  "Guide.Supplier.Field.PaymentTerms":"Agreed payment terms",
  "Guide.Item.Tab.ItemData":"Enter item basics, category, unit, specs, and supplier info",
  "Guide.Item.Tab.Accounting":"Set accounting mappings (sales revenue, COGS, etc.)",
  "Guide.Item.Tab.Photos":"Upload and manage product photos",
  "Guide.Item.Field.Barcode":"Item barcode for quick scanning",
  "Guide.Item.Field.Type":"Item type (standard / BOM composite)",
  "Guide.Item.Field.TaxRate":"Default tax rate, auto-filled when creating documents",
  "Guide.Company.Tab.CompanyData":"Enter company basics, tax ID, address, and contact info",
  "Guide.Company.Tab.Logo":"Upload company logo; appears on printed reports and documents",
  "Guide.Company.Tab.BankAccounts":"Maintain company bank accounts for financial management",
  "Guide.CrmLead.Tab.LeadInfo":"Enter lead basics, contacts, estimated value, and development stage",
  "Guide.CrmLead.Tab.FollowUp":"Record each follow-up method, content, and next contact plan",
  "Guide.CrmLead.Field.Source":"Lead source channel (web, referral, exhibition, etc.)",
  "Guide.CrmLead.Field.Stage":"Development stage (initial contact, needs confirmed, quoting, closed, etc.)",
  "Guide.CrmLead.Field.Assigned":"Salesperson responsible for this lead",
  "Guide.CrmLead.Field.Value":"Estimated opportunity value",
  "Field.PurchasingCompany":"Purchasing Company","Field.Purchaser":"Purchaser","Field.PurchaseDate":"Purchase Date",
  "Field.SalesDeliveryDate":"Delivery Date","Field.CompanyAddress":"Company Address","Field.CompanyDescription":"Company Description",
  "Field.ContactPhone":"Contact Phone","Field.MobilePhone":"Mobile Phone","Field.ContactAddress":"Contact Address",
  "Field.ShippingAddress":"Shipping Address","Field.CollectionDay":"Collection Day","Field.CreditRating":"Credit Rating",
  "Field.DefaultDiscountRate":"Default Discount Rate","Field.BillingAddress":"Billing Address",
  "Field.CurrentPayable":"Current Payable","Field.PaymentDay":"Payment Day",
  "Field.SupplierCode":"Supplier Code","Field.SupplierName":"Supplier Name","Field.SupplierStatus":"Supplier Status","Field.SupplierType":"Supplier Type",
  "Field.ItemCode":"Item Code","Field.Barcode":"Barcode","Field.ItemName":"Item Name","Field.Type":"Type",
  "Field.PurchaseUnit":"Purchase Unit","Field.ProductionUnit":"Production Unit","Field.Specification":"Specification",
  "Field.CompanyName":"Company Name","Field.EstablishedDate":"Established Date","Field.ResponsiblePerson":"Representative",
  "Field.Industry":"Industry","Field.LeadSource":"Lead Source","Field.LeadStage":"Lead Stage",
  "Field.AssignedEmployee":"Assigned Employee","Field.EstimatedValue":"Estimated Value",
  "Field.SalesManager":"Sales Manager",
};

insertKeys(path.join(resxDir, "SharedResource.resx"), zhTW);
insertKeys(path.join(resxDir, "SharedResource.en-US.resx"), enUS);
insertKeys(path.join(resxDir, "SharedResource.ja-JP.resx"), enUS);
insertKeys(path.join(resxDir, "SharedResource.zh-CN.resx"), enUS);
insertKeys(path.join(resxDir, "SharedResource.fil.resx"), enUS);
console.log("All done!");
