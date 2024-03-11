using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.DevTools.V122.Network;
using System.Collections.Concurrent;
using DevToolsSessionDomains = OpenQA.Selenium.DevTools.V122.DevToolsSessionDomains;
using Fetch = OpenQA.Selenium.DevTools.V122.Fetch;

namespace Metalhead.SeleniumCaptureHttpResponse.CDP;

public class CaptureResponse(DriverOptions driverSettings)
{
    public ConcurrentBag<ResponseData> GetResponseData(string url, List<string> captureUrls, TimeSpan timeout)
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
        var devTools = webDriver as IDevTools;
        IDevToolsSession devToolsSession = devTools.GetDevToolsSession();

        var fetchAdaptor = devToolsSession.GetVersionSpecificDomains<DevToolsSessionDomains>().Fetch;
        var enableCommandSettings = new Fetch.EnableCommandSettings();

        List<Fetch.RequestPattern> requestPatterns = [];
        foreach (var captureUrl in captureUrls)
        {
            requestPatterns.Add(new Fetch.RequestPattern
            {
                // Optional: Wildcards are allowed ('*' = zero or more, '?' = one). Escape character is backslash. Omitting is equivalent to "*".
                UrlPattern = captureUrl,
                // Optional: Stage at which to begin intercepting requests. Default is Request.
                RequestStage = Fetch.RequestStage.Response,
                // Optional: If set, only requests for matching resource types will be intercepted.
                ResourceType = ResourceType.XHR
            });
        }
        enableCommandSettings.Patterns = [.. requestPatterns];        
        fetchAdaptor.Enable(enableCommandSettings); // Enables issuing of RequestPaused events.

        async void ResponseInterceptedAsync(object? sender, Fetch.RequestPausedEventArgs e)
        {
            // Wait for response body.
            var getResponseBodyCommandResponse = await fetchAdaptor.GetResponseBody(new Fetch.GetResponseBodyCommandSettings()
            {
                RequestId = e.RequestId
            });

            SeleniumCDT.CaptureHttpResponse.Response? response = new(e, getResponseBodyCommandResponse);
            if (response?.RequestPausedEventArgs?.ResponseStatusCode == 200)
            {
                var data = responseData.First(r => r.Url.Equals(e.Request.Url, StringComparison.OrdinalIgnoreCase));
                data.CaptureSuccess = true;
                data.Body = response.ToString(); // Capture the HTTP response body.
            }

            // Continue loading paused response.  Fetch.FulfillRequest can be used instead of Fetch.ContinueResponse.
            await fetchAdaptor.ContinueResponse(new Fetch.ContinueResponseCommandSettings()
            {
                RequestId = e.RequestId
            });

            _eventWaitHandleLookup[e.Request.Url].Set(); // Signal this HTTP response has been captured.
        }

        fetchAdaptor.RequestPaused += ResponseInterceptedAsync;

        webDriver.Url = url;

        // Wait for signals that all HTTP responses have been captured, unless the timeout is exceeded.
        WaitHandle.WaitAll(_eventWaitHandles, timeout);

        fetchAdaptor.RequestPaused -= ResponseInterceptedAsync;
        devToolsSession.Dispose();
        webDriver.Quit();

        return responseData;
    }

    private static IWebDriver CreateWebDriver(string browserExecutableFullPath, string webDriverPath)
    {
        var service = ChromeDriverService.CreateDefaultService(webDriverPath);
        service.EnableVerboseLogging = false;

        var options = new ChromeOptions { BinaryLocation = browserExecutableFullPath };
        options.AddArgument("incognito");

        return new ChromeDriver(service, options);
    }
}
