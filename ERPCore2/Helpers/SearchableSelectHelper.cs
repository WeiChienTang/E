using Microsoft.AspNetCore.Components;
using ERPCore2.Components.Shared.SubCollections;

namespace ERPCore2.Helpers
{
    /// <summary>
    /// 可搜尋下拉選單輔助類別
    /// </summary>
    public static class SearchableSelectHelper
    {
        /// <summary>
        /// 建立可搜尋下拉選單欄位定義
        /// </summary>
        /// <typeparam name="TItem">項目類型</typeparam>
        /// <typeparam name="TSelectItem">下拉選項類型</typeparam>
        /// <param name="title">欄位標題</param>
        /// <param name="width">欄位寬度</param>
        /// <param name="searchValuePropertyName">搜尋值屬性名稱</param>
        /// <param name="selectedItemPropertyName">選中項目屬性名稱</param>
        /// <param name="filteredItemsPropertyName">過濾項目列表屬性名稱</param>
        /// <param name="showDropdownPropertyName">顯示下拉選單屬性名稱</param>
        /// <param name="selectedIndexPropertyName">選中索引屬性名稱</param>
        /// <param name="availableItemsProvider">可用項目提供者</param>
        /// <param name="itemDisplayFormatter">項目顯示格式化函數</param>
        /// <param name="searchFilter">搜尋過濾函數</param>
        /// <param name="onSearchInputChanged">搜尋輸入變更事件</param>
        /// <param name="onItemSelected">項目選擇事件</param>
        /// <param name="onInputFocus">輸入框焦點事件</param>
        /// <param name="onInputBlur">輸入框失焦事件</param>
        /// <param name="onItemMouseEnter">項目滑鼠移入事件</param>
        /// <param name="placeholder">佔位符文字</param>
        /// <param name="maxDisplayItems">最大顯示項目數量</param>
        /// <param name="isReadOnly">是否唯讀</param>
        /// <returns>設定好的欄位定義</returns>
        public static InteractiveColumnDefinition CreateSearchableSelect<TItem, TSelectItem>(
            string title,
            string width = "25%",
            string searchValuePropertyName = "SearchValue",
            string selectedItemPropertyName = "SelectedItem",
            string filteredItemsPropertyName = "FilteredItems",
            string showDropdownPropertyName = "ShowDropdown",
            string selectedIndexPropertyName = "SelectedIndex",
            Func<IEnumerable<TSelectItem>>? availableItemsProvider = null,
            Func<TSelectItem, string>? itemDisplayFormatter = null,
            Func<TSelectItem, string, bool>? searchFilter = null,
            EventCallback<(TItem item, string? searchValue)>? onSearchInputChanged = null,
            EventCallback<(TItem item, TSelectItem? selectedItem)>? onItemSelected = null,
            EventCallback<TItem>? onInputFocus = null,
            EventCallback<TItem>? onInputBlur = null,
            EventCallback<(TItem item, int index)>? onItemMouseEnter = null,
            string placeholder = "請選擇...",
            int maxDisplayItems = 20,
            bool isReadOnly = false)
            where TItem : class
            where TSelectItem : class
        {
            var column = new InteractiveColumnDefinition
            {
                Title = title,
                ColumnType = InteractiveColumnType.SearchableSelect,
                Width = width,
                SearchValuePropertyName = searchValuePropertyName,
                SelectedItemPropertyName = selectedItemPropertyName,
                FilteredItemsPropertyName = filteredItemsPropertyName,
                ShowDropdownPropertyName = showDropdownPropertyName,
                SelectedIndexPropertyName = selectedIndexPropertyName,
                Placeholder = placeholder,
                MaxDisplayItems = maxDisplayItems,
                IsReadOnly = isReadOnly
            };

            // 設定可用項目提供者
            if (availableItemsProvider != null)
            {
                column.AvailableItemsProvider = () => availableItemsProvider().Cast<object>();
            }

            // 設定項目顯示格式化函數
            if (itemDisplayFormatter != null)
            {
                column.ItemDisplayFormatter = obj => obj is TSelectItem item ? itemDisplayFormatter(item) : obj?.ToString() ?? "";
            }

            // 設定搜尋過濾函數
            if (searchFilter != null)
            {
                column.SearchFilter = (obj, searchValue) => obj is TSelectItem item && searchFilter(item, searchValue);
            }

            // 設定事件處理器 - 使用 Action 委託進行類型轉換
            if (onSearchInputChanged.HasValue)
            {
                column.SearchInputChangedAction = (item, searchValue) => 
                {
                    if (item is TItem typedItem)
                    {
                        onSearchInputChanged.Value.InvokeAsync((typedItem, searchValue));
                    }
                };
            }
            
            if (onItemSelected.HasValue)
            {
                column.ItemSelectedAction = (item, selectedItem) => 
                {
                    if (item is TItem typedItem)
                    {
                        onItemSelected.Value.InvokeAsync((typedItem, (TSelectItem?)selectedItem));
                    }
                };
            }
            
            if (onInputFocus.HasValue)
            {
                column.InputFocusAction = (item) => 
                {
                    if (item is TItem typedItem)
                    {
                        onInputFocus.Value.InvokeAsync(typedItem);
                    }
                };
            }
            
            if (onInputBlur.HasValue)
            {
                column.InputBlurAction = (item) => 
                {
                    if (item is TItem typedItem)
                    {
                        onInputBlur.Value.InvokeAsync(typedItem);
                    }
                };
            }
            
            if (onItemMouseEnter.HasValue)
            {
                column.ItemMouseEnterAction = (item, index) => 
                {
                    if (item is TItem typedItem)
                    {
                        onItemMouseEnter.Value.InvokeAsync((typedItem, index));
                    }
                };
            }

            // 啟用鍵盤導航
            column.EnableKeyboardNavigation = true;

            // 設定鍵盤導航相關函數
            column.GetDropdownItems = (item) => {
                var filteredItems = GetPropertyValue(item, filteredItemsPropertyName) as IEnumerable<object>;
                return filteredItems ?? new List<object>();
            };

            column.GetSelectedIndex = (item) => {
                return GetPropertyValue(item, selectedIndexPropertyName) as int? ?? -1;
            };

            column.SetSelectedIndex = (item, index) => {
                SetPropertyValue(item, selectedIndexPropertyName, index);
            };

            column.GetShowDropdown = (item) => {
                return GetPropertyValue(item, showDropdownPropertyName) as bool? ?? false;
            };

            column.SetShowDropdown = (item, show) => {
                SetPropertyValue(item, showDropdownPropertyName, show);
            };

            if (onItemSelected.HasValue)
            {
                // 使用委託方式處理項目選擇事件
                var originalCallback = onItemSelected.Value;
                column.OnDropdownItemSelected = EventCallback.Factory.Create<(object, object?)>(
                    originalCallback,
                    async (args) => {
                        // 調用原始的 onItemSelected 事件
                        if (args.Item1 is TItem typedItem)
                        {
                            await originalCallback.InvokeAsync((typedItem, (TSelectItem?)args.Item2));
                        }
                    });
            }

            return column;
        }

