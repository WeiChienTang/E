using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ERPCore2.Helpers.EditModal
{
    /// <summary>
    /// 表單區段定義生成器 - 簡化 formSections 的建立
    /// 提供流暢API來定義表單欄位的區段歸屬
    /// </summary>
    /// <typeparam name="TEntity">實體類型</typeparam>
    public class FormSectionHelper<TEntity> where TEntity : class
    {
        private readonly Dictionary<string, string> _sections = new();

        /// <summary>
        /// 私有建構子 - 使用靜態工廠方法建立實例
        /// </summary>
        private FormSectionHelper()
        {
        }

        /// <summary>
        /// 建立新的 FormSectionHelper 實例
        /// </summary>
        /// <returns>FormSectionHelper 實例</returns>
        public static FormSectionHelper<TEntity> Create()
        {
            return new FormSectionHelper<TEntity>();
        }

        /// <summary>
        /// 將多個屬性加入指定區段 - 使用 Lambda Expression (類型安全)
        /// </summary>
        /// <param name="sectionName">區段名稱</param>
        /// <param name="propertySelectors">屬性選擇器陣列</param>
        /// <returns>當前實例，支援鏈式呼叫</returns>
        public FormSectionHelper<TEntity> AddToSection(string sectionName, params Expression<Func<TEntity, object?>>[] propertySelectors)
        {
            foreach (var selector in propertySelectors)
            {
                var propertyName = GetPropertyName(selector);
                _sections[propertyName] = sectionName;
            }
            return this;
        }

        /// <summary>
        /// 將多個自訂欄位名稱加入指定區段 - 用於非實體屬性的欄位 (如 FilterProductId)
        /// </summary>
        /// <param name="sectionName">區段名稱</param>
        /// <param name="fieldNames">欄位名稱陣列</param>
        /// <returns>當前實例，支援鏈式呼叫</returns>
        public FormSectionHelper<TEntity> AddCustomFields(string sectionName, params string[] fieldNames)
        {
            foreach (var fieldName in fieldNames)
            {
                _sections[fieldName] = sectionName;
            }
            return this;
        }

        /// <summary>
        /// 條件性地將屬性加入區段 - 用於基於權限或狀態的動態欄位
        /// </summary>
        /// <param name="condition">是否加入的條件</param>
        /// <param name="sectionName">區段名稱</param>
        /// <param name="propertySelectors">屬性選擇器陣列</param>
        /// <returns>當前實例，支援鏈式呼叫</returns>
        public FormSectionHelper<TEntity> AddIf(bool condition, string sectionName, params Expression<Func<TEntity, object?>>[] propertySelectors)
        {
            if (condition)
            {
                return AddToSection(sectionName, propertySelectors);
            }
            return this;
        }

        /// <summary>
        /// 條件性地將自訂欄位加入區段
        /// </summary>
        /// <param name="condition">是否加入的條件</param>
        /// <param name="sectionName">區段名稱</param>
        /// <param name="fieldNames">欄位名稱陣列</param>
        /// <returns>當前實例，支援鏈式呼叫</returns>
        public FormSectionHelper<TEntity> AddCustomFieldsIf(bool condition, string sectionName, params string[] fieldNames)
        {
            if (condition)
            {
                return AddCustomFields(sectionName, fieldNames);
            }
            return this;
        }

        /// <summary>
        /// 建立最終的 Dictionary - 用於傳遞給 GenericEditModalComponent
        /// </summary>
        /// <returns>欄位名稱到區段名稱的映射字典</returns>
        public Dictionary<string, string> Build()
        {
            return new Dictionary<string, string>(_sections);
        }

        /// <summary>
        /// 從 Lambda Expression 提取屬性名稱
        /// </summary>
        private static string GetPropertyName(Expression<Func<TEntity, object?>> propertySelector)
        {
            if (propertySelector.Body is MemberExpression memberExpression)
            {
                return memberExpression.Member.Name;
            }

            if (propertySelector.Body is UnaryExpression unaryExpression &&
                unaryExpression.Operand is MemberExpression operand)
            {
                return operand.Member.Name;
            }

            throw new ArgumentException($"無效的屬性選擇器表達式: {propertySelector}");
        }
    }

    /// <summary>
    /// 常用區段名稱常數 - 提供統一的區段命名
    /// </summary>
    public static class FormSectionNames
    {
        /// <summary>基本資訊</summary>
        public const string BasicInfo = "基本資訊";

        /// <summary>聯絡資訊</summary>
        public const string ContactInfo = "聯絡資訊";

        /// <summary>聯絡人資訊</summary>
        public const string ContactPersonInfo = "聯絡人資訊";

        /// <summary>金額資訊</summary>
        public const string AmountInfo = "金額資訊";

        /// <summary>金額資訊(系統自動計算)</summary>
        public const string AmountInfoAutoCalculated = "金額資訊(系統自動計算)";

        /// <summary>付款資訊</summary>
        public const string PaymentInfo = "付款資訊";

        /// <summary>額外資料</summary>
        public const string AdditionalData = "額外資料";

        /// <summary>額外資訊</summary>
        public const string AdditionalInfo = "額外資訊";

        /// <summary>其他資訊</summary>
        public const string OtherInfo = "其他資訊";

        /// <summary>組織架構</summary>
        public const string OrganizationStructure = "組織架構";

        /// <summary>任職資訊</summary>
        public const string EmploymentInfo = "任職資訊";

        /// <summary>公司資料</summary>
        public const string CompanyData = "公司資料";

        /// <summary>帳號資訊</summary>
        public const string AccountInfo = "帳號資訊";

        /// <summary>業務資訊</summary>
        public const string SalesInfo = "業務資訊";

        /// <summary>交易條件</summary>
        public const string TradingTerms = "交易條件";
    }
}
