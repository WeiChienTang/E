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
    debugModal: function (modalId) {
        console.log('=== Modal Debug Info ===');
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
