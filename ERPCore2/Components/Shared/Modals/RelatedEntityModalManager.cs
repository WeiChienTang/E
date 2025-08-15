using ERPCore2.Data;
using ERPCore2.Components.Shared.Forms;
using ERPCore2.Services;
using Microsoft.AspNetCore.Components;

namespace ERPCore2.Components.Shared.Modals;

/// <summary>
/// 泛型的相關實體 Modal 管理器
/// 用於處理表單欄位旁邊的新增/編輯按鈕功能
/// </summary>
/// <typeparam name="TRelatedEntity">相關實體類型 (如 Department, EmployeePosition 等)</typeparam>
public class RelatedEntityModalManager<TRelatedEntity> where TRelatedEntity : BaseEntity
{
    /// <summary>
    /// 通知服務
    /// </summary>
    public INotificationService NotificationService { get; set; } = default!;
    
    /// <summary>
    /// 實體名稱 (用於顯示訊息)
    /// </summary>
    public string EntityName { get; set; } = typeof(TRelatedEntity).Name;
    
    /// <summary>
    /// 實體顯示名稱 (用於按鈕文字和訊息)
    /// </summary>
    public string EntityDisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// Modal 可見性狀態
    /// </summary>
    public bool IsModalVisible { get; private set; } = false;
    
    /// <summary>
    /// 目前選擇的實體 ID
    /// </summary>
    public int? SelectedEntityId { get; private set; } = null;
    
    /// <summary>
    /// Modal 可見性變更事件
    /// </summary>
    public EventCallback<bool> IsModalVisibleChanged { get; set; }
    
    /// <summary>
    /// 實體儲存成功事件
    /// </summary>
    public EventCallback<TRelatedEntity> OnEntitySaved { get; set; }
    
    /// <summary>
    /// Modal 取消事件
    /// </summary>
    public EventCallback OnModalCancel { get; set; }
    
    /// <summary>
    /// 重新載入資料的回調函式
    /// </summary>
    public Func<Task>? ReloadDataCallback { get; set; }
    
    /// <summary>
    /// 自訂的後處理邏輯 (在實體儲存成功後執行)
    /// </summary>
    public Func<TRelatedEntity, Task>? CustomPostProcessCallback { get; set; }
    
    /// <summary>
    /// 自動選擇新實體的回調函式
    /// </summary>
    public Action<int>? AutoSelectCallback { get; set; }
    
    /// <summary>
    /// 觸發狀態變更通知
    /// </summary>
    public Action? StateHasChangedCallback { get; set; }
    
    /// <summary>
    /// 預填值字典 - 用於新增模式時預設值
    /// </summary>
    public Dictionary<string, object?> PrefilledValues { get; private set; } = new();
    
    /// <summary>
    /// 開啟 Modal
    /// </summary>
    /// <param name="entityId">實體 ID，null 表示新增模式</param>
    public async Task OpenModalAsync(int? entityId)
    {
        try
        {
            SelectedEntityId = entityId;
            IsModalVisible = true;
            StateHasChangedCallback?.Invoke();
            
            if (IsModalVisibleChanged.HasDelegate)
            {
                await IsModalVisibleChanged.InvokeAsync(true);
            }
        }
        catch (Exception ex)
        {
            await NotificationService.ShowErrorAsync($"開啟{EntityDisplayName}編輯視窗時發生錯誤：{ex.Message}");
        }
    }
    
    /// <summary>
    /// 開啟 Modal 並預填指定的欄位值
    /// </summary>
    /// <param name="entityId">實體 ID，null 表示新增模式</param>
    /// <param name="prefilledValues">要預填的欄位值</param>
    public async Task OpenModalWithPrefilledValuesAsync(int? entityId, Dictionary<string, object?> prefilledValues)
    {
        try
        {
            SelectedEntityId = entityId;
            PrefilledValues = prefilledValues ?? new Dictionary<string, object?>();
            IsModalVisible = true;
            StateHasChangedCallback?.Invoke();
            
            if (IsModalVisibleChanged.HasDelegate)
            {
                await IsModalVisibleChanged.InvokeAsync(true);
            }
        }
        catch (Exception ex)
        {
            await NotificationService.ShowErrorAsync($"開啟{EntityDisplayName}編輯視窗時發生錯誤：{ex.Message}");
        }
    }
    
    /// <summary>
    /// 關閉 Modal
    /// </summary>
    public async Task CloseModalAsync()
    {
        try
        {
            IsModalVisible = false;
            SelectedEntityId = null;
            PrefilledValues.Clear(); // 清理預填值
            StateHasChangedCallback?.Invoke();
            
            if (IsModalVisibleChanged.HasDelegate)
            {
                await IsModalVisibleChanged.InvokeAsync(false);
            }
        }
        catch (Exception ex)
        {
            await NotificationService.ShowErrorAsync($"關閉{EntityDisplayName}編輯視窗時發生錯誤：{ex.Message}");
        }
    }
    
