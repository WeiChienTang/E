// content-zoom-helper.js
// 管理字型縮放偏好設定的讀取與套用
// 由 DisplayTab 在儲存設定後呼叫，在 App.razor 的 inline script 負責初始載入

// ContentZoom enum 值對應的 rem 字串
var contentZoomMap = {
    1: '0.75rem',   // XSmall  75%
    2: '0.9rem',    // Small   90%
    3: '1rem',      // Medium  100%（預設）
    4: '1.1rem',    // Large   110%
    5: '1.25rem',   // XLarge  125%
    6: '1.5rem'     // XXLarge 150%
};

// 套用縮放並寫入 cookie（由 Blazor 元件在儲存後呼叫）
window.setContentZoom = function (enumValue) {
    var zoomRem = contentZoomMap[enumValue] || '1rem';
    document.cookie = 'ERPCore2.ContentZoom=' + zoomRem + ';path=/;max-age=31536000';
    document.documentElement.style.setProperty('--content-zoom', zoomRem);
};
