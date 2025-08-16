using ERPCore2.Components.Shared.Forms;
using ERPCore2.Data.Entities;
using ERPCore2.Services;

namespace ERPCore2.Helpers
{
    /// <summary>
    /// 廠商欄位配置
    /// </summary>
    public class SupplierFieldConfiguration : BaseFieldConfiguration<Supplier>
    {
        private readonly List<SupplierType> _supplierTypes;
        private readonly INotificationService? _notificationService;
        
        public SupplierFieldConfiguration(List<SupplierType> supplierTypes, INotificationService? notificationService = null)
        {
            _supplierTypes = supplierTypes;
            _notificationService = notificationService;
        }
        
        public override Dictionary<string, FieldDefinition<Supplier>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<Supplier>>
                {
                    {
                        nameof(Supplier.Code),
                        new FieldDefinition<Supplier>
                        {
                            PropertyName = nameof(Supplier.Code),
                            DisplayName = "廠商代碼",
                            FilterPlaceholder = "輸入廠商代碼搜尋",
                            TableOrder = 1,
                            FilterOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Supplier.Code), s => s.Code)
                        }
                    },
                    {
                        nameof(Supplier.CompanyName),
                        new FieldDefinition<Supplier>
                        {
                            PropertyName = nameof(Supplier.CompanyName),
                            DisplayName = "公司名稱",
                            FilterPlaceholder = "輸入公司名稱搜尋",
                            TableOrder = 2,
                            FilterOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Supplier.CompanyName), s => s.CompanyName)
                        }
                    },
                    {
                        nameof(Supplier.ContactPerson),
                        new FieldDefinition<Supplier>
                        {
                            PropertyName = nameof(Supplier.ContactPerson),
                            DisplayName = "聯絡人",
                            FilterPlaceholder = "輸入聯絡人姓名搜尋",
                            TableOrder = 3,
                            FilterOrder = 3,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Supplier.ContactPerson), s => s.ContactPerson, allowNull: true)
                        }
                    },
                    {
                        nameof(Supplier.SupplierTypeId),
                        new FieldDefinition<Supplier>
                        {
                            PropertyName = "SupplierType.TypeName", // 用於表格顯示
                            FilterPropertyName = nameof(Supplier.SupplierTypeId), // 用於篩選器
                            DisplayName = "廠商類型",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 4,
                            FilterOrder = 4,
                            Options = _supplierTypes.Select(st => new SelectOption 
                            { 
                                Text = st.TypeName, 
                                Value = st.Id.ToString() 
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(Supplier.SupplierTypeId), s => s.SupplierTypeId)
                        }
                    },
                    {
                        nameof(Supplier.TaxNumber),
                        new FieldDefinition<Supplier>
                        {
                            PropertyName = nameof(Supplier.TaxNumber),
                            DisplayName = "統一編號",
                            FilterPlaceholder = "輸入統一編號搜尋",
                            TableOrder = 5,
                            FilterOrder = 5,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Supplier.TaxNumber), s => s.TaxNumber, allowNull: true)
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: new { SupplierTypesCount = _supplierTypes?.Count ?? 0 });
                });

                // 通知使用者
                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("初始化廠商欄位配置時發生錯誤，已使用預設配置");
                    });
                }

                // 回傳空的配置，讓頁面使用預設行為
                return new Dictionary<string, FieldDefinition<Supplier>>();
            }
        }
    }
}
