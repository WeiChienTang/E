using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

public static class VehicleMaintenanceGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-vmaint-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items = { new("Guide.VehicleMaintenance.Description") }
            },

            new GuideSection
            {
                Id = "guide-vmaint-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.Name", "Guide.VehicleMaintenance.Field.Vehicle"),
                    new("Field.Description", "Guide.VehicleMaintenance.Field.Description"),
                    new("Field.Remarks", "Guide.VehicleMaintenance.Field.Remarks")
                }
            },
        }
    };
}
