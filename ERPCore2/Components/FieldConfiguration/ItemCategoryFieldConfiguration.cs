using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Data.Entities;
using ERPCore2.Services;
using ERPCore2.Helpers;
using Microsoft.AspNetCore.Components;
using ERPCore2.Models.Enums;
using ERPCore2.Components.Shared.Modal;
using ERPCore2.Components.Shared.Table;
using ERPCore2.Components.Shared.Page;
using ERPCore2.Components.Shared.Statistics;
namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 品項分類欄位配置
    /// </summary>
    public class ItemCategoryFieldConfiguration : BaseFieldConfiguration<ItemCategory>
    {
        private readonly INotificationService? _notificationService;
        
        public ItemCategoryFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }
        
        public override Dictionary<string, FieldDefinition<ItemCategory>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<ItemCategory>>
                {
                    {
                        nameof(ItemCategory.Code),
                        new FieldDefinition<ItemCategory>
                        {
                            PropertyName = nameof(ItemCategory.Code),
                            DisplayName = Dn("Field.CategoryCode", "分類編號"),
                            FilterPlaceholder = Fp("Field.CategoryCode", "輸入分類編號搜尋"),
                            TableOrder = 1,
                            Width = "120px",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(ItemCategory.Code), pc => pc.Code, allowNull: true)
                        }
                    },
                    {
                        nameof(ItemCategory.Name),
                        new FieldDefinition<ItemCategory>
                        {
                            PropertyName = nameof(ItemCategory.Name),
                            DisplayName = Dn("Field.CategoryName", "分類名稱"),
                            FilterPlaceholder = Fp("Field.CategoryName", "輸入分類名稱搜尋"),
                            TableOrder = 2,
                            Width = "150px",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(ItemCategory.Name), pc => pc.Name)
                        }
                    },
                    {
                        nameof(ItemCategory.IsSaleable),
                        new FieldDefinition<ItemCategory>
                        {
                            PropertyName = nameof(ItemCategory.IsSaleable),
                            DisplayName = Dn("Field.ForSale", "可販售"),
                            TableOrder = 3,
                            Width = "90px",
                            FilterFunction = (model, query) =>
                            {
                                var filterValue = model.GetFilterValue(nameof(ItemCategory.IsSaleable))?.ToString();
                                if (!string.IsNullOrWhiteSpace(filterValue) && bool.TryParse(filterValue, out var isSaleable))
                                {
                                    query = query.Where(pc => pc.IsSaleable == isSaleable);
                                }
                                return query;
                            },
                            CustomTemplate = (context) =>
                            {
                                var category = context as ItemCategory;
                                if (category == null) 
                                {
                                    return builder => { };
                                }
                                
                                return builder =>
                                {
                                    builder.OpenComponent<ERPCore2.Components.Shared.UI.Badge.GenericStatusBadgeComponent>(0);
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
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "品項分類欄位配置初始化失敗");
                });

                // 通知使用者
                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("品項分類欄位配置初始化失敗，已使用預設配置");
                    });
                }
                
                // 回傳安全的預設配置
                return new Dictionary<string, FieldDefinition<ItemCategory>>();
            }
        }
    }
}


