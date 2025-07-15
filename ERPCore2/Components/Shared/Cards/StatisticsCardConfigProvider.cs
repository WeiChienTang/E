using ERPCore2.Components.Shared.Cards;
using Microsoft.AspNetCore.Components;

namespace ERPCore2.Components.Shared.Cards
{
    /// <summary>
    /// 統計卡片預設配置提供者 - 提供常用的卡片配置組合
    /// </summary>
    public static class StatisticsCardConfigProvider
    {
        /// <summary>
        /// 庫存管理統計卡片配置
        /// </summary>
        public static List<GenericStatisticsCards.StatisticsCardConfig> InventoryConfigs(
            EventCallback? onTotalProductsClick = null,
            EventCallback? onTotalValueClick = null,
            EventCallback? onLowStockClick = null,
            EventCallback? onZeroStockClick = null) => new()
        {
            new()
            {
                Title = "總商品數量",
                DataKey = "TotalProducts",
                IconClass = "fas fa-boxes",
                BorderColor = "primary",
                TextColor = "primary",
                OnClick = onTotalProductsClick ?? default
            },
            new()
            {
                Title = "總庫存價值",
                DataKey = "TotalInventoryValue",
                IconClass = "fas fa-dollar-sign",
                BorderColor = "success",
                TextColor = "success",
                IsCurrency = true,
                OnClick = onTotalValueClick ?? default
            },
            new()
            {
                Title = "低庫存警戒",
                DataKey = "LowStockCount",
                IconClass = "fas fa-exclamation-triangle",
                BorderColor = "warning",
                TextColor = "warning",
                OnClick = onLowStockClick ?? default
            },
            new()
            {
                Title = "零庫存商品",
                DataKey = "ZeroStockCount",
                IconClass = "fas fa-ban",
                BorderColor = "danger",
                TextColor = "danger",
                OnClick = onZeroStockClick ?? default
            }
        };

        /// <summary>
        /// 採購訂單統計卡片配置
        /// </summary>
        public static List<GenericStatisticsCards.StatisticsCardConfig> PurchaseOrderConfigs(
            EventCallback? onTotalOrdersClick = null,
            EventCallback? onPendingOrdersClick = null,
            EventCallback? onCompletedOrdersClick = null,
            EventCallback? onTotalAmountClick = null) => new()
        {
            new()
            {
                Title = "總訂單數",
                DataKey = "TotalOrders",
                IconClass = "fas fa-file-invoice",
                BorderColor = "primary",
                TextColor = "primary",
                OnClick = onTotalOrdersClick ?? default
            },
            new()
            {
                Title = "待進貨訂單",
                DataKey = "PendingOrders",
                IconClass = "fas fa-clock",
                BorderColor = "warning",
                TextColor = "warning",
                OnClick = onPendingOrdersClick ?? default
            },
            new()
            {
                Title = "已完成訂單",
                DataKey = "CompletedOrders",
                IconClass = "fas fa-check-circle",
                BorderColor = "success",
                TextColor = "success",
                OnClick = onCompletedOrdersClick ?? default
            },
            new()
            {
                Title = "訂單總金額",
                DataKey = "TotalAmount",
                IconClass = "fas fa-dollar-sign",
                BorderColor = "info",
                TextColor = "info",
                IsCurrency = true,
                OnClick = onTotalAmountClick ?? default
            }
        };

        /// <summary>
        /// 進貨單統計卡片配置
        /// </summary>
        public static List<GenericStatisticsCards.StatisticsCardConfig> PurchaseReceiptConfigs(
            EventCallback? onTotalReceiptsClick = null,
            EventCallback? onPendingReceiptsClick = null,
            EventCallback? onCompletedReceiptsClick = null,
            EventCallback? onTotalAmountClick = null) => new()
        {
            new()
            {
                Title = "總進貨單數",
                DataKey = "TotalReceipts",
                IconClass = "fas fa-file-invoice",
                BorderColor = "primary",
                TextColor = "primary",
                OnClick = onTotalReceiptsClick ?? default
            },
            new()
            {
                Title = "待驗收進貨",
                DataKey = "PendingReceipts",
                IconClass = "fas fa-clock",
                BorderColor = "warning",
                TextColor = "warning",
                OnClick = onPendingReceiptsClick ?? default
            },
            new()
            {
                Title = "已完成進貨",
                DataKey = "CompletedReceipts",
                IconClass = "fas fa-check-circle",
                BorderColor = "success",
                TextColor = "success",
                OnClick = onCompletedReceiptsClick ?? default
            },
            new()
            {
                Title = "進貨總金額",
                DataKey = "TotalAmount",
                IconClass = "fas fa-dollar-sign",
                BorderColor = "info",
                TextColor = "info",
                IsCurrency = true,
                OnClick = onTotalAmountClick ?? default
            }
        };

