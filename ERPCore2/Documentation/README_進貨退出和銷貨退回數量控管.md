# 進貨退出和銷貨退回數量控管機制

## 📋 問題描述

目前系統在進貨退出和銷貨退回的設計上存在數量控管漏洞：

### 進貨退出問題
1. **PurchaseReceivingDetail**（進貨明細）沒有記錄累計退貨數量
2. 可以建立多張退貨單，總退貨數量可能超過進貨數量
3. 無法快速得知某個進貨明細還能退多少
4. 只能透過 `PurchaseReturnDetail.PurchaseReceivingDetailId` 反向查詢，需要 JOIN + SUM，效能差

**範例情境：**
- 進貨20個商品
- 建立退貨單A退10個 ✓
- 建立退貨單B退15個 ✓（應該要被阻止，但目前沒有）
- 總退貨數量25個 > 進貨數量20個 ❌

### 銷貨退回問題（預期也有類似問題）
1. **SalesInvoiceDetail**（銷貨明細）可能也沒有記錄累計退貨數量
2. 可以建立多張退貨單，總退貨數量可能超過出貨數量
3. 需要類似的控管機制

---

## 🎯 解決方案

### 設計理念
參考系統現有的設計模式（訂單追蹤進貨數量、進貨追蹤付款金額），在來源明細中增加**累計退貨數量**欄位。

### 方案優勢
✅ 符合現有設計模式（`ReceivedQuantity`、`TotalPaidAmount` 等）  
✅ 查詢效能好，不需要 JOIN  
✅ 容易維護數據一致性  
✅ 使用者體驗好，可即時看到可退數量  
✅ 驗證規則簡單明確  

---

## 🔧 進貨退出修改計劃

### 1. Entity 層修改

#### PurchaseReceivingDetail.cs
新增以下欄位：

```csharp
[Display(Name = "累計退貨數量")]
public int TotalReturnedQuantity { get; set; } = 0;

[Display(Name = "剩餘可退數量")]
[NotMapped]
public int RemainingReturnableQuantity => ReceivedQuantity - TotalReturnedQuantity;
```

**說明：**
- `TotalReturnedQuantity`：儲存在資料庫，追蹤累計退貨數量
- `RemainingReturnableQuantity`：計算屬性，不儲存，用於顯示和驗證

#### 建立 Migration
```bash
dotnet ef migrations add AddTotalReturnedQuantityToPurchaseReceivingDetail
dotnet ef database update
```

---

### 2. Service 層修改

#### PurchaseReturnService.cs
在退貨單儲存/刪除時，同步更新進貨明細的累計退貨數量。

**新增退貨時：**
```csharp
// 更新進貨明細的累計退貨數量
receivingDetail.TotalReturnedQuantity += returnDetail.ReturnQuantity;
```

**刪除退貨時：**
```csharp
// 回退進貨明細的累計退貨數量
receivingDetail.TotalReturnedQuantity -= returnDetail.ReturnQuantity;
```

**修改退貨數量時：**
```csharp
// 先回退舊數量，再加上新數量
receivingDetail.TotalReturnedQuantity -= oldQuantity;
receivingDetail.TotalReturnedQuantity += newQuantity;
```

#### PurchaseReceivingDetailService.cs
可能需要新增查詢方法（根據現有實作決定）。

---

### 3. UI 層修改

#### PurchaseReceivingTable.razor
在明細表格中顯示退貨相關資訊：

**新增欄位：**
1. **已退數量** - 顯示 `TotalReturnedQuantity`

**範例顯示：**
```
商品編號/名稱 | 採購數量 | 入庫數量 | 已退數量 | 單價 | 稅率% | 小計 | 倉庫 | ...
```

**欄位配置建議：**
- 已退數量：唯讀欄位，灰色顯示
- 當已退數量 > 0 時，以特殊樣式提示（如：藍色文字或資訊圖示）
- 當已退數量 = 入庫數量時，以警告樣式顯示（如：橙色文字，表示已全部退貨）

#### PurchaseReceivingEditModalComponent.razor
- 確保在載入明細時正確顯示退貨數量
- 已有 `LoadReturnedQuantitiesAsync()` 方法，需要改為直接讀取 `TotalReturnedQuantity` 欄位（不再需要 JOIN 查詢）

#### PurchaseReturnEditModalComponent.razor
**轉退貨驗證邏輯：**

