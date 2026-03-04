using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Services;
using Microsoft.JSInterop;

namespace ERPCore2.Helpers.Common;

/// <summary>
/// 聯絡資訊快捷連結 Helper
/// 為電話、信箱、地址、網站欄位產生可點擊的 ActionButton（撥打電話、寄信、Google Maps 定位等）
/// 僅在資料已儲存時啟用，避免對尚未儲存的新資料產生無效連結
/// </summary>
public class ContactLinkHelper
{
    private readonly IJSRuntime _jsRuntime;
    private readonly INotificationService _notificationService;

    public ContactLinkHelper(IJSRuntime jsRuntime, INotificationService notificationService)
    {
        _jsRuntime = jsRuntime;
        _notificationService = notificationService;
    }

    /// <summary>
    /// 電話欄位 → 撥打電話 (tel:)
    /// </summary>
    public FieldActionButton CreatePhoneButton(Func<string?> valueGetter, Func<bool> isSavedCheck)
    {
        return new FieldActionButton
        {
            Text = "",
            IconClass = "bi bi-telephone-outbound",
            Title = "撥打電話",
            Variant = "OutlinePrimary",
            Size = "Small",
            OnClick = async () => await OpenLinkAsync(
                valueGetter, isSavedCheck,
                v => $"tel:{v.Replace("-", "").Replace(" ", "")}",
                "電話號碼")
        };
    }

    /// <summary>
    /// 信箱欄位 → 系統郵件 (mailto:)
    /// </summary>
    public FieldActionButton CreateMailButton(Func<string?> valueGetter, Func<bool> isSavedCheck)
    {
        return new FieldActionButton
        {
            Text = "",
            IconClass = "bi bi-envelope",
            Title = "使用系統郵件寄信",
            Variant = "OutlinePrimary",
            Size = "Small",
            OnClick = async () => await OpenLinkAsync(
                valueGetter, isSavedCheck,
                v => $"mailto:{v}",
                "電子信箱")
        };
    }

    /// <summary>
    /// 信箱欄位 → Gmail 撰寫視窗
    /// </summary>
    public FieldActionButton CreateGmailButton(Func<string?> valueGetter, Func<bool> isSavedCheck)
    {
        return new FieldActionButton
        {
            Text = "",
            IconClass = "bi bi-google",
            Title = "使用 Gmail 寄信",
            Variant = "OutlinePrimary",
            Size = "Small",
            OnClick = async () => await OpenLinkAsync(
                valueGetter, isSavedCheck,
                v => $"https://mail.google.com/mail/?view=cm&fs=1&to={Uri.EscapeDataString(v)}",
                "電子信箱")
        };
    }

    /// <summary>
    /// 地址欄位 → Google Maps 定位
    /// </summary>
    public FieldActionButton CreateMapButton(Func<string?> valueGetter, Func<bool> isSavedCheck)
    {
        return new FieldActionButton
        {
            Text = "",
            IconClass = "bi bi-geo-alt",
            Title = "在 Google Maps 上查看",
            Variant = "OutlinePrimary",
            Size = "Small",
            OnClick = async () => await OpenLinkAsync(
                valueGetter, isSavedCheck,
                v => $"https://www.google.com/maps/search/?api=1&query={Uri.EscapeDataString(v)}",
                "地址")
        };
    }

    /// <summary>
    /// 網站欄位 → 開啟網站
    /// </summary>
    public FieldActionButton CreateWebButton(Func<string?> valueGetter, Func<bool> isSavedCheck)
    {
        return new FieldActionButton
        {
            Text = "",
            IconClass = "bi bi-globe",
            Title = "開啟網站",
            Variant = "OutlinePrimary",
            Size = "Small",
            OnClick = async () => await OpenLinkAsync(
                valueGetter, isSavedCheck,
                v => v.StartsWith("http") ? v : $"https://{v}",
                "網站網址")
        };
    }

    // ===== 組合方法：一次產生某類型欄位的所有按鈕 =====

    /// <summary>
    /// 電話欄位的按鈕組（撥號）
    /// </summary>
    public List<FieldActionButton> CreatePhoneButtons(Func<string?> valueGetter, Func<bool> isSavedCheck)
        => new() { CreatePhoneButton(valueGetter, isSavedCheck) };

    /// <summary>
    /// 信箱欄位的按鈕組（系統郵件 + Gmail）
    /// </summary>
    public List<FieldActionButton> CreateEmailButtons(Func<string?> valueGetter, Func<bool> isSavedCheck)
        => new() { CreateMailButton(valueGetter, isSavedCheck), CreateGmailButton(valueGetter, isSavedCheck) };

    /// <summary>
    /// 地址欄位的按鈕組（Google Maps）
    /// </summary>
    public List<FieldActionButton> CreateAddressButtons(Func<string?> valueGetter, Func<bool> isSavedCheck)
        => new() { CreateMapButton(valueGetter, isSavedCheck) };

    /// <summary>
    /// 網站欄位的按鈕組（開啟連結）
    /// </summary>
    public List<FieldActionButton> CreateWebsiteButtons(Func<string?> valueGetter, Func<bool> isSavedCheck)
        => new() { CreateWebButton(valueGetter, isSavedCheck) };

    // ===== 內部方法 =====

    private async Task OpenLinkAsync(
        Func<string?> valueGetter,
        Func<bool> isSavedCheck,
        Func<string, string> urlBuilder,
        string fieldDisplayName)
    {
        try
        {
            if (!isSavedCheck())
            {
                await _notificationService.ShowWarningAsync("請先儲存資料後再使用此功能");
                return;
            }

            var value = valueGetter();
            if (string.IsNullOrWhiteSpace(value))
            {
                await _notificationService.ShowWarningAsync($"{fieldDisplayName}尚未填寫");
                return;
            }

            var url = urlBuilder(value.Trim());
            await _jsRuntime.InvokeVoidAsync("open", url, "_blank");
        }
        catch (Exception)
        {
            await _notificationService.ShowErrorAsync("開啟連結時發生錯誤");
        }
    }
}
