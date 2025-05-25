using Dapper;

using Microsoft.Extensions.Diagnostics.HealthChecks;

using Npgsql;

namespace IdCardApi.HealthChecks;

public class PlanDbHealthCheck(IConfiguration config) : IHealthCheck
{
    private readonly string _planDbConnexString = config.GetPlanDbConnectionString();

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connex = new(_planDbConnexString);
        _ = (await connex.QueryAsync<int>(new CommandDefinition(
                "select top 1 ck from plan", cancellationToken: cancellationToken)))
            .Single();
        return HealthCheckResult.Healthy();
    }
}