```csharp
// 從進貨單選擇商品時，檢查可退數量
if (receivingDetail.RemainingReturnableQuantity <= 0)
{
    await NotificationService.ShowWarningAsync(
        $"商品「{product.Name}」已無可退數量");
    return;
}

// 輸入退貨數量時，驗證
if (returnQuantity > receivingDetail.RemainingReturnableQuantity)
{
    await NotificationService.ShowErrorAsync(
        $"退貨數量 {returnQuantity} 超過剩餘可退數量 {receivingDetail.RemainingReturnableQuantity}");
    return;
}
```

**顯示改進：**
- 在選擇進貨明細時，顯示「已退數量」和「剩餘可退數量」資訊（提示文字，不是表格欄位）
- 退貨數量輸入框的 `max` 屬性設為 `RemainingReturnableQuantity`
- 當可退數量不足時，顯示明確的錯誤訊息

---

### 4. 驗證邏輯

#### 退貨數量驗證
在多個層級進行驗證：

**前端驗證（UI層）：**
- 輸入框限制最大值
- 即時提示可退數量

**Service層驗證：**
```csharp
public async Task<bool> ValidateReturnQuantityAsync(
    int receivingDetailId, int returnQuantity)
{
    var receivingDetail = await _context.PurchaseReceivingDetails
        .FindAsync(receivingDetailId);
    
    if (receivingDetail == null)
        return false;
    
    if (returnQuantity > receivingDetail.RemainingReturnableQuantity)
    {
        throw new InvalidOperationException(
            $"退貨數量 {returnQuantity} 超過剩餘可退數量 " +
            $"{receivingDetail.RemainingReturnableQuantity}");
    }
    
    return true;
}
```

**資料庫約束（可選）：**
```sql
ALTER TABLE PurchaseReceivingDetails
ADD CONSTRAINT CK_TotalReturnedQuantity_Valid
CHECK (TotalReturnedQuantity >= 0 AND TotalReturnedQuantity <= ReceivedQuantity);
```

---

### 5. 資料遷移處理

#### 現有資料的處理
對於已存在的進貨明細，需要計算並回填累計退貨數量：

```csharp
// 在 Migration 的 Up 方法中執行
public partial class AddTotalReturnedQuantityToPurchaseReceivingDetail : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // 1. 新增欄位
        migrationBuilder.AddColumn<int>(
            name: "TotalReturnedQuantity",
            table: "PurchaseReceivingDetails",
            nullable: false,
            defaultValue: 0);
        
        // 2. 回填現有資料的退貨數量
        migrationBuilder.Sql(@"
            UPDATE PurchaseReceivingDetails
            SET TotalReturnedQuantity = (
                SELECT ISNULL(SUM(ReturnQuantity), 0)
                FROM PurchaseReturnDetails
                WHERE PurchaseReturnDetails.PurchaseReceivingDetailId = PurchaseReceivingDetails.Id
            )
        ");
    }
}
```

---

## 🔄 銷貨退回修改計劃（類似結構）

### 1. Entity 層修改

#### SalesInvoiceDetail.cs
新增以下欄位：

```csharp
[Display(Name = "累計退貨數量")]
public int TotalReturnedQuantity { get; set; } = 0;

[Display(Name = "剩餘可退數量")]
[NotMapped]
public int RemainingReturnableQuantity => ShippedQuantity - TotalReturnedQuantity;
```

### 2. Service 層修改

#### SalesReturnService.cs
- 退貨儲存時更新 `SalesInvoiceDetail.TotalReturnedQuantity`
- 退貨刪除時回退數量

### 3. UI 層修改

#### SalesInvoiceTable.razor
- 顯示「已退數量」欄位（唯讀）

#### SalesReturnEditModalComponent.razor
- 驗證退貨數量不超過可退數量
- 在選擇銷貨明細時，顯示「已退數量」和「剩餘可退數量」資訊（提示文字）

---

## 📊 資料完整性檢查

### 檢查現有資料的一致性
建立 SQL 查詢檢查是否有退貨數量異常：

```sql
-- 檢查進貨明細的退貨數量是否超過進貨數量
SELECT 
    prd.Id,
    prd.Code AS 進貨單號,
    p.Code AS 商品編號,
    p.Name AS 商品名稱,
    prd.ReceivedQuantity AS 進貨數量,
    ISNULL(SUM(prtd.ReturnQuantity), 0) AS 累計退貨數量,
    prd.ReceivedQuantity - ISNULL(SUM(prtd.ReturnQuantity), 0) AS 可退數量
FROM PurchaseReceivingDetails prd
INNER JOIN Products p ON prd.ProductId = p.Id
LEFT JOIN PurchaseReturnDetails prtd ON prtd.PurchaseReceivingDetailId = prd.Id
GROUP BY prd.Id, prd.Code, p.Code, p.Name, prd.ReceivedQuantity
HAVING ISNULL(SUM(prtd.ReturnQuantity), 0) > prd.ReceivedQuantity
ORDER BY prd.Id;
```

