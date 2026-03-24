using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

public static class SetoffDocumentGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-setoff-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items = { new("Guide.SetoffDocument.Description") }
            },

            new GuideSection
            {
                Id = "guide-setoff-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.Code", "Guide.SetoffDocument.Field.Code"),
                    new("Field.Name", "Guide.SetoffDocument.Field.Type"),
                    new("Field.Remarks", "Guide.SetoffDocument.Field.Remarks")
                }
            },

            new GuideSection
            {
                Id = "guide-setoff-tips",
                TitleKey = "Guide.Tips",
                Icon = "bi-lightbulb",
                BookmarkLabel = "提示",
                BookmarkColor = "#06B6D4",
                Type = GuideSectionType.Tips,
                Items =
                {
                    new("Guide.SetoffDocument.Tip1", GuideItemStyle.Tip),
                    new("Guide.SetoffDocument.Warning1", GuideItemStyle.Warning)
                }
            },
        }
    };
}
