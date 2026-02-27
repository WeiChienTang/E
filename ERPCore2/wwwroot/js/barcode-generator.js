// barcode-generator.js
// 條碼生成輔助函數
// 注意：條碼圖片目前由後端 BarcodeGeneratorService 產生（使用 BarcodeStandard + SkiaSharp）
// 此檔案保留供未來需要前端條碼渲染時使用（搭配 JsBarcode CDN）

window.barcodeGenerator = {
    // 使用 JsBarcode 在指定 canvas/svg 元素上渲染條碼
    render: function (elementId, value, options) {
        if (typeof JsBarcode === 'undefined') return;
        try {
            JsBarcode('#' + elementId, value, options || {});
        } catch (e) {
            console.warn('barcode-generator: render failed', e);
        }
    }
};
