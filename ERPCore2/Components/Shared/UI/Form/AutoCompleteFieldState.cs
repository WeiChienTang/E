namespace ERPCore2.Components.Shared.UI.Form;

/// <summary>
/// AutoComplete 欄位狀態管理類別
/// 封裝單一 AutoComplete 欄位的所有狀態，避免使用多個 Dictionary 管理
/// </summary>
public class AutoCompleteFieldState : IDisposable
{
    /// <summary>
    /// 搜尋結果選項清單
    /// </summary>
    public List<SelectOption> Options { get; set; } = new();

    /// <summary>
    /// 是否正在載入中
    /// </summary>
    public bool IsLoading { get; set; }

    /// <summary>
    /// 下拉選單是否可見
    /// </summary>
    public bool IsVisible { get; set; }

    /// <summary>
    /// 搜尋延遲計時器
    /// </summary>
    public Timer? SearchTimer { get; set; }

    /// <summary>
    /// 顯示值（使用者看到的文字）
    /// </summary>
    public string DisplayValue { get; set; } = string.Empty;

    /// <summary>
    /// 目前高亮的選項索引（-1 表示無高亮）
    /// </summary>
    public int HighlightedIndex { get; set; } = -1;

    /// <summary>
    /// 是否正在使用鍵盤導航
    /// </summary>
    public bool IsKeyboardNavigating { get; set; }

    /// <summary>
    /// 欄位是否已失去焦點
    /// </summary>
    public bool HasBlurred { get; set; }

    /// <summary>
    /// 重設鍵盤導航狀態
    /// </summary>
    public void ResetKeyboardNavigation()
    {
        HighlightedIndex = -1;
        IsKeyboardNavigating = false;
    }

    /// <summary>
    /// 隱藏下拉選單並清理狀態
    /// </summary>
    public void HideDropdown()
    {
        IsVisible = false;
        IsLoading = false;
        ResetKeyboardNavigation();
    }

    /// <summary>
    /// 清除搜尋計時器
    /// </summary>
    public void ClearTimer()
    {
        SearchTimer?.Dispose();
        SearchTimer = null;
    }

    /// <summary>
    /// 釋放資源
    /// </summary>
    public void Dispose()
    {
        ClearTimer();
    }
}

/// <summary>
/// AutoComplete 欄位狀態管理器
/// 管理多個 AutoComplete 欄位的狀態
/// </summary>
public class AutoCompleteStateManager : IDisposable
{
    private readonly Dictionary<string, AutoCompleteFieldState> _states = new();

    /// <summary>
    /// 取得或建立欄位狀態
    /// </summary>
    /// <param name="fieldId">欄位識別碼</param>
    /// <returns>欄位狀態</returns>
    public AutoCompleteFieldState GetOrCreate(string fieldId)
    {
        if (!_states.TryGetValue(fieldId, out var state))
        {
            state = new AutoCompleteFieldState();
            _states[fieldId] = state;
        }
        return state;
    }

    /// <summary>
    /// 嘗試取得欄位狀態
    /// </summary>
    /// <param name="fieldId">欄位識別碼</param>
    /// <param name="state">輸出的欄位狀態</param>
    /// <returns>是否存在該欄位狀態</returns>
    public bool TryGet(string fieldId, out AutoCompleteFieldState? state)
    {
        return _states.TryGetValue(fieldId, out state);
    }

    /// <summary>
    /// 檢查欄位狀態是否存在
    /// </summary>
    /// <param name="fieldId">欄位識別碼</param>
    /// <returns>是否存在</returns>
    public bool Contains(string fieldId) => _states.ContainsKey(fieldId);

    /// <summary>
    /// 移除欄位狀態
    /// </summary>
    /// <param name="fieldId">欄位識別碼</param>
    public void Remove(string fieldId)
    {
        if (_states.TryGetValue(fieldId, out var state))
        {
            state.Dispose();
            _states.Remove(fieldId);
        }
    }

    /// <summary>
    /// 清除所有欄位狀態
    /// </summary>
    public void Clear()
    {
        foreach (var state in _states.Values)
        {
            state.Dispose();
        }
        _states.Clear();
    }

    /// <summary>
    /// 釋放所有資源
    /// </summary>
    public void Dispose()
    {
        Clear();
    }
}
