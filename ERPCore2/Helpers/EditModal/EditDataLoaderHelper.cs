using ERPCore2.Data;
using ERPCore2.Models.Enums;
using ERPCore2.Services;

namespace ERPCore2.Helpers.EditModal;

/// <summary>
/// DataLoader 共用輔助類別
/// 封裝 EditModalComponent 中 LoadXxxData 方法的共用模式：
/// 新增模式 → 產生編號 + 預設狀態 + 預填值
/// 編輯模式 → GetByIdAsync 載入
/// 錯誤處理 → ShowErrorAsync + 安全預設值
/// </summary>
public static class EditDataLoaderHelper
{
    /// <summary>
    /// 載入或建立實體（使用字串前綴產生編號）
    /// </summary>
    /// <typeparam name="TEntity">實體類型（需繼承 BaseEntity）</typeparam>
    /// <typeparam name="TService">服務類型（需實作 IGenericManagementService）</typeparam>
    /// <param name="entityId">實體 ID（null 表示新增模式）</param>
    /// <param name="service">服務實例</param>
    /// <param name="notificationService">通知服務</param>
    /// <param name="entityDisplayName">實體顯示名稱（用於錯誤訊息）</param>
    /// <param name="codePrefix">編號前綴</param>
    /// <param name="prefilledValues">預填值</param>
    /// <param name="additionalInit">額外初始化動作（在預填值之前執行）</param>
    public static async Task<TEntity?> LoadOrCreateAsync<TEntity, TService>(
        int? entityId,
        TService service,
        INotificationService notificationService,
        string entityDisplayName,
        string codePrefix,
        Dictionary<string, object?>? prefilledValues = null,
        Action<TEntity>? additionalInit = null)
        where TEntity : BaseEntity, new()
        where TService : class, IGenericManagementService<TEntity>
    {
        try
        {
            if (!entityId.HasValue)
            {
                var newEntity = new TEntity
                {
                    Code = await EntityCodeGenerationHelper.GenerateForEntity<TEntity, TService>(service, codePrefix),
                    Status = EntityStatus.Active
                };

                additionalInit?.Invoke(newEntity);
                PrefilledValueHelper.ApplyPrefilledValues(newEntity, prefilledValues);

                return newEntity;
            }

            return await service.GetByIdAsync(entityId.Value)
                   ?? new TEntity { Status = EntityStatus.Active };
        }
        catch (Exception ex)
        {
            await notificationService.ShowErrorAsync($"載入{entityDisplayName}資料時發生錯誤：{ex.Message}");
            return new TEntity { Status = EntityStatus.Active };
        }
    }
}
