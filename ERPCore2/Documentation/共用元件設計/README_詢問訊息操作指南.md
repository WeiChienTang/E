# 詢問訊息操作指南

## 概述

本指南說明如何使用 `GenericConfirmModalComponent` 通用確認對話框組件，取代原生 JavaScript `confirm()` 對話框，提供更美觀且一致的使用者體驗。

## 組件位置

```
Components/Shared/UI/GenericConfirmModalComponent.razor
```

## 使用時機

- 需要使用者確認「是」或「否」的操作
- 需要顯示條件清單讓使用者了解即將執行的操作內容
- 需要比原生 `confirm()` 更美觀的對話框樣式
- 需要顯示多行文字或格式化內容

## 基本用法

### 步驟 1：在頁面中宣告組件

```razor
<!-- 確認 Modal -->
<GenericConfirmModalComponent IsVisible="@_showConfirmModal"
                             IsVisibleChanged="@((bool visible) => _showConfirmModal = visible)"
                             Title="確認操作"
                             Message="您確定要執行此操作嗎？"
                             ConfirmButtonText="確認"
                             CancelButtonText="取消"
                             OnConfirm="@HandleConfirm"
                             OnCancel="@(() => _showConfirmModal = false)" />
```

### 步驟 2：在 @code 區塊中添加狀態和方法

```csharp
@code {
    // 控制 Modal 顯示狀態
    private bool _showConfirmModal = false;
    
    // 顯示確認 Modal
    private void ShowConfirmModal()
    {
        _showConfirmModal = true;
        StateHasChanged();
    }
    
    // 處理確認按鈕點擊
    private async Task HandleConfirm()
    {
        // 執行確認後的操作
        await DoSomething();
    }
}
```

## 進階用法：顯示條件清單

適用於需要讓使用者了解操作條件的場景，例如載入資料、批次處理等。

### 範例：載入確認

```razor
<GenericConfirmModalComponent IsVisible="@_showLoadConfirmModal"
                             IsVisibleChanged="@((bool visible) => _showLoadConfirmModal = visible)"
                             Title="載入確認"
                             Icon="bi-box-arrow-in-down"
                             Message="即將載入以下條件的資料："
                             Conditions="@_loadConditions"
                             SummaryMessage="@_loadSummaryMessage"
                             ConfirmButtonText="確認載入"
                             CancelButtonText="取消"
                             ConfirmButtonVariant="ButtonVariant.Blue"
                             OnConfirm="@ExecuteLoad"
                             OnCancel="@(() => _showLoadConfirmModal = false)" />
```

```csharp
@code {
    private bool _showLoadConfirmModal = false;
    private List<string> _loadConditions = new();
    private string _loadSummaryMessage = string.Empty;
    
    private async Task ShowLoadConfirmModal()
    {
        // 建立條件清單
        _loadConditions = new List<string>
        {
            $"廠商：{supplierName}",
            $"採購單：{orderCode}",
            $"商品篩選：{productName}"
        };
        
        // 計算資料筆數
        int count = await GetDataCountAsync();
        _loadSummaryMessage = $"共 {count} 筆，是否確定載入？";
        
        // 顯示確認 Modal
        _showLoadConfirmModal = true;
        StateHasChanged();
    }
    
    private async Task ExecuteLoad()
    {
        // 執行載入邏輯
        await LoadDataAsync();
    }
}
```

## 組件參數說明

| 參數 | 類型 | 預設值 | 說明 |
|------|------|--------|------|
| `IsVisible` | `bool` | `false` | 控制 Modal 是否顯示 |
| `IsVisibleChanged` | `EventCallback<bool>` | - | 顯示狀態變更事件 |
| `Title` | `string` | `"確認"` | Modal 標題 |
| `Icon` | `string` | `"bi-question-circle"` | 標題圖示（Bootstrap Icon） |
| `Message` | `string?` | `null` | 主訊息文字 |
| `Conditions` | `List<string>?` | `null` | 條件清單（每項會顯示勾選圖示） |
| `SummaryMessage` | `string?` | `null` | 總計/摘要訊息（粗體藍色顯示） |
| `ConfirmButtonText` | `string` | `"確認"` | 確認按鈕文字 |
| `CancelButtonText` | `string` | `"取消"` | 取消按鈕文字 |
| `ConfirmButtonVariant` | `ButtonVariant` | `ButtonVariant.Blue` | 確認按鈕樣式 |
| `HeaderColor` | `HeaderVariant` | `HeaderVariant.Primary` | 標題列顏色 |
| `OnConfirm` | `EventCallback` | - | 確認按鈕點擊事件 |
| `OnCancel` | `EventCallback` | - | 取消按鈕點擊事件 |
| `ChildContent` | `RenderFragment?` | `null` | 自訂內容區塊 |

