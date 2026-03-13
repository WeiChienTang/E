# 照片上傳系統說明

## 設計原則

- 照片**不存入資料庫**，只存相對路徑
- 上傳後**立即儲存**，不需要按儲存按鈕
- 依照片數量分為兩種模式：**單張（欄位）** 和 **多張（資料表）**

---

## 模式一：單張照片（欄位）

適用實體直接在自身資料表新增 `PhotoPath` 欄位。

### 適用對象

| 實體 | 欄位 | 子目錄 |
|------|------|--------|
| Company | `LogoPath` (`nvarchar(500)`) | `company-logos/` |
| Employee | `PhotoPath` (`nvarchar(500)`) | `employee-photos/` |

### 服務方法

#### ICompanyService
| 方法 | 說明 |
|------|------|
| `UpdateLogoPathAsync(int companyId, string logoPath)` | 更新 Logo 路徑 |
| `ClearLogoAsync(int companyId)` | 清除 Logo（設為 null） |

#### IEmployeeService
| 方法 | 說明 |
|------|------|
| `UpdatePhotoPathAsync(int employeeId, string photoPath)` | 更新員工照片路徑 |
| `ClearPhotoAsync(int employeeId)` | 清除員工照片（設為 null） |

### Tab 元件

| 元件 | 路徑 | 參數 |
|------|------|------|
| `CompanyLogoTab` | `Components/Pages/Systems/` | `CompanyId`, `InitialLogoPath` |
| `EmployeePhotoTab` | `Components/Pages/Employees/EmployeeEditModal/` | `EmployeeId`, `InitialPhotoPath` |

#### 公開方法

```
void Load(int id, string? photoPath)   // 載入（主組件透過 @ref 呼叫）
void Clear()                            // 清除（新增模式時呼叫）
```

---

## 模式二：多張照片（資料表）

適用需要多張照片的實體，各自建立獨立的照片資料表。

### 資料表：BusinessCards

名片照片，供**客戶**與**廠商**共用（Polymorphic 設計）。

| 欄位 | 型別 | 說明 |
|------|------|------|
| `Id` | `int` PK | 自動遞增 |
| `OwnerType` | `nvarchar(20)` NOT NULL | `"Customer"` 或 `"Supplier"` |
| `OwnerId` | `int` NOT NULL | 對應實體 ID |
| `ContactPersonName` | `nvarchar(50)` | 聯絡人姓名（失焦自動儲存） |
| `JobTitle` | `nvarchar(50)` | 職稱（失焦自動儲存） |
| `PhotoPath` | `nvarchar(500)` NOT NULL | 相對路徑 |
| `OriginalFileName` | `nvarchar(255)` | 原始檔名 |
| `SortOrder` | `int` | 排序 |
| Index | `(OwnerType, OwnerId)` | 複合索引 |

#### IBusinessCardService 方法

| 方法 | 說明 |
|------|------|
| `GetByOwnerAsync(string ownerType, int ownerId)` | 取得指定擁有者的名片列表 |
| 繼承自 `IGenericManagementService` | `CreateAsync`, `UpdateAsync`, `DeleteAsync` 等 |

#### 子目錄
`uploads/business-cards/`

---

### 資料表：ProductPhotos

商品照片，每張商品可有多張。

| 欄位 | 型別 | 說明 |
|------|------|------|
| `Id` | `int` PK | 自動遞增 |
| `ProductId` | `int` NOT NULL FK → Products | 商品 ID（Cascade Delete） |
| `PhotoPath` | `nvarchar(500)` NOT NULL | 相對路徑 |
| `OriginalFileName` | `nvarchar(255)` | 原始檔名 |
| `Caption` | `nvarchar(100)` | 圖說（失焦自動儲存） |
| `SortOrder` | `int` | 排序 |
| Index | `(ProductId)` | 索引 |

#### IProductPhotoService 方法

| 方法 | 說明 |
|------|------|
| `GetByProductAsync(int productId)` | 取得指定商品的照片列表 |
| 繼承自 `IGenericManagementService` | `CreateAsync`, `UpdateAsync`, `DeleteAsync` 等 |

#### 子目錄
`uploads/product-photos/`

---

### Tab 元件（多張）

| 元件 | 路徑 | 參數 |
|------|------|------|
| `BusinessCardTab` | `Components/Shared/BusinessCard/` | `OwnerType`, `OwnerId` |
| `ProductPhotoTab` | `Components/Pages/Products/` | `ProductId` |

#### 公開方法

```
Task LoadAsync(int id)   // 從 DB 載入（主組件透過 @ref 呼叫）
void Clear()             // 清除（新增模式時呼叫）
```

---

## 檔案上傳輔助：FileUploadHelper

路徑：`Helpers/Common/FileUploadHelper.cs`

| 方法 | 用途 | 大小限制 |
|------|------|----------|
| `UploadCompanyLogoAsync(file, companyId, env, oldPath?)` | 公司 Logo | 500 KB |
| `UploadImageAsync(file, subFolder, env, oldPath?)` | 一般圖片（名片、員工照、商品照） | 2 MB |
| `DeleteFile(relativePath, env)` | 刪除實體檔案 | — |

支援格式：`.jpg`, `.jpeg`, `.png`, `.gif`, `.svg`

---

## 靜態檔案服務（Program.cs）

執行期上傳的檔案不在 `MapStaticAssets()` 編譯時清單中，需透過 `UseStaticFiles` 提供：

```csharp
// 必須在 MapStaticAssets() 之前
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),  // wwwroot/uploads
    RequestPath = "/uploads"
});
app.MapStaticAssets();
```

開發模式下仍會出現 `StaticAssetDevelopmentRuntimeHandler` 警告，這是**預期行為**，不影響實際運作。

---

## 與 EditModal 整合模式

主組件透過 `@ref` 取得 Tab 元件參考，在以下時機呼叫公開方法：

| 事件 | 動作 |
|------|------|
| `OnEntityLoaded`（編輯模式載入） | 呼叫 `LoadAsync(id)` 或 `Load(id, path)` |
| `OnEntityLoaded`（新增模式，id=0） | 呼叫 `Clear()` |
| `OnSaveSuccess`（儲存後） | 重新呼叫 `LoadAsync(id)` 或 `Load(id, path)` |

Tab 內部使用 `_loadedXxxId` 追蹤已載入的 ID，避免 `OnParametersSetAsync` 重複載入。
