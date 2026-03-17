# 允許草稿功能設計說明

> 最後更新：2026-03-09

---

## 零、已實作模組追蹤

| 模組 | Index | EditModal | 完成日期 |
|------|:-----:|:---------:|---------|
| **Employee（員工）** | ✅ | ✅ | 2026-03-09 |
| **Department（部門）** | ✅ | ✅ | 2026-03-09 |
| **EmployeePosition（職位）** | ✅ | ✅ | 2026-03-09 |
| **Supplier（廠商）** | ✅ | ✅ | 2026-03-09 |
| **Customer（客戶）** | ✅ | ✅ | 2026-03-09 |
| **Product（品項）** | ✅ | ✅ | 2026-03-09 |
| **ProductCategory（品項類型）** | ✅ | ✅ | 2026-03-09 |
| **Unit（單位）** | ✅ | ✅ | 2026-03-09 |
| **Size（尺寸）** | ✅ | ✅ | 2026-03-09 |
| **PurchaseOrder（採購單）** | ✅ | ✅ | 2026-03-09 |
| **PurchaseReceiving（進貨）** | ✅ | ✅ | 2026-03-09 |
| **PurchaseReturn（進貨退出）** | ✅ | ✅ | 2026-03-09 |
| **Quotation（報價）** | ✅ | ✅ | 2026-03-09 |
| **SalesOrder（訂單）** | ✅ | ✅ | 2026-03-09 |
| **SalesDelivery（銷貨）** | ✅ | ✅ | 2026-03-09 |
| **SalesReturn（銷貨退回）** | ✅ | ✅ | 2026-03-09 |
| **Vehicle（車輛）** | ✅ | ✅ | 2026-03-09 |
| **VehicleMaintenance（保養紀錄）** | ✅ | ✅ | 2026-03-09 |
| **VehicleType（車型）** | ✅ | ✅ | 2026-03-09 |

---

## 一、設計概念

使用者按「儲存」時若驗證失敗，系統根據情境決定行為：

| 情境 | 行為 |
|------|------|
| 新增模式 + 驗證失敗 | 彈出「資料不完整」dialog，詢問是否存草稿（不 toast） |
| 草稿編輯模式 + 驗證失敗 | 同上 |
| 正式記錄編輯 + 驗證失敗 | 只 toast 錯誤，不出現 dialog |

- 草稿模式下（新增 / 草稿編輯），`GenericFormComponent` 的次要 Tab 自動禁用
- 草稿儲存跳過前端驗證與 `ValidateAsync()`，但 DB NOT NULL 約束無法跳過

---

## 二、已完成的基礎建設（不需再修改）

以下為一次性修改，已套用至所有模組，**新模組不需要重做**：

| 檔案 | 已做的事 |
|------|---------|
| `Data/BaseEntity.cs` | 新增 `IsDraft bool` 欄位 |
| `Migrations/20260309_AddBaseEntityIsDraft` | 所有資料表加入 `IsDraft bit NOT NULL DEFAULT 0` |
| `IGenericManagementService.cs` | 新增 `GetAllIncludingDraftsAsync()` 介面 |
| `GenericManagementService.cs` | `GetAllAsync()` 過濾草稿；`CreateAsync/UpdateAsync` 加 IsDraft guard（跳過 ValidateAsync） |
| `GenericEditModalComponent.razor` | 草稿確認 modal HTML、`ShowDraftButton` / `DraftSuccessMessage` 參數、`PendingSaveError` / `WasOriginallyDraft` public 屬性、`DisableSecondaryTabs` 傳入 GenericFormComponent |
| `GenericEditModalComponent.Save.cs` | `HandleSave()` / `GenericSave()` 草稿 dialog 邏輯、`wasOriginallyDraft` 捕捉與還原 |
| `GenericFormComponent.razor.cs/.razor/.razor.css` | `DisableSecondaryTabs` 參數 + 禁用 Tab 渲染 + CSS 樣式 |
| `GenericIndexPageComponent.razor/.DataLoader.cs/.razor.css` | `ShowDraftTab` 參數、正式/草稿切換按鈕、`ApplyFilters()` IsDraft 過濾 |
| 所有 resx 檔（5 語言） | `Button.SaveDraft`、`Label.Draft`、`Label.Official`、`Modal.SaveAsDraftTitle/Message` 等 i18n 鍵 |

