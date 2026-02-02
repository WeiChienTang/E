// Bootstrap Modal Helper Functions for Blazor
window.bootstrapHelpers = {
    showModal: function (modalId) {
        try {
            console.log('Attempting to show modal:', modalId);
            
            // 檢查 Bootstrap 是否已載入
            if (typeof bootstrap === 'undefined') {
                console.error('Bootstrap is not loaded');
                return false;
            }
            
            const modalElement = document.getElementById(modalId);
            if (!modalElement) {
                console.error('Modal element not found:', modalId);
                return false;
            }
            
            console.log('Modal element found, creating Bootstrap modal instance');
            
            // 檢查是否已經有現有的模態實例
            let modal = bootstrap.Modal.getInstance(modalElement);
            
            if (!modal) {
                modal = new bootstrap.Modal(modalElement, {
                    backdrop: 'static',
                    keyboard: false
                });
            }
            
            modal.show();
            console.log('Modal shown successfully');
            return true;
            
        } catch (error) {
            console.error('Error showing modal:', error);
            return false;
        }
    },

    hideModal: function (modalId) {
        try {
            console.log('Attempting to hide modal:', modalId);
            
            if (typeof bootstrap === 'undefined') {
                console.error('Bootstrap is not loaded');
                return false;
            }
            
            const modalElement = document.getElementById(modalId);
            if (!modalElement) {
                console.error('Modal element not found:', modalId);
                return false;
            }
            
            const modal = bootstrap.Modal.getInstance(modalElement);
            if (modal) {
                modal.hide();
                console.log('Modal hidden successfully');
            } else {
                console.warn('No modal instance found for:', modalId);
            }
            
            return true;
            
        } catch (error) {
            console.error('Error hiding modal:', error);
            return false;
        }
    },

    isBootstrapLoaded: function () {
        const isLoaded = typeof bootstrap !== 'undefined';
        console.log('Bootstrap loaded:', isLoaded);
        return isLoaded;
    },
    
    // 新增調試函數
    debugModal: function (modalId) {        console.log('=== Modal Debug Info ===');
        console.log('Modal ID:', modalId);
        console.log('Bootstrap loaded:', typeof bootstrap !== 'undefined');
        
        const modalElement = document.getElementById(modalId);
        console.log('Modal element exists:', !!modalElement);
        
        if (modalElement) {
            console.log('Modal element:', modalElement);
            const instance = bootstrap?.Modal?.getInstance(modalElement);
            console.log('Existing instance:', !!instance);
        }
        
        console.log('========================');
    }
};

// 自動完成滾動輔助函式
window.scrollToElement = function (elementId) {
    try {
        const element = document.getElementById(elementId);
        if (element) {
            element.scrollIntoView({
                behavior: 'smooth',
                block: 'nearest',
                inline: 'nearest'
            });
            return true;
        }
        return false;
    } catch (error) {
        console.error('Error scrolling to element:', error);
        return false;
    }
};

// Popover 輔助函式
window.popoverHelpers = {
    // 追蹤是否已綁定全域點擊事件
    _documentClickBound: false,

    // 綁定全域點擊事件，點擊外部時關閉所有 popover
    _bindDocumentClick: function () {
        if (this._documentClickBound) return;
        
        document.addEventListener('click', function (e) {
            // 檢查點擊的元素是否在 popover 內部或是 popover 觸發按鈕
            const isPopoverTrigger = e.target.closest('[data-bs-toggle="popover"]');
            const isInsidePopover = e.target.closest('.popover');
            
            if (!isPopoverTrigger && !isInsidePopover) {
                // 點擊在 popover 外部，關閉所有 popover
                document.querySelectorAll('[data-bs-toggle="popover"]').forEach(function (el) {
                    const instance = bootstrap.Popover.getInstance(el);
                    if (instance) {
                        instance.hide();
                    }
                });
            }
        });
        
        this._documentClickBound = true;
    },

    // 初始化頁面上所有的 popover
    initializeAll: function () {
        try {
            const popoverTriggerList = document.querySelectorAll('[data-bs-toggle="popover"]');
            popoverTriggerList.forEach(function (popoverTriggerEl) {
                // 檢查是否已經初始化
                if (!bootstrap.Popover.getInstance(popoverTriggerEl)) {
                    new bootstrap.Popover(popoverTriggerEl, {
                        html: true,
                        trigger: 'click',
                        placement: 'right',
                        sanitize: false
                    });
                }
            });
            
            // 綁定全域點擊事件
            this._bindDocumentClick();
            
            console.log('Popovers initialized:', popoverTriggerList.length);
            return true;
        } catch (error) {
            console.error('Error initializing popovers:', error);
            return false;
        }
    },

    // 初始化特定容器內的 popover
    initializeInContainer: function (containerId) {
        try {
            const container = document.getElementById(containerId);
            if (!container) {
                console.warn('Container not found:', containerId);
                return false;
            }

            const popoverTriggerList = container.querySelectorAll('[data-bs-toggle="popover"]');
            popoverTriggerList.forEach(function (popoverTriggerEl) {
                if (!bootstrap.Popover.getInstance(popoverTriggerEl)) {
                    new bootstrap.Popover(popoverTriggerEl, {
                        html: true,
                        trigger: 'click',
                        placement: 'right',
                        sanitize: false
                    });
                }
            });
            
            // 綁定全域點擊事件
            this._bindDocumentClick();
            
            console.log('Popovers initialized in container:', popoverTriggerList.length);
            return true;
        } catch (error) {
            console.error('Error initializing popovers in container:', error);
            return false;
        }
    },

    // 銷毀所有 popover
    disposeAll: function () {
        try {
            const popoverTriggerList = document.querySelectorAll('[data-bs-toggle="popover"]');
            popoverTriggerList.forEach(function (popoverTriggerEl) {
                const instance = bootstrap.Popover.getInstance(popoverTriggerEl);
                if (instance) {
                    instance.dispose();
                }
            });
            return true;
        } catch (error) {
            console.error('Error disposing popovers:', error);
            return false;
        }
    }
};

/**
 * 取得元素的邊界矩形資訊（供 Blazor AutoComplete 下拉選單定位使用）
 * @param {HTMLElement} element - DOM 元素
 * @returns {Object} 包含 top, left, bottom, right, width, height 的物件
 */
window.getElementBoundingRect = function (element) {
    if (!element) {
        return null;
    }
    
    try {
        const rect = element.getBoundingClientRect();
        return {
            top: rect.top,
            left: rect.left,
            bottom: rect.bottom,
            right: rect.right,
            width: rect.width,
            height: rect.height
        };
    } catch (error) {
        console.error('Error getting element bounding rect:', error);
        return null;
    }
};