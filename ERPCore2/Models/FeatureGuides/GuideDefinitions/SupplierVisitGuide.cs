using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

public static class SupplierVisitGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-sv-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items = { new("Guide.SupplierVisit.Description") }
            },

            new GuideSection
            {
                Id = "guide-sv-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.Supplier", "Guide.SupplierVisit.Field.Supplier"),
                    new("Field.VisitDate", "Guide.SupplierVisit.Field.Date"),
                    new("Field.VisitType", "Guide.SupplierVisit.Field.Type"),
                    new("Field.Employee", "Guide.SupplierVisit.Field.Employee"),
                    new("Field.Remarks", "Guide.SupplierVisit.Field.Remarks")
                }
            },
        }
    };
}
