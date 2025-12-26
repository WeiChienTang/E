/**
 * 數字輸入輔助工具 - 提供即時的 Min/Max 範圍限制
 */
window.NumberInputHelper = {
    /**
     * 初始化數字輸入欄位的即時範圍限制
     * @param {HTMLInputElement} element - 輸入元素
     * @param {number|null} min - 最小值
     * @param {number|null} max - 最大值
     */
    initRangeLimit: function (element, min, max) {
        if (!element) return;

        // 儲存上一個有效值
        let lastValidValue = element.value;

        // 處理輸入事件 - 即時驗證
        element.addEventListener('input', function (e) {
            let value = e.target.value;

            // 允許空值、負號（如果 min < 0）和正在輸入的小數點
            if (value === '' || value === '-' || value === '.') {
                return;
            }

            // 嘗試解析數值
            let numValue = parseFloat(value);
            if (isNaN(numValue)) {
                // 無效輸入，還原為上一個有效值
                e.target.value = lastValidValue;
                return;
            }

            // 檢查最大值限制
            if (max !== null && numValue > max) {
                e.target.value = max;
                lastValidValue = max.toString();
                // 觸發 change 事件讓 Blazor 知道值已變更
                e.target.dispatchEvent(new Event('change', { bubbles: true }));
                return;
            }

            // 檢查最小值限制
            if (min !== null && numValue < min) {
                e.target.value = min;
                lastValidValue = min.toString();
                // 觸發 change 事件讓 Blazor 知道值已變更
                e.target.dispatchEvent(new Event('change', { bubbles: true }));
                return;
            }

            // 有效值，更新記錄
            lastValidValue = value;
        });

        // 處理失去焦點事件 - 最終驗證
        element.addEventListener('blur', function (e) {
            let value = e.target.value;
            if (value === '' || value === '-' || value === '.') {
                return;
            }

            let numValue = parseFloat(value);
            if (isNaN(numValue)) {
                e.target.value = '';
                return;
            }

            // 確保值在範圍內
            if (max !== null && numValue > max) {
                e.target.value = max;
                e.target.dispatchEvent(new Event('change', { bubbles: true }));
            } else if (min !== null && numValue < min) {
                e.target.value = min;
                e.target.dispatchEvent(new Event('change', { bubbles: true }));
            }
        });

        // 處理貼上事件
        element.addEventListener('paste', function (e) {
            setTimeout(function () {
                let value = e.target.value;
                let numValue = parseFloat(value);

                if (isNaN(numValue)) {
                    e.target.value = lastValidValue;
                    return;
                }

                if (max !== null && numValue > max) {
                    e.target.value = max;
                    e.target.dispatchEvent(new Event('change', { bubbles: true }));
                } else if (min !== null && numValue < min) {
                    e.target.value = min;
                    e.target.dispatchEvent(new Event('change', { bubbles: true }));
                }

                lastValidValue = e.target.value;
            }, 0);
        });
    },

    /**
     * 透過元素 ID 初始化範圍限制
     * @param {string} elementId - 元素 ID
     * @param {number|null} min - 最小值
     * @param {number|null} max - 最大值
     */
    initRangeLimitById: function (elementId, min, max) {
        let element = document.getElementById(elementId);
        if (element) {
            this.initRangeLimit(element, min, max);
        }
    }
};
