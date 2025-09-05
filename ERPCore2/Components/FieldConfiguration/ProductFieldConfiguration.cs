using ERPCore2.Components.Shared.Forms;
using ERPCore2.Data.Entities;
using ERPCore2.Services;
using ERPCore2.Helpers;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 產品欄位配置
    /// </summary>
    public class ProductFieldConfiguration : BaseFieldConfiguration<Product>
    {
        private readonly List<ProductCategory> _productCategories;
        private readonly List<Supplier> _suppliers;
        private readonly List<Unit> _units;
        private readonly List<Size> _sizes;
        private readonly List<Warehouse> _warehouses;
        private readonly List<WarehouseLocation> _warehouseLocations;
        private readonly INotificationService? _notificationService;
        
        public ProductFieldConfiguration(
            List<ProductCategory> productCategories, 
            List<Supplier> suppliers,
            List<Unit> units,
            List<Size> sizes,
            List<Warehouse> warehouses,
            List<WarehouseLocation> warehouseLocations,
            INotificationService? notificationService = null)
        {
            _productCategories = productCategories;
            _suppliers = suppliers;
            _units = units;
            _sizes = sizes;
            _warehouses = warehouses;
            _warehouseLocations = warehouseLocations;
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
                            DisplayName = "產品代碼",
                            FilterPlaceholder = "輸入產品代碼搜尋",
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
                            DisplayName = "產品名稱",
                            FilterPlaceholder = "輸入產品名稱搜尋",
                            TableOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Product.Name), p => p.Name)
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
                            TableOrder = 3,
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
                            DisplayName = "產品分類",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 4,
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
                            TableOrder = 5,
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
                        nameof(Product.WarehouseId),
                        new FieldDefinition<Product>
                        {
                            PropertyName = "Warehouse.Name", // 表格顯示用
                            FilterPropertyName = nameof(Product.WarehouseId), // 篩選器用
                            DisplayName = "倉庫",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 6,
                            Options = _warehouses.Select(w => new SelectOption 
                            { 
                                Text = w.Name, 
                                Value = w.Id.ToString() 
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(Product.WarehouseId), p => p.WarehouseId)
                        }
                    },
                    {
                        nameof(Product.WarehouseLocationId),
                        new FieldDefinition<Product>
                        {
                            PropertyName = "WarehouseLocation.Name", // 表格顯示用
                            FilterPropertyName = nameof(Product.WarehouseLocationId), // 篩選器用
                            DisplayName = "倉庫位置",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 7,
                            Options = _warehouseLocations.Select(wl => new SelectOption 
                            { 
                                Text = wl.Name, 
                                Value = wl.Id.ToString() 
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(Product.WarehouseLocationId), p => p.WarehouseLocationId)
                        }
                    },

                };
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "產品欄位配置初始化失敗");
                });

                // 通知使用者
                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("產品欄位配置初始化失敗，已使用預設配置");
                    });
                }
                
                // 回傳安全的預設配置
                return new Dictionary<string, FieldDefinition<Product>>();
            }
        }
    }
}
