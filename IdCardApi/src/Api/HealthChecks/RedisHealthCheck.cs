using System.Text;

using Microsoft.Extensions.Diagnostics.HealthChecks;

using StackExchange.Redis;

namespace IdCardApi.HealthChecks;

public class RedisHealthCheck(
    IConfiguration config,
    ILogger<BlobContainerHealthCheck> logger) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken)
    {
        const string key = "healthCheck-IdCard";
        string value = Guid.NewGuid().ToString();

        logger.LogInformation("Connecting to Redis server");
        ConnectionMultiplexer redis =
            await ConnectionMultiplexer.ConnectAsync(config.GetConnectionString("Redis")!);
        IDatabase db = redis.GetDatabase();

        logger.LogInformation("Setting key");
        using MemoryStream stream = new();
        stream.Write(Encoding.UTF8.GetBytes(value));
        stream.Position = 0;
        bool set = await db.StringSetAsync(new RedisKey(key), RedisValue.CreateFrom(stream));
        if (cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(
                "Cancellation was requested, could not complete health check. Returning degrated");
            return HealthCheckResult.Degraded();
        }

        if (!set)
        {
            logger.LogError("Failed to set the key");
            return HealthCheckResult.Unhealthy();
        }

        logger.LogInformation("Getting key");
        RedisValue redisValue = await db.StringGetAsync(new RedisKey(key));
        if (cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(
                "Cancellation was requested, could not complete health check. Returning degrated");
            return HealthCheckResult.Degraded();
        }

        if (!redisValue.HasValue)
        {
            logger.LogError("Failed to get the key's value");
            return HealthCheckResult.Unhealthy();
        }

        if (!redisValue.ToString().Equals(value, StringComparison.Ordinal))
        {
            logger.LogError("The key's actual value doesn't match they key's expected value");
            return HealthCheckResult.Unhealthy();
        }

        logger.LogInformation("Health check passed");
        return HealthCheckResult.Healthy();
    }
}
