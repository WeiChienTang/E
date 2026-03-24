using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

public static class EmployeeLicenseGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-elic-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items = { new("Guide.EmployeeLicense.Description") }
            },

            new GuideSection
            {
                Id = "guide-elic-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.LicenseName", "Guide.EmployeeLicense.Field.Name"),
                    new("Field.LicenseNumber", "Guide.EmployeeLicense.Field.Number"),
                    new("Field.IssuingAuthority", "Guide.EmployeeLicense.Field.Authority"),
                    new("Field.ExpiryDate", "Guide.EmployeeLicense.Field.Expiry"),
                    new("Field.Remarks", "Guide.EmployeeLicense.Field.Remarks")
                }
            },
        }
    };
}
