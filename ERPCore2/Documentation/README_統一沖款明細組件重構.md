# 統一沖款明細組件重構說明

## 📋 重構目標

將應收帳款和應付帳款的沖款明細管理組件統一為單一通用組件，減少程式碼重複，提升可維護性。

## ✅ 已完成工作

### 1. 統一資料表欄位名稱 ✅
- **SalesOrderDetail.Subtotal** → **SubtotalAmount**
- **SalesReturnDetail.ReturnSubtotal** → **ReturnSubtotalAmount** 
- **PurchaseReturnDetail.ReturnSubtotal** → **ReturnSubtotalAmount**

### 2. 修改 AccountsPayableSetoffDetail ✅
- **PurchaseOrderDetailId** → **PurchaseReceivingDetailId**
- 更新所有導航屬性和計算屬性
- 修正 DocumentType, DocumentNumber, ProductName, Quantity, UnitName 等計算屬性

### 3. 建立 SetoffMode 列舉 ✅
```csharp
public enum SetoffMode
{
    Receivable = 1,  // 應收帳款
    Payable = 2      // 應付帳款
}
```

### 4. 擴展 SetoffDetailDto ✅
- 新增 `Mode` 屬性 (SetoffMode)
- 新增 `PartnerId` 和 `PartnerName` (統一的合作對象屬性)
- 新增 `SupplierId` 和 `SupplierName` (應付帳款專用)
- 保留 `CustomerId` 和 `CustomerName` (向後相容，標記為 Obsolete)
- 新增支援 "PurchaseReceiving" 和 "PurchaseReturn" 類型

### 5. 建立通用組件 SetoffDetailManagerComponent ✅
- 支援兩種模式：應收帳款 (Receivable) 和應付帳款 (Payable)
- 參數:
  - `Mode`: 設定應收或應付模式
  - `PartnerId`: 統一的合作對象 ID (建議使用)
  - `CustomerId`: 向後相容 (僅應收帳款)
  - `SupplierId`: 應付帳款專用
- 動態 UI:
  - `GetModeClass()`: 根據模式回傳 CSS 類別
  - `GetEmptyMessage()`: 根據模式顯示適當的空白訊息
- 參數追蹤優化，避免不必要的重新渲染

## ⚠️ 待處理工作

### 1. 修正所有對舊欄位名稱的引用 🔴 **必須完成**

需要全域搜尋並替換以下內容：

#### 在 Services 和 Components 中：

**SalesOrderDetail:**
```csharp
// 舊寫法
detail.Subtotal
salesOrderDetails.Sum(d => d.Subtotal)

// 新寫法
detail.SubtotalAmount
salesOrderDetails.Sum(d => d.SubtotalAmount)
```

**SalesReturnDetail:**
```csharp
// 舊寫法
detail.ReturnSubtotal

// 新寫法
detail.ReturnSubtotalAmount
```

**PurchaseReturnDetail:**
```csharp
// 舊寫法
detail.ReturnSubtotal

// 新寫法
detail.ReturnSubtotalAmount
```

#### 受影響的檔案清單（共31個錯誤）:
1. `Services/Sales/SalesOrderService.cs`
2. `Services/Sales/SalesOrderDetailService.cs`
3. `Services/Sales/SalesReturnService.cs`
4. `Services/Sales/SalesReturnDetailService.cs`
5. `Services/Purchase/PurchaseReturnDetailService.cs`
6. `Services/FinancialManagement/AccountsReceivableSetoffService.cs`
7. `Services/FinancialManagement/AccountsReceivableSetoffDetailService.cs`
8. `Components/Shared/SubCollections/SalesOrderDetailManagerComponent.razor`
9. `Components/Pages/Sales/SalesOrderEditModalComponent.razor`

### 2. 更新 AccountsReceivableSetoffDetailService 🔴 **建議優化**

將 Service 中使用 `CustomerId` 和 `CustomerName` 的地方改為使用 `PartnerId` 和 `PartnerName`，以消除 Obsolete 警告。

### 3. 建立應付帳款 Service 🟡 **待實作**

建立以下檔案：
- `Services/FinancialManagement/IAccountsPayableSetoffDetailService.cs`
- `Services/FinancialManagement/AccountsPayableSetoffDetailService.cs`

參考 `AccountsReceivableSetoffDetailService` 的實作，但處理的是：
- `PurchaseReceivingDetail` (取代 SalesOrderDetail)
- `PurchaseReturnDetail` (對應 SalesReturnDetail)

### 4. 修改通用組件的 Service 注入 🟡 **待實作**

