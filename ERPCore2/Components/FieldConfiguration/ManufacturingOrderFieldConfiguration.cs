using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Data.Entities;
using ERPCore2.Services;
using ERPCore2.Helpers;
using ERPCore2.Models.Enums;
using ERPCore2.Components.Shared.Modal;
using ERPCore2.Components.Shared.Table;
using ERPCore2.Components.Shared.Page;
using Microsoft.AspNetCore.Components;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 製令單欄位配置（基於 ProductionScheduleItem）
    /// </summary>
    public class ManufacturingOrderFieldConfiguration : BaseFieldConfiguration<ProductionScheduleItem>
    {
        private readonly List<Item> _products;
        private readonly List<Employee> _employees;
        private readonly INotificationService? _notificationService;

        public ManufacturingOrderFieldConfiguration(
            List<Item> products,
            List<Employee> employees,
            INotificationService? notificationService = null)
        {
            _products = products;
            _employees = employees;
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<ProductionScheduleItem>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<ProductionScheduleItem>>
                {
                    {
                        nameof(ProductionScheduleItem.Code),
                        new FieldDefinition<ProductionScheduleItem>
                        {
                            PropertyName = nameof(ProductionScheduleItem.Code),
                            DisplayName = Dn("Field.ManufacturingOrderCode", "製令單號"),
                            FilterPlaceholder = Fp("Field.ManufacturingOrderCode", "輸入製令單號搜尋"),
                            TableOrder = 1,
                            Width = "130px",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(ProductionScheduleItem.Code), psi => psi.Code)
                        }
                    },
                    {
                        nameof(ProductionScheduleItem.ItemId),
                        new FieldDefinition<ProductionScheduleItem>
                        {
                            PropertyName = "Item.Name",
                            FilterPropertyName = nameof(ProductionScheduleItem.ItemId),
                            DisplayName = Dn("Entity.Item", "品項"),
                            FilterType = SearchFilterType.Select,
                            TableOrder = 2,
                            Width = "150px",
                            Options = _products.Select(p => new SelectOption
                            {
                                Text = $"{p.Code} - {p.Name}".Trim(' ', '-'),
                                Value = p.Id.ToString()
                            }).ToList(),
                            NullDisplayText = "-",
                            FilterFunction = (model, query) => FilterHelper.ApplyIntIdFilter(
                                model, query, nameof(ProductionScheduleItem.ItemId), psi => psi.ItemId),
                            CustomTemplate = item => builder =>
                            {
                                var psi = (ProductionScheduleItem)item;
                                builder.AddContent(0, psi.Item?.Name ?? "-");
                            }
                        }
                    },
                    {
                        nameof(ProductionScheduleItem.ProductionItemStatus),
                        new FieldDefinition<ProductionScheduleItem>
                        {
                            PropertyName = nameof(ProductionScheduleItem.ProductionItemStatus),
                            DisplayName = Dn("Field.ProductionStatus", "生產狀態"),
                            FilterType = SearchFilterType.Select,
                            TableOrder = 3,
                            Width = "100px",
                            Options = Enum.GetValues<ProductionItemStatus>().Select(s => new SelectOption
                            {
                                Text = s switch
                                {
                                    ProductionItemStatus.Pending => L?["Production.Pending"].ToString() ?? "待生產",
                                    ProductionItemStatus.WaitingMaterial => L?["Production.WaitingMaterial"].ToString() ?? "等待領料",
                                    ProductionItemStatus.InProgress => L?["Production.InProgress"].ToString() ?? "生產中",
                                    ProductionItemStatus.Paused => L?["Production.Paused"].ToString() ?? "已暫停",
                                    ProductionItemStatus.Completed => L?["Production.Completed"].ToString() ?? "已完成",
                                    ProductionItemStatus.Aborted => L?["Production.Aborted"].ToString() ?? "已終止",
                                    _ => s.ToString()
                                },
                                Value = ((int)s).ToString()
                            }).ToList(),
                            FilterFunction = (model, query) =>
                            {
                                var filterValue = model.GetFilterValue(nameof(ProductionScheduleItem.ProductionItemStatus))?.ToString();
                                if (string.IsNullOrWhiteSpace(filterValue) || !int.TryParse(filterValue, out var statusInt))
                                    return query;
                                var status = (ProductionItemStatus)statusInt;
                                return query.Where(psi => psi.ProductionItemStatus == status);
                            },
                            CustomTemplate = item => builder =>
                            {
                                var psi = (ProductionScheduleItem)item;
                                var (text, cssClass) = psi.ProductionItemStatus switch
                                {
                                    ProductionItemStatus.Pending => ("待生產", "badge bg-secondary"),
                                    ProductionItemStatus.WaitingMaterial => ("等待領料", "badge bg-warning text-dark"),
                                    ProductionItemStatus.InProgress => ("生產中", "badge bg-primary"),
                                    ProductionItemStatus.Paused => ("已暫停", "badge bg-warning text-dark"),
                                    ProductionItemStatus.Completed => ("已完成", "badge bg-success"),
                                    ProductionItemStatus.Aborted => ("已終止", "badge bg-danger"),
                                    _ => (psi.ProductionItemStatus.ToString(), "badge bg-secondary")
                                };
                                builder.OpenElement(0, "span");
                                builder.AddAttribute(1, "class", cssClass);
                                builder.AddContent(2, text);
                                builder.CloseElement();
                            }
                        }
                    },
                    {
                        nameof(ProductionScheduleItem.ScheduledQuantity),
                        new FieldDefinition<ProductionScheduleItem>
                        {
                            PropertyName = nameof(ProductionScheduleItem.ScheduledQuantity),
                            DisplayName = Dn("Field.ScheduledQuantity", "計劃數量"),
                            TableOrder = 4,
                            Width = "100px",
                            ColumnType = ColumnDataType.Number,
                            ShowInFilter = false,
                            CustomTemplate = item => builder =>
                            {
                                var psi = (ProductionScheduleItem)item;
                                builder.AddContent(0, psi.ScheduledQuantity.ToString("N0"));
                            }
                        }
                    },
                    {
                        nameof(ProductionScheduleItem.CompletedQuantity),
                        new FieldDefinition<ProductionScheduleItem>
                        {
                            PropertyName = nameof(ProductionScheduleItem.CompletedQuantity),
                            DisplayName = Dn("Field.CompletedQuantity", "完成數量"),
                            TableOrder = 5,
                            Width = "100px",
                            ColumnType = ColumnDataType.Number,
                            ShowInFilter = false,
                            CustomTemplate = item => builder =>
                            {
                                var psi = (ProductionScheduleItem)item;
                                builder.AddContent(0, psi.CompletedQuantity.ToString("N0"));
                            }
                        }
                    },
                    {
                        nameof(ProductionScheduleItem.PlannedStartDate),
                        new FieldDefinition<ProductionScheduleItem>
                        {
                            PropertyName = nameof(ProductionScheduleItem.PlannedStartDate),
                            DisplayName = Dn("Field.PlannedStartDate", "預計開始"),
                            FilterType = SearchFilterType.Date,
                            TableOrder = 6,
                            Width = "110px",
                            FilterFunction = (model, query) => FilterHelper.ApplyDateRangeFilter(
                                model, query, nameof(ProductionScheduleItem.PlannedStartDate),
                                psi => psi.PlannedStartDate ?? DateTime.MinValue),
                            CustomTemplate = item => builder =>
                            {
                                var psi = (ProductionScheduleItem)item;
                                builder.AddContent(0, psi.PlannedStartDate?.ToString("yyyy/MM/dd") ?? "-");
                            }
                        }
                    },
                    {
                        nameof(ProductionScheduleItem.PlannedEndDate),
                        new FieldDefinition<ProductionScheduleItem>
                        {
                            PropertyName = nameof(ProductionScheduleItem.PlannedEndDate),
                            DisplayName = Dn("Field.PlannedEndDate", "預計完成"),
                            TableOrder = 7,
                            Width = "110px",
                            ShowInFilter = false,
                            CustomTemplate = item => builder =>
                            {
                                var psi = (ProductionScheduleItem)item;
                                builder.AddContent(0, psi.PlannedEndDate?.ToString("yyyy/MM/dd") ?? "-");
                            }
                        }
                    },
                    {
                        nameof(ProductionScheduleItem.ResponsibleEmployeeId),
                        new FieldDefinition<ProductionScheduleItem>
                        {
                            PropertyName = "ResponsibleEmployee.Name",
                            FilterPropertyName = nameof(ProductionScheduleItem.ResponsibleEmployeeId),
                            DisplayName = Dn("Field.ResponsibleEmployee", "負責人員"),
                            FilterType = SearchFilterType.Select,
                            TableOrder = 8,
                            Width = "110px",
                            Options = _employees.Select(e => new SelectOption
                            {
                                Text = $"{e.Code} - {e.Name}".Trim(' ', '-'),
                                Value = e.Id.ToString()
                            }).ToList(),
                            NullDisplayText = "-",
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(ProductionScheduleItem.ResponsibleEmployeeId), psi => psi.ResponsibleEmployeeId),
                            CustomTemplate = item => builder =>
                            {
                                var psi = (ProductionScheduleItem)item;
                                builder.AddContent(0, psi.ResponsibleEmployee?.Name ?? "-");
                            }
                        }
                    },
                };
            }
            catch (Exception ex)
            {
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "初始化製令單欄位配置失敗");
                });

                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("初始化製令單欄位配置時發生錯誤，已使用預設配置");
                    });
                }

                return new Dictionary<string, FieldDefinition<ProductionScheduleItem>>();
            }
        }
    }
}
