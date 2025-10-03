using ERPCore2.Data;
using Microsoft.AspNetCore.Components;

namespace ERPCore2.Helpers
{
    /// <summary>
    /// 自動空行管理 Helper
    /// 提供統一的「始終保持一行空行可以輸入」功能
    /// </summary>
    public static class AutoEmptyRowHelper
    {
        /// <summary>
        /// 泛型版本的自動空行管理器
        /// </summary>
        /// <typeparam name="TItem">項目類型</typeparam>
        public static class For<TItem> where TItem : BaseEntity, new()
        {
            /// <summary>
            /// 檢查項目是否為空行
            /// </summary>
            /// <param name="item">要檢查的項目</param>
            /// <param name="isEmptyChecker">自訂的空行檢查邏輯</param>
            /// <returns>是否為空行</returns>
            public static bool IsEmptyRow(TItem item, Func<TItem, bool> isEmptyChecker)
            {
                return isEmptyChecker(item);
            }

            /// <summary>
            /// 確保清單中始終有一行空行
            /// </summary>
            /// <param name="items">項目清單</param>
            /// <param name="isEmptyChecker">空行檢查邏輯</param>
            /// <param name="createEmptyItem">建立空項目的邏輯</param>
            /// <param name="setParentId">設定父項目ID的邏輯（可選）</param>
            /// <param name="parentId">父項目ID（可選）</param>
            public static void EnsureOneEmptyRow(
                List<TItem> items, 
                Func<TItem, bool> isEmptyChecker,
                Func<TItem> createEmptyItem,
                Action<TItem, int>? setParentId = null,
                int? parentId = null)
            {
                var emptyCount = items.Count(item => isEmptyChecker(item));

                if (emptyCount < 1)
                {
                    var newItem = createEmptyItem();
                    if (setParentId != null && parentId.HasValue)
                    {
                        setParentId(newItem, parentId.Value);
                    }
                    items.Add(newItem);
                }
            }

            /// <summary>
            /// 處理輸入變更，自動新增空行
            /// </summary>
            /// <param name="items">項目清單</param>
            /// <param name="currentItem">當前編輯的項目</param>
            /// <param name="isEmptyChecker">空行檢查邏輯</param>
            /// <param name="createEmptyItem">建立空項目的邏輯</param>
            /// <param name="wasEmpty">輸入前是否為空行</param>
            /// <param name="hasValueNow">現在是否有值</param>
            /// <param name="setParentId">設定父項目ID的邏輯（可選）</param>
            /// <param name="parentId">父項目ID（可選）</param>
            /// <returns>是否有新增空行</returns>
            public static bool HandleInputChange(
                List<TItem> items,
                TItem currentItem,
                Func<TItem, bool> isEmptyChecker,
                Func<TItem> createEmptyItem,
                bool wasEmpty,
                bool hasValueNow,
                Action<TItem, int>? setParentId = null,
                int? parentId = null)
            {
                // 只有在原本是空行且現在有值，並且當前沒有其他空行時才新增
                if (wasEmpty && hasValueNow)
                {
                    // 檢查除了當前項目外，是否還有其他空行
                    var otherEmptyItems = items.Where(item => !ReferenceEquals(item, currentItem) && isEmptyChecker(item)).ToList();
                    
                    // 如果沒有其他空行，才新增新的空行
                    if (!otherEmptyItems.Any())
                    {
                        var newItem = createEmptyItem();
                        if (setParentId != null && parentId.HasValue)
                        {
                            setParentId(newItem, parentId.Value);
                        }
                        items.Add(newItem);
                        return true;
                    }
                }
                return false;
            }

            /// <summary>
            /// 檢查整行是否真的從空變成有值（更精確的判斷）
            /// 這個方法在設定值後才檢查整行狀態，避免單一欄位判斷的問題
            /// </summary>
            /// <param name="items">項目清單</param>
            /// <param name="currentItem">當前編輯的項目</param>
            /// <param name="isEmptyChecker">空行檢查邏輯</param>
            /// <param name="createEmptyItem">建立空項目的邏輯</param>
            /// <param name="wasEmpty">輸入前是否為空行</param>
            /// <param name="setParentId">設定父項目ID的邏輯（可選）</param>
            /// <param name="parentId">父項目ID（可選）</param>
            /// <returns>是否有新增空行</returns>
            public static bool HandleInputChangeAdvanced(
                List<TItem> items,
                TItem currentItem,
                Func<TItem, bool> isEmptyChecker,
                Func<TItem> createEmptyItem,
                bool wasEmpty,
                Action<TItem, int>? setParentId = null,
                int? parentId = null)
            {
                var isEmptyNow = isEmptyChecker(currentItem);

                // 只有在原本是空行，現在變成非空行，且沒有其他空行時才新增
                if (wasEmpty && !isEmptyNow)
                {
                    // 檢查除了當前項目外，是否還有其他空行
                    var otherEmptyItems = items.Where(item => !ReferenceEquals(item, currentItem) && isEmptyChecker(item)).ToList();
                    
                    // 如果沒有其他空行，才新增新的空行
                    if (!otherEmptyItems.Any())
                    {
                        var newItem = createEmptyItem();
                        if (setParentId != null && parentId.HasValue)
                        {
                            setParentId(newItem, parentId.Value);
                        }
                        items.Add(newItem);
                        return true;
                    }
                }
                return false;
            }

