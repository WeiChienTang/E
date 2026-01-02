/**
 * InteractiveTableComponent 下拉選單動態定位腳本
 * 從 InteractiveTableComponent.razor 抽離出來的 JavaScript 功能
 */

// 使用 IIFE 避免重複初始化
(function () {
    // 檢查是否已初始化
    if (window.searchableDropdownInitialized) {
        return;
    }
    window.searchableDropdownInitialized = true;

    /**
     * 定位搜尋下拉選單
     * 自動調整下拉選單位置，避免超出視窗範圍
     */
    function positionSearchableDropdown() {
        const dropdowns = document.querySelectorAll('.searchable-dropdown.show');

        dropdowns.forEach(dropdown => {
            const input = dropdown.previousElementSibling;
            if (!input) return;

            const inputRect = input.getBoundingClientRect();
            const dropdownRect = dropdown.getBoundingClientRect();
            const viewportHeight = window.innerHeight;
            const viewportWidth = window.innerWidth;

            // 計算最佳位置
            let top = inputRect.bottom + window.scrollY;
            let left = inputRect.left + window.scrollX;

            // 檢查是否需要向上顯示（空間不足時）
            if (inputRect.bottom + dropdownRect.height > viewportHeight) {
                if (inputRect.top > dropdownRect.height) {
                    top = inputRect.top + window.scrollY - dropdownRect.height;
                }
            }

            // 檢查是否需要調整水平位置（避免超出螢幕）
            if (left + dropdownRect.width > viewportWidth) {
                left = viewportWidth - dropdownRect.width - 10;
                if (left < 10) left = 10;
            }

            // 應用計算出的位置
            dropdown.style.position = 'fixed';
            dropdown.style.top = (top - window.scrollY) + 'px';
            dropdown.style.left = (left - window.scrollX) + 'px';
            dropdown.style.width = Math.max(inputRect.width, 200) + 'px';
        });
    }

    // 當頁面載入完成或下拉選單顯示時執行定位
    document.addEventListener('DOMContentLoaded', positionSearchableDropdown);

    // 使用 MutationObserver 監聽下拉選單的顯示/隱藏
    const observer = new MutationObserver(function (mutations) {
        mutations.forEach(function (mutation) {
            if (mutation.type === 'attributes' && mutation.attributeName === 'class') {
                const target = mutation.target;
                if (target.classList.contains('searchable-dropdown') && target.classList.contains('show')) {
                    setTimeout(positionSearchableDropdown, 10);
                }
            } else if (mutation.type === 'childList') {
                const addedNodes = Array.from(mutation.addedNodes);
                addedNodes.forEach(node => {
                    if (node.nodeType === 1 && node.classList &&
                        node.classList.contains('searchable-dropdown') &&
                        node.classList.contains('show')) {
                        setTimeout(positionSearchableDropdown, 10);
                    }
                });
            }
        });
    });

    // 開始觀察整個文檔
    observer.observe(document.body, {
        childList: true,
        subtree: true,
        attributes: true,
        attributeFilter: ['class']
    });

    // 滾動和視窗大小變化時重新定位
    window.addEventListener('scroll', positionSearchableDropdown);
    window.addEventListener('resize', positionSearchableDropdown);
})();

/**
 * 將選中的下拉選單項目滾動到可視範圍內
 * @param {string} dropdownId - 下拉選單的 DOM ID
 * @param {string} itemId - 選中項目的 DOM ID
 */
window.scrollDropdownItemIntoView = function (dropdownId, itemId) {
    try {
        const dropdown = document.getElementById(dropdownId);
        const item = document.getElementById(itemId);

        if (!dropdown || !item) {
            return;
        }

        const dropdownRect = dropdown.getBoundingClientRect();
        const itemRect = item.getBoundingClientRect();

        // 計算項目相對於下拉容器的位置
        const itemTop = item.offsetTop;
        const itemBottom = itemTop + item.offsetHeight;
        const dropdownScrollTop = dropdown.scrollTop;
        const dropdownHeight = dropdown.clientHeight;

        // 檢查項目是否在可視範圍內
        const isItemVisible = itemTop >= dropdownScrollTop &&
            itemBottom <= dropdownScrollTop + dropdownHeight;

        if (!isItemVisible) {
            // 如果項目在可視範圍上方，滾動到項目頂部
            if (itemTop < dropdownScrollTop) {
                dropdown.scrollTop = itemTop;
            }
            // 如果項目在可視範圍下方，滾動到項目底部
            else if (itemBottom > dropdownScrollTop + dropdownHeight) {
                dropdown.scrollTop = itemBottom - dropdownHeight;
            }
        }
    } catch (error) {
        console.warn('Error scrolling dropdown item into view:', error);
    }
};
