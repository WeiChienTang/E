using ERPCore2.Components.Shared.Forms;
using ERPCore2.Data.Entities;
using ERPCore2.Services;
using ERPCore2.Helpers;
using Microsoft.AspNetCore.Components;
using ERPCore2.Data.Enums;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 產品分類欄位配置
    /// </summary>
    public class ProductCategoryFieldConfiguration : BaseFieldConfiguration<ProductCategory>
    {
        private readonly INotificationService? _notificationService;
        
        public ProductCategoryFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }
        
        public override Dictionary<string, FieldDefinition<ProductCategory>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<ProductCategory>>
                {
                    {
                        nameof(ProductCategory.Code),
                        new FieldDefinition<ProductCategory>
                        {
                            PropertyName = nameof(ProductCategory.Code),
                            DisplayName = "分類代碼",
                            FilterPlaceholder = "輸入分類代碼搜尋",
                            TableOrder = 1,
                            HeaderStyle = "width: 180px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(ProductCategory.Code), pc => pc.Code, allowNull: true)
                        }
                    },
                    {
                        nameof(ProductCategory.Name),
                        new FieldDefinition<ProductCategory>
                        {
                            PropertyName = nameof(ProductCategory.Name),
                            DisplayName = "分類名稱",
                            FilterPlaceholder = "輸入分類名稱搜尋",
                            TableOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(ProductCategory.Name), pc => pc.Name)
                        }
                    },
                    {
                        nameof(ProductCategory.IsSaleable),
                        new FieldDefinition<ProductCategory>
                        {
                            PropertyName = nameof(ProductCategory.IsSaleable),
                            DisplayName = "可販售",
                            TableOrder = 3,
                            FilterFunction = (model, query) =>
                            {
                                var filterValue = model.GetFilterValue(nameof(ProductCategory.IsSaleable))?.ToString();
                                if (!string.IsNullOrWhiteSpace(filterValue) && bool.TryParse(filterValue, out var isSaleable))
                                {
                                    query = query.Where(pc => pc.IsSaleable == isSaleable);
                                }
                                return query;
                            },
                            CustomTemplate = (context) =>
                            {
                                var category = context as ProductCategory;
                                if (category == null) 
                                {
                                    return builder => { };
                                }
                                
                                return builder =>
                                {
                                    builder.OpenComponent<ERPCore2.Components.Shared.GenericComponent.Badge.GenericStatusBadgeComponent>(0);
                                    builder.AddAttribute(1, "Status", category.IsSaleable ? EntityStatus.Active : EntityStatus.Inactive);
                                    builder.AddAttribute(2, "CustomText", category.IsSaleable ? "可販售" : "不販售");
                                    builder.CloseComponent();
                                };
                            }
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "產品分類欄位配置初始化失敗");
                });

                // 通知使用者
                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("產品分類欄位配置初始化失敗，已使用預設配置");
                    });
                }
                
                // 回傳安全的預設配置
                return new Dictionary<string, FieldDefinition<ProductCategory>>();
            }
        }
    }
}
