using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

public static class CrmLeadGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-crm-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items =
                {
                    new("Guide.CrmLead.Description")
                }
            },

            new GuideSection
            {
                Id = "guide-crm-tabs",
                TitleKey = "Guide.TabDescriptions",
                Icon = "bi-folder2-open",
                BookmarkLabel = "分頁",
                BookmarkColor = "#8B5CF6",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Tab.LeadInfo", "Guide.CrmLead.Tab.LeadInfo"),
                    new("Tab.FollowUpRecords", "Guide.CrmLead.Tab.FollowUp")
                }
            },

            new GuideSection
            {
                Id = "guide-crm-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.CompanyName", "Guide.CrmLead.Field.CompanyName"),
                    new("Field.ContactPerson", "Guide.CrmLead.Field.Contact"),
                    new("Field.ContactPhone", "Guide.CrmLead.Field.Phone"),
                    new("Field.Email", "Guide.CrmLead.Field.Email"),
                    new("Field.Industry", "Guide.CrmLead.Field.Industry"),
                    new("Field.LeadSource", "Guide.CrmLead.Field.Source"),
                    new("Field.LeadStage", "Guide.CrmLead.Field.Stage"),
                    new("Field.AssignedEmployee", "Guide.CrmLead.Field.Assigned"),
                    new("Field.EstimatedValue", "Guide.CrmLead.Field.Value"),
                    new("Field.Remarks", "Guide.CrmLead.Field.Remarks")
                }
            },

            new GuideSection
            {
                Id = "guide-crm-actions",
                TitleKey = "Guide.SalesOrder.ActionsTitle",
                Icon = "bi-gear",
                BookmarkLabel = "功能",
                BookmarkColor = "#D946EF",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Guide.CrmLead.Action.ConvertLabel", "Guide.CrmLead.Action.Convert")
                }
            },

            new GuideSection
            {
                Id = "guide-crm-tips",
                TitleKey = "Guide.Tips",
                Icon = "bi-lightbulb",
                BookmarkLabel = "提示",
                BookmarkColor = "#06B6D4",
                Type = GuideSectionType.Tips,
                Items =
                {
                    new("Guide.CrmLead.Tip1", GuideItemStyle.Tip),
                    new("Guide.CrmLead.Warning1", GuideItemStyle.Warning)
                }
            },
        }
    };
}
