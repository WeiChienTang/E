using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Components.Shared.Modal;
using ERPCore2.Components.Shared.Table;
using ERPCore2.Components.Shared.Page;
using ERPCore2.Components.Shared.Statistics;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Services;
using ERPCore2.Helpers;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 庫存異動記錄欄位配置（主檔）
    /// 注意：明細資料透過 InventoryTransactionTable 元件顯示
    /// </summary>
    public class InventoryTransactionFieldConfiguration : BaseFieldConfiguration<InventoryTransaction>
    {
        private readonly List<Warehouse> _warehouses;
        private readonly List<Employee> _employees;
        private readonly INotificationService? _notificationService;

        public InventoryTransactionFieldConfiguration(
            List<Warehouse> warehouses,
            List<Employee>? employees = null,
            INotificationService? notificationService = null)
        {
            _warehouses = warehouses ?? new List<Warehouse>();
            _employees = employees ?? new List<Employee>();
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
                            DisplayName = Dn("Field.TransactionCode", "交易單號"),
                            FilterPlaceholder = Fp("Field.TransactionCode", "輸入交易單號搜尋"),
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
                            DisplayName = Dn("Field.TransactionType", "交易類型"),
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
                            DisplayName = Dn("Field.TransactionDate", "交易日期"),
                            FilterType = SearchFilterType.DateRange,
                            ColumnType = ColumnDataType.Date,
                            TableOrder = 3,
                            FilterFunction = (model, query) => FilterHelper.ApplyDateRangeFilter(
                                model, query, nameof(InventoryTransaction.TransactionDate), t => t.TransactionDate)
                        }
                    },
                    {
                        nameof(InventoryTransaction.SourceDocumentType),
                        new FieldDefinition<InventoryTransaction>
                        {
                            PropertyName = nameof(InventoryTransaction.SourceDocumentType),
                            DisplayName = Dn("Field.SourceDocumentType", "來源類型"),
                            TableOrder = 4,
                            CustomTemplate = (data) => (RenderFragment)((builder) =>
                            {
                                if (data is InventoryTransaction transaction)
                                {
                                    var displayName = InventorySourceDocumentTypes.GetDisplayName(transaction.SourceDocumentType);
                                    builder.AddContent(0, displayName);
                                }
                            }),
                            NullDisplayText = "-",
                            ShowInFilter = false
                        }
                    },
                    {
                        nameof(InventoryTransaction.SourceDocumentId),
                        new FieldDefinition<InventoryTransaction>
                        {
                            PropertyName = nameof(InventoryTransaction.SourceDocumentId),
                            DisplayName = Dn("Field.SourceDocumentId", "來源單據ID"),
                            TableOrder = 5,
                            NullDisplayText = "-",
                            ShowInFilter = false
                        }
                    },
                    {
                        nameof(InventoryTransaction.WarehouseId),
                        new FieldDefinition<InventoryTransaction>
                        {
                            PropertyName = "Warehouse.Name",
                            FilterPropertyName = nameof(InventoryTransaction.WarehouseId),
                            DisplayName = Dn("Field.Warehouse", "倉庫"),
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
                };
            }
            catch (Exception ex)
            {
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: new 
                    { 
                        WarehousesCount = _warehouses?.Count ?? 0,
                        EmployeesCount = _employees?.Count ?? 0
                    });
                });

                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("初始化庫存異動欄位配置時發生錯誤，已使用預設配置");
                    });
                }

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
                InventoryTransactionTypeEnum.SalesReturn => "bg-info",
                InventoryTransactionTypeEnum.Adjustment => "bg-info",
                InventoryTransactionTypeEnum.Transfer => "bg-secondary",
                InventoryTransactionTypeEnum.StockTaking => "bg-dark",
                InventoryTransactionTypeEnum.ProductionConsumption => "bg-danger",
                InventoryTransactionTypeEnum.ProductionCompletion => "bg-success",
                InventoryTransactionTypeEnum.Scrap => "bg-danger",
                InventoryTransactionTypeEnum.MaterialIssue => "bg-warning",
                InventoryTransactionTypeEnum.MaterialReturn => "bg-info",
                InventoryTransactionTypeEnum.OpeningBalance => "bg-light text-dark",
                _ => "bg-light text-dark"
            };
        }
    }
}
