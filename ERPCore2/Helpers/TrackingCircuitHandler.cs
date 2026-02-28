using Microsoft.AspNetCore.Components.Server.Circuits;

namespace ERPCore2.Helpers
{
    /// <summary>
    /// 追蹤 Blazor Circuit 生命週期事件的診斷用 Handler
    /// 用於偵測 Circuit 是否正常建立與連線
    /// </summary>
    public class TrackingCircuitHandler : CircuitHandler
    {
        public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            return base.OnCircuitOpenedAsync(circuit, cancellationToken);
        }

        public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            return base.OnConnectionUpAsync(circuit, cancellationToken);
        }

        public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            return base.OnConnectionDownAsync(circuit, cancellationToken);
        }

        public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            return base.OnCircuitClosedAsync(circuit, cancellationToken);
        }
    }
}
