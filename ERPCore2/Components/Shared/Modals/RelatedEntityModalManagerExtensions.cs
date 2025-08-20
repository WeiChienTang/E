using ERPCore2.Data;
using ERPCore2.Data.Entities;
using ERPCore2.Components.Shared.Forms;
using ERPCore2.Components.Shared.Modals;
using ERPCore2.Components.Pages.Employees;
using ERPCore2.Services;
using Microsoft.AspNetCore.Components;

namespace ERPCore2.Components.Shared.Modals;

/// <summary>
/// Modal 組件的 RelatedEntityModalManager 輔助擴展方法
/// </summary>
public static class ModalComponentExtensions
{
    /// <summary>
    /// 為 Modal 組件創建並配置 RelatedEntityModalManager
    /// </summary>
    /// <typeparam name="TRelatedEntity">相關實體類型</typeparam>
    /// <param name="notificationService">通知服務</param>
    /// <param name="config">配置</param>
    /// <param name="reloadDataCallback">重新載入資料回調</param>
    /// <param name="stateHasChangedCallback">狀態變更回調</param>
    /// <param name="autoSelectCallback">自動選擇回調</param>
    /// <returns>配置好的管理器</returns>
    public static RelatedEntityModalManager<TRelatedEntity> CreateRelatedEntityManager<TRelatedEntity>(
        INotificationService notificationService,
        RelatedEntityModalConfig<TRelatedEntity> config,
        Func<Task>? reloadDataCallback = null,
        Action? stateHasChangedCallback = null,
        Action<int>? autoSelectCallback = null) 
        where TRelatedEntity : BaseEntity
    {
        return new RelatedEntityModalManager<TRelatedEntity>
        {
            NotificationService = notificationService,
            EntityDisplayName = config.EntityDisplayName,
            ReloadDataCallback = reloadDataCallback,
            StateHasChangedCallback = stateHasChangedCallback,
            AutoSelectCallback = autoSelectCallback
        };
    }
}

/// <summary>
/// 常用實體的預設配置
/// </summary>
public static class CommonEntityConfigs
{
    /// <summary>
    /// 部門配置
    /// </summary>
    public static RelatedEntityModalConfig<Department> Department => new()
    {
        EntityDisplayName = "部門",
        PropertyName = nameof(Employee.DepartmentId),
        ModalComponentType = typeof(DepartmentEditModalComponent),
        EnableAutoSelect = true
    };
    
    /// <summary>
    /// 員工職位配置
    /// </summary>
    public static RelatedEntityModalConfig<EmployeePosition> EmployeePosition => new()
    {
        EntityDisplayName = "職位",
        PropertyName = nameof(Employee.EmployeePositionId),
        ModalComponentType = typeof(ComponentBase), // 暫時使用基礎類型，實際使用時替換
        EnableAutoSelect = true
    };
    
    /// <summary>
    /// 通用配置建立方法
    /// </summary>
    /// <typeparam name="TEntity">實體類型</typeparam>
    /// <param name="entityDisplayName">實體顯示名稱</param>
    /// <param name="propertyName">屬性名稱</param>
    /// <param name="modalComponentType">Modal 組件類型</param>
    /// <returns>配置</returns>
    public static RelatedEntityModalConfig<TEntity> Create<TEntity>(
        string entityDisplayName,
        string propertyName,
        Type modalComponentType) where TEntity : BaseEntity
    {
        return new RelatedEntityModalConfig<TEntity>
        {
            EntityDisplayName = entityDisplayName,
            PropertyName = propertyName,
            ModalComponentType = modalComponentType,
            EnableAutoSelect = true
        };
    }
}

/// <summary>
/// 表單欄位動態按鈕更新輔助類別
/// </summary>
public static class FormFieldActionButtonHelper
{
    /// <summary>
    /// 處理欄位變更並更新相關的 ActionButton
    /// </summary>
    /// <typeparam name="TRelatedEntity">相關實體類型</typeparam>
    /// <param name="fieldChange">欄位變更資訊</param>
    /// <param name="managers">管理器字典 (PropertyName -> Manager)</param>
    /// <param name="formFields">表單欄位列表</param>
    public static void HandleFieldChangedForActionButtons<TRelatedEntity>(
        (string PropertyName, object? Value) fieldChange,
        Dictionary<string, RelatedEntityModalManager<TRelatedEntity>> managers,
        List<FormFieldDefinition>? formFields) 
        where TRelatedEntity : BaseEntity
    {
        if (managers.TryGetValue(fieldChange.PropertyName, out var manager))
        {
            // 解析新的 ID 值
            int? newId = null;
            if (fieldChange.Value != null && int.TryParse(fieldChange.Value.ToString(), out int id))
            {
                newId = id;
            }
            
            // 更新按鈕
            manager.UpdateFieldActionButtons(formFields, fieldChange.PropertyName, newId);
        }
    }
    
