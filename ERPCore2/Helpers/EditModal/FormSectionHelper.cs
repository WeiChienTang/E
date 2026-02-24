using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;

namespace ERPCore2.Helpers.EditModal
{
    /// <summary>
    /// Tab 頁籤定義 - 定義一個 Tab 包含哪些 Section
    /// </summary>
    public class FormTabDefinition
    {
        /// <summary>Tab 標籤文字</summary>
        public string Label { get; set; } = "";

        /// <summary>Tab 圖示 CSS class（例如 "bi bi-person-fill"），可為 null</summary>
        public string? Icon { get; set; }

        /// <summary>此 Tab 包含的 Section 名稱清單</summary>
        public List<string> SectionNames { get; set; } = new();

        /// <summary>
        /// 自訂內容 RenderFragment（非 null 時，此 Tab 渲染自訂內容而非表單欄位區段）
        /// 當設定此屬性時，SectionNames 可以為空列表
        /// </summary>
        public RenderFragment? CustomContent { get; set; }

        /// <summary>
        /// 額外內容 RenderFragment（在表單欄位區段之後渲染，CustomContent 為 null 時有效）
        /// </summary>
        public RenderFragment? ExtraContent { get; set; }
    }

    /// <summary>
    /// FormSectionHelper.BuildAll() 的回傳結果 - 同時包含 FieldSections 和 TabDefinitions
    /// </summary>
    public class FormLayoutResult
    {
        /// <summary>欄位名稱到區段名稱的映射字典</summary>
        public Dictionary<string, string> FieldSections { get; set; } = new();

        /// <summary>Tab 頁籤定義清單（為 null 時表示不使用 Tab 佈局）</summary>
        public List<FormTabDefinition>? TabDefinitions { get; set; }
    }

    /// <summary>
    /// 表單區段定義生成器 - 簡化 formSections 的建立
    /// 提供流暢API來定義表單欄位的區段歸屬，並可選擇性地將區段歸組到 Tab 頁籤
    /// </summary>
    /// <typeparam name="TEntity">實體類型</typeparam>
    public class FormSectionHelper<TEntity> where TEntity : class
    {
        private readonly Dictionary<string, string> _sections = new();
        private readonly List<FormTabDefinition> _tabDefinitions = new();

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
        /// 將多個 Section 歸組到一個 Tab 頁籤
        /// </summary>
        /// <param name="tabLabel">Tab 標籤文字</param>
        /// <param name="icon">Tab 圖示 CSS class（例如 "bi bi-person-fill"），可為 null</param>
        /// <param name="sectionNames">此 Tab 包含的 Section 名稱</param>
        /// <returns>當前實例，支援鏈式呼叫</returns>
        public FormSectionHelper<TEntity> GroupIntoTab(string tabLabel, string? icon, params string[] sectionNames)
        {
            _tabDefinitions.Add(new FormTabDefinition
            {
                Label = tabLabel,
                Icon = icon,
                SectionNames = sectionNames.ToList()
            });
            return this;
        }

        /// <summary>
        /// 建立一個自訂內容的 Tab 頁籤（不包含表單欄位，而是渲染自訂 RenderFragment）
        /// </summary>
        /// <param name="tabLabel">Tab 標籤文字</param>
        /// <param name="icon">Tab 圖示 CSS class（例如 "bi bi-truck"），可為 null</param>
        /// <param name="customContent">自訂內容 RenderFragment</param>
        /// <returns>當前實例，支援鏈式呼叫</returns>
        public FormSectionHelper<TEntity> GroupIntoCustomTab(string tabLabel, string? icon, RenderFragment customContent)
        {
            _tabDefinitions.Add(new FormTabDefinition
            {
                Label = tabLabel,
                Icon = icon,
                SectionNames = new List<string>(),
                CustomContent = customContent
            });
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
        /// 建立完整的表單佈局結果（包含 FieldSections 和 TabDefinitions）
        /// 當有使用 GroupIntoTab() 時，請使用此方法取代 Build()
        /// </summary>
        /// <returns>包含 FieldSections 和 TabDefinitions 的 FormLayoutResult</returns>
        public FormLayoutResult BuildAll()
        {
            return new FormLayoutResult
            {
                FieldSections = Build(),
                TabDefinitions = _tabDefinitions.Count > 0 ? _tabDefinitions.ToList() : null
            };
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

        /// <summary>篩選條件（虛擬欄位，不儲存至資料庫，僅用於篩選下方明細 Table）</summary>
        public const string FilterConditions = "篩選條件";

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

        /// <summary>單位設定</summary>
        public const string UnitSettings = "單位設定";

        /// <summary>分類與規格</summary>
        public const string CategoryAndSpecification = "分類與規格";

        /// <summary>財務與備註</summary>
        public const string FinanceAndRemarks = "財務與備註";

        /// <summary>配給裝備</summary>
        public const string EquipmentAssignment = "配給裝備";
    }
}
