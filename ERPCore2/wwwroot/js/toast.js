// Toast 通知系統
class ToastManager {
    constructor() {
        this.container = null;
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
                this.container.className = 'toast-container position-fixed top-0 start-50 translate-middle-x p-3';
                this.container.style.zIndex = '9999';
                document.body.appendChild(this.container);
                console.log('Toast container created');
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

        const toast = this.createToast(type, message, title);
        this.container.appendChild(toast);

        // 觸發動畫
        setTimeout(() => {
            toast.classList.add('show');
        }, 100);

        // 自動隱藏
        setTimeout(() => {
            this.hide(toast);
        }, 5000);

        return toast;
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
            success: 'text-success',
            error: 'text-danger',
            warning: 'text-warning',
            info: 'text-info'
        };

        const toast = document.createElement('div');
        toast.id = toastId;
        toast.className = `toast align-items-center text-dark bg-light border-0 toast-${type}`;
        toast.setAttribute('role', 'alert');
        toast.setAttribute('aria-live', 'assertive');
        toast.setAttribute('aria-atomic', 'true');

        toast.innerHTML = `
            <div class="d-flex">
                <div class="toast-body d-flex align-items-center">
                    <i class="${iconMap[type]} ${colorMap[type]} me-2"></i>
                    <div>
                        ${title ? `<div class="fw-bold">${title}</div>` : ''}
                        <div>${message}</div>
                    </div>
                </div>
                <button type="button" class="btn-close me-2 m-auto" aria-label="Close" onclick="toastManager.hide(document.getElementById('${toastId}'))"></button>
            </div>
        `;

        return toast;
    }

    hide(toast) {
        if (toast && toast.parentElement) {
            toast.classList.remove('show');
            toast.classList.add('hide');
            
            setTimeout(() => {
                if (toast.parentElement) {
                    toast.parentElement.removeChild(toast);
                }
            }, 300);
        }
    }
}

// 全域實例
window.toastManager = new ToastManager();

// 全域函數
window.showToast = function(type, message, title) {
    return toastManager.show(type, message, title);
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
