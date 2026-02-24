# HTTPS 憑證設計（失敗記錄）

> **狀態：已放棄，系統恢復為 HTTP 模式（port 6011）**
> 記錄日期：2026-02-24

---

## 目標

讓部署在客戶內部伺服器的 ERP 系統支援 HTTPS，解決瀏覽器顯示「不安全」的問題。
由於無法取得正式 CA 憑證（內部伺服器無公開網域），計畫使用**自簽憑證**搭配「一鍵安裝」機制讓使用者信任。

---

## 設計架構

### 流程設計

```
管理員：系統設定 → 憑證管理 → 產生憑證
    ↓
產生 cert.pfx（伺服器用，含私鑰）+ erpcore2.cer（使用者安裝用）
    ↓
使用者開啟 http://伺服器IP:6011/install-cert
    ↓
下載 install-cert.bat → 雙擊執行 → UAC 確認 → 自動安裝到 Windows 信任存放區
    ↓
重新開啟瀏覽器 → 連線 https://伺服器IP:6012 → 預期不再顯示警告
```

### 建立的檔案

| 檔案 | 說明 |
|------|------|
| `Controllers/CertificateController.cs` | 提供 `/api/certificate/cer`（下載公鑰）和 `/api/certificate/installer`（下載 .bat 安裝程式） |
| `Services/Systems/ICertificateService.cs` | 憑證服務介面 |
| `Services/Systems/CertificateService.cs` | 憑證產生邏輯（RSA 2048、SAN 設定、.pfx/.cer 儲存） |
| `Components/Pages/InstallCert.razor` | 使用者安裝說明頁面（`/install-cert`，無需登入） |

### 修改的檔案

| 檔案 | 修改內容 |
|------|----------|
| `Program.cs` | 條件式 Kestrel HTTPS 設定、憑證預先驗證、.pwd 密碼檔讀取、/api/certificate 排除 HTTPS 重新導向 |
| `appsettings.Production.json` | 新增 `HttpsConfig` 區段（Enabled/HttpPort/HttpsPort/CertificatePath/CertificatePassword） |
| `Components/Pages/Systems/SystemParameterSettingsModal.razor` | 新增憑證管理分頁（產生憑證、顯示下載連結、輸入額外 SAN） |
| `Services/Auth/PermissionCheckMiddleware.cs` | 新增 `/install-cert` 為公開路徑（無需登入） |
| `Data/ServiceRegistration.cs` | 註冊 `ICertificateService` |

---

## 遭遇問題與處理過程

### 問題 1：install-cert.bat 中文字重疊、亂碼

**現象**：執行 .bat 後畫面上中文文字重疊顯示，並出現「XXX 不是內部或外部命令」錯誤。

**原因**：Windows cmd.exe 的已知 Bug——`chcp 65001`（切換為 UTF-8）啟用後，`echo` 輸出中文時，多位元組字元的第一個位元組會被當成獨立字元輸出，產生雙重顯示；剩餘位元組則被解析為命令，導致錯誤。

**解決**：將 .bat 內所有 `echo` 文字改為英文，移除 `chcp 65001`，並將輸出編碼改為 `Encoding.ASCII`。

---

### 問題 2：安裝憑證後瀏覽器仍顯示「不安全」（SAN 不包含外部 IP）

**現象**：使用者安裝憑證後，連線 `https://59.126.246.98:6012` 仍顯示安全警告。

**原因**：瀏覽器驗證 TLS 憑證時，會比對連線的 IP/主機名稱是否列於憑證的 **SAN（Subject Alternative Names）** 中。`Dns.GetHostAddressesAsync(Dns.GetHostName())` 只能取得伺服器的內網 IP（如 `192.168.50.50`），無法自動取得路由器的外部 IP（`59.126.246.98`）。

**解決**：在憑證管理 UI 新增「對外 IP / 主機名稱」輸入欄，管理員手動填入後，產生憑證時會一併加入 SAN。

---

### 問題 3：Hairpin NAT（同機測試外部 IP 失敗）

**現象**：在伺服器本機使用 `59.126.246.98:6011` 測試時無法連線。

**原因**：從同一台機器透過外部 IP 連回自己，需要路由器支援 **Hairpin NAT（NAT 回流）**。部分路由器不支援此功能。

**解決**：說明同機測試應使用 `192.168.50.50`（內網）或 `localhost`；外部 IP 需從不同機器測試。

---

### 問題 4：啟用 HTTPS 後外部連線完全中斷

**現象**：HTTPS 啟用後，外部使用者無論使用 port 6011 或 6012 都無法連線。

**原因**：`UseHttpsRedirection` 啟用後，所有 HTTP 6011 的請求被 301 重新導向至 HTTPS 6012。但路由器的 port forwarding 只有設定 6011，沒有設定 6012，導致 6012 無法從外部存取。

**解決**：在路由器新增 6012 的 port forwarding 規則。

---

### 問題 5：下載連結回傳 ERR_EMPTY_RESPONSE

**現象**：點擊「下載安裝程式」或「下載憑證」後，瀏覽器顯示 `ERR_EMPTY_RESPONSE`，沒有任何資料傳回。

**原因**：管理員從 HTTPS（port 6012）頁面操作，但 UI 產生的下載 URL 仍使用 HTTP port（`http://IP:6012/...`）。TLS port 收到純 HTTP 請求時，伺服器直接關閉連線，不傳回任何資料。

