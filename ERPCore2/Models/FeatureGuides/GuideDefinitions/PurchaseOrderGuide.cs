using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

/// <summary>
/// 採購單功能說明定義
/// </summary>
public static class PurchaseOrderGuide
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
                Id = "guide-po-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items =
                {
                    new("Guide.PurchaseOrder.Description"),
                    new("Guide.PurchaseOrder.TabOverview"),
                }
            },

            // ===== 操作步驟 =====
            new GuideSection
            {
                Id = "guide-po-steps",
                TitleKey = "Guide.Steps",
                Icon = "bi-list-ol",
                BookmarkLabel = "步驟",
                BookmarkColor = "#10B981",
                Type = GuideSectionType.Steps,
                Items =
                {
                    new("Guide.PurchaseOrder.Step1"),
                    new("Guide.PurchaseOrder.Step2"),
                    new("Guide.PurchaseOrder.Step3"),
                    new("Guide.PurchaseOrder.Step4"),
                    new("Guide.PurchaseOrder.Step5"),
                }
            },

            // ===== 欄位說明 =====
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
                    new("Field.Supplier", "Guide.PurchaseOrder.Field.Supplier"),
                    new("Field.Company", "Guide.PurchaseOrder.Field.Company"),
                    new("Field.PurchasePersonnel", "Guide.PurchaseOrder.Field.Personnel"),
                    new("Field.SalesOrderDate", "Guide.PurchaseOrder.Field.OrderDate"),
                    new("Field.ExpectedDeliveryDate", "Guide.PurchaseOrder.Field.ExpectedDate"),
                    new("Field.TaxType", "Guide.PurchaseOrder.Field.TaxMethod"),
                    new("Field.Remarks", "Guide.PurchaseOrder.Field.Remarks"),
                }
            },

            // ===== 金額欄位 =====
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
                    new("Field.PurchaseTotalAmountIncludingTax", "Guide.PurchaseOrder.Field.TotalWithTax"),
                }
            },

            // ===== 功能按鈕 =====
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
                    new("Guide.PurchaseOrder.Action.PrintLabel", "Guide.PurchaseOrder.Action.Print"),
                }
            },

            // ===== 提示與警告 =====
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
                    new("Guide.PurchaseOrder.Warning2", GuideItemStyle.Warning),
                }
            },

            // ===== 常見問題 =====
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
                    new("Guide.PurchaseOrder.Faq3Q", "Guide.PurchaseOrder.Faq3A"),
                }
            },
        }
    };
}
