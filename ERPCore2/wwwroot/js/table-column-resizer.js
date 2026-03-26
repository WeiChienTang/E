/**
 * 表格欄位寬度拖曳調整 (Column Resize)
 * 供 GenericTableComponent / GenericInteractiveTableComponent 共用
 *
 * 拖曳時只顯示藍色參考線（零 reflow），mouseup 才一次性套用寬度（Excel 行為）。
 */
window.tableColumnResizer = {
    /** 目前正在拖曳的狀態 */
    _active: null,
    /** 參考線 DOM 元素（lazy 建立，全域共用） */
    _guideLine: null,

    /**
     * 為指定表格初始化欄位 resize 功能
     * @param {string} tableId - table 元素的 id
     */
    init: function (tableId) {
        var table = document.getElementById(tableId);
        if (!table) return;

        // 避免重複綁定
        if (table.dataset.resizerInit === '1') return;

        var resizers = table.querySelectorAll('.column-resizer');
        // 如果 thead 尚未渲染（resizer 為 0），不設 flag，允許下次重試
        if (resizers.length === 0) return;

        table.dataset.resizerInit = '1';

        // 為所有 resizer handle 綁定 mousedown
        resizers.forEach(function (resizer) {
            resizer.addEventListener('mousedown', function (e) {
                tableColumnResizer._startResize(e, table);
            });
        });
    },

    /** 取得或建立參考線 */
    _getGuideLine: function () {
        if (!this._guideLine) {
            var line = document.createElement('div');
            line.className = 'column-resize-guide';
            document.body.appendChild(line);
            this._guideLine = line;
        }
        return this._guideLine;
    },

    /** @private */
    _startResize: function (e, table) {
        e.preventDefault();
        e.stopPropagation();

        var th = e.target.parentElement;
        var startX = e.clientX;
        var startWidth = th.offsetWidth;

        // 計算參考線的垂直範圍（整個 table 區域）
        var tableRect = table.getBoundingClientRect();

        // 視覺回饋
        e.target.classList.add('resizing');
        table.classList.add('resizing');

        // 顯示參考線在目前滑鼠位置
        var guide = this._getGuideLine();
        guide.style.left = startX + 'px';
        guide.style.top = tableRect.top + 'px';
        guide.style.height = tableRect.height + 'px';
        guide.style.display = 'block';

        var state = {
            table: table,
            resizer: e.target,
            th: th,
            startX: startX,
            startWidth: startWidth,
            guide: guide,
            minWidth: 50
        };
        tableColumnResizer._active = state;

        // 全域監聽（capture 確保不被其他元素攔截）
        document.addEventListener('mousemove', tableColumnResizer._onMouseMove, true);
        document.addEventListener('mouseup', tableColumnResizer._onMouseUp, true);
        document.body.style.cursor = 'col-resize';
    },

    /** @private — 拖曳中只移動參考線，不動表格 */
    _onMouseMove: function (e) {
        var s = tableColumnResizer._active;
        if (!s) return;

        // 只限制最小寬度，不限制最大（可自由跨越其他欄位）
        var delta = e.clientX - s.startX;
        if (s.startWidth + delta < s.minWidth) delta = s.minWidth - s.startWidth;

        s.guide.style.left = (s.startX + delta) + 'px';
    },

    /** @private — 放開時只改當前欄位寬度，其他欄位不動，表格自然伸縮 */
    _onMouseUp: function (e) {
        var s = tableColumnResizer._active;
        if (!s) return;

        // 隱藏參考線
        s.guide.style.display = 'none';

        // 計算最終寬度（同樣只限制最小值）
        var delta = e.clientX - s.startX;
        if (s.startWidth + delta < s.minWidth) delta = s.minWidth - s.startWidth;

        s.th.style.width = (s.startWidth + delta) + 'px';

        // 清除視覺狀態
        s.resizer.classList.remove('resizing');
        s.table.classList.remove('resizing');
        document.body.style.cursor = '';

        document.removeEventListener('mousemove', tableColumnResizer._onMouseMove, true);
        document.removeEventListener('mouseup', tableColumnResizer._onMouseUp, true);

        tableColumnResizer._active = null;
    }
};

/* ── 右鍵選單：全域攔截瀏覽器預設選單 ── */
if (!window._tableContextMenuListenerRegistered) {
    window._tableContextMenuListenerRegistered = true;
    document.addEventListener('contextmenu', function (e) {
        if (e.target.closest('[data-ctx-menu="1"]')) {
            e.preventDefault();
        }
    }, true);
}

/* ── 右鍵選單：顯示後修正超出視窗邊界的位置 ── */
if (!window.tableContextMenu) {
    window.tableContextMenu = {
        adjustPosition: function (menuId) {
            var menu = document.getElementById(menuId);
            if (!menu) return;

            var rect = menu.getBoundingClientRect();
            var vw = window.innerWidth;
            var vh = window.innerHeight;

            var x = parseFloat(menu.style.left) || 0;
            var y = parseFloat(menu.style.top) || 0;

            if (x + rect.width > vw) x = Math.max(0, x - rect.width);
            if (y + rect.height > vh) y = Math.max(0, y - rect.height);

            menu.style.left = x + 'px';
            menu.style.top = y + 'px';
        }
    };
}
