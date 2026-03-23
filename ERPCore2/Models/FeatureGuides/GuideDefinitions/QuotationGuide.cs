using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

/// <summary>
/// 報價單功能說明定義
/// </summary>
public static class QuotationGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            // ===== 功能概述 =====
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
                    new("Guide.Quotation.Description"),
                    new("Guide.Quotation.TabOverview"),
                }
            },

            // ===== 操作步驟 =====
            new GuideSection
            {
                Id = "guide-qt-steps",
                TitleKey = "Guide.Steps",
                Icon = "bi-list-ol",
                BookmarkLabel = "步驟",
                BookmarkColor = "#10B981",
                Type = GuideSectionType.Steps,
                Items =
                {
                    new("Guide.Quotation.Step1"),
                    new("Guide.Quotation.Step2"),
                    new("Guide.Quotation.Step3"),
                    new("Guide.Quotation.Step4"),
                    new("Guide.Quotation.Step5"),
                }
            },

            // ===== 基本資料欄位 =====
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
                    new("Field.Customer", "Guide.Quotation.Field.Customer"),
                    new("Field.Company", "Guide.Quotation.Field.Company"),
                    new("Field.SalesPerson", "Guide.Quotation.Field.Salesperson"),
                    new("Field.QuotationDate", "Guide.Quotation.Field.Date"),
                    new("Field.ProjectName", "Guide.Quotation.Field.ProjectName"),
                    new("Field.TaxType", "Guide.Quotation.Field.TaxMethod"),
                    new("Field.PaymentTerms", "Guide.Quotation.Field.PaymentTerms"),
                    new("Field.DeliveryTerms", "Guide.Quotation.Field.DeliveryTerms"),
                    new("Field.Remarks", "Guide.Quotation.Field.Remarks"),
                }
            },

            // ===== 金額欄位 =====
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
                    new("Field.TotalAmount", "Guide.Quotation.Field.TotalAmount"),
                }
            },

            // ===== 功能按鈕 =====
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
                    new("Guide.Quotation.Action.DraftLabel", "Guide.Quotation.Action.Draft"),
                }
            },

            // ===== 提示與警告 =====
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
                    new("Guide.Quotation.Warning2", GuideItemStyle.Warning),
                }
            },

            // ===== 常見問題 =====
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
                    new("Guide.Quotation.Faq3Q", "Guide.Quotation.Faq3A"),
                }
            },
        }
    };
}
