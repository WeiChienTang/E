using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

public static class CustomerBankAccountGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-cba-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items = { new("Guide.CustomerBankAccount.Description") }
            },

            new GuideSection
            {
                Id = "guide-cba-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.BankName", "Guide.CustomerBankAccount.Field.Bank"),
                    new("Field.Code", "Guide.CustomerBankAccount.Field.AccountNumber"),
                    new("Field.Name", "Guide.CustomerBankAccount.Field.AccountName"),
                    new("Field.Remarks", "Guide.CustomerBankAccount.Field.Remarks")
                }
            },
        }
    };
}
