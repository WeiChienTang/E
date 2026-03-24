using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

public static class CustomerComplaintGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-ccmp-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items = { new("Guide.CustomerComplaint.Description") }
            },

            new GuideSection
            {
                Id = "guide-ccmp-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.Customer", "Guide.CustomerComplaint.Field.Customer"),
                    new("Field.Name", "Guide.CustomerComplaint.Field.Title"),
                    new("Field.Description", "Guide.CustomerComplaint.Field.Description"),
                    new("Field.Remarks", "Guide.CustomerComplaint.Field.Remarks")
                }
            },

            new GuideSection
            {
                Id = "guide-ccmp-tips",
                TitleKey = "Guide.Tips",
                Icon = "bi-lightbulb",
                BookmarkLabel = "提示",
                BookmarkColor = "#06B6D4",
                Type = GuideSectionType.Tips,
                Items =
                {
                    new("Guide.CustomerComplaint.Tip1", GuideItemStyle.Tip)
                }
            },
        }
    };
}
