using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Metalhead.SeleniumCaptureHttpResponse.CDP;
using Metalhead.SeleniumCaptureHttpResponse.Core;

var builder = Host.CreateApplicationBuilder();

try
{
    builder.Services.AddOptions<DriverOptions>().Bind(builder.Configuration.GetSection(DriverOptions.SectionName));
    builder.Services.AddSingleton<IValidateOptions<DriverOptions>, DriverOptionsValidation>();
    builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<DriverOptions>>().Value);

    builder.Services.AddSingleton<CaptureResponse>();

    using var host = builder.Build();

    using var serviceScope = host.Services.CreateScope();
    var serviceProvider = serviceScope.ServiceProvider;

    var captureResponse = serviceProvider.GetRequiredService<CaptureResponse>();

    // Define the URL to fetch, and the URL(s) to capture HTTP responses from.
    var responseData = captureResponse.GetResponseData(
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
            Console.WriteLine(data.Url);
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
}
catch (OptionsValidationException ex)
{
    var message = ex.Message.Replace("; ", Environment.NewLine);
    Console.WriteLine($"""
        Application exited due to invalid app settings:
        {message}
        """);
}
catch (Exception ex)
{
    Console.WriteLine($"""
        Application exited unexpectedly: {ex.Message}
        {ex.StackTrace}
        """);
}
finally
{
    Environment.Exit(1);
}