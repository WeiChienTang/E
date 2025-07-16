// Modal 相關的 JavaScript 函數

window.showModal = function (modalId) {
    var modal = document.getElementById(modalId);
    if (modal) {
        // 使用 Bootstrap 5 的 Modal API
        var bootstrapModal = new bootstrap.Modal(modal);
        bootstrapModal.show();
    }
};

window.hideModal = function (modalId) {
    var modal = document.getElementById(modalId);
    if (modal) {
        // 使用 Bootstrap 5 的 Modal API
        var bootstrapModal = bootstrap.Modal.getInstance(modal);
        if (bootstrapModal) {
            bootstrapModal.hide();
        }
    }
};

// 當 modal 關閉時清理
window.addEventListener('DOMContentLoaded', function () {
    document.addEventListener('hidden.bs.modal', function (event) {
        // 確保 modal backdrop 被移除
        var backdrops = document.querySelectorAll('.modal-backdrop');
        backdrops.forEach(function (backdrop) {
            backdrop.remove();
        });
        
        // 移除 body 的 modal-open class
        document.body.classList.remove('modal-open');
        document.body.style.removeProperty('overflow');
        document.body.style.removeProperty('padding-right');
    });
});
