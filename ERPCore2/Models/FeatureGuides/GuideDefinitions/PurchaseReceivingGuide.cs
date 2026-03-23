using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

/// <summary>
/// 進貨單功能說明定義
/// </summary>
public static class PurchaseReceivingGuide
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
                Id = "guide-pr-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items =
                {
                    new("Guide.PurchaseReceiving.Description"),
                    new("Guide.PurchaseReceiving.TabOverview"),
                }
            },

            // ===== 操作步驟 =====
            new GuideSection
            {
                Id = "guide-pr-steps",
                TitleKey = "Guide.Steps",
                Icon = "bi-list-ol",
                BookmarkLabel = "步驟",
                BookmarkColor = "#10B981",
                Type = GuideSectionType.Steps,
                Items =
                {
                    new("Guide.PurchaseReceiving.Step1"),
                    new("Guide.PurchaseReceiving.Step2"),
                    new("Guide.PurchaseReceiving.Step3"),
                    new("Guide.PurchaseReceiving.Step4"),
                    new("Guide.PurchaseReceiving.Step5"),
                }
            },

            // ===== 欄位說明 =====
            new GuideSection
            {
                Id = "guide-pr-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.PurchaseReceivingCode", "Guide.PurchaseReceiving.Field.Code"),
                    new("Field.Supplier", "Guide.PurchaseReceiving.Field.Supplier"),
                    new("Field.ReceiptDate", "Guide.PurchaseReceiving.Field.ReceiptDate"),
                    new("Field.TaxType", "Guide.PurchaseReceiving.Field.TaxMethod"),
                    new("Field.BatchNumber", "Guide.PurchaseReceiving.Field.BatchNumber"),
                    new("Field.Remarks", "Guide.PurchaseReceiving.Field.Remarks"),
                }
            },

            // ===== 金額欄位 =====
            new GuideSection
            {
                Id = "guide-pr-amount",
                TitleKey = "Guide.SalesOrder.AmountFieldsTitle",
                Icon = "bi-calculator",
                BookmarkLabel = "金額",
                BookmarkColor = "#059669",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.TotalAmount", "Guide.PurchaseReceiving.Field.TotalAmount"),
                    new("Field.PurchaseReceivingTaxAmount", "Guide.PurchaseReceiving.Field.TaxAmount"),
                    new("Field.PurchaseReceivingTotalAmountIncludingTax", "Guide.PurchaseReceiving.Field.TotalWithTax"),
                }
            },

            // ===== 明細操作 =====
            new GuideSection
            {
                Id = "guide-pr-details",
                TitleKey = "Guide.SalesOrder.DetailTitle",
                Icon = "bi-table",
                BookmarkLabel = "明細",
                BookmarkColor = "#8B5CF6",
                Type = GuideSectionType.Steps,
                Items =
                {
                    new("Guide.PurchaseReceiving.Detail1"),
                    new("Guide.PurchaseReceiving.Detail2"),
                    new("Guide.PurchaseReceiving.Detail3"),
                    new("Guide.PurchaseReceiving.Detail4"),
                }
            },

            // ===== 功能按鈕 =====
            new GuideSection
            {
                Id = "guide-pr-actions",
                TitleKey = "Guide.SalesOrder.ActionsTitle",
                Icon = "bi-gear",
                BookmarkLabel = "功能",
                BookmarkColor = "#D946EF",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Guide.PurchaseReceiving.Action.ReturnLabel", "Guide.PurchaseReceiving.Action.Return"),
                    new("Guide.PurchaseReceiving.Action.SetoffLabel", "Guide.PurchaseReceiving.Action.Setoff"),
                }
            },

            // ===== 提示與警告 =====
            new GuideSection
            {
                Id = "guide-pr-tips",
                TitleKey = "Guide.Tips",
                Icon = "bi-lightbulb",
                BookmarkLabel = "提示",
                BookmarkColor = "#06B6D4",
                Type = GuideSectionType.Tips,
                Items =
                {
                    new("Guide.PurchaseReceiving.Tip1", GuideItemStyle.Tip),
                    new("Guide.PurchaseReceiving.Tip2", GuideItemStyle.Tip),
                    new("Guide.PurchaseReceiving.Warning1", GuideItemStyle.Warning),
                }
            },

            // ===== 常見問題 =====
            new GuideSection
            {
                Id = "guide-pr-faq",
                TitleKey = "Guide.SalesOrder.FaqTitle",
                Icon = "bi-question-diamond",
                BookmarkLabel = "FAQ",
                BookmarkColor = "#6366F1",
                Type = GuideSectionType.FAQ,
                Items =
                {
                    new("Guide.PurchaseReceiving.Faq1Q", "Guide.PurchaseReceiving.Faq1A"),
                    new("Guide.PurchaseReceiving.Faq2Q", "Guide.PurchaseReceiving.Faq2A"),
                }
            },
        }
    };
}
