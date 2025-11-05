using ERPCore2.Data;
using ERPCore2.Components.Shared.Forms;
using ERPCore2.Components.Shared.Modals;
using ERPCore2.Components.Shared.PageModel.EditModalComponent;
using ERPCore2.Services;

namespace ERPCore2.Helpers;

/// <summary>
/// RelatedEntityModalManager 的統一初始化 Helper
/// 用於簡化在 EditModal 組件中建立和配置 Modal 管理器的重複程式碼
/// </summary>
public static class RelatedEntityModalManagerHelper
{
    /// <summary>
    /// 建立標準的 RelatedEntityModalManager
    /// 適用於大多數場景的預設配置
    /// </summary>
    /// <typeparam name="TEntity">主實體類型（如 ProductComposition）</typeparam>
    /// <typeparam name="TRelatedEntity">相關實體類型（如 Product, Customer, Employee）</typeparam>
    /// <typeparam name="TMainService">主 EditModal 的服務類型</typeparam>
    /// <param name="config">配置參數</param>
    /// <returns>配置好的 RelatedEntityModalManager</returns>
    public static RelatedEntityModalManager<TRelatedEntity> CreateStandardManager<TEntity, TRelatedEntity, TMainService>(
        StandardModalManagerConfig<TEntity, TRelatedEntity, TMainService> config)
        where TEntity : BaseEntity, new()
        where TRelatedEntity : BaseEntity
        where TMainService : class
    {
        var builder = new RelatedEntityManagerBuilder<TRelatedEntity>(
            config.NotificationService, 
            config.EntityDisplayName)
            .WithPropertyName(config.PropertyName);
        
        // 設定重新載入資料回調
        if (config.ReloadDataCallback != null)
        {
            builder.WithReloadCallback(config.ReloadDataCallback);
        }
        
        // 設定狀態變更回調
        if (config.StateChangedCallback != null)
        {
            builder.WithStateChangedCallback(config.StateChangedCallback);
        }

        // 設定自動選擇回調
        if (config.AutoSelectAction != null)
        {
            builder.WithAutoSelectCallback(entityId =>
            {
                // 使用閉包延遲讀取 Entity，在執行時才取得最新值
                var editModal = config.GetEditModalComponent?.Invoke();
                var entity = editModal?.Entity;
                config.AutoSelectAction?.Invoke(entity, entityId);
            });
        }

        // 設定儲存後的處理
        if (config.InitializeFormFieldsCallback != null)
        {
            builder.WithCustomPostProcess(async entity =>
            {
                await config.InitializeFormFieldsCallback.Invoke();
            });
        }

        // 設定刷新相依組件（使用閉包延遲讀取，避免初始化時 editModalComponent 為 null）
        if (config.RefreshAutoCompleteFields)
        {
            builder.WithRefreshDependentComponents(async entity =>
            {
                // 執行時才讀取 editModalComponent，確保能取得最新值
                var editModal = config.GetEditModalComponent?.Invoke();
                if (editModal != null)
                {
                    await editModal.RefreshAutoCompleteFieldsAsync();
                }
            });
        }

        return builder.Build();
    }
}

/// <summary>
/// 標準 Modal 管理器配置
/// </summary>
/// <typeparam name="TEntity">主實體類型</typeparam>
/// <typeparam name="TRelatedEntity">相關實體類型</typeparam>
/// <typeparam name="TMainService">主 EditModal 的服務類型</typeparam>
public class StandardModalManagerConfig<TEntity, TRelatedEntity, TMainService>
    where TEntity : BaseEntity, new()
    where TRelatedEntity : BaseEntity
    where TMainService : class
{
    /// <summary>
    /// 通知服務
    /// </summary>
    public required INotificationService NotificationService { get; set; }

    /// <summary>
    /// 實體顯示名稱（如 "商品"、"客戶"）
    /// </summary>
    public required string EntityDisplayName { get; set; }

    /// <summary>
    /// 屬性名稱（如 nameof(ProductComposition.ParentProductId)）
    /// </summary>
    public required string PropertyName { get; set; }

    /// <summary>
    /// EditModal 組件參考取得器（使用 Func 延遲取得，避免初始化時為 null）
    /// </summary>
    public Func<GenericEditModalComponent<TEntity, TMainService>?>? GetEditModalComponent { get; set; }

    /// <summary>
    /// 重新載入資料的回調
    /// </summary>
    public Func<Task>? ReloadDataCallback { get; set; }

    /// <summary>
    /// 狀態變更回調
    /// </summary>
    public Action? StateChangedCallback { get; set; }

    /// <summary>
    /// 初始化表單欄位回調
    /// </summary>
    public Func<Task>? InitializeFormFieldsCallback { get; set; }

    /// <summary>
    /// 自動選擇動作（將選中的 ID 設定到主實體的對應屬性）
    /// </summary>
    public Action<TEntity?, int>? AutoSelectAction { get; set; }

    /// <summary>
    /// 是否刷新 AutoComplete 欄位（預設為 true）
    /// </summary>
    public bool RefreshAutoCompleteFields { get; set; } = true;
}