**解決**：改為從 `IConfiguration` 讀取 `HttpsConfig:HttpPort`，並根據目前頁面的 scheme（HTTP/HTTPS）動態決定下載連結使用的協定與 port。

---

### 問題 6：Chrome 靜默封鎖下載（Mixed Content）

**現象**：在 HTTPS 頁面下，點擊 HTTP 下載連結後沒有任何反應，也沒有錯誤訊息。

**原因**：Chrome 81+ 起，HTTPS 頁面發起的 HTTP 檔案下載（Mixed Content）會被靜默封鎖，瀏覽器不顯示任何錯誤。

**解決**：動態判斷目前頁面的 scheme，若為 HTTPS 則使用 HTTPS 下載連結，若為 HTTP 則使用 HTTP 連結。

---

### 問題 7：每次 publish 後伺服器崩潰（CryptographicException）

**現象**：每次更新程式碼並重新發布後，伺服器啟動時拋出 `CryptographicException: The specified network password is not correct`，無法啟動。

**原因**：`CertificateService.UpdateProductionConfig()` 在執行時更新了 `ContentRootPath` 下的 `appsettings.Production.json`（寫入新的 cert.pfx 密碼）。但 `dotnet publish` 會將**原始碼目錄**中的 `appsettings.Production.json`（舊密碼）複製到輸出目錄，**覆蓋**執行時已更新的版本。下次啟動時，伺服器讀到舊密碼但 cert.pfx 是新密碼，導致驗證失敗。

**已實施的緩解措施**：
1. `Program.cs` 新增憑證預先驗證：密碼不符時自動降級為 HTTP 模式，不崩潰
2. 新增 `cert.pwd` 獨立密碼檔（與 cert.pfx 同目錄）：啟動時優先讀取此檔，因其不在 publish 輸出中，不受覆蓋影響

---

### 問題 8：下載安裝程式後執行失敗（SSL 信任死結）

**現象**：
```
[Error] Download failed: 基礎連接已關閉: 無法為 SSL/TLS 安全通道建立信任關係。
```

**原因**：安裝程式嘗試從 `http://IP:6011/api/certificate/cer` 下載憑證，但 `UseHttpsRedirection` 將此請求 301 重新導向至 `https://IP:6012/...`。PowerShell 跟隨重新導向後嘗試建立 HTTPS 連線，但自簽憑證此時尚未被用戶端信任，導致 SSL 握手失敗。

這是典型的**「需要憑證才能下載憑證」死結**。

**解決**：在 `Program.cs` 的 `UseHttpsRedirection` 改為 `UseWhen`，將 `/api/certificate` 路徑排除在重新導向範圍外：
```csharp
app.UseWhen(
    context => !context.Request.Path.StartsWithSegments("/api/certificate"),
    appBuilder => appBuilder.UseHttpsRedirection()
);
```

---

### 問題 9：安裝成功但瀏覽器仍顯示「不安全」（最終失敗）

**現象**：使用者執行安裝程式，顯示「Certificate installed successfully!」，但連線 `https://59.126.246.98:6012` 瀏覽器仍顯示安全警告。連線 `http://59.126.246.98:6011` 則無法登入。

**可能原因（未能確認）**：
- 憑證已安裝至信任存放區，但 SAN 仍不包含 `59.126.246.98`（重新產生憑證後使用者安裝的可能是舊版本）
- Chrome 的憑證快取需要完全關閉瀏覽器（含背景執行）才能清除
- Chrome 對自簽憑證的額外安全限制（CT Log、憑證透明度要求）
- 憑證安裝到使用者存放區而非機器存放區導致部分場景不生效

**當下狀態**：無法確認根本原因，且每次嘗試修復都引入新問題，決定放棄此方案。

---

## 目前狀態

| 項目 | 狀態 |
|------|------|
| `appsettings.Production.json` → `HttpsConfig.Enabled` | **false**（已關閉） |
| 伺服器運作模式 | **HTTP port 6011** |
| 系統設定「安全憑證」Tab | **已註解隱藏**（`SystemParameterSettingsModal.razor`） |
| 憑證相關後端程式碼 | 保留但 UI 入口已關閉（不影響 HTTP 運作） |

---

## 保留的程式碼

所有相關程式碼保留在 codebase 中，未刪除。若未來需要重新嘗試，可將 `appsettings.Production.json` 中的 `HttpsConfig.Enabled` 改回 `true` 並重新產生憑證。

保留理由：
- 程式碼本身邏輯正確，問題主要在部署環境（路由器 NAT、Windows 憑證信任機制、Chrome 安全政策）
- 未來可能換用 Let's Encrypt 或購買正式 SSL 憑證，屆時可參考現有架構

---

## 替代方案建議（未來考慮）

| 方案 | 說明 | 難度 |
|------|------|------|
| 購買正式 SSL 憑證 | 需要公開網域名稱（domain），用戶瀏覽器直接信任，無需手動安裝 | 中 |
| Let's Encrypt | 免費正式憑證，需要公開網域 + 80/443 port 可從外部存取 | 中 |
| 反向代理（Nginx/Caddy） | 在前端架設代理伺服器處理 HTTPS，後端維持 HTTP | 高 |
| 使用者接受安全警告 | 點擊「進階 → 繼續前往」，不需安裝憑證 | 低（但體驗差） |
| 不使用 HTTPS | 內部網路環境下維持 HTTP，外部存取透過 VPN | 低 |
