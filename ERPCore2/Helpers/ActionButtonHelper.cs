using ERPCore2.Components.Shared.PageModel;
using ERPCore2.Components.Shared.Forms;
using ERPCore2.Data;
using ERPCore2.Data.Entities;
using ERPCore2.Models;
using ERPCore2.Services;
using System.Reflection;

namespace ERPCore2.Helpers
{
    /// <summary>
    /// ActionButton 操作的統一處理 Helper
    /// 提供統一的 ActionButton 產生和更新方法，避免各個 EditModalComponent 中的重複代碼
    /// </summary>
    public class ActionButtonHelper
    {
        private readonly INotificationService _notificationService;

        public ActionButtonHelper(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }
        /// <summary>
        /// 泛型產生 FieldActionButtons 方法
        /// 統一處理從實體屬性取得 ID 並呼叫 ModalManager.GenerateActionButtons 的邏輯
        /// </summary>
        /// <typeparam name="TEntity">實體類型，必須繼承 BaseEntity</typeparam>
        /// <typeparam name="TService">服務類型，必須實作 IGenericManagementService</typeparam>
        /// <param name="editModalComponent">編輯 Modal 組件參考</param>
        /// <param name="modalManager">相關實體的 Modal 管理器</param>
        /// <param name="propertyName">實體屬性名稱（如 nameof(Product.ProductCategoryId)）</param>
        /// <returns>FieldActionButton 清單</returns>
        public async Task<List<FieldActionButton>> GenerateFieldActionButtonsAsync<TEntity, TService>(
            GenericEditModalComponent<TEntity, TService>? editModalComponent,
            object modalManager,
            string propertyName)
            where TEntity : BaseEntity, new()
            where TService : class, IGenericManagementService<TEntity>
        {
            try
            {
                // 從實體中取得對應屬性的值
                var currentId = GetEntityPropertyValue<int?>(editModalComponent?.Entity, propertyName);
                
                // 使用反射呼叫 modalManager 的 GenerateActionButtons 方法
                var generateMethod = modalManager?.GetType().GetMethod("GenerateActionButtons");
                if (generateMethod != null && modalManager != null)
                {
                    var result = generateMethod.Invoke(modalManager, new object?[] { currentId });
                    return result as List<FieldActionButton> ?? new List<FieldActionButton>();
                }
                
                return new List<FieldActionButton>();
            }
            catch (Exception ex)
            {
                // 記錄錯誤到資料庫
                var errorId = await ErrorHandlingHelper.HandlePageErrorAsync(
                    ex, 
                    nameof(GenerateFieldActionButtonsAsync), 
                    GetType(),
                    new { PropertyName = propertyName, EntityType = typeof(TEntity).Name }
                );

                // 通知使用者發生錯誤
                await _notificationService.ShowErrorAsync(
                    "載入操作按鈕時發生錯誤，請重新整理頁面或聯繫系統管理員",
                    "載入失敗"
                );

                // 發生錯誤時返回空清單，避免中斷頁面運作
                return new List<FieldActionButton>();
            }
        }

        /// <summary>
        /// 統一處理欄位變更的 ActionButtons 更新
        /// 處理 OnFieldValueChanged 事件中的 ActionButtons 更新邏輯
        /// </summary>
        /// <param name="modalManager">相關實體的 Modal 管理器</param>
        /// <param name="formFields">表單欄位定義清單</param>
        /// <param name="propertyName">變更的屬性名稱</param>
        /// <param name="value">新的屬性值</param>
        public async Task UpdateFieldActionButtonsAsync(
            object modalManager,
            List<FormFieldDefinition> formFields,
            string propertyName,
            object? value)
        {
            try
            {
                // 將值轉換為 int? 類型
                int? intValue = null;
                if (value != null && int.TryParse(value.ToString(), out int id))
                {
                    intValue = id;
                }
                
                // 使用反射呼叫 modalManager 的 UpdateFieldActionButtons 方法
                var updateMethod = modalManager?.GetType().GetMethod("UpdateFieldActionButtons");
                if (updateMethod != null && modalManager != null)
                {
                    updateMethod.Invoke(modalManager, new object?[] { formFields, propertyName, intValue });
                }
            }
            catch (Exception ex)
            {
                // 記錄錯誤到資料庫
                var errorId = await ErrorHandlingHelper.HandlePageErrorAsync(
                    ex, 
                    nameof(UpdateFieldActionButtonsAsync), 
                    GetType(),
                    new { PropertyName = propertyName, Value = value?.ToString() }
                );

                // 通知使用者發生錯誤
                await _notificationService.ShowWarningAsync(
                    "更新操作按鈕時發生錯誤，按鈕功能可能暫時無法使用",
                    "更新警告"
                );
            }
        }

