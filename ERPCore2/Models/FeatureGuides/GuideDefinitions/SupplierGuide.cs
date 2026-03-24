using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

public static class SupplierGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-supp-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items =
                {
                    new("Guide.Supplier.Description")
                }
            },

            new GuideSection
            {
                Id = "guide-supp-tabs",
                TitleKey = "Guide.TabDescriptions",
                Icon = "bi-folder2-open",
                BookmarkLabel = "分頁",
                BookmarkColor = "#8B5CF6",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Tab.SupplierData", "Guide.Supplier.Tab.SupplierData"),
                    new("Tab.VehicleInfo", "Guide.Supplier.Tab.VehicleInfo"),
                    new("Tab.AccountingInfo", "Guide.Supplier.Tab.Accounting"),
                    new("Tab.BankAccounts", "Guide.Supplier.Tab.BankAccounts"),
                    new("Tab.SupplierItems", "Guide.Supplier.Tab.Items"),
                    new("Tab.VisitRecords", "Guide.Supplier.Tab.Visits"),
                    new("Tab.TransactionRecords", "Guide.Supplier.Tab.Transactions"),
                    new("Tab.BusinessCards", "Guide.Supplier.Tab.BusinessCards")
                }
            },

            new GuideSection
            {
                Id = "guide-supp-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.SupplierCode", "Guide.Supplier.Field.Code"),
                    new("Field.SupplierName", "Guide.Supplier.Field.Name"),
                    new("Field.ContactPerson", "Guide.Supplier.Field.Contact"),
                    new("Field.ContactPhone", "Guide.Supplier.Field.Phone"),
                    new("Field.Email", "Guide.Supplier.Field.Email"),
                    new("Field.ContactAddress", "Guide.Supplier.Field.Address"),
                    new("Field.TaxNumber", "Guide.Supplier.Field.TaxNumber"),
                    new("Field.PaymentTerms", "Guide.Supplier.Field.PaymentTerms"),
                    new("Field.Remarks", "Guide.Supplier.Field.Remarks")
                }
            },

            new GuideSection
            {
                Id = "guide-supp-tips",
                TitleKey = "Guide.Tips",
                Icon = "bi-lightbulb",
                BookmarkLabel = "提示",
                BookmarkColor = "#06B6D4",
                Type = GuideSectionType.Tips,
                Items =
                {
                    new("Guide.Supplier.Tip1", GuideItemStyle.Tip)
                }
            },

            new GuideSection
            {
                Id = "guide-supp-faq",
                TitleKey = "Guide.SalesOrder.FaqTitle",
                Icon = "bi-question-diamond",
                BookmarkLabel = "FAQ",
                BookmarkColor = "#6366F1",
                Type = GuideSectionType.FAQ,
                Items =
                {
                    new("Guide.Supplier.Faq1Q", "Guide.Supplier.Faq1A")
                }
            },
        }
    };
}
