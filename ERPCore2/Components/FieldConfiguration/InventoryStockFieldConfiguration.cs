using ERPCore2.Components.Shared.Forms;
using ERPCore2.Data.Entities;
using ERPCore2.Services;
using ERPCore2.Helpers;

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
                            FilterOrder = 1,
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
                            FilterOrder = 2,
                            HeaderStyle = "width: 150px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, "ProductCode", s => s.Product.Code, allowNull: true)
                        }
                    },
                    {
                        nameof(InventoryStock.WarehouseId),
                        new FieldDefinition<InventoryStock>
                        {
                            PropertyName = "Warehouse.Name", // 表格顯示用
                            FilterPropertyName = nameof(InventoryStock.WarehouseId), // 篩選器用
                            DisplayName = "倉庫",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 3,
                            FilterOrder = 3,
                            HeaderStyle = "width: 150px;",
                            Options = _warehouses.Select(w => new SelectOption 
                            { 
                                Text = w.Name, 
                                Value = w.Id.ToString() 
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(InventoryStock.WarehouseId), s => s.WarehouseId)
                        }
                    },
                    {
                        nameof(InventoryStock.WarehouseLocationId),
                        new FieldDefinition<InventoryStock>
                        {
                            PropertyName = "WarehouseLocation.Name", // 表格顯示用
                            FilterPropertyName = nameof(InventoryStock.WarehouseLocationId), // 篩選器用
                            DisplayName = "庫位",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 4,
                            FilterOrder = 4,
                            HeaderStyle = "width: 120px;",
                            Options = _warehouseLocations.Select(wl => new SelectOption 
                            { 
                                Text = $"{wl.Code} - {wl.Name}", 
                                Value = wl.Id.ToString() 
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(InventoryStock.WarehouseLocationId), s => s.WarehouseLocationId)
                        }
                    },
                    {
                        nameof(InventoryStock.CurrentStock),
                        new FieldDefinition<InventoryStock>
                        {
                            PropertyName = nameof(InventoryStock.CurrentStock),
                            DisplayName = "現有庫存",
                            FilterType = SearchFilterType.NumberRange,
                            TableOrder = 5,
                            FilterOrder = 5,
                            HeaderStyle = "width: 100px; text-align: right;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(InventoryStock.CurrentStock), s => s.CurrentStock.ToString())
                        }
                    },
                    {
                        nameof(InventoryStock.ReservedStock),
                        new FieldDefinition<InventoryStock>
                        {
                            PropertyName = nameof(InventoryStock.ReservedStock),
                            DisplayName = "預留庫存",
                            TableOrder = 6,
                            FilterOrder = 0, // 不在篩選器中顯示
                            HeaderStyle = "width: 100px; text-align: right;",
                            ShowInFilter = false
                        }
                    },
                    {
                        nameof(InventoryStock.AvailableStock),
                        new FieldDefinition<InventoryStock>
                        {
                            PropertyName = nameof(InventoryStock.AvailableStock),
                            DisplayName = "可用庫存",
                            TableOrder = 7,
                            FilterOrder = 0, // 不在篩選器中顯示
                            HeaderStyle = "width: 100px; text-align: right;",
                            ShowInFilter = false
                        }
                    },
                    {
                        nameof(InventoryStock.InTransitStock),
                        new FieldDefinition<InventoryStock>
                        {
                            PropertyName = nameof(InventoryStock.InTransitStock),
                            DisplayName = "在途庫存",
                            TableOrder = 8,
                            FilterOrder = 0, // 不在篩選器中顯示
                            HeaderStyle = "width: 100px; text-align: right;",
                            ShowInFilter = false
                        }
                    },
                    {
                        nameof(InventoryStock.MinStockLevel),
                        new FieldDefinition<InventoryStock>
                        {
                            PropertyName = nameof(InventoryStock.MinStockLevel),
                            DisplayName = "最低警戒線",
                            FilterType = SearchFilterType.NumberRange,
                            TableOrder = 9,
                            FilterOrder = 6,
                            HeaderStyle = "width: 100px; text-align: right;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(InventoryStock.MinStockLevel), s => s.MinStockLevel.ToString(), allowNull: true)
                        }
                    },
                    {
                        nameof(InventoryStock.MaxStockLevel),
                        new FieldDefinition<InventoryStock>
                        {
                            PropertyName = nameof(InventoryStock.MaxStockLevel),
                            DisplayName = "最高警戒線",
                            FilterType = SearchFilterType.NumberRange,
                            TableOrder = 10,
                            FilterOrder = 7,
                            HeaderStyle = "width: 100px; text-align: right;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(InventoryStock.MaxStockLevel), s => s.MaxStockLevel.ToString(), allowNull: true)
                        }
                    },
                    {
                        nameof(InventoryStock.AverageCost),
                        new FieldDefinition<InventoryStock>
                        {
                            PropertyName = nameof(InventoryStock.AverageCost),
                            DisplayName = "平均成本",
                            FilterType = SearchFilterType.NumberRange,
                            TableOrder = 11,
                            FilterOrder = 8,
                            HeaderStyle = "width: 120px; text-align: right;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(InventoryStock.AverageCost), s => s.AverageCost.ToString(), allowNull: true)
                        }
                    },
                    {
                        nameof(InventoryStock.LastTransactionDate),
                        new FieldDefinition<InventoryStock>
                        {
                            PropertyName = nameof(InventoryStock.LastTransactionDate),
                            DisplayName = "最後交易日期",
                            FilterType = SearchFilterType.DateRange,
                            TableOrder = 12,
                            FilterOrder = 9,
                            HeaderStyle = "width: 130px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableDateRangeFilter(
                                model, query, nameof(InventoryStock.LastTransactionDate), s => s.LastTransactionDate)
                        }
                    }
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
            return query => query.OrderBy(s => s.Product!.Code).ThenBy(s => s.Warehouse!.Name).ThenBy(s => s.WarehouseLocation!.Code);
        }
    }
}