    /// <summary>
    /// 處理 Modal 可見性變更
    /// </summary>
    /// <param name="isVisible">是否可見</param>
    public async Task HandleModalVisibilityChangedAsync(bool isVisible)
    {
        try
        {
            IsModalVisible = isVisible;
            if (!isVisible)
            {
                SelectedEntityId = null;
            }
            StateHasChangedCallback?.Invoke();
        }
        catch (Exception ex)
        {
            await NotificationService.ShowErrorAsync($"處理{EntityDisplayName}視窗狀態變更時發生錯誤：{ex.Message}");
        }
    }
    
    /// <summary>
    /// 處理實體儲存成功事件
    /// </summary>
    /// <param name="savedEntity">已儲存的實體</param>
    /// <param name="shouldAutoSelect">是否自動選擇新實體 (當原本沒有選擇時)</param>
    public async Task HandleEntitySavedAsync(TRelatedEntity savedEntity, bool shouldAutoSelect = true)
    {
        try
        {
            // 重新載入資料
            if (ReloadDataCallback != null)
            {
                await ReloadDataCallback();
            }
            
            // 執行自訂後處理邏輯
            if (CustomPostProcessCallback != null)
            {
                await CustomPostProcessCallback(savedEntity);
            }
            
            // 自動選擇新實體 (如果原本沒有選擇且啟用自動選擇)
            if (shouldAutoSelect && AutoSelectCallback != null && SelectedEntityId == null)
            {
                AutoSelectCallback(savedEntity.Id);
            }
            
            // 關閉 Modal
            await CloseModalAsync();
            
            // 顯示成功訊息
            var entityDisplayText = GetEntityDisplayText(savedEntity);
            await NotificationService.ShowSuccessAsync($"{EntityDisplayName}「{entityDisplayText}」已成功儲存，選項已更新");
            
            // 觸發實體儲存事件
            if (OnEntitySaved.HasDelegate)
            {
                await OnEntitySaved.InvokeAsync(savedEntity);
            }
        }
        catch (Exception ex)
        {
            await NotificationService.ShowErrorAsync($"處理{EntityDisplayName}儲存成功事件時發生錯誤：{ex.Message}");
        }
    }
    
    /// <summary>
    /// 處理 Modal 取消事件
    /// </summary>
    public async Task HandleModalCancelAsync()
    {
        try
        {
            await CloseModalAsync();
            
            if (OnModalCancel.HasDelegate)
            {
                await OnModalCancel.InvokeAsync();
            }
        }
        catch (Exception ex)
        {
            await NotificationService.ShowErrorAsync($"處理{EntityDisplayName}編輯取消時發生錯誤：{ex.Message}");
        }
    }
    
    /// <summary>
    /// 產生 FieldActionButton 列表 (分開的新增和編輯按鈕)
    /// </summary>
    /// <param name="currentSelectedId">目前選擇的實體 ID</param>
    /// <returns>按鈕列表</returns>
    public List<FieldActionButton> GenerateActionButtons(int? currentSelectedId)
    {
        var buttons = new List<FieldActionButton>();
        
        // 新增按鈕 - 永遠顯示
        buttons.Add(new FieldActionButton
        {
            Text = "新增",
            Variant = "OutlinePrimary",
            Size = "Small",
            Title = $"新增新的{EntityDisplayName}",
            OnClick = () => OpenModalAsync(null)
        });
        
        // 編輯按鈕 - 只有在有選擇值時才顯示
        if (currentSelectedId.HasValue)
        {
            buttons.Add(new FieldActionButton
            {
                Text = "編輯",
                Variant = "OutlineSecondary",
                Size = "Small",
                Title = $"編輯目前選擇的{EntityDisplayName}",
                OnClick = () => OpenModalAsync(currentSelectedId.Value)
            });
        }
        
        return buttons;
    }
    
    /// <summary>
    /// 產生 FieldActionButton 列表 (分開的新增和編輯按鈕，支援智能檢查)
    /// </summary>
    /// <param name="currentSelectedId">目前選擇的實體 ID</param>
    /// <param name="currentInputValue">目前輸入框的值</param>
    /// <param name="availableEntities">可用的實體列表，用於檢查是否存在</param>
    /// <param name="searchProperty">要搜尋的屬性名稱 (預設為 "Name")</param>
    /// <returns>按鈕列表</returns>
    public List<FieldActionButton> GenerateActionButtonsWithSmartCheck(
        int? currentSelectedId, 
        string? currentInputValue, 
        IEnumerable<TRelatedEntity>? availableEntities = null,
        string searchProperty = "Name")
    {
        var buttons = new List<FieldActionButton>();
        
        // 新增按鈕 - 永遠顯示，會讀取輸入框的值作為預填
        buttons.Add(new FieldActionButton
        {
            Text = "新增",
            Variant = "OutlinePrimary",
            Size = "Small",
            Title = $"新增新的{EntityDisplayName}",
            OnClick = async () =>
            {
                var prefilledValues = new Dictionary<string, object?>();
                
                // 如果有輸入值，預填到對應的屬性
                if (!string.IsNullOrWhiteSpace(currentInputValue))
                {
                    prefilledValues[searchProperty] = currentInputValue;
                }
                
                await OpenModalWithPrefilledValuesAsync(null, prefilledValues);
            }
        });
        
        // 編輯按鈕 - 智能檢查邏輯
        if (!string.IsNullOrWhiteSpace(currentInputValue))
        {
            buttons.Add(new FieldActionButton
            {
                Text = "編輯",
                Variant = "OutlineSecondary",
                Size = "Small",
                Title = $"編輯目前輸入的{EntityDisplayName}",
                OnClick = async () =>
                {
                    // 如果有明確的選擇 ID，直接編輯
                    if (currentSelectedId.HasValue)
                    {
                        await OpenModalAsync(currentSelectedId.Value);
                        return;
                    }
                    
                    // 如果沒有選擇 ID 但有輸入值，嘗試在可用實體中尋找
                    if (availableEntities != null)
                    {
                        var matchedEntity = FindEntityByProperty(availableEntities, searchProperty, currentInputValue);
                        if (matchedEntity != null)
                        {
                            await OpenModalAsync(matchedEntity.Id);
                            return;
                        }
                    }
                    
                    // 如果找不到，顯示通知
                    await NotificationService.ShowWarningAsync($"找不到名稱為「{currentInputValue}」的{EntityDisplayName}，請先新增此資料");
                }
            });
        }
        
        return buttons;
    }
    
