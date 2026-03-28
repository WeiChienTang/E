/**
 * IME 輸入法組字 Helper
 * 解決 Blazor @oninput + value 綁定在 IME 組字期間造成文字重複的問題
 *
 * 原理：在 compositionstart ~ compositionend 期間，透過 JS callback 通知 C# 端
 * 組字狀態，讓 C# 端在組字中跳過 @oninput 處理。compositionend 時送回最終值。
 */
window.ImeHelper = {
    /**
     * 為指定 input 元素掛載 IME composition 事件監聽
     * @param {HTMLInputElement} element - 目標 input 元素
     * @param {DotNetObjectReference} dotnetRef - C# 物件參考
     */
    attach: function (element, dotnetRef) {
        if (!element || element._imeAttached) return;

        element._onCompositionStart = function () {
            dotnetRef.invokeMethodAsync('OnImeCompositionStart');
        };

        element._onCompositionEnd = function () {
            dotnetRef.invokeMethodAsync('OnImeCompositionEnd', element.value);
        };

        element.addEventListener('compositionstart', element._onCompositionStart);
        element.addEventListener('compositionend', element._onCompositionEnd);
        element._imeAttached = true;
    },

    /**
     * 清除指定元素的 IME 監聽
     * @param {HTMLInputElement} element
     */
    detach: function (element) {
        if (!element || !element._imeAttached) return;

        element.removeEventListener('compositionstart', element._onCompositionStart);
        element.removeEventListener('compositionend', element._onCompositionEnd);
        element._imeAttached = false;
    },

    /**
     * 容器級事件委派：為容器內所有 input/textarea 自動追蹤 IME 狀態
     * 在組字期間於 capture 階段攔截 input 事件，防止 Blazor 回寫值導致重複
     * compositionend 後手動觸發 input 事件讓 Blazor 取得最終值
     * 適用於表格等動態渲染大量 input 元素的場景
     * @param {HTMLElement} container - 容器元素
     */
    attachContainer: function (container) {
        if (!container || container._imeContainerAttached) return;

        container.addEventListener('compositionstart', function (e) {
            if (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA') {
                e.target._imeComposing = true;
            }
        });

        // 在 capture 階段攔截 input 事件：組字期間阻止事件到達 Blazor
        container.addEventListener('input', function (e) {
            if (e.target._imeComposing) {
                e.stopPropagation();
            }
        }, true);

        container.addEventListener('compositionend', function (e) {
            if (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA') {
                e.target._imeComposing = false;
                // 手動觸發 input 事件讓 Blazor 拿到最終值
                e.target.dispatchEvent(new Event('input', { bubbles: true }));
            }
        });
        container._imeContainerAttached = true;
    }
};
