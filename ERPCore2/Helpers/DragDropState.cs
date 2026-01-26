namespace ERPCore2.Helpers;

/// <summary>
/// 拖放狀態管理 - 用於在不同組件間傳遞拖曳中的項目
/// 支援跨表格拖放和同表格內排序
/// </summary>
public static class DragDropState
{
    /// <summary>
    /// 當前拖曳中的項目
    /// </summary>
    public static object? DraggedItem { get; set; }
    
    /// <summary>
    /// 拖曳來源的群組識別（相同群組才能互相拖放）
    /// </summary>
    public static string? DragDropGroup { get; set; }
    
    /// <summary>
    /// 拖曳項目的原始索引（用於同表格排序）
    /// </summary>
    public static int? DraggedIndex { get; set; }
    
    /// <summary>
    /// 拖曳來源表格的識別碼
    /// </summary>
    public static string? SourceTableId { get; set; }
    
    /// <summary>
    /// 是否正在拖曳中
    /// </summary>
    public static bool IsDragging => DraggedItem != null;
    
    /// <summary>
    /// 開始拖曳
    /// </summary>
    public static void StartDrag(object item, string group, int index, string tableId)
    {
        DraggedItem = item;
        DragDropGroup = group;
        DraggedIndex = index;
        SourceTableId = tableId;
    }
    
    /// <summary>
    /// 結束拖曳，清除狀態
    /// </summary>
    public static void EndDrag()
    {
        DraggedItem = null;
        DragDropGroup = null;
        DraggedIndex = null;
        SourceTableId = null;
    }
    
    /// <summary>
    /// 檢查是否可以放置到指定群組
    /// </summary>
    public static bool CanDropTo(string targetGroup)
    {
        return IsDragging && DragDropGroup == targetGroup;
    }
    
    /// <summary>
    /// 檢查是否來自同一個表格（用於排序功能）
    /// </summary>
    public static bool IsFromSameTable(string tableId)
    {
        return SourceTableId == tableId;
    }
    
    /// <summary>
    /// 取得拖曳中的項目（泛型版本）
    /// </summary>
    public static T? GetDraggedItem<T>() where T : class
    {
        return DraggedItem as T;
    }
}
