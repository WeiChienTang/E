using ERPCore2.Components.Shared.Forms;

namespace ERPCore2.Helpers.EditModal;

/// <summary>
/// 表單欄位鎖定輔助工具
/// 提供統一的欄位鎖定/解鎖邏輯，避免在各個編輯 Modal 中重複相同的程式碼
/// 
/// 使用場景：
/// - 當明細有其他動作（退貨、沖款等）時，需要鎖定主檔欄位
/// - 當單據已審核通過時，需要鎖定主檔欄位
/// - 當單據已轉單時，需要鎖定主檔欄位
/// </summary>
public static class FormFieldLockHelper
{
    /// <summary>
    /// 鎖定或解鎖單一欄位
    /// </summary>
    /// <param name="formFields">表單欄位清單</param>
    /// <param name="propertyName">欄位屬性名稱</param>
    /// <param name="isLocked">是否鎖定（true=鎖定，false=解鎖）</param>
    /// <param name="actionButtonsGetter">取得操作按鈕的非同步方法（解鎖時使用）</param>
    /// <returns>是否成功找到並更新欄位</returns>
    public static bool LockField(
        List<FormFieldDefinition> formFields,
        string propertyName,
        bool isLocked,
        Func<Task<List<FieldActionButton>>>? actionButtonsGetter = null)
    {
        var field = formFields.FirstOrDefault(f => f.PropertyName == propertyName);
        if (field == null)
            return false;

        field.IsReadOnly = isLocked;

        // 處理操作按鈕
        if (isLocked)
        {
            // 鎖定時移除所有操作按鈕
            field.ActionButtons = new List<FieldActionButton>();
        }
        else if (actionButtonsGetter != null)
        {
            // 解鎖時恢復操作按鈕（如果有提供 getter）
            // 注意：這裡使用 .Result 是因為 Blazor 組件中的同步方法需要
            field.ActionButtons = actionButtonsGetter().Result;
        }

        return true;
    }

