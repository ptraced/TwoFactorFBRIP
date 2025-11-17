# TwoFactorFBRIP

A .NET 10 client library for interacting with the 2FA.fb.rip API to generate time-based one-time passwords (TOTP).

## Installation

Install the package via NuGet Package Manager:

```
Install-Package TwoFactorFBRIP
```

Or via .NET CLI:

```
dotnet add package TwoFactorFBRIP
```

## Usage

### Basic Usage

```csharp
using TwoFactorFBRIP;

// Create a client instance
using var client = new TwoFactorClient();

// Get OTP for a secret (async)
var response = await client.GetOtpAsync("TWOFACTORSECRET");

if (response.Ok && response.Data != null)
{
    Console.WriteLine($"OTP: {response.Data.Otp}");
    Console.WriteLine($"Time Remaining: {response.Data.TimeRemaining} seconds");
}
```

### Synchronous Usage

```csharp
using TwoFactorFBRIP;

// Create a client instance
using var client = new TwoFactorClient();

// Get OTP for a secret (sync)
var response = client.GetOtp("TWOFACTORSECRET");

if (response.Ok && response.Data != null)
{
    Console.WriteLine($"OTP: {response.Data.Otp}");
    Console.WriteLine($"Time Remaining: {response.Data.TimeRemaining} seconds");
}
```

### Using with HttpClient Factory

```csharp
using TwoFactorFBRIP;
using Microsoft.Extensions.DependencyInjection;

// Using dependency injection
var services = new ServiceCollection();
services.AddHttpClient();
var serviceProvider = services.BuildServiceProvider();

var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
var httpClient = httpClientFactory.CreateClient();

// Create client with custom HttpClient
using var client = new TwoFactorClient(httpClient);

var response = await client.GetOtpAsync("TWOFACTORSECRET");
```

## API Response

The API returns a response in the following format:

```json
{
  "ok": true,
  "data": {
    "otp": "384866",
    "timeRemaining": 10
  }
}
```

This is mapped to the `OtpResponse` class:

- `Ok` (bool): Indicates if the request was successful
- `Data` (OtpData): Contains the OTP information
  - `Otp` (string): The generated 6-digit OTP code
  - `TimeRemaining` (int): Time in seconds until the OTP expires

## Error Handling

The client throws the following exceptions:

- `ArgumentNullException`: When the secret is null or empty
- `HttpRequestException`: When the API request fails
- `JsonException`: When the response cannot be parsed

```csharp
try
{
    var response = await client.GetOtpAsync("INVALID_SECRET");
}
catch (ArgumentNullException ex)
{
    Console.WriteLine($"Invalid secret: {ex.Message}");
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"API request failed: {ex.Message}");
}
catch (JsonException ex)
{
    Console.WriteLine($"Failed to parse response: {ex.Message}");
}
```

## License

MIT License