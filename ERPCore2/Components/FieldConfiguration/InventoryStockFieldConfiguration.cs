using ERPCore2.Components.Shared.Forms;
using ERPCore2.Data.Entities;
using ERPCore2.Services;
using ERPCore2.Helpers;
using Microsoft.AspNetCore.Components;

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
        private readonly INotificationService? _notificationService;

        public InventoryStockFieldConfiguration(
            List<Product> products, 
            List<Warehouse> warehouses, 
            List<WarehouseLocation> warehouseLocations,
            INotificationService? notificationService = null)
        {
            _products = products;
            _warehouses = warehouses;
            _warehouseLocations = warehouseLocations;
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<InventoryStock>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<InventoryStock>>
                {
                    {
                        nameof(InventoryStock.ProductId),
                        new FieldDefinition<InventoryStock>
                        {
                            PropertyName = "Product.Name", // 表格顯示用
                            FilterPropertyName = nameof(InventoryStock.ProductId), // 篩選器用
                            DisplayName = "商品",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 1,
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
                        "ProductCode",
                        new FieldDefinition<InventoryStock>
                        {
                            PropertyName = "Product.Code",
                            DisplayName = "商品代碼",
                            FilterType = SearchFilterType.Text,
                            FilterPlaceholder = "輸入商品代碼搜尋",
                            TableOrder = 2,
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
                        nameof(InventoryStock.TotalCurrentStock),
                        new FieldDefinition<InventoryStock>
                        {
                            PropertyName = nameof(InventoryStock.TotalCurrentStock),
                            DisplayName = "現有庫存",
                            FilterType = SearchFilterType.Text,
                            TableOrder = 3,
                            HeaderStyle = "width: 100px; text-align: right;",
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
                            TableOrder = 7,
                            HeaderStyle = "width: 120px; text-align: right;",
                            ShowInFilter = false,
                            CustomTemplate = (data) => (RenderFragment)((builder) =>
                            {
                                if (data is InventoryStock stock)
                                {
                                    var value = stock.WeightedAverageCost.HasValue 
                                        ? stock.WeightedAverageCost.Value.ToString("N2") 
                                        : "-";
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

        protected override Func<IQueryable<InventoryStock>, IOrderedQueryable<InventoryStock>> GetDefaultSort()
        {
            // 按產品ID排序
            return query => query.OrderBy(s => s.ProductId);
        }
    }
}
