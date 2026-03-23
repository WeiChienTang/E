using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

/// <summary>
/// 銷售訂單功能說明定義
/// </summary>
public static class SalesOrderGuide
{
    /// <summary>捕獲本檔案路徑（CallerFilePath 取的是呼叫處，所以必須在同一個檔案內呼叫）</summary>
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            // ===== 功能概述 =====
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
                    new("Guide.SalesOrder.Description"),
                    new("Guide.SalesOrder.TabOverview"),
                }
            },

            // ===== 操作步驟 =====
            new GuideSection
            {
                Id = "guide-so-steps",
                TitleKey = "Guide.Steps",
                Icon = "bi-list-ol",
                BookmarkLabel = "步驟",
                BookmarkColor = "#10B981",
                Type = GuideSectionType.Steps,
                Items =
                {
                    new("Guide.SalesOrder.Step1"),
                    new("Guide.SalesOrder.Step2"),
                    new("Guide.SalesOrder.Step3"),
                    new("Guide.SalesOrder.Step4"),
                    new("Guide.SalesOrder.Step5"),
                    new("Guide.SalesOrder.Step6"),
                }
            },

            // ===== 基本資料欄位 =====
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
                    new("Field.Customer", "Guide.SalesOrder.Field.Customer"),
                    new("Field.Company", "Guide.SalesOrder.Field.Company"),
                    new("Field.SalesPerson", "Guide.SalesOrder.Field.Salesperson"),
                    new("Field.QuotationCreator", "Guide.SalesOrder.Field.FormCreator"),
                    new("Field.SalesOrderDate", "Guide.SalesOrder.Field.OrderDate"),
                    new("Field.ExpectedDeliveryDate", "Guide.SalesOrder.Field.ExpectedDeliveryDate"),
                    new("Field.PaymentTerms", "Guide.SalesOrder.Field.PaymentTerms"),
                    new("Field.DeliveryTerms", "Guide.SalesOrder.Field.DeliveryTerms"),
                    new("Field.Remarks", "Guide.SalesOrder.Field.Remarks"),
                }
            },

            // ===== 金額與稅務欄位 =====
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
                    new("Field.TotalAmountWithTax", "Guide.SalesOrder.Field.TotalAmountWithTax"),
                }
            },

            // ===== 明細操作 =====
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
                    new("Guide.SalesOrder.Detail6"),
                }
            },

            // ===== 功能按鈕 =====
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
                    new("Guide.SalesOrder.Action.DraftLabel", "Guide.SalesOrder.Action.Draft"),
                }
            },

            // ===== 審核流程 =====
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
                    new("Guide.SalesOrder.Approval4"),
                }
            },

            // ===== 提示與警告 =====
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
                    new("Guide.SalesOrder.Warning3", GuideItemStyle.Warning),
                }
            },

            // ===== 常見問題 =====
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
                    new("Guide.SalesOrder.Faq5Q", "Guide.SalesOrder.Faq5A"),
                }
            },
        }
    };
}
