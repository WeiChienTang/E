using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

/// <summary>
/// Currency 功能說明定義
/// </summary>
public static class CurrencyGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-cur-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items =
                {
                    new("Guide.Currency.Description"),
                }
            },

            new GuideSection
            {
                Id = "guide-cur-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.Code", "Guide.Currency.Field.Code"),
                    new("Field.Name", "Guide.Currency.Field.Name"),
                    new("Field.Symbol", "Guide.Currency.Field.Symbol"),
                    new("Field.IsBaseCurrency", "Guide.Currency.Field.IsBase"),
                    new("Field.ExchangeRate", "Guide.Currency.Field.Rate"),
                    new("Field.Remarks", "Guide.Currency.Field.Remarks"),
                }
            },
        }
    };
}
