/**
 * 日期輸入診斷工具 - Chrome 日期跳轉問題分析
 * 用於診斷為什麼年份需要輸入6位數才跳轉的問題
 */
window.DateInputDiagnostics = {
    logs: [],
    activeInput: null,
    
    /**
     * 初始化診斷工具
     */
    initialize: function () {
        console.log('%c[日期診斷] 診斷工具已啟動', 'color: #4CAF50; font-weight: bold; font-size: 14px;');
        console.log('%c請在日期欄位中輸入，所有事件都會被記錄', 'color: #2196F3;');
        
        // 監聽所有相關事件
        this.attachEventListeners();
        
        // 提供清空日誌的方法
        window.clearDateLogs = () => {
            this.logs = [];
            console.clear();
            console.log('%c[日期診斷] 日誌已清空', 'color: #FF9800;');
        };
        
        // 提供匯出日誌的方法
        window.exportDateLogs = () => {
            console.table(this.logs);
            return this.logs;
        };
    },
    
    /**
     * 附加所有事件監聽器
     */
    attachEventListeners: function () {
        const events = [
            'focus', 'blur', 
            'keydown', 'keypress', 'keyup',
            'input', 'beforeinput', 'change',
            'compositionstart', 'compositionupdate', 'compositionend',
            'select', 'selectstart'
        ];
        
        events.forEach(eventType => {
            document.addEventListener(eventType, (e) => {
                if (e.target.type === 'date') {
                    this.logEvent(eventType, e);
                }
            }, true);
        });
    },
    
    /**
     * 記錄事件詳情
     */
    logEvent: function (eventType, event) {
        const input = event.target;
        
        // 如果是新的輸入框，記錄輸入框資訊
        if (eventType === 'focus' && this.activeInput !== input) {
            this.activeInput = input;
            this.logInputInfo(input);
        }
        
        const logEntry = {
            時間: new Date().toISOString().split('T')[1].split('.')[0],
            事件類型: eventType,
            按鍵: this.getKeyInfo(event),
            輸入框值: input.value,
            值長度: input.value.length,
            選擇範圍: this.getSelectionInfo(input),
            輸入法: this.getIMEInfo(event),
            備註: this.getEventNotes(eventType, event)
        };
        
        this.logs.push(logEntry);
        
        // 在控制台輸出（使用不同顏色區分）
        const color = this.getEventColor(eventType);
        console.log(
            `%c[${logEntry.時間}] ${eventType.padEnd(15)} | 按鍵: ${logEntry.按鍵.padEnd(10)} | 值: "${input.value}" (長度: ${input.value.length})`,
            `color: ${color};`
        );
        
        // 關鍵事件額外提示
        if (eventType === 'input' && input.value.length > 10) {
            console.warn('⚠️ 警告：日期值長度超過10（正常應為 yyyy-MM-dd）', input.value);
        }
    },
    
    /**
     * 記錄輸入框資訊
     */
    logInputInfo: function (input) {
        console.group('%c📊 輸入框資訊', 'color: #9C27B0; font-weight: bold;');
        console.log('ID:', input.id);
        console.log('Name:', input.name);
        console.log('Type:', input.type);
        console.log('ReadOnly:', input.readOnly);
        console.log('Disabled:', input.disabled);
        console.log('MaxLength:', input.maxLength);
        console.log('AutoComplete:', input.autocomplete);
        console.log('InputMode:', input.inputMode);
        console.log('Pattern:', input.pattern);
        console.log('當前值:', input.value);
        
        // 檢查是否有 Shadow DOM
        if (input.shadowRoot) {
            console.log('Shadow DOM:', '存在（Chrome 使用 Shadow DOM 實作日期選擇器）');
        }
        
        // 檢查計算後的樣式
        const styles = window.getComputedStyle(input);
        console.log('IME Mode:', styles.imeMode);
        
        console.groupEnd();
    },
    
    /**
     * 取得按鍵資訊
     */
    getKeyInfo: function (event) {
        if (!event.key) return '-';
        
        const modifiers = [];
        if (event.ctrlKey) modifiers.push('Ctrl');
        if (event.altKey) modifiers.push('Alt');
        if (event.shiftKey) modifiers.push('Shift');
        if (event.metaKey) modifiers.push('Meta');
        
        const prefix = modifiers.length > 0 ? modifiers.join('+') + '+' : '';
        return prefix + event.key;
    },
    
    /**
     * 取得選擇範圍資訊
     */
    getSelectionInfo: function (input) {
        try {
            if (input.selectionStart !== null && input.selectionEnd !== null) {
                return `[${input.selectionStart}, ${input.selectionEnd}]`;
            }
        } catch (e) {
            // date input 可能不支援 selection API
        }
        return '-';
    },
    
    /**
     * 取得輸入法資訊
     */
    getIMEInfo: function (event) {
        if (event.type.startsWith('composition')) {
            return `使用中 (${event.data || '-'})`;
        }
        return event.isComposing ? '使用中' : '未使用';
    },
    
    /**
     * 取得事件備註
     */
    getEventNotes: function (eventType, event) {
        const notes = [];
        
        if (eventType === 'input' && event.inputType) {
            notes.push(`inputType: ${event.inputType}`);
        }
        
        if (eventType === 'beforeinput' && event.data) {
            notes.push(`data: "${event.data}"`);
        }
        
        if (event.defaultPrevented) {
            notes.push('已阻止預設行為');
        }
        
        return notes.join(', ') || '-';
    },
    
    /**
     * 取得事件顏色（用於控制台輸出）
     */
    getEventColor: function (eventType) {
        const colorMap = {
            'focus': '#4CAF50',
            'blur': '#9E9E9E',
            'keydown': '#2196F3',
            'keypress': '#03A9F4',
            'keyup': '#00BCD4',
            'input': '#FF9800',
            'beforeinput': '#FF5722',
            'change': '#F44336',
            'compositionstart': '#9C27B0',
            'compositionupdate': '#9C27B0',
            'compositionend': '#9C27B0',
        };
        return colorMap[eventType] || '#757575';
    }
};

// 自動初始化
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', function() {
        window.DateInputDiagnostics.initialize();
    });
} else {
    window.DateInputDiagnostics.initialize();
}

// 提供全域快捷方法
console.log('%c💡 診斷工具快捷命令:', 'color: #FF9800; font-weight: bold;');
console.log('%c  clearDateLogs()  - 清空日誌', 'color: #2196F3;');
console.log('%c  exportDateLogs() - 匯出日誌表格', 'color: #2196F3;');
