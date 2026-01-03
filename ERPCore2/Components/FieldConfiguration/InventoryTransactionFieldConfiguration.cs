using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Components.Shared.PageTemplate;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;
using ERPCore2.Helpers;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 庫存異動記錄欄位配置
    /// </summary>
    public class InventoryTransactionFieldConfiguration : BaseFieldConfiguration<InventoryTransaction>
    {
        private readonly List<Product> _products;
        private readonly List<Warehouse> _warehouses;
        private readonly List<WarehouseLocation> _warehouseLocations;
        private readonly INotificationService? _notificationService;

        public InventoryTransactionFieldConfiguration(
            List<Product> products,
            List<Warehouse> warehouses,
            List<WarehouseLocation>? warehouseLocations = null,
            INotificationService? notificationService = null)
        {
            _products = products ?? new List<Product>();
            _warehouses = warehouses ?? new List<Warehouse>();
            _warehouseLocations = warehouseLocations ?? new List<WarehouseLocation>();
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<InventoryTransaction>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<InventoryTransaction>>
                {
                    {
                        nameof(InventoryTransaction.TransactionNumber),
                        new FieldDefinition<InventoryTransaction>
                        {
                            PropertyName = nameof(InventoryTransaction.TransactionNumber),
                            DisplayName = "交易單號",
                            FilterPlaceholder = "輸入交易單號搜尋",
                            TableOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(InventoryTransaction.TransactionNumber), t => t.TransactionNumber)
                        }
                    },
                    {
                        nameof(InventoryTransaction.TransactionType),
                        new FieldDefinition<InventoryTransaction>
                        {
                            PropertyName = nameof(InventoryTransaction.TransactionType),
                            DisplayName = "交易類型",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 2,
                            Options = GetTransactionTypeOptions(),
                            CustomTemplate = (data) => (RenderFragment)((builder) =>
                            {
                                if (data is InventoryTransaction transaction)
                                {
                                    var description = GetEnumDescription(transaction.TransactionType);
                                    var badgeClass = GetTransactionTypeBadgeClass(transaction.TransactionType);
                                    
                                    builder.OpenElement(0, "span");
                                    builder.AddAttribute(1, "class", $"badge {badgeClass}");
                                    builder.AddContent(2, description);
                                    builder.CloseElement();
                                }
                            }),
                            FilterFunction = (model, query) => FilterHelper.ApplyIntIdFilter(
                                model, query, nameof(InventoryTransaction.TransactionType), t => (int)t.TransactionType)
                        }
                    },
                    {
                        nameof(InventoryTransaction.TransactionDate),
                        new FieldDefinition<InventoryTransaction>
                        {
                            PropertyName = nameof(InventoryTransaction.TransactionDate),
                            DisplayName = "交易日期",
                            FilterType = SearchFilterType.DateRange,
                            ColumnType = ColumnDataType.Date,
                            TableOrder = 3,
                            FilterFunction = (model, query) => FilterHelper.ApplyDateRangeFilter(
                                model, query, nameof(InventoryTransaction.TransactionDate), t => t.TransactionDate)
                        }
                    },
                    {
                        nameof(InventoryTransaction.ProductId),
                        new FieldDefinition<InventoryTransaction>
                        {
                            PropertyName = "Product.Code", // 表格顯示用
                            FilterPropertyName = nameof(InventoryTransaction.ProductId), // 篩選器用
                            DisplayName = "商品編號",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 4,
                            Options = _products.Select(p => new SelectOption 
                            { 
                                Text = $"{p.Code} - {p.Name}", 
                                Value = p.Id.ToString() 
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(InventoryTransaction.ProductId), t => t.ProductId)
                        }
                    },
                    {
                        "ProductName",
                        new FieldDefinition<InventoryTransaction>
                        {
                            PropertyName = "Product.Name",
                            DisplayName = "商品名稱",
                            TableOrder = 5,
                            ShowInFilter = false,
                        }
                    },
                    {
                        nameof(InventoryTransaction.WarehouseId),
                        new FieldDefinition<InventoryTransaction>
                        {
                            PropertyName = "Warehouse.Name", // 表格顯示用
                            FilterPropertyName = nameof(InventoryTransaction.WarehouseId), // 篩選器用
                            DisplayName = "倉庫",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 6,
                            Options = _warehouses.Select(w => new SelectOption 
                            { 
                                Text = $"{w.Code} - {w.Name}", 
                                Value = w.Id.ToString() 
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(InventoryTransaction.WarehouseId), t => t.WarehouseId)
                        }
                    },
                    {
                        nameof(InventoryTransaction.WarehouseLocationId),
                        new FieldDefinition<InventoryTransaction>
                        {
                            PropertyName = "WarehouseLocation.Name", // 表格顯示用
                            FilterPropertyName = nameof(InventoryTransaction.WarehouseLocationId), // 篩選器用
                            DisplayName = "庫位",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 7,
                            NullDisplayText = "無",
                            Options = _warehouseLocations.Select(wl => new SelectOption 
                            { 
                                Text = $"{wl.Code} - {wl.Name}", 
                                Value = wl.Id.ToString() 
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(InventoryTransaction.WarehouseLocationId), t => t.WarehouseLocationId)
                        }
                    },
                    {
                        nameof(InventoryTransaction.Quantity),
                        new FieldDefinition<InventoryTransaction>
                        {
                            PropertyName = nameof(InventoryTransaction.Quantity),
                            DisplayName = "數量",
                            FilterType = SearchFilterType.NumberRange,
                            ColumnType = ColumnDataType.Number,
                            TableOrder = 8,
                            CustomTemplate = (data) => (RenderFragment)((builder) =>
                            {
                                if (data is InventoryTransaction transaction)
                                {
                                    var quantity = transaction.Quantity;
                                    var textClass = quantity >= 0 ? "text-success" : "text-danger";
                                    var symbol = quantity >= 0 ? "+" : "";
                                    
                                    builder.OpenElement(0, "span");
                                    builder.AddAttribute(1, "class", textClass);
                                    builder.AddContent(2, $"{symbol}{quantity:N0}");
                                    builder.CloseElement();
                                }
                            }),
                            ShowInFilter = false // 數量範圍篩選暫不提供
                        }
                    },
                    {
                        nameof(InventoryTransaction.UnitCost),
                        new FieldDefinition<InventoryTransaction>
                        {
                            PropertyName = nameof(InventoryTransaction.UnitCost),
                            DisplayName = "單位成本",
                            FilterType = SearchFilterType.NumberRange,
                            ColumnType = ColumnDataType.Currency,
                            TableOrder = 9,
                            NullDisplayText = "-",
                            ShowInFilter = false // 成本範圍篩選暫不提供
                        }
                    },
                    {
                        nameof(InventoryTransaction.StockBefore),
                        new FieldDefinition<InventoryTransaction>
                        {
                            PropertyName = nameof(InventoryTransaction.StockBefore),
                            DisplayName = "交易前庫存",
                            ColumnType = ColumnDataType.Number,
                            TableOrder = 10,
                            ShowInFilter = false,
                        }
                    },
                    {
                        nameof(InventoryTransaction.StockAfter),
                        new FieldDefinition<InventoryTransaction>
                        {
                            PropertyName = nameof(InventoryTransaction.StockAfter),
                            DisplayName = "交易後庫存",
                            ColumnType = ColumnDataType.Number,
                            TableOrder = 11,
                            ShowInFilter = false,
                        }
                    },
                    {
                        nameof(InventoryTransaction.TransactionBatchNumber),
                        new FieldDefinition<InventoryTransaction>
                        {
                            PropertyName = nameof(InventoryTransaction.TransactionBatchNumber),
                            DisplayName = "批號",
                            FilterPlaceholder = "輸入批號搜尋",
                            TableOrder = 14,
                            NullDisplayText = "-",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(InventoryTransaction.TransactionBatchNumber), t => t.TransactionBatchNumber, allowNull: true)
                        }
                    },
                    {
                        nameof(InventoryTransaction.TransactionBatchDate),
                        new FieldDefinition<InventoryTransaction>
                        {
                            PropertyName = nameof(InventoryTransaction.TransactionBatchDate),
                            DisplayName = "批次進貨日期",
                            FilterType = SearchFilterType.DateRange,
                            ColumnType = ColumnDataType.Date,
                            TableOrder = 15,
                            FilterOrder = 0, // 不在篩選器中顯示
                            ShowInFilter = false,
                            NullDisplayText = "-"
                        }
                    },
                };
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: new 
                    { 
                        ProductsCount = _products?.Count ?? 0,
                        WarehousesCount = _warehouses?.Count ?? 0,
                        WarehouseLocationsCount = _warehouseLocations?.Count ?? 0
                    });
                });

                // 通知使用者
                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("初始化庫存異動欄位配置時發生錯誤，已使用預設配置");
                    });
                }

                // 回傳空的配置，讓頁面使用預設行為
                return new Dictionary<string, FieldDefinition<InventoryTransaction>>();
            }
        }

        /// <summary>
        /// 取得交易類型選項
        /// </summary>
        private List<SelectOption> GetTransactionTypeOptions()
        {
            try
            {
                return Enum.GetValues<InventoryTransactionTypeEnum>()
                    .Select(e => new SelectOption
                    {
                        Text = GetEnumDescription(e),
                        Value = ((int)e).ToString()
                    }).ToList();
            }
            catch (Exception)
            {
                return new List<SelectOption>();
            }
        }

        /// <summary>
        /// 取得枚舉描述
        /// </summary>
        private string GetEnumDescription(InventoryTransactionTypeEnum value)
        {
            try
            {
                var fieldInfo = value.GetType().GetField(value.ToString());
                var attribute = fieldInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                    .FirstOrDefault() as DescriptionAttribute;
                return attribute?.Description ?? value.ToString();
            }
            catch (Exception)
            {
                return value.ToString();
            }
        }

        /// <summary>
        /// 取得交易類型徽章樣式
        /// </summary>
        private string GetTransactionTypeBadgeClass(InventoryTransactionTypeEnum transactionType)
        {
            return transactionType switch
            {
                InventoryTransactionTypeEnum.Purchase => "bg-success",
                InventoryTransactionTypeEnum.Sale => "bg-primary",
                InventoryTransactionTypeEnum.Return => "bg-warning",
                InventoryTransactionTypeEnum.Adjustment => "bg-info",
                InventoryTransactionTypeEnum.Transfer => "bg-secondary",
                InventoryTransactionTypeEnum.StockTaking => "bg-dark",
                InventoryTransactionTypeEnum.ProductionConsumption => "bg-danger",
                InventoryTransactionTypeEnum.ProductionCompletion => "bg-success",
                InventoryTransactionTypeEnum.Scrap => "bg-danger",
                InventoryTransactionTypeEnum.OpeningBalance => "bg-light text-dark",
                _ => "bg-light text-dark"
            };
        }

        /// <summary>
    }
}
