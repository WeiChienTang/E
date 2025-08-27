// Blazor 錯誤處理器 - 用於處理常見的 JavaScript interop 錯誤
(function() {
    'use strict';
    
    // 儲存原始的 console.error 方法
    const originalConsoleError = console.error;
    
    // 已知的安全錯誤模式列表
    const knownSafeErrorPatterns = [
        /There was an exception invoking '__Dispose'/,
        /Error: There was an exception invoking '__Dispose'/,
        /CircuitOptions\.DetailedErrors/,
        /cleanupEscKeyListener/,
        /JSDisconnectedException/,
        /The circuit failed to process the message/,
        /The connection was disconnected before invocation result was received/
    ];
    
    // 覆寫 console.error 方法
    console.error = function(...args) {
        // 檢查是否為已知的安全錯誤
        const errorMessage = args.join(' ');
        const isSafeError = knownSafeErrorPatterns.some(pattern => pattern.test(errorMessage));
        
        if (isSafeError) {
            // 將安全錯誤降級為 debug 訊息
            console.debug('Blazor interop notice (safe to ignore):', ...args);
            return;
        }
        
        // 對於真正的錯誤，保持原始行為
        originalConsoleError.apply(console, args);
    };
    
    // 全域錯誤事件處理器
    window.addEventListener('error', function(event) {
        const errorMessage = event.message || '';
        const isSafeError = knownSafeErrorPatterns.some(pattern => pattern.test(errorMessage));
        
        if (isSafeError) {
            // 防止錯誤顯示在控制台
            event.preventDefault();
            event.stopPropagation();
            console.debug('Blazor interop error handled (safe to ignore):', errorMessage);
            return false;
        }
        
        return true;
    });
    
    // Promise rejection 處理器
    window.addEventListener('unhandledrejection', function(event) {
        const errorMessage = event.reason?.message || event.reason?.toString() || '';
        const isSafeError = knownSafeErrorPatterns.some(pattern => pattern.test(errorMessage));
        
        if (isSafeError) {
            // 防止錯誤顯示在控制台
            event.preventDefault();
            console.debug('Blazor interop promise rejection handled (safe to ignore):', errorMessage);
            return;
        }
    });
    
    console.log('Blazor error handler initialized - known safe errors will be filtered');
})();