---

## ✅ 測試檢查清單

### 進貨退出測試

- [ ] 建立進貨單，進貨數量 20
- [ ] 建立退貨單，退貨數量 10（應成功，剩餘可退 10）
- [ ] 再建立退貨單，退貨數量 15（應失敗，超過可退數量）
- [ ] 修改第一張退貨單，數量改為 5（應成功，剩餘可退 15）
- [ ] 刪除第一張退貨單（應成功，剩餘可退 20）
- [ ] 在進貨明細表格中正確顯示「已退數量」
- [ ] 已有退貨記錄的進貨明細應被鎖定，無法刪除
- [ ] 轉退貨功能應正確計算和驗證可退數量（顯示為提示文字）

### 銷貨退回測試（進貨退出測試完成後）

- [ ] 建立銷貨單，出貨數量 30
- [ ] 建立退貨單，退貨數量 10（應成功，剩餘可退 20）
- [ ] 再建立退貨單，退貨數量 25（應失敗，超過可退數量）
- [ ] 修改第一張退貨單，數量改為 15（應成功，剩餘可退 15）
- [ ] 刪除第一張退貨單（應成功，剩餘可退 30）
- [ ] 在銷貨明細表格中正確顯示「已退數量」

### 資料遷移測試

- [ ] Migration 正確建立欄位
- [ ] 現有資料的退貨數量正確回填
- [ ] 沒有退貨記錄的明細，TotalReturnedQuantity = 0
- [ ] 有退貨記錄的明細，TotalReturnedQuantity = 實際退貨總和

---

## 📝 實作順序建議

### 第一階段：進貨退出（優先）
1. ✅ 修改 `PurchaseReceivingDetail` Entity（新增欄位）
2. ✅ 建立並執行 Migration（含資料回填）
3. ✅ 修改 `PurchaseReturnService`（更新/刪除時同步數量）
4. ✅ 修改 `PurchaseReceivingTable`（顯示退貨數量欄位）
5. ✅ 簡化 `LoadReturnedQuantitiesAsync`（直接讀取欄位，不需 JOIN）
6. ✅ 修改 `PurchaseReturnEditModalComponent`（驗證和提示）
7. ✅ 測試和驗證

### 第二階段：銷貨退回（參考進貨退出）
1. ⏳ 修改 `SalesInvoiceDetail` Entity
2. ⏳ 建立並執行 Migration
3. ⏳ 修改 `SalesReturnService`
4. ⏳ 修改 `SalesInvoiceTable`
5. ⏳ 修改 `SalesReturnEditModalComponent`
6. ⏳ 測試和驗證

---

## 🎯 預期效果

### 使用者體驗改進
- ✅ 明確顯示每筆明細的已退數量
- ✅ 防止退貨數量超過進貨/出貨數量（透過後端計算可退數量驗證）
- ✅ 即時驗證，避免錯誤操作
- ✅ 提供清晰的錯誤訊息

### 系統架構改進
- ✅ 數據一致性保障
- ✅ 查詢效能提升（不需要 JOIN + SUM）
- ✅ 符合系統現有設計模式
- ✅ 易於維護和擴展

### 資料完整性改進
- ✅ 退貨數量受到嚴格控管
- ✅ 無法建立超量退貨單
- ✅ 歷史資料可追溯
- ✅ 支援多層級驗證

---

## 📚 相關文件

- [README_進貨明細鎖定主檔欄位.md](README_進貨明細鎖定主檔欄位.md) - 明細鎖定機制
- [README_A單轉B單.md](README_A單轉B單.md) - 單據轉換機制
- 資料庫設計文件（待補充）

---

## 🔍 注意事項

1. **併發控制**：多人同時建立退貨單時，需要資料庫層級的鎖定機制
2. **權限控制**：確保只有有權限的使用者才能建立退貨單
3. **稽核記錄**：記錄退貨數量變更的歷史（透過現有的 Audit 機制）
4. **報表影響**：確認現有報表是否需要顯示「已退數量」欄位
5. **效能監控**：Migration 回填大量資料時可能耗時，建議在非營業時間執行

---

**文件版本：** 1.0  
**建立日期：** 2025-12-11  
**最後更新：** 2025-12-11  
**負責人：** 開發團隊
