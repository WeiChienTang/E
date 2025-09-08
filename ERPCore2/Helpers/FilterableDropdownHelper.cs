using Microsoft.AspNetCore.Components;
using System.ComponentModel;

namespace ERPCore2.Helpers
{
    /// <summary>
    /// 可篩選下拉選單 Helper - 提供統一的「可輸入篩選的下拉選單」功能
    /// 適用於表格中需要下拉選擇且可透過輸入進行篩選的欄位
    /// </summary>
    public static class FilterableDropdownHelper
    {
        /// <summary>
        /// 泛型版本的可篩選下拉選單管理器
        /// </summary>
        /// <typeparam name="TRowItem">行項目類型</typeparam>
        /// <typeparam name="TDropdownItem">下拉選項類型</typeparam>
        public static class For<TRowItem, TDropdownItem>
        {
            /// <summary>
            /// 可篩選下拉選單配置
            /// </summary>
            public class DropdownConfig
            {
                /// <summary>
                /// 下拉選項顯示格式化函數
                /// </summary>
                public Func<TDropdownItem, string> DisplayFormatter { get; set; } = item => item?.ToString() ?? "";
                
                /// <summary>
                /// 文字篩選函數 - 根據輸入文字篩選下拉選項
                /// </summary>
                public Func<string, IEnumerable<TDropdownItem>, IEnumerable<TDropdownItem>> TextFilter { get; set; } = DefaultTextFilter;
                
                /// <summary>
                /// 外部篩選函數 - 根據其他條件篩選下拉選項（如供應商篩選商品）
                /// </summary>
                public Func<IEnumerable<TDropdownItem>>? ExternalFilter { get; set; }
                
                /// <summary>
                /// 最大顯示項目數量
                /// </summary>
                public int MaxDisplayItems { get; set; } = 20;
                
                /// <summary>
                /// 下拉選單樣式類別
                /// </summary>
                public string DropdownCssClass { get; set; } = "dropdown-menu show position-fixed w-auto shadow";
                
                /// <summary>
                /// 下拉選單樣式
                /// </summary>
                public string DropdownStyle { get; set; } = "z-index: 9999; max-height: 200px; overflow-y: auto; overflow-x: hidden; border: 1px solid #dee2e6; min-width: 300px; max-width: 500px;";
                
                /// <summary>
                /// 下拉選項樣式類別
                /// </summary>
                public string DropdownItemCssClass { get; set; } = "dropdown-item";
                
                /// <summary>
                /// 下拉選項樣式
                /// </summary>
                public string DropdownItemStyle { get; set; } = "cursor: pointer; padding: 8px 12px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis;";
                
                /// <summary>
                /// 選中項目的額外樣式類別
                /// </summary>
                public string SelectedItemCssClass { get; set; } = "active bg-primary text-white";
            }

            /// <summary>
            /// 可篩選下拉選單狀態
            /// </summary>
            public class DropdownState
            {
                /// <summary>
                /// 搜尋文字
                /// </summary>
                public string SearchText { get; set; } = string.Empty;
                
                /// <summary>
                /// 是否顯示下拉選單
                /// </summary>
                public bool ShowDropdown { get; set; } = false;
                
                /// <summary>
                /// 選中的索引
                /// </summary>
                public int SelectedIndex { get; set; } = -1;
                
                /// <summary>
                /// 篩選後的選項
                /// </summary>
                public List<TDropdownItem> FilteredItems { get; set; } = new List<TDropdownItem>();
                
                /// <summary>
                /// 選中的項目
                /// </summary>
                public TDropdownItem? SelectedItem { get; set; }
            }

            /// <summary>
            /// 預設文字篩選邏輯 - 轉換為字串後進行包含比對
            /// </summary>
            /// <param name="searchText">搜尋文字</param>
            /// <param name="items">原始項目清單</param>
            /// <returns>篩選後的項目清單</returns>
            public static IEnumerable<TDropdownItem> DefaultTextFilter(string searchText, IEnumerable<TDropdownItem> items)
            {
                if (string.IsNullOrWhiteSpace(searchText))
                    return items;

                return items.Where(item => 
                    item?.ToString()?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true);
            }

            /// <summary>
            /// 處理輸入事件 - 更新搜尋文字並篩選選項
            /// </summary>
            /// <param name="state">下拉狀態</param>
            /// <param name="config">下拉配置</param>
            /// <param name="searchText">搜尋文字</param>
            /// <param name="sourceItems">原始項目清單</param>
            /// <returns>是否有篩選結果</returns>
            public static bool HandleInputChange(DropdownState state, DropdownConfig config, string? searchText, IEnumerable<TDropdownItem> sourceItems)
            {
                state.SearchText = searchText ?? string.Empty;
                
                // 重置選中項目（當輸入改變時）
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    state.SelectedItem = default(TDropdownItem);
                }
                
                // 應用外部篩選
                var availableItems = config.ExternalFilter?.Invoke() ?? sourceItems;
                
                // 應用文字篩選
                var filteredItems = config.TextFilter(state.SearchText, availableItems)
                    .Take(config.MaxDisplayItems)
                    .ToList();
                
                state.FilteredItems = filteredItems;
                state.ShowDropdown = filteredItems.Any();
                state.SelectedIndex = filteredItems.Any() ? 0 : -1;
                
                return filteredItems.Any();
            }

