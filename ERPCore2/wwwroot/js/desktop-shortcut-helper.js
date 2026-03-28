/**
 * 桌面捷徑下載輔助函數
 * 產生 Windows .url 捷徑檔案，讓使用者可以快速從桌面開啟系統
 */
window.desktopShortcutHelper = {
    /**
     * 偵測是否為行動裝置
     */
    isMobile: function () {
        return /Android|iPhone|iPad|iPod|Opera Mini|IEMobile|WPDesktop/i.test(navigator.userAgent);
    },

    /**
     * 下載桌面捷徑檔案
     * @returns {string} 空字串表示成功，非空字串表示需顯示給使用者的提示訊息
     */
    download: function () {
        try {
            if (this.isMobile()) {
                return "mobile";
            }

            var origin = window.location.origin;
            var content = "[InternetShortcut]\r\nURL=" + origin + "\r\nIconIndex=0\r\n";

            var blob = new Blob([content], { type: "application/internet-shortcut" });
            var url = URL.createObjectURL(blob);
            var link = document.createElement("a");
            link.href = url;
            link.download = "ERPCore2.url";

            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);

            URL.revokeObjectURL(url);
            return "";
        } catch (e) {
            console.error("下載桌面捷徑失敗:", e);
            return "error";
        }
    }
};
