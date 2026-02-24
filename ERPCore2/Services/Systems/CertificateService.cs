using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Nodes;

namespace ERPCore2.Services
{
    public class CertificateService : ICertificateService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CertificateService> _logger;

        public CertificateService(
            IWebHostEnvironment environment,
            IConfiguration configuration,
            ILogger<CertificateService> logger)
        {
            _environment = environment;
            _configuration = configuration;
            _logger = logger;
        }

        public bool CertificateExists()
        {
            var cerPath = Path.Combine(_environment.ContentRootPath, "Resources", "erpcore2.cer");
            return File.Exists(cerPath);
        }

        public async Task<(bool Success, string Message)> GenerateSelfSignedCertificateAsync(string[]? extraSans = null)
        {
            try
            {
                var pfxPath = _configuration["HttpsConfig:CertificatePath"]
                    ?? Path.Combine(_environment.ContentRootPath, "cert.pfx");
                var cerPath = Path.Combine(_environment.ContentRootPath, "Resources", "erpcore2.cer");

                // 確保目錄存在
                var pfxDir = Path.GetDirectoryName(pfxPath);
                if (!string.IsNullOrEmpty(pfxDir))
                    Directory.CreateDirectory(pfxDir);
                Directory.CreateDirectory(Path.GetDirectoryName(cerPath)!);

                // 產生隨機密碼
                var password = Convert.ToBase64String(RandomNumberGenerator.GetBytes(24));

                // 建立 RSA 2048 金鑰
                using var rsa = RSA.Create(2048);

                var certRequest = new CertificateRequest(
                    "CN=ERPCore2 Server",
                    rsa,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1);

                // 加入 SAN：localhost + 127.0.0.1 + 所有伺服器 IPv4
                var sanBuilder = new SubjectAlternativeNameBuilder();
                sanBuilder.AddDnsName("localhost");
                sanBuilder.AddIpAddress(IPAddress.Loopback);

                try
                {
                    var hostIps = await Dns.GetHostAddressesAsync(Dns.GetHostName());
                    foreach (var ip in hostIps.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork))
                        sanBuilder.AddIpAddress(ip);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "無法取得伺服器 IP，僅使用 localhost");
                }

                // 加入管理員手動指定的對外 IP 或主機名稱
                if (extraSans != null)
                {
                    foreach (var san in extraSans)
                    {
                        var trimmed = san.Trim();
                        if (string.IsNullOrEmpty(trimmed)) continue;

                        if (IPAddress.TryParse(trimmed, out var extraIp))
                            sanBuilder.AddIpAddress(extraIp);
                        else
                            sanBuilder.AddDnsName(trimmed);
                    }
                }

                certRequest.CertificateExtensions.Add(sanBuilder.Build());
                certRequest.CertificateExtensions.Add(
                    new X509BasicConstraintsExtension(false, false, 0, false));
                certRequest.CertificateExtensions.Add(
                    new X509KeyUsageExtension(
                        X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, false));
                certRequest.CertificateExtensions.Add(
                    new X509EnhancedKeyUsageExtension(
                        new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, false)); // TLS Server

                // 建立自簽憑證（有效期 10 年）
                var cert = certRequest.CreateSelfSigned(
                    DateTimeOffset.UtcNow.AddDays(-1),
                    DateTimeOffset.UtcNow.AddYears(10));

                // 儲存 .pfx（含私鑰，伺服器用）
                var pfxBytes = cert.Export(X509ContentType.Pfx, password);
                await File.WriteAllBytesAsync(pfxPath, pfxBytes);

                // 儲存 .cer（公鑰，使用者安裝用）
                var cerBytes = cert.Export(X509ContentType.Cert);
                await File.WriteAllBytesAsync(cerPath, cerBytes);

                // 更新 appsettings.Production.json 的密碼與路徑
                UpdateProductionConfig(pfxPath, password);

                // 同時將密碼寫入獨立的 .pwd 檔案（與 cert.pfx 同目錄）
                // 這樣即使 publish 覆蓋 appsettings.Production.json，密碼仍可從此檔案讀取
                // 使用者不需要在每次程式更新後重新安裝憑證
                var pwdFilePath = Path.ChangeExtension(pfxPath, ".pwd");
                File.WriteAllText(pwdFilePath, password);

                _logger.LogInformation("自簽憑證產生成功，路徑：{PfxPath}", pfxPath);
                return (true, $"憑證產生成功！請重新啟動伺服器以啟用 HTTPS。\n憑證路徑：{pfxPath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "產生自簽憑證失敗");
                return (false, $"產生憑證失敗：{ex.Message}");
            }
        }

        private void UpdateProductionConfig(string certPath, string certPassword)
        {
            try
            {
                var configPath = Path.Combine(_environment.ContentRootPath, "appsettings.Production.json");
                if (!File.Exists(configPath)) return;

                var json = File.ReadAllText(configPath);
                var node = JsonNode.Parse(json);
                if (node == null) return;

                var httpsConfig = node["HttpsConfig"] as JsonObject;
                if (httpsConfig == null) return;

                httpsConfig["CertificatePath"] = certPath;
                httpsConfig["CertificatePassword"] = certPassword;
                httpsConfig["Enabled"] = true;

                File.WriteAllText(configPath, node.ToJsonString(new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                }));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "更新設定檔失敗，請手動更新 appsettings.Production.json");
            }
        }
    }
}
