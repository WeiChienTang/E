using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

/// <summary>
/// Bank 功能說明定義
/// </summary>
public static class BankGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-bank-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items =
                {
                    new("Guide.Bank.Description"),
                }
            },

            new GuideSection
            {
                Id = "guide-bank-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.Code", "Guide.Bank.Field.Code"),
                    new("Field.BankName", "Guide.Bank.Field.BankName"),
                    new("Field.BankNameEn", "Guide.Bank.Field.BankNameEn"),
                    new("Field.SwiftCode", "Guide.Bank.Field.SwiftCode"),
                    new("Field.Remarks", "Guide.Bank.Field.Remarks"),
                }
            },
        }
    };
}