            /// <summary>
            /// 處理焦點事件 - 顯示下拉選單
            /// </summary>
            /// <param name="state">下拉狀態</param>
            /// <param name="config">下拉配置</param>
            /// <param name="sourceItems">原始項目清單</param>
            public static void HandleFocus(DropdownState state, DropdownConfig config, IEnumerable<TDropdownItem> sourceItems)
            {
                if (!string.IsNullOrWhiteSpace(state.SearchText))
                {
                    HandleInputChange(state, config, state.SearchText, sourceItems);
                }
                else
                {
                    // 顯示所有可用選項
                    var availableItems = config.ExternalFilter?.Invoke() ?? sourceItems;
                    state.FilteredItems = availableItems.Take(config.MaxDisplayItems).ToList();
                    state.ShowDropdown = state.FilteredItems.Any();
                    state.SelectedIndex = -1;
                }
            }

            /// <summary>
            /// 處理失去焦點事件 - 延遲隱藏下拉選單
            /// </summary>
            /// <param name="state">下拉狀態</param>
            /// <param name="delayMs">延遲毫秒數，預設100ms</param>
            /// <returns>延遲任務</returns>
            public static async Task HandleBlurAsync(DropdownState state, int delayMs = 100)
            {
                // 延遲關閉下拉選單，讓用戶有時間點擊下拉選項
                await Task.Delay(delayMs);
                state.ShowDropdown = false;
            }

            /// <summary>
            /// 選擇項目
            /// </summary>
            /// <param name="state">下拉狀態</param>
            /// <param name="config">下拉配置</param>
            /// <param name="selectedItem">選中的項目</param>
            public static void SelectItem(DropdownState state, DropdownConfig config, TDropdownItem selectedItem)
            {
                state.SelectedItem = selectedItem;
                state.SearchText = config.DisplayFormatter(selectedItem);
                state.ShowDropdown = false;
                state.SelectedIndex = -1;
            }

            /// <summary>
            /// 處理鍵盤導航 - 上下箭頭選擇
            /// </summary>
            /// <param name="state">下拉狀態</param>
            /// <param name="keyCode">鍵盤按鍵代碼</param>
            /// <returns>是否處理了按鍵事件</returns>
            public static bool HandleKeyNavigation(DropdownState state, string keyCode)
            {
                if (!state.ShowDropdown || !state.FilteredItems.Any())
                    return false;

                switch (keyCode)
                {
                    case "ArrowDown":
                        state.SelectedIndex = Math.Min(state.SelectedIndex + 1, state.FilteredItems.Count - 1);
                        return true;
                    
                    case "ArrowUp":
                        state.SelectedIndex = Math.Max(state.SelectedIndex - 1, 0);
                        return true;
                    
                    case "Enter":
                        if (state.SelectedIndex >= 0 && state.SelectedIndex < state.FilteredItems.Count)
                        {
                            var selectedItem = state.FilteredItems[state.SelectedIndex];
                            state.SelectedItem = selectedItem;
                            state.ShowDropdown = false;
                            return true;
                        }
                        break;
                    
                    case "Escape":
                        state.ShowDropdown = false;
                        return true;
                }

                return false;
            }

            /// <summary>
            /// 取得當前選中項目的顯示文字
            /// </summary>
            /// <param name="state">下拉狀態</param>
            /// <param name="config">下拉配置</param>
            /// <returns>顯示文字</returns>
            public static string GetDisplayText(DropdownState state, DropdownConfig config)
            {
                if (state.SelectedItem != null)
                {
                    return config.DisplayFormatter(state.SelectedItem);
                }
                return state.SearchText;
            }

            /// <summary>
            /// 重置下拉狀態
            /// </summary>
            /// <param name="state">下拉狀態</param>
            public static void Reset(DropdownState state)
            {
                state.SearchText = string.Empty;
                state.ShowDropdown = false;
                state.SelectedIndex = -1;
                state.FilteredItems.Clear();
                state.SelectedItem = default(TDropdownItem);
            }

            /// <summary>
            /// 設定選中項目（用於初始化或外部設定）
            /// </summary>
            /// <param name="state">下拉狀態</param>
            /// <param name="config">下拉配置</param>
            /// <param name="item">要設定的項目</param>
            public static void SetSelectedItem(DropdownState state, DropdownConfig config, TDropdownItem? item)
            {
                state.SelectedItem = item;
                state.SearchText = item != null ? config.DisplayFormatter(item) : string.Empty;
                state.ShowDropdown = false;
                state.SelectedIndex = -1;
            }

            /// <summary>
            /// 檢查是否有選中項目
            /// </summary>
            /// <param name="state">下拉狀態</param>
            /// <returns>是否有選中項目</returns>
            public static bool HasSelectedItem(DropdownState state)
            {
                return state.SelectedItem != null;
            }

            /// <summary>
            /// 取得下拉選項的CSS類別（包含選中狀態）
            /// </summary>
            /// <param name="config">下拉配置</param>
            /// <param name="index">選項索引</param>
            /// <param name="selectedIndex">選中索引</param>
            /// <returns>CSS類別字串</returns>
            public static string GetDropdownItemCssClass(DropdownConfig config, int index, int selectedIndex)
            {
                var cssClass = config.DropdownItemCssClass;
                if (index == selectedIndex)
                {
                    cssClass += " " + config.SelectedItemCssClass;
                }
                return cssClass;
            }
        }
    }
}
