/**
 * 剪貼簿輔助函數
 * 提供複製文字到剪貼簿的功能
 */

/**
 * 複製文字到剪貼簿
 * @param {string} text - 要複製的文字
 * @returns {Promise<boolean>} - 是否成功
 */
async function copyToClipboard(text) {
    try {
        // 優先使用現代 Clipboard API
        if (navigator.clipboard && navigator.clipboard.writeText) {
            await navigator.clipboard.writeText(text);
            return true;
        }
        
        // 降級方案：使用 execCommand（舊瀏覽器）
        return copyToClipboardFallback(text);
    } catch (error) {
        console.error('複製到剪貼簿失敗:', error);
        
        // 嘗試降級方案
        try {
            return copyToClipboardFallback(text);
        } catch (fallbackError) {
            console.error('降級複製方案也失敗:', fallbackError);
            return false;
        }
    }
}

/**
 * 降級複製方案 - 使用 textarea 和 execCommand
 * @param {string} text - 要複製的文字
 * @returns {boolean} - 是否成功
 */
function copyToClipboardFallback(text) {
    const textarea = document.createElement('textarea');
    textarea.value = text;
    
    // 設定樣式讓 textarea 不可見
    textarea.style.position = 'fixed';
    textarea.style.left = '-9999px';
    textarea.style.top = '-9999px';
    textarea.style.opacity = '0';
    textarea.setAttribute('readonly', '');
    
    document.body.appendChild(textarea);
    
    try {
        textarea.select();
        textarea.setSelectionRange(0, textarea.value.length);
        
        const successful = document.execCommand('copy');
        return successful;
    } finally {
        document.body.removeChild(textarea);
    }
}

// 確保函數在全域範圍可用
window.copyToClipboard = copyToClipboard;
