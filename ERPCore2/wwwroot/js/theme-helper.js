// theme-helper.js
// 管理介面主題切換（Light / Dark）
// AppTheme enum：Light = 1, Dark = 2
// 行為對應：
//   Light (1)  → 'light'：強制亮色（Bootstrap 預設）
//   Dark  (2)  → 'dark' ：強制深色

var themeEnumMap = {
    1: 'light',
    2: 'dark'
};

// 將主題值套用到 html[data-bs-theme]
function applyThemeValue(themeValue) {
    document.documentElement.setAttribute('data-bs-theme', themeValue);
}

// 由 Blazor 元件在儲存後呼叫
window.setAppTheme = function (enumValue) {
    var themeValue = themeEnumMap[enumValue] || 'light';
    document.cookie = 'ERPCore2.Theme=' + themeValue + ';path=/;max-age=31536000';
    applyThemeValue(themeValue);
};