    /// <summary>
    /// 根據屬性值尋找實體
    /// </summary>
    /// <param name="entities">實體列表</param>
    /// <param name="propertyName">屬性名稱</param>
    /// <param name="searchValue">搜尋值</param>
    /// <returns>匹配的實體，如果找不到則返回 null</returns>
    private TRelatedEntity? FindEntityByProperty(IEnumerable<TRelatedEntity> entities, string propertyName, string searchValue)
    {
        try
        {
            var entityType = typeof(TRelatedEntity);
            var property = entityType.GetProperty(propertyName);
            if (property == null)
                return null;
            
            return entities.FirstOrDefault(entity =>
            {
                var propertyValue = property.GetValue(entity)?.ToString();
                return string.Equals(propertyValue, searchValue, StringComparison.OrdinalIgnoreCase);
            });
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// 更新欄位的 ActionButtons (用於動態更新)
    /// </summary>
    /// <param name="formFields">表單欄位列表</param>
    /// <param name="propertyName">屬性名稱</param>
    /// <param name="newSelectedId">新的選擇 ID</param>
    public void UpdateFieldActionButtons(List<FormFieldDefinition>? formFields, string propertyName, int? newSelectedId)
    {
        var targetField = formFields?.FirstOrDefault(f => f.PropertyName == propertyName);
        if (targetField != null)
        {
            targetField.ActionButtons = GenerateActionButtons(newSelectedId);
            StateHasChangedCallback?.Invoke();
        }
    }
    
    /// <summary>
    /// 取得實體的顯示文字 (用於訊息顯示)
    /// </summary>
    /// <param name="entity">實體</param>
    /// <returns>顯示文字</returns>
    private string GetEntityDisplayText(TRelatedEntity entity)
    {
        // 嘗試使用常見的顯示屬性
        var entityType = typeof(TRelatedEntity);
        
        // 優先順序：Name > Title > Code > Id
        var nameProperty = entityType.GetProperty("Name");
        if (nameProperty != null)
        {
            var nameValue = nameProperty.GetValue(entity)?.ToString();
            if (!string.IsNullOrEmpty(nameValue))
                return nameValue;
        }
        
        var titleProperty = entityType.GetProperty("Title");
        if (titleProperty != null)
        {
            var titleValue = titleProperty.GetValue(entity)?.ToString();
            if (!string.IsNullOrEmpty(titleValue))
                return titleValue;
        }
        
        var codeProperty = entityType.GetProperty("Code");
        if (codeProperty != null)
        {
            var codeValue = codeProperty.GetValue(entity)?.ToString();
            if (!string.IsNullOrEmpty(codeValue))
                return codeValue;
        }
        
        // 回退到 ID
        return entity.Id.ToString();
    }
}

/// <summary>
/// RelatedEntityModalManager 的配置類別
/// </summary>
/// <typeparam name="TRelatedEntity">相關實體類型</typeparam>
public class RelatedEntityModalConfig<TRelatedEntity> where TRelatedEntity : BaseEntity
{
    /// <summary>
    /// 實體顯示名稱
    /// </summary>
    public string EntityDisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// 目標屬性名稱 (如 nameof(Employee.DepartmentId))
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;
    
    /// <summary>
    /// Modal 組件類型
    /// </summary>
    public Type ModalComponentType { get; set; } = default!;
    
    /// <summary>
    /// 是否啟用自動選擇新實體
    /// </summary>
    public bool EnableAutoSelect { get; set; } = true;
    
    /// <summary>
    /// 自訂的按鈕樣式
    /// </summary>
    public string ButtonVariant { get; set; } = "OutlinePrimary";
    
    /// <summary>
    /// 自訂的按鈕大小
    /// </summary>
    public string ButtonSize { get; set; } = "Small";
}