    /// <summary>
    /// 鎖定或解鎖多個欄位
    /// </summary>
    /// <param name="formFields">表單欄位清單</param>
    /// <param name="propertyNames">欄位屬性名稱清單</param>
    /// <param name="isLocked">是否鎖定（true=鎖定，false=解鎖）</param>
    /// <param name="actionButtonsMap">欄位名稱到操作按鈕 getter 的對應表（解鎖時使用）</param>
    /// <returns>成功更新的欄位數量</returns>
    public static int LockMultipleFields(
        List<FormFieldDefinition> formFields,
        IEnumerable<string> propertyNames,
        bool isLocked,
        Dictionary<string, Func<Task<List<FieldActionButton>>>>? actionButtonsMap = null)
    {
        int count = 0;

        foreach (var propertyName in propertyNames)
        {
            // 嘗試從對應表中取得該欄位的 ActionButtons getter
            Func<Task<List<FieldActionButton>>>? getter = null;
            if (actionButtonsMap != null && actionButtonsMap.ContainsKey(propertyName))
            {
                getter = actionButtonsMap[propertyName];
            }

            if (LockField(formFields, propertyName, isLocked, getter))
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// 鎖定或解鎖欄位（同步版本，適用於沒有 ActionButtons 的場景）
    /// </summary>
    /// <param name="formFields">表單欄位清單</param>
    /// <param name="propertyName">欄位屬性名稱</param>
    /// <param name="isLocked">是否鎖定</param>
    /// <returns>是否成功找到並更新欄位</returns>
    public static bool LockFieldSimple(
        List<FormFieldDefinition> formFields,
        string propertyName,
        bool isLocked)
    {
        var field = formFields.FirstOrDefault(f => f.PropertyName == propertyName);
        if (field == null)
            return false;

        field.IsReadOnly = isLocked;

        // 簡化版本：鎖定時總是清空 ActionButtons
        if (isLocked)
        {
            field.ActionButtons = new List<FieldActionButton>();
        }

        return true;
    }

    /// <summary>
    /// 批次鎖定多個欄位（簡化版本，不處理 ActionButtons 恢復）
    /// </summary>
    /// <param name="formFields">表單欄位清單</param>
    /// <param name="propertyNames">欄位屬性名稱清單</param>
    /// <param name="isLocked">是否鎖定</param>
    /// <returns>成功更新的欄位數量</returns>
    public static int LockMultipleFieldsSimple(
        List<FormFieldDefinition> formFields,
        IEnumerable<string> propertyNames,
        bool isLocked)
    {
        int count = 0;

        foreach (var propertyName in propertyNames)
        {
            if (LockFieldSimple(formFields, propertyName, isLocked))
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// 建立欄位鎖定配置（建構器模式）
    /// 用於更複雜的鎖定場景
    /// </summary>
    public class FieldLockConfiguration
    {
        /// <summary>欄位屬性名稱</summary>
        public string PropertyName { get; set; } = string.Empty;

        /// <summary>是否鎖定</summary>
        public bool IsLocked { get; set; }

        /// <summary>操作按鈕取得方法（選用）</summary>
        public Func<Task<List<FieldActionButton>>>? ActionButtonsGetter { get; set; }

        /// <summary>是否保留現有 ActionButtons（預設 false，鎖定時會清空）</summary>
        public bool KeepExistingActionButtons { get; set; } = false;
    }

    /// <summary>
    /// 使用配置批次鎖定欄位（進階版本）
    /// </summary>
    /// <param name="formFields">表單欄位清單</param>
    /// <param name="configurations">欄位鎖定配置清單</param>
    /// <returns>成功更新的欄位數量</returns>
    public static int LockFieldsWithConfiguration(
        List<FormFieldDefinition> formFields,
        IEnumerable<FieldLockConfiguration> configurations)
    {
        int count = 0;

        foreach (var config in configurations)
        {
            var field = formFields.FirstOrDefault(f => f.PropertyName == config.PropertyName);
            if (field == null)
                continue;

            field.IsReadOnly = config.IsLocked;

            // 處理操作按鈕
            if (config.IsLocked && !config.KeepExistingActionButtons)
            {
                field.ActionButtons = new List<FieldActionButton>();
            }
            else if (!config.IsLocked && config.ActionButtonsGetter != null)
            {
                field.ActionButtons = config.ActionButtonsGetter().Result;
            }

            count++;
        }

        return count;
    }

    /// <summary>
    /// 快速建立標準的主檔欄位鎖定配置
    /// 適用於常見的進銷存單據場景
    /// </summary>
    /// <param name="isLocked">是否鎖定</param>
    /// <param name="actionButtonsMap">欄位名稱到操作按鈕 getter 的對應表（選用）</param>
    /// <returns>標準欄位鎖定配置清單</returns>
    public static List<FieldLockConfiguration> CreateStandardMainEntityLockConfig(
        bool isLocked,
        Dictionary<string, Func<Task<List<FieldActionButton>>>>? actionButtonsMap = null)
    {
        var configs = new List<FieldLockConfiguration>();

        // 常見的主檔欄位（這些欄位通常在有不可刪除明細時需要鎖定）
        var standardFields = new[]
        {
            "CustomerId",           // 客戶
            "SupplierId",           // 廠商
            "EmployeeId",           // 業務員/經辦人
            "CompanyId",            // 公司
            "OrderDate",            // 訂單日期
            "ReceiptDate",          // 進貨日期
            "ReturnDate",           // 退貨日期
            "ExpectedDeliveryDate", // 預定出貨日期
            "PaymentTerms",         // 付款條件
            "DeliveryTerms",        // 交貨條件
            "FilterProductId",      // 篩選產品
            "SalesOrderId",         // 銷售單
            "PurchaseOrderId",      // 採購單
            "ReturnReasonId"        // 退貨原因
        };

        foreach (var fieldName in standardFields)
        {
            var config = new FieldLockConfiguration
            {
                PropertyName = fieldName,
                IsLocked = isLocked
            };

            // 如果有提供 ActionButtons getter，則加入
            if (actionButtonsMap != null && actionButtonsMap.ContainsKey(fieldName))
            {
                config.ActionButtonsGetter = actionButtonsMap[fieldName];
            }

            configs.Add(config);
        }

        return configs;
    }

    /// <summary>
    /// 檢查欄位是否被鎖定
    /// </summary>
    /// <param name="formFields">表單欄位清單</param>
    /// <param name="propertyName">欄位屬性名稱</param>
    /// <returns>是否被鎖定（如果找不到欄位，回傳 null）</returns>
    public static bool? IsFieldLocked(List<FormFieldDefinition> formFields, string propertyName)
    {
        var field = formFields.FirstOrDefault(f => f.PropertyName == propertyName);
        return field?.IsReadOnly;
    }

    /// <summary>
    /// 取得所有被鎖定的欄位名稱
    /// </summary>
    /// <param name="formFields">表單欄位清單</param>
    /// <returns>被鎖定的欄位屬性名稱清單</returns>
    public static List<string> GetLockedFields(List<FormFieldDefinition> formFields)
    {
        return formFields
            .Where(f => f.IsReadOnly)
            .Select(f => f.PropertyName)
            .ToList();
    }
}
