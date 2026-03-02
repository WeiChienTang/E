/**
 * ERPCore2 Debug Helper
 * SuperAdmin 專屬的開發輔助快捷鍵系統
 *
 * 方式三（Shift+Alt+K）：切換 i18n Key 顯示模式，在表單欄位 Label 旁顯示推斷的資源鍵名稱
 * 方式四（Shift+Alt+P）：切換 PropertyName 顯示模式，在表單欄位 Label 旁顯示 C# 屬性名稱
 *
 * 透過 MainLayout.OnAfterRenderAsync 在確認 IsSuperAdmin 後才呼叫 initDebugShortcuts()
 * 一般使用者永遠不會啟用此模組。
 */
window.DebugHelper = {

    /**
     * 初始化 Debug 快捷鍵監聽器（只應在 SuperAdmin 登入後呼叫一次）
     */
    initDebugShortcuts: function () {
        if (window._debugShortcutsRegistered) return;
        window._debugShortcutsRegistered = true;

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
