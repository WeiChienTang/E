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
