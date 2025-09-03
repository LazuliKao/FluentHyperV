using FluentHyperV.Cloudinit.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace FluentHyperV.Cloudinit.Services;

/// <summary>
/// Service for validating cloud-init configurations against the official schema
/// </summary>
public class CloudInitValidator : ICloudInitValidator, IDisposable
{
    private const string SchemaUrl = "https://raw.githubusercontent.com/canonical/cloud-init/main/cloudinit/config/schemas/schema-cloud-config-v1.json";
    
    private readonly ILogger<CloudInitValidator> _logger;
    private readonly HttpClient _httpClient;
    private JSchema? _cachedSchema;
    private readonly SemaphoreSlim _schemaSemaphore = new(1, 1);

    public CloudInitValidator(ILogger<CloudInitValidator> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    /// <summary>
    /// Validates a cloud-init configuration against the official JSON schema
    /// </summary>
    public async Task<ValidationResult> ValidateAsync(string configurationJson, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Validating cloud-init configuration");

            // Ensure we have the schema
            var schema = await GetSchemaAsync(cancellationToken);
            if (schema == null)
            {
                _logger.LogWarning("Could not load cloud-init schema, skipping validation");
                return ValidationResult.Success(); // Skip validation if schema unavailable
            }

            // Parse the JSON configuration
            JObject config;
            try
            {
                config = JObject.Parse(configurationJson);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Invalid JSON in cloud-init configuration");
                return ValidationResult.Failure($"Invalid JSON: {ex.Message}");
            }

            // Validate against schema
            var errors = new List<string>();
            var isValid = config.IsValid(schema, out IList<string> schemaErrors);

            if (!isValid && schemaErrors != null)
            {
                errors.AddRange(schemaErrors);
            }

            if (errors.Count > 0)
            {
                _logger.LogWarning("Cloud-init configuration validation failed with {ErrorCount} errors", errors.Count);
                foreach (var error in errors)
                {
                    _logger.LogWarning("Validation error: {Error}", error);
                }
                return ValidationResult.Failure(errors.ToArray());
            }

            _logger.LogInformation("Cloud-init configuration validation successful");
            return ValidationResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cloud-init configuration validation");
            return ValidationResult.Failure($"Validation error: {ex.Message}");
        }
    }

    /// <summary>
    /// Downloads and caches the latest cloud-init schema
    /// </summary>
    public async Task UpdateSchemaAsync(CancellationToken cancellationToken = default)
    {
        await _schemaSemaphore.WaitAsync(cancellationToken);
        try
        {
            _logger.LogInformation("Downloading cloud-init schema from {Url}", SchemaUrl);

            var response = await _httpClient.GetAsync(SchemaUrl, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to download schema: {StatusCode}", response.StatusCode);
                return;
            }

            var schemaJson = await response.Content.ReadAsStringAsync(cancellationToken);
            _cachedSchema = JSchema.Parse(schemaJson);
            
            _logger.LogInformation("Successfully downloaded and cached cloud-init schema");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update cloud-init schema");
        }
        finally
        {
            _schemaSemaphore.Release();
        }
    }

    private async Task<JSchema?> GetSchemaAsync(CancellationToken cancellationToken)
    {
        if (_cachedSchema != null)
        {
            return _cachedSchema;
        }

        await UpdateSchemaAsync(cancellationToken);
        return _cachedSchema;
    }

    public void Dispose()
    {
        _schemaSemaphore?.Dispose();
    }
}