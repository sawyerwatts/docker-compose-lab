using System.Globalization;

using Microsoft.Data.SqlClient;

using Npgsql;

namespace IdCardApi;

public static class ConfigExtensions
{
    public static string GetEligDbConnectionString(this IConfiguration config)
    {
        const string prefix = "EligDb";

        string host = config.GetConnectionString($"{prefix}Host")!;
        if (string.IsNullOrWhiteSpace(host))
            throw new InvalidOperationException("The elig DB's host was not configured");

        string port = config.GetConnectionString($"{prefix}Port")!;
        if (string.IsNullOrWhiteSpace(port))
            throw new InvalidOperationException("The elig DB's port was not configured");

        string username = config.GetConnectionString($"{prefix}Username")!;
        if (string.IsNullOrWhiteSpace(username))
            throw new InvalidOperationException("The elig DB's username was not configured");

        string password = config.GetConnectionString($"{prefix}Password")!;
        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("The elig DB's password was not configured");

        NpgsqlConnectionStringBuilder builder = new()
        {
            Host = host,
            Port = int.Parse(port, provider: CultureInfo.InvariantCulture),
            Database = "eligdb",
            Username = username,
            Password = password,
        };
        return builder.ConnectionString;
    }

    public static string GetPlanDbConnectionString(this IConfiguration config)
    {
        const string prefix = "PlanDb";

        string host = config.GetConnectionString($"{prefix}Host")!;
        if (string.IsNullOrWhiteSpace(host))
            throw new InvalidOperationException("The plan DB's host was not configured");

        string port = config.GetConnectionString($"{prefix}Port")!;
        if (string.IsNullOrWhiteSpace(port))
            throw new InvalidOperationException("The plan DB's port was not configured");

        string username = config.GetConnectionString($"{prefix}Username")!;
        if (string.IsNullOrWhiteSpace(username))
            throw new InvalidOperationException("The plan DB's username was not configured");

        string password = config.GetConnectionString($"{prefix}Password")!;
        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("The plan DB's password was not configured");

        string encrypt = config.GetConnectionString($"{prefix}Encrypt")!;
        if (string.IsNullOrWhiteSpace(encrypt))
            encrypt = true.ToString();

        SqlConnectionStringBuilder builder = new()
        {
            DataSource = $"{host},{int.Parse(port, provider: CultureInfo.InvariantCulture)}",
            InitialCatalog = "plandb",
            UserID = username,
            Password = password,
            Encrypt = bool.Parse(encrypt),
        };
        return builder.ConnectionString;
    }
}
