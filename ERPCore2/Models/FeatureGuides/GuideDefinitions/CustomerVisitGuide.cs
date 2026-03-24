using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

public static class CustomerVisitGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-cv-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items = { new("Guide.CustomerVisit.Description") }
            },

            new GuideSection
            {
                Id = "guide-cv-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.Customer", "Guide.CustomerVisit.Field.Customer"),
                    new("Field.VisitDate", "Guide.CustomerVisit.Field.Date"),
                    new("Field.VisitType", "Guide.CustomerVisit.Field.Type"),
                    new("Field.Employee", "Guide.CustomerVisit.Field.Employee"),
                    new("Field.Remarks", "Guide.CustomerVisit.Field.Remarks")
                }
            },
        }
    };
}
