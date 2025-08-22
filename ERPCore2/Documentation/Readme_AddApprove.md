# ERPCore2 審核功能新增指南

## 概述

本文件說明如何在 ERPCore2 系統中為任何實體新增審核功能。審核功能允許具有特定權限的使用者對資料進行審核、通過或駁回操作。

## 系統架構

審核功能採用組件化設計，主要分為以下層次：

1. **GenericEditModalComponent** - 提供通用審核UI和邏輯
2. **具體實體Modal組件** - 實作具體業務邏輯
3. **Service層** - 處理審核業務邏輯
4. **Entity層** - 實體審核狀態欄位

## 實作步驟

### 步驟 1：準備實體審核欄位

確保您的實體包含審核相關欄位：

```csharp
public class YourEntity : BaseEntity
{
    // 其他業務欄位...
    
    public DateTime? ApprovedAt { get; set; }
    public int? ApprovedById { get; set; }
    public virtual Employee? ApprovedBy { get; set; }
    
    // 可選：駁回相關欄位
    public DateTime? RejectedAt { get; set; }
    public int? RejectedById { get; set; }
    public string? RejectionReason { get; set; }
}
```

### 步驟 2：Service 層新增審核方法

在您的實體Service中新增審核相關方法：

```csharp
public interface IYourEntityService : IGenericService<YourEntity>
{
    Task<ServiceResult<bool>> ApproveAsync(int entityId, int approverId);
    Task<ServiceResult<bool>> RejectAsync(int entityId, int rejectedById, string? reason = null);
}

public class YourEntityService : IYourEntityService
{
    // 其他方法...
    
    public async Task<ServiceResult<bool>> ApproveAsync(int entityId, int approverId)
    {
        try
        {
            var entity = await GetByIdAsync(entityId);
            if (entity == null)
                return ServiceResult<bool>.Failure("找不到指定的記錄");
            
            entity.ApprovedAt = DateTime.Now;
            entity.ApprovedById = approverId;
            entity.UpdatedAt = DateTime.Now;
            
            await _context.SaveChangesAsync();
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Failure($"審核失敗：{ex.Message}");
        }
    }
    
    public async Task<ServiceResult<bool>> RejectAsync(int entityId, int rejectedById, string? reason = null)
    {
        try
        {
            var entity = await GetByIdAsync(entityId);
            if (entity == null)
                return ServiceResult<bool>.Failure("找不到指定的記錄");
            
            entity.RejectedAt = DateTime.Now;
            entity.RejectedById = rejectedById;
            entity.RejectionReason = reason;
            entity.UpdatedAt = DateTime.Now;
            
            await _context.SaveChangesAsync();
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Failure($"駁回失敗：{ex.Message}");
        }
    }
}
```

### 步驟 3：新增權限設定

在權限系統中新增審核權限：

```csharp
// 在 ModulePermissionMatrix.cs 或相關權限配置中
public static readonly Permission YourEntityApprove = new("YourEntity.Approve", "審核實體");
```

### 步驟 4：修改 Modal 組件

參考 `PurchaseOrderEditModalComponent.razor` 的實作方式：

#### 4.1 注入必要服務

```csharp
@inject IYourEntityService YourEntityService
@inject AuthenticationStateProvider AuthenticationStateProvider
@using Microsoft.AspNetCore.Components.Authorization
@using System.Security.Claims
```

#### 4.2 配置 GenericEditModalComponent

```csharp
<GenericEditModalComponent TEntity="YourEntity" 
                          TService="IYourEntityService"
                          @ref="editModalComponent"
                          IsVisible="@IsVisible"
                          IsVisibleChanged="@IsVisibleChanged"
                          Id="@YourEntityId"
                          Service="@YourEntityService"
                          EntityName="您的實體"
                          EntityNamePlural="您的實體"
                          ModalTitle="@(YourEntityId.HasValue ? "編輯實體" : "新增實體")"
                          ShowApprovalSection="@ShouldShowApprovalSection()"
                          ApprovalPermission="YourEntity.Approve"
                          OnApprove="@HandleEntityApprove"
                          OnReject="@HandleEntityReject"
                          GetApprovalStatus="@GetEntityApprovalStatus"
                          GetApprovalHistory="@GetEntityApprovalHistory"
                          >
</GenericEditModalComponent>
```

#### 4.3 實作審核相關方法

