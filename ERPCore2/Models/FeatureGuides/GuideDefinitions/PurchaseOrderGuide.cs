using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

public static class PurchaseOrderGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-po-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items =
                {
                    new("Guide.PurchaseOrder.Description")
                }
            },

            new GuideSection
            {
                Id = "guide-po-tabs",
                TitleKey = "Guide.TabDescriptions",
                Icon = "bi-folder2-open",
                BookmarkLabel = "分頁",
                BookmarkColor = "#8B5CF6",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Tab.PurchaseOrderData", "Guide.PurchaseOrder.Tab.OrderData"),
                    new("Tab.SupplierInfo", "Guide.PurchaseOrder.Tab.SupplierInfo")
                }
            },

            new GuideSection
            {
                Id = "guide-po-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.PurchaseOrderCode", "Guide.PurchaseOrder.Field.Code"),
                    new("Entity.Supplier", "Guide.PurchaseOrder.Field.Supplier"),
                    new("Field.PurchasingCompany", "Guide.PurchaseOrder.Field.Company"),
                    new("Field.Purchaser", "Guide.PurchaseOrder.Field.Personnel"),
                    new("Field.PurchaseDate", "Guide.PurchaseOrder.Field.OrderDate"),
                    new("Field.DeliveryDate", "Guide.PurchaseOrder.Field.ExpectedDate"),
                    new("Field.TaxType", "Guide.PurchaseOrder.Field.TaxMethod"),
                    new("Field.Remarks", "Guide.PurchaseOrder.Field.Remarks")
                }
            },

            new GuideSection
            {
                Id = "guide-po-amount",
                TitleKey = "Guide.SalesOrder.AmountFieldsTitle",
                Icon = "bi-calculator",
                BookmarkLabel = "金額",
                BookmarkColor = "#059669",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.TotalAmount", "Guide.PurchaseOrder.Field.TotalAmount"),
                    new("Field.PurchaseTaxAmount", "Guide.PurchaseOrder.Field.TaxAmount"),
                    new("Field.PurchaseTotalAmountIncludingTax", "Guide.PurchaseOrder.Field.TotalWithTax")
                }
            },

            new GuideSection
            {
                Id = "guide-po-actions",
                TitleKey = "Guide.SalesOrder.ActionsTitle",
                Icon = "bi-gear",
                BookmarkLabel = "功能",
                BookmarkColor = "#D946EF",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Guide.PurchaseOrder.Action.ConvertLabel", "Guide.PurchaseOrder.Action.Convert"),
                    new("Guide.PurchaseOrder.Action.CopyMsgLabel", "Guide.PurchaseOrder.Action.CopyMsg"),
                    new("Guide.PurchaseOrder.Action.PrintLabel", "Guide.PurchaseOrder.Action.Print")
                }
            },

            new GuideSection
            {
                Id = "guide-po-tips",
                TitleKey = "Guide.Tips",
                Icon = "bi-lightbulb",
                BookmarkLabel = "提示",
                BookmarkColor = "#06B6D4",
                Type = GuideSectionType.Tips,
                Items =
                {
                    new("Guide.PurchaseOrder.Tip1", GuideItemStyle.Tip),
                    new("Guide.PurchaseOrder.Tip2", GuideItemStyle.Tip),
                    new("Guide.PurchaseOrder.Warning1", GuideItemStyle.Warning),
                    new("Guide.PurchaseOrder.Warning2", GuideItemStyle.Warning)
                }
            },

            new GuideSection
            {
                Id = "guide-po-faq",
                TitleKey = "Guide.SalesOrder.FaqTitle",
                Icon = "bi-question-diamond",
                BookmarkLabel = "FAQ",
                BookmarkColor = "#6366F1",
                Type = GuideSectionType.FAQ,
                Items =
                {
                    new("Guide.PurchaseOrder.Faq1Q", "Guide.PurchaseOrder.Faq1A"),
                    new("Guide.PurchaseOrder.Faq2Q", "Guide.PurchaseOrder.Faq2A"),
                    new("Guide.PurchaseOrder.Faq3Q", "Guide.PurchaseOrder.Faq3A")
                }
            },
        }
    };
}
