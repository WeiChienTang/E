// ===================================================
// 條碼生成 JavaScript 函數
// 使用 JsBarcode 套件生成條碼圖片
// ===================================================

/**
 * 生成條碼圖片
 * @param {string} elementId - SVG 元素的 ID
 * @param {string} barcodeValue - 條碼值
 * @param {number} width - 條碼寬度（預設 2）
 * @param {number} height - 條碼高度（預設 60）
 */
window.generateBarcode = function (elementId, barcodeValue, width = 2, height = 60) {
    try {
        const element = document.getElementById(elementId);
        
        if (!element) {
            console.warn(`找不到元素: ${elementId}`);
            return;
        }
        
        if (!barcodeValue || barcodeValue.trim() === '') {
            console.warn(`條碼值為空: ${elementId}`);
            return;
        }
        
        // 檢查 JsBarcode 是否已載入
        if (typeof JsBarcode === 'undefined') {
            console.error('JsBarcode 套件未載入');
            return;
        }
        
        // 生成條碼
        JsBarcode(element, barcodeValue, {
            format: "CODE128",
            width: width,
            height: height,
            displayValue: false, // 不在條碼下方顯示文字（我們會自己渲染）
            margin: 2,
            background: "#ffffff",
            lineColor: "#000000"
        });
        
    } catch (error) {
        console.error(`生成條碼失敗 (${elementId}):`, error);
    }
};

/**
 * 批次生成多個條碼
 * @param {Array} barcodeConfigs - 條碼配置陣列 [{elementId, value, width, height}]
 */
window.generateBarcodes = function (barcodeConfigs) {
    if (!Array.isArray(barcodeConfigs)) {
        console.error('barcodeConfigs 必須是陣列');
        return;
    }
    
    barcodeConfigs.forEach(config => {
        window.generateBarcode(
            config.elementId,
            config.value,
            config.width || 2,
            config.height || 60
        );
    });
};

/**
 * 清除條碼
 * @param {string} elementId - SVG 元素的 ID
 */
window.clearBarcode = function (elementId) {
    try {
        const element = document.getElementById(elementId);
        if (element) {
            element.innerHTML = '';
        }
    } catch (error) {
        console.error(`清除條碼失敗 (${elementId}):`, error);
    }
};