        /// <summary>
        /// 使用反射取得屬性值
        /// </summary>
        private static object? GetPropertyValue(object obj, string propertyName)
        {
            var property = obj.GetType().GetProperty(propertyName);
            return property?.GetValue(obj);
        }

        /// <summary>
        /// 使用反射設定屬性值
        /// </summary>
        private static void SetPropertyValue(object obj, string propertyName, object? value)
        {
            var property = obj.GetType().GetProperty(propertyName);
            if (property != null && property.CanWrite)
            {
                if (value != null && property.PropertyType != value.GetType())
                {
                    if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        var underlyingType = Nullable.GetUnderlyingType(property.PropertyType);
                        value = Convert.ChangeType(value, underlyingType!);
                    }
                    else
                    {
                        value = Convert.ChangeType(value, property.PropertyType);
                    }
                }
                property.SetValue(obj, value);
            }
        }
        
        /// <summary>
        /// 建立標準的商品搜尋下拉選單欄位定義
        /// </summary>
        /// <typeparam name="TItem">項目類型</typeparam>
        /// <typeparam name="TProduct">商品類型</typeparam>
        /// <param name="title">欄位標題</param>
        /// <param name="availableProductsProvider">可用商品提供者</param>
        /// <param name="onSearchInputChanged">搜尋輸入變更事件</param>
        /// <param name="onProductSelected">商品選擇事件</param>
        /// <param name="onInputFocus">輸入框焦點事件</param>
        /// <param name="onInputBlur">輸入框失焦事件</param>
        /// <param name="onItemMouseEnter">項目滑鼠移入事件</param>
        /// <param name="isReadOnly">是否唯讀</param>
        /// <returns>設定好的商品搜尋欄位定義</returns>
        public static InteractiveColumnDefinition CreateProductSearchableSelect<TItem, TProduct>(
            string title = "商品",
            Func<IEnumerable<TProduct>>? availableProductsProvider = null,
            EventCallback<(TItem item, string? searchValue)>? onSearchInputChanged = null,
            EventCallback<(TItem item, TProduct? selectedProduct)>? onProductSelected = null,
            EventCallback<TItem>? onInputFocus = null,
            EventCallback<TItem>? onInputBlur = null,
            EventCallback<(TItem item, int index)>? onItemMouseEnter = null,
            bool isReadOnly = false)
            where TItem : class
            where TProduct : class
        {
            return CreateSearchableSelect<TItem, TProduct>(
                title: title,
                width: "25%",
                searchValuePropertyName: "ProductSearch",
                selectedItemPropertyName: "SelectedProduct",
                filteredItemsPropertyName: "FilteredProducts",
                showDropdownPropertyName: "ShowDropdown",
                selectedIndexPropertyName: "SelectedIndex",
                availableItemsProvider: availableProductsProvider,
                itemDisplayFormatter: product => FormatProductDisplay(product),
                searchFilter: (product, searchValue) => FilterProduct(product, searchValue),
                onSearchInputChanged: onSearchInputChanged,
                onItemSelected: onProductSelected,
                onInputFocus: onInputFocus,
                onInputBlur: onInputBlur,
                onItemMouseEnter: onItemMouseEnter,
                placeholder: "請選擇商品...",
                maxDisplayItems: 20,
                isReadOnly: isReadOnly
            );
        }

