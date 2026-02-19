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
                            DisplayName = "廢料單號",
                            FilterPlaceholder = "輸入單號搜尋",
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
                            DisplayName = "記錄日期",
                            FilterPlaceholder = "選擇日期範圍",
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
                            DisplayName = "車輛",
                            FilterPlaceholder = "輸入車牌搜尋",
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
                            DisplayName = "廢料類型",
                            FilterPlaceholder = "選擇廢料類型",
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
                            DisplayName = "客戶",
                            FilterPlaceholder = "輸入客戶搜尋",
                            TableOrder = 5,
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(WasteRecord.CustomerId), wr => wr.CustomerId)
                        }
                    },
                    {
                        nameof(WasteRecord.TotalWeight),
                        new FieldDefinition<WasteRecord>
                        {
                            PropertyName = nameof(WasteRecord.TotalWeight),
                            DisplayName = "總重量(kg)",
                            TableOrder = 6,
                            ColumnType = ColumnDataType.Number,
                            ShowInFilter = false
                        }
                    },
                    {
                        nameof(WasteRecord.DisposalFee),
                        new FieldDefinition<WasteRecord>
                        {
                            PropertyName = nameof(WasteRecord.DisposalFee),
                            DisplayName = "處理費",
                            TableOrder = 7,
                            ColumnType = ColumnDataType.Currency,
                            ShowInFilter = false
                        }
                    },
                    {
                        nameof(WasteRecord.PurchaseFee),
                        new FieldDefinition<WasteRecord>
                        {
                            PropertyName = nameof(WasteRecord.PurchaseFee),
                            DisplayName = "採購費",
                            TableOrder = 8,
                            ColumnType = ColumnDataType.Currency,
                            ShowInFilter = false
                        }
                    },
                    {
                        nameof(WasteRecord.NetAmount),
                        new FieldDefinition<WasteRecord>
                        {
                            PropertyName = nameof(WasteRecord.NetAmount),
                            DisplayName = "淨額",
                            TableOrder = 9,
                            ColumnType = ColumnDataType.Currency,
                            ShowInFilter = false
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
