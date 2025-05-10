using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace IdCardApi.HealthChecks;

public class SampleHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(HealthCheckResult.Healthy());
    }
}
