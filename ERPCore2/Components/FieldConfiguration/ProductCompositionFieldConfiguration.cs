using ERPCore2.Components.Shared.Forms;
using ERPCore2.Data.Entities;
using ERPCore2.Services;
using ERPCore2.Helpers;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 商品合成欄位配置
    /// </summary>
    public class ProductCompositionFieldConfiguration : BaseFieldConfiguration<ProductComposition>
    {
        private readonly List<Product> _products;
        private readonly List<CompositionCategory> _compositionCategories;
        private readonly INotificationService? _notificationService;
        
        public ProductCompositionFieldConfiguration(
            List<Product> products,
            List<CompositionCategory> compositionCategories,
            INotificationService? notificationService = null)
        {
            _products = products;
            _compositionCategories = compositionCategories;
            _notificationService = notificationService;
        }
        
        public override Dictionary<string, FieldDefinition<ProductComposition>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<ProductComposition>>
                {
                    {
                        nameof(ProductComposition.Code),
                        new FieldDefinition<ProductComposition>
                        {
                            PropertyName = nameof(ProductComposition.Code),
                            DisplayName = "配方代碼",
                            FilterPlaceholder = "輸入配方代碼搜尋",
                            TableOrder = 1,
                            HeaderStyle = "width: 150px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(ProductComposition.Code), pc => pc.Code)
                        }
                    },
                    {
                        nameof(ProductComposition.ParentProductId),
                        new FieldDefinition<ProductComposition>
                        {
                            PropertyName = "ParentProduct.Name",
                            FilterPropertyName = nameof(ProductComposition.ParentProductId),
                            DisplayName = "成品",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 2,
                            HeaderStyle = "width: 200px;",
                            Options = _products.Select(p => new SelectOption 
                            { 
                                Text = $"{p.Code} - {p.Name}", 
                                Value = p.Id.ToString() 
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyIntIdFilter(
                                model, query, nameof(ProductComposition.ParentProductId), pc => pc.ParentProductId),
                            CustomTemplate = item => builder =>
                            {
                                var composition = (ProductComposition)item;
                                if (composition.ParentProduct != null)
                                {
                                    builder.OpenElement(0, "div");
                                    builder.AddContent(1, composition.ParentProduct.Name);
                                    if (!string.IsNullOrWhiteSpace(composition.ParentProduct.Code))
                                    {
                                        builder.OpenElement(2, "small");
                                        builder.AddAttribute(3, "class", "text-muted ms-1");
                                        builder.AddContent(4, $"({composition.ParentProduct.Code})");
                                        builder.CloseElement();
                                    }
                                    builder.CloseElement();
                                }
                            }
                        }
                    },
                    {
                        nameof(ProductComposition.Specification),
                        new FieldDefinition<ProductComposition>
                        {
                            PropertyName = nameof(ProductComposition.Specification),
                            DisplayName = "規格",
                            FilterPlaceholder = "輸入規格搜尋",
                            TableOrder = 3,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(ProductComposition.Specification), pc => pc.Specification)
                        }
                    },
                    {
                        nameof(ProductComposition.CompositionCategoryId),
                        new FieldDefinition<ProductComposition>
                        {
                            PropertyName = "CompositionCategory.Name",
                            FilterPropertyName = nameof(ProductComposition.CompositionCategoryId),
                            DisplayName = "配方類型",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 4,
                            HeaderStyle = "width: 150px;",
                            Options = _compositionCategories.Select(cc => new SelectOption
                            {
                                Text = cc.Name,
                                Value = cc.Id.ToString()
                            }).ToList(),
                            FilterFunction = (model, query) =>
                            {
                                var filterValue = model.GetFilterValue(nameof(ProductComposition.CompositionCategoryId));
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
                                var composition = (ProductComposition)item;
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
                        new FieldDefinition<ProductComposition>
                        {
                            PropertyName = "ComponentCount",
                            DisplayName = "組件數",
                            ShowInFilter = false,
                            TableOrder = 5,
                            HeaderStyle = "width: 80px; text-align: center;",
                            CustomTemplate = item => builder =>
                            {
                                var composition = (ProductComposition)item;
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
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "初始化商品合成欄位配置失敗");
                });

                // 通知使用者
                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("初始化商品合成欄位配置時發生錯誤，已使用預設配置");
                    });
                }

                // 回傳空的配置，讓頁面使用預設行為
                return new Dictionary<string, FieldDefinition<ProductComposition>>();
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
