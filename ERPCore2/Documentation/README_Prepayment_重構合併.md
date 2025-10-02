# 預先款項實體重構 - 合併 Prepaid 與 Prepayment

## 📋 重構概述

**日期**: 2025年10月2日  
**目的**: 將 `Prepaid`(預付款) 和 `Prepayment`(預收款) 兩個獨立實體合併為統一的 `Prepayment` 實體

## 🎯 重構原因

1. **業務邏輯相似性**: 預付款和預收款本質上都是「預先支付/收取的款項」，只是方向相反
2. **避免重複代碼**: 兩者的欄位結構幾乎完全相同（日期、金額、備註等）
3. **統一管理更直觀**: 現實業務中就是「預收/預付/其他」三種選擇，用一個實體搭配類型欄位更符合業務思維
4. **擴展性更好**: 未來如需新增其他類型，只需擴展 enum，不需要新增實體
5. **減少維護成本**: 降低代碼重複，統一管理邏輯

## 📊 資料庫變更

### 原有結構

#### Prepaid（預付款）
- `PrepaidDate` - 預付款日期
- `PrepaidAmount` - 預付款金額
- `SupplierId` (必填) - 供應商ID

#### Prepayment（預收款）
- `PrepaymentDate` - 預收款日期
- `PrepaymentAmount` - 預收款金額
- `SetoffId` (必填) - 應收沖款單ID

### 新的統一結構

#### Prepayment（統一預先款項）
- `PrepaymentType` (新增) - 款項類型（預收/預付/其他）
- `PaymentDate` (重命名) - 款項日期
- `Amount` (重命名) - 款項金額
- `CustomerId` (新增, 可為null) - 客戶ID（預收款時使用）
- `SupplierId` (新增, 可為null) - 供應商ID（預付款時使用）
- `SetoffId` (修改為可為null) - 應收沖款單ID（預收款時使用）

## 🔧 技術實作

### 1. 新增 PrepaymentType 列舉

**檔案**: `Data/Enums/PrepaymentType.cs`

```csharp
public enum PrepaymentType
{
    [Display(Name = "預收款")]
    Prepayment = 1,
    
    [Display(Name = "預付款")]
    Prepaid = 2,
    
    [Display(Name = "其他")]
    Other = 3
}
```

### 2. 重構 Prepayment 實體

**檔案**: `Data/Entities/FinancialManagement/Prepayment.cs`

**主要變更**:
- 新增 `PrepaymentType` 欄位來區分款項類型
- 將所有關聯欄位改為可為 null（`CustomerId`, `SupplierId`, `SetoffId`）
- 重命名欄位以更通用：`PrepaymentDate` → `PaymentDate`、`PrepaymentAmount` → `Amount`
- 新增三個導航屬性：`Customer`、`Supplier`、`AccountsReceivableSetoff`

### 3. 更新 DbContext

**檔案**: `Data/Context/AppDbContext.cs`

**變更**:
- 移除 `DbSet<Prepaid> Prepaids`
- 保留 `DbSet<Prepayment> Prepayments`
- 更新 `modelBuilder` 配置，新增所有關聯和索引

### 4. 資料遷移

**Migration**: `20251002031444_MergePrepaidIntoPrepayment`

#### Up 方法執行步驟:

1. **重命名欄位**
   - `PrepaymentDate` → `PaymentDate`
   - `PrepaymentAmount` → `Amount`

2. **修改欄位屬性**
   - 將 `SetoffId` 改為可為 null

3. **新增欄位**
   - `PrepaymentType` (預設值: 1 = 預收款)
   - `CustomerId` (可為 null)
   - `SupplierId` (可為 null)

4. **資料遷移**
   - 更新現有 Prepayments 資料的 `PrepaymentType = 1` (預收款)
   - 將 Prepaids 表的所有資料插入到 Prepayments，並設定 `PrepaymentType = 2` (預付款)

5. **刪除舊表**
   - 刪除 Prepaids 表

6. **建立索引和外鍵**
   - 新增 `CustomerId`、`SupplierId`、`PrepaymentType` 索引
   - 新增客戶和供應商的外鍵關聯

#### Down 方法（回滾支援）:

