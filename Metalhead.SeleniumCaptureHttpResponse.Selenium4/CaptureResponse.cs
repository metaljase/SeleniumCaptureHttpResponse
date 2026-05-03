using System.Collections.Concurrent;
using OpenQA.Selenium;

using Metalhead.SeleniumCaptureHttpResponse.Core;

namespace Metalhead.SeleniumCaptureHttpResponse.Selenium4;

public class CaptureResponse(Core.DriverOptions driverOptions)
{
    public async Task<ConcurrentBag<ResponseData>> GetResponseData(string url, List<string> captureUrls, TimeSpan timeout)
    {
        EventWaitHandle[] eventWaitHandles = new EventWaitHandle[captureUrls.Count];
        Dictionary<string, EventWaitHandle> eventWaitHandleLookup = [];
        ConcurrentBag<ResponseData> responseData = [];

        for (int i = 0; i < captureUrls.Count; i++)
        {
            eventWaitHandles[i] = new AutoResetEvent(false);
            eventWaitHandleLookup.Add(captureUrls[i], eventWaitHandles[i]);
            responseData.Add(new ResponseData(captureUrls[i]));
        }

        using var webDriver = WebDriverHelper.CreateWebDriver(driverOptions.BrowserExecutableFullPath, driverOptions.WebDriverPath);
        INetwork networkInterceptor = webDriver.Manage().Network;
        bool monitoringStarted = false;

        try
        {
            foreach (var captureUrl in eventWaitHandleLookup.Keys)
            {
                networkInterceptor.AddResponseHandler(new NetworkResponseHandler
                {
                    ResponseMatcher = response => response.Url?.Equals(captureUrl, StringComparison.OrdinalIgnoreCase) ?? false,
                    ResponseTransformer = response =>
                    {
                        if (response.Url is not null)
                        {
                            var data = responseData.First(r => r.Url.Equals(response.Url, StringComparison.OrdinalIgnoreCase));
                            data.CaptureSuccess = true;
                            data.Body = response.Body; // Capture the HTTP response body.

                            eventWaitHandleLookup[response.Url].Set(); // Signal this HTTP response has been captured.
                        }
                        return response;
                    }
                });
            }

            await networkInterceptor.StartMonitoring();
            monitoringStarted = true;
            webDriver.Navigate().GoToUrl(url);

            // Wait for signals that all HTTP responses have been captured, unless the timeout is exceeded.
            WaitHandle.WaitAll(eventWaitHandles, timeout);
            return responseData;
        }
        finally
        {
            if (monitoringStarted)
                await networkInterceptor.StopMonitoring();
        }
    }
}
