using ERPCore2.Data;
using ERPCore2.Components.Shared.UI.Form;

namespace ERPCore2.Components.Shared.Modal;

/// <summary>
/// AutoComplete 智能處理（partial class）
/// </summary>
public partial class GenericEditModalComponent<TEntity, TService>
    where TEntity : BaseEntity, new()
{
    // ===== AutoComplete 智能處理 =====

    /// <summary>
    /// 取得處理後的表單欄位（結果快取，僅在資料變更時重建）
    /// </summary>
    private List<FormFieldDefinition> GetProcessedFormFields()
    {
        return _processedFormFieldsCache ??= BuildProcessedFormFields();
    }

    /// <summary>
    /// 實際建立處理後的表單欄位（結果由 GetProcessedFormFields 快取）
    /// </summary>
    private List<FormFieldDefinition> BuildProcessedFormFields()
    {
        var processedFields = new List<FormFieldDefinition>();

        foreach (var field in FormFields)
        {
            var processedField = new FormFieldDefinition
            {
                PropertyName = field.PropertyName,
                Label = field.Label,
                LabelFactory = field.LabelFactory,
                FieldType = field.FieldType,
                Placeholder = field.Placeholder,
                HelpText = field.HelpText,
                IsRequired = field.IsRequired,
                IsReadOnly = field.IsReadOnly,
                IsDisabled = field.IsDisabled,
                IsVisible = field.IsVisible,
                CssClass = field.CssClass,
                ContainerCssClass = field.ContainerCssClass,
                Min = field.Min,
                Max = field.Max,
                Step = field.Step,
                DecimalPlaces = field.DecimalPlaces,
                UseThousandsSeparator = field.UseThousandsSeparator,
                BindOnInput = field.BindOnInput,
                MinLength = field.MinLength,
                MaxLength = field.MaxLength,
                MaxBytes = field.MaxBytes,
                Rows = field.Rows,
                ShowCharacterCount = field.ShowCharacterCount,
                Options = field.Options,
                AutoCompleteDelayMs = field.AutoCompleteDelayMs,
                MinSearchLength = field.MinSearchLength,
                DefaultValue = field.DefaultValue,
                ValidationRules = field.ValidationRules,
                Order = field.Order,
                GroupName = field.GroupName,
                ActionButtons = field.ActionButtons,
                LabelHelpItems = field.LabelHelpItems,
                AutoCompleteAttribute = field.AutoCompleteAttribute,
                IsFilterOnly = field.IsFilterOnly,
            };

            // 如果是 AutoComplete 欄位，包裝搜尋函式以支援智能預填
            if (field.FieldType == FormFieldType.AutoComplete)
            {
                if (field.SearchFunction != null)
                {
                    var originalSearchFunction = field.SearchFunction;
                    processedField.SearchFunction = async (searchTerm) =>
                    {
                        _lastSearchTerms[field.PropertyName] = searchTerm ?? string.Empty;
                        return await originalSearchFunction(searchTerm ?? string.Empty);
                    };
                }
                else if (AutoCompleteCollections?.ContainsKey(field.PropertyName) == true)
                {
                    processedField.SearchFunction = CreateAutoSearchFunction(field.PropertyName);
                }

                // 處理智能按鈕
                if (field.ActionButtons != null && field.ActionButtons.Any())
                    processedField.ActionButtons = ProcessActionButtonsForAutoComplete(field);
            }

            processedFields.Add(processedField);
        }

        return processedFields;
    }

    /// <summary>
    /// 處理 AutoComplete 欄位的操作按鈕，支援智能預填和分離的新增/編輯按鈕
    /// </summary>
    private List<FieldActionButton> ProcessActionButtonsForAutoComplete(FormFieldDefinition field)
    {
        var processedButtons = new List<FieldActionButton>();

        // 智能按鈕選擇：根據輸入值是否存在於資料庫決定顯示哪個按鈕
        var searchTerm = _lastSearchTerms.GetValueOrDefault(field.PropertyName, string.Empty);

        // 先檢查實體欄位的實際值，如果為空則確保 searchTerm 也為空
        var currentValue = GetPropertyValue(Entity, field.PropertyName);
        var currentId = currentValue as int?;

        // 如果實體值為空或 0，強制清空 searchTerm（避免殘留舊關鍵字）
        if (currentId == null || currentId.Value <= 0)
        {
            searchTerm = string.Empty;
            _lastSearchTerms[field.PropertyName] = string.Empty;
        }
        // 如果 searchTerm 為空但實體有值（編輯模式），從 AutoComplete 集合中找出對應的顯示名稱
        else if (string.IsNullOrWhiteSpace(searchTerm) &&
            Entity != null &&
            AutoCompleteCollections?.ContainsKey(field.PropertyName) == true)
        {
            if (currentId.HasValue && currentId.Value > 0)
            {
                var availableEntities = AutoCompleteCollections[field.PropertyName];
                var displayProperty = AutoCompleteDisplayProperties?.GetValueOrDefault(field.PropertyName, "Name") ?? "Name";

                var matchedByIdEntity = availableEntities.FirstOrDefault(e =>
                {
                    var idValue = GetPropertyValue(e, "Id") as int?;
                    return idValue == currentId.Value;
                });

                if (matchedByIdEntity != null)
                {
                    var displayValue = GetPropertyValue(matchedByIdEntity, displayProperty)?.ToString();
                    if (!string.IsNullOrWhiteSpace(displayValue))
                    {
                        searchTerm = displayValue;
                        _lastSearchTerms[field.PropertyName] = displayValue;
                    }
                }
            }
        }

        bool entityExists = false;
        object? matchedEntity = null;

        // 檢查輸入值是否存在於 AutoComplete 集合中
        if (AutoCompleteCollections?.ContainsKey(field.PropertyName) == true &&
            !string.IsNullOrWhiteSpace(searchTerm))
        {
            var availableEntities = AutoCompleteCollections[field.PropertyName];
            var displayProperty = AutoCompleteDisplayProperties?.GetValueOrDefault(field.PropertyName, "Name") ?? "Name";
            matchedEntity = FindMatchingEntity(availableEntities, displayProperty, searchTerm);
            entityExists = matchedEntity != null;
        }

        foreach (var button in field.ActionButtons!)
        {
            // 智能過濾：依 ButtonRole 判斷，避免依賴顯示文字（i18n 安全）
            if (entityExists && button.Role == FieldActionButtonRole.Add)
                continue; // 實體存在時跳過新增按鈕

            if (!entityExists && button.Role == FieldActionButtonRole.Edit)
                continue; // 實體不存在時跳過編輯按鈕

            var processedButton = new FieldActionButton
            {
                Role = button.Role,
                Text = button.Text,
                Variant = button.Variant,
                Size = button.Size,
                IconClass = button.IconClass,
                Title = button.Title,
                IsDisabled = button.IsDisabled
            };

            // 如果是新增按鈕且有預填器，包裝點擊事件以支援智能預填
            if (button.Role == FieldActionButtonRole.Add &&
                AutoCompletePrefillers?.ContainsKey(field.PropertyName) == true &&
                button.OnClick != null)
            {
                var originalOnClick = button.OnClick;
                processedButton.OnClick = async () =>
                {
                    var term = _lastSearchTerms.GetValueOrDefault(field.PropertyName, string.Empty);
                    if (!string.IsNullOrWhiteSpace(term))
                    {
                        var prefilledValues = AutoCompletePrefillers[field.PropertyName](term);

                        if (ModalManagers?.ContainsKey(field.PropertyName) == true)
                        {
                            var modalManager = ModalManagers[field.PropertyName];
                            var openMethod = GetCachedMethod(modalManager.GetType(), "OpenModalWithPrefilledValuesAsync");
                            if (openMethod != null)
                            {
                                var task = (Task?)openMethod.Invoke(modalManager, new object[] { null!, prefilledValues });
                                if (task != null)
                                {
                                    await task;
                                    return;
                                }
                            }
                        }
                    }

                    await originalOnClick();
                };
            }
            // 如果是編輯按鈕，包裝點擊事件以支援智能編輯
            else if (button.Role == FieldActionButtonRole.Edit && button.OnClick != null)
            {
                var capturedMatchedEntity = matchedEntity;
                var originalOnClick = button.OnClick;
                processedButton.OnClick = async () =>
                {
                    if (ModalManagers?.ContainsKey(field.PropertyName) == true && capturedMatchedEntity != null)
                    {
                        var modalManager = ModalManagers[field.PropertyName];
                        var entityIdValue = GetPropertyValue(capturedMatchedEntity, "Id") as int?;
                        if (entityIdValue.HasValue)
                        {
                            var openMethod = GetCachedMethod(modalManager.GetType(), "OpenModalAsync");
                            if (openMethod != null)
                            {
                                var task = (Task?)openMethod.Invoke(modalManager, new object[] { entityIdValue.Value });
                                if (task != null)
                                {
                                    await task;
                                    return;
                                }
                            }
                        }
                    }

                    await originalOnClick();
                };
            }
            else
            {
                processedButton.OnClick = button.OnClick;
            }

            processedButtons.Add(processedButton);
        }

        return processedButtons;
    }

    /// <summary>
    /// 為指定屬性建立自動搜尋函式
    /// </summary>
    private Func<string, Task<List<SelectOption>>> CreateAutoSearchFunction(string propertyName)
    {
        return (searchTerm) =>
        {
            try
            {
                _lastSearchTerms[propertyName] = searchTerm ?? string.Empty;

                if (!AutoCompleteCollections!.ContainsKey(propertyName))
                    return Task.FromResult(new List<SelectOption>());

                var collection = AutoCompleteCollections[propertyName];
                var displayProperty = AutoCompleteDisplayProperties?.GetValueOrDefault(propertyName, "Name") ?? "Name";
                var valueProperty = AutoCompleteValueProperties?.GetValueOrDefault(propertyName, "Id") ?? "Id";
                var maxResults = AutoCompleteMaxResults?.GetValueOrDefault(propertyName, 100) ?? 100;

                var result = collection
                    .Where(item =>
                    {
                        if (string.IsNullOrEmpty(searchTerm)) return true;
                        var displayValue = GetPropertyValue(item, displayProperty)?.ToString() ?? string.Empty;
                        return displayValue.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
                    })
                    .Take(maxResults)
                    .Select(item => new SelectOption
                    {
                        Text = GetPropertyValue(item, displayProperty)?.ToString() ?? string.Empty,
                        Value = GetPropertyValue(item, valueProperty)?.ToString() ?? string.Empty
                    })
                    .ToList();
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                LogError($"AutoSearch_{propertyName}", ex);
                return Task.FromResult(new List<SelectOption>());
            }
        };
    }

    /// <summary>
    /// 在集合中尋找匹配的實體
    /// </summary>
    private object? FindMatchingEntity(IEnumerable<object> entities, string propertyName, string searchValue)
    {
        try
        {
            return entities.FirstOrDefault(entity =>
            {
                var propertyValue = GetPropertyValue(entity, propertyName)?.ToString();
                return string.Equals(propertyValue, searchValue, StringComparison.OrdinalIgnoreCase);
            });
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 使用快取反射取得物件屬性值
    /// </summary>
    private object? GetPropertyValue(object obj, string propertyName)
    {
        try
        {
            var property = GetCachedProperty(obj.GetType(), propertyName);
            return property?.GetValue(obj);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 更新所有帶有 ActionButtons 的欄位，確保按鈕狀態與實體資料同步
    /// </summary>
    private void UpdateAllActionButtons()
    {
        try
        {
            if (Entity == null || ModalManagers == null || !ModalManagers.Any() || FormFields == null || !FormFields.Any())
                return;

            var fieldsWithButtons = FormFields.Where(f => f.ActionButtons != null && f.ActionButtons.Any()).ToList();

            foreach (var field in fieldsWithButtons)
            {
                if (ModalManagers.ContainsKey(field.PropertyName))
                {
                    var modalManager = ModalManagers[field.PropertyName];
                    var currentValue = GetPropertyValue(Entity, field.PropertyName);
                    var intValue = currentValue as int?;

                    var updateMethod = GetCachedMethod(modalManager!.GetType(), "UpdateFieldActionButtons");
                    if (updateMethod != null)
                        updateMethod.Invoke(modalManager, new object?[] { FormFields, field.PropertyName, intValue });
                }
            }

            // ActionButtons 已更新，使處理後的欄位快取失效
            _processedFormFieldsCache = null;
        }
        catch (Exception ex)
        {
            _ = Task.Run(() => LogError("UpdateAllActionButtons", ex));
        }
    }
}
