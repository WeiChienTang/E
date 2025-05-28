# Blazor Server ERP 設計流程指南

## 架構概覽

本指南採用簡化且高效的三層架構，遵循以下依賴流程：

```
資料庫 ↔ DbContext (EF Core) ↔ Service ↔ Blazor Pages
```

### 核心設計原則
- **關注分離**：每一層都有單一責任
- **依賴反轉**：依賴抽象而非具體實作
- **SOLID 原則**：維持乾淨且可維護的程式碼
- **簡潔性**：避免不必要的抽象層，EF Core 已提供完整的資料存取功能

### 為什麼選擇這個架構？
- **EF Core 本身就是完整的資料存取層**：`DbContext` = 工作單元，`DbSet<T>` = 資料集合
- **Service 層優點**：單一責任、更好效能、更容易維護、符合現代 .NET 最佳實務
- **直接整合**：減少不必要的層級和複雜性

---

## 開發流程：Data → Service → Pages

### 第一步：資料層設計 (Data)
> 詳細內容請參考：`Readme_Data.md`

**目的**：定義資料結構，建立資料庫基礎

**主要任務**：
1. **設計實體 (Entities)**
   - 定義業務實體和屬性
   - 添加資料註解進行驗證
   - 包含稽核欄位 (CreatedDate, ModifiedDate)
   - 定義狀態列舉

2. **設定 DbContext**
   - 配置實體關係和約束
   - 設定索引和預設值
   - 定義外鍵關係

3. **資料庫遷移**
   - 建立和套用 Migrations
   - 驗證資料庫結構

**檢查清單**：
- [ ] 實體名稱反映業務領域
- [ ] 所有必要屬性都有適當驗證
- [ ] 包含稽核欄位和狀態管理
- [ ] DbContext 正確設定所有關係
- [ ] Migrations 已建立並測試

---

### 第二步：服務層實作 (Service)
> 詳細內容請參考：`Readme_Service.md`

**目的**：實作業務邏輯、資料操作和驗證

**主要任務**：
1. **定義服務介面**
   - 標準 CRUD 操作
   - 業務特定方法
   - 分頁和查詢方法

2. **實作服務類別**
   - 業務驗證和規則
   - 資料存取操作 (直接使用 EF Core)
   - 錯誤處理和日誌記錄
   - 交易管理 (複雜操作時)

3. **建立請求/回應模型**
   - Create/Update 請求模型
   - ServiceResult 回應模式
   - 驗證屬性

**核心模式**：
- **ServiceResult 模式**：統一的成功/失敗回應結構
- **軟刪除**：使用 Status 欄位而非實際刪除
- **業務驗證**：在服務層實作所有業務規則
- **直接 EF Core**：無需額外的資料存取層

**檢查清單**：
- [ ] 服務介面包含所有必要方法
- [ ] 實作完整的業務驗證
- [ ] 使用 ServiceResult 模式統一回應
- [ ] 實作軟刪除和重複檢查
- [ ] 包含適當的錯誤處理和日誌
- [ ] 複雜操作使用交易管理

---

### 第三步：頁面層開發 (Pages)
> 詳細內容請參考：`Readme_Pages.md`

**目的**：處理使用者介面和互動

**主要任務**：
1. **建立 Blazor 頁面**
   - 資料顯示和列表
   - 表單處理 (新增/編輯)
   - 使用者互動 (刪除確認等)

2. **實作 UI 邏輯**
   - 載入狀態管理
   - 錯誤和成功訊息顯示
   - 表單驗證處理

3. **整合服務層**
   - 注入和使用服務
   - 處理服務回應
   - 適當的例外處理

**關鍵 UI 模式**：
- **載入狀態**：向使用者顯示處理進度
- **訊息回饋**：成功/錯誤訊息的一致顯示
- **表單驗證**：即時驗證和錯誤顯示
- **確認對話框**：破壞性操作的確認機制

**檢查清單**：
- [ ] 實作適當的載入狀態顯示
- [ ] 包含錯誤處理和使用者回饋
- [ ] 表單驗證正確運作
- [ ] 破壞性操作有確認對話框
- [ ] 響應式設計考量
- [ ] 遵循一致的 UI 模式

---

## 依賴注入設定

```csharp
// Program.cs 基本設定
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IYourEntityService, YourEntityService>();

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
```

---

## 命名慣例

- **實體**：單數名詞 (Customer, Order, Product)
- **DbSets**：複數名詞 (Customers, Orders, Products)  
- **Services**：[實體]Service (CustomerService)
- **介面**：I[實體]Service (ICustomerService)
- **頁面**：/[實體複數]/Index.razor (/Customers/Index.razor)

---

## 檔案組織結構

```
/Data
  /Entities
    Customer.cs
    Order.cs
  AppDbContext.cs
  
/Services
  /Models
    CustomerModels.cs
  /Interfaces
    ICustomerService.cs
  CustomerService.cs
  
/Pages
  /Customers
    Index.razor
  /Orders
    Index.razor
```

---

## 測試策略

- **單元測試**：使用 EF Core In-Memory 資料庫測試服務層
- **整合測試**：測試完整的端到端工作流程
- **UI 測試**：使用 Blazor 測試工具測試頁面互動

---

## 品質標準

- 一致使用 async/await
- 實作適當的錯誤處理
- 遵循 C# 編碼慣例
- 使用有意義的命名
- 保持方法專注且單一目的
- 適當的日誌記錄

---

## 常見模式摘要

- **錯誤處理**：ServiceResult 模式統一處理
- **驗證**：資料註解 + 業務驗證
- **交易**：複雜操作使用 EF Core 交易
- **安全性**：輸入驗證和授權
- **效能**：async/await、索引、分頁、投影查詢

---

*本指南提供現代、高效的 Blazor Server ERP 開發基礎。通過遵循 Data → Service → Pages 的開發流程，您可以建構出可維護、可測試且高效能的企業級應用程式。*