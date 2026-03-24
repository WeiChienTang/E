using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

public static class EmployeeTrainingRecordGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-etr-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items = { new("Guide.EmployeeTrainingRecord.Description") }
            },

            new GuideSection
            {
                Id = "guide-etr-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.CourseName", "Guide.EmployeeTrainingRecord.Field.Course"),
                    new("Field.TrainingDate", "Guide.EmployeeTrainingRecord.Field.Date"),
                    new("Field.TrainingHours", "Guide.EmployeeTrainingRecord.Field.Hours"),
                    new("Field.TrainingOrganization", "Guide.EmployeeTrainingRecord.Field.Organization")
                }
            },
        }
    };
}
