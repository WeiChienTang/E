# E - ERP系統

## 專案說明

這是一個基於 .NET Core 的企業資源規劃 (ERP) 系統，提供銷售、採購、庫存、財務等管理功能。

## 技術架構

- **後端**: ASP.NET Core
- **前端**: Blazor Server
- **資料庫**: SQL Server
- **ORM**: Entity Framework Core

## 主要功能模組

### 銷售管理 (Sales)
- 銷貨訂單管理
- 銷貨出貨管理
- 銷貨退回管理
- 報價單管理

### 採購管理 (Purchase)
- 採購訂單管理
- 採購進貨管理
- 採購退回管理
- 批次審核功能

### 庫存管理 (Warehouse)
- 庫存盤點
- 領料單管理
- 庫存查詢

### 商品管理 (Product)
- 商品資料維護
- 商品組成/配方管理
- 商品供應商管理
- 條碼列印

### 財務管理 (Financial)
- 沖銷管理（商品、預付款、付款）
- 稅率計算（外加稅、內含稅、免稅）
- 帳款管理

## 開發資訊

### 系統需求
- .NET 8.0 或更高版本
- SQL Server 2019 或更高版本
- Visual Studio 2022 或 Visual Studio Code

### 建置專案
```bash
cd ERPCore2
dotnet restore
dotnet build
```

### 執行專案
```bash
cd ERPCore2
dotnet run
```

## AI 輔助開發

本專案使用 **GitHub Copilot** 進行程式碼開發與重構，採用 **Claude Sonnet 3.5** 模型。

AI 輔助開發範疇包括：
- 程式碼重構與優化
- 文件撰寫
- Helper 類別設計
- 程式邏輯改善
- 錯誤修復與除錯

## 專案文件

詳細的開發文件請參考：
- [互動Table說明](ERPCore2/Documentation/README_互動Table說明.md)
- [資料載入和渲染問題](ERPCore2/Documentation/README_資料載入和渲染問題.md)
- [A單轉B單](ERPCore2/Documentation/README_A單轉B單.md)
- [稅率欄位改版指南](ERPCore2/Documentation/README_稅率欄位改版指南.md)
- [InteractiveTable重構計劃](ERPCore2/Helpers/InteractiveTableComponentHelper/README_InteractiveTable重構計劃.md)

## 授權

本專案為私有專案，版權所有。

---

**維護者**: WeiChienTang  
**最後更新**: 2025年12月11日  
**AI 模型**: GitHub Copilot (Claude Sonnet 3.5)
