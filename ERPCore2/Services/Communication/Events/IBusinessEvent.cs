namespace ERPCore2.Services.Communication.Events
{
    /// <summary>
    /// 業務事件標記介面 - 所有業務事件的基礎
    /// </summary>
    public interface IBusinessEvent
    {
        /// <summary>事件發生時間 (UTC)</summary>
        DateTime OccurredAt { get; }

        /// <summary>觸發事件的使用者 (EmployeeId)</summary>
        int? TriggeredBy { get; }

        /// <summary>來源模組名稱（如 "SalesOrder"）</summary>
        string SourceModule { get; }

        /// <summary>來源記錄 ID</summary>
        int SourceId { get; }
    }

    /// <summary>
    /// 業務事件處理器介面
    /// </summary>
    public interface IBusinessEventHandler<in TEvent> where TEvent : IBusinessEvent
    {
        Task HandleAsync(TEvent businessEvent, CancellationToken ct = default);
    }

    /// <summary>
    /// 輕量級進程內事件匯流排
    /// </summary>
    public interface IEventBus
    {
        /// <summary>發布事件（非同步，所有已註冊的 handler 依序執行）</summary>
        Task PublishAsync<TEvent>(TEvent businessEvent, CancellationToken ct = default)
            where TEvent : IBusinessEvent;
    }
}
