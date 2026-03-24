using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

public static class CompanyGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-comp-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items =
                {
                    new("Guide.Company.Description")
                }
            },

            new GuideSection
            {
                Id = "guide-comp-tabs",
                TitleKey = "Guide.TabDescriptions",
                Icon = "bi-folder2-open",
                BookmarkLabel = "分頁",
                BookmarkColor = "#8B5CF6",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Tab.CompanyData", "Guide.Company.Tab.CompanyData"),
                    new("Tab.CompanyLogo", "Guide.Company.Tab.Logo"),
                    new("Tab.BankAccounts", "Guide.Company.Tab.BankAccounts")
                }
            },

            new GuideSection
            {
                Id = "guide-comp-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.Code", "Guide.Company.Field.Code"),
                    new("Field.CompanyName", "Guide.Company.Field.Name"),
                    new("Field.ShortName", "Guide.Company.Field.ShortName"),
                    new("Field.TaxNumber", "Guide.Company.Field.TaxId"),
                    new("Field.ResponsiblePerson", "Guide.Company.Field.Representative"),
                    new("Field.CompanyAddress", "Guide.Company.Field.Address"),
                    new("Field.Phone", "Guide.Company.Field.Phone"),
                    new("Field.Email", "Guide.Company.Field.Email"),
                    new("Field.Website", "Guide.Company.Field.Website"),
                    new("Field.IsDefaultCompany", "Guide.Company.Field.IsDefault")
                }
            },

            new GuideSection
            {
                Id = "guide-comp-tips",
                TitleKey = "Guide.Tips",
                Icon = "bi-lightbulb",
                BookmarkLabel = "提示",
                BookmarkColor = "#06B6D4",
                Type = GuideSectionType.Tips,
                Items =
                {
                    new("Guide.Company.Tip1", GuideItemStyle.Tip)
                }
            },
        }
    };
}
