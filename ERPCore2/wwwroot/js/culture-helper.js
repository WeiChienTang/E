// culture-helper.js
// 寫入 .AspNetCore.Culture cookie 並重新載入頁面以套用語言設定
// 由 PersonalPreferenceModalComponent 在語言變更後呼叫

window.setCultureAndReload = function (culture) {
    // 防循環偵測：若 3 秒內已 reload 過就停止，避免無限 reload loop
    const lastKey = 'erpcore2_cultureReload';
    const lastTs = parseInt(localStorage.getItem(lastKey) || '0');
    const now = Date.now();
    if (now - lastTs < 3000) {
        console.error('[CultureHelper] 偵測到快速重複 reload，已中止。目前 culture:', culture);
        return;
    }
    localStorage.setItem(lastKey, now.toString());

    console.log('[CultureHelper] 切換語言 →', culture);
    const cookieValue = `c=${culture}|uic=${culture}`;
    document.cookie = `.AspNetCore.Culture=${cookieValue};path=/;max-age=31536000`;
    window.location.reload();
};
