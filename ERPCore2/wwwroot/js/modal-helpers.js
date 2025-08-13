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

// 全域變數來存儲 MutationObserver
var buttonTabNavigationObserver = null;

// 設置按鈕 Tab 導航功能
window.setupButtonTabNavigation = function () {
    // 移除之前的事件監聽器（如果存在）
    document.removeEventListener('keydown', handleButtonTabNavigation);
    document.removeEventListener('focusin', handleButtonFocus);
    
    // 停止之前的 observer
    if (buttonTabNavigationObserver) {
        buttonTabNavigationObserver.disconnect();
    }
    
    // 添加新的事件監聽器
    document.addEventListener('keydown', handleButtonTabNavigation);
    document.addEventListener('focusin', handleButtonFocus);
    
    // 延遲處理，確保 DOM 完全渲染後再設置，並重試多次
    var retryCount = 0;
    var maxRetries = 5;
    
    function setupWithRetry() {
        retryCount++;
        
        // 將所有現有按鈕設為不可 Tab 聚焦
        var buttonCount = setButtonsNonFocusable();
        
        // 如果沒找到按鈕且還有重試次數，繼續重試
        if (buttonCount === 0 && retryCount < maxRetries) {
            setTimeout(setupWithRetry, 200 * retryCount); // 增加延遲時間
        } else {
            // 設置 MutationObserver 來監控新添加的按鈕
            setupButtonObserver();
        }
    }
    
    setTimeout(setupWithRetry, 100); // 初始延遲
};

// 清理按鈕 Tab 導航功能
window.cleanupButtonTabNavigation = function () {
    document.removeEventListener('keydown', handleButtonTabNavigation);
    document.removeEventListener('focusin', handleButtonFocus);
    
    // 停止 observer
    if (buttonTabNavigationObserver) {
        buttonTabNavigationObserver.disconnect();
        buttonTabNavigationObserver = null;
    }
};

// 設置 MutationObserver 來監控動態添加的按鈕
function setupButtonObserver() {
    buttonTabNavigationObserver = new MutationObserver(function(mutations) {
        mutations.forEach(function(mutation) {
            if (mutation.type === 'childList') {
                mutation.addedNodes.forEach(function(node) {
                    if (node.nodeType === Node.ELEMENT_NODE) {
                        // 檢查新添加的節點是否為按鈕
                        if (isButton(node)) {
                            node.setAttribute('tabindex', '-1');
                        } else if (node.closest && node.closest('.modal-footer')) {
                            // 如果是 modal-footer 中的按鈕，確保它保持可聚焦
                            if (node.hasAttribute('tabindex') && node.getAttribute('tabindex') === '-1') {
                                node.removeAttribute('tabindex');
                            }
                        }
                        
                        // 檢查新添加節點的子元素中是否有按鈕
                        var buttons = node.querySelectorAll(
                            'button, ' +
                            'input[type="button"], ' +
                            'input[type="submit"], ' +
                            'input[type="reset"], ' +
                            '[role="button"], ' +
                            '.btn, ' +
                            '.button, ' +
                            '.btn-close, ' +
                            '[data-bs-toggle="button"], ' +
                            '[aria-label="關閉"]'
                        );
                        buttons.forEach(function(button) {
                            if (isButton(button)) {
                                button.setAttribute('tabindex', '-1');
                            } else if (button.closest('.modal-footer')) {
                                // modal-footer 中的按鈕保持可聚焦
                                if (button.hasAttribute('tabindex') && button.getAttribute('tabindex') === '-1') {
                                    button.removeAttribute('tabindex');
                                }
                            }
                        });
                    }
                });
            }
        });
    });
    
    // 開始觀察 modal 容器的變化
    var modal = document.querySelector('.modal.show');
    if (modal) {
        buttonTabNavigationObserver.observe(modal, {
            childList: true,
            subtree: true
        });
    } else {
        // 如果沒有找到 modal，觀察整個文件
        buttonTabNavigationObserver.observe(document.body, {
            childList: true,
            subtree: true
        });
    }
}

// 將按鈕設為不可 Tab 聚焦
function setButtonsNonFocusable() {
    // 找到當前 modal 中的所有按鈕
    var modal = document.querySelector('.modal.show');
    if (!modal) {
        modal = document;
    }
    
    // 更詳細的按鈕選擇器
    var buttons = modal.querySelectorAll(
        'button, ' +
        'input[type="button"], ' +
        'input[type="submit"], ' +
        'input[type="reset"], ' +
        '[role="button"], ' +
        '.btn, ' +
        '.button, ' +
        '.btn-close, ' +
        '[data-bs-toggle="button"], ' +
        '[aria-label="關閉"]'
    );
    
    var count = 0;
    var skippedCount = 0;
    buttons.forEach(function(button) {
        // 檢查是否為我們要跳過的按鈕類型
        if (isButton(button)) {
            // 設置 tabindex="-1" 使其不可通過 Tab 鍵聚焦，但仍可點擊
            button.setAttribute('tabindex', '-1');
            count++;
        } else if (button.closest('.modal-footer')) {
            // modal-footer 中的按鈕保持可聚焦
            if (button.hasAttribute('tabindex') && button.getAttribute('tabindex') === '-1') {
                button.removeAttribute('tabindex');
            }
            skippedCount++;
        }
    });
    
    return count; // 回傳處理的按鈕數量
}

