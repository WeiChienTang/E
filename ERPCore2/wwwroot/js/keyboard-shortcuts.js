/**
 * 全域鍵盤快捷鍵管理
 * 支援可設定的快捷鍵組合，並提供快捷鍵擷取功能
 */
window.KeyboardShortcuts = {
    _dotNetHelper: null,

    // 目前生效的快捷鍵組合（由 initialize / configure 設定）
    _shortcuts: {
        pageSearch:   { modifier: 'alt', key: 's' },
        reportSearch: { modifier: 'alt', key: 'r' },
        stickyNotes:  { modifier: 'alt', key: 'n' },
        calendar:     { modifier: 'alt', key: 'c' },
        quickAction:  { modifier: 'alt', key: 'q' }
    },

    /**
     * 初始化快捷鍵監聽器
     * @param {object} dotNetHelper - MainLayout 的 DotNetObjectReference
     * @param {object} config - 快捷鍵設定，如 { pageSearch: 'Alt+S', ... }
     */
    initialize: function (dotNetHelper, config) {
        this._dotNetHelper = dotNetHelper;
        if (config) {
            this._applyConfig(config);
        }
        this._boundHandler = this._handleKeyDown.bind(this);
        document.addEventListener('keydown', this._boundHandler);
    },

    /**
     * 動態更新快捷鍵設定（儲存後立即套用，不需重整頁面）
     * @param {object} config - 快捷鍵設定，如 { pageSearch: 'Alt+G', ... }
     */
    configure: function (config) {
        if (config) {
            this._applyConfig(config);
        }
    },

    /**
     * 解析並套用設定物件到 _shortcuts
     */
    _applyConfig: function (config) {
        const actions = ['pageSearch', 'reportSearch', 'stickyNotes', 'calendar', 'quickAction'];
        for (const action of actions) {
            if (config[action]) {
                const parsed = this._parseCombo(config[action]);
                if (parsed) {
                    this._shortcuts[action] = parsed;
                }
            }
        }
    },

    /**
     * 解析 "Alt+S" → { modifier: 'alt', key: 's' }
     */
    _parseCombo: function (combo) {
        if (!combo) return null;
        const parts = combo.split('+').map(p => p.trim().toLowerCase());
        if (parts.length === 2) {
            const modifier = parts[0];
            const key = parts[1];
            if ((modifier === 'alt' || modifier === 'ctrl') && key.length === 1) {
                return { modifier, key };
            }
        }
        return null;
    },

    /**
     * 比對鍵盤事件是否符合指定的快捷鍵
     */
    _matchesShortcut: function (event, shortcut) {
        const isAlt  = shortcut.modifier === 'alt'  && event.altKey  && !event.ctrlKey;
        const isCtrl = shortcut.modifier === 'ctrl' && event.ctrlKey && !event.altKey;
        return (isAlt || isCtrl) && !event.shiftKey &&
               event.key.toLowerCase() === shortcut.key;
    },

    /**
     * 處理鍵盤按下事件
     */
    _handleKeyDown: function (event) {
        // 在輸入框中不觸發
        if (this._isInputElement(event.target)) return;
        // 有 Modal 開啟時不觸發
        if (this._hasOpenModal()) return;

        const s = this._shortcuts;

        if (this._matchesShortcut(event, s.pageSearch)) {
            event.preventDefault(); event.stopPropagation();
            this._dotNetHelper?.invokeMethodAsync('OpenPageSearch')
                .catch(e => console.error('[KeyboardShortcuts] OpenPageSearch 失敗:', e));

        } else if (this._matchesShortcut(event, s.reportSearch)) {
            event.preventDefault(); event.stopPropagation();
            this._dotNetHelper?.invokeMethodAsync('OpenReportSearch')
                .catch(e => console.error('[KeyboardShortcuts] OpenReportSearch 失敗:', e));

        } else if (this._matchesShortcut(event, s.stickyNotes)) {
            event.preventDefault(); event.stopPropagation();
            this._dotNetHelper?.invokeMethodAsync('OpenStickyNotes')
                .catch(e => console.error('[KeyboardShortcuts] OpenStickyNotes 失敗:', e));

        } else if (this._matchesShortcut(event, s.calendar)) {
            event.preventDefault(); event.stopPropagation();
            this._dotNetHelper?.invokeMethodAsync('OpenCalendar')
                .catch(e => console.error('[KeyboardShortcuts] OpenCalendar 失敗:', e));

        } else if (this._matchesShortcut(event, s.quickAction)) {
            event.preventDefault(); event.stopPropagation();
            // QuickActionMenu 管理自身狀態，直接點擊主按鈕
            document.querySelector('.quick-action-main-btn')?.click();
        }
    },

    /**
     * 清理資源並移除事件監聽器
     */
    dispose: function () {
        if (this._boundHandler) {
            document.removeEventListener('keydown', this._boundHandler);
            this._boundHandler = null;
        }
        this._dotNetHelper = null;
        cancelShortcutCapture();
    },

    // ===== 內部工具方法 =====

    _isInputElement: function (element) {
        if (!element) return false;
        const tag = element.tagName.toLowerCase();
        if (tag === 'input' || tag === 'textarea' || tag === 'select') return true;
        if (element.contentEditable === 'true' || element.isContentEditable) return true;
        return false;
    },

    _hasOpenModal: function () {
        if (document.body.classList.contains('modal-open')) return true;
        const modalWithShow = document.querySelector('.modal.show');
        if (modalWithShow) {
            const style = window.getComputedStyle(modalWithShow);
            if (style.display === 'block' || style.display === 'flex') return true;
        }
        const allModals = document.querySelectorAll('.modal');
        for (let modal of allModals) {
            const style = window.getComputedStyle(modal);
            if (style.display !== 'none' && style.visibility !== 'hidden') return true;
        }
        const backdrop = document.querySelector('.modal-backdrop.show');
        if (backdrop) {
            const style = window.getComputedStyle(backdrop);
            if (style.display !== 'none') return true;
        }
        return false;
    }
};

