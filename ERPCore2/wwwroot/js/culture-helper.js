// culture-helper.js
// 寫入 .AspNetCore.Culture cookie 並重新載入頁面以套用語言設定
// 由 PersonalPreferenceModalComponent 在語言變更後呼叫

// 只寫 cookie，不 reload（由 C# NavigationManager.NavigateTo 負責 forceLoad）
// 避免 window.location.reload() 在 InvokeVoidAsync 回傳前切斷 SignalR 連線導致無限讀取
window.setCultureCookie = function (culture) {
    console.log('[CultureHelper] 設定語言 cookie →', culture);
    const cookieValue = `c=${culture}|uic=${culture}`;
    document.cookie = `.AspNetCore.Culture=${cookieValue};path=/;max-age=31536000`;
};

// 舊版（保留相容性，不建議再使用）
window.setCultureAndReload = function (culture) {
    window.setCultureCookie(culture);
    window.location.reload();
};
