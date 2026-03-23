using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

/// <summary>
/// 出貨單功能說明定義
/// </summary>
public static class SalesDeliveryGuide
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
                Id = "guide-sd-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items =
                {
                    new("Guide.SalesDelivery.Description"),
                    new("Guide.SalesDelivery.TabOverview"),
                }
            },

            // ===== 操作步驟 =====
            new GuideSection
            {
                Id = "guide-sd-steps",
                TitleKey = "Guide.Steps",
                Icon = "bi-list-ol",
                BookmarkLabel = "步驟",
                BookmarkColor = "#10B981",
                Type = GuideSectionType.Steps,
                Items =
                {
                    new("Guide.SalesDelivery.Step1"),
                    new("Guide.SalesDelivery.Step2"),
                    new("Guide.SalesDelivery.Step3"),
                    new("Guide.SalesDelivery.Step4"),
                    new("Guide.SalesDelivery.Step5"),
                }
            },

            // ===== 欄位說明 =====
            new GuideSection
            {
                Id = "guide-sd-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.SalesDeliveryCode", "Guide.SalesDelivery.Field.Code"),
                    new("Field.Customer", "Guide.SalesDelivery.Field.Customer"),
                    new("Field.SalesPerson", "Guide.SalesDelivery.Field.Salesperson"),
                    new("Field.DeliveryDate", "Guide.SalesDelivery.Field.DeliveryDate"),
                    new("Field.DeliveryAddress", "Guide.SalesDelivery.Field.Address"),
                    new("Field.TaxType", "Guide.SalesDelivery.Field.TaxMethod"),
                    new("Field.Remarks", "Guide.SalesDelivery.Field.Remarks"),
                }
            },

            // ===== 金額欄位 =====
            new GuideSection
            {
                Id = "guide-sd-amount",
                TitleKey = "Guide.SalesOrder.AmountFieldsTitle",
                Icon = "bi-calculator",
                BookmarkLabel = "金額",
                BookmarkColor = "#059669",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.TotalAmount", "Guide.SalesDelivery.Field.TotalAmount"),
                    new("Field.DiscountAmount", "Guide.SalesDelivery.Field.Discount"),
                    new("Field.TaxAmount", "Guide.SalesDelivery.Field.TaxAmount"),
                    new("Field.TotalAmountWithTax", "Guide.SalesDelivery.Field.TotalWithTax"),
                }
            },

            // ===== 功能按鈕 =====
            new GuideSection
            {
                Id = "guide-sd-actions",
                TitleKey = "Guide.SalesOrder.ActionsTitle",
                Icon = "bi-gear",
                BookmarkLabel = "功能",
                BookmarkColor = "#D946EF",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Guide.SalesDelivery.Action.ReturnLabel", "Guide.SalesDelivery.Action.Return"),
                    new("Guide.SalesDelivery.Action.SetoffLabel", "Guide.SalesDelivery.Action.Setoff"),
                    new("Guide.SalesDelivery.Action.PrintLabel", "Guide.SalesDelivery.Action.Print"),
                }
            },

            // ===== 提示與警告 =====
            new GuideSection
            {
                Id = "guide-sd-tips",
                TitleKey = "Guide.Tips",
                Icon = "bi-lightbulb",
                BookmarkLabel = "提示",
                BookmarkColor = "#06B6D4",
                Type = GuideSectionType.Tips,
                Items =
                {
                    new("Guide.SalesDelivery.Tip1", GuideItemStyle.Tip),
                    new("Guide.SalesDelivery.Tip2", GuideItemStyle.Tip),
                    new("Guide.SalesDelivery.Warning1", GuideItemStyle.Warning),
                    new("Guide.SalesDelivery.Warning2", GuideItemStyle.Warning),
                }
            },

            // ===== 常見問題 =====
            new GuideSection
            {
                Id = "guide-sd-faq",
                TitleKey = "Guide.SalesOrder.FaqTitle",
                Icon = "bi-question-diamond",
                BookmarkLabel = "FAQ",
                BookmarkColor = "#6366F1",
                Type = GuideSectionType.FAQ,
                Items =
                {
                    new("Guide.SalesDelivery.Faq1Q", "Guide.SalesDelivery.Faq1A"),
                    new("Guide.SalesDelivery.Faq2Q", "Guide.SalesDelivery.Faq2A"),
                    new("Guide.SalesDelivery.Faq3Q", "Guide.SalesDelivery.Faq3A"),
                }
            },
        }
    };
}
