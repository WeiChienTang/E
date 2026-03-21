/**
 * 使用者偏好設定 localStorage 輔助函式
 * Key 格式：erp_prefs_{employeeId}
 * 所有偏好（除語言外）以單一 JSON 物件儲存，以 EmployeeId 區隔不同使用者
 */
window.PreferenceHelper = (function () {

    function _key(employeeId) {
        return 'erp_prefs_' + employeeId;
    }

    return {
        /** 檢查指定使用者是否已有 localStorage 偏好資料 */
        hasData: function (employeeId) {
            return localStorage.getItem(_key(employeeId)) !== null;
        },

        /** 讀取偏好設定，回傳 JSON 字串；無資料時回傳 null */
        getJson: function (employeeId) {
            return localStorage.getItem(_key(employeeId));
        },

        /** 儲存偏好設定（JSON 字串） */
        saveJson: function (employeeId, json) {
            try {
                localStorage.setItem(_key(employeeId), json);
            } catch (e) {
                console.error('[PreferenceHelper] 儲存失敗：', e);
            }
        },

        /** 匯出偏好設定為 JSON 檔案下載 */
        exportToFile: function (employeeId) {
            var raw = localStorage.getItem(_key(employeeId));
            if (!raw) {
                console.warn('[PreferenceHelper] 無偏好資料可匯出');
                return;
            }
            try {
                var blob = new Blob([raw], { type: 'application/json' });
                var url = URL.createObjectURL(blob);
                var a = document.createElement('a');
                a.href = url;
                a.download = 'erp-preferences.json';
                document.body.appendChild(a);
                a.click();
                document.body.removeChild(a);
                URL.revokeObjectURL(url);
            } catch (e) {
                console.error('[PreferenceHelper] 匯出失敗：', e);
            }
        },

        /**
         * 開啟檔案選擇器，讀取 JSON 後回傳其內容字串。
         * 使用者取消時回傳 null。
         */
        importFromFile: function () {
            return new Promise(function (resolve) {
                var input = document.createElement('input');
                input.type = 'file';
                input.accept = '.json,application/json';
                input.style.display = 'none';
                document.body.appendChild(input);

                input.onchange = function (e) {
                    var file = e.target.files[0];
                    document.body.removeChild(input);
                    if (!file) { resolve(null); return; }

                    var reader = new FileReader();
                    reader.onload = function (ev) { resolve(ev.target.result); };
                    reader.onerror = function () { resolve(null); };
                    reader.readAsText(file, 'UTF-8');
                };

                // 使用者按取消時（某些瀏覽器會觸發 cancel event）
                input.addEventListener('cancel', function () {
                    document.body.removeChild(input);
                    resolve(null);
                });

                input.click();
            });
        }
    };
})();