---

## 三、新模組套用草稿功能（每個模組各做一次）

### 步驟 1：Index 頁

**檔案：** `Components/Pages/[Module]/[Module]Index.razor`

1. `DataLoader` 改用 `GetAllIncludingDraftsAsync()`（原本是 `GetAllAsync()`）
2. `GenericIndexPageComponent` 加上 `ShowDraftTab="true"`

```razor
<GenericIndexPageComponent ...
                          DataLoader="@LoadXxxAsync"
                          ShowDraftTab="true" />
```

```csharp
private async Task<List<Xxx>> LoadXxxAsync()
    => await DataLoaderHelper.LoadAsync(
        () => XxxService.GetAllIncludingDraftsAsync(), ...);
```

> ⚠️ 若繼續用 `GetAllAsync()`，草稿 Tab 永遠空白（該方法已過濾草稿）。

---

### 步驟 2：EditModal

**檔案：** `Components/Pages/[Module]/[Module]EditModal/[Module]EditModalComponent.razor`

在 `GenericEditModalComponent` 加上兩個參數：

```razor
<GenericEditModalComponent ...
                          ShowDraftButton="true"
                          DraftSuccessMessage="XXX草稿已儲存，請記得補齊必填資料" />
```

---

### 步驟 3：SaveHandler 修改（使用 SaveHandler 才需要；UseGenericSave 可跳過）

SaveHandler 需同時做三件事：

**① 加 `if (!entity.IsDraft)` guard** — 草稿重試時帶 `IsDraft=true` 呼叫，不可再進驗證

**② 收集所有錯誤到 List** — 不要逐一 `return false`，要先收集完再判斷

**③ 用 `isDraftEligible` 決定錯誤去向** — 草稿情境寫入 `PendingSaveError`，正式編輯才 toast

```csharp
private async Task<bool> SaveXxx(Xxx entity)
{
    try
    {
        if (!entity.IsDraft)
        {
            bool isDraftEligible = editModalComponent?.ShowDraftButton == true &&
                                   (!XxxId.HasValue || editModalComponent?.WasOriginallyDraft == true);

            var errors = new List<string>();

            // 所有必填驗證統一收集，不要逐一 return false
            if (string.IsNullOrWhiteSpace(entity.SomeField))
                errors.Add("XXX為必填");
            // ... 其他驗證 ...

            if (errors.Any())
            {
                if (isDraftEligible)
                    editModalComponent!.PendingSaveError = string.Join("; ", errors);
                else
                    await NotificationService.ShowErrorAsync(string.Join("；", errors));
                return false;
            }
        }

        ServiceResult result = XxxId.HasValue
            ? await XxxService.UpdateAsync(entity)
            : await XxxService.CreateAsync(entity);

        return result.IsSuccess;
    }
    catch (Exception)
    {
        _ = NotificationService.ShowErrorAsync("儲存時發生錯誤");
        return false;
    }
}
```

> **注意**：錯誤用 `"; "` 連接（半形分號），草稿 dialog 用 `Split(';', ...)` 拆成 `<li>` 逐項顯示。

---

### UseGenericSave vs SaveHandler 差異

| | UseGenericSave | SaveHandler |
|---|---|---|
| 草稿 dialog 觸發 | `GenericSave()` 偵測 service 失敗 | `HandleSave()` 偵測 SaveHandler 返回 false |
| 錯誤來源 | `serviceResult.ErrorMessage` | SaveHandler 寫入 `PendingSaveError` |
| 需修改 SaveHandler | ❌ | ✅（guard + 收集錯誤 + PendingSaveError） |
| 草稿資格條件 | `ShowDraftButton && (!Id.HasValue \|\| wasOriginallyDraft)` | 同左 |

---

