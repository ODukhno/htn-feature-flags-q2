using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace QResourceServer
{
    public class HealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
            => Task.FromResult(HealthCheckResult.Healthy());
    }
}
