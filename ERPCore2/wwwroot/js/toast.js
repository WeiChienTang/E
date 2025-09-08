// Toast 通知系統 - 創新滑入式設計
class ToastManager {
    constructor() {
        this.container = null;
        this.maxToasts = 3; // 最多同時顯示3個toast
        this.autoHideDelay = 3000; // 預設3秒自動關閉
        this.collapseDelay = 2000; // 收合後2秒關閉
        this.initContainer();
    }

    initContainer() {
        // 等待 DOM 準備好
        const ensureContainer = () => {
            // 檢查是否已存在容器
            this.container = document.getElementById('toast-container');
            if (!this.container) {
                this.container = document.createElement('div');
                this.container.id = 'toast-container';
                this.container.className = 'toast-container-slide position-fixed';
                this.container.style.zIndex = '9999';
                this.container.style.top = '50%';
                this.container.style.right = '20px';
                this.container.style.transform = 'translateY(-50%)';
                this.container.style.width = '320px';
                this.container.style.maxHeight = 'calc(100vh - 40px)';
                this.container.style.overflowY = 'visible'; // 改為 visible 避免拉桿
                this.container.style.pointerEvents = 'none'; // 讓容器本身不阻擋事件
                document.body.appendChild(this.container);
                console.log('Toast container created with slide animation');
            }
        };

        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', ensureContainer);
        } else {
            ensureContainer();
        }
    }

    show(type, message, title) {
        // 確保容器存在
        if (!this.container || !document.body.contains(this.container)) {
            this.initContainer();
        }

        // 檢查並移除超過數量限制的toast
        this.limitToasts();

        const toast = this.createToast(type, message, title);
        this.container.appendChild(toast);

        // 觸發滑入動畫
        setTimeout(() => {
            toast.classList.add('slide-in');
        }, 50);

        // 自動隱藏 (3秒後自動關閉)
        const autoHideTimeout = setTimeout(() => {
            this.hide(toast);
        }, this.autoHideDelay);

        // 儲存 timeout ID 以便取消
        toast.autoHideTimeout = autoHideTimeout;

        return toast;
    }

    limitToasts() {
        const existingToasts = this.container.querySelectorAll('.toast');
        if (existingToasts.length >= this.maxToasts) {
            // 移除最舊的toast（從最前面開始移除）
            const toastsToRemove = existingToasts.length - this.maxToasts + 1;
            for (let i = 0; i < toastsToRemove; i++) {
                this.hide(existingToasts[i]);
            }
        }
    }

    createToast(type, message, title) {
        const toastId = 'toast-' + Date.now() + '-' + Math.random().toString(36).substr(2, 9);
        
        const iconMap = {
            success: 'fas fa-check-circle',
            error: 'fas fa-times-circle',
            warning: 'fas fa-exclamation-triangle',
            info: 'fas fa-info-circle'
        };

        const colorMap = {
            success: 'success',
            error: 'danger',
            warning: 'warning',
            info: 'info'
        };

        const toast = document.createElement('div');
        toast.id = toastId;
        toast.className = `toast-slide toast-slide-${type}`;
        toast.setAttribute('role', 'alert');
        toast.setAttribute('aria-live', 'assertive');
        toast.setAttribute('aria-atomic', 'true');

        // 計算預覽文字（限制長度）
        const previewMessage = message.length > 30 ? message.substring(0, 30) + '...' : message;
        const previewTitle = title && title.length > 15 ? title.substring(0, 15) + '...' : title;

        toast.innerHTML = `
            <div class="toast-slide-compact" onclick="toastManager.toggleExpand('${toastId}')">
                <div class="toast-slide-icon">
                    <i class="${iconMap[type]}"></i>
                </div>
                <div class="toast-slide-content">
                    ${previewTitle ? `<div class="toast-slide-title">${previewTitle}</div>` : ''}
                    <div class="toast-slide-message">${previewMessage}</div>
                </div>
                <div class="toast-slide-actions" onclick="event.stopPropagation()">
                    <button type="button" class="toast-slide-expand" aria-label="展開" onclick="toastManager.toggleExpand('${toastId}')">
                        <i class="fas fa-chevron-left"></i>
                    </button>
                    <button type="button" class="toast-slide-close" aria-label="關閉" onclick="toastManager.hide(document.getElementById('${toastId}'))">
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
                    <button type="button" class="toast-slide-collapse" aria-label="收合" onclick="toastManager.toggleExpand('${toastId}')">
                        <i class="fas fa-chevron-right"></i>
                    </button>
                    <button type="button" class="toast-slide-close" aria-label="關閉" onclick="toastManager.hide(document.getElementById('${toastId}'))">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
            </div>
        `;

        // 儲存完整訊息資訊
        toast.dataset.fullMessage = message;
        toast.dataset.fullTitle = title || '';
        toast.dataset.type = type;

        return toast;
    }

    hide(toast) {
        if (toast && toast.parentElement) {
            // 取消自動隱藏 timeout
            if (toast.autoHideTimeout) {
                clearTimeout(toast.autoHideTimeout);
            }
            
            toast.classList.remove('slide-in');
            toast.classList.add('slide-out');
            
            setTimeout(() => {
                if (toast.parentElement) {
                    toast.parentElement.removeChild(toast);
                }
            }, 300);
        }
    }

    // 切換展開/收合狀態
    toggleExpand(toastId) {
        const toast = document.getElementById(toastId);
        if (!toast) return;

        const isExpanded = toast.classList.contains('expanded');
        
        if (isExpanded) {
            toast.classList.remove('expanded');
            // 重新啟動自動隱藏
            toast.autoHideTimeout = setTimeout(() => {
                this.hide(toast);
            }, this.collapseDelay); // 收合後2秒關閉
        } else {
            toast.classList.add('expanded');
            // 取消自動隱藏，讓用戶有時間閱讀
            if (toast.autoHideTimeout) {
                clearTimeout(toast.autoHideTimeout);
            }
        }
    }

    // 清除所有toast
    clearAll() {
        if (this.container) {
            const toasts = this.container.querySelectorAll('.toast');
            toasts.forEach(toast => this.hide(toast));
        }
    }

    // 設定最大toast數量
    setMaxToasts(max) {
        this.maxToasts = max;
        this.limitToasts(); // 立即應用新的限制
    }

    // 設定自動關閉時間 (毫秒)
    setAutoHideDelay(delay) {
        this.autoHideDelay = delay;
    }

    // 設定收合後關閉時間 (毫秒)
    setCollapseDelay(delay) {
        this.collapseDelay = delay;
    }
}

