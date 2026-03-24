using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

public static class StockTakingGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-stk-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items = { new("Guide.StockTaking.Description") }
            },

            new GuideSection
            {
                Id = "guide-stk-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.Code", "Guide.StockTaking.Field.Code"),
                    new("Field.Warehouse", "Guide.StockTaking.Field.Warehouse"),
                    new("Field.Employee", "Guide.StockTaking.Field.Employee"),
                    new("Field.Remarks", "Guide.StockTaking.Field.Remarks")
                }
            },

            new GuideSection
            {
                Id = "guide-stk-tips",
                TitleKey = "Guide.Tips",
                Icon = "bi-lightbulb",
                BookmarkLabel = "提示",
                BookmarkColor = "#06B6D4",
                Type = GuideSectionType.Tips,
                Items =
                {
                    new("Guide.StockTaking.Tip1", GuideItemStyle.Tip),
                    new("Guide.StockTaking.Warning1", GuideItemStyle.Warning)
                }
            },
        }
    };
}
