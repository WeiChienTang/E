using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services.Communication.Events
{
    /// <summary>
    /// 輕量級進程內事件匯流排（Singleton）
    /// 使用 IServiceScopeFactory 為每次 PublishAsync 建立新的 Scope，
    /// 確保 EF Core DbContext 等 Scoped 服務正確運作。
    /// </summary>
    public class InProcessEventBus : IEventBus
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<InProcessEventBus> _logger;

        public InProcessEventBus(
            IServiceScopeFactory scopeFactory,
            ILogger<InProcessEventBus> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task PublishAsync<TEvent>(TEvent businessEvent, CancellationToken ct = default)
            where TEvent : IBusinessEvent
        {
            using var scope = _scopeFactory.CreateScope();
            var handlers = scope.ServiceProvider.GetServices<IBusinessEventHandler<TEvent>>();

            foreach (var handler in handlers)
            {
                try
                {
                    await handler.HandleAsync(businessEvent, ct);
                }
                catch (Exception ex)
                {
                    // 單一 handler 失敗不應阻斷其他 handler
                    _logger.LogError(ex,
                        "事件處理器 {HandlerType} 處理 {EventType} 時發生錯誤。SourceModule={SourceModule}, SourceId={SourceId}",
                        handler.GetType().Name,
                        typeof(TEvent).Name,
                        businessEvent.SourceModule,
                        businessEvent.SourceId);
                }
            }
        }
    }
}
