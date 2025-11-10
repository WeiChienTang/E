using ERPCore2.Data.Entities;
using Microsoft.AspNetCore.Components;
using ERPCore2.Components.Shared.SubCollections;

namespace ERPCore2.Helpers
{
    /// <summary>
    /// 採購訂單明細可搜尋下拉選單輔助類別
    /// 專門處理採購明細選擇，包含採購單資訊顯示
    /// </summary>
    public static class PurchaseOrderDetailSelectHelper
    {
        /// <summary>
        /// 建立採購明細搜尋下拉選單欄位定義
        /// </summary>
        /// <typeparam name="TItem">項目類型</typeparam>
        /// <param name="title">欄位標題</param>
        /// <param name="availablePurchaseDetailsProvider">可用採購明細提供者</param>
        /// <param name="onSearchInputChanged">搜尋輸入變更事件</param>
        /// <param name="onPurchaseDetailSelected">採購明細選擇事件</param>
        /// <param name="onInputFocus">輸入框焦點事件</param>
        /// <param name="onInputBlur">輸入框失焦事件</param>
        /// <param name="onItemMouseEnter">項目滑鼠移入事件</param>
        /// <param name="isReadOnly">是否唯讀</param>
        /// <returns>設定好的採購明細搜尋欄位定義</returns>
        public static InteractiveColumnDefinition CreatePurchaseDetailSearchableSelect<TItem>(
            string title = "採購明細",
            Func<IEnumerable<PurchaseOrderDetail>>? availablePurchaseDetailsProvider = null,
            EventCallback<(TItem item, string? searchValue)>? onSearchInputChanged = null,
            EventCallback<(TItem item, PurchaseOrderDetail? selectedDetail)>? onPurchaseDetailSelected = null,
            EventCallback<TItem>? onInputFocus = null,
            EventCallback<TItem>? onInputBlur = null,
            EventCallback<(TItem item, int index)>? onItemMouseEnter = null,
            bool isReadOnly = false)
            where TItem : class
        {
            return SearchableSelectHelper.CreateSearchableSelect<TItem, PurchaseOrderDetail>(
                title: title,
                width: "25%",
                searchValuePropertyName: "ProductSearch", // 沿用現有的搜尋欄位名稱
                selectedItemPropertyName: "SelectedPurchaseDetail", // 新的選中項目屬性
                filteredItemsPropertyName: "FilteredPurchaseDetails", // 新的過濾清單屬性
                showDropdownPropertyName: "ShowDropdown",
                selectedIndexPropertyName: "SelectedIndex",
                availableItemsProvider: availablePurchaseDetailsProvider,
                itemDisplayFormatter: detail => FormatPurchaseDetailDisplay(detail),
                searchFilter: (detail, searchValue) => FilterPurchaseDetail(detail, searchValue),
                onSearchInputChanged: onSearchInputChanged,
                onItemSelected: onPurchaseDetailSelected,
                onInputFocus: onInputFocus,
                onInputBlur: onInputBlur,
                onItemMouseEnter: onItemMouseEnter,
                placeholder: "請選擇採購明細...",
                maxDisplayItems: 20,
                isReadOnly: isReadOnly
            );
        }

        /// <summary>
        /// 格式化採購明細顯示文字
        /// 顯示格式：採購單 A1 [產品編號] 產品名稱 (剩餘: X個)
        /// </summary>
        private static string FormatPurchaseDetailDisplay(PurchaseOrderDetail detail)
        {
            if (detail?.Product == null || detail.PurchaseOrder == null) return "";
            
            var product = detail.Product;
            var purchaseOrderNumber = detail.PurchaseOrder.Code ?? "N/A";
            var remaining = detail.OrderQuantity - detail.ReceivedQuantity;
            
            var productDisplay = !string.IsNullOrEmpty(product.Code) && !string.IsNullOrEmpty(product.Name)
                ? $"[{product.Code}] {product.Name}"
                : (!string.IsNullOrEmpty(product.Code) ? $"[{product.Code}]" : product.Name);
            
            return $"<div class='purchase-detail-item'>" +
                   $"<div class='purchase-order-info'><small>採購單 {purchaseOrderNumber}</small></div>" +
                   $"<div class='product-info'><strong>{productDisplay}</strong></div>";
        }

        /// <summary>
        /// 過濾採購明細
        /// 支援搜尋：商品代碼、商品名稱、採購單號
        /// </summary>
        private static bool FilterPurchaseDetail(PurchaseOrderDetail detail, string searchValue)
        {
            if (detail?.Product == null || string.IsNullOrWhiteSpace(searchValue)) return true;
            
            var product = detail.Product;
            var purchaseOrder = detail.PurchaseOrder;
            
            // 商品代碼搜尋
            var codeMatch = product.Code?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) == true;
            
            // 商品名稱搜尋
            var nameMatch = product.Name?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) == true;
            
            // 採購單號搜尋
            var purchaseOrderMatch = purchaseOrder?.Code?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) == true;
            
            return codeMatch || nameMatch || purchaseOrderMatch;
        }
    }
}