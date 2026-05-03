using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Metalhead.SeleniumCaptureHttpResponse.Core;

public static class WebDriverHelper
{
    public static IWebDriver CreateWebDriver(string? browserExecutableFullPath, string? webDriverPath)
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
