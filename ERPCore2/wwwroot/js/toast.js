// Toast 通知系統
// 統一行為：所有類型皆有進度條 + Hover 展開暫停 + 複製訊息 + 關閉按鈕

class ToastManager {
    constructor() {
        this.container = null;
        this.maxToasts = 3;
        this.canHover = window.matchMedia('(hover: hover)').matches;
        // 各類型自動消失延遲（ms）— 預設 2 秒，可由使用者自訂
        this.delays = { success: 2000, error: 2000, warning: 2000, info: 2000 };
        this.initContainer();
    }

    // 各類型自動消失延遲（ms）— 讀取使用者自訂值
    getDelay(type) {
        return this.delays[type] ?? 2000;
    }

    // 設定各類型的自動消失延遲（由 Blazor interop 呼叫）
    setDelays(delays) {
        if (delays) {
            if (delays.success != null) this.delays.success = delays.success;
            if (delays.error   != null) this.delays.error   = delays.error;
            if (delays.warning != null) this.delays.warning = delays.warning;
            if (delays.info    != null) this.delays.info    = delays.info;
        }
    }

    initContainer() {
        const ensureContainer = () => {
            this.container = document.getElementById('toast-container');
            if (!this.container) {
                this.container = document.createElement('div');
                this.container.id = 'toast-container';
                this.container.className = 'toast-container-slide position-fixed';
                this.container.style.cssText = 'z-index:9999;bottom:20px;right:20px;width:320px;pointer-events:none;';
                document.body.appendChild(this.container);
            }
        };

        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', ensureContainer);
        } else {
            ensureContainer();
        }
    }

    show(type, message, title) {
        if (!this.container || !document.body.contains(this.container)) {
            this.initContainer();
        }

        this.limitToasts();

        const delay = this.getDelay(type);
        const toast = this.createToast(type, message, title, delay);
        this.container.appendChild(toast);

        // 滑入動畫 → 啟動進度條 + 計時器
        setTimeout(() => {
            toast.classList.add('slide-in');

            // delay=0 代表不自動關閉
            if (delay > 0) {
                const progress = toast.querySelector('.toast-progress');
                if (progress) {
                    progress.style.animationDuration = delay + 'ms';
                    progress.style.animationPlayState = 'running';
                }
                toast.remainingTime = delay;
                toast.startTime = Date.now();
                toast.autoHideTimeout = setTimeout(() => this.hide(toast), delay);
            } else {
                // 不自動關閉：隱藏進度條
                const progress = toast.querySelector('.toast-progress');
                if (progress) progress.style.display = 'none';
                toast.remainingTime = 0;
            }
        }, 50);

        return toast;
    }

    limitToasts() {
        const toasts = this.container.querySelectorAll('.toast-slide');
        if (toasts.length >= this.maxToasts) {
            const toRemove = toasts.length - this.maxToasts + 1;
            for (let i = 0; i < toRemove; i++) {
                this.hide(toasts[i]);
            }
        }
    }

    createToast(type, message, title, delay) {
        const toastId = 'toast-' + Date.now() + '-' + Math.random().toString(36).substr(2, 9);

        const iconMap = {
            success: 'fas fa-check-circle',
            error:   'fas fa-times-circle',
            warning: 'fas fa-exclamation-triangle',
            info:    'fas fa-info-circle'
        };

        const toast = document.createElement('div');
        toast.id = toastId;
        toast.className = `toast-slide toast-slide-${type}`;
        toast.setAttribute('role', 'alert');
        toast.setAttribute('aria-live', type === 'error' ? 'assertive' : 'polite');
        toast.setAttribute('aria-atomic', 'true');

        // 儲存原始訊息供複製
        toast._rawTitle = title || '';
        toast._rawMessage = message || '';

        toast.innerHTML = `
            <div class="toast-slide-compact">
                <div class="toast-slide-icon">
                    <i class="${iconMap[type]}"></i>
                </div>
                <div class="toast-slide-content">
                    ${title ? `<div class="toast-slide-title">${title}</div>` : ''}
                    <div class="toast-slide-message">${message}</div>
                </div>
                <div class="toast-slide-actions">
                    <button type="button" class="toast-slide-close" aria-label="關閉">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
            </div>
            <div class="toast-slide-expanded">
                <div class="toast-slide-expanded-content">
                    ${title ? `<div class="toast-slide-expanded-title">${title}</div>` : ''}
                    <div class="toast-slide-expanded-message">${message}</div>
                </div>
                <div class="toast-slide-expanded-actions">
                    <button type="button" class="toast-copy-btn">
                        <i class="fas fa-copy"></i> 複製訊息
                    </button>
                    <button type="button" class="toast-expanded-close-btn">
                        <i class="fas fa-times"></i> 關閉
                    </button>
                </div>
            </div>
            <div class="toast-progress"></div>
        `;

        // ── 綁定關閉按鈕 ──
        toast.querySelectorAll('.toast-slide-close, .toast-expanded-close-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.stopPropagation();
                this.hide(toast);
            });
        });

        // ── 綁定複製按鈕 ──
        const copyBtn = toast.querySelector('.toast-copy-btn');
        if (copyBtn) {
            copyBtn.addEventListener('click', (e) => {
                e.stopPropagation();
                this.copyMessage(toast, copyBtn);
            });
        }

        // ── 桌面：Hover 展開 + 暫停計時 ──
        toast.addEventListener('mouseenter', () => {
            this.expandToast(toast);
        });

        toast.addEventListener('mouseleave', () => {
            this.collapseToast(toast);
        });

        // ── 行動裝置：點擊展開/收合（桌面用 hover，不用 click）──
        toast.addEventListener('click', (e) => {
            if (e.target.closest('button')) return;
            if (this.canHover) return;
            if (toast.classList.contains('expanded')) {
                this.collapseToast(toast);
            } else {
                this.expandToast(toast);
            }
        });

        return toast;
    }

    // ── 展開 toast：暫停計時 + 進度條 ──
    expandToast(toast) {
        toast.classList.add('expanded');
        if (toast.autoHideTimeout) {
            const elapsed = Date.now() - (toast.startTime || Date.now());
            toast.remainingTime = Math.max(500, (toast.remainingTime || 0) - elapsed);
            clearTimeout(toast.autoHideTimeout);
            toast.autoHideTimeout = null;
        }
        const progress = toast.querySelector('.toast-progress');
        if (progress) progress.style.animationPlayState = 'paused';
    }

    // ── 收合 toast：繼續計時 + 進度條 ──
    collapseToast(toast) {
        toast.classList.remove('expanded');
        if (toast.remainingTime > 0) {
            const progress = toast.querySelector('.toast-progress');
            if (progress) progress.style.animationPlayState = 'running';
            toast.startTime = Date.now();
            toast.autoHideTimeout = setTimeout(() => this.hide(toast), toast.remainingTime);
        }
    }

    // ── 複製訊息到剪貼簿 ──
    copyMessage(toast, btn) {
        // 組合標題+訊息，移除 HTML 標籤
        const tmp = document.createElement('div');
        let raw = '';
        if (toast._rawTitle) raw += toast._rawTitle + '\n';
        raw += toast._rawMessage;
        tmp.innerHTML = raw;
        const text = tmp.textContent || tmp.innerText || '';

        const onCopied = () => {
            btn.classList.add('copied');
            btn.innerHTML = '<i class="fas fa-check"></i> 已複製';
            setTimeout(() => {
                btn.classList.remove('copied');
                btn.innerHTML = '<i class="fas fa-copy"></i> 複製訊息';
            }, 1500);
        };

        if (navigator.clipboard && navigator.clipboard.writeText) {
            navigator.clipboard.writeText(text).then(onCopied).catch(() => {
                this.fallbackCopy(text);
                onCopied();
            });
        } else {
            this.fallbackCopy(text);
            onCopied();
        }
    }

    // ── 剪貼簿 fallback（舊瀏覽器）──
    fallbackCopy(text) {
        const textarea = document.createElement('textarea');
        textarea.value = text;
        textarea.style.cssText = 'position:fixed;opacity:0;pointer-events:none;';
        document.body.appendChild(textarea);
        textarea.select();
        try { document.execCommand('copy'); } catch (_) { /* ignore */ }
        document.body.removeChild(textarea);
    }

    hide(toast) {
        if (toast && toast.parentElement) {
            if (toast.autoHideTimeout) {
                clearTimeout(toast.autoHideTimeout);
            }
            toast.classList.remove('slide-in', 'expanded');
            toast.classList.add('slide-out');
            setTimeout(() => {
                if (toast.parentElement) {
                    toast.parentElement.removeChild(toast);
                }
            }, 280);
        }
    }

    // 保留相容性（舊版透過 onclick 呼叫）
    toggleExpand(toastId) {
        const toast = document.getElementById(toastId);
        if (!toast) return;
        if (toast.classList.contains('expanded')) {
            this.collapseToast(toast);
        } else {
            this.expandToast(toast);
        }
    }

    clearAll() {
        if (this.container) {
            this.container.querySelectorAll('.toast-slide').forEach(t => this.hide(t));
        }
    }

    setMaxToasts(max) {
        this.maxToasts = max;
        this.limitToasts();
    }
}

