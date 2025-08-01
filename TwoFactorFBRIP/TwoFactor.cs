using System.Text.Json;
using System.Text.Json.Serialization;

namespace TwoFactorFBRIP;

/// <summary>
/// Response model for the 2FA API
/// </summary>
public class OtpResponse
{
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    [JsonPropertyName("data")]
    public OtpData? Data { get; set; }
}

/// <summary>
/// Data model containing the OTP information
/// </summary>
public class OtpData
{
    [JsonPropertyName("otp")]
    public string Otp { get; set; } = string.Empty;

    [JsonPropertyName("timeRemaining")]
    public int TimeRemaining { get; set; }
}

/// <summary>
/// Client for interacting with the 2FA.fb.rip API
/// </summary>
public class TwoFactorClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly bool _disposeHttpClient;
    private const string BaseUrl = "https://2fa.fb.rip/api/otp/";

    /// <summary>
    /// Initializes a new instance of the TwoFactorClient class with a default HttpClient
    /// </summary>
    public TwoFactorClient() : this(new HttpClient(), true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the TwoFactorClient class with a provided HttpClient
    /// </summary>
    /// <param name="httpClient">The HttpClient to use for API requests</param>
    /// <param name="disposeHttpClient">Whether to dispose the HttpClient when this instance is disposed</param>
    public TwoFactorClient(HttpClient httpClient, bool disposeHttpClient = false)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _disposeHttpClient = disposeHttpClient;
    }

    /// <summary>
    /// Gets a time-based one-time password (TOTP) for the specified secret
    /// </summary>
    /// <param name="secret">The two-factor authentication secret</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The OTP response containing the generated code and time remaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when secret is null or empty</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails</exception>
    /// <exception cref="JsonException">Thrown when the response cannot be parsed</exception>
    public async Task<OtpResponse> GetOtpAsync(string secret, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(secret))
            throw new ArgumentNullException(nameof(secret), "Secret cannot be null or empty");

        var url = $"{BaseUrl}{Uri.EscapeDataString(secret)}";
        
        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var otpResponse = JsonSerializer.Deserialize<OtpResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return otpResponse ?? throw new JsonException("Failed to deserialize response");
        }
        catch (HttpRequestException)
        {
            throw;
        }
        catch (JsonException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to get OTP for secret: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets a time-based one-time password (TOTP) for the specified secret (synchronous version)
    /// </summary>
    /// <param name="secret">The two-factor authentication secret</param>
    /// <returns>The OTP response containing the generated code and time remaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when secret is null or empty</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails</exception>
    /// <exception cref="JsonException">Thrown when the response cannot be parsed</exception>
    public OtpResponse GetOtp(string secret)
    {
        return GetOtpAsync(secret).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Disposes the HttpClient if it was created by this instance
    /// </summary>
    public void Dispose()
    {
        if (_disposeHttpClient)
        {
            _httpClient?.Dispose();
        }
    }
}
