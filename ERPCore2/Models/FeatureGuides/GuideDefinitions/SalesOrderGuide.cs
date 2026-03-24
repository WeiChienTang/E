using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

public static class SalesOrderGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-so-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items =
                {
                    new("Guide.SalesOrder.Description")
                }
            },

            new GuideSection
            {
                Id = "guide-so-tabs",
                TitleKey = "Guide.TabDescriptions",
                Icon = "bi-folder2-open",
                BookmarkLabel = "分頁",
                BookmarkColor = "#8B5CF6",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Tab.OrderData", "Guide.SalesOrder.Tab.OrderData"),
                    new("Tab.CustomerInfo", "Guide.SalesOrder.Tab.CustomerInfo"),
                    new("Tab.SalesOrderPhotos", "Guide.SalesOrder.Tab.Photos")
                }
            },

            new GuideSection
            {
                Id = "guide-so-fields-basic",
                TitleKey = "Guide.SalesOrder.BasicFieldsTitle",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.SalesOrderCode", "Guide.SalesOrder.Field.Code"),
                    new("Entity.Customer", "Guide.SalesOrder.Field.Customer"),
                    new("Entity.Company", "Guide.SalesOrder.Field.Company"),
                    new("Field.SalesPerson", "Guide.SalesOrder.Field.Salesperson"),
                    new("Field.QuotationCreator", "Guide.SalesOrder.Field.FormCreator"),
                    new("Field.SalesOrderDate", "Guide.SalesOrder.Field.OrderDate"),
                    new("Field.ExpectedDeliveryDate", "Guide.SalesOrder.Field.ExpectedDeliveryDate"),
                    new("Field.PaymentTerms", "Guide.SalesOrder.Field.PaymentTerms"),
                    new("Field.DeliveryTerms", "Guide.SalesOrder.Field.DeliveryTerms"),
                    new("Field.Remarks", "Guide.SalesOrder.Field.Remarks")
                }
            },

            new GuideSection
            {
                Id = "guide-so-fields-amount",
                TitleKey = "Guide.SalesOrder.AmountFieldsTitle",
                Icon = "bi-calculator",
                BookmarkLabel = "金額",
                BookmarkColor = "#059669",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.TaxType", "Guide.SalesOrder.Field.TaxMethod"),
                    new("Field.TotalAmountFull", "Guide.SalesOrder.Field.TotalAmount"),
                    new("Field.DiscountAmount", "Guide.SalesOrder.Field.DiscountAmount"),
                    new("Field.TaxAmount", "Guide.SalesOrder.Field.SalesTaxAmount"),
                    new("Field.TotalAmountWithTax", "Guide.SalesOrder.Field.TotalAmountWithTax")
                }
            },

            new GuideSection
            {
                Id = "guide-so-details",
                TitleKey = "Guide.SalesOrder.DetailTitle",
                Icon = "bi-table",
                BookmarkLabel = "明細",
                BookmarkColor = "#8B5CF6",
                Type = GuideSectionType.Steps,
                Items =
                {
                    new("Guide.SalesOrder.Detail1"),
                    new("Guide.SalesOrder.Detail2"),
                    new("Guide.SalesOrder.Detail3"),
                    new("Guide.SalesOrder.Detail4"),
                    new("Guide.SalesOrder.Detail5"),
                    new("Guide.SalesOrder.Detail6")
                }
            },

            new GuideSection
            {
                Id = "guide-so-actions",
                TitleKey = "Guide.SalesOrder.ActionsTitle",
                Icon = "bi-gear",
                BookmarkLabel = "功能",
                BookmarkColor = "#D946EF",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Guide.SalesOrder.Action.ConvertLabel", "Guide.SalesOrder.Action.Convert"),
                    new("Guide.SalesOrder.Action.ScheduleLabel", "Guide.SalesOrder.Action.Schedule"),
                    new("Guide.SalesOrder.Action.InventoryLabel", "Guide.SalesOrder.Action.Inventory"),
                    new("Guide.SalesOrder.Action.PrintLabel", "Guide.SalesOrder.Action.Print"),
                    new("Guide.SalesOrder.Action.DraftLabel", "Guide.SalesOrder.Action.Draft")
                }
            },

            new GuideSection
            {
                Id = "guide-so-approval",
                TitleKey = "Guide.SalesOrder.ApprovalTitle",
                Icon = "bi-clipboard-check",
                BookmarkLabel = "審核",
                BookmarkColor = "#EF4444",
                Type = GuideSectionType.Steps,
                Items =
                {
                    new("Guide.SalesOrder.Approval1"),
                    new("Guide.SalesOrder.Approval2"),
                    new("Guide.SalesOrder.Approval3"),
                    new("Guide.SalesOrder.Approval4")
                }
            },

            new GuideSection
            {
                Id = "guide-so-tips",
                TitleKey = "Guide.Tips",
                Icon = "bi-lightbulb",
                BookmarkLabel = "提示",
                BookmarkColor = "#06B6D4",
                Type = GuideSectionType.Tips,
                Items =
                {
                    new("Guide.SalesOrder.Tip1", GuideItemStyle.Tip),
                    new("Guide.SalesOrder.Tip2", GuideItemStyle.Tip),
                    new("Guide.SalesOrder.Tip3", GuideItemStyle.Tip),
                    new("Guide.SalesOrder.Warning1", GuideItemStyle.Warning),
                    new("Guide.SalesOrder.Warning2", GuideItemStyle.Warning),
                    new("Guide.SalesOrder.Warning3", GuideItemStyle.Warning)
                }
            },

            new GuideSection
            {
                Id = "guide-so-faq",
                TitleKey = "Guide.SalesOrder.FaqTitle",
                Icon = "bi-question-diamond",
                BookmarkLabel = "FAQ",
                BookmarkColor = "#6366F1",
                Type = GuideSectionType.FAQ,
                Items =
                {
                    new("Guide.SalesOrder.Faq1Q", "Guide.SalesOrder.Faq1A"),
                    new("Guide.SalesOrder.Faq2Q", "Guide.SalesOrder.Faq2A"),
                    new("Guide.SalesOrder.Faq3Q", "Guide.SalesOrder.Faq3A"),
                    new("Guide.SalesOrder.Faq4Q", "Guide.SalesOrder.Faq4A"),
                    new("Guide.SalesOrder.Faq5Q", "Guide.SalesOrder.Faq5A")
                }
            },
        }
    };
}
