/**
 * ERPCore2 Debug Helper
 * SuperAdmin 專屬的開發輔助快捷鍵系統
 *
 * 方式一：Modal 底部顯示組件名稱（點擊複製）
 * 方式二：頁面下方置中 Badge 顯示頁面名稱（點擊複製）
 * 方式三（Shift+Alt+K）：切換 i18n Key 顯示模式，在表單欄位 Label 旁顯示推斷的資源鍵名稱（點擊複製）
 * 方式四（Shift+Alt+P）：切換 PropertyName 顯示模式，在表單欄位 Label 旁顯示 C# 屬性名稱（點擊複製）
 * 方式五：InteractiveTableComponent 欄位標頭 Debug 標記（與方式三/四共用快捷鍵，同步顯示）（點擊複製）
 *
 * 透過 MainLayout.OnAfterRenderAsync 在確認 IsSuperAdmin 後才呼叫 initDebugShortcuts()
 * 一般使用者永遠不會啟用此模組。
 */
window.DebugHelper = {

    /**
     * 初始化 Debug 快捷鍵監聽器（只應在 SuperAdmin 登入後呼叫一次）
     * 同時掛載方式三/四/五欄位 span 與表格 Badge 的點擊複製監聽（capture phase）
     */
    initDebugShortcuts: function () {
        if (window._debugShortcutsRegistered) return;
        window._debugShortcutsRegistered = true;

        // 鍵盤快捷鍵
        document.addEventListener('keydown', function (e) {
            if (!e.shiftKey || !e.altKey) return;

            // Shift+Alt+P：切換 PropertyName 顯示（方式四）
            if (e.key === 'P') {
                e.preventDefault();
                const html = document.documentElement;
                html.classList.toggle('debug-props');
                const on = html.classList.contains('debug-props');
                DebugHelper._notify(
                    '方式四：PropertyName ' + (on ? 'ON  (Shift+Alt+P 關閉)' : 'OFF'),
                    '#f8c8a0'
                );
            }

            // Shift+Alt+K：切換 i18n Key 顯示（方式三）
            if (e.key === 'K') {
                e.preventDefault();
                const html = document.documentElement;
                html.classList.toggle('debug-i18n');
                const on = html.classList.contains('debug-i18n');
                DebugHelper._notify(
                    '方式三：i18n Key ' + (on ? 'ON  (Shift+Alt+K 關閉)' : 'OFF'),
                    '#90ee90'
                );
            }
        });

        // 方式三/四/五：點擊欄位/表頭 Debug span 即複製（capture phase 攔截，避免觸發 label focus）
        document.addEventListener('click', function (e) {
            const html = document.documentElement;
            const target = e.target;

            // 方式四 / 方式五(props)：PropertyName 欄位標籤 + 表格欄頭
            if ((target.classList.contains('field-debug-name') || target.classList.contains('col-debug-name'))
                && html.classList.contains('debug-props')) {
                e.stopPropagation();
                e.preventDefault();
                DebugHelper.copyText(target.textContent.trim(), '#f8c8a0');
                return;
            }

            // 方式三 / 方式五(i18n)：i18n Key 欄位標籤 + 表格欄頭
            if ((target.classList.contains('field-debug-i18n') || target.classList.contains('col-debug-i18n'))
                && html.classList.contains('debug-i18n')) {
                e.stopPropagation();
                e.preventDefault();
                DebugHelper.copyText(target.textContent.trim(), '#90ee90');
                return;
            }

            // 方式五：點擊表格名稱 Badge 複製
            if (target.classList.contains('table-debug-name')
                && (html.classList.contains('debug-props') || html.classList.contains('debug-i18n'))) {
                e.stopPropagation();
                e.preventDefault();
                DebugHelper.copyText(target.textContent.trim(), '#7ec8e3');
            }
        }, true); // true = capture phase，在 label 之前攔截
    },

    /**
     * 複製文字到剪貼簿並顯示確認通知
     * 供方式一/二的 Blazor @onclick 直接呼叫，以及方式三/四/五的 JS 內部呼叫
     * @param {string} text  - 要複製的文字
     * @param {string} color - 通知文字顏色（預設 #7ec8e3）
     */
    copyText: function (text, color) {
        if (!text) return;
        color = color || '#7ec8e3';
        navigator.clipboard.writeText(text).then(function () {
            DebugHelper._notify('📋 已複製：' + text, color);
        }).catch(function () {
            DebugHelper._notify('複製失敗（需要 HTTPS 或允許剪貼簿權限）', '#ff6b6b');
        });
    },

    /**
     * 在右下角顯示短暫的 Debug 狀態通知（2 秒後自動消失）
     * 注意：此通知在 z-index 9999，高於頁面 badge（999）但可能被 Modal（1050+）覆蓋
     */
    _notify: function (message, color) {
        const el = document.createElement('div');
        el.style.cssText = [
            'position:fixed',
            'bottom:3rem',
            'right:1rem',
            'z-index:9999',
            'background:#1a1a2e',
            'color:' + color,
            'font-size:0.7rem',
            'font-family:monospace',
            'padding:0.35rem 0.75rem',
            'border-radius:4px',
            'border:1px solid #30363d',
            'pointer-events:none',
            'opacity:1',
            'transition:opacity 0.4s ease',
            'letter-spacing:0.03em'
        ].join(';');
        el.textContent = '\uD83D\uDD27 ' + message;
        document.body.appendChild(el);
        setTimeout(function () {
            el.style.opacity = '0';
            setTimeout(function () { el.remove(); }, 450);
        }, 2000);
    }
};
