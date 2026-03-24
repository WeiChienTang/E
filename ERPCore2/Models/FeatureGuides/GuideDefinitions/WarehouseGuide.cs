using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

/// <summary>
/// Warehouse 功能說明定義
/// </summary>
public static class WarehouseGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-wh-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items =
                {
                    new("Guide.Warehouse.Description"),
                }
            },

            new GuideSection
            {
                Id = "guide-wh-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.Code", "Guide.Warehouse.Field.Code"),
                    new("Field.Name", "Guide.Warehouse.Field.Name"),
                    new("Field.ContactPerson", "Guide.Warehouse.Field.Contact"),
                    new("Field.Phone", "Guide.Warehouse.Field.Phone"),
                    new("Field.Address", "Guide.Warehouse.Field.Address"),
                    new("Field.Remarks", "Guide.Warehouse.Field.Remarks"),
                }
            },

            new GuideSection
            {
                Id = "guide-wh-tips",
                TitleKey = "Guide.Tips",
                Icon = "bi-lightbulb",
                BookmarkLabel = "提示",
                BookmarkColor = "#06B6D4",
                Type = GuideSectionType.Tips,
                Items =
                {
                    new("Guide.Warehouse.Tip1", GuideItemStyle.Tip),
                }
            },
        }
    };
}
