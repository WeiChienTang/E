using ERPCore2.Data.Entities;
using Microsoft.AspNetCore.Components;
using ERPCore2.Components.Shared.SubCollections;

namespace ERPCore2.Helpers
{
    /// <summary>
    /// 商品選擇輔助類別 - 提供統一的商品/明細選擇器
    /// 專門處理各種商品選擇場景，包含純商品、訂單明細、採購明細、進貨明細等
    /// </summary>
    public static class ProductSelectHelper
    {
        #region 純商品選擇器 - 適用於 SalesOrderDetailManagerComponent

        /// <summary>
        /// 建立純商品選擇器
        /// 適用於銷售訂單等需要直接選擇商品的場景
        /// </summary>
        /// <typeparam name="TItem">項目類型</typeparam>
        /// <param name="title">欄位標題</param>
        /// <param name="width">欄位寬度</param>
        /// <param name="availableProductsProvider">可用商品提供者</param>
        /// <param name="onProductSelected">商品選擇事件</param>
        /// <param name="onSearchInputChanged">搜尋輸入變更事件</param>
        /// <param name="onInputFocus">輸入框焦點事件</param>
        /// <param name="onInputBlur">輸入框失焦事件</param>
        /// <param name="onItemMouseEnter">項目滑鼠移入事件</param>
        /// <param name="placeholder">佔位符文字</param>
        /// <param name="isReadOnly">是否唯讀</param>
        /// <returns>設定好的商品選擇欄位定義</returns>
        public static InteractiveColumnDefinition CreateProductSelect<TItem>(
            string title = "商品",
            string width = "25%",
            Func<IEnumerable<Product>>? availableProductsProvider = null,
            EventCallback<(TItem item, Product? selectedProduct)>? onProductSelected = null,
            EventCallback<(TItem item, string? searchValue)>? onSearchInputChanged = null,
            EventCallback<TItem>? onInputFocus = null,
            EventCallback<TItem>? onInputBlur = null,
            EventCallback<(TItem item, int index)>? onItemMouseEnter = null,
            string placeholder = "請選擇商品...",
            bool isReadOnly = false)
            where TItem : class
        {
            return SearchableSelectHelper.CreateSearchableSelect<TItem, Product>(
                title: title,
                width: width,
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
                placeholder: placeholder,
                maxDisplayItems: 20,
                isReadOnly: isReadOnly
            );
        }

        #endregion

        #region 銷售訂單明細選擇器 - 適用於 SalesReturnDetailManagerComponent

        /// <summary>
        /// 建立銷售訂單明細選擇器
        /// 適用於銷售退貨等需要選擇原始銷售訂單明細的場景
        /// </summary>
        /// <typeparam name="TItem">項目類型</typeparam>
        /// <param name="title">欄位標題</param>
        /// <param name="width">欄位寬度</param>
        /// <param name="availableOrderDetailsProvider">可用訂單明細提供者</param>
        /// <param name="onOrderDetailSelected">訂單明細選擇事件</param>
        /// <param name="onSearchInputChanged">搜尋輸入變更事件</param>
        /// <param name="onInputFocus">輸入框焦點事件</param>
        /// <param name="onInputBlur">輸入框失焦事件</param>
        /// <param name="onItemMouseEnter">項目滑鼠移入事件</param>
        /// <param name="placeholder">佔位符文字</param>
        /// <param name="isReadOnly">是否唯讀</param>
        /// <returns>設定好的訂單明細選擇欄位定義</returns>
        public static InteractiveColumnDefinition CreateOrderDetailSelect<TItem>(
            string title = "銷售商品",
            string width = "25%",
            Func<IEnumerable<SalesOrderDetail>>? availableOrderDetailsProvider = null,
            EventCallback<(TItem item, SalesOrderDetail? selectedDetail)>? onOrderDetailSelected = null,
            EventCallback<(TItem item, string? searchValue)>? onSearchInputChanged = null,
            EventCallback<TItem>? onInputFocus = null,
            EventCallback<TItem>? onInputBlur = null,
            EventCallback<(TItem item, int index)>? onItemMouseEnter = null,
            string placeholder = "請選擇銷售商品...",
            bool isReadOnly = false)
            where TItem : class
        {
            return SearchableSelectHelper.CreateSearchableSelect<TItem, SalesOrderDetail>(
                title: title,
                width: width,
                searchValuePropertyName: "OrderDetailSearch",
                selectedItemPropertyName: "SelectedOrderDetail",
                filteredItemsPropertyName: "FilteredOrderDetails",
                showDropdownPropertyName: "ShowDropdown",
                selectedIndexPropertyName: "SelectedIndex",
                availableItemsProvider: availableOrderDetailsProvider,
                itemDisplayFormatter: detail => FormatSalesOrderDetailDisplay(detail),
                searchFilter: (detail, searchValue) => FilterSalesOrderDetail(detail, searchValue),
                onSearchInputChanged: onSearchInputChanged,
                onItemSelected: onOrderDetailSelected,
                onInputFocus: onInputFocus,
                onInputBlur: onInputBlur,
                onItemMouseEnter: onItemMouseEnter,
                placeholder: placeholder,
                maxDisplayItems: 20,
                isReadOnly: isReadOnly
            );
        }

