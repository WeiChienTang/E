// culture-helper.js
// 寫入 .AspNetCore.Culture cookie 並重新載入頁面以套用語言設定
// 由 PersonalPreferenceModalComponent 在語言變更後呼叫

window.setCultureAndReload = function (culture) {
    const cookieValue = `c=${culture}|uic=${culture}`;
    document.cookie = `.AspNetCore.Culture=${cookieValue};path=/;max-age=31536000`;
    window.location.reload();
};
