/**
 * 批次列印輔助函數
 * 提供批次列印報表的 JavaScript 支援功能
 */

/**
 * 開啟批次列印視窗
 * @param {string} apiUrl - API 端點 URL
 * @param {string} jsonPayload - JSON 格式的批次列印條件
 */
window.openBatchPrintWindow = function(apiUrl, jsonPayload) {
    fetch(apiUrl, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: jsonPayload
    })
    .then(response => {
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        return response.text();
    })
    .then(html => {
        // 建立隱藏的 iframe 來載入報表內容
        const iframe = document.createElement('iframe');
        iframe.style.position = 'absolute';
        iframe.style.width = '0';
        iframe.style.height = '0';
        iframe.style.border = 'none';
        iframe.style.visibility = 'hidden';
        
        document.body.appendChild(iframe);
        
        // 寫入報表內容到 iframe
        const iframeDoc = iframe.contentWindow.document;
        iframeDoc.open();
        iframeDoc.write(html);
        iframeDoc.close();
        
        // 等待內容載入完成後直接觸發列印
        iframe.onload = function() {
            setTimeout(function() {
                iframe.contentWindow.print();
                
                // 列印完成後移除 iframe
                setTimeout(function() {
                    document.body.removeChild(iframe);
                }, 1000);
            }, 500);
        };
    })
    .catch(error => {
        console.error('批次列印錯誤:', error);
        alert(`批次列印失敗：${error.message}\n\n請稍後再試或聯絡系統管理員`);
    });
};

/**
 * 直接在當前視窗開啟批次列印（不使用新視窗）
 * @param {string} apiUrl - API 端點 URL
 * @param {string} jsonPayload - JSON 格式的批次列印條件
 */
window.openBatchPrintInCurrentWindow = function(apiUrl, jsonPayload) {
    fetch(apiUrl, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: jsonPayload
    })
    .then(response => {
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        return response.blob();
    })
    .then(blob => {
        // 建立 Blob URL 並在當前視窗開啟
        const url = URL.createObjectURL(blob);
        window.location.href = url;
    })
    .catch(error => {
        console.error('批次列印錯誤:', error);
        alert(`批次列印失敗：${error.message}\n\n請稍後再試或聯絡系統管理員`);
    });
};
