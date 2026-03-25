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
    /// 庫存欄位配置
    /// </summary>
    public class InventoryStockFieldConfiguration : BaseFieldConfiguration<InventoryStock>
    {
        private readonly List<Item> _products;
        private readonly List<Warehouse> _warehouses;
        private readonly List<WarehouseLocation> _warehouseLocations;
        private readonly List<ItemCategory> _productCategories;
        private readonly INotificationService? _notificationService;

        public InventoryStockFieldConfiguration(
            List<Item> products, 
            List<Warehouse> warehouses, 
            List<WarehouseLocation> warehouseLocations,
            List<ItemCategory> productCategories,
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
                        "ItemCode",
                        new FieldDefinition<InventoryStock>
                        {
                            PropertyName = "Item.Code",
                            DisplayName = Dn("Field.ItemCode", "品項編號"),
                            FilterType = SearchFilterType.Text,
                            FilterPlaceholder = Fp("Field.ItemCode", "輸入品項編號搜尋"),
                            TableOrder = 1,
                            Width = "120px",
                            FilterFunction = (model, query) => {
                                var filterValue = model.GetFilterValue("ItemCode")?.ToString();
                                if (!string.IsNullOrWhiteSpace(filterValue))
                                {
                                    // 使用安全的方式查詢，避免導航屬性的 null reference
                                    query = query.Where(s => s.Item != null &&
                                                           s.Item.Code != null &&
                                                           s.Item.Code.Contains(filterValue));
                                }
                                return query;
                            }
                        }
                    },
                    {
                        nameof(InventoryStock.ItemId),
                        new FieldDefinition<InventoryStock>
                        {
                            PropertyName = "Item.Name", // 表格顯示用
                            FilterPropertyName = nameof(InventoryStock.ItemId), // 篩選器用
                            DisplayName = Dn("Field.ItemName", "品項名稱"),
                            FilterType = SearchFilterType.Select,
                            TableOrder = 2,
                            Width = "150px",
                            Options = _products.Select(p => new SelectOption
                            {
                                Text = $"{p.Code} - {p.Name}",
                                Value = p.Id.ToString()
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(InventoryStock.ItemId), s => s.ItemId)
                        }
                    },
                    {
                        "ItemCategoryId",
                        new FieldDefinition<InventoryStock>
                        {
                            PropertyName = "Item.ItemCategory.Name",
                            FilterPropertyName = "ItemCategoryId",
                            DisplayName = Dn("Field.ItemCategory", "品項類型"),
                            FilterType = SearchFilterType.Select,
                            TableOrder = 3,
                            Width = "120px",
                            Options = _productCategories.Select(pc => new SelectOption
                            {
                                Text = pc.Name!,
                                Value = pc.Id.ToString()
                            }).ToList(),
                            FilterFunction = (model, query) => {
                                var filterValue = model.GetFilterValue("ItemCategoryId")?.ToString();
                                if (!string.IsNullOrWhiteSpace(filterValue) && int.TryParse(filterValue, out int categoryId))
                                {
                                    query = query.Where(s => s.Item != null && 
                                                           s.Item.ItemCategoryId == categoryId);
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
                            DisplayName = Dn("Field.CurrentStock", "現有庫存"),
                            FilterType = SearchFilterType.Text,
                            TableOrder = 4,
                            Width = "110px",
                            ShowInFilter = false,
                            CustomTemplate = (data) => (RenderFragment)((builder) =>
                            {
                                if (data is InventoryStock stock)
                                {
                                    var valueStr = NumberFormatHelper.FormatSmart(stock.TotalCurrentStock, useThousandsSeparator: true);
                                    var unitName = stock.Item?.Unit?.Name ?? "";
                                    builder.AddContent(0, string.IsNullOrEmpty(unitName) ? valueStr : $"{valueStr} {unitName}");
                                }
                            })
                        }
                    },
                    {
                        "ProductionUnitStock",
                        new FieldDefinition<InventoryStock>
                        {
                            PropertyName = "Item.ProductionUnit.Name",
                            DisplayName = Dn("Field.ProductionUnitStock", "製程庫存"),
                            FilterType = SearchFilterType.Text,
                            TableOrder = 5,
                            Width = "110px",
                            ShowInFilter = false,
                            CustomTemplate = (data) => (RenderFragment)((builder) =>
                            {
                                if (data is InventoryStock stock)
                                {
                                    var product = stock.Item;
                                    if (product == null || !product.ProductionUnitId.HasValue ||
                                        product.ProductionUnitId.Value == product.UnitId ||
                                        !product.ProductionUnitConversionRate.HasValue ||
                                        product.ProductionUnitConversionRate.Value <= 0)
                                    {
                                        builder.AddContent(0, "-");
                                        return;
                                    }

                                    var converted = stock.TotalCurrentStock * product.ProductionUnitConversionRate.Value;
                                    var productionUnitName = product.ProductionUnit?.Name ?? "";
                                    var valueStr = NumberFormatHelper.FormatSmart(converted, useThousandsSeparator: true);
                                    builder.AddContent(0, string.IsNullOrEmpty(productionUnitName) ? valueStr : $"{valueStr} {productionUnitName}");
                                }
                            })
                        }
                    },
                        
                    {
                        "ProductionUnitConversion",
                        new FieldDefinition<InventoryStock>
                        {
                            PropertyName = "Item.ProductionUnit.Name",
                            DisplayName = Dn("Field.ProductionUnitConversion", "製程換算"),
                            FilterType = SearchFilterType.Text,
                            TableOrder = 6,
                            Width = "120px",
                            ShowInFilter = false,
                            CustomTemplate = (data) => (RenderFragment)((builder) =>
                            {
                                if (data is InventoryStock stock)
                                {
                                    var product = stock.Item;
                                    if (product == null || !product.ProductionUnitId.HasValue ||
                                        product.ProductionUnitId.Value == product.UnitId)
                                    {
                                        builder.AddContent(0, "-");
                                        return;
                                    }

                                    var purchaseUnitName = product.Unit?.Name ?? "";
                                    var productionUnitName = product.ProductionUnit?.Name ?? "";
                                    var rate = product.ProductionUnitConversionRate;

                                    if (rate.HasValue && !string.IsNullOrEmpty(purchaseUnitName) && !string.IsNullOrEmpty(productionUnitName))
                                    {
                                        var rateStr = NumberFormatHelper.FormatSmart(rate.Value, useThousandsSeparator: false);
                                        builder.AddContent(0, $"1{purchaseUnitName}={rateStr}{productionUnitName}");
                                    }
                                    else if (!string.IsNullOrEmpty(productionUnitName))
                                    {
                                        builder.AddContent(0, productionUnitName);
                                    }
                                    else
                                    {
                                        builder.AddContent(0, "-");
                                    }
                                }
                            })
                        }
                    },
                    {
                        nameof(InventoryStock.WeightedAverageCost),
                        new FieldDefinition<InventoryStock>
                        {
                            PropertyName = nameof(InventoryStock.WeightedAverageCost),
                            DisplayName = Dn("Field.AverageCost", "平均成本"),
                            FilterType = SearchFilterType.Text,
                            TableOrder = 7,
                            Width = "120px",
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


