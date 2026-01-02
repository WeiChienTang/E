namespace ERPCore2.Components.Shared.Forms;

/// <summary>
/// 表單相關常數定義
/// </summary>
public static class FormConstants
{
    /// <summary>
    /// 動作按鈕文字常數
    /// </summary>
    public static class ActionButtonText
    {
        public const string Add = "新增";
        public const string Edit = "編輯";
        public const string Copy = "複製";
        public const string CopyProductCode = "同產編";
        public const string GoTo = "前往";
        public const string Delete = "刪除";
        public const string Search = "搜尋";
    }

    /// <summary>
    /// Font Awesome 圖示 CSS 類別
    /// </summary>
    public static class Icons
    {
        public const string Add = "fas fa-plus-circle";
        public const string Edit = "fas fa-edit";
        public const string Copy = "fas fa-copy";
        public const string GoTo = "fas fa-external-link-alt";
        public const string Delete = "fas fa-trash-alt";
        public const string Search = "fas fa-search";
        public const string Default = "fas fa-ellipsis-h";
    }

    /// <summary>
    /// 按鈕樣式變體
    /// </summary>
    public static class ButtonVariants
    {
        public const string Primary = "Primary";
        public const string Secondary = "Secondary";
        public const string Success = "Success";
        public const string Danger = "Danger";
        public const string Warning = "Warning";
        public const string Info = "Info";
        public const string OutlinePrimary = "OutlinePrimary";
        public const string OutlineSecondary = "OutlineSecondary";
        public const string OutlineSuccess = "OutlineSuccess";
        public const string OutlineDanger = "OutlineDanger";
        public const string OutlineWarning = "OutlineWarning";
        public const string OutlineInfo = "OutlineInfo";
    }

    /// <summary>
    /// 按鈕尺寸
    /// </summary>
    public static class ButtonSizes
    {
        public const string Large = "Large";
        public const string Normal = "Normal";
        public const string Small = "Small";
    }

    /// <summary>
    /// 顏色 CSS 類別
    /// </summary>
    public static class ColorClasses
    {
        public const string Primary = "primary";
        public const string Secondary = "secondary";
        public const string Success = "success";
        public const string Danger = "danger";
        public const string Warning = "warning";
        public const string Info = "info";
    }

    /// <summary>
    /// CSS 類別常數
    /// </summary>
    public static class CssClasses
    {
        public const string FormControl = "form-control";
        public const string FormLabel = "form-label fw-bold";
        public const string FormCheck = "form-check";
        public const string FormCheckInput = "form-check-input";
        public const string FormCheckLabel = "form-check-label";
        public const string HasActionButtons = "has-action-buttons";
        public const string InputWithActionButtons = "input-with-action-buttons";
        public const string MultipleButtons = "multiple-buttons";
        public const string PositionRelative = "position-relative";
        public const string ListGroup = "list-group";
        public const string ListGroupItem = "list-group-item list-group-item-action";
        public const string ListGroupItemActive = "list-group-item list-group-item-action active";
    }

    /// <summary>
    /// 預設值
    /// </summary>
    public static class Defaults
    {
        public const int AutoCompleteDelayMs = 300;
        public const int MinSearchLength = 1;
        public const int TextAreaRows = 3;
        public const int MaxCharacters = 500;
        public const int MaxBytes = 500;
        public const string MobilePhonePlaceholder = "0912-345-678";
        public const string MobilePhoneTitle = "請輸入有效的台灣手機號碼（10碼，以09開頭）";
        public const int MobilePhoneMaxLength = 12;
        public const string DateMin = "1900-01-01";
        public const string DateMax = "2099-12-31";
        public const int DropdownMaxHeight = 200;
        public const int DropdownZIndex = 1000;
        public const int BlurDelayMs = 300;
    }

    /// <summary>
    /// 自動完成 HTML 屬性值
    /// </summary>
    public static class AutoCompleteAttributes
    {
        public const string Off = "off";
        public const string NewPassword = "new-password";
        public const string Tel = "tel";
    }

    /// <summary>
    /// 根據按鈕文字取得對應圖示
    /// </summary>
    public static string GetIconForButtonText(string buttonText)
    {
        return buttonText switch
        {
            ActionButtonText.Add => Icons.Add,
            ActionButtonText.Edit => Icons.Edit,
            ActionButtonText.Copy or ActionButtonText.CopyProductCode => Icons.Copy,
            ActionButtonText.GoTo => Icons.GoTo,
            ActionButtonText.Delete => Icons.Delete,
            ActionButtonText.Search => Icons.Search,
            _ => Icons.Default
        };
    }

    /// <summary>
    /// 根據變體取得顏色類別
    /// </summary>
    public static string GetColorClassForVariant(string variant)
    {
        return variant switch
        {
            ButtonVariants.OutlinePrimary or ButtonVariants.Primary => ColorClasses.Primary,
            ButtonVariants.OutlineSecondary or ButtonVariants.Secondary => ColorClasses.Secondary,
            ButtonVariants.OutlineSuccess or ButtonVariants.Success => ColorClasses.Success,
            ButtonVariants.OutlineDanger or ButtonVariants.Danger => ColorClasses.Danger,
            ButtonVariants.OutlineWarning or ButtonVariants.Warning => ColorClasses.Warning,
            ButtonVariants.OutlineInfo or ButtonVariants.Info => ColorClasses.Info,
            _ => ColorClasses.Primary
        };
    }
}
