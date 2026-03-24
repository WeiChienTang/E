using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

public static class PurchaseReturnGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-prt-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items =
                {
                    new("Guide.PurchaseReturn.Description")
                }
            },

            new GuideSection
            {
                Id = "guide-prt-tabs",
                TitleKey = "Guide.TabDescriptions",
                Icon = "bi-folder2-open",
                BookmarkLabel = "分頁",
                BookmarkColor = "#8B5CF6",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Tab.ReturnData", "Guide.PurchaseReturn.Tab.ReturnData"),
                    new("Tab.SupplierInfo", "Guide.PurchaseReturn.Tab.SupplierInfo")
                }
            },

            new GuideSection
            {
                Id = "guide-prt-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.PurchaseReturnCode", "Guide.PurchaseReturn.Field.Code"),
                    new("Entity.Supplier", "Guide.PurchaseReturn.Field.Supplier"),
                    new("Field.PurchaseReturnDate", "Guide.PurchaseReturn.Field.ReturnDate"),
                    new("Entity.PurchaseReturnReason", "Guide.PurchaseReturn.Field.Reason"),
                    new("Field.TaxType", "Guide.PurchaseReturn.Field.TaxMethod"),
                    new("Field.Remarks", "Guide.PurchaseReturn.Field.Remarks")
                }
            },

            new GuideSection
            {
                Id = "guide-prt-amount",
                TitleKey = "Guide.SalesOrder.AmountFieldsTitle",
                Icon = "bi-calculator",
                BookmarkLabel = "金額",
                BookmarkColor = "#059669",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.TotalReturnAmount", "Guide.PurchaseReturn.Field.TotalReturn"),
                    new("Field.ReturnTaxAmount", "Guide.PurchaseReturn.Field.TaxAmount"),
                    new("Field.TotalReturnAmountWithTax", "Guide.PurchaseReturn.Field.TotalWithTax")
                }
            },

            new GuideSection
            {
                Id = "guide-prt-actions",
                TitleKey = "Guide.SalesOrder.ActionsTitle",
                Icon = "bi-gear",
                BookmarkLabel = "功能",
                BookmarkColor = "#D946EF",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Guide.PurchaseReturn.Action.SetoffLabel", "Guide.PurchaseReturn.Action.Setoff")
                }
            },

            new GuideSection
            {
                Id = "guide-prt-tips",
                TitleKey = "Guide.Tips",
                Icon = "bi-lightbulb",
                BookmarkLabel = "提示",
                BookmarkColor = "#06B6D4",
                Type = GuideSectionType.Tips,
                Items =
                {
                    new("Guide.PurchaseReturn.Tip1", GuideItemStyle.Tip),
                    new("Guide.PurchaseReturn.Warning1", GuideItemStyle.Warning),
                    new("Guide.PurchaseReturn.Warning2", GuideItemStyle.Warning)
                }
            },

            new GuideSection
            {
                Id = "guide-prt-faq",
                TitleKey = "Guide.SalesOrder.FaqTitle",
                Icon = "bi-question-diamond",
                BookmarkLabel = "FAQ",
                BookmarkColor = "#6366F1",
                Type = GuideSectionType.FAQ,
                Items =
                {
                    new("Guide.PurchaseReturn.Faq1Q", "Guide.PurchaseReturn.Faq1A"),
                    new("Guide.PurchaseReturn.Faq2Q", "Guide.PurchaseReturn.Faq2A")
                }
            },
        }
    };
}
