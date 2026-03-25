using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Data.Entities;
using ERPCore2.Services;
using ERPCore2.Helpers;
using ERPCore2.Components.Shared.Modal;
using ERPCore2.Components.Shared.Table;
using ERPCore2.Components.Shared.Page;
using ERPCore2.Components.Shared.Statistics;
namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 品項合成欄位配置
    /// </summary>
    public class ItemCompositionFieldConfiguration : BaseFieldConfiguration<ItemComposition>
    {
        private readonly List<Item> _products;
        private readonly List<CompositionCategory> _compositionCategories;
        private readonly INotificationService? _notificationService;
        
        public ItemCompositionFieldConfiguration(
            List<Item> products,
            List<CompositionCategory> compositionCategories,
            INotificationService? notificationService = null)
        {
            _products = products;
            _compositionCategories = compositionCategories;
            _notificationService = notificationService;
        }
        
        public override Dictionary<string, FieldDefinition<ItemComposition>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<ItemComposition>>
                {
                    {
                        nameof(ItemComposition.Code),
                        new FieldDefinition<ItemComposition>
                        {
                            PropertyName = nameof(ItemComposition.Code),
                            DisplayName = Dn("Field.CompositionCode", "配方編號"),
                            FilterPlaceholder = Fp("Field.CompositionCode", "輸入配方編號搜尋"),
                            TableOrder = 1,
                            Width = "120px",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(ItemComposition.Code), pc => pc.Code)
                        }
                    },
                    {
                        nameof(ItemComposition.Name),
                        new FieldDefinition<ItemComposition>
                        {
                            PropertyName = nameof(ItemComposition.Name),
                            DisplayName = Dn("Field.CompositionName", "清單名稱"),
                            FilterPlaceholder = Fp("Field.CompositionName", "輸入清單名稱搜尋"),
                            TableOrder = 2,
                            Width = "150px",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(ItemComposition.Name), pc => pc.Name)
                        }
                    },
                    {
                        nameof(ItemComposition.ParentItemId),
                        new FieldDefinition<ItemComposition>
                        {
                            PropertyName = "ParentItem.Name",
                            FilterPropertyName = nameof(ItemComposition.ParentItemId),
                            DisplayName = Dn("Entity.Item", "品項"),
                            FilterType = SearchFilterType.Select,
                            TableOrder = 3,
                            Width = "130px",
                            Options = _products.Select(p => new SelectOption 
                            { 
                                Text = $"{p.Name}", 
                                Value = p.Id.ToString() 
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyIntIdFilter(
                                model, query, nameof(ItemComposition.ParentItemId), pc => pc.ParentItemId ?? 0),
                            CustomTemplate = item => builder =>
                            {
                                var composition = (ItemComposition)item;
                                if (composition.ParentItem != null)
                                {
                                    builder.OpenElement(0, "div");
                                    builder.AddContent(1, composition.ParentItem.Name);
                                    if (!string.IsNullOrWhiteSpace(composition.ParentItem.Code))
                                    {
                                        builder.OpenElement(2, "small");
                                        builder.AddAttribute(3, "class", "text-muted ms-1");
                                        builder.AddContent(4, $"({composition.ParentItem.Code})");
                                        builder.CloseElement();
                                    }
                                    builder.CloseElement();
                                }
                            }
                        }
                    },
                    {
                        nameof(ItemComposition.Specification),
                        new FieldDefinition<ItemComposition>
                        {
                            PropertyName = nameof(ItemComposition.Specification),
                            DisplayName = Dn("Field.Spec", "規格"),
                            FilterPlaceholder = Fp("Field.Spec", "輸入規格搜尋"),
                            TableOrder = 4,
                            Width = "120px",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(ItemComposition.Specification), pc => pc.Specification)
                        }
                    },
                    {
                        nameof(ItemComposition.CompositionCategoryId),
                        new FieldDefinition<ItemComposition>
                        {
                            PropertyName = "CompositionCategory.Name",
                            FilterPropertyName = nameof(ItemComposition.CompositionCategoryId),
                            DisplayName = Dn("Field.CompositionType", "配方類型"),
                            FilterType = SearchFilterType.Select,
                            TableOrder = 5,
                            Width = "110px",
                            Options = _compositionCategories.Select(cc => new SelectOption
                            {
                                Text = cc.Name,
                                Value = cc.Id.ToString()
                            }).ToList(),
                            FilterFunction = (model, query) =>
                            {
                                var filterValue = model.GetFilterValue(nameof(ItemComposition.CompositionCategoryId));
                                if (!string.IsNullOrWhiteSpace(filterValue?.ToString()))
                                {
                                    if (int.TryParse(filterValue.ToString(), out int categoryId))
                                    {
                                        query = query.Where(pc => pc.CompositionCategoryId == categoryId);
                                    }
                                }
                                return query;
                            },
                            CustomTemplate = item => builder =>
                            {
                                var composition = (ItemComposition)item;
                                if (composition.CompositionCategory != null)
                                {
                                    var badgeClass = GetBadgeClassByName(composition.CompositionCategory.Name);
                                    builder.OpenElement(0, "span");
                                    builder.AddAttribute(1, "class", $"badge {badgeClass}");
                                    builder.AddContent(2, composition.CompositionCategory.Name);
                                    builder.CloseElement();
                                }
                            }
                        }
                    },
                    {
                        "ComponentCount",
                        new FieldDefinition<ItemComposition>
                        {
                            PropertyName = "ComponentCount",
                            DisplayName = Dn("Field.ComponentCount", "組件數"),
                            ShowInFilter = false,
                            TableOrder = 6,
                            Width = "90px",
                            CustomTemplate = item => builder =>
                            {
                                var composition = (ItemComposition)item;
                                builder.OpenElement(0, "div");
                                builder.AddAttribute(1, "class", "text-center");
                                builder.OpenElement(2, "span");
                                builder.AddAttribute(3, "class", "badge bg-secondary");
                                builder.AddContent(4, composition.CompositionDetails?.Count ?? 0);
                                builder.CloseElement();
                                builder.CloseElement();
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
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "初始化品項合成欄位配置失敗");
                });

                // 通知使用者
                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("初始化品項合成欄位配置時發生錯誤，已使用預設配置");
                    });
                }

                // 回傳空的配置，讓頁面使用預設行為
                return new Dictionary<string, FieldDefinition<ItemComposition>>();
            }
        }

        /// <summary>
        /// 根據名稱取得 Badge 樣式
        /// </summary>
        private static string GetBadgeClassByName(string name)
        {
            return name switch
            {
                "標準配方" => "bg-primary",
                "替代配方" => "bg-warning",
                "簡化配方" => "bg-info",
                "客製配方" => "bg-secondary",
                _ => "bg-secondary"
            };
        }
    }
}


