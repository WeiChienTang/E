namespace ERPCore2.Services
{
    public interface ICertificateService
    {
        /// <summary>
        /// 檢查憑證公鑰檔案 (erpcore2.cer) 是否存在於 Resources 資料夾
        /// </summary>
        bool CertificateExists();

        /// <summary>
        /// 一鍵產生自簽憑證：
        /// 1. 自動偵測所有伺服器 IP 加入 SAN
        /// 2. 儲存 .pfx 到設定路徑
        /// 3. 儲存 .cer 到 Resources 資料夾
        /// 4. 更新 appsettings.Production.json 的密碼欄位
        /// </summary>
        /// <returns>(成功, 訊息)</returns>
        /// <param name="extraSans">額外加入憑證 SAN 的 IP 或主機名稱（例如對外 IP）</param>
        Task<(bool Success, string Message)> GenerateSelfSignedCertificateAsync(string[]? extraSans = null);
    }
}
