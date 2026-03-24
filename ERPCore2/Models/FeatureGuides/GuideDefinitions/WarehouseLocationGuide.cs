using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

/// <summary>
/// WarehouseLocation 功能說明定義
/// </summary>
public static class WarehouseLocationGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-wl-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items =
                {
                    new("Guide.WarehouseLocation.Description"),
                }
            },

            new GuideSection
            {
                Id = "guide-wl-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.Code", "Guide.WarehouseLocation.Field.Code"),
                    new("Field.Name", "Guide.WarehouseLocation.Field.Name"),
                    new("Field.Warehouse", "Guide.WarehouseLocation.Field.Warehouse"),
                    new("Field.Zone", "Guide.WarehouseLocation.Field.Zone"),
                    new("Field.Aisle", "Guide.WarehouseLocation.Field.Aisle"),
                    new("Field.Level", "Guide.WarehouseLocation.Field.Level"),
                    new("Field.Position", "Guide.WarehouseLocation.Field.Position"),
                    new("Field.MaxCapacity", "Guide.WarehouseLocation.Field.MaxCapacity"),
                    new("Field.Remarks", "Guide.WarehouseLocation.Field.Remarks"),
                }
            },
        }
    };
}
