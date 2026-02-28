// theme-helper.js
// 管理介面主題切換（Light / Dark / System）
// AppTheme enum：Light = 1, Dark = 2, System = 3
// 行為對應：
//   Light (1)  → 'system'：跟隨作業系統主題（OS 亮→亮，OS 暗→暗）
//   Dark  (2)  → 'dark'  ：強制深色
//   System(3)  → 'light' ：強制亮色（Bootstrap 預設，即原始設計樣式）

var themeEnumMap = {
    1: 'system',
    2: 'dark',
    3: 'light'
};

// 將主題值套用到 html[data-bs-theme]
function applyThemeValue(themeValue) {
    if (themeValue === 'system') {
        var prefersDark = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
        document.documentElement.setAttribute('data-bs-theme', prefersDark ? 'dark' : 'light');
    } else {
        document.documentElement.setAttribute('data-bs-theme', themeValue);
    }
}

// 由 Blazor 元件在儲存後呼叫
window.setAppTheme = function (enumValue) {
    var themeValue = themeEnumMap[enumValue] || 'system';
    document.cookie = 'ERPCore2.Theme=' + themeValue + ';path=/;max-age=31536000';
    applyThemeValue(themeValue);

    // 若為系統設定，監聽 OS 主題變更以即時跟隨
    if (window._themeMediaQuery && window._themeChangeHandler) {
        window._themeMediaQuery.removeEventListener('change', window._themeChangeHandler);
    }

    if (themeValue === 'system') {
        window._themeMediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
        window._themeChangeHandler = function (e) {
            var match = document.cookie.match(/ERPCore2\.Theme=([^;]+)/);
            var current = match ? decodeURIComponent(match[1]) : 'system';
            if (current === 'system') {
                document.documentElement.setAttribute('data-bs-theme', e.matches ? 'dark' : 'light');
            }
        };
        window._themeMediaQuery.addEventListener('change', window._themeChangeHandler);
    }
};
