using ERPCore2.Components.Shared.Forms;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;
using ERPCore2.Helpers;
using Microsoft.AspNetCore.Components;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 商品欄位配置
    /// </summary>
    public class ProductFieldConfiguration : BaseFieldConfiguration<Product>
    {
        private readonly List<ProductCategory> _productCategories;
        private readonly List<Supplier> _suppliers;
        private readonly List<Unit> _units;
        private readonly List<Size> _sizes;
        private readonly INotificationService? _notificationService;
        
        public ProductFieldConfiguration(
            List<ProductCategory> productCategories, 
            List<Supplier> suppliers,
            List<Unit> units,
            List<Size> sizes,
            INotificationService? notificationService = null)
        {
            _productCategories = productCategories;
            _suppliers = suppliers;
            _units = units;
            _sizes = sizes;
            _notificationService = notificationService;
        }
        
        public override Dictionary<string, FieldDefinition<Product>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<Product>>
                {
                    {
                        nameof(Product.Code),
                        new FieldDefinition<Product>
                        {
                            PropertyName = nameof(Product.Code),
                            DisplayName = "商品代碼",
                            FilterPlaceholder = "輸入商品代碼搜尋",
                            TableOrder = 1,
                            HeaderStyle = "width: 180px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Product.Code), p => p.Code)
                        }
                    },
                    {
                        nameof(Product.Name),
                        new FieldDefinition<Product>
                        {
                            PropertyName = nameof(Product.Name),
                            DisplayName = "商品名稱",
                            FilterPlaceholder = "輸入商品名稱搜尋",
                            TableOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Product.Name), p => p.Name)
                        }
                    },
                    {
                        nameof(Product.Barcode),
                        new FieldDefinition<Product>
                        {
                            PropertyName = nameof(Product.Barcode),
                            DisplayName = "條碼編號",
                            FilterPlaceholder = "輸入條碼編號搜尋",
                            TableOrder = 3,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Product.Barcode), p => p.Barcode)
                        }
                    },
                    {
                        nameof(Product.SizeId),
                        new FieldDefinition<Product>
                        {
                            PropertyName = "Size.Name", // 表格顯示用
                            FilterPropertyName = nameof(Product.SizeId), // 篩選器用
                            DisplayName = "尺寸",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 4,
                            Options = _sizes.Select(s => new SelectOption 
                            { 
                                Text = s.Name, 
                                Value = s.Id.ToString() 
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(Product.SizeId), p => p.SizeId)
                        }
                    },
                    {
                        nameof(Product.ProductCategoryId),
                        new FieldDefinition<Product>
                        {
                            PropertyName = "ProductCategory.Name", // 表格顯示用
                            FilterPropertyName = nameof(Product.ProductCategoryId), // 篩選器用
                            DisplayName = "商品分類",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 5,
                            Options = _productCategories.Select(pc => new SelectOption 
                            { 
                                Text = pc.Name, 
                                Value = pc.Id.ToString() 
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(Product.ProductCategoryId), p => p.ProductCategoryId)
                        }
                    },
                    {
                        nameof(Product.UnitId),
                        new FieldDefinition<Product>
                        {
                            PropertyName = "Unit.Name", // 表格顯示用
                            FilterPropertyName = nameof(Product.UnitId), // 篩選器用
                            DisplayName = "單位",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 6,
                            Options = _units.Select(u => new SelectOption 
                            { 
                                Text = u.Name, 
                                Value = u.Id.ToString() 
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(Product.UnitId), p => p.UnitId)
                        }
                    },
                    {
                        nameof(Product.ProcurementType),
                        new FieldDefinition<Product>
                        {
                            PropertyName = nameof(Product.ProcurementType),
                            DisplayName = "採購類型",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 7,
                            Options = Enum.GetValues(typeof(ProcurementType))
                                .Cast<ProcurementType>()
                                .Select(e => new SelectOption
                                {
                                    Text = GetProcurementTypeDisplayName(e),
                                    Value = ((int)e).ToString()
                                })
                                .ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyIntIdFilter(
                                model, query, nameof(Product.ProcurementType), p => (int)p.ProcurementType),
                            CustomTemplate = new RenderFragment<object>(data => builder =>
                            {
                                if (data is Product product)
                                {
                                    var type = product.ProcurementType;
                                    var displayName = GetProcurementTypeDisplayName(type);
                                    var badgeClass = type switch
                                    {
                                        ProcurementType.Purchased => "bg-secondary",
                                        ProcurementType.Manufactured => "bg-primary",
                                        ProcurementType.Outsourced => "bg-info",
                                        _ => "bg-secondary"
                                    };
                                    
                                    builder.OpenElement(0, "span");
                                    builder.AddAttribute(1, "class", $"badge {badgeClass}");
                                    builder.AddContent(2, displayName);
                                    builder.CloseElement();
                                }
                            })
                        }
                    }

                };
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "商品欄位配置初始化失敗");
                });

                // 通知使用者
                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("商品欄位配置初始化失敗，已使用預設配置");
                    });
                }
                
                // 回傳安全的預設配置
                return new Dictionary<string, FieldDefinition<Product>>();
            }
        }
        
        /// <summary>
        /// 覆寫預設排序 - 按商品名稱升序排列（與 Service 層一致）
        /// </summary>
        protected override Func<IQueryable<Product>, IQueryable<Product>> GetDefaultSort()
        {
            return query => query.OrderBy(p => p.Name);
        }
        
        /// <summary>
        /// 取得採購類型的顯示名稱
        /// </summary>
        private static string GetProcurementTypeDisplayName(ProcurementType procurementType)
        {
            return procurementType switch
            {
                ProcurementType.Purchased => "外購",
                ProcurementType.Manufactured => "自製",
                ProcurementType.Outsourced => "委外",
                _ => procurementType.ToString()
            };
        }
    }
}