        #endregion

        #region 採購明細選擇器 - 適用於 PurchaseReceivingDetailManagerComponent

        /// <summary>
        /// 建立採購明細選擇器
        /// 適用於採購入庫等需要選擇原始採購訂單明細的場景
        /// </summary>
        /// <typeparam name="TItem">項目類型</typeparam>
        /// <param name="title">欄位標題</param>
        /// <param name="width">欄位寬度</param>
        /// <param name="availablePurchaseDetailsProvider">可用採購明細提供者</param>
        /// <param name="onPurchaseDetailSelected">採購明細選擇事件</param>
        /// <param name="onSearchInputChanged">搜尋輸入變更事件</param>
        /// <param name="onInputFocus">輸入框焦點事件</param>
        /// <param name="onInputBlur">輸入框失焦事件</param>
        /// <param name="onItemMouseEnter">項目滑鼠移入事件</param>
        /// <param name="placeholder">佔位符文字</param>
        /// <param name="isReadOnly">是否唯讀</param>
        /// <returns>設定好的採購明細選擇欄位定義</returns>
        public static InteractiveColumnDefinition CreatePurchaseDetailSelect<TItem>(
            string title = "商品",
            string width = "25%",
            Func<IEnumerable<PurchaseOrderDetail>>? availablePurchaseDetailsProvider = null,
            EventCallback<(TItem item, PurchaseOrderDetail? selectedDetail)>? onPurchaseDetailSelected = null,
            EventCallback<(TItem item, string? searchValue)>? onSearchInputChanged = null,
            EventCallback<TItem>? onInputFocus = null,
            EventCallback<TItem>? onInputBlur = null,
            EventCallback<(TItem item, int index)>? onItemMouseEnter = null,
            string placeholder = "請選擇採購明細...",
            bool isReadOnly = false)
            where TItem : class
        {
            return SearchableSelectHelper.CreateSearchableSelect<TItem, PurchaseOrderDetail>(
                title: title,
                width: width,
                searchValuePropertyName: "ProductSearch",
                selectedItemPropertyName: "SelectedPurchaseDetail",
                filteredItemsPropertyName: "FilteredPurchaseDetails",
                showDropdownPropertyName: "ShowDropdown",
                selectedIndexPropertyName: "SelectedIndex",
                availableItemsProvider: availablePurchaseDetailsProvider,
                itemDisplayFormatter: detail => FormatPurchaseOrderDetailDisplay(detail),
                searchFilter: (detail, searchValue) => FilterPurchaseOrderDetail(detail, searchValue),
                onSearchInputChanged: onSearchInputChanged,
                onItemSelected: onPurchaseDetailSelected,
                onInputFocus: onInputFocus,
                onInputBlur: onInputBlur,
                onItemMouseEnter: onItemMouseEnter,
                placeholder: placeholder,
                maxDisplayItems: 20,
                isReadOnly: isReadOnly
            );
        }

        #endregion

        #region 進貨明細選擇器 - 適用於 PurchaseReturnDetailManagerComponent