// 全域實例
window.toastManager = new ToastManager();

// 全域函數
window.showToast = function(type, message, title) {
    return toastManager.show(type, message, title);
};

// 清除所有toast
window.clearAllToasts = function() {
    toastManager.clearAll();
};

// 設定最大toast數量
window.setMaxToasts = function(max) {
    toastManager.setMaxToasts(max);
};

// 設定自動關閉時間
window.setToastAutoHideDelay = function(delay) {
    toastManager.setAutoHideDelay(delay);
};

// 設定收合後關閉時間
window.setToastCollapseDelay = function(delay) {
    toastManager.setCollapseDelay(delay);
};

// 相容性函數，逐步替換 alert
window.showSuccess = function(message, title = '成功') {
    return showToast('success', message, title);
};

window.showError = function(message, title = '錯誤') {
    return showToast('error', message, title);
};

window.showWarning = function(message, title = '警告') {
    return showToast('warning', message, title);
};

window.showInfo = function(message, title = '資訊') {
    return showToast('info', message, title);
};

// 驗證必填欄位
window.validateRequiredFields = function() {
    const requiredFields = document.querySelectorAll('input[required], select[required], textarea[required]');
    const emptyFields = [];
    
    for (let field of requiredFields) {
        const label = document.querySelector(`label[for="${field.id}"]`) || 
                     field.closest('.form-group, .mb-3')?.querySelector('label');
        const fieldName = label ? label.textContent.replace('*', '').trim() : field.name || field.id || '欄位';
        
        if (!field.value || field.value.trim() === '') {
            emptyFields.push(fieldName);
            
            // 添加視覺提示
            field.classList.add('is-invalid');
            
            // 移除之前的錯誤訊息
            const existingError = field.nextElementSibling;
            if (existingError && existingError.classList.contains('invalid-feedback')) {
                existingError.remove();
            }
            
            // 添加錯誤訊息
            const errorDiv = document.createElement('div');
            errorDiv.className = 'invalid-feedback';
            errorDiv.textContent = `${fieldName} 為必填欄位`;
            field.parentNode.insertBefore(errorDiv, field.nextSibling);
        } else {
            // 移除錯誤樣式
            field.classList.remove('is-invalid');
            const errorDiv = field.nextElementSibling;
            if (errorDiv && errorDiv.classList.contains('invalid-feedback')) {
                errorDiv.remove();
            }
        }
    }
    
    if (emptyFields.length > 0) {
        return `請填寫以下必填欄位：\n${emptyFields.join('、')}`;
    }
    
    return null;
};

// 清除驗證樣式
window.clearValidationStyles = function() {
    const invalidFields = document.querySelectorAll('.is-invalid');
    const errorMessages = document.querySelectorAll('.invalid-feedback');
    
    invalidFields.forEach(field => field.classList.remove('is-invalid'));
    errorMessages.forEach(msg => msg.remove());
};
