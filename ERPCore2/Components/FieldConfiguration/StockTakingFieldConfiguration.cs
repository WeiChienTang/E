using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Helpers;
using ERPCore2.Models;
using ERPCore2.Services;
using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Components.Shared.PageTemplate;
using Microsoft.AspNetCore.Components;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 盤點單欄位配置
    /// </summary>
    public class StockTakingFieldConfiguration : BaseFieldConfiguration<StockTaking>
    {
        private readonly List<Warehouse> _warehouses;
        private readonly List<Employee> _employees;
        private readonly INotificationService? _notificationService;

        public StockTakingFieldConfiguration(
            List<Warehouse> warehouses,
            List<Employee> employees,
            INotificationService? notificationService = null)
        {
            _warehouses = warehouses;
            _employees = employees;
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<StockTaking>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<StockTaking>>
                {
                    {
                        nameof(StockTaking.TakingNumber),
                        new FieldDefinition<StockTaking>
                        {
                            PropertyName = nameof(StockTaking.TakingNumber),
                            DisplayName = "盤點單號",
                            FilterPlaceholder = "輸入盤點單號搜尋",
                            TableOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(StockTaking.TakingNumber), st => st.TakingNumber)
                        }
                    },
                    {
                        nameof(StockTaking.TakingDate),
                        new FieldDefinition<StockTaking>
                        {
                            PropertyName = nameof(StockTaking.TakingDate),
                            DisplayName = "盤點日期",
                            FilterType = SearchFilterType.DateRange,
                            ColumnType = ColumnDataType.Date,
                            TableOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyDateRangeFilter(
                                model, query, nameof(StockTaking.TakingDate), st => st.TakingDate)
                        }
                    },
                    {
                        nameof(StockTaking.WarehouseId),
                        new FieldDefinition<StockTaking>
                        {
                            PropertyName = "Warehouse.Name",
                            FilterPropertyName = nameof(StockTaking.WarehouseId),
                            DisplayName = "倉庫",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 3,
                            Options = _warehouses.Select(w => new SelectOption
                            {
                                Text = w.Name ?? "",
                                Value = w.Id.ToString()
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyIntIdFilter(
                                model, query, nameof(StockTaking.WarehouseId), st => st.WarehouseId)
                        }
                    },
                    {
                        nameof(StockTaking.TakingType),
                        new FieldDefinition<StockTaking>
                        {
                            PropertyName = nameof(StockTaking.TakingType),
                            DisplayName = "盤點類型",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 4,
                            Options = GetTakingTypeOptions(),
                            CustomTemplate = CreateTakingTypeTemplate(),
                            FilterFunction = (model, query) => ApplyTakingTypeFilter(model, query)
                        }
                    },
                    {
                        nameof(StockTaking.TotalItems),
                        new FieldDefinition<StockTaking>
                        {
                            PropertyName = nameof(StockTaking.TotalItems),
                            DisplayName = "盤點項目數",
                            ColumnType = ColumnDataType.Number,
                            TableOrder = 6,
                            ShowInFilter = false,
                            CustomTemplate = value => builder =>
                            {
                                if (value is StockTaking stockTaking)
                                {
                                    builder.AddContent(0, NumberFormatHelper.FormatSmart(stockTaking.TotalItems));
                                }
                            }
                        }
                    },
                    {
                        nameof(StockTaking.CompletedItems),
                        new FieldDefinition<StockTaking>
                        {
                            PropertyName = nameof(StockTaking.CompletedItems),
                            DisplayName = "已盤點數",
                            ColumnType = ColumnDataType.Number,
                            TableOrder = 7,
                            ShowInFilter = false,
                            CustomTemplate = value => builder =>
                            {
                                if (value is StockTaking stockTaking)
                                {
                                    builder.AddContent(0, NumberFormatHelper.FormatSmart(stockTaking.CompletedItems));
                                }
                            }
                        }
                    },
                    {
                        nameof(StockTaking.DifferenceItems),
                        new FieldDefinition<StockTaking>
                        {
                            PropertyName = nameof(StockTaking.DifferenceItems),
                            DisplayName = "差異項目數",
                            ColumnType = ColumnDataType.Number,
                            TableOrder = 8,
                            ShowInFilter = false,
                            CustomTemplate = value => builder =>
                            {
                                if (value is StockTaking stockTaking)
                                {
                                    builder.AddContent(0, NumberFormatHelper.FormatSmart(stockTaking.DifferenceItems));
                                }
                            }
                        }
                    },
                    {
                        nameof(StockTaking.DifferenceAmount),
                        new FieldDefinition<StockTaking>
                        {
                            PropertyName = nameof(StockTaking.DifferenceAmount),
                            DisplayName = "盤盈盤虧",
                            ColumnType = ColumnDataType.Number,
                            TableOrder = 9,
                            ShowInFilter = false,
                            CustomTemplate = value => builder =>
                            {
                                if (value is StockTaking stockTaking)
                                {
                                    var amount = stockTaking.DifferenceAmount;
                                    var colorClass = amount > 0 ? "text-success" : amount < 0 ? "text-danger" : "";
                                    
                                    builder.OpenElement(0, "span");
                                    if (!string.IsNullOrEmpty(colorClass))
                                    {
                                        builder.AddAttribute(1, "class", colorClass);
                                    }
                                    builder.AddContent(2, NumberFormatHelper.FormatSmart(amount));
                                    builder.CloseElement();
                                }
                            }
                        }
                    },
                    {
                        nameof(StockTaking.CompletionRate),
                        new FieldDefinition<StockTaking>
                        {
                            PropertyName = nameof(StockTaking.CompletionRate),
                            DisplayName = "完成率(%)",
                            ColumnType = ColumnDataType.Number,
                            TableOrder = 10,
                        }
                    },
                    {
                        nameof(StockTaking.Remarks),
                        new FieldDefinition<StockTaking>
                        {
                            PropertyName = nameof(StockTaking.Remarks),
                            DisplayName = "備註",
                            FilterPlaceholder = "輸入備註搜尋",
                            TableOrder = 11,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(StockTaking.Remarks), st => st.Remarks, allowNull: true)
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[StockTakingFieldConfiguration] GetFieldDefinitions 發生錯誤: {ex.Message}");
                
                _ = Task.Run(async () =>
                {
                    if (_notificationService != null)
                    {
                        await _notificationService.ShowErrorAsync("載入盤點欄位配置時發生錯誤");
                    }
                });

                return new Dictionary<string, FieldDefinition<StockTaking>>();
            }
        }

        // ===== 篩選方法 =====

        private static IQueryable<StockTaking> ApplyTakingTypeFilter(SearchFilterModel model, IQueryable<StockTaking> query)
        {
            if (!model.SelectFilters.TryGetValue(nameof(StockTaking.TakingType), out var value) ||
                string.IsNullOrEmpty(value))
            {
                return query;
            }

            if (int.TryParse(value, out var typeInt) && Enum.IsDefined(typeof(StockTakingTypeEnum), typeInt))
            {
                var takingType = (StockTakingTypeEnum)typeInt;
                return query.Where(st => st.TakingType == takingType);
            }

            return query;
        }

        private static IQueryable<StockTaking> ApplyTakingStatusFilter(SearchFilterModel model, IQueryable<StockTaking> query)
        {
            if (!model.SelectFilters.TryGetValue(nameof(StockTaking.TakingStatus), out var value) ||
                string.IsNullOrEmpty(value))
            {
                return query;
            }

            if (int.TryParse(value, out var statusInt) && Enum.IsDefined(typeof(StockTakingStatusEnum), statusInt))
            {
                var status = (StockTakingStatusEnum)statusInt;
                return query.Where(st => st.TakingStatus == status);
            }

            return query;
        }

        // ===== 自訂模板 =====

        private static RenderFragment<object> CreateTakingTypeTemplate()
        {
            return value => builder =>
            {
                if (value is StockTaking stockTaking)
                {
                    var displayName = GetTakingTypeDisplayName(stockTaking.TakingType);
                    builder.AddContent(0, displayName);
                }
            };
        }

        private static RenderFragment<object> CreateTakingStatusTemplate()
        {
            return value => builder =>
            {
                if (value is StockTaking stockTaking)
                {
                    var (badgeClass, displayName) = GetTakingStatusBadge(stockTaking.TakingStatus);
                    builder.OpenElement(0, "span");
                    builder.AddAttribute(1, "class", $"badge {badgeClass}");
                    builder.AddContent(2, displayName);
                    builder.CloseElement();
                }
            };
        }

        // ===== 輔助方法 =====

        private static List<SelectOption> GetTakingTypeOptions()
        {
            return new List<SelectOption>
            {
                new SelectOption { Text = "全盤", Value = ((int)StockTakingTypeEnum.Full).ToString() },
                new SelectOption { Text = "循環盤點", Value = ((int)StockTakingTypeEnum.Cycle).ToString() },
                new SelectOption { Text = "抽樣盤點", Value = ((int)StockTakingTypeEnum.Sample).ToString() },
                new SelectOption { Text = "特定商品盤點", Value = ((int)StockTakingTypeEnum.Specific).ToString() },
                new SelectOption { Text = "特定位置盤點", Value = ((int)StockTakingTypeEnum.Location).ToString() }
            };
        }

        private static List<SelectOption> GetTakingStatusOptions()
        {
            return new List<SelectOption>
            {
                new SelectOption { Text = "草稿", Value = ((int)StockTakingStatusEnum.Draft).ToString() },
                new SelectOption { Text = "進行中", Value = ((int)StockTakingStatusEnum.InProgress).ToString() },
                new SelectOption { Text = "已完成", Value = ((int)StockTakingStatusEnum.Completed).ToString() },
                new SelectOption { Text = "待審核", Value = ((int)StockTakingStatusEnum.PendingApproval).ToString() },
                new SelectOption { Text = "已審核", Value = ((int)StockTakingStatusEnum.Approved).ToString() },
                new SelectOption { Text = "已取消", Value = ((int)StockTakingStatusEnum.Cancelled).ToString() }
            };
        }

        private static string GetTakingTypeDisplayName(StockTakingTypeEnum takingType)
        {
            return takingType switch
            {
                StockTakingTypeEnum.Full => "全盤",
                StockTakingTypeEnum.Cycle => "循環盤點",
                StockTakingTypeEnum.Sample => "抽樣盤點",
                StockTakingTypeEnum.Specific => "特定商品盤點",
                StockTakingTypeEnum.Location => "特定位置盤點",
                _ => takingType.ToString()
            };
        }

        private static (string badgeClass, string displayName) GetTakingStatusBadge(StockTakingStatusEnum status)
        {
            return status switch
            {
                StockTakingStatusEnum.Draft => ("bg-secondary", "草稿"),
                StockTakingStatusEnum.InProgress => ("bg-info text-dark", "進行中"),
                StockTakingStatusEnum.Completed => ("bg-success", "已完成"),
                StockTakingStatusEnum.PendingApproval => ("bg-warning text-dark", "待審核"),
                StockTakingStatusEnum.Approved => ("bg-primary", "已審核"),
                StockTakingStatusEnum.Cancelled => ("bg-dark", "已取消"),
                _ => ("bg-secondary", status.ToString())
            };
        }
    }
}
