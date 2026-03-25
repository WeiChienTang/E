using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Data.Entities;
using ERPCore2.Services;
using ERPCore2.Helpers;
using Microsoft.AspNetCore.Components;
using ERPCore2.Components.Shared.Modal;
using ERPCore2.Components.Shared.Table;
using ERPCore2.Components.Shared.Page;
using ERPCore2.Components.Shared.Statistics;
namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 品項欄位配置
    /// </summary>
    public class ItemFieldConfiguration : BaseFieldConfiguration<Item>
    {
        private readonly List<ItemCategory> _productCategories;
        private readonly List<Supplier> _suppliers;
        private readonly List<Unit> _units;
        private readonly List<Size> _sizes;
        private readonly INotificationService? _notificationService;
        
        public ItemFieldConfiguration(
            List<ItemCategory> productCategories, 
            List<Supplier> suppliers,
            List<Unit> units,
            List<Size> sizes,
            INotificationService? notificationService = null)
        {
            _productCategories = productCategories;
            _suppliers = suppliers;
            _units = units;
            _sizes = sizes;
            _notificationService = notificationService;
        }
        
        public override Dictionary<string, FieldDefinition<Item>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<Item>>
                {
                    {
                        nameof(Item.Code),
                        new FieldDefinition<Item>
                        {
                            PropertyName = nameof(Item.Code),
                            DisplayName = Dn("Field.ItemCode", "品項編號"),
                            FilterPlaceholder = Fp("Field.ItemCode", "輸入品項編號搜尋"),
                            TableOrder = 1,
                            Width = "120px",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Item.Code), p => p.Code)
                        }
                    },
                    {
                        nameof(Item.Name),
                        new FieldDefinition<Item>
                        {
                            PropertyName = nameof(Item.Name),
                            DisplayName = Dn("Field.ItemName", "品項名稱"),
                            FilterPlaceholder = Fp("Field.ItemName", "輸入品項名稱搜尋"),
                            TableOrder = 2,
                            Width = "150px",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Item.Name), p => p.Name)
                        }
                    },
                    {
                        nameof(Item.Barcode),
                        new FieldDefinition<Item>
                        {
                            PropertyName = nameof(Item.Barcode),
                            DisplayName = Dn("Field.Barcode", "條碼編號"),
                            FilterPlaceholder = Fp("Field.Barcode", "輸入條碼編號搜尋"),
                            TableOrder = 3,
                            Width = "130px",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Item.Barcode), p => p.Barcode)
                        }
                    },
                    {
                        nameof(Item.SizeId),
                        new FieldDefinition<Item>
                        {
                            PropertyName = "Size.Name", // 表格顯示用
                            FilterPropertyName = nameof(Item.SizeId), // 篩選器用
                            DisplayName = Dn("Field.Size", "尺寸"),
                            FilterType = SearchFilterType.Select,
                            TableOrder = 4,
                            Width = "90px",
                            Options = _sizes.Select(s => new SelectOption
                            {
                                Text = s.Name!,
                                Value = s.Id.ToString()
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(Item.SizeId), p => p.SizeId)
                        }
                    },
                    {
                        nameof(Item.ItemCategoryId),
                        new FieldDefinition<Item>
                        {
                            PropertyName = "ItemCategory.Name", // 表格顯示用
                            FilterPropertyName = nameof(Item.ItemCategoryId), // 篩選器用
                            DisplayName = Dn("Field.CategoryName", "品項分類"),
                            FilterType = SearchFilterType.Select,
                            TableOrder = 5,
                            Width = "120px",
                            Options = _productCategories.Select(pc => new SelectOption
                            {
                                Text = pc.Name!,
                                Value = pc.Id.ToString()
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(Item.ItemCategoryId), p => p.ItemCategoryId)
                        }
                    },
                    {
                        nameof(Item.UnitId),
                        new FieldDefinition<Item>
                        {
                            PropertyName = "Unit.Name", // 表格顯示用
                            FilterPropertyName = nameof(Item.UnitId), // 篩選器用
                            DisplayName = Dn("Field.PurchaseUnit", "採購單位"),
                            FilterType = SearchFilterType.Select,
                            TableOrder = 6,
                            Width = "100px",
                            Options = _units.Select(u => new SelectOption
                            {
                                Text = u.Name!,
                                Value = u.Id.ToString()
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(Item.UnitId), p => p.UnitId)
                        }
                    },
                    {
                        nameof(Item.ProductionUnitId),
                        new FieldDefinition<Item>
                        {
                            PropertyName = "ProductionUnit.Name", // 表格顯示用
                            FilterPropertyName = nameof(Item.ProductionUnitId), // 篩選器用
                            DisplayName = Dn("Field.ProductionUnit", "製程單位"),
                            FilterType = SearchFilterType.Select,
                            TableOrder = 7,
                            Width = "100px",
                            Options = _units.Select(u => new SelectOption
                            {
                                Text = u.Name!,
                                Value = u.Id.ToString()
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(Item.ProductionUnitId), p => p.ProductionUnitId)
                        }
                    },
                };
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "品項欄位配置初始化失敗");
                });

                // 通知使用者
                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("品項欄位配置初始化失敗，已使用預設配置");
                    });
                }
                
                // 回傳安全的預設配置
                return new Dictionary<string, FieldDefinition<Item>>();
            }
        }
        
    }
}


