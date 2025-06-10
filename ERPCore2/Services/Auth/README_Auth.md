# 美莊磚業 ERP 驗證系統文檔

## 系統概述

美莊磚業 ERP 系統採用 **Blazor Server** 原生驗證架構，完全移除了 API 依賴，實現了高效的權限管理和使用者驗證機制。

## 驗證流程架構

```
用戶登入 → AuthenticationService → Claims序列化 → Cookie設定 → 權限驗證 → 選單顯示
```

### 詳細流程說明

1. **使用者輸入憑證** (`Login.razor`)
2. **驗證服務處理** (`AuthenticationService.LoginAsync`)
3. **Claims 建立和序列化**
4. **導航至驗證端點** (`/api/auth/signin`)
5. **Cookie 設定和重定向** (`AuthController`)
6. **權限狀態同步** (`CustomRevalidatingServerAuthenticationStateProvider`)
7. **選單項目授權顯示** (`NavMenu.razor` + `PermissionCheck`)

## 核心檔案說明

### 🔐 驗證服務層

#### `AuthenticationService.cs`
- **功能**: 主要驗證邏輯處理
- **方法**:
  - `LoginAsync(username, password)`: 驗證使用者憑證
  - `UpdateLastLoginAsync(employeeId)`: 更新最後登入時間
- **依賴**: `IEmployeeService`, `ILogger`

#### `EmployeeService.cs`
- **功能**: 員工資料管理和驗證
- **方法**:
  - `GetByUsernameAsync(username)`: 根據使用者名稱獲取員工資料
  - `UpdateLastLoginAsync(employeeId)`: 更新員工最後登入時間
- **依賴**: 直接資料庫連接

#### `PermissionService.cs`
- **功能**: 權限檢查和管理
- **方法**:
  - `HasPermissionAsync(employeeId, permission)`: 檢查特定權限
  - `GetEmployeePermissionsAsync(employeeId)`: 獲取員工所有權限
- **權限類型**: `Customer.View`, `System.Admin`, `Report.View`, 等

#### `CustomRevalidatingServerAuthenticationStateProvider.cs`
- **功能**: Blazor Server 驗證狀態提供者
- **特點**:
  - 繼承 `RevalidatingServerAuthenticationStateProvider`
  - 定期重新驗證使用者狀態
  - 與 `IEmployeeService` 整合驗證

### 🎯 介面定義層

#### `Interfaces/IAuthenticationService.cs`
- 定義驗證服務契約

#### `Interfaces/IEmployeeService.cs`
- 定義員工服務契約

#### `Interfaces/IPermissionService.cs`
- 定義權限服務契約

### 🔧 控制器層

#### `Controllers/AuthController.cs`
- **端點**: `GET /api/auth/signin`
- **功能**: 
  - 接收序列化的 Claims 參數
  - 設定驗證 Cookie
  - 重定向至目標頁面
- **安全性**: 僅處理內部驗證流程

### 🎨 前端組件層

#### `Components/Pages/Auth/Login.razor`
- **功能**: 使用者登入介面
- **流程**:
  1. 接收使用者輸入
  2. 呼叫 `AuthenticationService.LoginAsync`
  3. 建立 Claims 並序列化
  4. 導航至 `/api/auth/signin` 設定 Cookie

#### `Components/Pages/Auth/Logout.razor`
- **功能**: 使用者登出處理
- **機制**: 使用 Blazor 原生 `SignOutManager`

#### `Components/Layout/NavMenu.razor`
- **功能**: 主導航選單
- **權限控制**: 
  - 使用 `<AuthorizeView>` 控制登入狀態顯示
  - 使用 `<PermissionCheck>` 控制選單項目權限

#### `Components/Shared/PermissionCheck.razor`
- **功能**: 權限檢查組件
- **用法**: `<PermissionCheck Permission="Customer.View">內容</PermissionCheck>`
- **邏輯**: 自動獲取當前使用者權限並檢查

## 系統配置

### `Program.cs` 關鍵配置

```csharp
// Cookie 驗證配置
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/auth/login";
        options.LogoutPath = "/auth/logout";
        options.AccessDeniedPath = "/auth/access-denied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

// 自訂驗證狀態提供者
builder.Services.AddScoped<AuthenticationStateProvider, CustomRevalidatingServerAuthenticationStateProvider>();
```

## 權限系統

### 支援的權限類型
- `Customer.View` - 客戶管理檢視
- `Customer.Edit` - 客戶資料編輯
- `System.Admin` - 系統管理
- `Report.View` - 報表檢視
- `Report.Generate` - 報表產生
- 更多權限可在資料庫中配置

### 權限檢查機制
1. **資料庫驗證**: 每次權限檢查都會查詢最新的資料庫權限
2. **快取機制**: 可在 `PermissionService` 中實作快取提升效能
3. **即時更新**: 權限變更後立即生效，無需重新登入

## 安全性特點

### 🛡️ 安全措施
1. **Cookie 安全性**: HttpOnly, Secure, SameSite 保護
2. **Claims 驗證**: 所有使用者資訊都經過 Claims 驗證
3. **權限即時檢查**: 每次操作都檢查最新權限
4. **自動登出**: 長時間無操作自動登出
5. **重新驗證**: 定期重新驗證使用者狀態

### 🔒 資料保護
- 密碼雜湊: 使用安全的密碼雜湊演算法
- Session 管理: 安全的 Session 生命週期管理
- CSRF 保護: 內建 CSRF 攻擊防護

## 故障排除

### 常見問題

1. **選單項目不顯示**
   - 檢查使用者權限設定
   - 確認 `PermissionCheck` 組件正確使用
   - 驗證資料庫權限資料

2. **驗證狀態不同步**
   - 確認 `CustomRevalidatingServerAuthenticationStateProvider` 正確註冊
   - 檢查 Cookie 設定是否正確

3. **登入後重定向失敗**
   - 檢查 `AuthController.SignIn` 方法
   - 確認 `returnUrl` 參數處理

### 除錯技巧

1. **啟用詳細日誌**:
```csharp
builder.Logging.SetMinimumLevel(LogLevel.Debug);
```

2. **權限檢查除錯**:
```csharp
Console.WriteLine($"檢查權限: {permission} for User: {employeeId}");
```

3. **Cookie 檢查**: 使用瀏覽器開發者工具檢查 Cookie 設定

## 效能優化建議

1. **權限快取**: 在 `PermissionService` 實作 Redis 快取
2. **資料庫連接**: 使用連接池優化資料庫效能
3. **Claims 優化**: 減少 Claims 中的資料量
4. **組件快取**: 對 `PermissionCheck` 組件實作快取機制

## 維護指南

### 新增權限
1. 在資料庫中新增權限記錄
2. 在 `NavMenu.razor` 中使用 `<PermissionCheck Permission="新權限">` 
3. 在相關頁面組件中添加權限檢查

### 新增驗證提供者
1. 實作 `IAuthenticationService` 介面
2. 在 `Program.cs` 中註冊新服務
3. 更新 `Login.razor` 使用新的驗證邏輯

### 資料庫結構變更
1. 更新 `EmployeeService` 中的 SQL 查詢
2. 修改相關的資料模型
3. 測試驗證和權限流程

---

## 版本資訊
- **版本**: 2.0
- **最後更新**: 2025年6月
- **架構**: Blazor Server + Cookie Authentication
- **狀態**: 生產就緒 ✅

## 聯絡資訊
如有技術問題，請聯絡開發團隊或查閱相關程式碼註解。
