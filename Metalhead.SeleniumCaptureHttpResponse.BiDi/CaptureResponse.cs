using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Collections.Concurrent;

namespace Metalhead.SeleniumCaptureHttpResponse.BiDi;

public class CaptureResponse(DriverOptions driverSettings)
{
    public async Task<ConcurrentBag<ResponseData>> GetResponseData(string url, List<string> captureUrls, TimeSpan timeout)
    {
        EventWaitHandle[] _eventWaitHandles = new EventWaitHandle[captureUrls.Count];
        Dictionary<string, EventWaitHandle> _eventWaitHandleLookup = [];
        ConcurrentBag<ResponseData> responseData = [];

        for (int i = 0; i < captureUrls.Count; i++)
        {
            _eventWaitHandles[i] = new AutoResetEvent(false);
            _eventWaitHandleLookup.Add(captureUrls[i], _eventWaitHandles[i]);
            responseData.Add(new ResponseData(captureUrls[i]));
        }

        using var webDriver = CreateWebDriver(driverSettings.BrowserExecutableFullPath, driverSettings.WebDriverPath);
        INetwork networkInterceptor = webDriver.Manage().Network;

        foreach (var captureUrl in _eventWaitHandleLookup.Keys)
        {
            networkInterceptor.AddResponseHandler(new NetworkResponseHandler
            {
                ResponseMatcher = response => response.Url.Equals(captureUrl, StringComparison.OrdinalIgnoreCase),
                ResponseTransformer = response =>
                {
                    var data = responseData.First(r => r.Url.Equals(response.Url, StringComparison.OrdinalIgnoreCase));
                    data.CaptureSuccess = true;
                    data.Body = response.Body; // Capture the HTTP response body.
                    
                    _eventWaitHandleLookup[response.Url].Set(); // Signal this HTTP response has been captured.
                    return response;
                }
            });
        }

        await networkInterceptor.StartMonitoring();
        webDriver.Navigate().GoToUrl(url);

        // Wait for signals that all HTTP responses have been captured, unless the timeout is exceeded.
        WaitHandle.WaitAll(_eventWaitHandles, timeout);

        await networkInterceptor.StopMonitoring();
        webDriver.Quit();
        return responseData;
    }

    private static IWebDriver CreateWebDriver(string? browserExecutableFullPath, string? webDriverPath)
    {
        webDriverPath = string.IsNullOrWhiteSpace(webDriverPath) ? null : webDriverPath;
        browserExecutableFullPath = string.IsNullOrWhiteSpace(browserExecutableFullPath) ? null : browserExecutableFullPath;

        var service = ChromeDriverService.CreateDefaultService(webDriverPath);
        service.EnableVerboseLogging = false;

        var options = new ChromeOptions { BinaryLocation = browserExecutableFullPath };
        options.AddArgument("incognito");

        return new ChromeDriver(service, options);
    }
}
