using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Data.Entities;
using ERPCore2.Services;
using ERPCore2.Helpers;
using ERPCore2.Components.Shared.PageTemplate;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 倉庫內位置欄位配置
    /// </summary>
    public class WarehouseLocationFieldConfiguration : BaseFieldConfiguration<WarehouseLocation>
    {
        private readonly List<Warehouse> _warehouses;
        private readonly INotificationService? _notificationService;
        
        public WarehouseLocationFieldConfiguration(List<Warehouse> warehouses, INotificationService? notificationService = null)
        {
            _warehouses = warehouses;
            _notificationService = notificationService;
        }
        
        public override Dictionary<string, FieldDefinition<WarehouseLocation>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<WarehouseLocation>>
                {
                    {
                        nameof(WarehouseLocation.Code),
                        new FieldDefinition<WarehouseLocation>
                        {
                            PropertyName = nameof(WarehouseLocation.Code),
                            DisplayName = "庫位編號",
                            FilterPlaceholder = "輸入庫位編號搜尋",
                            TableOrder = 1,
                            FilterOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, "Code", wl => wl.Code, allowNull: true)
                        }
                    },
                    {
                        nameof(WarehouseLocation.Name),
                        new FieldDefinition<WarehouseLocation>
                        {
                            PropertyName = nameof(WarehouseLocation.Name),
                            DisplayName = "庫位名稱",
                            FilterPlaceholder = "輸入庫位名稱搜尋",
                            TableOrder = 2,
                            FilterOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, "Name", wl => wl.Name)
                        }
                    },
                    {
                        nameof(WarehouseLocation.WarehouseId),
                        new FieldDefinition<WarehouseLocation>
                        {
                            PropertyName = "Warehouse.Name", // 表格顯示用
                            FilterPropertyName = nameof(WarehouseLocation.WarehouseId), // 篩選器用
                            DisplayName = "所屬倉庫",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 3,
                            FilterOrder = 3,
                            Options = _warehouses.Select(w => new SelectOption 
                            { 
                                Text = w.Name, 
                                Value = w.Id.ToString() 
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, "WarehouseId", wl => wl.WarehouseId)
                        }
                    },
                    {
                        nameof(WarehouseLocation.Zone),
                        new FieldDefinition<WarehouseLocation>
                        {
                            PropertyName = nameof(WarehouseLocation.Zone),
                            DisplayName = "區域",
                            FilterPlaceholder = "輸入區域搜尋",
                            TableOrder = 4,
                            FilterOrder = 4,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, "Zone", wl => wl.Zone, allowNull: true)
                        }
                    },
                    {
                        nameof(WarehouseLocation.Aisle),
                        new FieldDefinition<WarehouseLocation>
                        {
                            PropertyName = nameof(WarehouseLocation.Aisle),
                            DisplayName = "排號",
                            FilterPlaceholder = "輸入排號搜尋",
                            TableOrder = 5,
                            FilterOrder = 5,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, "Aisle", wl => wl.Aisle, allowNull: true)
                        }
                    },
                    {
                        nameof(WarehouseLocation.Level),
                        new FieldDefinition<WarehouseLocation>
                        {
                            PropertyName = nameof(WarehouseLocation.Level),
                            DisplayName = "層號",
                            FilterPlaceholder = "輸入層號搜尋",
                            TableOrder = 6,
                            FilterOrder = 6,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, "Level", wl => wl.Level, allowNull: true)
                        }
                    },
                    {
                        nameof(WarehouseLocation.Position),
                        new FieldDefinition<WarehouseLocation>
                        {
                            PropertyName = nameof(WarehouseLocation.Position),
                            DisplayName = "位號",
                            FilterPlaceholder = "輸入位號搜尋",
                            TableOrder = 7,
                            FilterOrder = 7,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, "Position", wl => wl.Position, allowNull: true)
                        }
                    },
                    {
                        nameof(WarehouseLocation.MaxCapacity),
                        new FieldDefinition<WarehouseLocation>
                        {
                            PropertyName = nameof(WarehouseLocation.MaxCapacity),
                            DisplayName = "最大容量",
                            FilterType = SearchFilterType.Number,
                            FilterPlaceholder = "輸入最大容量搜尋",
                            TableOrder = 8,
                            FilterOrder = 8,
                            FilterFunction = (model, query) => 
                            {
                                var filterValue = model.GetFilterValue("MaxCapacity")?.ToString();
                                if (!string.IsNullOrWhiteSpace(filterValue) && int.TryParse(filterValue, out var capacity))
                                {
                                    return query.Where(wl => wl.MaxCapacity == capacity);
                                }
                                return query;
                            }
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                // 記錄錯誤並通知使用者
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(
                        ex, 
                        nameof(GetFieldDefinitions), 
                        GetType(),
                        additionalData: "建立倉庫內位置欄位定義失敗"
                    );
                    
                    if (_notificationService != null)
                    {
                        await _notificationService.ShowErrorAsync("載入欄位配置失敗，請重新整理頁面");
                    }
                });

                // 提供安全的後備值
                return new Dictionary<string, FieldDefinition<WarehouseLocation>>();
            }
        }
    }
}


