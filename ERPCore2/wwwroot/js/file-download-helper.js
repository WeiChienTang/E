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
 * 將報表頁面圖片嵌入可列印的 HTML 視窗，讓使用者透過瀏覽器選擇任何本機印表機列印
 * @param {string[]} base64Images - Base64 編碼的 PNG 圖片陣列（每個元素對應一頁）
 * @param {string} title - 列印標題
 * @param {number} widthCm - 頁面寬度（公分），預設 21（A4）
 * @param {number} heightCm - 頁面高度（公分），預設 29.7（A4）
 
window.browserPrintImages = function (base64Images, title, widthCm, heightCm) {
    try {
        widthCm = widthCm || 21;
        heightCm = heightCm || 29.7;

        const printWindow = window.open('', '_blank', 'width=800,height=600');
        if (!printWindow) {
            console.error('無法開啟列印視窗，請確認瀏覽器未封鎖彈出視窗');
            return false;
        }

        let content = `<!DOCTYPE html>
<html>
<head>
<meta charset="utf-8">
<title>${title || '報表列印'}</title>
<style>
    @page { margin: 0; size: ${widthCm}cm ${heightCm}cm; }
    html, body { margin: 0; padding: 0; background: white; }
    .page {
        width: ${widthCm}cm;
        height: ${heightCm}cm;
        page-break-after: always;
        overflow: hidden;
    }
    .page:last-child { page-break-after: auto; }
    img { width: 100%; height: 100%; display: block; object-fit: fill; }
</style>
</head>
<body>`;

        base64Images.forEach(function (img) {
            content += `<div class="page"><img src="data:image/png;base64,${img}"></div>`;
        });

        content += '</body></html>';

        printWindow.document.write(content);
        printWindow.document.close();

        printWindow.onload = function () {
            printWindow.focus();
            printWindow.print();
        };

        return true;
    } catch (e) {
        console.error('本機列印失敗:', e);
        return false;
    }
};
*/
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
