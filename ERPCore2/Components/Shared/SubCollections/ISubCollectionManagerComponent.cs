using ERPCore2.Data;
using ERPCore2.Data.Enums;

namespace ERPCore2.Components.Shared.SubCollections
{
    /// <summary>
    /// 子集合管理元件介面 - 定義所有子集合管理元件應實作的基本操作
    /// </summary>
    /// <typeparam name="TSubEntity">子實體類型</typeparam>
    /// <typeparam name="TParentEntity">父實體類型</typeparam>
    /// <typeparam name="TOptionEntity">選項實體類型（如AddressType, ContactType等）</typeparam>
    public interface ISubCollectionManagerComponent<TSubEntity, TParentEntity, TOptionEntity>
        where TSubEntity : BaseEntity, new()
        where TParentEntity : BaseEntity
        where TOptionEntity : BaseEntity
    {
        /// <summary>
        /// 子集合清單
        /// </summary>
        List<TSubEntity> Items { get; set; }

        /// <summary>
        /// 選項清單（如地址類型、聯絡類型等）
        /// </summary>
        List<TOptionEntity> Options { get; set; }

        /// <summary>
        /// 父實體ID
        /// </summary>
        int ParentEntityId { get; set; }

        /// <summary>
        /// 集合標題
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// 集合副標題
        /// </summary>
        string Subtitle { get; set; }

        /// <summary>
        /// 集合圖示
        /// </summary>
        string Icon { get; set; }

        /// <summary>
        /// 是否唯讀
        /// </summary>
        bool IsReadOnly { get; set; }

        /// <summary>
        /// 新增項目
        /// </summary>
        Task AddItemAsync();

        /// <summary>
        /// 移除項目
        /// </summary>
        /// <param name="index">項目索引</param>
        Task RemoveItemAsync(int index);

        /// <summary>
        /// 設定主要項目
        /// </summary>
        /// <param name="index">項目索引</param>
        Task SetPrimaryItemAsync(int index);

        /// <summary>
        /// 驗證所有項目
        /// </summary>
        /// <returns>驗證結果</returns>
        Task<bool> ValidateAllItemsAsync();

        /// <summary>
        /// 初始化預設項目
        /// </summary>
        Task InitializeDefaultItemsAsync();

        /// <summary>
        /// 項目變更事件
        /// </summary>
        event EventHandler<SubCollectionChangedEventArgs<TSubEntity>>? ItemsChanged;
    }

    /// <summary>
    /// 子集合變更事件參數
    /// </summary>
    /// <typeparam name="TSubEntity">子實體類型</typeparam>
    public class SubCollectionChangedEventArgs<TSubEntity> : EventArgs
        where TSubEntity : BaseEntity
    {
        public SubCollectionChangeType ChangeType { get; set; }
        public TSubEntity? Item { get; set; }
        public int Index { get; set; }
        public List<TSubEntity> AllItems { get; set; } = new();

        public SubCollectionChangedEventArgs(SubCollectionChangeType changeType, List<TSubEntity> allItems)
        {
            ChangeType = changeType;
            AllItems = allItems;
        }

        public SubCollectionChangedEventArgs(SubCollectionChangeType changeType, TSubEntity item, int index, List<TSubEntity> allItems)
        {
            ChangeType = changeType;
            Item = item;
            Index = index;
            AllItems = allItems;
        }
    }

    /// <summary>
    /// 子集合變更類型
    /// </summary>
    public enum SubCollectionChangeType
    {
        Added,
        Removed,
        Modified,
        PrimaryChanged,
        Initialized
    }
}
