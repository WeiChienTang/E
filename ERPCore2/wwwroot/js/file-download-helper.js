/**
 * 檔案下載輔助函數
 * 用於在瀏覽器中下載由 C# 生成的檔案
 */

/**
 * 從 Base64 字串下載檔案
 * @param {string} fileName - 檔案名稱（含副檔名）
 * @param {string} base64Content - Base64 編碼的檔案內容
 * @param {string} contentType - MIME 類型（如 application/vnd.openxmlformats-officedocument.spreadsheetml.sheet）
 */
window.downloadFileFromBase64 = function (fileName, base64Content, contentType) {
    try {
        // 將 Base64 轉換為 Blob
        const byteCharacters = atob(base64Content);
        const byteNumbers = new Array(byteCharacters.length);
        for (let i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
        }
        const byteArray = new Uint8Array(byteNumbers);
        const blob = new Blob([byteArray], { type: contentType });

        // 建立下載連結
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = fileName;

        // 觸發下載
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);

        // 釋放 URL
        URL.revokeObjectURL(url);

        return true;
    } catch (e) {
        console.error('下載檔案失敗:', e);
        return false;
    }
};

/**
 * 開啟瀏覽器列印對話框（本機列印）
 * 使用隱藏 iframe 嵌入報表圖片並呼叫 window.print()。
 * 相較於 popup 視窗，iframe 不受彈出視窗封鎖限制，且 @page size CSS 較可靠。
 * 圖片直接以公分為單位設定尺寸，確保列印尺寸與紙張完全符合。
 * @param {string[]} base64Images - Base64 編碼的 PNG 圖片陣列（每個元素對應一頁）
 * @param {string} title - 列印標題
 * @param {number} widthCm - 頁面寬度（公分），預設 21（A4）
 * @param {number} heightCm - 頁面高度（公分），預設 29.7（A4）
 */
window.browserPrintImages = function (base64Images, title, widthCm, heightCm) {
    try {
        widthCm = widthCm || 21;
        heightCm = heightCm || 29.7;

        // 建立隱藏 iframe，避免彈出視窗被封鎖，同時讓 @page size CSS 更可靠
        var iframe = document.createElement('iframe');
        iframe.style.cssText = 'position:fixed;top:-9999px;left:-9999px;width:1px;height:1px;border:none;visibility:hidden;';
        document.body.appendChild(iframe);

        var doc = iframe.contentDocument || iframe.contentWindow.document;

        // widthCm/heightCm 已由 C# 端依紙張方向正確傳入（Landscape 時 width > height）
        var orientation = widthCm > heightCm ? 'landscape' : 'portrait';

        var html = '<!DOCTYPE html><html><head><meta charset="utf-8"><title>'
            + (title || '報表列印')
            + '</title><style>'
            // 明確指定紙張尺寸與方向，確保瀏覽器及印表機驅動程式正確套用
            + '@page{margin:0;size:' + widthCm + 'cm ' + heightCm + 'cm ' + orientation + ';}'
            + 'html,body{margin:0;padding:0;background:white;}'
            // 每張圖片直接以 cm 設定，確保與渲染時的紙張尺寸一致
            + 'img{display:block;width:' + widthCm + 'cm;height:' + heightCm + 'cm;page-break-after:always;}'
            + 'img:last-child{page-break-after:auto;}'
            + '</style></head><body>';

        base64Images.forEach(function (img) {
            html += '<img src="data:image/png;base64,' + img + '">';
        });
        html += '</body></html>';

        doc.open();
        doc.write(html);
        doc.close();

        // 等待 Base64 圖片在 iframe 內完成解碼與繪製，再觸發列印
        setTimeout(function () {
            if (iframe.contentWindow) {
                iframe.contentWindow.focus();
                iframe.contentWindow.print();
            }
            // 列印對話框關閉後清理 iframe（延遲以確保列印任務已送出）
            setTimeout(function () {
                if (iframe.parentNode) {
                    iframe.parentNode.removeChild(iframe);
                }
            }, 3000);
        }, 500);

        return true;
    } catch (e) {
        console.error('本機列印失敗:', e);
        return false;
    }
};
/**
 * 從 byte 陣列下載檔案
 * @param {string} fileName - 檔案名稱（含副檔名）
 * @param {Uint8Array} byteArray - byte 陣列
 * @param {string} contentType - MIME 類型
 */
window.downloadFileFromBytes = function (fileName, byteArray, contentType) {
    try {
        const blob = new Blob([byteArray], { type: contentType });
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = fileName;

        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);

        URL.revokeObjectURL(url);

        return true;
    } catch (e) {
        console.error('下載檔案失敗:', e);
        return false;
    }
};
