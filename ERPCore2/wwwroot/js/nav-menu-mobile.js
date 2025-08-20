/**
 * 手機導航選單自動隱藏功能
 * 當用戶在手機模式下點擊任何導航連結後，自動隱藏導航選單
 */

window.navMenuMobile = {
    /**
     * 初始化手機導航選單功能
     */
    init: function() {
        // 等待 DOM 完全載入
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', this.setupEventListeners.bind(this));
        } else {
            this.setupEventListeners();
        }
    },

    /**
     * 設置事件監聽器
     */
    setupEventListeners: function() {
        // 監聽所有導航連結的點擊事件
        document.addEventListener('click', this.handleNavClick.bind(this));
        
        // 監聽頁面內容區域的點擊事件（點擊內容區域時隱藏選單）
        document.addEventListener('click', this.handleContentClick.bind(this));
        
        // 監聽視窗大小變化，當螢幕變大時自動隱藏選單
        window.addEventListener('resize', this.handleResize.bind(this));
        
        // 監聽 ESC 鍵，按下時隱藏選單
        document.addEventListener('keydown', this.handleKeyDown.bind(this));
    },

    /**
     * 處理導航連結點擊事件
     */
    handleNavClick: function(event) {
        const target = event.target;
        
        // 檢查是否為導航連結
        if (this.isNavLink(target)) {
            // 延遲隱藏選單，讓連結有時間執行
            setTimeout(() => {
                this.hideNavMenu();
            }, 100);
        }
    },

    /**
     * 處理內容區域點擊事件
     */
    handleContentClick: function(event) {
        const target = event.target;
        
        // 檢查點擊是否在導航選單外部且在手機模式下
        if (!this.isInsideNavMenu(target) && 
            this.isMobileMode() && 
            this.isNavMenuOpen()) {
            this.hideNavMenu();
        }
    },

    /**
     * 處理鍵盤按下事件
     */
    handleKeyDown: function(event) {
        // ESC 鍵關閉選單
        if (event.key === 'Escape' && this.isMobileMode() && this.isNavMenuOpen()) {
            this.hideNavMenu();
        }
    },

    /**
     * 處理視窗大小變化
     */
    handleResize: function() {
        // 如果螢幕變大到桌面模式，隱藏選單
        if (!this.isMobileMode()) {
            this.hideNavMenu();
        }
    },

    /**
     * 檢查目標元素是否為導航連結
     */
    isNavLink: function(element) {
        // 檢查是否為直接的導航連結
        if (element.classList.contains('nav-menu-nav-link')) {
            return true;
        }
        
        // 檢查是否為下拉選單項目
        if (element.classList.contains('dropdown-item') || 
            element.closest('.dropdown-item')) {
            return true;
        }
        
        // 檢查是否在導航選單內的連結
        const navScrollable = element.closest('.nav-scrollable');
        if (navScrollable && (element.tagName === 'A' || element.closest('a'))) {
            return true;
        }
        
        return false;
    },

    /**
     * 檢查目標元素是否在導航選單內部
     */
    isInsideNavMenu: function(element) {
        return element.closest('.nav-scrollable') !== null ||
               element.closest('.navbar-toggler') !== null ||
               element.closest('.top-row') !== null;
    },

    /**
     * 檢查是否為手機模式
     */
    isMobileMode: function() {
        // 檢查是否有 navbar-toggler 顯示（表示手機模式）
        const toggler = document.querySelector('.navbar-toggler');
        return toggler && window.getComputedStyle(toggler).display !== 'none';
    },

    /**
     * 檢查導航選單是否開啟
     */
    isNavMenuOpen: function() {
        const toggler = document.querySelector('.navbar-toggler');
        return toggler && toggler.checked;
    },

    /**
     * 隱藏導航選單
     */
    hideNavMenu: function() {
        const toggler = document.querySelector('.navbar-toggler');
        if (toggler && toggler.checked) {
            toggler.checked = false;
            
            // 觸發 change 事件以確保任何其他監聽器都能接收到狀態變化
            const changeEvent = new Event('change', { bubbles: true });
            toggler.dispatchEvent(changeEvent);
        }
    },

    /**
     * 顯示導航選單
     */
    showNavMenu: function() {
        const toggler = document.querySelector('.navbar-toggler');
        if (toggler && !toggler.checked) {
            toggler.checked = true;
            
            // 觸發 change 事件
            const changeEvent = new Event('change', { bubbles: true });
            toggler.dispatchEvent(changeEvent);
        }
    },

    /**
     * 切換導航選單顯示狀態
     */
    toggleNavMenu: function() {
        const toggler = document.querySelector('.navbar-toggler');
        if (toggler) {
            toggler.checked = !toggler.checked;
            
            // 觸發 change 事件
            const changeEvent = new Event('change', { bubbles: true });
            toggler.dispatchEvent(changeEvent);
        }
    }
};

// 自動初始化
window.navMenuMobile.init();
