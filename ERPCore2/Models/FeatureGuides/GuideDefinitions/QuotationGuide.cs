using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

public static class QuotationGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-qt-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items =
                {
                    new("Guide.Quotation.Description")
                }
            },

            new GuideSection
            {
                Id = "guide-qt-tabs",
                TitleKey = "Guide.TabDescriptions",
                Icon = "bi-folder2-open",
                BookmarkLabel = "分頁",
                BookmarkColor = "#8B5CF6",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Tab.QuotationData", "Guide.Quotation.Tab.QuotationData"),
                    new("Tab.CustomerInfo", "Guide.Quotation.Tab.CustomerInfo"),
                    new("Tab.QuotationPhotos", "Guide.Quotation.Tab.Photos")
                }
            },

            new GuideSection
            {
                Id = "guide-qt-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.QuotationCode", "Guide.Quotation.Field.Code"),
                    new("Entity.Customer", "Guide.Quotation.Field.Customer"),
                    new("Entity.Company", "Guide.Quotation.Field.Company"),
                    new("Field.SalesPerson", "Guide.Quotation.Field.Salesperson"),
                    new("Field.QuotationDate", "Guide.Quotation.Field.Date"),
                    new("Field.ProjectName", "Guide.Quotation.Field.ProjectName"),
                    new("Field.TaxType", "Guide.Quotation.Field.TaxMethod"),
                    new("Field.PaymentTerms", "Guide.Quotation.Field.PaymentTerms"),
                    new("Field.DeliveryTerms", "Guide.Quotation.Field.DeliveryTerms"),
                    new("Field.Remarks", "Guide.Quotation.Field.Remarks")
                }
            },

            new GuideSection
            {
                Id = "guide-qt-amount",
                TitleKey = "Guide.SalesOrder.AmountFieldsTitle",
                Icon = "bi-calculator",
                BookmarkLabel = "金額",
                BookmarkColor = "#059669",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.SubtotalBeforeDiscount", "Guide.Quotation.Field.Subtotal"),
                    new("Field.DiscountAmount", "Guide.Quotation.Field.Discount"),
                    new("Field.QuotationTaxAmount", "Guide.Quotation.Field.TaxAmount"),
                    new("Field.TotalAmount", "Guide.Quotation.Field.TotalAmount")
                }
            },

            new GuideSection
            {
                Id = "guide-qt-actions",
                TitleKey = "Guide.SalesOrder.ActionsTitle",
                Icon = "bi-gear",
                BookmarkLabel = "功能",
                BookmarkColor = "#D946EF",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Guide.Quotation.Action.ConvertLabel", "Guide.Quotation.Action.Convert"),
                    new("Guide.Quotation.Action.PrintLabel", "Guide.Quotation.Action.Print"),
                    new("Guide.Quotation.Action.DraftLabel", "Guide.Quotation.Action.Draft")
                }
            },

            new GuideSection
            {
                Id = "guide-qt-tips",
                TitleKey = "Guide.Tips",
                Icon = "bi-lightbulb",
                BookmarkLabel = "提示",
                BookmarkColor = "#06B6D4",
                Type = GuideSectionType.Tips,
                Items =
                {
                    new("Guide.Quotation.Tip1", GuideItemStyle.Tip),
                    new("Guide.Quotation.Tip2", GuideItemStyle.Tip),
                    new("Guide.Quotation.Warning1", GuideItemStyle.Warning),
                    new("Guide.Quotation.Warning2", GuideItemStyle.Warning)
                }
            },

            new GuideSection
            {
                Id = "guide-qt-faq",
                TitleKey = "Guide.SalesOrder.FaqTitle",
                Icon = "bi-question-diamond",
                BookmarkLabel = "FAQ",
                BookmarkColor = "#6366F1",
                Type = GuideSectionType.FAQ,
                Items =
                {
                    new("Guide.Quotation.Faq1Q", "Guide.Quotation.Faq1A"),
                    new("Guide.Quotation.Faq2Q", "Guide.Quotation.Faq2A"),
                    new("Guide.Quotation.Faq3Q", "Guide.Quotation.Faq3A")
                }
            },
        }
    };
}