## 標題列顏色選項 (HeaderVariant)

| 值 | 說明 |
|----|------|
| `Default` | 預設白底黑字 |
| `Primary` | Bootstrap 藍色 |
| `Secondary` | Bootstrap 灰色 |
| `Success` | Bootstrap 綠色 |
| `Danger` | Bootstrap 紅色 |
| `Warning` | Bootstrap 黃色 |
| `Info` | Bootstrap 淺藍色 |
| `Dark` | Bootstrap 深色 |
| `ProjectPrimary` | 專案主色 #1F2937 |

## 按鈕樣式選項 (ButtonVariant)

| 值 | 說明 |
|----|------|
| `Blue` | 藍色（主要操作） |
| `Green` | 綠色（成功/確認） |
| `Red` | 紅色（危險/刪除） |
| `Gray` | 灰色（取消/次要） |
| `DarkBlue` | 深藍色 |
| `Cyan` | 青色 |
| `Purple` | 紫色 |
| `Pink` | 粉紅色 |
| `Orange` | 橙色 |
| `Black` | 黑色 |
| `White` | 白色 |
| `Outline*` | 以上所有顏色的輪廓版本 (如 OutlineBlue, OutlineCyan 等) |

## 實際應用範例

### 範例 1：簡單確認

```razor
<GenericConfirmModalComponent IsVisible="@_showDeleteConfirm"
                             IsVisibleChanged="@((bool v) => _showDeleteConfirm = v)"
                             Title="刪除確認"
                             Icon="bi-trash"
                             HeaderColor="BaseModalComponent.HeaderVariant.Danger"
                             Message="確定要刪除此項目嗎？此操作無法復原。"
                             ConfirmButtonText="確定刪除"
                             ConfirmButtonVariant="ButtonVariant.Red"
                             OnConfirm="@DeleteItem" />
```

### 範例 2：批次操作確認

```razor
<GenericConfirmModalComponent IsVisible="@_showBatchConfirm"
                             IsVisibleChanged="@((bool v) => _showBatchConfirm = v)"
                             Title="批次處理確認"
                             Icon="bi-list-check"
                             Message="即將執行以下批次操作："
                             Conditions="@(new List<string> {
                                 \"更新 15 筆訂單狀態\",
                                 \"發送通知信件\",
                                 \"記錄操作日誌\"
                             })"
                             SummaryMessage="是否確定執行？"
                             ConfirmButtonText="執行"
                             OnConfirm="@ExecuteBatch" />
```

### 範例 3：自訂內容

```razor
<GenericConfirmModalComponent IsVisible="@_showCustomConfirm"
                             IsVisibleChanged="@((bool v) => _showCustomConfirm = v)"
                             Title="選擇選項"
                             OnConfirm="@HandleCustomConfirm">
    <div class="mb-3">
        <label class="form-label">請選擇處理方式：</label>
        <select class="form-select" @bind="_selectedOption">
            <option value="1">選項 A</option>
            <option value="2">選項 B</option>
        </select>
    </div>
</GenericConfirmModalComponent>
```

## 與原生 confirm() 的比較

| 特性 | 原生 confirm() | GenericConfirmModalComponent |
|------|---------------|------------------------------|
| 外觀 | 瀏覽器預設樣式 | 可自訂、與系統風格一致 |
| 條件清單 | 不支援 | ✓ 支援 |
| 格式化內容 | 僅純文字 | ✓ 支援 HTML/Blazor |
| 自訂按鈕 | 不支援 | ✓ 支援 |
| 圖示 | 不支援 | ✓ 支援 |
| 非同步操作 | 阻塞式 | ✓ 非阻塞 |

## 注意事項

1. **狀態管理**：記得在 `OnConfirm` 和 `OnCancel` 事件後，Modal 會自動關閉，不需手動設置 `IsVisible = false`

2. **防重複提交**：如果確認操作需要較長時間，建議在 `OnConfirm` 中添加 loading 狀態

3. **條件驗證**：在顯示確認 Modal 前，先驗證必要條件是否滿足：
   ```csharp
   private async Task ShowConfirmModal()
   {
       if (!IsConditionMet())
       {
           await NotificationService.ShowWarningAsync("請先滿足必要條件");
           return;
       }
       
       _showConfirmModal = true;
   }
   ```

4. **動態條件**：條件清單和摘要訊息應在顯示 Modal 前動態計算，確保資訊準確

## 相關檔案

- 組件：`Components/Shared/UI/GenericConfirmModalComponent.razor`
- 基礎 Modal：`Components/Shared/Base/BaseModalComponent.razor`
- 按鈕組件：`Components/Shared/UI/Button/GenericButtonComponent.razor`

## 應用實例

- `PurchaseReceivingTable.razor` - 進貨載入確認
- 可擴展至其他需要確認操作的場景