// ===== 快捷鍵擷取（供 ShortcutKeysTab 使用） =====

/**
 * 進入快捷鍵擷取模式：下一次 Alt/Ctrl + 字母 將回傳給 C#
 * @param {object} dotNetRef  - ShortcutKeysTab 的 DotNetObjectReference
 * @param {string} method     - 要呼叫的 [JSInvokable] 方法名稱
 * @param {string} actionId   - 傳回給 C# 的動作識別碼
 */
window.startShortcutCapture = function (dotNetRef, method, actionId) {
    cancelShortcutCapture(); // 先取消任何進行中的擷取

    window._shortcutCaptureHandler = function (event) {
        // Escape = 取消
        if (event.key === 'Escape') {
            event.preventDefault();
            event.stopPropagation();
            cancelShortcutCapture();
            dotNetRef.invokeMethodAsync(method, null, actionId);
            return;
        }

        // 只擷取 Alt+字母 或 Ctrl+字母
        if ((event.altKey || event.ctrlKey) && !event.shiftKey) {
            const key = event.key;
            if (key.length === 1 && /^[a-zA-Z]$/.test(key)) {
                event.preventDefault();
                event.stopPropagation();
                const modifier = event.altKey ? 'Alt' : 'Ctrl';
                const combo = `${modifier}+${key.toUpperCase()}`;
                cancelShortcutCapture();
                dotNetRef.invokeMethodAsync(method, combo, actionId);
            }
        }
    };

    document.addEventListener('keydown', window._shortcutCaptureHandler, true);
};

/**
 * 取消快捷鍵擷取模式
 */
window.cancelShortcutCapture = function () {
    if (window._shortcutCaptureHandler) {
        document.removeEventListener('keydown', window._shortcutCaptureHandler, true);
        window._shortcutCaptureHandler = null;
    }
};
