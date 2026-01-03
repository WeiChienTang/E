using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Data.Entities;
using ERPCore2.Services;
using ERPCore2.Helpers;
using Microsoft.AspNetCore.Components;
using ERPCore2.Components.Shared.PageTemplate;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 庫存欄位配置
    /// </summary>
    public class InventoryStockFieldConfiguration : BaseFieldConfiguration<InventoryStock>
    {
        private readonly List<Product> _products;
        private readonly List<Warehouse> _warehouses;
        private readonly List<WarehouseLocation> _warehouseLocations;
        private readonly List<ProductCategory> _productCategories;
        private readonly INotificationService? _notificationService;

        public InventoryStockFieldConfiguration(
            List<Product> products, 
            List<Warehouse> warehouses, 
            List<WarehouseLocation> warehouseLocations,
            List<ProductCategory> productCategories,
            INotificationService? notificationService = null)
        {
            _products = products;
            _warehouses = warehouses;
            _warehouseLocations = warehouseLocations;
            _productCategories = productCategories;
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<InventoryStock>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<InventoryStock>>
                {
                    {
                        "ProductCode",
                        new FieldDefinition<InventoryStock>
                        {
                            PropertyName = "Product.Code",
                            DisplayName = "商品編號",
                            FilterType = SearchFilterType.Text,
                            FilterPlaceholder = "輸入商品編號搜尋",
                            TableOrder = 1,
                            HeaderStyle = "width: 150px;",
                            FilterFunction = (model, query) => {
                                var filterValue = model.GetFilterValue("ProductCode")?.ToString();
                                if (!string.IsNullOrWhiteSpace(filterValue))
                                {
                                    // 使用安全的方式查詢，避免導航屬性的 null reference
                                    query = query.Where(s => s.Product != null &&
                                                           s.Product.Code != null &&
                                                           s.Product.Code.Contains(filterValue));
                                }
                                return query;
                            }
                        }
                    },
                    {
                        nameof(InventoryStock.ProductId),
                        new FieldDefinition<InventoryStock>
                        {
                            PropertyName = "Product.Name", // 表格顯示用
                            FilterPropertyName = nameof(InventoryStock.ProductId), // 篩選器用
                            DisplayName = "商品名稱",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 2,
                            HeaderStyle = "width: 200px;",
                            Options = _products.Select(p => new SelectOption
                            {
                                Text = $"{p.Code} - {p.Name}",
                                Value = p.Id.ToString()
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(InventoryStock.ProductId), s => s.ProductId)
                        }
                    },
                    {
                        "ProductCategoryId",
                        new FieldDefinition<InventoryStock>
                        {
                            PropertyName = "Product.ProductCategory.Name",
                            FilterPropertyName = "ProductCategoryId",
                            DisplayName = "商品類型",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 3,
                            HeaderStyle = "width: 150px;",
                            Options = _productCategories.Select(pc => new SelectOption
                            {
                                Text = pc.Name,
                                Value = pc.Id.ToString()
                            }).ToList(),
                            FilterFunction = (model, query) => {
                                var filterValue = model.GetFilterValue("ProductCategoryId")?.ToString();
                                if (!string.IsNullOrWhiteSpace(filterValue) && int.TryParse(filterValue, out int categoryId))
                                {
                                    query = query.Where(s => s.Product != null && 
                                                           s.Product.ProductCategoryId == categoryId);
                                }
                                return query;
                            }
                        }
                    },
                        {
                        nameof(InventoryStock.TotalCurrentStock),
                        new FieldDefinition<InventoryStock>
                        {
                            PropertyName = nameof(InventoryStock.TotalCurrentStock),
                            DisplayName = "現有庫存",
                            FilterType = SearchFilterType.Text,
                            TableOrder = 4,
                            HeaderStyle = "width: 100px; text-align: right;",
                            ShowInFilter = false,
                            CustomTemplate = (data) => (RenderFragment)((builder) =>
                            {
                                if (data is InventoryStock stock)
                                {
                                    // 使用 FormatSmart：整數不顯示小數點，有小數才顯示
                                    var value = NumberFormatHelper.FormatSmart(stock.TotalCurrentStock);
                                    builder.AddContent(0, value);
                                }
                            })
                        }
                    },
                    {
                        "ProductUnit",
                        new FieldDefinition<InventoryStock>
                        {
                            PropertyName = "Product.Unit.Name",
                            DisplayName = "商品單位",
                            FilterType = SearchFilterType.Text,
                            TableOrder = 5,
                            HeaderStyle = "width: 80px;",
                            ShowInFilter = false
                        }
                    },
                    {
                        nameof(InventoryStock.WeightedAverageCost),
                        new FieldDefinition<InventoryStock>
                        {
                            PropertyName = nameof(InventoryStock.WeightedAverageCost),
                            DisplayName = "平均成本",
                            FilterType = SearchFilterType.Text,
                            TableOrder = 6,
                            HeaderStyle = "width: 120px; text-align: right;",
                            ShowInFilter = false,
                            CustomTemplate = (data) => (RenderFragment)((builder) =>
                            {
                                if (data is InventoryStock stock)
                                {
                                    // 使用 FormatSmart：整數不顯示小數點，有小數才顯示，null 顯示 "-"
                                    var value = NumberFormatHelper.FormatSmart(
                                        stock.WeightedAverageCost, 
                                        decimalPlaces: 2, 
                                        useThousandsSeparator: true, 
                                        nullDisplayText: "-");
                                    builder.AddContent(0, value);
                                }
                            })
                        }
                    },
                };
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await ErrorHandlingHelper.HandlePageErrorAsync(
                            ex, 
                            nameof(GetFieldDefinitions), 
                            GetType(), 
                            additionalData: "取得庫存欄位定義失敗"
                        );
                        
                        if (_notificationService != null)
                        {
                            await _notificationService.ShowErrorAsync("取得庫存欄位定義失敗");
                        }
                    }
                    catch
                    {
                        // 如果連錯誤處理都失敗，至少要有基本的除錯資訊
                        System.Diagnostics.Debug.WriteLine($"InventoryStockFieldConfiguration.GetFieldDefinitions 發生錯誤: {ex.Message}");
                    }
                });

                // 回傳空的字典以避免系統崩潰
                return new Dictionary<string, FieldDefinition<InventoryStock>>();
            }
        }
    }
}


