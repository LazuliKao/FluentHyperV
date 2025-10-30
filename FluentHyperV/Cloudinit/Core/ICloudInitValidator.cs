namespace FluentHyperV.Cloudinit.Core;

/// <summary>
/// Interface for cloud-init configuration validation
/// </summary>
public interface ICloudInitValidator
{
    /// <summary>
    /// Validates a cloud-init configuration against the official JSON schema
    /// </summary>
    /// <param name="configurationJson">The configuration in JSON format</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result with any errors</returns>
    Task<ValidationResult> ValidateAsync(string configurationJson, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads and caches the latest cloud-init schema
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    Task UpdateSchemaAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the result of cloud-init configuration validation
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Whether the configuration is valid
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Validation error messages
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Validation warning messages
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Creates a successful validation result
    /// </summary>
    public static ValidationResult Success() => new() { IsValid = true };

    /// <summary>
    /// Creates a failed validation result with errors
    /// </summary>
    /// <param name="errors">Validation errors</param>
    public static ValidationResult Failure(params string[] errors) => new()
    {
        IsValid = false,
        Errors = errors.ToList()
    };
}