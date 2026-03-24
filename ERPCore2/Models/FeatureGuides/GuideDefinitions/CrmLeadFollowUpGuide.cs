using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

public static class CrmLeadFollowUpGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-crmfu-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items = { new("Guide.CrmLeadFollowUp.Description") }
            },

            new GuideSection
            {
                Id = "guide-crmfu-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.Name", "Guide.CrmLeadFollowUp.Field.Type"),
                    new("Field.Description", "Guide.CrmLeadFollowUp.Field.Notes"),
                    new("Field.Remarks", "Guide.CrmLeadFollowUp.Field.Remarks")
                }
            },
        }
    };
}
