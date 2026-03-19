using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Data.Entities;
using ERPCore2.Services;
using ERPCore2.Helpers;
using ERPCore2.Components.Shared.Modal;
using ERPCore2.Components.Shared.Table;
using ERPCore2.Components.Shared.Page;
using ERPCore2.Components.Shared.Statistics;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 磅秤紀錄欄位配置
    /// </summary>
    public class ScaleRecordFieldConfiguration : BaseFieldConfiguration<ScaleRecord>
    {
        private readonly INotificationService? _notificationService;

        public ScaleRecordFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<ScaleRecord>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<ScaleRecord>>
                {
                    {
                        nameof(ScaleRecord.Code),
                        new FieldDefinition<ScaleRecord>
                        {
                            PropertyName = nameof(ScaleRecord.Code),
                            DisplayName = Dn("Field.WasteRecordCode", "磅秤紀錄單號"),
                            FilterPlaceholder = Fp("Field.WasteRecordCode", "輸入單號搜尋"),
                            TableOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(ScaleRecord.Code), sr => sr.Code, allowNull: true)
                        }
                    },
                    {
                        nameof(ScaleRecord.RecordDate),
                        new FieldDefinition<ScaleRecord>
                        {
                            PropertyName = nameof(ScaleRecord.RecordDate),
                            DisplayName = Dn("Field.RecordDate", "記錄日期"),
                            FilterPlaceholder = Fp("Field.RecordDate", "選擇日期範圍"),
                            TableOrder = 2,
                            FilterType = SearchFilterType.DateRange,
                            ColumnType = ColumnDataType.Date,
                            FilterFunction = (model, query) => FilterHelper.ApplyDateRangeFilter(
                                model, query, nameof(ScaleRecord.RecordDate), sr => sr.RecordDate)
                        }
                    },
                    {
                        nameof(ScaleRecord.VehicleId),
                        new FieldDefinition<ScaleRecord>
                        {
                            PropertyName = "Vehicle.LicensePlate",
                            FilterPropertyName = nameof(ScaleRecord.VehicleId),
                            DisplayName = Dn("Field.Vehicle", "車輛"),
                            FilterPlaceholder = Fp("Field.Vehicle", "輸入車牌搜尋"),
                            TableOrder = 3,
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(ScaleRecord.VehicleId), sr => sr.VehicleId)
                        }
                    },
                    {
                        nameof(ScaleRecord.CustomerId),
                        new FieldDefinition<ScaleRecord>
                        {
                            PropertyName = "Customer.CompanyName",
                            FilterPropertyName = nameof(ScaleRecord.CustomerId),
                            DisplayName = Dn("Field.Customer", "客戶"),
                            FilterPlaceholder = Fp("Field.Customer", "輸入客戶搜尋"),
                            TableOrder = 4,
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(ScaleRecord.CustomerId), sr => sr.CustomerId)
                        }
                    },
                    {
                        nameof(ScaleRecord.ItemId),
                        new FieldDefinition<ScaleRecord>
                        {
                            PropertyName = "Item.Name",
                            FilterPropertyName = nameof(ScaleRecord.ItemId),
                            DisplayName = "品項",
                            FilterPlaceholder = "輸入品項搜尋",
                            TableOrder = 5,
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(ScaleRecord.ItemId), sr => sr.ItemId)
                        }
                    },
                    {
                        nameof(ScaleRecord.EntryWeight),
                        new FieldDefinition<ScaleRecord>
                        {
                            PropertyName = nameof(ScaleRecord.EntryWeight),
                            DisplayName = "進場重量 (kg)",
                            TableOrder = 6,
                            ColumnType = ColumnDataType.Number,
                            ShowInFilter = false,
                            CustomTemplate = value => builder =>
                            {
                                if (value is ScaleRecord sr)
                                    builder.AddContent(0, NumberFormatHelper.FormatSmart(sr.EntryWeight));
                            }
                        }
                    },
                    {
                        nameof(ScaleRecord.ExitWeight),
                        new FieldDefinition<ScaleRecord>
                        {
                            PropertyName = nameof(ScaleRecord.ExitWeight),
                            DisplayName = "出場重量 (kg)",
                            TableOrder = 7,
                            ColumnType = ColumnDataType.Number,
                            ShowInFilter = false,
                            CustomTemplate = value => builder =>
                            {
                                if (value is ScaleRecord sr)
                                    builder.AddContent(0, NumberFormatHelper.FormatSmart(sr.ExitWeight));
                            }
                        }
                    },
                    {
                        nameof(ScaleRecord.NetWeight),
                        new FieldDefinition<ScaleRecord>
                        {
                            PropertyName = nameof(ScaleRecord.NetWeight),
                            DisplayName = "淨重 (kg)",
                            TableOrder = 8,
                            ColumnType = ColumnDataType.Number,
                            ShowInFilter = false,
                            CustomTemplate = value => builder =>
                            {
                                if (value is ScaleRecord sr)
                                    builder.AddContent(0, NumberFormatHelper.FormatSmart(sr.NetWeight));
                            }
                        }
                    },
                    {
                        nameof(ScaleRecord.WarehouseId),
                        new FieldDefinition<ScaleRecord>
                        {
                            PropertyName = "Warehouse.Name",
                            FilterPropertyName = nameof(ScaleRecord.WarehouseId),
                            DisplayName = Dn("Field.InboundWarehouse", "入庫倉庫"),
                            FilterPlaceholder = Fp("Field.InboundWarehouse", "輸入倉庫搜尋"),
                            TableOrder = 9,
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(ScaleRecord.WarehouseId), sr => sr.WarehouseId)
                        }
                    },
                    {
                        nameof(ScaleRecord.DisposalFee),
                        new FieldDefinition<ScaleRecord>
                        {
                            PropertyName = nameof(ScaleRecord.DisposalFee),
                            DisplayName = Dn("Field.DisposalFee", "處理費"),
                            TableOrder = 10,
                            ColumnType = ColumnDataType.Currency,
                            ShowInFilter = false,
                            CustomTemplate = value => builder =>
                            {
                                if (value is ScaleRecord sr)
                                    builder.AddContent(0, sr.DisposalFee.HasValue ? $"NT$ {NumberFormatHelper.FormatSmart(sr.DisposalFee)}" : "");
                            }
                        }
                    },
                    {
                        nameof(ScaleRecord.PurchaseFee),
                        new FieldDefinition<ScaleRecord>
                        {
                            PropertyName = nameof(ScaleRecord.PurchaseFee),
                            DisplayName = Dn("Field.PurchaseFee", "採購費"),
                            TableOrder = 11,
                            ColumnType = ColumnDataType.Currency,
                            ShowInFilter = false,
                            CustomTemplate = value => builder =>
                            {
                                if (value is ScaleRecord sr)
                                    builder.AddContent(0, sr.PurchaseFee.HasValue ? $"NT$ {NumberFormatHelper.FormatSmart(sr.PurchaseFee)}" : "");
                            }
                        }
                    },
                    {
                        nameof(ScaleRecord.NetAmount),
                        new FieldDefinition<ScaleRecord>
                        {
                            PropertyName = nameof(ScaleRecord.NetAmount),
                            DisplayName = Dn("Field.NetAmount", "淨額"),
                            TableOrder = 12,
                            ColumnType = ColumnDataType.Currency,
                            ShowInFilter = false,
                            CustomTemplate = value => builder =>
                            {
                                if (value is ScaleRecord sr)
                                    builder.AddContent(0, sr.NetAmount.HasValue ? $"NT$ {NumberFormatHelper.FormatSmart(sr.NetAmount)}" : "");
                            }
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "磅秤紀錄欄位配置初始化失敗");
                });

                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("磅秤紀錄欄位配置初始化失敗，已使用預設配置");
                    });
                }

                return new Dictionary<string, FieldDefinition<ScaleRecord>>();
            }
        }
    }
}