        /// <summary>
        /// 使用反射從實體物件中取得指定屬性的值
        /// </summary>
        /// <typeparam name="T">回傳值的類型</typeparam>
        /// <param name="entity">實體物件</param>
        /// <param name="propertyName">屬性名稱</param>
        /// <returns>屬性值，如果取得失敗則返回預設值</returns>
        private static T? GetEntityPropertyValue<T>(object? entity, string propertyName)
        {
            try
            {
                if (entity == null) return default(T);
                
                var property = entity.GetType().GetProperty(propertyName);
                if (property == null) return default(T);
                
                var value = property.GetValue(entity);
                if (value == null) return default(T);
                
                // 嘗試轉換為目標類型
                if (value is T directValue)
                {
                    return directValue;
                }
                
                // 如果是可空類型，嘗試轉換
                var targetType = typeof(T);
                var underlyingType = Nullable.GetUnderlyingType(targetType);
                if (underlyingType != null)
                {
                    return (T?)Convert.ChangeType(value, underlyingType);
                }
                
                return (T?)Convert.ChangeType(value, targetType);
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// 產生帶有特殊邏輯的 ActionButtons（如 Employee.RoleActionButtons 的 IsSystemUser 邏輯）
        /// </summary>
        /// <typeparam name="TEntity">實體類型</typeparam>
        /// <typeparam name="TService">服務類型</typeparam>
        /// <param name="editModalComponent">編輯 Modal 組件參考</param>
        /// <param name="modalManager">相關實體的 Modal 管理器</param>
        /// <param name="propertyName">實體屬性名稱</param>
        /// <param name="customLogic">自訂邏輯委派，用於修改基本按鈕</param>
        /// <returns>FieldActionButton 清單</returns>
        public async Task<List<FieldActionButton>> GenerateFieldActionButtonsWithCustomLogicAsync<TEntity, TService>(
            GenericEditModalComponent<TEntity, TService>? editModalComponent,
            object modalManager,
            string propertyName,
            Action<List<FieldActionButton>, TEntity?> customLogic)
            where TEntity : BaseEntity, new()
            where TService : class, IGenericManagementService<TEntity>
        {
            try
            {
                // 先使用基本方法產生按鈕
                var buttons = await GenerateFieldActionButtonsAsync(editModalComponent, modalManager, propertyName);
                
                // 套用自訂邏輯
                customLogic?.Invoke(buttons, editModalComponent?.Entity);
                
                return buttons;
            }
            catch (Exception ex)
            {
                // 記錄錯誤到資料庫
                var errorId = await ErrorHandlingHelper.HandlePageErrorAsync(
                    ex, 
                    nameof(GenerateFieldActionButtonsWithCustomLogicAsync), 
                    GetType(),
                    new { PropertyName = propertyName, EntityType = typeof(TEntity).Name }
                );

                // 通知使用者發生錯誤
                await _notificationService.ShowErrorAsync(
                    "載入特殊操作按鈕時發生錯誤，請重新整理頁面或聯繫系統管理員",
                    "載入失敗"
                );

                return new List<FieldActionButton>();
            }
        }
    }
}
