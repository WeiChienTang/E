using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

/// <summary>
/// 銷貨退回功能說明定義
/// </summary>
public static class SalesReturnGuide
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
                Id = "guide-sr-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items =
                {
                    new("Guide.SalesReturn.Description"),
                    new("Guide.SalesReturn.TabOverview"),
                }
            },

            // ===== 操作步驟 =====
            new GuideSection
            {
                Id = "guide-sr-steps",
                TitleKey = "Guide.Steps",
                Icon = "bi-list-ol",
                BookmarkLabel = "步驟",
                BookmarkColor = "#10B981",
                Type = GuideSectionType.Steps,
                Items =
                {
                    new("Guide.SalesReturn.Step1"),
                    new("Guide.SalesReturn.Step2"),
                    new("Guide.SalesReturn.Step3"),
                    new("Guide.SalesReturn.Step4"),
                    new("Guide.SalesReturn.Step5"),
                }
            },

            // ===== 欄位說明 =====
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
                    new("Field.Customer", "Guide.SalesReturn.Field.Customer"),
                    new("Field.SalesReturnDate", "Guide.SalesReturn.Field.ReturnDate"),
                    new("Field.ReturnReason", "Guide.SalesReturn.Field.Reason"),
                    new("Field.TaxType", "Guide.SalesReturn.Field.TaxMethod"),
                    new("Field.Remarks", "Guide.SalesReturn.Field.Remarks"),
                }
            },

            // ===== 金額欄位 =====
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
                    new("Field.TotalReturnAmountWithTax", "Guide.SalesReturn.Field.TotalWithTax"),
                }
            },

            // ===== 功能按鈕 =====
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
                    new("Guide.SalesReturn.Action.SetoffLabel", "Guide.SalesReturn.Action.Setoff"),
                }
            },

            // ===== 提示與警告 =====
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
                    new("Guide.SalesReturn.Warning2", GuideItemStyle.Warning),
                }
            },

            // ===== 常見問題 =====
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
                    new("Guide.SalesReturn.Faq2Q", "Guide.SalesReturn.Faq2A"),
                }
            },
        }
    };
}
