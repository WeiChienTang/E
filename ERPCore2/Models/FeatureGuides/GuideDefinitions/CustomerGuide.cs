using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

public static class CustomerGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-cust-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items =
                {
                    new("Guide.Customer.Description")
                }
            },

            new GuideSection
            {
                Id = "guide-cust-tabs",
                TitleKey = "Guide.TabDescriptions",
                Icon = "bi-folder2-open",
                BookmarkLabel = "分頁",
                BookmarkColor = "#8B5CF6",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Tab.CustomerData", "Guide.Customer.Tab.CustomerData"),
                    new("Tab.VehicleInfo", "Guide.Customer.Tab.VehicleInfo"),
                    new("Tab.AccountingInfo", "Guide.Customer.Tab.Accounting"),
                    new("Tab.BankAccounts", "Guide.Customer.Tab.BankAccounts"),
                    new("Tab.CustomerItems", "Guide.Customer.Tab.Items"),
                    new("Tab.VisitRecords", "Guide.Customer.Tab.Visits"),
                    new("Tab.ComplaintRecords", "Guide.Customer.Tab.Complaints"),
                    new("Tab.TransactionRecords", "Guide.Customer.Tab.Transactions"),
                    new("Tab.BusinessCards", "Guide.Customer.Tab.BusinessCards")
                }
            },

            new GuideSection
            {
                Id = "guide-cust-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.CustomerCode", "Guide.Customer.Field.Code"),
                    new("Field.CompanyName", "Guide.Customer.Field.Name"),
                    new("Field.CustomerType", "Guide.Customer.Field.Type"),
                    new("Field.ContactPerson", "Guide.Customer.Field.Contact"),
                    new("Field.ContactPhone", "Guide.Customer.Field.Phone"),
                    new("Field.Email", "Guide.Customer.Field.Email"),
                    new("Field.ContactAddress", "Guide.Customer.Field.Address"),
                    new("Field.TaxNumber", "Guide.Customer.Field.TaxNumber"),
                    new("Field.SalesManager", "Guide.Customer.Field.SalesManager"),
                    new("Field.PaymentTerms", "Guide.Customer.Field.PaymentTerms"),
                    new("Field.Remarks", "Guide.Customer.Field.Remarks")
                }
            },

            new GuideSection
            {
                Id = "guide-cust-tips",
                TitleKey = "Guide.Tips",
                Icon = "bi-lightbulb",
                BookmarkLabel = "提示",
                BookmarkColor = "#06B6D4",
                Type = GuideSectionType.Tips,
                Items =
                {
                    new("Guide.Customer.Tip1", GuideItemStyle.Tip),
                    new("Guide.Customer.Warning1", GuideItemStyle.Warning)
                }
            },

            new GuideSection
            {
                Id = "guide-cust-faq",
                TitleKey = "Guide.SalesOrder.FaqTitle",
                Icon = "bi-question-diamond",
                BookmarkLabel = "FAQ",
                BookmarkColor = "#6366F1",
                Type = GuideSectionType.FAQ,
                Items =
                {
                    new("Guide.Customer.Faq1Q", "Guide.Customer.Faq1A")
                }
            },
        }
    };
}