// 處理按鈕 Tab 導航的函數（保留作為備用）
function handleButtonTabNavigation(event) {
    // 只處理 Tab 鍵
    if (event.key !== 'Tab') {
        return;
    }
    
    // 檢查當前焦點元素是否為按鈕
    var activeElement = document.activeElement;
    if (!activeElement || !isButton(activeElement)) {
        return;
    }
    
    // 防止預設的 Tab 行為
    event.preventDefault();
    
    // 跳到下一個非按鈕元素
    skipToNextNonButtonElement(activeElement, event.shiftKey ? -1 : 1);
}

// 處理按鈕焦點事件（備用機制）
function handleButtonFocus(event) {
    var activeElement = event.target;
    
    // 檢查焦點元素是否為按鈕
    if (activeElement && isButton(activeElement)) {
        // 如果按鈕獲得焦點，立即跳到下一個非按鈕元素
        skipToNextNonButtonElement(activeElement, 1);
    }
}

// 跳到下一個非按鈕元素的核心邏輯
function skipToNextNonButtonElement(currentElement, direction) {
    if (typeof direction === 'undefined') {
        // 如果沒有指定方向，根據最近的 Tab 按鍵方向決定
        // 預設向前
        direction = 1;
    }
    
    // 獲取所有可聚焦的元素
    var focusableElements = getFocusableElements();
    var currentIndex = focusableElements.indexOf(currentElement);
    
    if (currentIndex === -1) {
        return;
    }
    
    var nextIndex = currentIndex + direction;
    var attemptCount = 0;
    var maxAttempts = focusableElements.length;
    
    // 找到下一個非按鈕元素
    while (nextIndex >= 0 && nextIndex < focusableElements.length && attemptCount < maxAttempts) {
        var nextElement = focusableElements[nextIndex];
        
        // 如果找到非按鈕元素，聚焦並退出
        if (!isButton(nextElement)) {
            nextElement.focus();
            return;
        }
        
        nextIndex += direction;
        attemptCount++;
    }
    
    // 如果沒找到非按鈕元素，進行邊界處理
    if (direction > 0) {
        // 向前搜尋，從頭開始找第一個非按鈕元素
        for (var i = 0; i < currentIndex; i++) {
            if (!isButton(focusableElements[i])) {
                focusableElements[i].focus();
                return;
            }
        }
    } else {
        // 向後搜尋，從尾部開始找最後一個非按鈕元素
        for (var i = focusableElements.length - 1; i > currentIndex; i--) {
            if (!isButton(focusableElements[i])) {
                focusableElements[i].focus();
                return;
            }
        }
    }
}

// 檢查元素是否為按鈕
function isButton(element) {
    if (!element) return false;
    
    var tagName = element.tagName.toLowerCase();
    var type = element.type ? element.type.toLowerCase() : '';
    var role = element.getAttribute('role');
    var className = element.className || '';
    
    // 更精確的按鈕識別，針對 Bootstrap 和常見按鈕類別
    var isButtonElement = tagName === 'button' || 
                         (tagName === 'input' && (type === 'button' || type === 'submit' || type === 'reset')) ||
                         role === 'button' ||
                         className.includes('btn') ||
                         className.includes('button') ||
                         element.getAttribute('data-bs-toggle') === 'button' ||
                         element.classList.contains('btn-close') ||
                         element.getAttribute('aria-label') === '關閉';
    
    // 如果是按鈕，檢查是否在 modal-footer 中（這些按鈕應該保持可聚焦）
    if (isButtonElement) {
        var isInModalFooter = element.closest('.modal-footer') !== null;
        
        // 如果在 modal-footer 中，則不將其視為需要跳過的按鈕
        if (isInModalFooter) {
            return false; // 不要跳過這個按鈕
        }
    }
    
    return isButtonElement;
}

// 獲取所有可聚焦的元素
function getFocusableElements() {
    var selector = 'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])';
    var elements = document.querySelectorAll(selector);
    
    // 過濾掉隱藏或禁用的元素，並且只在當前顯示的 modal 中查找
    var visibleElements = Array.from(elements).filter(function(element) {
        // 檢查元素是否可見且可用
        var isVisible = !element.disabled && 
                       !element.hidden && 
                       element.offsetWidth > 0 && 
                       element.offsetHeight > 0 &&
                       element.tabIndex !== -1;
        
        if (!isVisible) return false;
        
        // 檢查元素是否在當前顯示的 modal 中
        var modal = element.closest('.modal.show');
        return modal !== null;
    });
    
    return visibleElements;
}

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
        
        // 移除 Tab 導航事件監聽器
        document.removeEventListener('keydown', handleButtonTabNavigation);
    });
});
