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
    /// 廢料記錄欄位配置
    /// </summary>
    public class WasteRecordFieldConfiguration : BaseFieldConfiguration<WasteRecord>
    {
        private readonly INotificationService? _notificationService;

        public WasteRecordFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<WasteRecord>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<WasteRecord>>
                {
                    {
                        nameof(WasteRecord.Code),
                        new FieldDefinition<WasteRecord>
                        {
                            PropertyName = nameof(WasteRecord.Code),
                            DisplayName = Dn("Field.WasteRecordCode", "廢料單號"),
                            FilterPlaceholder = Fp("Field.WasteRecordCode", "輸入單號搜尋"),
                            TableOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(WasteRecord.Code), wr => wr.Code, allowNull: true)
                        }
                    },
                    {
                        nameof(WasteRecord.RecordDate),
                        new FieldDefinition<WasteRecord>
                        {
                            PropertyName = nameof(WasteRecord.RecordDate),
                            DisplayName = Dn("Field.RecordDate", "記錄日期"),
                            FilterPlaceholder = Fp("Field.RecordDate", "選擇日期範圍"),
                            TableOrder = 2,
                            FilterType = SearchFilterType.DateRange,
                            ColumnType = ColumnDataType.Date,
                            FilterFunction = (model, query) => FilterHelper.ApplyDateRangeFilter(
                                model, query, nameof(WasteRecord.RecordDate), wr => wr.RecordDate)
                        }
                    },
                    {
                        nameof(WasteRecord.VehicleId),
                        new FieldDefinition<WasteRecord>
                        {
                            PropertyName = "Vehicle.LicensePlate",
                            FilterPropertyName = nameof(WasteRecord.VehicleId),
                            DisplayName = Dn("Field.Vehicle", "車輛"),
                            FilterPlaceholder = Fp("Field.Vehicle", "輸入車牌搜尋"),
                            TableOrder = 3,
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(WasteRecord.VehicleId), wr => wr.VehicleId)
                        }
                    },
                    {
                        nameof(WasteRecord.WasteTypeId),
                        new FieldDefinition<WasteRecord>
                        {
                            PropertyName = "WasteType.Name",
                            FilterPropertyName = nameof(WasteRecord.WasteTypeId),
                            DisplayName = Dn("Field.WasteType", "廢料類型"),
                            FilterPlaceholder = Fp("Field.WasteType", "選擇廢料類型"),
                            TableOrder = 4,
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(WasteRecord.WasteTypeId), wr => wr.WasteTypeId)
                        }
                    },
                    {
                        nameof(WasteRecord.CustomerId),
                        new FieldDefinition<WasteRecord>
                        {
                            PropertyName = "Customer.CompanyName",
                            FilterPropertyName = nameof(WasteRecord.CustomerId),
                            DisplayName = Dn("Field.Customer", "客戶"),
                            FilterPlaceholder = Fp("Field.Customer", "輸入客戶搜尋"),
                            TableOrder = 5,
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(WasteRecord.CustomerId), wr => wr.CustomerId)
                        }
                    },
                    {
                        nameof(WasteRecord.WarehouseId),
                        new FieldDefinition<WasteRecord>
                        {
                            PropertyName = "Warehouse.Name",
                            FilterPropertyName = nameof(WasteRecord.WarehouseId),
                            DisplayName = Dn("Field.InboundWarehouse", "入庫倉庫"),
                            FilterPlaceholder = Fp("Field.InboundWarehouse", "輸入倉庫搜尋"),
                            TableOrder = 6,
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(WasteRecord.WarehouseId), wr => wr.WarehouseId)
                        }
                    },
                    {
                        nameof(WasteRecord.TotalWeight),
                        new FieldDefinition<WasteRecord>
                        {
                            PropertyName = nameof(WasteRecord.TotalWeight),
                            DisplayName = Dn("Field.TotalWeight", "總重量"),
                            TableOrder = 7,
                            ColumnType = ColumnDataType.Number,
                            ShowInFilter = false,
                            CustomTemplate = value => builder =>
                            {
                                if (value is WasteRecord wr)
                                {
                                    builder.AddContent(0, NumberFormatHelper.FormatSmart(wr.TotalWeight));
                                }
                            }
                        }
                    },
                    {
                        nameof(WasteRecord.DisposalFee),
                        new FieldDefinition<WasteRecord>
                        {
                            PropertyName = nameof(WasteRecord.DisposalFee),
                            DisplayName = Dn("Field.DisposalFee", "處理費"),
                            TableOrder = 8,
                            ColumnType = ColumnDataType.Currency,
                            ShowInFilter = false,
                            CustomTemplate = value => builder =>
                            {
                                if (value is WasteRecord wr)
                                {
                                    builder.AddContent(0, wr.DisposalFee.HasValue ? $"NT$ {NumberFormatHelper.FormatSmart(wr.DisposalFee)}" : "");
                                }
                            }
                        }
                    },
                    {
                        nameof(WasteRecord.PurchaseFee),
                        new FieldDefinition<WasteRecord>
                        {
                            PropertyName = nameof(WasteRecord.PurchaseFee),
                            DisplayName = Dn("Field.PurchaseFee", "採購費"),
                            TableOrder = 9,
                            ColumnType = ColumnDataType.Currency,
                            ShowInFilter = false,
                            CustomTemplate = value => builder =>
                            {
                                if (value is WasteRecord wr)
                                {
                                    builder.AddContent(0, wr.PurchaseFee.HasValue ? $"NT$ {NumberFormatHelper.FormatSmart(wr.PurchaseFee)}" : "");
                                }
                            }
                        }
                    },
                    {
                        nameof(WasteRecord.NetAmount),
                        new FieldDefinition<WasteRecord>
                        {
                            PropertyName = nameof(WasteRecord.NetAmount),
                            DisplayName = Dn("Field.NetAmount", "淨額"),
                            TableOrder = 10,
                            ColumnType = ColumnDataType.Currency,
                            ShowInFilter = false,
                            CustomTemplate = value => builder =>
                            {
                                if (value is WasteRecord wr)
                                {
                                    builder.AddContent(0, wr.NetAmount.HasValue ? $"NT$ {NumberFormatHelper.FormatSmart(wr.NetAmount)}" : "");
                                }
                            }
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "廢料記錄欄位配置初始化失敗");
                });

                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("廢料記錄欄位配置初始化失敗，已使用預設配置");
                    });
                }

                return new Dictionary<string, FieldDefinition<WasteRecord>>();
            }
        }
    }
}
