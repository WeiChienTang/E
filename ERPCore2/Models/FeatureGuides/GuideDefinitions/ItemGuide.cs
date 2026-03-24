using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

public static class ItemGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-item-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items =
                {
                    new("Guide.Item.Description")
                }
            },

            new GuideSection
            {
                Id = "guide-item-tabs",
                TitleKey = "Guide.TabDescriptions",
                Icon = "bi-folder2-open",
                BookmarkLabel = "分頁",
                BookmarkColor = "#8B5CF6",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Tab.ItemData", "Guide.Item.Tab.ItemData"),
                    new("Tab.AccountingInfo", "Guide.Item.Tab.Accounting"),
                    new("Tab.ItemPhotos", "Guide.Item.Tab.Photos")
                }
            },

            new GuideSection
            {
                Id = "guide-item-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.ItemCode", "Guide.Item.Field.Code"),
                    new("Field.Barcode", "Guide.Item.Field.Barcode"),
                    new("Field.ItemName", "Guide.Item.Field.Name"),
                    new("Field.ItemCategory", "Guide.Item.Field.Category"),
                    new("Field.Unit", "Guide.Item.Field.Unit"),
                    new("Entity.Size", "Guide.Item.Field.Size"),
                    new("Field.Type", "Guide.Item.Field.Type"),
                    new("Field.TaxRate", "Guide.Item.Field.TaxRate"),
                    new("Field.Specification", "Guide.Item.Field.Spec"),
                    new("Field.Remarks", "Guide.Item.Field.Remarks")
                }
            },

            new GuideSection
            {
                Id = "guide-item-actions",
                TitleKey = "Guide.SalesOrder.ActionsTitle",
                Icon = "bi-gear",
                BookmarkLabel = "功能",
                BookmarkColor = "#D946EF",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Guide.Item.Action.PrintLabel", "Guide.Item.Action.Print")
                }
            },

            new GuideSection
            {
                Id = "guide-item-tips",
                TitleKey = "Guide.Tips",
                Icon = "bi-lightbulb",
                BookmarkLabel = "提示",
                BookmarkColor = "#06B6D4",
                Type = GuideSectionType.Tips,
                Items =
                {
                    new("Guide.Item.Tip1", GuideItemStyle.Tip),
                    new("Guide.Item.Warning1", GuideItemStyle.Warning)
                }
            },

            new GuideSection
            {
                Id = "guide-item-faq",
                TitleKey = "Guide.SalesOrder.FaqTitle",
                Icon = "bi-question-diamond",
                BookmarkLabel = "FAQ",
                BookmarkColor = "#6366F1",
                Type = GuideSectionType.FAQ,
                Items =
                {
                    new("Guide.Item.Faq1Q", "Guide.Item.Faq1A"),
                    new("Guide.Item.Faq2Q", "Guide.Item.Faq2A")
                }
            },
        }
    };
}
