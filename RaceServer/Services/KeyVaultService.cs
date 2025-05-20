using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace RaceServer.Services;

public interface IKeyVaultService
{
    Task<string> GetSecretAsync(string secretName);
    Task SetSecretAsync(string secretName, string value);
    Task DeleteSecretAsync(string secretName);
}

public class KeyVaultService : IKeyVaultService
{
    private readonly SecretClient _secretClient;
    private readonly ILogger<KeyVaultService> _logger;
    private readonly string _keyVaultUrl;

    public KeyVaultService(ILogger<KeyVaultService> logger)
    {
        _logger = logger;
        _keyVaultUrl = Environment.GetEnvironmentVariable("AZURE_KEY_VAULT_URL")
            ?? throw new InvalidOperationException("AZURE_KEY_VAULT_URL environment variable is not set");

        var credential = new DefaultAzureCredential();
        _secretClient = new SecretClient(new Uri(_keyVaultUrl), credential);
    }

    public async Task<string> GetSecretAsync(string secretName)
    {
        try
        {
            var secret = await _secretClient.GetSecretAsync(secretName);
            return secret.Value.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving secret '{secretName}': {ex.Message}");
            throw;
        }
    }

    public async Task SetSecretAsync(string secretName, string value)
    {
        try
        {
            await _secretClient.SetSecretAsync(secretName, value);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error setting secret '{secretName}': {ex.Message}");
            throw;
        }
    }

    public async Task DeleteSecretAsync(string secretName)
    {
        try
        {
            await _secretClient.StartDeleteSecretAsync(secretName);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting secret '{secretName}': {ex.Message}");
            throw;
        }
    }
}
