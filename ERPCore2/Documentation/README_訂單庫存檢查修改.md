# 訂單庫存檢查功能重構

## 修改日期
2025年12月9日

## 問題分析

### 現況問題
1. **資訊分散**
   - 明細表 (`SalesOrderTable.razor`): 只能查看產品本身的庫存
   - 組合編輯 (`SalesOrderCompositionEditModal.razor`): 只能查看該組合元件的庫存
   - 使用者需要在兩個不同位置來回查看，無法獲得完整資訊

2. **缺乏整體視角**
   - 無法一次性看到整張訂單的庫存狀況
   - 無法同時檢視直接產品和組合產品中各元件的庫存
   - 難以判斷整體訂單是否有足夠庫存

3. **操作繁瑣**
   - 需要逐一點開每個明細或組合才能檢查
   - 對於明細較多的訂單特別不便
   - 增加使用者的認知負擔

## 解決方案

### 核心概念
在 `SalesOrderEditModalComponent.razor` 新增「檢視庫存」按鈕，提供統一的庫存檢視介面。

### 功能位置
- **位置**: `SalesOrderEditModalComponent.razor` 的功能按鈕區
- **並列按鈕**: 與「轉銷貨」按鈕並列
- **邏輯**: 訂單層級的全局功能

### 功能規劃

#### 1. 基本功能 (階段一)
- **樹狀結構顯示**
  - 第一層: 訂單明細 (產品、訂購數量)
  - 第二層: 組合產品展開顯示元件及所需數量
  - 可展開/收合設計

- **庫存對比資訊**
  - 所需數量 (訂單需求)
  - 現有庫存 (可用庫存)
  - 缺口數量 (不足時顯示)

- **視覺化狀態指標**
  - 🟢 綠色: 庫存充足
  - 🟡 黃色: 庫存警戒 (低於安全庫存)
  - 🔴 紅色: 庫存不足

- **彙總資訊**
  - 整張訂單的庫存滿足率
  - 不足項目數量統計
  - 關鍵缺貨提示

#### 2. 進階功能 (階段二)
- **倉庫維度**
  - 顯示各倉庫的庫存分布
  - 可切換顯示總庫存或分倉庫庫存

- **時間維度**
  - 當前庫存狀況
  - 預計交貨日的預估庫存 (考慮其他訂單佔用)

- **智能建議**
  - 替代料件建議
  - 建議採購數量
  - 預計到貨時間

## 技術實作

### 新增元件
```
InventoryCheckModal.razor           # 庫存檢視主要彈窗
InventoryCheckModalComponent.razor  # 庫存檢視元件邏輯
```

### 資料結構
```csharp
public class OrderInventoryCheckItem
{
    public int Level { get; set; }                    // 層級 (1=明細, 2=組成)
    public int? ParentDetailId { get; set; }          // 父層明細ID
    public int ProductId { get; set; }                // 產品ID
    public string ProductCode { get; set; }           // 產品編號
    public string ProductName { get; set; }           // 產品名稱
    public decimal RequiredQuantity { get; set; }     // 需求數量
    public decimal AvailableStock { get; set; }       // 可用庫存
    public decimal ShortageQuantity { get; set; }     // 缺口數量
    public InventoryStatus Status { get; set; }       // 庫存狀態
    public List<OrderInventoryCheckItem> Children { get; set; }  // 子項目
}

public enum InventoryStatus
{
    Sufficient,   // 充足
    Warning,      // 警戒
    Insufficient  // 不足
}
```

### Service 方法
```csharp
// SalesOrderService.cs
public async Task<OrderInventoryCheckResult> GetOrderInventoryCheckAsync(int salesOrderId)
{
    // 1. 取得訂單明細
    // 2. 展開組合產品的元件
    // 3. 查詢各產品/元件的庫存
    // 4. 計算需求與庫存差異
    // 5. 產生樹狀結構資料
}
```

### UI 顯示範例
```
訂單編號: SO-2025001  整體滿足率: 75% ⚠️

📦 產品A (產品)
   所需: 10 個 | 庫存: 15 個 | 狀態: 🟢 充足

📦 產品B (組合)
   所需: 5 組 | 狀態: 🔴 不足
   └─ 🔩 元件B1
      所需: 15 個 (5組 × 3個) | 庫存: 10 個 | 缺口: 5 個 | 狀態: 🔴 不足
   └─ 🔩 元件B2
      所需: 5 個 (5組 × 1個) | 庫存: 8 個 | 狀態: 🟢 充足

📦 產品C (產品)
   所需: 20 個 | 庫存: 18 個 | 缺口: 2 個 | 狀態: 🔴 不足

---
⚠️ 警告: 2 個項目庫存不足
```

## 實作步驟

### 第一階段: 基本功能
- [ ] 1. 建立 `InventoryCheckModal.razor` 彈窗元件
- [ ] 2. 建立 `InventoryCheckModalComponent.razor` 邏輯元件
- [ ] 3. 建立資料模型 `OrderInventoryCheckItem`
- [ ] 4. 在 `SalesOrderService` 新增庫存檢查方法
- [ ] 5. 實作訂單明細與組合元件的展開邏輯
- [ ] 6. 實作庫存查詢與對比計算
- [ ] 7. 實作樹狀結構顯示 UI
- [ ] 8. 實作顏色狀態標示
- [ ] 9. 在 `SalesOrderEditModalComponent.razor` 新增「檢視庫存」按鈕
- [ ] 10. 測試基本功能

### 第二階段: 進階功能
- [ ] 1. 新增倉庫維度查詢
- [ ] 2. 新增時間維度預估
- [ ] 3. 新增智能建議功能
- [ ] 4. 效能優化 (快取、延遲載入)
- [ ] 5. 完整測試

## 效能考量

### 潛在問題
- 訂單明細數量多
- 組合產品層級深
- 需要查詢大量庫存資料

### 優化方案
1. **按需載入**: 預設只載入第一層，展開時才載入子層
2. **快取機制**: 短時間內重複查詢使用快取
3. **批次查詢**: 一次查詢所有相關產品的庫存，避免 N+1 問題
4. **非同步處理**: 使用 async/await 避免阻塞

## 與現有功能的關係

### 保留現有功能
- `SalesOrderTable.razor` 的庫存檢視: 保留，作為快速檢視
- `SalesOrderCompositionEditModal.razor` 的庫存檢視: 保留，作為編輯時參考

### 使用場景區分
- **明細表快速檢視**: 單一產品的快速確認
- **組合編輯檢視**: 編輯組合時的即時參考
- **統一庫存檢視**: 整張訂單的完整分析和決策支援

## 預期效益

1. **提升使用者體驗**
   - 減少 60% 的點擊次數
   - 降低認知負擔
   - 提供更直觀的決策資訊

2. **減少錯誤**
   - 避免遺漏檢查
   - 提前發現庫存問題
   - 減少無法履行的訂單

3. **提升效率**
   - 加快訂單審核速度
   - 減少來回查詢時間
   - 提升整體作業流程

## 後續擴展可能

1. **列印功能**: 匯出庫存檢查報表
2. **郵件通知**: 庫存不足時自動通知採購
3. **整合採購**: 直接從缺口產生採購建議單
4. **歷史追蹤**: 記錄每次庫存檢查的結果
5. **批次檢查**: 一次檢查多張訂單的庫存狀況

## 注意事項

1. **權限控制**: 確保只有有權限的使用者可以查看庫存資訊
2. **即時性**: 庫存資料需要即時更新，避免誤判
3. **準確性**: 計算需求數量時要考慮單位轉換
4. **使用者教育**: 需要教育使用者新功能的使用方式和時機