        /// <summary>
        /// 建立進貨明細選擇器
        /// 適用於採購退回等需要選擇原始進貨明細的場景
        /// </summary>
        /// <typeparam name="TItem">項目類型</typeparam>
        /// <param name="title">欄位標題</param>
        /// <param name="width">欄位寬度</param>
        /// <param name="availableReceivingDetailsProvider">可用進貨明細提供者</param>
        /// <param name="onReceivingDetailSelected">進貨明細選擇事件</param>
        /// <param name="onSearchInputChanged">搜尋輸入變更事件</param>
        /// <param name="onInputFocus">輸入框焦點事件</param>
        /// <param name="onInputBlur">輸入框失焦事件</param>
        /// <param name="onItemMouseEnter">項目滑鼠移入事件</param>
        /// <param name="placeholder">佔位符文字</param>
        /// <param name="isReadOnly">是否唯讀</param>
        /// <returns>設定好的進貨明細選擇欄位定義</returns>
        public static InteractiveColumnDefinition CreateReceivingDetailSelect<TItem>(
            string title = "商品",
            string width = "25%",
            Func<IEnumerable<PurchaseReceivingDetail>>? availableReceivingDetailsProvider = null,
            EventCallback<(TItem item, PurchaseReceivingDetail? selectedDetail)>? onReceivingDetailSelected = null,
            EventCallback<(TItem item, string? searchValue)>? onSearchInputChanged = null,
            EventCallback<TItem>? onInputFocus = null,
            EventCallback<TItem>? onInputBlur = null,
            EventCallback<(TItem item, int index)>? onItemMouseEnter = null,
            string placeholder = "請選擇進貨明細...",
            bool isReadOnly = false)
            where TItem : class
        {
            return SearchableSelectHelper.CreateSearchableSelect<TItem, PurchaseReceivingDetail>(
                title: title,
                width: width,
                searchValuePropertyName: "ReceivingDetailSearch",
                selectedItemPropertyName: "SelectedReceivingDetail",
                filteredItemsPropertyName: "FilteredReceivingDetails",
                showDropdownPropertyName: "ShowDropdown",
                selectedIndexPropertyName: "SelectedIndex",
                availableItemsProvider: availableReceivingDetailsProvider,
                itemDisplayFormatter: detail => FormatPurchaseReceivingDetailDisplay(detail),
                searchFilter: (detail, searchValue) => FilterPurchaseReceivingDetail(detail, searchValue),
                onSearchInputChanged: onSearchInputChanged,
                onItemSelected: onReceivingDetailSelected,
                onInputFocus: onInputFocus,
                onInputBlur: onInputBlur,
                onItemMouseEnter: onItemMouseEnter,
                placeholder: placeholder,
                maxDisplayItems: 20,
                isReadOnly: isReadOnly
            );
        }

        #endregion

        #region 私有方法 - 格式化與過濾邏輯

        /// <summary>
        /// 格式化商品顯示文字
        /// 顯示格式：[商品代碼] 商品名稱
        /// </summary>
        private static string FormatProductDisplay(Product product)
        {
            if (product == null) return "";
            
            var productDisplay = !string.IsNullOrEmpty(product.Code) && !string.IsNullOrEmpty(product.Name)
                ? $"[{product.Code}] {product.Name}"
                : (!string.IsNullOrEmpty(product.Code) ? $"[{product.Code}]" : product.Name ?? "");
            
            return productDisplay;
        }

        /// <summary>
        /// 過濾商品
        /// 支援搜尋：商品代碼、商品名稱
        /// </summary>
        private static bool FilterProduct(Product product, string searchValue)
        {
            if (product == null || string.IsNullOrWhiteSpace(searchValue)) return true;
            
            // 商品代碼搜尋
            var codeMatch = product.Code?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) == true;
            
            // 商品名稱搜尋
            var nameMatch = product.Name?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) == true;
            
