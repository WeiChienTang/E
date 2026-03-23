using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

/// <summary>
/// 採購退回功能說明定義
/// </summary>
public static class PurchaseReturnGuide
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
                Id = "guide-prt-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items =
                {
                    new("Guide.PurchaseReturn.Description"),
                    new("Guide.PurchaseReturn.TabOverview"),
                }
            },

            // ===== 操作步驟 =====
            new GuideSection
            {
                Id = "guide-prt-steps",
                TitleKey = "Guide.Steps",
                Icon = "bi-list-ol",
                BookmarkLabel = "步驟",
                BookmarkColor = "#10B981",
                Type = GuideSectionType.Steps,
                Items =
                {
                    new("Guide.PurchaseReturn.Step1"),
                    new("Guide.PurchaseReturn.Step2"),
                    new("Guide.PurchaseReturn.Step3"),
                    new("Guide.PurchaseReturn.Step4"),
                    new("Guide.PurchaseReturn.Step5"),
                }
            },

            // ===== 欄位說明 =====
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
                    new("Field.Supplier", "Guide.PurchaseReturn.Field.Supplier"),
                    new("Field.PurchaseReturnDate", "Guide.PurchaseReturn.Field.ReturnDate"),
                    new("Field.ReturnReason", "Guide.PurchaseReturn.Field.Reason"),
                    new("Field.TaxType", "Guide.PurchaseReturn.Field.TaxMethod"),
                    new("Field.Remarks", "Guide.PurchaseReturn.Field.Remarks"),
                }
            },

            // ===== 金額欄位 =====
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
                    new("Field.TotalReturnAmountWithTax", "Guide.PurchaseReturn.Field.TotalWithTax"),
                }
            },

            // ===== 功能按鈕 =====
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
                    new("Guide.PurchaseReturn.Action.SetoffLabel", "Guide.PurchaseReturn.Action.Setoff"),
                }
            },

            // ===== 提示與警告 =====
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
                    new("Guide.PurchaseReturn.Warning2", GuideItemStyle.Warning),
                }
            },

            // ===== 常見問題 =====
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
                    new("Guide.PurchaseReturn.Faq2Q", "Guide.PurchaseReturn.Faq2A"),
                }
            },
        }
    };
}
