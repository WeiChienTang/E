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
    /// 倉庫欄位配置
    /// </summary>
    public class WarehouseFieldConfiguration : BaseFieldConfiguration<Warehouse>
    {
        private readonly INotificationService? _notificationService;
        
        public WarehouseFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }
        
        public override Dictionary<string, FieldDefinition<Warehouse>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<Warehouse>>
                {
                    {
                        nameof(Warehouse.Code),
                        new FieldDefinition<Warehouse>
                        {
                            PropertyName = nameof(Warehouse.Code),
                            DisplayName = Dn("Field.WarehouseCode", "倉庫編號"),
                            FilterPlaceholder = Fp("Field.WarehouseCode", "輸入倉庫編號搜尋"),
                            TableOrder = 1,
                            FilterOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, "Code", w => w.Code, allowNull: true)
                        }
                    },
                    {
                        nameof(Warehouse.Name),
                        new FieldDefinition<Warehouse>
                        {
                            PropertyName = nameof(Warehouse.Name),
                            DisplayName = Dn("Field.WarehouseName", "倉庫名稱"),
                            FilterPlaceholder = Fp("Field.WarehouseName", "輸入倉庫名稱搜尋"),
                            TableOrder = 2,
                            FilterOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, "Name", w => w.Name)
                        }
                    },
                    {
                        nameof(Warehouse.ContactPerson),
                        new FieldDefinition<Warehouse>
                        {
                            PropertyName = nameof(Warehouse.ContactPerson),
                            DisplayName = Dn("Field.ContactPerson", "聯絡人"),
                            FilterPlaceholder = Fp("Field.ContactPerson", "輸入聯絡人姓名搜尋"),
                            TableOrder = 3,
                            FilterOrder = 3,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, "ContactPerson", w => w.ContactPerson, allowNull: true)
                        }
                    },
                    {
                        nameof(Warehouse.Phone),
                        new FieldDefinition<Warehouse>
                        {
                            PropertyName = nameof(Warehouse.Phone),
                            DisplayName = Dn("Field.ContactPhone", "聯絡電話"),
                            TableOrder = 4,
                            FilterOrder = 0, // 不在篩選器中顯示
                            ShowInFilter = false
                        }
                    },
                    {
                        nameof(Warehouse.Address),
                        new FieldDefinition<Warehouse>
                        {
                            PropertyName = nameof(Warehouse.Address),
                            DisplayName = Dn("Field.Address", "地址"),
                            FilterPlaceholder = Fp("Field.Address", "輸入地址搜尋"),
                            TableOrder = 5,
                            FilterOrder = 4,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, "Address", w => w.Address, allowNull: true)
                        }
                    },
                };
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "倉庫欄位配置初始化失敗");
                });

                // 通知使用者
                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("倉庫欄位配置初始化失敗，已使用預設配置");
                    });
                }
                
                // 回傳安全的預設配置
                return new Dictionary<string, FieldDefinition<Warehouse>>();
            }
        }
    }
}