            return codeMatch || nameMatch;
        }

        /// <summary>
        /// 過濾泛型商品
        /// 使用格式化函數或反射來檢查商品內容
        /// </summary>
        private static bool FilterGenericProduct<TProduct>(TProduct product, string searchValue, Func<TProduct, string>? displayFormatter)
        {
            if (product == null || string.IsNullOrWhiteSpace(searchValue)) return true;
            
            // 如果有顯示格式化函數，優先使用
            if (displayFormatter != null)
            {
                var displayText = displayFormatter(product);
                return displayText?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) == true;
            }
            
            // 嘗試使用反射檢查 Code 和 Name 屬性
            var productType = product.GetType();
            var codeProperty = productType.GetProperty("Code");
            var nameProperty = productType.GetProperty("Name");
            
            if (codeProperty != null)
            {
                var code = codeProperty.GetValue(product) as string;
                if (code?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) == true)
                    return true;
            }
            
            if (nameProperty != null)
            {
                var name = nameProperty.GetValue(product) as string;
                if (name?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) == true)
                    return true;
            }
            
            // 最後回到字串匹配
            return product.ToString()?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) == true;
        }

        /// <summary>
        /// 格式化銷售訂單明細顯示文字
        /// 顯示格式：銷售單 A1 [產品編號] 產品名稱 (剩餘: X個)
        /// </summary>
        private static string FormatSalesOrderDetailDisplay(SalesOrderDetail detail)
        {
            if (detail?.Product == null || detail.SalesOrder == null) return "";
            
            var product = detail.Product;
            var orderNumber = detail.SalesOrder.SalesOrderNumber ?? "N/A";
            
            var productDisplay = !string.IsNullOrEmpty(product.Code) && !string.IsNullOrEmpty(product.Name)
                ? $"[{product.Code}] {product.Name}"
                : (!string.IsNullOrEmpty(product.Code) ? $"[{product.Code}]" : product.Name);
            
            return $"<span class='text-primary'>[{orderNumber}]</span> {productDisplay}";
        }

        /// <summary>
        /// 過濾銷售訂單明細
        /// 支援搜尋：商品代碼、商品名稱、銷售單號
        /// </summary>
        private static bool FilterSalesOrderDetail(SalesOrderDetail detail, string searchValue)
        {
            if (detail?.Product == null || string.IsNullOrWhiteSpace(searchValue)) return true;
            
            var product = detail.Product;
            var salesOrder = detail.SalesOrder;
            
            // 商品代碼搜尋
            var codeMatch = product.Code?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) == true;
            
            // 商品名稱搜尋
            var nameMatch = product.Name?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) == true;
            
            // 銷售單號搜尋
            var orderMatch = salesOrder?.SalesOrderNumber?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) == true;
            
            return codeMatch || nameMatch || orderMatch;
        }

        /// <summary>
        /// 格式化採購明細顯示文字
        /// 顯示格式：採購單 A1 [產品編號] 產品名稱 (剩餘: X個)
        /// </summary>
        private static string FormatPurchaseOrderDetailDisplay(PurchaseOrderDetail detail)
        {
            if (detail?.Product == null || detail.PurchaseOrder == null) return "";
            
            var product = detail.Product;
            var purchaseOrderNumber = detail.PurchaseOrder.PurchaseOrderNumber ?? "N/A";
            var remaining = detail.OrderQuantity - detail.ReceivedQuantity;
            
            var productDisplay = !string.IsNullOrEmpty(product.Code) && !string.IsNullOrEmpty(product.Name)
                ? $"[{product.Code}] {product.Name}"
                : (!string.IsNullOrEmpty(product.Code) ? $"[{product.Code}]" : product.Name);
            
            return $"<div class='purchase-detail-item'>" +
                   $"<div class='purchase-order-info'><small>採購單 {purchaseOrderNumber}</small></div>" +
                   $"<div class='product-info'><strong>{productDisplay}</strong></div>" +
                   $"</div>";
        }

        /// <summary>
        /// 過濾採購明細
        /// 支援搜尋：商品代碼、商品名稱、採購單號
        /// </summary>
        private static bool FilterPurchaseOrderDetail(PurchaseOrderDetail detail, string searchValue)
        {
            if (detail?.Product == null || string.IsNullOrWhiteSpace(searchValue)) return true;
            
            var product = detail.Product;
            var purchaseOrder = detail.PurchaseOrder;
            
            // 商品代碼搜尋
            var codeMatch = product.Code?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) == true;
            
            // 商品名稱搜尋
            var nameMatch = product.Name?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) == true;
            
            // 採購單號搜尋
            var purchaseOrderMatch = purchaseOrder?.PurchaseOrderNumber?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) == true;
            
            return codeMatch || nameMatch || purchaseOrderMatch;
        }

        /// <summary>
        /// 格式化進貨明細顯示文字
        /// 顯示格式：進貨單 R1 [產品編號] 產品名稱
        /// </summary>
        private static string FormatPurchaseReceivingDetailDisplay(PurchaseReceivingDetail detail)
        {
            if (detail?.Product == null || detail.PurchaseReceiving == null) return "";
            
            var product = detail.Product;
            var receiptNumber = detail.PurchaseReceiving.ReceiptNumber ?? "N/A";
            
            var productDisplay = !string.IsNullOrEmpty(product.Code) && !string.IsNullOrEmpty(product.Name)
                ? $"[{product.Code}] {product.Name}"
                : (!string.IsNullOrEmpty(product.Code) ? $"[{product.Code}]" : product.Name);
            
            return $"<span class='text-info'>[{receiptNumber}]</span> {productDisplay}";
        }

        /// <summary>
        /// 過濾進貨明細
        /// 支援搜尋：商品代碼、商品名稱、進貨單號
        /// </summary>
        private static bool FilterPurchaseReceivingDetail(PurchaseReceivingDetail detail, string searchValue)
        {
            if (detail?.Product == null || string.IsNullOrWhiteSpace(searchValue)) return true;
            
            var product = detail.Product;
            var purchaseReceiving = detail.PurchaseReceiving;
            
            // 商品代碼搜尋
            var codeMatch = product.Code?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) == true;
            
            // 商品名稱搜尋
            var nameMatch = product.Name?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) == true;
            
            // 進貨單號搜尋
            var receiptMatch = purchaseReceiving?.ReceiptNumber?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) == true;
            
            return codeMatch || nameMatch || receiptMatch;
        }

        #endregion
    }
}