        /// <summary>
        /// 格式化商品顯示文字（假設商品有 Code 和 Name 屬性）
        /// </summary>
        private static string FormatProductDisplay<TProduct>(TProduct product)
        {
            if (product == null) return "";
            
            var type = typeof(TProduct);
            var codeProperty = type.GetProperty("Code");
            var nameProperty = type.GetProperty("Name");
            
            var code = codeProperty?.GetValue(product)?.ToString() ?? "";
            var name = nameProperty?.GetValue(product)?.ToString() ?? "";
            
            if (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(name))
            {
                return $"<strong>{code}</strong> - {name}";
            }
            else if (!string.IsNullOrEmpty(code))
            {
                return $"<strong>{code}</strong>";
            }
            else if (!string.IsNullOrEmpty(name))
            {
                return name;
            }
            
            return product.ToString() ?? "";
        }

        /// <summary>
        /// 過濾商品（假設商品有 Code 和 Name 屬性）
        /// </summary>
        private static bool FilterProduct<TProduct>(TProduct product, string searchValue)
        {
            if (product == null || string.IsNullOrWhiteSpace(searchValue)) return true;
            
            var type = typeof(TProduct);
            var codeProperty = type.GetProperty("Code");
            var nameProperty = type.GetProperty("Name");
            
            var code = codeProperty?.GetValue(product)?.ToString() ?? "";
            var name = nameProperty?.GetValue(product)?.ToString() ?? "";
            
            return code.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                   name.Contains(searchValue, StringComparison.OrdinalIgnoreCase);
        }
    }
}
