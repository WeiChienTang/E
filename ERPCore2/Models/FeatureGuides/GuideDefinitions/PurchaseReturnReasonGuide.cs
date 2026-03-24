using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

/// <summary>
/// PurchaseReturnReason 功能說明定義
/// </summary>
public static class PurchaseReturnReasonGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-prr-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items =
                {
                    new("Guide.PurchaseReturnReason.Description"),
                }
            },

            new GuideSection
            {
                Id = "guide-prr-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.Code", "Guide.PurchaseReturnReason.Field.Code"),
                    new("Field.Name", "Guide.PurchaseReturnReason.Field.Name"),
                    new("Field.Remarks", "Guide.PurchaseReturnReason.Field.Remarks"),
                }
            },
        }
    };
}
