using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace IdCardApi.HealthChecks;

public class BlobContainerHealthCheck(
    IConfiguration config,
    ILogger<BlobContainerHealthCheck> logger) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken)
    {
        const string healthCheckBlob = "healthCheck/idCardApi.txt";

        // The correct way to do this is to use the DI library, register a named blob client, etc,
        // but that's not the focus of this experiment, so whatev.
        logger.LogInformation("Creating client");
        BlobContainerClient container = new(config.GetConnectionString("BlobContainer")!, "id-card");

        logger.LogInformation("Writing blob to container");
        using MemoryStream stream = new();
        stream.Write("hello, blob!"u8);
        stream.Position = 0;
        _ = await container.UploadBlobAsync(healthCheckBlob, stream, cancellationToken);
        if (cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(
                "Cancellation was requested, could not complete health check. Returning degrated");
            return HealthCheckResult.Degraded();
        }

        logger.LogInformation("Deleting blob from container");
        Response resp = await container.DeleteBlobAsync(healthCheckBlob, DeleteSnapshotsOption.IncludeSnapshots,
            cancellationToken: cancellationToken);
        if (resp.IsError)
        {
            logger.LogError("Could not delete the blob: {Reason}", resp.ReasonPhrase);
            return HealthCheckResult.Unhealthy();
        }

        logger.LogInformation("Health check passed");
        return HealthCheckResult.Healthy();
    }
}
