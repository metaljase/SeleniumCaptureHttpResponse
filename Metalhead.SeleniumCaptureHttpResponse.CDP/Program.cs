using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.DevTools.V122.Network;
using DevToolsSessionDomains = OpenQA.Selenium.DevTools.V122.DevToolsSessionDomains;
using Fetch = OpenQA.Selenium.DevTools.V122.Fetch;

namespace Metalhead.SeleniumCDT.CaptureHttpResponse
{
    class Program
    {
        private static EventWaitHandle? s_waitForHttpResponse;

        static void Main()
        {
            // Load configuration and settings.
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true);
            var config = builder.Build();
            var settings = config.GetSection(nameof(Settings)).Get<Settings>();

            if (string.IsNullOrWhiteSpace(settings.BrowserExecutableFullPath))
            {
                throw new InvalidOperationException($"{nameof(settings.BrowserExecutableFullPath)} path in appsettings.json cannot be null or empty.");
            }
            else if (settings.BrowserExecutableFullPath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                throw new InvalidOperationException($"{nameof(settings.BrowserExecutableFullPath)} path in appsettings.json ('{settings.BrowserExecutableFullPath}') contains invalid characters.");
            }

            if (string.IsNullOrWhiteSpace(settings.WebDriverPath))
            {
                throw new InvalidOperationException($"{nameof(settings.WebDriverPath)} path appsettings.json cannot be null or empty.");
            }
            else if (settings.WebDriverPath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                throw new InvalidOperationException($"{nameof(settings.WebDriverPath)} path in appsettings.json ('{settings.WebDriverPath}') contains invalid characters.");
            }

            // EventWaitHandle to block thread while web page is loading, then unblock once HTTP response is captured.
            s_waitForHttpResponse = new EventWaitHandle(false, EventResetMode.AutoReset);

            using var webDriver = CreateWebDriver(settings.BrowserExecutableFullPath, settings.WebDriverPath);
            var devTools = webDriver as IDevTools;
            IDevToolsSession devToolsSession = devTools.GetDevToolsSession();

            var fetchAdaptor = devToolsSession.GetVersionSpecificDomains<DevToolsSessionDomains>().Fetch;
            var enableCommandSettings = new Fetch.EnableCommandSettings();
            var requestPattern = new Fetch.RequestPattern
            {
                // Optional: Wildcards are allowed ('*' = zero or more, '?' = one). Escape character is backslash. Omitting is equivalent to "*".
                UrlPattern = "https://jsonplaceholder.typicode.com/users/",
                // Optional: Stage at which to begin intercepting requests. Default is Request.
                RequestStage = Fetch.RequestStage.Response,
                // Optional: If set, only requests for matching resource types will be intercepted.
                ResourceType = ResourceType.XHR
            };
            enableCommandSettings.Patterns = [requestPattern];
            // Enables issuing of RequestPaused events.
            fetchAdaptor.Enable(enableCommandSettings);

            Response? response = null;

            async void ResponseInterceptedAsync(object? sender, Fetch.RequestPausedEventArgs e)
            {
                // Wait for response body.
                var getResponseBodyCommandResponse = await fetchAdaptor.GetResponseBody(new Fetch.GetResponseBodyCommandSettings()
                {
                    RequestId = e.RequestId
                });

                // Store response and message body.
                response = new Response(e, getResponseBodyCommandResponse);

                // Continue loading paused response.  Fetch.FulfillRequest can be used instead of Fetch.ContinueResponse.
                await fetchAdaptor.ContinueResponse(new Fetch.ContinueResponseCommandSettings()
                {
                    RequestId = e.RequestId
                });

                // Captured HTTP response; unblock the thread.
                s_waitForHttpResponse?.Set();
            }

            fetchAdaptor.RequestPaused += ResponseInterceptedAsync;

            webDriver.Url = "https://metaljase.github.io/SeleniumCaptureHttpResponse.html";

            // Wait until thread is unblocked (in ResponseInterceptedAsync), unless timeout is exceeded.
            if (!s_waitForHttpResponse.WaitOne(TimeSpan.FromSeconds(10)))
            {
                Console.WriteLine("Timeout while waiting for HTTP response.");
            }
            else if (response?.RequestPausedEventArgs?.ResponseStatusCode == 200)
            {
                // Output contents of message body returned in HTTP response.
                Console.WriteLine(response);
            }

            fetchAdaptor.RequestPaused -= ResponseInterceptedAsync;
            devToolsSession.Dispose();
            s_waitForHttpResponse.Dispose();

            Console.WriteLine("Press any key to quit web driver...");
            Console.ReadKey(true);
            webDriver.Quit();
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
}