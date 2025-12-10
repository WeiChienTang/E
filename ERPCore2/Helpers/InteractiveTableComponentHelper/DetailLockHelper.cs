namespace ERPCore2.Helpers.InteractiveTableComponentHelper
{
    /// <summary>
    /// 明細鎖定檢查輔助類
    /// 用於檢查明細項目是否因為有相關記錄而無法刪除或修改
    /// 
    /// 核心原則：「只要該明細有下一步，就需要鎖起來」
    /// 
    /// 使用場景：
    /// - 採購單明細：已有進貨記錄 → 鎖定
    /// - 採購進貨明細：已有退貨或沖款記錄 → 鎖定
    /// - 採購退回明細：已有沖款記錄 → 鎖定
    /// - 報價單明細：已轉為銷貨單 → 鎖定
    /// - 銷貨訂單明細：已有退貨或沖款記錄 → 鎖定
    /// - 銷貨退回明細：已有沖款記錄 → 鎖定
    /// </summary>
    public static class DetailLockHelper
    {
        /// <summary>
        /// 檢查實體是否有付款/收款記錄
        /// 支援的屬性名稱: TotalPaidAmount (應付款), TotalReceivedAmount (應收款)
        /// </summary>
        /// <typeparam name="TEntity">實體類型</typeparam>
        /// <param name="entity">要檢查的實體</param>
        /// <returns>true 表示有付款/收款記錄，false 表示沒有</returns>
        public static bool HasPaymentRecord<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity == null) return false;
            
            var type = entity.GetType();
            
            // 檢查 TotalPaidAmount (應付款)
            var paidProperty = type.GetProperty("TotalPaidAmount");
            if (paidProperty != null && paidProperty.PropertyType == typeof(decimal))
            {
                var value = (decimal)(paidProperty.GetValue(entity) ?? 0m);
                if (value > 0) return true;
            }
            
            // 檢查 TotalReceivedAmount (應收款)
            var receivedProperty = type.GetProperty("TotalReceivedAmount");
            if (receivedProperty != null && receivedProperty.PropertyType == typeof(decimal))
            {
                var value = (decimal)(receivedProperty.GetValue(entity) ?? 0m);
                if (value > 0) return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 取得付款/收款金額
        /// </summary>
        /// <typeparam name="TEntity">實體類型</typeparam>
        /// <param name="entity">要檢查的實體</param>
        /// <returns>付款或收款金額，如果兩者都存在則返回較大值</returns>
        public static decimal GetPaymentAmount<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity == null) return 0;
            
            var type = entity.GetType();
            decimal amount = 0;
            
            // 檢查 TotalPaidAmount
            var paidProperty = type.GetProperty("TotalPaidAmount");
            if (paidProperty != null && paidProperty.PropertyType == typeof(decimal))
            {
                amount = Math.Max(amount, (decimal)(paidProperty.GetValue(entity) ?? 0m));
            }
            
            // 檢查 TotalReceivedAmount
            var receivedProperty = type.GetProperty("TotalReceivedAmount");
            if (receivedProperty != null && receivedProperty.PropertyType == typeof(decimal))
            {
                amount = Math.Max(amount, (decimal)(receivedProperty.GetValue(entity) ?? 0m));
            }
            
            return amount;
        }
        
        /// <summary>
        /// 檢查實體是否有退貨記錄 (透過外部字典)
        /// </summary>
        /// <typeparam name="TEntity">實體類型</typeparam>
        /// <param name="entity">要檢查的實體</param>
        /// <param name="returnedQuantities">已退貨數量字典 (Key: DetailId, Value: 已退貨數量)</param>
        /// <returns>true 表示有退貨記錄，false 表示沒有</returns>
        public static bool HasReturnRecord<TEntity>(
            TEntity entity, 
            Dictionary<int, decimal>? returnedQuantities) where TEntity : class
        {
            if (entity == null || returnedQuantities == null || !returnedQuantities.Any()) 
                return false;
            
            var idProperty = entity.GetType().GetProperty("Id");
            if (idProperty != null && idProperty.PropertyType == typeof(int))
            {
                var id = (int)(idProperty.GetValue(entity) ?? 0);
                return id > 0 && returnedQuantities.ContainsKey(id);
            }
            
            return false;
        }
        
        /// <summary>
        /// 取得退貨數量
        /// </summary>
        /// <typeparam name="TEntity">實體類型</typeparam>
        /// <param name="entity">要檢查的實體</param>
        /// <param name="returnedQuantities">已退貨數量字典</param>
        /// <returns>已退貨數量</returns>
        public static decimal GetReturnedQuantity<TEntity>(
            TEntity entity, 
            Dictionary<int, decimal>? returnedQuantities) where TEntity : class
        {
            if (entity == null || returnedQuantities == null) return 0;
            
            var idProperty = entity.GetType().GetProperty("Id");
            if (idProperty != null && idProperty.PropertyType == typeof(int))
            {
                var id = (int)(idProperty.GetValue(entity) ?? 0);
                return id > 0 && returnedQuantities.TryGetValue(id, out var qty) ? qty : 0;
            }
            
            return 0;
        }
        
        /// <summary>
        /// 檢查是否有轉單記錄
        /// 支援的屬性名稱: ConvertedQuantity (已轉單數量)
        /// </summary>
        /// <typeparam name="TEntity">實體類型</typeparam>
        /// <param name="entity">要檢查的實體</param>
        /// <returns>true 表示有轉單記錄，false 表示沒有</returns>
        public static bool HasConversionRecord<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity == null) return false;
            
            var type = entity.GetType();
            
            // 檢查 ConvertedQuantity
            var convertedProperty = type.GetProperty("ConvertedQuantity");
            if (convertedProperty != null && convertedProperty.PropertyType == typeof(decimal))
            {
                var value = (decimal)(convertedProperty.GetValue(entity) ?? 0m);
                return value > 0;
            }
            
            return false;
        }
        
        /// <summary>
        /// 取得已轉單數量
        /// </summary>
        /// <typeparam name="TEntity">實體類型</typeparam>
        /// <param name="entity">要檢查的實體</param>
        /// <returns>已轉單數量</returns>
        public static decimal GetConvertedQuantity<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity == null) return 0;
            
            var type = entity.GetType();
            
            var convertedProperty = type.GetProperty("ConvertedQuantity");
            if (convertedProperty != null && convertedProperty.PropertyType == typeof(decimal))
            {
                return (decimal)(convertedProperty.GetValue(entity) ?? 0m);
            }
            
            return 0;
        }
        
        /// <summary>
        /// 檢查是否有進貨記錄（採購單專用）
        /// 支援的屬性名稱: ReceivedQuantity (已入庫數量)
        /// </summary>
        /// <typeparam name="TEntity">實體類型</typeparam>
        /// <param name="entity">要檢查的實體</param>
        /// <returns>true 表示有進貨記錄，false 表示沒有</returns>
        public static bool HasReceivingRecord<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity == null) return false;
            
            var type = entity.GetType();
            
            // 檢查 ReceivedQuantity
            var receivedProperty = type.GetProperty("ReceivedQuantity");
            if (receivedProperty != null)
            {
                var propertyType = receivedProperty.PropertyType;
                
                // 支援 int 和 decimal 類型
                if (propertyType == typeof(int))
                {
                    var value = (int)(receivedProperty.GetValue(entity) ?? 0);
                    return value > 0;
                }
                else if (propertyType == typeof(decimal))
                {
                    var value = (decimal)(receivedProperty.GetValue(entity) ?? 0m);
                    return value > 0;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 取得已入庫數量
        /// </summary>
        /// <typeparam name="TEntity">實體類型</typeparam>
        /// <param name="entity">要檢查的實體</param>
        /// <returns>已入庫數量</returns>
        public static decimal GetReceivedQuantity<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity == null) return 0;
            
            var type = entity.GetType();
            
            var receivedProperty = type.GetProperty("ReceivedQuantity");
            if (receivedProperty != null)
            {
                var propertyType = receivedProperty.PropertyType;
                
                if (propertyType == typeof(int))
                {
                    return (int)(receivedProperty.GetValue(entity) ?? 0);
                }
                else if (propertyType == typeof(decimal))
                {
                    return (decimal)(receivedProperty.GetValue(entity) ?? 0m);
                }
            }
            
            return 0;
        }
        
        /// <summary>
        /// 綜合檢查項目是否可以刪除（單一檢查點版本）
        /// </summary>
        /// <typeparam name="TEntity">實體類型</typeparam>
        /// <param name="entity">要檢查的實體</param>
        /// <param name="reason">不可刪除的原因（輸出參數）</param>
        /// <param name="checkPayment">是否檢查付款/收款記錄</param>
        /// <param name="checkReturn">是否檢查退貨記錄</param>
        /// <param name="checkConversion">是否檢查轉單記錄</param>
        /// <param name="checkReceiving">是否檢查進貨記錄</param>
        /// <param name="checkDelivery">是否檢查出貨記錄（DeliveredQuantity）</param>
        /// <param name="checkSchedule">是否檢查排程記錄（ScheduledQuantity）</param>
        /// <param name="returnedQuantities">已退貨數量字典（當 checkReturn = true 時需要提供）</param>
        /// <returns>true 表示可以刪除，false 表示不可刪除</returns>
        public static bool CanDeleteItem<TEntity>(
            TEntity? entity,
            out string reason,
            bool checkPayment = false,
            bool checkReturn = false,
            bool checkConversion = false,
            bool checkReceiving = false,
            bool checkDelivery = false,
            bool checkSchedule = false,
            Dictionary<int, decimal>? returnedQuantities = null) where TEntity : class
        {
            reason = string.Empty;
            
            if (entity == null) return true;
            
            // 檢查退貨記錄
            if (checkReturn && HasReturnRecord(entity, returnedQuantities))
            {
                var returnedQty = GetReturnedQuantity(entity, returnedQuantities);
                reason = $"此商品已有退貨記錄（已退貨 {returnedQty} 個），無法刪除";
                return false;
            }
            
            // 檢查沖款記錄
            if (checkPayment && HasPaymentRecord(entity))
            {
                var amount = GetPaymentAmount(entity);
                
                // 判斷是付款還是收款
                var type = entity.GetType();
                var hasPaid = type.GetProperty("TotalPaidAmount") != null;
                var hasReceived = type.GetProperty("TotalReceivedAmount") != null;
                
                if (hasPaid && hasReceived)
                {
                    reason = $"此商品已有沖款記錄（金額 {amount:N0} 元），無法刪除";
                }
                else if (hasPaid)
                {
                    reason = $"此商品已有沖款記錄（已付款 {amount:N0} 元），無法刪除";
                }
                else if (hasReceived)
                {
                    reason = $"此商品已有沖款記錄（已收款 {amount:N0} 元），無法刪除";
                }
                
                return false;
            }
            
            // 檢查轉單記錄
            if (checkConversion && HasConversionRecord(entity))
            {
                var convertedQty = GetConvertedQuantity(entity);
                reason = $"此商品已轉單（已轉 {convertedQty} 個），無法刪除";
                return false;
            }
            
            // 檢查進貨記錄
            if (checkReceiving && HasReceivingRecord(entity))
            {
                var receivedQty = GetReceivedQuantity(entity);
                reason = $"此商品已有進貨記錄（已入庫 {receivedQty} 個），無法刪除";
                return false;
            }
            
            // 檢查出貨記錄（DeliveredQuantity）
            if (checkDelivery && HasDeliveryRecord(entity))
            {
                var deliveredQty = GetDeliveredQuantity(entity);
                reason = $"此商品已有出貨記錄（已出貨 {deliveredQty} 個），無法刪除";
                return false;
            }
            
            // 檢查排程記錄（ScheduledQuantity）
            if (checkSchedule && HasScheduleRecord(entity))
            {
                var scheduledQty = GetScheduledQuantity(entity);
                reason = $"此商品已有排程記錄（已排程 {scheduledQty} 個），無法刪除";
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 檢查是否有出貨記錄（檢查 DeliveredQuantity 欄位）
        /// </summary>
        private static bool HasDeliveryRecord<TEntity>(TEntity entity) where TEntity : class
        {
            var type = entity.GetType();
            var deliveredProperty = type.GetProperty("DeliveredQuantity");
            
            if (deliveredProperty != null && deliveredProperty.PropertyType == typeof(decimal))
            {
                var value = (decimal)(deliveredProperty.GetValue(entity) ?? 0m);
                return value > 0;
            }
            
            return false;
        }
        
        /// <summary>
        /// 取得已出貨數量
        /// </summary>
        private static decimal GetDeliveredQuantity<TEntity>(TEntity entity) where TEntity : class
        {
            var type = entity.GetType();
            var deliveredProperty = type.GetProperty("DeliveredQuantity");
            
            if (deliveredProperty != null && deliveredProperty.PropertyType == typeof(decimal))
            {
                return (decimal)(deliveredProperty.GetValue(entity) ?? 0m);
            }
            
            return 0;
        }
        
        /// <summary>
        /// 檢查是否有排程記錄（檢查 ScheduledQuantity 欄位）
        /// </summary>
        private static bool HasScheduleRecord<TEntity>(TEntity entity) where TEntity : class
        {
            var type = entity.GetType();
            var scheduledProperty = type.GetProperty("ScheduledQuantity");
            
            if (scheduledProperty != null && scheduledProperty.PropertyType == typeof(decimal))
            {
                var value = (decimal)(scheduledProperty.GetValue(entity) ?? 0m);
                return value > 0;
            }
            
            return false;
        }
        
        /// <summary>
        /// 取得已排程數量
        /// </summary>
        private static decimal GetScheduledQuantity<TEntity>(TEntity entity) where TEntity : class
        {
            var type = entity.GetType();
            var scheduledProperty = type.GetProperty("ScheduledQuantity");
            
            if (scheduledProperty != null && scheduledProperty.PropertyType == typeof(decimal))
            {
                return (decimal)(scheduledProperty.GetValue(entity) ?? 0m);
            }
            
            return 0;
        }
    }
}
