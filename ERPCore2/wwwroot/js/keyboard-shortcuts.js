/**
 * 全域鍵盤快捷鍵管理
 * 處理系統級的快捷鍵，避免與輸入元素和 Modal 衝突
 */
window.KeyboardShortcuts = {
    dotNetHelper: null,

    /**
     * 初始化快捷鍵監聽器
     * @param {object} dotNetHelper - .NET 物件參考，用於呼叫 C# 方法
     */
    initialize: function (dotNetHelper) {
        this.dotNetHelper = dotNetHelper;
        this.handleKeyDown = this.handleKeyDown.bind(this);
        document.addEventListener('keydown', this.handleKeyDown);
        console.log('[KeyboardShortcuts] 已初始化，快捷鍵: Alt + S');
    },

    /**
     * 處理鍵盤按下事件
     * @param {KeyboardEvent} event - 鍵盤事件
     */
    handleKeyDown: function (event) {
        // 1. 檢查是否在輸入元素中 - 避免干擾使用者輸入
        if (this.isInputElement(event.target)) {
            return;
        }

        // 2. 檢查是否有 Modal 已開啟 - 避免多重 Modal 衝突
        if (this.hasOpenModal()) {
            return;
        }

        // 3. 檢查 Alt + S 快捷鍵 (不含 Ctrl 和 Shift)
        if (event.altKey && 
            (event.key === 's' || event.key === 'S') && 
            !event.ctrlKey && 
            !event.shiftKey) {
            
            // 阻止預設行為（避免瀏覽器的 Alt + S 行為）
            event.preventDefault();
            event.stopPropagation();

            // 呼叫 C# 方法開啟頁面搜尋
            if (this.dotNetHelper) {
                this.dotNetHelper.invokeMethodAsync('OpenPageSearch')
                    .catch(error => {
                        console.error('[KeyboardShortcuts] 開啟頁面搜尋失敗:', error);
                    });
            }
        }
    },

    /**
     * 檢查元素是否為輸入類型（避免干擾使用者輸入）
     * @param {HTMLElement} element - 要檢查的元素
     * @returns {boolean} 是否為輸入元素
     */
    isInputElement: function (element) {
        if (!element) return false;

        const tagName = element.tagName.toLowerCase();
        
        // 檢查標準輸入元素
        if (tagName === 'input' || tagName === 'textarea' || tagName === 'select') {
            return true;
        }

        // 檢查可編輯元素（contenteditable）
        if (element.contentEditable === 'true' || element.isContentEditable) {
            return true;
        }

        return false;
    },

    /**
     * 檢查是否有 Bootstrap Modal 已開啟
     * @returns {boolean} 是否有 Modal 開啟
     */
    hasOpenModal: function () {
        // 檢查 Bootstrap Modal 的開啟狀態
        return document.querySelector('.modal.show') !== null ||
               document.querySelector('.modal-backdrop.show') !== null;
    },

    /**
     * 清理資源並移除事件監聽器
     */
    dispose: function () {
        if (this.handleKeyDown) {
            document.removeEventListener('keydown', this.handleKeyDown);
        }
        this.dotNetHelper = null;
        console.log('[KeyboardShortcuts] 已清理資源');
    }
};