如需回滾，會執行相反的操作：
1. 重建 Prepaids 表
2. 將預付款資料從 Prepayments 遷移回 Prepaids
3. 從 Prepayments 刪除預付款資料
4. 移除新增的欄位和索引
5. 還原欄位名稱和屬性

## ✅ 驗證結果

資料庫遷移已成功執行，確認：
- ✅ Prepaids 表已刪除
- ✅ Prepayments 表結構已更新
- ✅ 所有資料已正確遷移
- ✅ 索引和外鍵已正確建立
- ✅ 編譯無錯誤

## 📝 使用指南

### 建立預收款範例

```csharp
var prepayment = new Prepayment
{
    PrepaymentType = PrepaymentType.Prepayment, // 預收款
    PaymentDate = DateTime.Today,
    Amount = 10000,
    CustomerId = 1, // 指定客戶
    SetoffId = 5,   // 關聯沖款單（可選）
    Remarks = "客戶預付訂金"
};
```

### 建立預付款範例

```csharp
var prepaid = new Prepayment
{
    PrepaymentType = PrepaymentType.Prepaid, // 預付款
    PaymentDate = DateTime.Today,
    Amount = 20000,
    SupplierId = 10, // 指定供應商
    Remarks = "支付供應商預付款"
};
```

### 查詢範例

```csharp
// 查詢所有預收款
var prepayments = await _context.Prepayments
    .Where(p => p.PrepaymentType == PrepaymentType.Prepayment)
    .Include(p => p.Customer)
    .ToListAsync();

// 查詢所有預付款
var prepaids = await _context.Prepayments
    .Where(p => p.PrepaymentType == PrepaymentType.Prepaid)
    .Include(p => p.Supplier)
    .ToListAsync();
```

## 🔍 驗證邏輯建議

建議在 Service 層或實體中新增驗證邏輯：

```csharp
public class PrepaymentValidator
{
    public static bool Validate(Prepayment prepayment, out string errorMessage)
    {
        errorMessage = string.Empty;
        
        switch (prepayment.PrepaymentType)
        {
            case PrepaymentType.Prepayment: // 預收款
                if (!prepayment.CustomerId.HasValue && !prepayment.SetoffId.HasValue)
                {
                    errorMessage = "預收款必須指定客戶或沖款單";
                    return false;
                }
                break;
                
            case PrepaymentType.Prepaid: // 預付款
                if (!prepayment.SupplierId.HasValue)
                {
                    errorMessage = "預付款必須指定供應商";
                    return false;
                }
                break;
        }
        
        return true;
    }
}
```

## 🎉 重構效益

1. **代碼簡化**: 將兩個實體合併為一個，減少重複代碼
2. **維護成本降低**: 統一管理邏輯，只需維護一個實體
3. **擴展性提升**: 新增其他款項類型只需修改 enum
4. **業務邏輯更清晰**: 更符合實際業務流程
5. **資料查詢更靈活**: 可以在單一表中查詢所有預先款項

## 📌 注意事項

1. **外鍵關聯**: 
   - 預收款使用 `CustomerId` 或 `SetoffId`
   - 預付款使用 `SupplierId`
   - 務必根據類型填寫正確的關聯欄位

2. **資料驗證**: 
   - 建議在應用層實作驗證邏輯
   - 確保每種類型都填寫了必要的關聯欄位

3. **既有代碼**: 
   - 如有使用 Prepaid 實體的代碼需要更新
   - 目前專案中尚無 Service 或 Component 使用這兩個實體，因此無需額外修改

4. **Migration 可回滾**: 
   - 已實作完整的 Down 方法
   - 如有問題可以回滾到合併前的狀態

## 🔄 回滾方法

如需回滾到重構前的狀態：

```bash
dotnet ef database update 20251002030115_RemoveCompanyIdFromPrepayment
dotnet ef migrations remove
```

## 📚 相關檔案

- `Data/Enums/PrepaymentType.cs` - 款項類型列舉
- `Data/Entities/FinancialManagement/Prepayment.cs` - 統一的預先款項實體
- `Data/Context/AppDbContext.cs` - 資料庫上下文配置
- `Migrations/20251002031444_MergePrepaidIntoPrepayment.cs` - 資料遷移檔案
