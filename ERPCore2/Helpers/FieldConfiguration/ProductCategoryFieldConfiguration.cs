using ERPCore2.Components.Shared.Forms;
using ERPCore2.Data.Entities;
using ERPCore2.Services;

namespace ERPCore2.Helpers
{
    /// <summary>
    /// 產品分類欄位配置
    /// </summary>
    public class ProductCategoryFieldConfiguration : BaseFieldConfiguration<ProductCategory>
    {
        private readonly INotificationService? _notificationService;
        
        public ProductCategoryFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }
        
        public override Dictionary<string, FieldDefinition<ProductCategory>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<ProductCategory>>
                {
                    {
                        nameof(ProductCategory.Code),
                        new FieldDefinition<ProductCategory>
                        {
                            PropertyName = nameof(ProductCategory.Code),
                            DisplayName = "分類代碼",
                            FilterPlaceholder = "輸入分類代碼搜尋",
                            TableOrder = 1,
                            FilterOrder = 1,
                            HeaderStyle = "width: 180px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(ProductCategory.Code), pc => pc.Code, allowNull: true)
                        }
                    },
                    {
                        nameof(ProductCategory.CategoryName),
                        new FieldDefinition<ProductCategory>
                        {
                            PropertyName = nameof(ProductCategory.CategoryName),
                            DisplayName = "分類名稱",
                            FilterPlaceholder = "輸入分類名稱搜尋",
                            TableOrder = 2,
                            FilterOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(ProductCategory.CategoryName), pc => pc.CategoryName)
                        }
                    },
                    {
                        nameof(ProductCategory.Remarks),
                        new FieldDefinition<ProductCategory>
                        {
                            PropertyName = nameof(ProductCategory.Remarks),
                            DisplayName = "備註",
                            FilterPlaceholder = "輸入備註搜尋",
                            TableOrder = 3,
                            FilterOrder = 3,
                            ShowInFilter = false, // 不在篩選器中顯示
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(ProductCategory.Remarks), pc => pc.Remarks, allowNull: true)
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "產品分類欄位配置初始化失敗");
                });

                // 通知使用者
                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("產品分類欄位配置初始化失敗，已使用預設配置");
                    });
                }
                
                // 回傳安全的預設配置
                return new Dictionary<string, FieldDefinition<ProductCategory>>();
            }
        }
    }
}