// 全域實例
window.toastManager = new ToastManager();

// 全域函數
window.showToast = (type, message, title) => toastManager.show(type, message, title);
window.clearAllToasts = () => toastManager.clearAll();
window.setMaxToasts = (max) => toastManager.setMaxToasts(max);

// 相容性函數（保留舊呼叫介面）
window.showSuccess = (message, title = '成功') => showToast('success', message, title);
window.showError   = (message, title = '錯誤') => showToast('error',   message, title);
window.showWarning = (message, title = '警告') => showToast('warning', message, title);
window.showInfo    = (message, title = '資訊') => showToast('info',    message, title);

// 設定各類型 Toast 延遲（由偏好設定同步）
window.setToastDelays = (delays) => toastManager.setDelays(delays);

// 已由 per-type 延遲取代，保留為相容性存根
window.setToastAutoHideDelay = (_delay) => {};
window.setToastCollapseDelay = (_delay) => {};

// ── 表單驗證輔助（與 toast 共用此檔）──
window.validateRequiredFields = function () {
    const requiredFields = document.querySelectorAll('input[required], select[required], textarea[required]');
    const emptyFields = [];

    for (let field of requiredFields) {
        const label = document.querySelector(`label[for="${field.id}"]`) ||
                      field.closest('.form-group, .mb-3')?.querySelector('label');
        const fieldName = label ? label.textContent.replace('*', '').trim() : field.name || field.id || '欄位';

        if (!field.value || field.value.trim() === '') {
            emptyFields.push(fieldName);
            field.classList.add('is-invalid');

            const existingError = field.nextElementSibling;
            if (existingError && existingError.classList.contains('invalid-feedback')) {
                existingError.remove();
            }
            const errorDiv = document.createElement('div');
            errorDiv.className = 'invalid-feedback';
            errorDiv.textContent = `${fieldName} 為必填欄位`;
            field.parentNode.insertBefore(errorDiv, field.nextSibling);
        } else {
            field.classList.remove('is-invalid');
            const errorDiv = field.nextElementSibling;
            if (errorDiv && errorDiv.classList.contains('invalid-feedback')) {
                errorDiv.remove();
            }
        }
    }

    return emptyFields.length > 0 ? `請填寫以下必填欄位：\n${emptyFields.join('、')}` : null;
};

window.clearValidationStyles = function () {
    document.querySelectorAll('.is-invalid').forEach(f => f.classList.remove('is-invalid'));
    document.querySelectorAll('.invalid-feedback').forEach(m => m.remove());
};
