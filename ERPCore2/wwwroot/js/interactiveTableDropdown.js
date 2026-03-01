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
     * 檢測是否為手機裝置
     */
    function isMobile() {
        return window.innerWidth <= 767;
    }

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
            const viewportHeight = window.innerHeight;
            const viewportWidth = window.innerWidth;

            // 強制設定 fixed 定位
            dropdown.style.setProperty('position', 'fixed', 'important');
            dropdown.style.setProperty('z-index', '99999', 'important');
            
            // 計算下拉選單寬度
            let dropdownWidth;
            if (isMobile()) {
                // 手機版：寬度適應螢幕，但不超過視窗寬度
                dropdownWidth = Math.min(viewportWidth - 20, 300);
            } else {
                // 桌面版：至少與輸入框同寬
                dropdownWidth = Math.max(inputRect.width, 250);
            }

            // 計算位置
            let top = inputRect.bottom + 2;
            let left = inputRect.left;

            // 計算可用空間
            const spaceBelow = viewportHeight - inputRect.bottom - 10;
            const spaceAbove = inputRect.top - 10;
            
            // 設定最大高度
            let maxHeight = isMobile() ? Math.min(200, spaceBelow) : Math.min(300, spaceBelow);
            
            // 如果下方空間不足，嘗試向上顯示
            if (spaceBelow < 100 && spaceAbove > spaceBelow) {
                maxHeight = Math.min(isMobile() ? 200 : 300, spaceAbove);
                top = inputRect.top - maxHeight - 2;
            }

            // 確保不超出螢幕頂部
            if (top < 10) {
                top = 10;
                maxHeight = inputRect.top - 20;
            }

            // 調整水平位置，避免超出螢幕
            if (left + dropdownWidth > viewportWidth - 10) {
                left = viewportWidth - dropdownWidth - 10;
            }
            if (left < 10) left = 10;

            // 應用樣式
            dropdown.style.setProperty('top', top + 'px', 'important');
            dropdown.style.setProperty('left', left + 'px', 'important');
            dropdown.style.setProperty('width', dropdownWidth + 'px', 'important');
            dropdown.style.setProperty('max-height', maxHeight + 'px', 'important');
            dropdown.style.setProperty('overflow-y', 'auto', 'important');
            
            // 確保可見性
            dropdown.style.setProperty('display', 'block', 'important');
            dropdown.style.setProperty('opacity', '1', 'important');
            dropdown.style.setProperty('visibility', 'visible', 'important');

            // 視覺樣式（background-color、border、border-radius、box-shadow）
            // 由 app.css 的 .searchable-dropdown.show 規則處理，以支援深色模式 CSS 變數

            // iOS 平滑滾動
            dropdown.style.setProperty('-webkit-overflow-scrolling', 'touch', 'important');
        });
    }

    // 使用 MutationObserver 監聽下拉選單的顯示/隱藏
    const observer = new MutationObserver(function (mutations) {
        let shouldPosition = false;
        
        mutations.forEach(function (mutation) {
            if (mutation.type === 'attributes' && mutation.attributeName === 'class') {
                const target = mutation.target;
                if (target.classList && target.classList.contains('searchable-dropdown')) {
                    shouldPosition = true;
                }
            } else if (mutation.type === 'childList') {
                mutation.addedNodes.forEach(node => {
                    if (node.nodeType === 1) {
                        if (node.classList?.contains('searchable-dropdown') ||
                            node.querySelector?.('.searchable-dropdown')) {
                            shouldPosition = true;
                        }
                    }
                });
            }
        });
        
        if (shouldPosition) {
            // 多次執行以確保 DOM 完全更新
            requestAnimationFrame(function() {
                positionSearchableDropdown();
                setTimeout(positionSearchableDropdown, 50);
            });
        }
    });

    // 開始觀察整個文檔
    observer.observe(document.body, {
        childList: true,
        subtree: true,
        attributes: true,
        attributeFilter: ['class']
    });

    // 視窗大小變化時重新定位
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
