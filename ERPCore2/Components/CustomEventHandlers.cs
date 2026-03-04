using Microsoft.AspNetCore.Components;

namespace ERPCore2.Components;

/// <summary>
/// 註冊瀏覽器原生事件供 Blazor 使用
/// 主要用途：IME 輸入法組字事件（compositionstart / compositionend）
/// 解決中文/日文/韓文等 IME 輸入時文字重複的問題
/// </summary>
[EventHandler("oncompositionstart", typeof(EventArgs), enableStopPropagation: true, enablePreventDefault: true)]
[EventHandler("oncompositionend", typeof(EventArgs), enableStopPropagation: true, enablePreventDefault: true)]
public static class CustomEventHandlers
{
}