            /// <summary>
            /// 處理項目移除，確保還有空行
            /// </summary>
            /// <param name="items">項目清單</param>
            /// <param name="itemToRemove">要移除的項目</param>
            /// <param name="isEmptyChecker">空行檢查邏輯</param>
            /// <param name="createEmptyItem">建立空項目的邏輯</param>
            /// <param name="setParentId">設定父項目ID的邏輯（可選）</param>
            /// <param name="parentId">父項目ID（可選）</param>
            /// <returns>是否成功移除</returns>
            public static bool HandleItemRemove(
                List<TItem> items,
                TItem itemToRemove,
                Func<TItem, bool> isEmptyChecker,
                Func<TItem> createEmptyItem,
                Action<TItem, int>? setParentId = null,
                int? parentId = null)
            {
                if (items.Remove(itemToRemove))
                {
                    EnsureOneEmptyRow(items, isEmptyChecker, createEmptyItem, setParentId, parentId);
                    return true;
                }
                return false;
            }

            /// <summary>
            /// 取得非空行的項目清單（用於提交或驗證）
            /// </summary>
            /// <param name="items">項目清單</param>
            /// <param name="isEmptyChecker">空行檢查邏輯</param>
            /// <returns>非空行的項目清單</returns>
            public static List<TItem> GetNonEmptyItems(List<TItem> items, Func<TItem, bool> isEmptyChecker)
            {
                return items.Where(item => !isEmptyChecker(item)).ToList();
            }

            /// <summary>
            /// 驗證是否有足夠的非空項目
            /// </summary>
            /// <param name="items">項目清單</param>
            /// <param name="isEmptyChecker">空行檢查邏輯</param>
            /// <param name="minRequiredItems">最少需要的項目數量</param>
            /// <returns>是否有足夠的項目</returns>
            public static bool HasSufficientItems(List<TItem> items, Func<TItem, bool> isEmptyChecker, int minRequiredItems = 1)
            {
                var nonEmptyCount = items.Count(item => !isEmptyChecker(item));
                return nonEmptyCount >= minRequiredItems;
            }
        }

        /// <summary>
        /// 非泛型版本，適用於不繼承 BaseEntity 的類型
        /// </summary>
        /// <typeparam name="TItem">項目類型</typeparam>
        public static class ForAny<TItem> where TItem : new()
        {
            /// <summary>
            /// 檢查項目是否為空行
            /// </summary>
            public static bool IsEmptyRow(TItem item, Func<TItem, bool> isEmptyChecker)
            {
                return isEmptyChecker(item);
            }

            /// <summary>
            /// 確保清單中始終有一行空行
            /// </summary>
            public static void EnsureOneEmptyRow(
                List<TItem> items, 
                Func<TItem, bool> isEmptyChecker,
                Func<TItem> createEmptyItem)
            {
                var emptyCount = items.Count(item => isEmptyChecker(item));
                
                if (emptyCount < 1)
                {
                    items.Add(createEmptyItem());
                }
            }

            /// <summary>
            /// 處理輸入變更，自動新增空行
            /// </summary>
            public static bool HandleInputChange(
                List<TItem> items,
                TItem currentItem,
                Func<TItem, bool> isEmptyChecker,
                Func<TItem> createEmptyItem,
                bool wasEmpty,
                bool hasValueNow)
            {
                if (wasEmpty && hasValueNow)
                {
                    EnsureOneEmptyRow(items, isEmptyChecker, createEmptyItem);
                    return true;
                }
                return false;
            }

            /// <summary>
            /// 檢查整行是否真的從空變成有值（更精確的判斷）
            /// 這個方法在設定值後才檢查整行狀態，避免單一欄位判斷的問題
            /// </summary>
            /// <param name="items">項目清單</param>
            /// <param name="currentItem">當前編輯的項目</param>
            /// <param name="isEmptyChecker">空行檢查邏輯</param>
            /// <param name="createEmptyItem">建立空項目的邏輯</param>
            /// <param name="wasEmpty">輸入前是否為空行</param>
            /// <returns>是否有新增空行</returns>
            public static bool HandleInputChangeAdvanced(
                List<TItem> items,
                TItem currentItem,
                Func<TItem, bool> isEmptyChecker,
                Func<TItem> createEmptyItem,
                bool wasEmpty)
            {
                var isEmptyNow = isEmptyChecker(currentItem);

                // 只有在原本是空行，現在變成非空行，且沒有其他空行時才新增
                if (wasEmpty && !isEmptyNow)
                {
                    // 檢查除了當前項目外，是否還有其他空行
                    var otherEmptyItems = items.Where(item => !ReferenceEquals(item, currentItem) && isEmptyChecker(item)).ToList();
                    
                    // 如果沒有其他空行，才新增新的空行
                    if (!otherEmptyItems.Any())
                    {
                        var newItem = createEmptyItem();
                        items.Add(newItem);
                        return true;
                    }
                }
                return false;
            }

            /// <summary>
            /// 處理項目移除，確保還有空行
            /// </summary>
            public static bool HandleItemRemove(
                List<TItem> items,
                TItem itemToRemove,
                Func<TItem, bool> isEmptyChecker,
                Func<TItem> createEmptyItem)
            {
                if (items.Remove(itemToRemove))
                {
                    EnsureOneEmptyRow(items, isEmptyChecker, createEmptyItem);
                    return true;
                }
                return false;
            }

            /// <summary>
            /// 取得非空行的項目清單
            /// </summary>
            public static List<TItem> GetNonEmptyItems(List<TItem> items, Func<TItem, bool> isEmptyChecker)
            {
                return items.Where(item => !isEmptyChecker(item)).ToList();
            }

            /// <summary>
            /// 驗證是否有足夠的非空項目
            /// </summary>
            public static bool HasSufficientItems(List<TItem> items, Func<TItem, bool> isEmptyChecker, int minRequiredItems = 1)
            {
                var nonEmptyCount = items.Count(item => !isEmptyChecker(item));
                return nonEmptyCount >= minRequiredItems;
            }
        }
    }
}
