using Microsoft.AspNetCore.Components;

namespace ERPCore2.Helpers.EditModal;

/// <summary>
/// 文件轉換助手 - 處理 A單轉B單 的通用邏輯
/// <para>用於統一處理轉單時的 Modal 開啟、參數設定、子組件等待、自動載入等流程</para>
/// </summary>
/// <remarks>
/// <b>主要功能：</b>
/// <list type="bullet">
///   <item>統一轉單流程（設定參數 → 顯示 Modal → 等待就緒 → 自動載入）</item>
///   <item>處理子組件渲染時序問題（確保子組件完全就緒後再觸發自動載入）</item>
///   <item>提供錯誤處理和通知機制</item>
/// </list>
/// 
/// <b>使用範例：</b>
/// <code>
/// // 範例 1: 採購單 → 進貨單
/// await DocumentConversionHelper.ShowConversionModalAsync(
///     setPrefilledValues: () => {
///         PrefilledSupplierId = supplierId;
///         PrefilledPurchaseOrderId = purchaseOrderId;
///         shouldAutoLoadUnreceived = true;
///     },
///     isVisibleChanged: IsVisibleChanged,
///     autoLoadAction: async () => {
///         shouldAutoLoadUnreceived = false;
///         await purchaseReceivingDetailManager.LoadAllUnreceivedItems();
///     },
///     detailManagerReady: () => purchaseReceivingDetailManager != null,
///     shouldAutoLoad: () => shouldAutoLoadUnreceived,
///     stateHasChangedAction: StateHasChanged
/// );
/// 
/// // 範例 2: 銷貨訂單 → 銷貨退回
/// await DocumentConversionHelper.ShowConversionModalAsync(
///     setPrefilledValues: () => {
///         PrefilledCustomerId = customerId;
///         PrefilledSalesOrderId = salesOrderId;
///         shouldAutoLoadReturnable = true;
///     },
///     isVisibleChanged: IsVisibleChanged,
///     autoLoadAction: async () => {
///         shouldAutoLoadReturnable = false;
///         await salesReturnDetailManager.LoadAllReturnableItems();
///     },
///     detailManagerReady: () => salesReturnDetailManager != null,
///     shouldAutoLoad: () => shouldAutoLoadReturnable,
///     stateHasChangedAction: StateHasChanged
/// );
/// </code>
/// 
/// <b>設計理念：</b>
/// <para>
/// 原本每個轉單邏輯都需要重複實作：設定參數、顯示 Modal、等待渲染、自動載入等步驟。
/// 透過此 Helper 統一封裝這些重複邏輯，減少程式碼重複並確保轉單流程的一致性。
/// </para>
/// 
/// <b>注意事項：</b>
/// <list type="bullet">
///   <item>預設延遲時間為 500ms，可根據實際情況調整（適用於大多數場景）</item>
///   <item>自動載入需要 detailManager 已就緒且 shouldAutoLoad 為 true</item>
///   <item>建議在調用方保留錯誤處理邏輯以提供更精確的錯誤訊息</item>
/// </list>
/// </remarks>
public static class DocumentConversionHelper
{
    /// <summary>
    /// 顯示轉單 Modal 並執行自動載入
    /// </summary>
    /// <param name="setPrefilledValues">設定預填值的動作（如 PrefilledSupplierId、PrefilledOrderId、shouldAutoLoad 等）</param>
    /// <param name="isVisibleChanged">Modal 的 IsVisibleChanged EventCallback</param>
    /// <param name="autoLoadAction">自動載入明細的非同步動作（如 LoadAllUnreceivedItems、LoadAllReturnableItems）</param>
    /// <param name="detailManagerReady">檢查 DetailManager 是否已就緒的函數（如 () => detailManager != null）</param>
    /// <param name="shouldAutoLoad">檢查是否應該執行自動載入的函數（如 () => shouldAutoLoadFlag）</param>
    /// <param name="stateHasChangedAction">StateHasChanged 動作（用於通知 UI 更新）</param>
    /// <param name="delayMs">等待子組件就緒的延遲時間（毫秒），預設 500ms</param>
    /// <param name="invokeAsync">InvokeAsync 包裝函數（用於在正確的同步上下文中執行）</param>
    /// <returns>轉單操作是否成功</returns>
    /// <remarks>
    /// <b>執行流程：</b>
    /// <list type="number">
    ///   <item>執行 setPrefilledValues 設定預填參數和自動載入標記</item>
    ///   <item>觸發 IsVisibleChanged 顯示 Modal</item>
    ///   <item>等待指定的延遲時間（確保子組件完全渲染）</item>
    ///   <item>檢查 detailManager 是否就緒且 shouldAutoLoad 為 true</item>
    ///   <item>執行 autoLoadAction 載入明細資料</item>
    ///   <item>呼叫 StateHasChanged 更新 UI</item>
    /// </list>
    /// 
    /// <b>為什麼需要延遲？</b>
    /// <para>
    /// Modal 顯示後，子組件（如 DetailManager）需要時間渲染和初始化。
    /// 如果立即調用自動載入，可能因為子組件尚未準備好而失敗。
    /// 500ms 是經驗值，可應對大多數情況。
    /// </para>
    /// </remarks>
    public static async Task<bool> ShowConversionModalAsync(
        Action setPrefilledValues,
        EventCallback<bool> isVisibleChanged,
        Func<Task> autoLoadAction,
        Func<bool> detailManagerReady,
        Func<bool> shouldAutoLoad,
        Action stateHasChangedAction,
        int delayMs = 500,
        Func<Func<Task>, Task>? invokeAsync = null)
    {
        try
        {
            // 步驟 1: 設定預填值和自動載入標記
            setPrefilledValues();

            // 步驟 2: 觸發 Modal 顯示
            if (isVisibleChanged.HasDelegate)
            {
                await isVisibleChanged.InvokeAsync(true);
            }

            // 步驟 3: 等待子組件完全就緒
            // 增加延遲時間以確保子組件（如 DetailManager）完全渲染
            await Task.Delay(delayMs);

            // 步驟 4: 檢查子組件是否已就緒並執行自動載入
            if (detailManagerReady() && shouldAutoLoad())
            {
                if (invokeAsync != null)
                {
                    // 在正確的同步上下文中執行（Blazor 組件需要）
                    await invokeAsync(async () =>
                    {
                        await autoLoadAction();
                        stateHasChangedAction();
                    });
                }
                else
                {
                    // 直接執行（如果不需要 InvokeAsync）
                    await autoLoadAction();
                    stateHasChangedAction();
                }
            }

            return true;
        }
        catch
        {
            // 讓調用方處理具體的錯誤訊息
            return false;
        }
    }