### 步驟 4：確認 DB 欄位 nullable

草稿儲存時跳過驗證，但 DB NOT NULL 約束無法繞過。需確認實體中**使用者可能未填的欄位**都允許 null。

#### 檢查規則

| 欄位類型 | 需要 nullable？ | 做法 |
|---------|:-------------:|------|
| `string` 文字欄位 | ✅ 若使用者可能不填 | 改為 `string?` |
| `int` FK 外鍵 | ✅ 若關聯非必填 | 改為 `int?` |
| `DateTime` 日期 | ✅ 若日期可為空 | 改為 `DateTime?` |
| `enum` 列舉 | ✅ 若可未選 | 改為 `Enum?` |
| `bool` / `int`（有預設值）| ❌ 不需要 | 保持原樣，EF 使用預設值 |

若有欄位需要從 NOT NULL 改為 nullable，需新增 Migration：
```bash
dotnet ef migrations add Make[Module]FieldsNullable
dotnet ef database update
```

#### Employee 欄位 nullable 狀態（已確認 ✅，不需要紀錄各表的狀態，Employee欄位只是作為一個參考表）

Employee.cs 所有可能未填的欄位均已為 nullable，草稿儲存不會發生 NOT NULL 例外：

| 欄位 | 類型 | 狀態 |
|------|------|:----:|
| `Name`、`Account`、`Password` | `string?` | ✅ |
| `DepartmentId`、`EmployeePositionId`、`RoleId` | `int?` | ✅ |
| `BirthDate`、`HireDate`、`ResignationDate`、`LastLoginAt` | `DateTime?` | ✅ |
| `Gender`、`MaritalStatus`、`BloodType` | `Enum?` | ✅ |
| 其他文字欄位（手機/Email/地址/緊急聯絡等） | `string?` | ✅ |
| `IsSystemUser`、`IsSuperAdmin`、`FailedLoginAttempts` | `bool/int`（有預設值）| ✅ 不需改 |
| `EmployeeType`、`EmploymentStatus` | `Enum`（有預設值）| ✅ 不需改 |

---

## 三之二、含明細 Table 模組的額外注意事項（以 PurchaseOrder 為例）

> 適用：任何使用 **自訂 SaveHandler + 明細 Table** 的模組（PurchaseOrder、PurchaseReceiving、PurchaseReturn、SalesOrder 等）

### A. `CustomValidator` 參數對 SaveHandler 無效

`GenericEditModalComponent` 的 `CustomValidator` 參數**只在 `UseGenericSave` 路線（`GenericSave()` 內）才被呼叫**。
若使用自訂 `SaveHandler`，框架完全不呼叫 `CustomValidator`，設定它沒有任何效果。

> ✅ 結論：有自訂 SaveHandler 的模組，**直接移除 `CustomValidator` 參數**，在 SaveHandler 內部做驗證。

### B. SaveHandler 驗證需使用 `PendingSaveError`（不要 toast）

SaveHandler 中的業務驗證失敗時，**不要直接呼叫 `NotificationService.ShowErrorAsync`**，改為：

```csharp
private async Task<bool> SaveXxxWrapper(Xxx entity)
{
    if (!entity.IsDraft)  // ① 草稿模式跳過所有業務驗證
    {
        if (/* 驗證失敗 */)
        {
            editModalComponent!.PendingSaveError = "錯誤訊息";  // ② 寫入 PendingSaveError（不 toast）
            return false;
        }
    }
    return await SaveXxxWithDetails(entity);
}
```

若直接 toast 錯誤，草稿 dialog 仍會彈出但 `_draftValidationErrors` 為 null，dialog 內容空白。

### C. FK 欄位變成 `int?` 後，所有引用都要更新

將 `SupplierId: int → int?` 後，以下幾類程式碼會出現**編譯錯誤**：