```csharp
@code {
    /// <summary>
    /// 判斷是否應該顯示審核區域
    /// </summary>
    private bool ShouldShowApprovalSection()
    {
        // 只有編輯現有實體時才顯示審核區域
        return YourEntityId.HasValue && YourEntityId.Value > 0;
    }
    
    /// <summary>
    /// 處理實體審核通過
    /// </summary>
    private async Task<bool> HandleEntityApprove()
    {
        try
        {
            if (!YourEntityId.HasValue)
            {
                await NotificationService.ShowErrorAsync("無法審核：實體ID不存在");
                return false;
            }
            
            // 確認對話框
            var confirmed = await NotificationService.ShowConfirmAsync(
                "確定要審核通過此記錄嗎？審核通過後將無法再修改。", 
                "審核確認");
            
            if (!confirmed)
                return false;
            
            // 呼叫服務進行審核
            var currentUserId = await GetCurrentUserIdAsync();
            if (!currentUserId.HasValue)
            {
                await NotificationService.ShowErrorAsync("無法取得當前使用者資訊");
                return false;
            }
            
            var result = await YourEntityService.ApproveAsync(YourEntityId.Value, currentUserId.Value);
            
            if (result.IsSuccess)
            {
                await NotificationService.ShowSuccessAsync("審核通過成功");
                return true;
            }
            else
            {
                await NotificationService.ShowErrorAsync(result.ErrorMessage ?? "審核通過失敗");
                return false;
            }
        }
        catch (Exception ex)
        {
            await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(HandleEntityApprove), GetType(), 
                additionalData: $"實體審核通過失敗 - EntityId: {YourEntityId}");
            await NotificationService.ShowErrorAsync($"審核通過時發生錯誤：{ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 處理實體審核駁回
    /// </summary>
    private async Task<bool> HandleEntityReject()
    {
        try
        {
            if (!YourEntityId.HasValue)
            {
                await NotificationService.ShowErrorAsync("無法審核：實體ID不存在");
                return false;
            }
            
            // 可以加入駁回原因輸入對話框
            var confirmed = await NotificationService.ShowConfirmAsync(
                "確定要駁回此記錄嗎？駁回後記錄將退回修改。", 
                "審核確認");
            
            if (!confirmed)
                return false;
            
            var currentUserId = await GetCurrentUserIdAsync();
            if (!currentUserId.HasValue)
            {
                await NotificationService.ShowErrorAsync("無法取得當前使用者資訊");
                return false;
            }
            
            var result = await YourEntityService.RejectAsync(YourEntityId.Value, currentUserId.Value);
            
            if (result.IsSuccess)
            {
                await NotificationService.ShowSuccessAsync("駁回成功");
                return true;
            }
            else
            {
                await NotificationService.ShowErrorAsync(result.ErrorMessage ?? "駁回失敗");
                return false;
            }
        }
        catch (Exception ex)
        {
            await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(HandleEntityReject), GetType(), 
                additionalData: $"實體審核駁回失敗 - EntityId: {YourEntityId}");
            await NotificationService.ShowErrorAsync($"審核駁回時發生錯誤：{ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 取得實體審核狀態
    /// </summary>
    private async Task<string?> GetEntityApprovalStatus()
    {
        try
        {
            if (editModalComponent?.Entity == null)
                return null;
            
            var entity = editModalComponent.Entity;
            
            if (entity.RejectedAt.HasValue)
            {
                return $"已於 {entity.RejectedAt.Value:yyyy/MM/dd HH:mm} 駁回";
            }
            else if (entity.ApprovedAt.HasValue)
            {
                return $"已於 {entity.ApprovedAt.Value:yyyy/MM/dd HH:mm} 審核通過";
            }
            else
            {
                return "待審核";
            }
        }
        catch (Exception ex)
        {
            await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetEntityApprovalStatus), GetType(), 
                additionalData: $"取得實體審核狀態失敗 - EntityId: {YourEntityId}");
            return "無法取得審核狀態";
        }
    }
    
    /// <summary>
    /// 取得實體審核歷史
    /// </summary>
    private async Task<List<string>> GetEntityApprovalHistory()
    {
        try
        {
            if (editModalComponent?.Entity == null)
                return new List<string>();
            
            var entity = editModalComponent.Entity;
            var history = new List<string>();
            
            // 建立記錄
            if (entity.CreatedAt != default)
            {
                history.Add($"{entity.CreatedAt:yyyy/MM/dd HH:mm} - {entity.CreatedBy ?? "系統"} 建立記錄");
            }
            
            // 最後修改記錄
            if (entity.UpdatedAt.HasValue)
            {
                history.Add($"{entity.UpdatedAt.Value:yyyy/MM/dd HH:mm} - {entity.UpdatedBy ?? "系統"} 修改記錄");
            }
            
            // 駁回記錄
            if (entity.RejectedAt.HasValue)
            {
                var rejecterName = entity.RejectedBy?.Name ?? "審核人員";
                var reason = !string.IsNullOrEmpty(entity.RejectionReason) ? $" (原因：{entity.RejectionReason})" : "";
                history.Add($"{entity.RejectedAt.Value:yyyy/MM/dd HH:mm} - {rejecterName} 駁回{reason}");
            }
            
            // 審核記錄
            if (entity.ApprovedAt.HasValue)
            {
                var approverName = entity.ApprovedBy?.Name ?? "審核人員";
                history.Add($"{entity.ApprovedAt.Value:yyyy/MM/dd HH:mm} - {approverName} 審核通過");
            }
            
            return history;
        }
        catch (Exception ex)
        {
            await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetEntityApprovalHistory), GetType(), 
                additionalData: $"取得實體審核歷史失敗 - EntityId: {YourEntityId}");
            return new List<string> { "無法取得審核歷史" };
        }
    }
    
    /// <summary>
    /// 取得當前使用者ID
    /// </summary>
    private async Task<int?> GetCurrentUserIdAsync()
    {
        try
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (!user.Identity?.IsAuthenticated ?? true)
                return null;

            // 從Claims中取得使用者ID (通常存在NameIdentifier中)
            var employeeIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(employeeIdClaim, out int employeeId))
                return employeeId;

            return null;
        }
        catch (Exception ex)
        {
            await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetCurrentUserIdAsync), GetType(), 
                additionalData: "取得當前使用者ID失敗");
            return null;
        }
    }
}
```