    /// <summary>
    /// 顯示轉單 Modal 並執行自動載入（簡化版 - 自動處理常見場景）
    /// </summary>
    /// <typeparam name="TDetailManager">明細管理器類型（需有公開的載入方法）</typeparam>
    /// <param name="resetEntityId">重置主實體 ID 的動作（設為 null 表示新增模式）</param>
    /// <param name="setPrefilledValues">設定預填值的動作</param>
    /// <param name="isVisibleChanged">Modal 的 IsVisibleChanged EventCallback</param>
    /// <param name="detailManager">明細管理器實例</param>
    /// <param name="autoLoadMethodName">自動載入方法名稱（如 "LoadAllUnreceivedItems"）</param>
    /// <param name="resetShouldAutoLoad">重置 shouldAutoLoad 標記的動作（如 () => shouldAutoLoadUnreceived = false）</param>
    /// <param name="shouldAutoLoad">檢查是否應該執行自動載入的函數（如 () => shouldAutoLoadUnreceived）</param>
    /// <param name="stateHasChangedAction">StateHasChanged 動作</param>
    /// <param name="invokeAsync">InvokeAsync 包裝函數</param>
    /// <param name="delayMs">延遲時間（毫秒），預設 500ms</param>
    /// <returns>轉單操作是否成功</returns>
    /// <remarks>
    /// <b>此方法使用反射調用載入方法，適合標準化的轉單場景</b>
    /// <para>
    /// 如果您的載入方法名稱固定（如 LoadAllUnreceivedItems、LoadAllReturnableItems），
    /// 可以使用此簡化版本，無需手動撰寫 autoLoadAction。
    /// </para>
    /// 
    /// <b>使用範例：</b>
    /// <code>
    /// await DocumentConversionHelper.ShowConversionModalSimpleAsync(
    ///     resetEntityId: () => PurchaseReceivingId = null,
    ///     setPrefilledValues: () => {
    ///         PrefilledSupplierId = supplierId;
    ///         PrefilledPurchaseOrderId = purchaseOrderId;
    ///         shouldAutoLoadUnreceived = true;
    ///     },
    ///     isVisibleChanged: IsVisibleChanged,
    ///     detailManager: purchaseReceivingDetailManager,
    ///     autoLoadMethodName: "LoadAllUnreceivedItems",
    ///     resetShouldAutoLoad: () => shouldAutoLoadUnreceived = false,
    ///     shouldAutoLoad: () => shouldAutoLoadUnreceived,
    ///     stateHasChangedAction: StateHasChanged,
    ///     invokeAsync: InvokeAsync
    /// );
    /// </code>
    /// </remarks>
    public static async Task<bool> ShowConversionModalSimpleAsync<TDetailManager>(
        Action resetEntityId,
        Action setPrefilledValues,
        EventCallback<bool> isVisibleChanged,
        TDetailManager? detailManager,
        string autoLoadMethodName,
        Action resetShouldAutoLoad,
        Func<bool> shouldAutoLoad,
        Action stateHasChangedAction,
        Func<Func<Task>, Task> invokeAsync,
        int delayMs = 500)
        where TDetailManager : class
    {
        try
        {
            // 重置實體 ID（設為新增模式）
            resetEntityId();

            // 設定預填值
            setPrefilledValues();

            // 觸發 Modal 顯示
            if (isVisibleChanged.HasDelegate)
            {
                await isVisibleChanged.InvokeAsync(true);
            }

            // 等待子組件完全就緒
            await Task.Delay(delayMs);

            // 檢查子組件是否已就緒並執行自動載入
            if (detailManager != null && shouldAutoLoad())
            {
                await invokeAsync(async () =>
                {
                    // 重置 shouldAutoLoad 標記
                    resetShouldAutoLoad();

                    // 使用反射調用載入方法
                    var method = typeof(TDetailManager).GetMethod(autoLoadMethodName);
                    if (method != null)
                    {
                        var task = method.Invoke(detailManager, null) as Task;
                        if (task != null)
                        {
                            await task;
                        }
                    }

                    stateHasChangedAction();
                });
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 顯示轉單 Modal 並執行自訂載入邏輯（適用於複雜場景）
    /// </summary>
    /// <param name="resetEntityId">重置主實體 ID 的動作</param>
    /// <param name="setPrefilledValues">設定預填值的動作</param>
    /// <param name="isVisibleChanged">Modal 的 IsVisibleChanged EventCallback</param>
    /// <param name="customLoadAction">自訂載入邏輯（如從報價單載入明細到銷貨訂單）</param>
    /// <param name="detailManagerReady">檢查明細管理器是否就緒的函數</param>
    /// <param name="stateHasChangedAction">StateHasChanged 動作</param>
    /// <param name="invokeAsync">InvokeAsync 包裝函數</param>
    /// <param name="delayMs">延遲時間（毫秒），預設 500ms</param>
    /// <returns>轉單操作是否成功</returns>
    /// <remarks>
    /// <b>適用於需要自訂載入邏輯的場景</b>
    /// <para>
    /// 例如：報價單轉銷貨訂單時，需要執行 LoadQuotationDetailsToSalesOrder，
    /// 而不是簡單的 LoadAllXxxItems。
    /// </para>
    /// 
    /// <b>使用範例：</b>
    /// <code>
    /// await DocumentConversionHelper.ShowConversionModalWithCustomLoadAsync(
    ///     resetEntityId: () => SalesOrderId = null,
    ///     setPrefilledValues: () => {
    ///         PrefilledCustomerId = customerId;
    ///         PrefilledQuotationId = quotationId;
    ///     },
    ///     isVisibleChanged: IsVisibleChanged,
    ///     customLoadAction: async () => {
    ///         await LoadQuotationDetailsToSalesOrder(quotationId);
    ///     },
    ///     detailManagerReady: () => salesOrderDetailManager != null,
    ///     stateHasChangedAction: StateHasChanged,
    ///     invokeAsync: InvokeAsync
    /// );
    /// </code>
    /// </remarks>
    public static async Task<bool> ShowConversionModalWithCustomLoadAsync(
        Action resetEntityId,
        Action setPrefilledValues,
        EventCallback<bool> isVisibleChanged,
        Func<Task> customLoadAction,
        Func<bool> detailManagerReady,
        Action stateHasChangedAction,
        Func<Func<Task>, Task> invokeAsync,
        int delayMs = 500)
    {
        try
        {
            // 重置實體 ID（設為新增模式）
            resetEntityId();

            // 設定預填值
            setPrefilledValues();

            // 觸發 Modal 顯示
            if (isVisibleChanged.HasDelegate)
            {
                await isVisibleChanged.InvokeAsync(true);
            }

            // 等待子組件完全就緒
            await Task.Delay(delayMs);

            // 檢查子組件是否已就緒並執行自訂載入邏輯
            if (detailManagerReady())
            {
                await invokeAsync(async () =>
                {
                    await customLoadAction();
                    stateHasChangedAction();
                });
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}
