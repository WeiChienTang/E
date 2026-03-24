using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

public static class ScaleRecordGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-scr-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items = { new("Guide.ScaleRecord.Description") }
            },

            new GuideSection
            {
                Id = "guide-scr-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.Code", "Guide.ScaleRecord.Field.Code"),
                    new("Field.Name", "Guide.ScaleRecord.Field.Vehicle"),
                    new("Field.Customer", "Guide.ScaleRecord.Field.Customer"),
                    new("Field.Remarks", "Guide.ScaleRecord.Field.Remarks")
                }
            },

            new GuideSection
            {
                Id = "guide-scr-actions",
                TitleKey = "Guide.SalesOrder.ActionsTitle",
                Icon = "bi-gear",
                BookmarkLabel = "功能",
                BookmarkColor = "#D946EF",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Guide.ScaleRecord.Action.ReadLabel", "Guide.ScaleRecord.Action.Read")
                }
            },

            new GuideSection
            {
                Id = "guide-scr-tips",
                TitleKey = "Guide.Tips",
                Icon = "bi-lightbulb",
                BookmarkLabel = "提示",
                BookmarkColor = "#06B6D4",
                Type = GuideSectionType.Tips,
                Items =
                {
                    new("Guide.ScaleRecord.Tip1", GuideItemStyle.Tip)
                }
            },
        }
    };
}
