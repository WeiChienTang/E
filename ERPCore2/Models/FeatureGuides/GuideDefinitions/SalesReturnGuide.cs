using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

public static class SalesReturnGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-sr-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items =
                {
                    new("Guide.SalesReturn.Description")
                }
            },

            new GuideSection
            {
                Id = "guide-sr-tabs",
                TitleKey = "Guide.TabDescriptions",
                Icon = "bi-folder2-open",
                BookmarkLabel = "分頁",
                BookmarkColor = "#8B5CF6",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Tab.ReturnData", "Guide.SalesReturn.Tab.ReturnData"),
                    new("Tab.CustomerInfo", "Guide.SalesReturn.Tab.CustomerInfo")
                }
            },

            new GuideSection
            {
                Id = "guide-sr-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.SalesReturnCode", "Guide.SalesReturn.Field.Code"),
                    new("Entity.Customer", "Guide.SalesReturn.Field.Customer"),
                    new("Field.SalesReturnDate", "Guide.SalesReturn.Field.ReturnDate"),
                    new("Entity.SalesReturnReason", "Guide.SalesReturn.Field.Reason"),
                    new("Field.TaxType", "Guide.SalesReturn.Field.TaxMethod"),
                    new("Field.Remarks", "Guide.SalesReturn.Field.Remarks")
                }
            },

            new GuideSection
            {
                Id = "guide-sr-amount",
                TitleKey = "Guide.SalesOrder.AmountFieldsTitle",
                Icon = "bi-calculator",
                BookmarkLabel = "金額",
                BookmarkColor = "#059669",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.TotalReturnAmount", "Guide.SalesReturn.Field.TotalReturn"),
                    new("Field.DiscountAmount", "Guide.SalesReturn.Field.Discount"),
                    new("Field.ReturnTaxAmount", "Guide.SalesReturn.Field.TaxAmount"),
                    new("Field.TotalReturnAmountWithTax", "Guide.SalesReturn.Field.TotalWithTax")
                }
            },

            new GuideSection
            {
                Id = "guide-sr-actions",
                TitleKey = "Guide.SalesOrder.ActionsTitle",
                Icon = "bi-gear",
                BookmarkLabel = "功能",
                BookmarkColor = "#D946EF",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Guide.SalesReturn.Action.SetoffLabel", "Guide.SalesReturn.Action.Setoff")
                }
            },

            new GuideSection
            {
                Id = "guide-sr-tips",
                TitleKey = "Guide.Tips",
                Icon = "bi-lightbulb",
                BookmarkLabel = "提示",
                BookmarkColor = "#06B6D4",
                Type = GuideSectionType.Tips,
                Items =
                {
                    new("Guide.SalesReturn.Tip1", GuideItemStyle.Tip),
                    new("Guide.SalesReturn.Warning1", GuideItemStyle.Warning),
                    new("Guide.SalesReturn.Warning2", GuideItemStyle.Warning)
                }
            },

            new GuideSection
            {
                Id = "guide-sr-faq",
                TitleKey = "Guide.SalesOrder.FaqTitle",
                Icon = "bi-question-diamond",
                BookmarkLabel = "FAQ",
                BookmarkColor = "#6366F1",
                Type = GuideSectionType.FAQ,
                Items =
                {
                    new("Guide.SalesReturn.Faq1Q", "Guide.SalesReturn.Faq1A"),
                    new("Guide.SalesReturn.Faq2Q", "Guide.SalesReturn.Faq2A")
                }
            },
        }
    };
}
