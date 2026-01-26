namespace ERPCore2.Helpers;

/// <summary>
/// 拖放事件參數
/// </summary>
/// <typeparam name="T">表格項目的型別</typeparam>
public class DragDropEventArgs<T>
{
    /// <summary>
    /// 被放置的項目（原始型別為 object，需自行轉型）
    /// </summary>
    public object? DroppedItem { get; set; }
    
    /// <summary>
    /// 目標索引位置（如果放在某行上）
    /// </summary>
    public int? TargetIndex { get; set; }
    
    /// <summary>
    /// 來源表格識別碼
    /// </summary>
    public string SourceTableId { get; set; } = "";
    
    /// <summary>
    /// 目標表格識別碼
    /// </summary>
    public string TargetTableId { get; set; } = "";
    
    /// <summary>
    /// 取得強型別的放置項目
    /// </summary>
    public TItem? GetDroppedItem<TItem>() where TItem : class
    {
        return DroppedItem as TItem;
    }
}
