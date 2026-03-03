# 進銷存表單設計規範

## 更新日期
2026-03-03（審核欄位設計更新：ApprovedByDisplayName、showApprovalSection）

---

## 📋 元件職責

### 主組件：`XxxEditModalComponent.razor`

職責：協調所有子元件，管理主檔與明細的資料流。

**關鍵欄位：**
```csharp
// 明細資料
private List<XxxDetail> details;
private XxxTable? detailManager;

// 狀態旗標
private bool isManualApproval;      // 是否人工審核（false = 系統自動審核）
private bool showApprovalSection;   // 是否顯示審核資訊欄位（由系統參數 HideApprovalInfoSection 控制）
private bool hasUndeletableDetails;
private int _detailsDataVersion;    // 遞增觸發 Table 重載
```

**DataVersion 使用模式：**
```csharp
// 父元件儲存或切換上下筆後：
_detailsDataVersion++;
// Table 的 OnParametersSetAsync 偵測到 DataVersion 變化，自動重載
```

### 明細元件：`XxxTable.razor`

職責：管理商品明細列表、業務規則、欄位定義。

**對外事件：**

| EventCallback | 說明 |
|--------------|------|
| `OnDetailsChanged` | 明細變更時通知父元件（含金額計算） |
| `OnHasUndeletableDetailsChanged` | 不可刪除狀態變更時通知父元件（觸發欄位鎖定） |
| `OnOpenRelatedDocument` | 使用者點擊查看子進銷存時通知父元件開啟 Modal（部分模組） |

**公開方法：**

| 方法 | 說明 | 來源 |
|------|------|------|
| `RefreshDetailsAsync()` | 重載明細（子進銷存儲存後呼叫） | 繼承自基底類別 |

---

## 🔒 表單欄位鎖定邏輯

鎖定由 `ApprovalConfigHelper.ShouldLockFieldByApproval()` 統一判斷：

```
shouldLock = (isApprovalEnabled && IsApproved == true)
          OR hasUndeletableDetails == true
```

**鎖定時的行為：**
- 所有主表單欄位（單號、廠商/客戶、公司、日期、稅別、備註）→ `IsReadOnly = true`
- 對象欄位的 ActionButtons（新增/編輯按鈕）→ 回傳空列表
- Table 欄位：商品、數量、單價、稅率 → `IsDisabledFunc` 依 `DetailLockHelper` 判斷
- Table 欄位：狀態欄（完成進貨等）、備註 → **鎖定後仍可編輯**

**警告訊息（`FormHeaderContent`）：**
- 審核通過 → 顯示 `EditModalMessages.XxxApprovedWarning`
- 有下游記錄 → 顯示 `EditModalMessages.UndeletableDetailsWarning`

---

## 💰 金額計算流程

```
使用者編輯明細（數量 / 單價 / 稅率）
    ↓
XxxTable.NotifyDetailsChangedAsync()
    ↓ OnDetailsChanged
XxxEditModalComponent.HandleDetailsChanged()
    ↓
TaxCalculationHelper.CalculateFromDetails()
（依稅別：外加稅 / 內含稅 / 免稅）
    ↓
entity.TotalAmount                        ← 未稅總計
entity.XxxTaxAmount                       ← 稅額
entity.XxxTotalAmountIncludingTax         ← 含稅合計（唯讀顯示）
```

---

## ✅ 審核欄位設計

### 資料庫欄位（所有 7 個模組）

| 欄位 | 說明 |
|------|------|
| `ApprovedBy` | 審核者 Employee ID（FK） |
| `ApprovedAt` | 審核時間 |
| `IsApproved` | 是否核准（true = 核准） |
| `RejectReason` | 駁回原因 |

### 計算屬性（`[NotMapped]`）

```csharp
public string ApprovalStatusText =>
    IsApproved ? "已核准" :
    !string.IsNullOrEmpty(RejectReason) ? "已駁回" : "待審核";

public string? ApprovedAtText => ApprovedAt?.ToString("yyyy-MM-dd HH:mm");

// 審核者顯示名稱：null ApprovedBy = 系統自動審核
public string ApprovedByDisplayName =>
    IsApproved ? (ApprovedByUser?.Name ?? "系統自動審核") : "";
```

### 審核 / 駁回行為

| 動作 | ApprovedBy | ApprovedAt | IsApproved | RejectReason |
|------|-----------|-----------|------------|-------------|
| 人工核准 | = approvedBy（員工 ID） | = DateTime.Now | = true | = null（清除舊原因） |
| 系統自動核准 | = **null**（不記錄人員） | = DateTime.Now | = true | = null |
| 駁回 | = rejectedBy | = DateTime.Now | = false | = reason |

> **重要**：駁回時同樣記錄 `ApprovedBy`（誰駁回）和 `ApprovedAt`（何時駁回），欄位語義為「審核者」和「審核時間」，不限於核准場景。
>
> `ApprovedBy = null` 表示由系統自動審核，畫面顯示 `ApprovedByDisplayName` 屬性中的「系統自動審核」文字。

### 表單欄位顯示

```csharp
// 審核區塊欄位（showApprovalSection && Id > 0 時顯示）
{ PropertyName = "ApprovalStatusText",   Label = L["Approval.Status"] }
{ PropertyName = "ApprovedByDisplayName", Label = L["Approval.ApprovedBy"] }  // 人工審核者或「系統自動審核」
{ PropertyName = "ApprovedAtText",        Label = L["Approval.ApprovedAt"] }  // 審核時間
{ PropertyName = nameof(RejectReason),    Label = L["Approval.RejectReason"] }
```

> `showApprovalSection` 由 `SystemParameter.HideApprovalInfoSection` 控制（false = 顯示，true = 隱藏），
> 與 `isManualApproval` 無關，審核資訊欄位預設對所有模式都顯示。

---

## ➕ 新增模式的特殊處理

新增模式時：
- 「轉子單」按鈕禁用
- 「複製訊息」按鈕禁用
- 儲存成功後啟用上述按鈕

部分模組支援從其他頁面帶入預填資料（`ShowAddModalWithPrefilledData`）：
- 預填對象 ID、商品 ID、建議單價
- `LoadXxxData()` 讀取預填值並建立第一筆明細

---

## 🔗 相關文件

- [README_進銷存模組設計總綱.md](README_進銷存模組設計總綱.md)
- [README_BaseDetailTableComponent設計.md](README_BaseDetailTableComponent設計.md)
- [README_進銷存模組實作指南.md](README_進銷存模組實作指南.md)
- [../README_審核_各模組狀態.md](../README_審核_各模組狀態.md)
- [../Readme_審核機制設計說明.md](../Readme_審核機制設計說明.md)
