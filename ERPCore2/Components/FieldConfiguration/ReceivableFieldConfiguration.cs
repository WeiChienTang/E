using ERPCore2.Components.Shared.Forms;
using ERPCore2.Components.Shared.Tables;
using ERPCore2.FieldConfiguration;
using ERPCore2.Helpers;
using ERPCore2.Models;
using ERPCore2.Services;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 應收沖款欄位配置類別
    /// </summary>
    public class ReceivableFieldConfiguration : BaseFieldConfiguration<ReceivableViewModel>
    {
        private readonly INotificationService? _notificationService;

        public ReceivableFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<ReceivableViewModel>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<ReceivableViewModel>>
                {
                    {
                        nameof(ReceivableViewModel.DocumentType),
                        new FieldDefinition<ReceivableViewModel>
                        {
                            PropertyName = nameof(ReceivableViewModel.DocumentType),
                            DisplayName = "單據類型",
                            FilterType = SearchFilterType.Select,
                            ColumnType = ColumnDataType.Text,
                            TableOrder = 1,
                            FilterOrder = 1,
                            HeaderStyle = "width: 120px;",
                            Options = new List<SelectOption>
                            {
                                new SelectOption { Text = "銷貨訂單", Value = "SalesOrder" },
                                new SelectOption { Text = "採購退回", Value = "PurchaseReturn" }
                            }
                        }
                    },
                    {
                        nameof(ReceivableViewModel.DocumentNumber),
                        new FieldDefinition<ReceivableViewModel>
                        {
                            PropertyName = nameof(ReceivableViewModel.DocumentNumber),
                            DisplayName = "單據編號",
                            FilterPlaceholder = "輸入單據編號搜尋",
                            TableOrder = 2,
                            FilterOrder = 2,
                            HeaderStyle = "width: 150px;"
                        }
                    },
                    {
                        nameof(ReceivableViewModel.DocumentDate),
                        new FieldDefinition<ReceivableViewModel>
                        {
                            PropertyName = nameof(ReceivableViewModel.DocumentDate),
                            DisplayName = "單據日期",
                            FilterType = SearchFilterType.DateRange,
                            ColumnType = ColumnDataType.Date,
                            TableOrder = 3,
                            FilterOrder = 3,
                            HeaderStyle = "width: 120px;"
                        }
                    },
                    {
                        nameof(ReceivableViewModel.CustomerOrSupplier),
                        new FieldDefinition<ReceivableViewModel>
                        {
                            PropertyName = nameof(ReceivableViewModel.CustomerOrSupplier),
                            DisplayName = "往來對象",
                            FilterPlaceholder = "輸入客戶或供應商名稱",
                            TableOrder = 4,
                            FilterOrder = 4,
                            HeaderStyle = "width: 200px;"
                        }
                    },
                    {
                        nameof(ReceivableViewModel.ProductName),
                        new FieldDefinition<ReceivableViewModel>
                        {
                            PropertyName = nameof(ReceivableViewModel.ProductName),
                            DisplayName = "商品名稱",
                            FilterPlaceholder = "輸入商品名稱搜尋",
                            TableOrder = 5,
                            FilterOrder = 5,
                            HeaderStyle = "width: 200px;"
                        }
                    },
                    {
                        nameof(ReceivableViewModel.TotalAmount),
                        new FieldDefinition<ReceivableViewModel>
                        {
                            PropertyName = nameof(ReceivableViewModel.TotalAmount),
                            DisplayName = "應收總額",
                            FilterType = SearchFilterType.NumberRange,
                            ColumnType = ColumnDataType.Currency,
                            TableOrder = 6,
                            FilterOrder = 6,
                            HeaderStyle = "width: 120px; text-align: right;"
                        }
                    },
                    {
                        nameof(ReceivableViewModel.TotalReceivedAmount),
                        new FieldDefinition<ReceivableViewModel>
                        {
                            PropertyName = nameof(ReceivableViewModel.TotalReceivedAmount),
                            DisplayName = "累計收款",
                            ColumnType = ColumnDataType.Currency,
                            TableOrder = 7,
                            FilterOrder = 7,
                            ShowInFilter = false,
                            HeaderStyle = "width: 120px; text-align: right;"
                        }
                    },
                    {
                        nameof(ReceivableViewModel.BalanceAmount),
                        new FieldDefinition<ReceivableViewModel>
                        {
                            PropertyName = nameof(ReceivableViewModel.BalanceAmount),
                            DisplayName = "餘額",
                            ColumnType = ColumnDataType.Currency,
                            TableOrder = 8,
                            FilterOrder = 8,
                            HeaderStyle = "width: 120px; text-align: right;"
                        }
                    },
                    {
                        nameof(ReceivableViewModel.IsSettled),
                        new FieldDefinition<ReceivableViewModel>
                        {
                            PropertyName = nameof(ReceivableViewModel.IsSettled),
                            DisplayName = "結清狀態",
                            FilterType = SearchFilterType.Select,
                            ColumnType = ColumnDataType.Boolean,
                            TableOrder = 9,
                            FilterOrder = 9,
                            HeaderStyle = "width: 100px; text-align: center;",
                            Options = new List<SelectOption>
                            {
                                new SelectOption { Text = "未結清", Value = "false" },
                                new SelectOption { Text = "已結清", Value = "true" }
                            }
                        }
                    },
                    {
                        nameof(ReceivableViewModel.StatusText),
                        new FieldDefinition<ReceivableViewModel>
                        {
                            PropertyName = nameof(ReceivableViewModel.StatusText),
                            DisplayName = "狀態",
                            FilterType = SearchFilterType.Select,
                            ColumnType = ColumnDataType.Text,
                            TableOrder = 10,
                            FilterOrder = 10,
                            HeaderStyle = "width: 100px; text-align: center;",
                            Options = new List<SelectOption>
                            {
                                new SelectOption { Text = "未收款", Value = "未收款" },
                                new SelectOption { Text = "部分收款", Value = "部分收款" },
                                new SelectOption { Text = "已結清", Value = "已結清" },
                                new SelectOption { Text = "逾期", Value = "逾期" }
                            }
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                // 錯誤處理
                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await ErrorHandlingHelper.HandleServiceErrorAsync(
                            ex, nameof(GetFieldDefinitions), GetType());
                        await _notificationService.ShowErrorAsync("欄位配置載入失敗");
                    });
                }

                // 返回安全的後備配置
                return new Dictionary<string, FieldDefinition<ReceivableViewModel>>();
            }
        }

        /// <summary>
        /// 自訂預設排序：依單據日期遞減，然後依單據編號
        /// </summary>
        protected override Func<IQueryable<ReceivableViewModel>, IQueryable<ReceivableViewModel>> GetDefaultSort()
        {
            return query => query.OrderByDescending(r => r.DocumentDate)
                                .ThenBy(r => r.DocumentNumber);
        }
    }
}