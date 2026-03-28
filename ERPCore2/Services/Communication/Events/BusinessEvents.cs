namespace ERPCore2.Services.Communication.Events
{
    /// <summary>
    /// 業務事件基底類別
    /// </summary>
    public abstract record BusinessEventBase(
        string SourceModule,
        int SourceId,
        int? TriggeredBy
    ) : IBusinessEvent
    {
        public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    }

    /// <summary>
    /// 單據建立事件
    /// </summary>
    public record DocumentCreatedEvent(
        string SourceModule,
        int SourceId,
        int? TriggeredBy,
        string DocumentCode
    ) : BusinessEventBase(SourceModule, SourceId, TriggeredBy);

    /// <summary>
    /// 單據提交審核事件（人工審核模式下儲存正式單據時觸發）
    /// </summary>
    public record DocumentSubmittedForApprovalEvent(
        string SourceModule,
        int SourceId,
        int? TriggeredBy,
        string DocumentCode,
        decimal? Amount
    ) : BusinessEventBase(SourceModule, SourceId, TriggeredBy);

    /// <summary>
    /// 單據核准事件
    /// </summary>
    public record DocumentApprovedEvent(
        string SourceModule,
        int SourceId,
        int? TriggeredBy,
        string DocumentCode,
        int ApprovedBy
    ) : BusinessEventBase(SourceModule, SourceId, TriggeredBy);

    /// <summary>
    /// 單據駁回事件
    /// </summary>
    public record DocumentRejectedEvent(
        string SourceModule,
        int SourceId,
        int? TriggeredBy,
        string DocumentCode,
        int RejectedBy,
        string Reason
    ) : BusinessEventBase(SourceModule, SourceId, TriggeredBy);

    /// <summary>
    /// 實體狀態變更事件
    /// </summary>
    public record EntityStatusChangedEvent(
        string SourceModule,
        int SourceId,
        int? TriggeredBy,
        string OldStatus,
        string NewStatus
    ) : BusinessEventBase(SourceModule, SourceId, TriggeredBy);
}
