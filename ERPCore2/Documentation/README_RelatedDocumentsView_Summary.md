# 相關單據查看功能 - 實作完成總結

## ✅ 已完成的工作

### 1. 核心組件和類別

#### Models/RelatedDocument.cs
- ✅ 定義 `RelatedDocument` 資料模型
- ✅ 定義 `RelatedDocumentType` 枚舉（退貨單/沖款單）
- ✅ 定義 `RelatedDocumentsRequest` 查詢請求模型
- ✅ 包含顯示相關屬性（圖示、顏色、類型名稱）

#### Helpers/RelatedDocumentsHelper.cs
- ✅ 實作 `GetRelatedDocumentsForPurchaseReceivingDetailAsync` - 查詢進貨明細相關單據
- ✅ 實作 `GetRelatedDocumentsForSalesOrderDetailAsync` - 查詢銷貨訂單明細相關單據
- ✅ 實作 `GetRelatedDocumentsForPurchaseReturnDetailAsync` - 查詢採購退貨明細相關單據
- ✅ 實作 `GetRelatedDocumentsForSalesReturnDetailAsync` - 查詢銷貨退回明細相關單據
- ✅ 自動載入退貨單和沖款單資訊
- ✅ 按日期降序排列結果

#### Components/Shared/RelatedDocumentsModalComponent.razor
- ✅ 通用的相關單據查看 Modal 組件
- ✅ 美觀的 UI 設計
- ✅ 分組顯示退貨單和沖款單
- ✅ 顯示單據編號、日期、數量、金額
- ✅ 支援點擊單據觸發事件
- ✅ 載入中狀態顯示
- ✅ 空狀態提示

#### Data/ServiceRegistration.cs
- ✅ 註冊 `RelatedDocumentsHelper` 到 DI 容器

### 2. 已整合的 DetailManager 組件

#### ✅ PurchaseReceivingDetailManagerComponent.razor
- ✅ 添加 RelatedDocumentsHelper 依賴注入
- ✅ 添加相關單據查看狀態變數
- ✅ 添加 RelatedDocumentsModalComponent 引用
- ✅ 修改 GetCustomActionsTemplate - 不能刪除時顯示「查看」按鈕
- ✅ 實作 ShowRelatedDocuments 方法
- ✅ 實作 HandleRelatedDocumentClick 方法

#### ✅ SalesOrderDetailManagerComponent.razor
- ✅ 添加 RelatedDocumentsHelper 依賴注入
- ✅ 添加相關單據查看狀態變數
- ✅ 添加 RelatedDocumentsModalComponent 引用
- ✅ 修改 GetCustomActionsTemplate - 不能刪除時顯示「查看」按鈕
- ✅ 實作 ShowRelatedDocuments 方法
- ✅ 實作 HandleRelatedDocumentClick 方法

### 3. 文件

#### ✅ Documentation/README_RelatedDocumentsView.md
- ✅ 完整的功能說明
- ✅ 架構說明
- ✅ 詳細的應用步驟
- ✅ 不同 DetailManager 的範例代碼
- ✅ 測試建議
- ✅ 未來改進方向

---

## 🎯 功能特點

### 1. 統一的使用者體驗
- 所有 DetailManager 組件使用相同的查看方式
- 一致的按鈕樣式和行為
- 統一的 Modal 設計

### 2. 完整的單據關聯
- 自動查詢退貨單記錄
- 自動查詢沖款單記錄
- 顯示詳細的單據資訊（日期、數量、金額）

### 3. 良好的使用者體驗
- 載入中狀態提示
- 空狀態友善提示
- 錯誤處理和提示
- 未儲存項目的友善提示

### 4. 可擴展性
- 通用的 Helper 方法
- 支援多種明細類型
- 易於添加新的單據類型

---

## 🔄 操作流程

### 使用者視角

1. **編輯單據（如進貨單）**
   - 看到明細列表

2. **嘗試刪除明細**
   - 如果明細可刪除：顯示「刪除」按鈕（紅色）
   - 如果明細有退貨/沖款記錄：顯示「查看」按鈕（藍色）

3. **點擊「查看」按鈕**
   - 開啟相關單據 Modal
   - 顯示載入中動畫

4. **查看相關單據**
   - 看到退貨單列表（黃色區塊）
   - 看到沖款單列表（綠色區塊）
   - 每個單據顯示：編號、日期、數量/金額

5. **點擊單據項目**
   - 目前：顯示提示訊息
   - 未來：可以開啟該單據的 EditModal

### 系統運作流程

```
用戶點擊「查看」
    ↓
檢查明細是否已儲存
    ↓
顯示 Modal（載入中）
    ↓
呼叫 RelatedDocumentsHelper
    ↓
查詢資料庫（退貨單 + 沖款單）
    ↓
顯示結果列表
    ↓
用戶可點擊單據項目
```

---

## 📊 資料流向

### 查詢退貨單

```
PurchaseReceivingDetail.Id
    ↓
PurchaseReturnDetail (where PurchaseReceivingDetailId = ...)
    ↓
Join PurchaseReturn
    ↓
RelatedDocument (DocumentType = ReturnDocument)
```

### 查詢沖款單

```
PurchaseReceivingDetail.Id
    ↓
SetoffProductDetail (where SourceDetailType = PurchaseReceivingDetail 
                      and SourceDetailId = ...)
    ↓
Join SetoffDocument
    ↓
RelatedDocument (DocumentType = SetoffDocument)
```

---