```csharp
@inject IAccountsReceivableSetoffDetailService AccountsReceivableSetoffDetailService
@inject IAccountsPayableSetoffDetailService AccountsPayableSetoffDetailService

// 在 LoadDetailsAsync 中根據 Mode 選擇適當的 Service
private async Task LoadDetailsAsync()
{
    var service = Mode == SetoffMode.Receivable 
        ? (dynamic)AccountsReceivableSetoffDetailService 
        : (dynamic)AccountsPayableSetoffDetailService;
    
    // ... 使用 service 載入資料
}
```

### 5. 生成並執行 Migration 🔴 **必須完成**

```powershell
dotnet ef migrations add UnifySetoffDetailFieldsAndAddSetoffMode
dotnet ef database update
```

### 6. 更新現有頁面 🟡 **待處理**

搜尋所有使用 `AccountsReceivableSetoffDetailManagerComponent` 的頁面，評估是否需要改用新的 `SetoffDetailManagerComponent`。

使用方式：
```razor
@* 應收帳款使用方式 (向後相容) *@
<SetoffDetailManagerComponent 
    Mode="SetoffMode.Receivable"
    CustomerId="@customerId"
    OnSelectedDetailsChanged="HandleDetailsChanged" />

@* 應收帳款使用方式 (建議) *@
<SetoffDetailManagerComponent 
    Mode="SetoffMode.Receivable"
    PartnerId="@customerId"
    OnSelectedDetailsChanged="HandleDetailsChanged" />

@* 應付帳款使用方式 *@
<SetoffDetailManagerComponent 
    Mode="SetoffMode.Payable"
    PartnerId="@supplierId"
    OnSelectedDetailsChanged="HandleDetailsChanged" />
```

### 7. 測試 🟢 **最後步驟**

- [ ] 測試應收帳款沖款功能
- [ ] 測試應付帳款沖款功能
- [ ] 測試編輯模式
- [ ] 測試新增模式
- [ ] 測試驗證邏輯
- [ ] 測試折讓功能

## 📊 資料表對照表

### 應收帳款 vs 應付帳款

| 用途 | 應收帳款 | 應付帳款 |
|------|---------|---------|
| **沖款明細表** | AccountsReceivableSetoffDetail | AccountsPayableSetoffDetail |
| **正常單據** | SalesOrderDetail (銷貨) | PurchaseReceivingDetail (進貨) |
| **退貨單據** | SalesReturnDetail (銷退) | PurchaseReturnDetail (採退) |
| **合作對象** | Customer (客戶) | Supplier (供應商) |

### 欄位對照

| 概念 | 應收帳款欄位 | 應付帳款欄位 | DTO 統一欄位 |
|------|------------|------------|-------------|
| 應收/應付金額 | ReceivableAmount | PayableAmount | Amount |
| 累計金額 | AfterReceivedAmount | AfterPaidAmount | AfterAmount |
| 合作對象ID | CustomerId | SupplierId | PartnerId |
| 合作對象名稱 | CustomerName | SupplierName | PartnerName |

## 🎯 實作優先順序

1. **立即處理** (阻擋 Migration):
   - 修正所有 Subtotal → SubtotalAmount
   - 修正所有 ReturnSubtotal → ReturnSubtotalAmount

2. **核心功能** (完成基本重構):
   - 建立應付帳款 Service
   - 完成組件的 Service 切換邏輯

3. **資料庫更新**:
   - 生成 Migration
   - 執行 Migration
   - 驗證資料庫結構

4. **測試與優化**:
   - 功能測試
   - 效能測試
   - 程式碼清理

## 💡 設計決策

### 為什麼選擇擴展 DTO 而不是泛型？

1. **簡單性**: DTO 擴展比泛型組件更容易理解和維護
2. **靈活性**: 可以為應收/應付帳款提供特定的屬性和方法
3. **向後相容**: 透過 Obsolete 屬性保持向後相容性
4. **UI 一致性**: 使用相同的組件和 UI 邏輯

### 為什麼保留 CustomerId/SupplierId？

1. **向後相容**: 避免破壞現有程式碼
2. **清晰性**: 在特定情境下，使用明確的名稱更清楚
3. **漸進式遷移**: 允許逐步遷移到新的 PartnerId

## 📝 後續建議

1. **CSS 樣式差異化**: 為應收/應付帳款添加不同的顏色主題
2. **服務抽象化**: 考慮建立 `ISetoffDetailService<TDetail>` 統一介面
3. **文件更新**: 更新所有相關的技術文件和使用說明
4. **效能優化**: 評估大量資料載入時的效能表現

## 🔗 相關文件

- [應收沖款明細管理組件](README_應收沖款明細管理組件.md)
- [應收帳款_折讓與財務表](README_應收帳款_折讓與財務表.md)
- [InteractiveTableComponent](README_InteractiveTableComponent.md)

---

**最後更新**: 2025-10-02
**狀態**: 🟡 進行中 (約完成 60%)