        /// <summary>
        /// 盤點統計卡片配置
        /// </summary>
        public static List<GenericStatisticsCards.StatisticsCardConfig> StockTakingConfigs(
            EventCallback? onTotalStockTakingClick = null,
            EventCallback? onInProgressClick = null,
            EventCallback? onCompletedClick = null,
            EventCallback? onVarianceClick = null) => new()
        {
            new()
            {
                Title = "總盤點數",
                DataKey = "TotalStockTaking",
                IconClass = "fas fa-clipboard-list",
                BorderColor = "primary",
                TextColor = "primary",
                OnClick = onTotalStockTakingClick ?? default
            },
            new()
            {
                Title = "進行中盤點",
                DataKey = "InProgressCount",
                IconClass = "fas fa-spinner",
                BorderColor = "warning",
                TextColor = "warning",
                OnClick = onInProgressClick ?? default
            },
            new()
            {
                Title = "已完成盤點",
                DataKey = "CompletedCount",
                IconClass = "fas fa-check-circle",
                BorderColor = "success",
                TextColor = "success",
                OnClick = onCompletedClick ?? default
            },
            new()
            {
                Title = "盤點差異數",
                DataKey = "VarianceCount",
                IconClass = "fas fa-exclamation-triangle",
                BorderColor = "danger",
                TextColor = "danger",
                OnClick = onVarianceClick ?? default
            }
        };

        /// <summary>
        /// 庫存異動統計卡片配置
        /// </summary>
        public static List<GenericStatisticsCards.StatisticsCardConfig> InventoryTransactionConfigs(
            EventCallback? onTotalTransactionsClick = null,
            EventCallback? onInTransactionsClick = null,
            EventCallback? onOutTransactionsClick = null,
            EventCallback? onAdjustmentTransactionsClick = null) => new()
        {
            new()
            {
                Title = "總異動筆數",
                DataKey = "TotalTransactions",
                IconClass = "fas fa-exchange-alt",
                BorderColor = "primary",
                TextColor = "primary",
                OnClick = onTotalTransactionsClick ?? default
            },
            new()
            {
                Title = "入庫異動",
                DataKey = "InTransactions",
                IconClass = "fas fa-arrow-down",
                BorderColor = "success",
                TextColor = "success",
                OnClick = onInTransactionsClick ?? default
            },
            new()
            {
                Title = "出庫異動",
                DataKey = "OutTransactions",
                IconClass = "fas fa-arrow-up",
                BorderColor = "warning",
                TextColor = "warning",
                OnClick = onOutTransactionsClick ?? default
            },
            new()
            {
                Title = "調整異動",
                DataKey = "AdjustmentTransactions",
                IconClass = "fas fa-edit",
                BorderColor = "info",
                TextColor = "info",
                OnClick = onAdjustmentTransactionsClick ?? default
            }
        };

        /// <summary>
        /// 通用財務統計卡片配置
        /// </summary>
        public static List<GenericStatisticsCards.StatisticsCardConfig> FinancialConfigs(
            string totalCountKey = "TotalCount",
            string totalAmountKey = "TotalAmount",
            string avgAmountKey = "AverageAmount",
            string maxAmountKey = "MaxAmount",
            EventCallback? onTotalCountClick = null,
            EventCallback? onTotalAmountClick = null,
            EventCallback? onAvgAmountClick = null,
            EventCallback? onMaxAmountClick = null) => new()
        {
            new()
            {
                Title = "總筆數",
                DataKey = totalCountKey,
                IconClass = "fas fa-list-ol",
                BorderColor = "primary",
                TextColor = "primary",
                OnClick = onTotalCountClick ?? default
            },
            new()
            {
                Title = "總金額",
                DataKey = totalAmountKey,
                IconClass = "fas fa-dollar-sign",
                BorderColor = "success",
                TextColor = "success",
                IsCurrency = true,
                OnClick = onTotalAmountClick ?? default
            },
            new()
            {
                Title = "平均金額",
                DataKey = avgAmountKey,
                IconClass = "fas fa-chart-line",
                BorderColor = "info",
                TextColor = "info",
                IsCurrency = true,
                OnClick = onAvgAmountClick ?? default
            },
            new()
            {
                Title = "最高金額",
                DataKey = maxAmountKey,
                IconClass = "fas fa-crown",
                BorderColor = "warning",
                TextColor = "warning",
                IsCurrency = true,
                OnClick = onMaxAmountClick ?? default
            }
        };
    }
}
