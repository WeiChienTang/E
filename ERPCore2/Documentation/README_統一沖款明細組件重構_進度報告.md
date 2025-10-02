# 統一沖款明細組件重構 - 進度報告

**最後更新**: 2025-10-02
**狀態**: 🟢 核心功能已完成 (約完成 75%)

---

## ✅ 已完成工作

### 1. 統一資料表欄位名稱 ✅

**修改的欄位：**
- `SalesOrderDetail.Subtotal` → `SalesOrderDetail.SubtotalAmount`
- `SalesReturnDetail.ReturnSubtotal` → `SalesReturnDetail.ReturnSubtotalAmount`
- `PurchaseReturnDetail.ReturnSubtotal` → `PurchaseReturnDetail.ReturnSubtotalAmount`

**影響的檔案（已全部修正）：**
- ✅ Data/Entities/Sales/SalesOrderDetail.cs
- ✅ Data/Entities/Sales/SalesReturnDetail.cs
- ✅ Data/Entities/Purchase/PurchaseReturnDetail.cs
- ✅ Services/Sales/SalesOrderService.cs
- ✅ Services/Sales/SalesOrderDetailService.cs
- ✅ Services/Sales/SalesReturnService.cs
- ✅ Services/Sales/SalesReturnDetailService.cs
- ✅ Services/Purchase/PurchaseReturnDetailService.cs
- ✅ Services/FinancialManagement/AccountsReceivableSetoffService.cs
- ✅ Services/FinancialManagement/AccountsReceivableSetoffDetailService.cs (36處修改)
- ✅ Components/Pages/Sales/SalesOrderEditModalComponent.razor
- ✅ Components/Shared/SubCollections/SalesOrderDetailManagerComponent.razor
- ✅ Components/Shared/SubCollections/SalesReturnDetailManagerComponent.razor

### 2. 修改 AccountsPayableSetoffDetail ✅

**變更內容：**
- `PurchaseOrderDetailId` → `PurchaseReceivingDetailId`
- 更新導航屬性：`PurchaseOrderDetail` → `PurchaseReceivingDetail`
- 修正所有計算屬性和方法：
  - `DocumentType`: "PurchaseOrder" → "PurchaseReceiving"
  - `DocumentNumber`: 使用 `PurchaseReceiving.ReceiptNumber`
  - `ProductName`, `Quantity`, `UnitName` 等
  - `IsValid()` 驗證方法

**檔案：**
- ✅ Data/Entities/FinancialManagement/AccountsPayableSetoffDetail.cs

### 3. 建立 SetoffMode 列舉 ✅

```csharp
public enum SetoffMode
{
    Receivable = 1,  // 應收帳款
    Payable = 2      // 應付帳款
}
```

**檔案：**
- ✅ Data/Enums/SetoffMode.cs

### 4. 擴展 SetoffDetailDto ✅

**新增屬性：**
- `Mode` (SetoffMode): 區分應收/應付模式
- `PartnerId` (int): 統一的合作對象 ID
- `PartnerName` (string): 統一的合作對象名稱
- `SupplierId` / `SupplierName`: 應付帳款專用屬性
- `CustomerId` / `CustomerName`: 向後相容（標記為 Obsolete）

**支援的類型：**
- "SalesOrder" (銷貨訂單)
- "SalesReturn" (銷貨退回)
- "PurchaseReceiving" (採購進貨) ✨ 新增
- "PurchaseReturn" (採購退回) ✨ 新增

**檔案：**
- ✅ Models/SetoffDetailDto.cs

### 5. 建立通用組件 SetoffDetailManagerComponent ✅

**功能：**
- 支援兩種模式：`SetoffMode.Receivable` 和 `SetoffMode.Payable`
- 統一的參數介面：
  - `Mode`: 設定模式
  - `PartnerId`: 統一的合作對象 ID（建議使用）
  - `CustomerId`: 向後相容（僅應收帳款）
  - `SupplierId`: 應付帳款專用
- 動態 UI：
  - `GetModeClass()`: 回傳 "receivable" 或 "payable" CSS 類別
  - `GetEmptyMessage()`: 根據模式顯示適當訊息
- 優化的參數追蹤，避免不必要的重新渲染
- 自動設定每個 DTO 的 Mode 屬性

**檔案：**
- ✅ Components/Shared/SubCollections/SetoffDetailManagerComponent.razor（新建）
- ℹ️ Components/Shared/SubCollections/AccountsReceivableSetoffDetailManagerComponent.razor（保留作為舊版）

### 6. 生成並執行 Migration ✅

**Migration 名稱：** `UnifySetoffDetailFieldsAndAddSetoffMode`

**資料庫變更：**
```sql
-- 重新命名欄位
EXEC sp_rename 'SalesReturnDetails.ReturnSubtotal', 'ReturnSubtotalAmount', 'COLUMN';
EXEC sp_rename 'SalesOrderDetails.Subtotal', 'SubtotalAmount', 'COLUMN';
```

**執行狀態：** ✅ 成功執行

---

## ⚠️ 待處理工作

### 1. 建立應付帳款 Service 🟡 高優先級

需要建立以下檔案：
- `Services/FinancialManagement/IAccountsPayableSetoffDetailService.cs`
- `Services/FinancialManagement/AccountsPayableSetoffDetailService.cs`

**參考實作：** `AccountsReceivableSetoffDetailService.cs`

**主要差異：**
- 處理 `PurchaseReceivingDetail` (取代 SalesOrderDetail)
- 處理 `PurchaseReturnDetail` (對應 SalesReturnDetail)
- 合作對象從 `Customer` 改為 `Supplier`