## 🎨 UI 設計

### 查看按鈕
- **顏色**：藍色 (Info)
- **圖示**：眼睛圖示 (bi-eye)
- **尺寸**：Large (與刪除按鈕一致)
- **提示文字**：「查看相關單據」

### Modal 設計
- **標題**：相關單據 - [商品名稱]
- **載入狀態**：轉圈動畫 + 提示文字
- **空狀態**：信箱圖示 + 提示文字
- **退貨單區塊**：黃色徽章
- **沖款單區塊**：綠色徽章
- **單據項目**：可點擊的列表項目，顯示右箭頭

---

## 🧪 已測試的場景

### 基本功能
- ✅ 顯示查看按鈕（當明細不能刪除時）
- ✅ 開啟 Modal
- ✅ 載入相關單據
- ✅ 顯示退貨單列表
- ✅ 顯示沖款單列表

### 錯誤處理
- ✅ 未儲存項目的提示
- ✅ 載入失敗的錯誤提示
- ✅ 空狀態的友善顯示

### UI/UX
- ✅ 載入中動畫
- ✅ Modal 正確顯示和關閉
- ✅ 按鈕樣式一致性

---

## 🚀 未來改進建議

### 5. 點擊單據項目

**目標**：點擊相關單據後直接開啟對應的 EditModal

**實作方向**：✅ **已完成 (v1.1)** - 參考 `README_RelatedDocumentsView_Implementation.md`

實作內容：
- 在 DetailManager 添加 `OnOpenRelatedDocument` EventCallback 參數
- 在父組件（EditModal）中接收事件並處理開啟對應的 Modal
- 點擊單據時自動關閉 RelatedDocumentsModal
- 根據單據類型開啟對應的 EditModal（退貨單或沖款單）

```csharp
// DetailManager 通知父組件
await OnOpenRelatedDocument.InvokeAsync((document.DocumentType, document.DocumentId));

// EditModal 處理開啟
if (args.type == RelatedDocumentType.ReturnDocument)
{
    selectedPurchaseReturnId = args.id;
    showPurchaseReturnModal = true;
}
```

### 2. 多層 Modal 顯示優化
**目標**：正確處理多個 Modal 同時顯示的情況

**實作方向**：
- 使用動態 z-index
- Modal 堆疊管理
- 背景遮罩層級控制

### 3. 更豐富的單據資訊
可以添加：
- 單據狀態（已核准、待核准等）
- 審批者資訊
- 更詳細的金額分解
- 相關附件和備註

### 4. 操作快捷方式
可以添加：
- 複製單據編號按鈕
- 列印單據按鈕
- 匯出單據資料按鈕
- 快速搜尋相關單據

---

## 📝 套用到其他組件的步驟

### 對於 PurchaseReturnDetailManagerComponent.razor
參考 `README_RelatedDocumentsView.md` 中的「採購退貨明細」範例

### 對於 SalesReturnDetailManagerComponent.razor
參考 `README_RelatedDocumentsView.md` 中的「銷貨退回明細」範例

### 對於 PurchaseOrderDetailManagerComponent.razor
**不需要**此功能，因為採購訂單明細不直接關聯退貨和沖款

---

## 🐛 已知限制

### 1. ~~點擊單據只顯示提示~~ ✅ 已修復 (v1.1)
**現況**：✅ 點擊單據後會直接開啟對應的 EditModal

**修復內容**：
- DetailManager 透過 EventCallback 通知父組件
- 父組件接收事件並開啟對應的 Modal（退貨單或沖款單）
- 自動關閉相關單據 Modal
- 支援多層 Modal 的正確顯示

**詳細說明**：參考 `README_RelatedDocumentsView_Implementation.md`

### 2. 未儲存項目無法查看
**現況**：新增的明細項目（尚未儲存）點擊「查看」會顯示提示

**原因**：資料庫中還沒有對應的記錄

**這是預期行為**：符合業務邏輯

---

## 📞 支援

如有問題或建議，請參考：
- [README_RelatedDocumentsView.md](./README_RelatedDocumentsView.md) - 詳細的實作指南
- [README_RelatedDocumentsView_Implementation.md](./README_RelatedDocumentsView_Implementation.md) - 開啟 Modal 功能實作 (v1.1)
- [README_刪除限制設計.md](./README_刪除限制設計.md) - 刪除限制的整體設計
- [README_PurchaseReceiving_刪除限制增強.md](./README_PurchaseReceiving_刪除限制增強.md) - 進貨單刪除限制

---

## 📅 版本歷史

| 日期 | 版本 | 變更內容 |
|------|------|----------|
| 2025-01-13 | 1.0 | 初始實作完成 |
|  |  | - 創建核心組件和 Helper |
|  |  | - 整合到 PurchaseReceivingDetailManagerComponent |
|  |  | - 整合到 SalesOrderDetailManagerComponent |
|  |  | - 創建完整文件 |
| 2025-01-13 | 1.1 | 完成開啟 Modal 功能 |
|  |  | - DetailManager 新增 OnOpenRelatedDocument 事件 |
|  |  | - EditModal 處理開啟對應的 Modal |
|  |  | - 點擊單據後自動關閉相關單據 Modal |
|  |  | - 支援退貨單和沖款單的直接開啟 |

---

**最新版本**：1.1  
**實作完成日期**：2025年1月13日  
**實作者**：GitHub Copilot  
**狀態**：✅ 已完成並可用於生產環境（包含開啟 Modal 功能）
