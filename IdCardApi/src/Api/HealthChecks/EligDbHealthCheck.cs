using Dapper;

using Microsoft.Extensions.Diagnostics.HealthChecks;

using Npgsql;

namespace IdCardApi.HealthChecks;

public class EligDbHealthCheck(IConfiguration config) : IHealthCheck
{
    private readonly string _eligDbConnexString = config.GetEligDbConnectionString();

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connex = new(_eligDbConnexString);
        _ = (await connex.QueryAsync<int>(new CommandDefinition(
                "select ck from subscriber limit 1", cancellationToken: cancellationToken)))
            .Single();
        return HealthCheckResult.Healthy();
    }
}
