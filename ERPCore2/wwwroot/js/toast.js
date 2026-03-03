// Toast 通知系統
class ToastManager {
    constructor() {
        this.container = null;
        this.maxToasts = 3;
        this.initContainer();
    }

    // 各類型自動消失延遲（ms）。0 = 不自動消失
    getDelay(type) {
        return { success: 2500, info: 3500, warning: 5000, error: 0 }[type] ?? 3500;
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

        // 滑入動畫，並在動畫開始後啟動進度條與計時器
        setTimeout(() => {
            toast.classList.add('slide-in');

            if (delay > 0) {
                const progress = toast.querySelector('.toast-progress');
                if (progress) {
                    progress.style.animationDuration = delay + 'ms';
                    progress.style.animationPlayState = 'running';
                }
                toast.remainingTime = delay;
                toast.startTime = Date.now();
                toast.autoHideTimeout = setTimeout(() => this.hide(toast), delay);
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

        const isError = (type === 'error');
        // Warning 才有展開/收合功能（訊息足夠長才有意義）
        const hasExpandSection = isError || (type === 'warning' && message.length > 20);

        const toast = document.createElement('div');
        toast.id = toastId;
        toast.className = `toast-slide toast-slide-${type}`;
        toast.setAttribute('role', 'alert');
        toast.setAttribute('aria-live', isError ? 'assertive' : 'polite');
        toast.setAttribute('aria-atomic', 'true');

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
                    ${(hasExpandSection && !isError) ? `
                    <button type="button" class="toast-slide-expand" aria-label="展開" onclick="toastManager.toggleExpand('${toastId}')">
                        <i class="fas fa-chevron-left"></i>
                    </button>` : ''}
                    ${(type !== 'success') ? `
                    <button type="button" class="toast-slide-close" aria-label="關閉" onclick="toastManager.hide(document.getElementById('${toastId}'))">
                        <i class="fas fa-times"></i>
                    </button>` : ''}
                </div>
            </div>
            ${hasExpandSection ? `
            <div class="toast-slide-expanded">
                <div class="toast-slide-expanded-content">
                    ${title ? `<div class="toast-slide-expanded-title">${title}</div>` : ''}
                    <div class="toast-slide-expanded-message">${message}</div>
                </div>
                <div class="toast-slide-expanded-actions">
                    ${!isError ? `
                    <button type="button" class="toast-slide-collapse" aria-label="收合" onclick="toastManager.toggleExpand('${toastId}')">
                        <i class="fas fa-chevron-right"></i>
                    </button>` : ''}
                    <button type="button" class="toast-slide-close" aria-label="關閉" onclick="toastManager.hide(document.getElementById('${toastId}'))">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
            </div>` : ''}
            ${delay > 0 ? '<div class="toast-progress"></div>' : ''}
        `;

        // Error：預設展開
        if (isError) {
            toast.classList.add('expanded');
        }

        // 有倒計時的類型：滑鼠移入暫停，移出繼續
        if (delay > 0) {
            toast.addEventListener('mouseenter', () => {
                if (toast.autoHideTimeout) {
                    const elapsed = Date.now() - (toast.startTime || Date.now());
                    toast.remainingTime = Math.max(500, toast.remainingTime - elapsed);
                    clearTimeout(toast.autoHideTimeout);
                    toast.autoHideTimeout = null;
                }
                const progress = toast.querySelector('.toast-progress');
                if (progress) progress.style.animationPlayState = 'paused';
            });

            toast.addEventListener('mouseleave', () => {
                if (!toast.classList.contains('expanded')) {
                    const progress = toast.querySelector('.toast-progress');
                    if (progress) progress.style.animationPlayState = 'running';
                    toast.startTime = Date.now();
                    toast.autoHideTimeout = setTimeout(() => this.hide(toast), toast.remainingTime);
                }
            });
        }

        return toast;
    }

    hide(toast) {
        if (toast && toast.parentElement) {
            if (toast.autoHideTimeout) {
                clearTimeout(toast.autoHideTimeout);
            }
            toast.classList.remove('slide-in');
            toast.classList.add('slide-out');
            setTimeout(() => {
                if (toast.parentElement) {
                    toast.parentElement.removeChild(toast);
                }
            }, 280);
        }
    }

    toggleExpand(toastId) {
        const toast = document.getElementById(toastId);
        if (!toast) return;

        const isExpanded = toast.classList.contains('expanded');
        const progress = toast.querySelector('.toast-progress');

        if (isExpanded) {
            // 收合：繼續計時
            toast.classList.remove('expanded');
            if (toast.remainingTime > 0) {
                if (progress) progress.style.animationPlayState = 'running';
                toast.startTime = Date.now();
                toast.autoHideTimeout = setTimeout(() => this.hide(toast), toast.remainingTime);
            }
        } else {
            // 展開：暫停計時
            toast.classList.add('expanded');
            if (toast.autoHideTimeout) {
                const elapsed = Date.now() - (toast.startTime || Date.now());
                toast.remainingTime = Math.max(500, (toast.remainingTime || 0) - elapsed);
                clearTimeout(toast.autoHideTimeout);
                toast.autoHideTimeout = null;
            }
            if (progress) progress.style.animationPlayState = 'paused';
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
