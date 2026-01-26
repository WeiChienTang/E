// 拖放功能輔助腳本

// 確保拖曳行為正確運作
window.initDragDrop = function () {
    // 監聽所有可拖曳的行
    document.querySelectorAll('tr[draggable="true"]').forEach(row => {
        // 避免重複綁定：檢查是否已經初始化
        if (row.dataset.dragInitialized === 'true') return;
        row.dataset.dragInitialized = 'true';
        
        // 確保 dragstart 事件正確觸發
        row.addEventListener('dragstart', function (e) {
            e.dataTransfer.effectAllowed = 'move';
            e.dataTransfer.setData('text/plain', ''); // 必須設定資料才能拖曳
            this.classList.add('dragging');
        });

        row.addEventListener('dragend', function (e) {
            this.classList.remove('dragging');
        });
    });
};

// 防止可拖曳元素的文字選取（雙擊時）
// 注意：此函數不會阻止單擊事件，只阻止雙擊選取文字
window.preventTextSelectionOnDrag = function () {
    document.querySelectorAll('tr[draggable="true"]').forEach(row => {
        // 避免重複綁定
        if (row.dataset.selectPrevented === 'true') return;
        row.dataset.selectPrevented = 'true';
        
        row.addEventListener('mousedown', function (e) {
            // 只阻止雙擊時的文字選取，不阻止單擊
            if (e.detail > 1) {
                e.preventDefault();
            }
        });

        row.addEventListener('selectstart', function (e) {
            // 阻止文字選取開始
            e.preventDefault();
            return false;
        });
    });
};

// 在 Blazor 組件更新後重新初始化
window.reinitDragDrop = function () {
    window.initDragDrop();
    window.preventTextSelectionOnDrag();
};

// 設定 checkbox 的 indeterminate 狀態（半選狀態）
// HTML 無法透過 attribute 設定此狀態，必須透過 JavaScript
window.setCheckboxIndeterminate = function (elementId, isIndeterminate) {
    const checkbox = document.getElementById(elementId);
    if (checkbox) {
        checkbox.indeterminate = isIndeterminate;
    }
};