## 完整範例：採購單審核功能

以下是採購單審核功能的完整實作範例，可作為其他實體的參考：

### PurchaseOrderEditModalComponent.razor 關鍵配置

```csharp
// 在 GenericEditModalComponent 中的配置
ShowApprovalSection="@ShouldShowApprovalSection()"
ApprovalPermission="PurchaseOrder.Approve"
OnApprove="@HandlePurchaseOrderApprove"
OnReject="@HandlePurchaseOrderReject"
GetApprovalStatus="@GetPurchaseOrderApprovalStatus"
GetApprovalHistory="@GetPurchaseOrderApprovalHistory"
```

### 關鍵方法實作

```csharp
// 顯示條件
private bool ShouldShowApprovalSection()
{
    return PurchaseOrderId.HasValue && PurchaseOrderId.Value > 0;
}

// 審核通過處理
private async Task<bool> HandlePurchaseOrderApprove()
{
    // 確認對話框 -> 取得使用者ID -> 呼叫服務 -> 處理結果
    var result = await PurchaseOrderService.ApproveOrderAsync(PurchaseOrderId.Value, currentUserId.Value);
    return result.IsSuccess;
}
```

## 注意事項

### 1. 權限檢查
- 確保在權限系統中正確設定審核權限
- 使用 `PermissionCheck` 組件控制按鈕顯示

### 2. 使用者認證
- 使用 `AuthenticationStateProvider` 取得當前使用者
- 從 `ClaimTypes.NameIdentifier` 取得使用者ID

### 3. 錯誤處理
- 所有審核方法都應該包含完整的錯誤處理
- 使用 `ErrorHandlingHelper` 記錄詳細錯誤

### 4. UI 位置
- 審核區域位於元數據區段和 Modal Footer 之間
- 採用警告主題樣式，突出顯示重要性

### 5. 業務邏輯
- 審核通過後通常應限制再次編輯
- 考慮審核狀態對其他業務流程的影響

## 樣式配置

如需自訂審核區域樣式，可參考以下 CSS：

```css
.modal-approval-section {
    background-color: var(--bs-warning-bg-subtle);
    border: 1px solid var(--bs-warning-border-subtle);
    border-radius: 0.375rem;
    padding: 1rem;
    margin: 1rem 0;
}

.modal-approval-section h6 {
    color: var(--bs-warning-text-emphasis);
    margin-bottom: 0.75rem;
    font-weight: 600;
}

.approval-buttons .btn {
    margin-right: 0.5rem;
    margin-bottom: 0.5rem;
}

@media (max-width: 576px) {
    .approval-buttons .btn {
        width: 100%;
        margin-right: 0;
    }
}
```

## 測試建議

1. **權限測試** - 確認只有具備審核權限的使用者能看到審核按鈕
2. **狀態測試** - 驗證審核前後的狀態變化
3. **歷史記錄** - 確認審核歷史正確記錄
4. **錯誤處理** - 測試各種異常情況的處理

## 後續擴展

1. **批量審核** - 支援選擇多筆記錄進行批量審核
2. **審核流程** - 支援多級審核流程
3. **審核通知** - 整合通知系統，審核狀態變更時發送通知
4. **審核委派** - 支援審核權限委派功能

---

本指南提供了完整的審核功能實作框架，您可以根據具體業務需求進行調整和擴展。如有問題，請參考 `PurchaseOrderEditModalComponent.razor` 的完整實作。
