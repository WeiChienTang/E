using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Data;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using ERPCore2.Components.Shared.Modal;
using ERPCore2.Components.Shared.Table;
using ERPCore2.Components.Shared.Page;
using ERPCore2.Components.Shared.Statistics;
using Microsoft.Extensions.Localization;
namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 表單欄位配置輔助類別 - 提供標準欄位配置，特別是處理字數和位元組限制
    /// </summary>
    public static class FormFieldConfigurationHelper
    {
        /// <summary>
        /// 建立標準的 Remarks 欄位配置
        /// </summary>
        /// <typeparam name="TEntity">實體類型</typeparam>
        /// <param name="label">欄位標籤，預設為 "備註"</param>
        /// <param name="placeholder">佔位符文字，預設為 "請輸入備註"</param>
        /// <param name="helpText">說明文字，預設為 "其他需要補充的資訊"</param>
        /// <param name="rows">文字區域列數，預設為 2</param>
        /// <param name="containerCssClass">容器 CSS 類別，預設為 "col-12"</param>
        /// <param name="readOnly">是否為唯讀欄位，預設為 false</param>
        /// <returns>配置好的 FormFieldDefinition</returns>
        public static FormFieldDefinition CreateRemarksField<TEntity>(
            string? label = null,
            string? placeholder = null,
            string? helpText = null,
            int rows = 2,
            string containerCssClass = "col-12",
            bool readOnly = false,
            IStringLocalizer<ERPCore2.SharedResource>? l = null) where TEntity : BaseEntity
        {
            label ??= l?["Label.Remarks"].ToString() ?? "備註";
            placeholder ??= l?["Placeholder.Remarks"].ToString() ?? "請輸入備註";
            helpText ??= "";
            // 統一使用 100 字元限制，但保持 500 位元組的資料庫限制
            return new FormFieldDefinition
            {
                PropertyName = nameof(BaseEntity.Remarks),
                Label = label,
                FieldType = FormFieldType.TextAreaWithCharacterCount,
                Placeholder = placeholder,
                Rows = rows,
                MaxLength = 100,      // 統一字元限制
                MaxBytes = 500,       // 保持資料庫位元組限制
                HelpText = helpText,
                ContainerCssClass = containerCssClass,
                IsReadOnly = readOnly
            };
        }
        /// <summary>
        /// 取得實體屬性的 MaxLength 值
        /// </summary>
        /// <typeparam name="TEntity">實體類型</typeparam>
        /// <param name="propertyName">屬性名稱</param>
        /// <returns>MaxLength 值，如果沒有設定則返回 null</returns>
        public static int? GetMaxLengthFromEntity<TEntity>(string propertyName)
        {
            var property = typeof(TEntity).GetProperty(propertyName);
            var maxLengthAttribute = property?.GetCustomAttribute<MaxLengthAttribute>();
            return maxLengthAttribute?.Length;
        }

        /// <summary>
        /// 根據慣例自動推斷欄位的保護等級（FieldProtectionLevel）
        /// 規則：
        ///   1. PropertyName == "Code" → SystemRequired（業務識別碼，所有 Service 的 ValidateAsync 都檢查）
        ///   2. PropertyName 以 "Id" 結尾且 IsRequired == true → SystemRequired（必要外鍵）
        ///   3. 其餘 IsRequired == true → BusinessRequired（業務必要，但可由使用者覆蓋為選填）
        ///   4. 其餘 → Normal（完全自由配置）
        /// 個別模組仍可在呼叫後手動覆蓋特定欄位的 ProtectionLevel
        /// </summary>
        public static List<FormFieldDefinition> ApplyProtectionLevels(List<FormFieldDefinition> formFields)
        {
            foreach (var field in formFields)
            {
                // 已被手動設定過的（非 Normal）不覆蓋
                if (field.ProtectionLevel != FieldProtectionLevel.Normal)
                    continue;

                if (field.PropertyName == "Code")
                {
                    field.ProtectionLevel = FieldProtectionLevel.SystemRequired;
                }
                else if (field.PropertyName.EndsWith("Id") && field.IsRequired)
                {
                    field.ProtectionLevel = FieldProtectionLevel.SystemRequired;
                }
                else if (field.IsRequired)
                {
                    field.ProtectionLevel = FieldProtectionLevel.BusinessRequired;
                }
                // else: Normal (default)
            }

            return formFields;
        }

        /// <summary>
        /// 自動為所有包含 MaxLength 屬性的文字欄位套用字數限制
        /// </summary>
        /// <typeparam name="TEntity">實體類型</typeparam>
        /// <param name="formFields">表單欄位列表</param>
        /// <returns>更新後的表單欄位列表</returns>
        public static List<FormFieldDefinition> ApplyMaxLengthLimits<TEntity>(List<FormFieldDefinition> formFields)
        {
            foreach (var field in formFields)
            {
                if (field.FieldType == FormFieldType.Text || 
                    field.FieldType == FormFieldType.TextArea ||
                    field.FieldType == FormFieldType.TextAreaWithCharacterCount)
                {
                    // 如果沒有設定 MaxLength，嘗試從實體屬性取得
                    if (!field.MaxLength.HasValue)
                    {
                        var maxLength = GetMaxLengthFromEntity<TEntity>(field.PropertyName);
                        if (maxLength.HasValue)
                        {
                            field.MaxLength = maxLength.Value;
                            
                            // 如果是 TextArea 且字數較多，建議使用帶字數統計的版本
                            if (field.FieldType == FormFieldType.TextArea && maxLength.Value >= 100)
                            {
                                field.FieldType = FormFieldType.TextAreaWithCharacterCount;
                                field.MaxBytes = maxLength.Value;
                            }
                        }
                    }
                    
                    // 如果沒有設定 MaxBytes，預設與 MaxLength 相同
                    if (!field.MaxBytes.HasValue && field.MaxLength.HasValue)
                    {
                        field.MaxBytes = field.MaxLength.Value;
                    }
                }
            }
            
            return formFields;
        }
    }
}


