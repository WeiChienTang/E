# Data 資料夾說明文檔

## 概述
Data 資料夾包含了 ERPCore2 系統的資料存取層相關檔案，主要負責資料模型定義、資料庫上下文設定以及初始資料種子。

## 資料夾結構

```
Data/
├── Context/          # 資料庫上下文檔案
├── Entities/         # 實體類別檔案
│   ├── Commons/      # 共用實體類別
│   ├── Customers/    # 客戶相關實體類別
│   └── Industries/   # 行業相關實體類別
├── Enums/           # 列舉定義檔案
└── SeedData.cs      # 資料庫初始資料種子
```

## 命名規範

### 命名空間命名方式
所有 Data 資料夾下的檔案均使用廣域命名方式，**不包含具體的資料夾名稱**：

- ✅ **正確範例**：`ERPCore2.Data.Entities`
- ❌ **錯誤範例**：`ERPCore2.Data.Entities.Commons`

### 實際命名範例
- `Data/Entities/Commons/AddressType.cs` → `namespace ERPCore2.Data.Entities`
- `Data/Entities/Customers/Customer.cs` → `namespace ERPCore2.Data.Entities`
- `Data/Entities/Industries/Industry.cs` → `namespace ERPCore2.Data.Entities`
- `Data/Context/AppDbContext.cs` → `namespace ERPCore2.Data.Context`
- `Data/Enums/CommonEnums.cs` → `namespace ERPCore2.Data.Enums`

## 開發注意事項

1. **命名空間一致性**：所有新增的實體類別都必須遵循廣域命名規範
2. **資料夾分類**：新增實體時請根據業務領域建立適當的資料夾分類
3. **實體設計**：每個實體類別都應包含適當的資料驗證屬性和導航屬性
4. **文檔維護**：新增實體分類時請同步更新此 README 文檔

## 相關檔案說明

- `Context/AppDbContext.cs`：Entity Framework 資料庫上下文設定
- `Enums/CommonEnums.cs`：系統共用的列舉定義
- `SeedData.cs`：資料庫初始化資料種子檔案