| 位置 | 原始寫法 | 修正寫法 |
|------|---------|---------|
| 傳遞給 `int` 參數的方法 | `DoSomething(entity.SupplierId)` | `DoSomething(entity.SupplierId!.Value)` |
| LINQ `.Contains()` 比較 | `.Contains(po.SupplierId)` | `po.SupplierId.HasValue && .Contains(po.SupplierId.Value)` |
| 設定另一個 `int` 型屬性 | `obj.SupplierId = history.SupplierId` | `obj.SupplierId = history.SupplierId!.Value` |
| 服務層（Report/Recommendation 等）直接取值 | `GetByIdAsync(po.SupplierId)` | `po.SupplierId.HasValue ? GetByIdAsync(po.SupplierId.Value) : null` |

> ⚠️ 注意：改完 Entity 後一定要先 **`dotnet build` 確認無錯誤**，再執行 `dotnet ef migrations add`。
> 若先建 migration 再修 build 錯誤，migration 已寫入磁碟但模型不對，需先 `ef migrations remove`。

### D. FK 為 `int?` 後，Razor 條件判斷需更新

| 原始 | 問題 | 修正 |
|------|------|------|
| `if (entity.SupplierId <= 0)` | `null <= 0` 為 false，null 時條件不成立 | `if (!entity.SupplierId.HasValue \|\| entity.SupplierId.Value <= 0)` |
| `style="@(entity.SupplierId > 0 ? "" : "display:none")"` | `null > 0` 為 false，null 時隱藏，行為正確 | 不需改 |

### E. 含審核自動核准的模組，草稿不應自動審核

```csharp
// ❌ 原本：儲存後無條件自動審核
if (!isManualApproval && !result.Data.IsApproved)
    await Service.ApproveOrderAsync(result.Data.Id, null);

// ✅ 修正：草稿不自動審核
if (!entity.IsDraft && !isManualApproval && !result.Data.IsApproved)
    await Service.ApproveOrderAsync(result.Data.Id, null);
```

### F. 需要搜尋的相關服務檔案（FK 改 nullable 後）

將 FK 改為 `int?` 後，以下類型的服務通常也需要修正：

- **Report Service**（例如 `PurchaseOrderReportService.cs`）：`GetByIdAsync(po.SupplierId)` → `.HasValue ? GetByIdAsync(po.SupplierId.Value) : null`
- **Recommendation / Analysis Service**（例如 `SupplierRecommendationService.cs`）：LINQ `.Contains()` + 物件屬性設定
- **FieldConfiguration**：若有 `po => po.SupplierId` 的 lambda，EF LINQ 會自動處理，通常不需改

---

## 四、測試清單

**新增模式：**
- [ ] 不填欄位點儲存 → 只出現草稿 dialog（無 toast），dialog 內顯示錯誤清單
- [ ] 點「儲存草稿」→ 儲存成功，modal 不關閉，顯示草稿成功訊息
- [ ] 點「繼續編輯」→ dialog 關閉，可繼續填寫
- [ ] 次要 Tab 禁用（不可點擊）

**草稿編輯模式：**
- [ ] 開啟草稿記錄，次要 Tab 禁用
- [ ] 不填完整資料點儲存 → 只出現草稿 dialog（無 toast）
- [ ] 補齊資料點儲存 → IsDraft 變 false，Tab 恢復可用，modal 正常關閉

**正式記錄編輯：**
- [ ] 清空必填欄位點儲存 → 只顯示 toast 錯誤，不出現 dialog
- [ ] 所有 Tab 均可使用

**索引頁：**
- [ ] 預設顯示「正式」Tab，草稿記錄不出現
- [ ] 切換「草稿」Tab 只顯示草稿，Badge 顯示正確筆數

**服務層：**
- [ ] AutoComplete / 下拉選單不出現草稿記錄
- [ ] `GetAllAsync()` 不回傳草稿；`GetAllIncludingDraftsAsync()` 回傳全部

---

## 五、注意事項

- DB NOT NULL FK 的模組（如 `SalesOrder`）須另行規劃欄位 nullable，再套用草稿功能
- 若模組覆寫了 `ValidateAsync()`，不需修改，base class guard 已在前攔截
- `UseGenericSave` 路線不需動 SaveHandler，框架自動處理草稿 dialog
