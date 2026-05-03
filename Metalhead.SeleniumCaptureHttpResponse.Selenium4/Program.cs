using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Metalhead.SeleniumCaptureHttpResponse.Selenium4;

var builder = Host.CreateApplicationBuilder();

// Use the Options pattern to bind app settings.  Validation rules are defined in DriverOptions.
builder.Services.AddOptions<DriverOptions>()
    .Bind(builder.Configuration.GetSection(DriverOptions.DriverSettings))
    .ValidateOnStart();

builder.Services.AddSingleton<IValidateOptions<DriverOptions>, DriverOptionsValidation>();
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<DriverOptions>>().Value);
builder.Services.AddSingleton<CaptureResponse>();

using var host = builder.Build();

using var serviceScope = host.Services.CreateScope();
var serviceProvider = serviceScope.ServiceProvider;

host.Start(); // Trigger validation of app settings.
var captureResponse = serviceProvider.GetRequiredService<CaptureResponse>();

// Define the URL to fetch, and the URL(s) to capture HTTP responses from.
var responseData = await captureResponse.GetResponseData(
    "https://metaljase.github.io/SeleniumCaptureHttpResponse.html",
    [
        "https://jsonplaceholder.typicode.com/users/",
        "https://jsonplaceholder.typicode.com/albums/"
    ],
    TimeSpan.FromSeconds(20));

if (responseData.Any(r => r.CaptureSuccess == false))
{
    Console.WriteLine("Failed to capture HTTP response(s):");
    foreach (var data in responseData.Where(r => r.CaptureSuccess == false))
    {
        Console.WriteLine(data.Url);
    }
}

if (responseData.Any(r => r.CaptureSuccess))
{
    Console.WriteLine("Successfully captured HTTP response(s):");
    foreach (var data in responseData.Where(r => r.CaptureSuccess))
    {
        Console.WriteLine(data.Url);
        Console.WriteLine(data.Body);
    }
}