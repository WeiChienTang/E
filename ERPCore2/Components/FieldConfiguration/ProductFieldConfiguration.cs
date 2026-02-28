using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Data.Entities;
using ERPCore2.Services;
using ERPCore2.Helpers;
using Microsoft.AspNetCore.Components;
using ERPCore2.Components.Shared.Modal;
using ERPCore2.Components.Shared.Table;
using ERPCore2.Components.Shared.Page;
using ERPCore2.Components.Shared.Statistics;
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
                            DisplayName = Dn("Field.ProductCode", "商品編號"),
                            FilterPlaceholder = Fp("Field.ProductCode", "輸入商品編號搜尋"),
                            TableOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Product.Code), p => p.Code)
                        }
                    },
                    {
                        nameof(Product.Name),
                        new FieldDefinition<Product>
                        {
                            PropertyName = nameof(Product.Name),
                            DisplayName = Dn("Field.ProductName", "商品名稱"),
                            FilterPlaceholder = Fp("Field.ProductName", "輸入商品名稱搜尋"),
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
                            DisplayName = Dn("Field.Barcode", "條碼編號"),
                            FilterPlaceholder = Fp("Field.Barcode", "輸入條碼編號搜尋"),
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
                            DisplayName = Dn("Field.Size", "尺寸"),
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
                            DisplayName = Dn("Field.CategoryName", "商品分類"),
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
                            DisplayName = Dn("Field.PurchaseUnit", "採購單位"),
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
                        nameof(Product.ProductionUnitId),
                        new FieldDefinition<Product>
                        {
                            PropertyName = "ProductionUnit.Name", // 表格顯示用
                            FilterPropertyName = nameof(Product.ProductionUnitId), // 篩選器用
                            DisplayName = Dn("Field.ProductionUnit", "製程單位"),
                            FilterType = SearchFilterType.Select,
                            TableOrder = 7,
                            Options = _units.Select(u => new SelectOption 
                            { 
                                Text = u.Name, 
                                Value = u.Id.ToString() 
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(Product.ProductionUnitId), p => p.ProductionUnitId)
                        }
                    },
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
        
    }
}


