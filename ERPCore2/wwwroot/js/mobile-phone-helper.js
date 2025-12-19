/**
 * 手機號碼輸入輔助工具 - 用於過濾非數字並自動格式化
 */

/**
 * 設置 input 元素的值並保持光標位置
 * @param {HTMLInputElement} element - input 元素
 * @param {string} value - 要設置的值
 */
window.setInputValue = function (element, value) {
    if (element) {
        // 記錄原始光標位置和原始值長度
        var oldLength = element.value.length;
        var oldSelectionStart = element.selectionStart;
        
        // 設置新值
        element.value = value;
        
        // 計算新的光標位置
        var newLength = value.length;
        var diff = newLength - oldLength;
        var newPosition = Math.max(0, Math.min(oldSelectionStart + diff, newLength));
        
        // 恢復光標位置
        element.setSelectionRange(newPosition, newPosition);
    }
};

/**
 * 格式化手機號碼輸入 - 只允許數字，並自動格式化為 0912-345-678
 * @param {HTMLInputElement} element - input 元素
 * @param {Event} event - input 事件
 * @returns {string} - 過濾後的純數字（供 Blazor 儲存）
 */
window.formatMobilePhoneInput = function (element) {
    if (!element) return '';
    
    var value = element.value;
    
    // 移除所有非數字字元
    var digitsOnly = value.replace(/\D/g, '');
    
    // 限制最大長度為 10 碼
    if (digitsOnly.length > 10) {
        digitsOnly = digitsOnly.substring(0, 10);
    }
    
    // 格式化為 0912-345-678
    var formatted = '';
    if (digitsOnly.length > 0) {
        formatted = digitsOnly.substring(0, Math.min(4, digitsOnly.length));
    }
    if (digitsOnly.length > 4) {
        formatted += '-' + digitsOnly.substring(4, Math.min(7, digitsOnly.length));
    }
    if (digitsOnly.length > 7) {
        formatted += '-' + digitsOnly.substring(7);
    }
    
    // 記錄原始光標位置
    var oldSelectionStart = element.selectionStart;
    var oldLength = element.value.length;
    
    // 設置格式化後的值
    element.value = formatted;
    
    // 計算新的光標位置（考慮新增的連字符）
    var newLength = formatted.length;
    var diff = newLength - oldLength;
    var newPosition = Math.max(0, Math.min(oldSelectionStart + diff, newLength));
    
    // 恢復光標位置
    element.setSelectionRange(newPosition, newPosition);
    
    // 回傳純數字供 Blazor 儲存到資料庫
    return digitsOnly;
};