    /// <summary>
    /// 批量處理多個相關實體的欄位變更
    /// </summary>
    /// <param name="fieldChange">欄位變更資訊</param>
    /// <param name="entityManagers">實體管理器字典</param>
    /// <param name="formFields">表單欄位列表</param>
    public static void HandleFieldChangedForMultipleEntities(
        (string PropertyName, object? Value) fieldChange,
        Dictionary<string, object> entityManagers,
        List<FormFieldDefinition>? formFields)
    {
        if (entityManagers.TryGetValue(fieldChange.PropertyName, out var manager))
        {
            // 解析新的 ID 值
            int? newId = null;
            if (fieldChange.Value != null && int.TryParse(fieldChange.Value.ToString(), out int id))
            {
                newId = id;
            }
            
            // 使用反射調用 UpdateFieldActionButtons 方法
            var managerType = manager.GetType();
            var updateMethod = managerType.GetMethod("UpdateFieldActionButtons");
            if (updateMethod != null && formFields != null)
            {
                updateMethod.Invoke(manager, new object?[] { formFields, fieldChange.PropertyName, newId });
            }
        }
    }
}

/// <summary>
/// 建構器模式的 RelatedEntityModalManager 配置類別
/// </summary>
/// <typeparam name="TRelatedEntity">相關實體類型</typeparam>
public class RelatedEntityManagerBuilder<TRelatedEntity> where TRelatedEntity : BaseEntity
{
    private readonly RelatedEntityModalManager<TRelatedEntity> _manager;
    private readonly RelatedEntityModalConfig<TRelatedEntity> _config;
    
    public RelatedEntityManagerBuilder(INotificationService notificationService, string entityDisplayName)
    {
        _manager = new RelatedEntityModalManager<TRelatedEntity>
        {
            NotificationService = notificationService,
            EntityDisplayName = entityDisplayName
        };
        
        _config = new RelatedEntityModalConfig<TRelatedEntity>
        {
            EntityDisplayName = entityDisplayName
        };
    }
    
    /// <summary>
    /// 設定屬性名稱
    /// </summary>
    public RelatedEntityManagerBuilder<TRelatedEntity> WithPropertyName(string propertyName)
    {
        _config.PropertyName = propertyName;
        return this;
    }
    
    /// <summary>
    /// 設定重新載入資料回調
    /// </summary>
    public RelatedEntityManagerBuilder<TRelatedEntity> WithReloadCallback(Func<Task> callback)
    {
        _manager.ReloadDataCallback = callback;
        return this;
    }
    
    /// <summary>
    /// 設定狀態變更回調
    /// </summary>
    public RelatedEntityManagerBuilder<TRelatedEntity> WithStateChangedCallback(Action callback)
    {
        _manager.StateHasChangedCallback = callback;
        return this;
    }
    
    /// <summary>
    /// 設定自動選擇回調
    /// </summary>
    public RelatedEntityManagerBuilder<TRelatedEntity> WithAutoSelectCallback(Action<int> callback)
    {
        _manager.AutoSelectCallback = callback;
        return this;
    }
    
    /// <summary>
    /// 設定自訂後處理邏輯
    /// </summary>
    public RelatedEntityManagerBuilder<TRelatedEntity> WithCustomPostProcess(Func<TRelatedEntity, Task> callback)
    {
        _manager.CustomPostProcessCallback = callback;
        return this;
    }
    
    /// <summary>
    /// 設定按鈕樣式
    /// </summary>
    public RelatedEntityManagerBuilder<TRelatedEntity> WithButtonStyle(string variant = "OutlinePrimary", string size = "Small")
    {
        _config.ButtonVariant = variant;
        _config.ButtonSize = size;
        return this;
    }
    
    /// <summary>
    /// 建立管理器
    /// </summary>
    public RelatedEntityModalManager<TRelatedEntity> Build()
    {
        return _manager;
    }
    
    /// <summary>
    /// 建立管理器和配置
    /// </summary>
    public (RelatedEntityModalManager<TRelatedEntity> Manager, RelatedEntityModalConfig<TRelatedEntity> Config) BuildWithConfig()
    {
        return (_manager, _config);
    }
}
