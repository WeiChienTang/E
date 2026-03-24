using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

public static class VehicleGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-veh-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items = { new("Guide.Vehicle.Description") }
            },

            new GuideSection
            {
                Id = "guide-veh-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.Code", "Guide.Vehicle.Field.Code"),
                    new("Field.Name", "Guide.Vehicle.Field.LicensePlate"),
                    new("Field.VehicleType", "Guide.Vehicle.Field.Type"),
                    new("Field.Description", "Guide.Vehicle.Field.Spec"),
                    new("Field.Remarks", "Guide.Vehicle.Field.Remarks")
                }
            },

            new GuideSection
            {
                Id = "guide-veh-tips",
                TitleKey = "Guide.Tips",
                Icon = "bi-lightbulb",
                BookmarkLabel = "提示",
                BookmarkColor = "#06B6D4",
                Type = GuideSectionType.Tips,
                Items =
                {
                    new("Guide.Vehicle.Tip1", GuideItemStyle.Tip)
                }
            },
        }
    };
}