### 2. 完善通用組件的 Service 整合 🟡 高優先級

**需要修改：** `SetoffDetailManagerComponent.razor`

```csharp
// 在 LoadDetailsAsync 中根據 Mode 選擇適當的 Service
@inject IAccountsReceivableSetoffDetailService ReceivableService
@inject IAccountsPayableSetoffDetailService PayableService

private async Task LoadDetailsAsync()
{
    if (Mode == SetoffMode.Receivable)
    {
        Details = await ReceivableService.GetCustomerPendingDetailsAsync(...);
    }
    else
    {
        Details = await PayableService.GetSupplierPendingDetailsAsync(...);
    }
}
```

### 3. 消除 Obsolete 警告 🟢 低優先級

**受影響檔案：**
- `Services/FinancialManagement/AccountsReceivableSetoffDetailService.cs`
  - 第 682, 683, 731, 732 行

**修改方式：**
```csharp
// 舊寫法
CustomerId = customerId,
CustomerName = detail.SalesOrder.Customer?.CompanyName ?? "",

// 新寫法
PartnerId = customerId,
PartnerName = detail.SalesOrder.Customer?.CompanyName ?? "",
```

### 4. 更新現有頁面 🟢 低優先級

搜尋並更新所有使用 `AccountsReceivableSetoffDetailManagerComponent` 的頁面。

**使用方式：**
```razor
@* 應收帳款 (建議使用新組件) *@
<SetoffDetailManagerComponent 
    Mode="SetoffMode.Receivable"
    PartnerId="@customerId"
    OnSelectedDetailsChanged="HandleDetailsChanged" />

@* 應付帳款 *@
<SetoffDetailManagerComponent 
    Mode="SetoffMode.Payable"
    PartnerId="@supplierId"
    OnSelectedDetailsChanged="HandleDetailsChanged" />
```

### 5. 測試 🟡 必要

- [ ] 測試應收帳款沖款功能（新增模式）
- [ ] 測試應收帳款沖款功能（編輯模式）
- [ ] 測試應付帳款沖款功能（待實作 Service 後）
- [ ] 測試驗證邏輯
- [ ] 測試折讓功能
- [ ] 測試向後相容性

---

## 📊 完成度統計

| 任務 | 狀態 | 完成度 |
|------|------|--------|
| 統一資料表欄位名稱 | ✅ | 100% |
| 修改 AccountsPayableSetoffDetail | ✅ | 100% |
| 建立 SetoffMode 列舉 | ✅ | 100% |
| 擴展 SetoffDetailDto | ✅ | 100% |
| 建立通用組件 | ✅ | 100% |
| 生成並執行 Migration | ✅ | 100% |
| 建立應付帳款 Service | 🟡 | 0% |
| 更新現有頁面 | 🟡 | 0% |
| 測試功能完整性 | 🟡 | 0% |

**整體完成度：** 約 75%

---

## 🎯 下一步行動計畫

### 立即行動（建議順序）

1. **建立應付帳款 Service**
   - 複製 `AccountsReceivableSetoffDetailService.cs`
   - 修改為處理 `PurchaseReceivingDetail` 和 `PurchaseReturnDetail`
   - 修改合作對象從 Customer 改為 Supplier

2. **整合 Service 到組件**
   - 在 `SetoffDetailManagerComponent` 中注入兩個 Service
   - 根據 Mode 動態選擇使用哪個 Service
   - 測試模式切換功能

3. **註冊 Service**
   - 在 `Data/ServiceRegistration.cs` 中註冊新的 Service

4. **功能測試**
   - 測試應收帳款功能（確保沒有破壞現有功能）
   - 測試應付帳款功能（新功能）

### 可選優化

- 消除 Obsolete 警告（低優先級）
- 更新現有頁面使用新組件（漸進式）
- 新增 CSS 樣式差異化（應收/應付不同顏色）
- 撰寫單元測試

---

## 🔍 技術亮點

### 1. 向後相容性設計

通過 `Obsolete` 屬性標記舊的 API，同時提供新的統一 API：

```csharp
[Obsolete("請使用 PartnerId 代替")]
public int CustomerId 
{ 
    get => Mode == SetoffMode.Receivable ? PartnerId : 0;
    set => PartnerId = value;
}
```

### 2. 動態 UI

組件根據 `Mode` 動態調整顯示內容：
- CSS 類別
- 空白訊息
- 欄位標籤（未來可擴展）

### 3. 統一資料結構

透過 `SetoffDetailDto` 統一處理四種不同的明細類型，大幅簡化業務邏輯。

---

## 📝 注意事項

1. **資料庫已更新**：欄位名稱已經改變，舊的程式碼會編譯失敗
2. **向後相容**：舊組件 `AccountsReceivableSetoffDetailManagerComponent` 仍然保留
3. **警告訊息**：4 個 Obsolete 警告不影響功能，可以後續優化
4. **Service 尚未完成**：應付帳款的 Service 尚未實作，組件無法用於應付帳款

---

## 📖 相關文件

- [統一沖款明細組件重構說明](README_統一沖款明細組件重構.md)
- [應收沖款明細管理組件](README_應收沖款明細管理組件.md)
- [應收帳款_折讓與財務表](README_應收帳款_折讓與財務表.md)

---

**建立者**: GitHub Copilot  
**版本**: v1.0  
**最後更新**: 2025-10-02 13:19 UTC+8
